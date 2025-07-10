using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgHeader : OrgHeader, IUPEDocumentSupportable
	{
		public UPEOrgHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (MiscServ.SuspendSettingHasChanges())
			{
				IsITFChargableForThisImporter = true;
			}
		}

		CustomBusinessObject CustomsFields
		{
			get { return customsFields ?? (customsFields = ((ICustomFieldProvider)this).GetCustomBusinessObject()); }
		}
		CustomBusinessObject customsFields;

		protected override Type MiscServType
		{
			get { return typeof(UPEOrgMiscServ); }
		}

		#endregion

		#region New Properties

		public ZString AccountClass
		{
			get
			{
				ZString result = ZString.Empty;
				if (CompanyData.ARDebtorGroup != null)
				{
					result = CompanyData.ARDebtorGroup.OJ_Code;
				}
				return result;
			}
		}

		public ZPropertyInfo AccountClassInfo
		{
			get { return GetZPropertyInfo(nameof(AccountClass)); }
		}

		[MaxLength(OrgCusCode.Schema.OK_CustomsRegNoMaxLength)]
		public ZString AccountNumber
		{
			get { return CustomsCodes.GetCustomsRegNo(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, GlbCompany.CurrentCompany.Country); }
			set
			{
				CheckMaximumLength(AccountNumberInfo, value);
				OrgCusCode orgCusCode = CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, GlbCompany.CurrentCompany.Country);
				if (!value.IsEmpty)
				{
					if (orgCusCode == null)
					{
						orgCusCode = CustomsCodes.AddNew();
						orgCusCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
						orgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					}
					orgCusCode.OK_CustomsRegNo = value;
				}
				else if (orgCusCode != null)
				{
					orgCusCode.Delete();
				}
				AccountNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AccountNumberInfo
		{
			get { return GetZPropertyInfo(nameof(AccountNumber)); }
		}

		public ZBool IsDeliveryHandledByUPSForThisAlternateBroker
		{
			get { return !MiscServ.OM_CustomFlag1; }
			set { MiscServ.OM_CustomFlag1 = !value; }
		}

		public ZBool IsITFChargableForThisImporter
		{
			get { return MiscServ.OM_CustomFlag2; }
			set { MiscServ.OM_CustomFlag2 = value; }
		}

		public ZBool IsHighClaimer
		{
			get { return MiscServ.OM_CustomFlag3; }
			set { MiscServ.OM_CustomFlag3 = value; }
		}

		public ZString DefaultPayablesContactName
		{
			get
			{
				ZString result = "";
				if (!IsNull)
				{
					result = GetDefaultContact(ContactType.Payables).OC_ContactName;
				}
				return result;
			}
		}

		public ZPropertyInfo DefaultPayablesContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(DefaultPayablesContactName)); }
		}

		public ZString DefaultImportAirFreightAgentContactName
		{
			get
			{
				ZString result = "";
				if (!IsNull)
				{
					result = GetDefaultContact(ContactType.ImportAirFreightAgent).OC_ContactName;
				}
				return result;
			}
		}

		public ZPropertyInfo DefaultImportAirFreightAgentContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(DefaultImportAirFreightAgentContactName)); }
		}

		public bool HasUPSAccountClass
		{
			get { return UPSAccountClasses.Contains((string)AccountClass); }
		}

		public bool IsStandardAccount
		{
			get { return StandardAccountClasses.Contains((string)AccountClass); }
		}

		public bool IsPreferredAccount
		{
			get { return AccountClass == "2"; }
		}

		public bool IsCreditCardAccount
		{
			get { return CreditCardAccountClasses.Contains((string)AccountClass); }
		}

		public bool IsOnFileCODAccount
		{
			get { return OnFileCODAccountClasses.Contains((string)AccountClass); }
		}

		public bool IsARAccount
		{
			get { return ARAccountClasses.Contains((string)AccountClass); }
		}

		public bool IsStandardLegalAccount
		{
			get { return AccountClass == "13"; }
		}

		public bool IsCODAccount
		{
			get { return AccountClass == "10"; }
		}

		bool IsPreReleaseFlagConfigured
		{
			get
			{
				return CustomsFields != null && CustomsFields.HasPossiblyCustomProperty(PreReleaseNotificationCustomsFieldIdentifier);
			}
		}

		public ZBool ShouldReceiveCommercialInvoice
		{
			get { return MiscServ.OM_CustomFlag4; }
			set { MiscServ.OM_CustomFlag4 = value; }
		}

		public ZPropertyInfo ShouldReceiveCommercialInvoiceInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ShouldReceiveCommercialInvoice), x => MiscServ.OM_CustomFlag4Info); }
		}

		OrgContact GetDefaultContact(ContactType contactType)
		{
			return DefaultContactFinder.DefaultContact(contactType);
		}

		DefaultContactFinder DefaultContactFinder
		{
			get
			{
				if (fDefaultContactFinder == null)
				{
					fDefaultContactFinder = new DefaultContactFinder(this);
				}
				return fDefaultContactFinder;
			}
		}

		IList UPSAccountClasses
		{
			get
			{
				if (fUPSAccountClasses == null)
				{
					fUPSAccountClasses = new string[]
						{
							"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15"
						};
				}
				return fUPSAccountClasses;
			}
		}

		IList StandardAccountClasses
		{
			get
			{
				if (fStandardAccountClasses == null)
				{
					fStandardAccountClasses = new string[]
						{
							"1", "3", "4"
						};
				}
				return fStandardAccountClasses;
			}
		}

		IList CreditCardAccountClasses
		{
			get
			{
				if (fCreditCardAccountClasses == null)
				{
					fCreditCardAccountClasses = new string[]
						{
							"5", "6"
						};
				}
				return fCreditCardAccountClasses;
			}
		}

		IList OnFileCODAccountClasses
		{
			get
			{
				if (fOnFileCODAccountClasses == null)
				{
					fOnFileCODAccountClasses = new string[]
						{
							"11", "12", "14", "15"
						};
				}
				return fOnFileCODAccountClasses;
			}
		}

		IList ARAccountClasses
		{
			get
			{
				if (fARAccountClasses == null)
				{
					fARAccountClasses = new string[]
						{
							"7", "8", "9"
						};
				}
				return fARAccountClasses;
			}
		}

		IList fUPSAccountClasses;
		IList fStandardAccountClasses;
		IList fCreditCardAccountClasses;
		IList fOnFileCODAccountClasses;
		IList fARAccountClasses;
		DefaultContactFinder fDefaultContactFinder;

		#endregion

		#region Letter of Authority

		public ZDateTime LetterOfAuthorityExpirationDate
		{
			get { return MiscServ.OM_CustomDate1; }
			set { MiscServ.OM_CustomDate1 = value; }
		}

		public int LetterOfAuthorityDaysToExpiration
		{
			get
			{
				int result = int.MinValue;
				if (LetterOfAuthorityExpirationDate.IsValid)
				{
					result = (LetterOfAuthorityExpirationDate.Date - ZDateTime.Now.Date).Days;
				}
				return result;
			}
		}

		public ZDateTime LetterOfAuthorityDocumentDelivered
		{
			get
			{
				StmALogDependentCollection allLogs = Logs.GetAllLogs();
				allLogs.Sort(StmALogSchema.SL_PostedTimeUtc.Name, ListSortDirection.Descending);
				foreach (StmALog log in allLogs)
				{
					if (log.SL_SE_NKEvent == Events.DocumentDelivered.Code)
					{
						if (log.SL_Reference.IndexOf("Letter of Authority") != -1)
						{
							return log.SL_EventTime;
						}
					}
				}
				return ZDateTime.Empty;
			}
		}
		public ZDateTime DateLOAReceivedAuthorisingUPStoClearGoods
		{
			get { return MiscServ.OM_CustomDate2; }
			set { MiscServ.OM_CustomDate2 = value; }
		}

		public ZBool DateLOAReceivedAuthorisingUPStoClearGoodsIsEarlier7Days
		{
			get
			{
				ZBool result = false;
				if ((ZDateTime.Now.Date - DateLOAReceivedAuthorisingUPStoClearGoods.Date).Days <= 7)
				{
					result = true;
				}
				return result;
			}
		}

		#region UncompletedDeclarations

		protected List<string> jobNumbers;

		public IList<string> UncompletedJobNumbers
		{
			get { return jobNumbers ?? (jobNumbers = GetJobNumbers()); }
		}

		List<string> GetJobNumbers()
		{
			List<string> result = new List<string>();
			if (!IsLOAReceivedAuthorisingUPStoClearGoods)
			{
				DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, string.Format("SELECT * FROM ClientOrgJobNumbers ('{0}')", PK.ToString()));
				foreach (DataRow row in table.Rows)
				{
					result.Add((string)row["JobNumber"]);
				}
			}
			return result;
		}

		internal void ResetJobNumbers()
		{
			jobNumbers = null;
		}

		#endregion

		public ZBool IsLOAReceivedAuthorisingUPStoClearGoods
		{
			get { return ((UPEOrgMiscServ)MiscServ).LOAReceivedAuthorisingUPStoClearGoods; }
		}

		#endregion

		#region ICustomLabelsProvider

		public override CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
		{
			CustomLabelInfoList result = new CustomLabelInfoList(typeof(UPEOrgMiscServ), configOrg, (NoResString)"", factory);
			foreach (CustomLabelInfo info in base.GetCustomFields(configOrg, Factory))
			{
				result.Add(info);
			}

			result.Remove(OrgMiscServ.Schema.OM_CustomFlag1);
			result.Add(Core.Constants.CustomLabels.Organisation.CustomFlag1, OrgMiscServ.Schema.OM_CustomFlag1, (NoResString)"Delivery handled by UPS for this Alternate Broker", CustomLabelStyles.ShowByDefault);

			if (OH_IsConsignee)
			{
				result.Remove(OrgMiscServ.Schema.OM_CustomFlag2);
				result.Add(Core.Constants.CustomLabels.Organisation.CustomFlag2, OrgMiscServ.Schema.OM_CustomFlag2, (NoResString)"ITF Chargable", CustomLabelStyles.ShowByDefault);
				result.Remove(OrgMiscServ.Schema.OM_CustomFlag3);
				result.Add(Core.Constants.CustomLabels.Organisation.CustomFlag3, OrgMiscServ.Schema.OM_CustomFlag3, (NoResString)"High Claimer", CustomLabelStyles.ShowByDefault);
				result.Remove(OrgMiscServ.Schema.OM_CustomDate1);
				result.Add(Core.Constants.CustomLabels.Organisation.CustomDate1, OrgMiscServ.Schema.OM_CustomDate1, (NoResString)"Letter of Authority Expiration Date", CustomLabelStyles.ShowByDefault);
				result.Remove(OrgMiscServ.Schema.OM_CustomFlag4);
				result.Add(Core.Constants.CustomLabels.Organisation.CustomFlag4, OrgMiscServ.Schema.OM_CustomFlag4, (NoResString)"Should Receive Commercial Invoice", CustomLabelStyles.ShowByDefault);

				if (IsPreReleaseFlagConfigured)
				{ result.Add("Pre-Release Notification", "IsPreReleaseFeeApplicable", (NoResString)"Pre-Release Notification", CustomLabelStyles.ShowByDefault); }
			}

			#region LOA Received Authorising UPS to Clear Goods

			result.Add(Core.Constants.CustomLabels.Organisation.CustomAttribute3, UPEOrgMiscServ.LOAReceivedAuthorisingUPStoClearGoodsName, (NoResString)"LOA Received Authorising UPS to Clear Goods", CustomLabelStyles.ShowByDefault);

			result.Remove(OrgMiscServ.Schema.OM_CustomDate2);
			result.Add(Core.Constants.CustomLabels.Organisation.CustomDate2, OrgMiscServ.Schema.OM_CustomDate2, (NoResString)"Date LOA sent Authorising to Clear Goods", CustomLabelStyles.ShowByDefault);
			#endregion

			return result;
		}

		public ZBool IsPreReleaseContactFeeApplicable
		{
			get
			{
				return IsPreReleaseFlagConfigured && ((ZBool)CustomsFields.GetPossiblyCustomProperty(PreReleaseNotificationCustomsFieldIdentifier));
			}
			set
			{
				if (IsPreReleaseFlagConfigured)
				{
					CustomsFields.SetPossiblyCustomProperty(PreReleaseNotificationCustomsFieldIdentifier, value);
				}
			}
		}

		public const string PreReleaseNotificationFieldName = "Pre-Release Notification";
		static string PreReleaseNotificationCustomsFieldIdentifier
		{
			get { return CustomPropertyHelper.GeneratePropertyIdentifier(PreReleaseNotificationFieldName, typeof(ZBool)); }
		}

		#endregion

		#region IUPEDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return new UPEOrgHeaderDocumentSupporter(this); }
		}

		void IUPEDocumentSupportable.OnPrintBatchItemQueued(PrintBatchItemQueuedEventArgs e)
		{
		}

		#endregion
	}
}
