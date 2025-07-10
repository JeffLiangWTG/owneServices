using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class TransactionNumberSettingCollection : NonPersistentBusinessObjectCollection<TransactionNumberSetting>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are called at the last stage when all initialization is complete.")]
		public TransactionNumberSettingCollection(TransactionNumberSettingBO parent, bool areSettingsEditable)
			: base(parent.Factory)
		{
			this.Parent = parent;
			this.AreSettingsEditable = areSettingsEditable;
			LoadTransactionNumbers();
		}

		public TransactionNumberSettingBO Parent { get; }

		public bool AreSettingsEditable { get; }

		internal void LoadTransactionNumbers()
		{
			if (AreSettingsEditable)
			{
				var defaultSetting = new TransactionNumberSetting(Factory, "", null, null, null, true);
				if (defaultSetting.IsInitialized)
				{
					defaultSetting.ReadOnly = true;
					Add(defaultSetting);
				}
			}
			LoadTransactionNumberFromRegistry();
			LoadTransactionNumberFromOrgCountryData();
		}

		void LoadTransactionNumberFromRegistry()
		{
			var query = new ZQuery();
			query.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Canada);
			var companyCollection = Factory.Load<GlbCompany>(query);

			foreach (GlbCompany company in companyCollection)
			{
				ZString accSecurityNo = CACustomsDataRegistry.Instance.AccountSecurityNo.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (!accSecurityNo.IsEmpty)
				{
					AddTransactionNumberSettingsForAllBranchesAndTypes(FormatBrokerName(company), accSecurityNo);
					AddTransactionNumberSetting(FormatBrokerName(company), accSecurityNo, null, TransactionNumber.DIFNumberDeclarationType);
				}
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.Remove(elementToDelete);
		}

		protected override void OnRemoved(BusinessObject bizObj)
		{
			base.OnRemoved(bizObj);
			var transactionNumberSetting = (TransactionNumberSetting)bizObj;
			if (AreSettingsEditable)
			{
				transactionNumberSetting.IsEditable = false;
				transactionNumberSetting.NextNumberInfo.ClearValue();
				transactionNumberSetting.MinNumberInfo.ClearValue();
				transactionNumberSetting.MaxNumberInfo.ClearValue();
				transactionNumberSetting.ClearHasChanges();
				Parent.AvailableTransactionNumberSettingCollection.Add(bizObj);
			}
			else
			{
				transactionNumberSetting.IsEditable = true;
				Parent.ExistingTransactionNumberSettingCollection.Add(transactionNumberSetting);
			}
		}

		static string FormatBrokerName(GlbCompany company)
		{
			string res = company.GC_Code;
			if (company.OrgProxy != null)
			{
				res += " / ";
				res += company.OrgProxy?.OH_Code;
			}
			return res;
		}

		void AddTransactionNumberSettingsForAllBranchesAndTypes(ZString owner, ZString accSecurityNo)
		{
			foreach (var branch in CABranches)
			{
				foreach (var type in BranchRelatedRangeTypes)
				{
					AddTransactionNumberSetting(owner, accSecurityNo, branch, type);
				}
			}
			AddTransactionNumberSetting(owner, accSecurityNo);
		}

		void AddTransactionNumberSetting(ZString owner, ZString accSecurityNo, GlbBranch branch = null, string type = "")
		{
			TransactionNumberSetting setting = new TransactionNumberSetting(Factory, owner, accSecurityNo, branch, type, AreSettingsEditable);
			if (setting.IsInitialized != AreSettingsEditable)
			{
				return;
			}
			setting.IsEditable = setting.IsInitialized;
			if (!Contains(accSecurityNo, branch, type))
			{
				Add(setting);
			}
		}

		static IEnumerable<string> BranchRelatedRangeTypes
		{
			get { return new[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.B2Adjustments, JobMessageTypeList.Codes.LowValueShipments }; }
		}

		void LoadTransactionNumberFromOrgCountryData()
		{
			var query = new ZQuery();
			query.AddToFilter(OrgCountryDataSchema.OV_ImportCustomsDefaultAddInfo, SQLComparisonOperator.Contains, OrgImpAddInfo.Schema.ZO_AccountSecurityNumber.Substring(3));
			query.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Core.Constants.CountryCodes.Canada);
			var countryDataColl = Factory.Load<OrgCountryData>(query);

			foreach (OrgCountryData countryData in countryDataColl)
			{
				var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
				ZString accSecurityNo = addInfo.ZO_AccountSecurityNumber;
				if (!accSecurityNo.IsEmpty)
				{
					AddTransactionNumberSettingsForAllBranchesAndTypes(countryData.OrgHeader.OH_Code, accSecurityNo); // not sure if resource should be used here
				}
			}
		}

#if DEBUG
		public
#endif
			IEnumerable<GlbBranch> CABranches
		{
			get
			{
				if (allBranches == null || allBranches.Length == 0)
				{
					var query = new ZDBOnlyQuery(typeof(GlbBranch));
					query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
					var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
					companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Canada);
					companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
					query.AddSubQuery(companyQuery, JoinCondition.And);
					allBranches = Factory.Load<GlbBranch>(query);
				}
				return allBranches;
			}
		}
		GlbBranch[] allBranches;

		ZBool Contains(ZString accountSecurityNumber, GlbBranch branch, ZString type)
		{
			string rangeSeparator = TransactionNumber.GetRangeSeparator(accountSecurityNumber, branch, type);
			return this.Cast<TransactionNumberSetting>().Any(transactionNumberSetting => transactionNumberSetting.RangesSeparator == rangeSeparator);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
