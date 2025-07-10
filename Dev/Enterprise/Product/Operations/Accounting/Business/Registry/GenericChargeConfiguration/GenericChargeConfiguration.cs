using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;
using AccGenericChargeCollection = Enterprise.Accounting.Business.GenericCharge.GenericChargeCollection;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class GenericChargeConfiguration : RegistryBusinessObjectTemplate
	{
		public GenericChargeConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public GenericChargeConfiguration()
			: base()
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string ChargePK = "ChargePK";
			public const string ChargeDescription = "ChargeDescription";
		}

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new GenericChargeConfiguration(fallbackLevel, factory);
			clone.ChargePK = ChargePK;

			return clone;
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ChargePK, ChargePK.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			chargePK = new ZGuid(reader.ReadElementString(Schema.ChargePK));
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateChargePK();
		}

		#endregion

		#region Properties

		ZGuid chargePK;

		[List("ChargeList")]
		public ZGuid ChargePK
		{
			get { return chargePK; }
			set
			{
				if (chargePK != value)
				{
					SetNonPersistentPropertyValue(ChargePKInfo, ref chargePK, value);

					if (!IsValidationSuspended)
					{
						ValidateChargePK();
					}
				}
			}
		}

		public ZPropertyInfo ChargePKInfo
		{
			get { return GetZPropertyInfo(Schema.ChargePK); }
		}

		public void ValidateChargePK()
		{
			ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChargePKInfo, Res.GetString("7430F4C8-87EC-44C8-B9F0-D1D6A4730A71", "Charge or GL Account"));
			if (CurrentFallbackLevel != null)
			{
				ListValidation.ErrorIfInvalidPK(ChargePKInfo, ChargeList);
			}

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ChargePKInfo, ErrorDuplicateCharge);
			}
		}

		[BusinessObjectTestExclude]
		public ZString ChargeDescription
		{
			get { return Charge?.VC_Description ?? string.Empty; }
		}

		AccGenericCharge charge;
		public AccGenericCharge Charge
		{
			get
			{
				if (charge == null || chargePK != charge.PK)
				{
					charge = null;
					var chargeAccount = CurrentFactory.LoadTop1<AccGenericCharge>(new ZQuery(ViewGenericChargeSchema.PK, chargePK));
					if (chargeAccount != null)
					{
						charge = chargeAccount;
					}
				}
				return charge;
			}
		}

		#endregion

		#region ChargeList

		AccGenericChargeCollection chargeList;
		Guid? chargeListCompanyPK;
		public AccGenericChargeCollection ChargeList
		{
			get
			{
				if (chargeList == null || chargeListCompanyPK != CurrentFallbackLevel?.CompanyPK(false))
				{
					chargeListCompanyPK = CurrentFallbackLevel?.CompanyPK(false);
					var result = new ZDBOnlyQuery(typeof(AutoViewGenericCharge));

					result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_GC, chargeListCompanyPK);
					result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_GC, null);

					var gLAccountFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.AccountType.ProfitAndLossAccount);
					var balanceSheetFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.AccountType.BalanceSheetAccount);
					balanceSheetFilter.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_IsControlAccount, SQLComparisonOperator.Equal, false);
					gLAccountFilter.AddToFilter(balanceSheetFilter, JoinCondition.Or);

					var chargeFilter = new ZQuery();

					chargeFilter.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Comment);
					chargeFilter.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Margin);
					chargeFilter.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.NonAccrual);
					chargeFilter.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Revenue);
					chargeFilter.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.ManualJobAccrual);

					var commonFilter = new ZQuery(gLAccountFilter);
					commonFilter.AddToFilter(chargeFilter, JoinCondition.Or);

					var chargeCodeQuery = new ZDBOnlySubQuery(typeof(AutoViewGenericCharge), ViewGenericChargeSchema.PK);
					chargeCodeQuery.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_TableName, "Charge Code");
					chargeCodeQuery.AddToFilter(commonFilter);

					var globalGLHeaderSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeader), AccGLHeaderSchema.PK);
					globalGLHeaderSubQuery.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, true);
					globalGLHeaderSubQuery.AddToFilter(commonFilter);

					var nonGlobalGLHeaderSubQuery = new ZDBOnlySubQuery(typeof(AutoViewGenericCharge), ViewGenericChargeSchema.PK);
					var isGlobalFilterSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeader), AccGLHeaderSchema.PK);
					isGlobalFilterSubQuery.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, false);
					if (chargeListCompanyPK != null)
					{
						var companyFilterSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderCompanyFilter), AccGLHeaderCompanyFilterSchema.ACF_AG_Header);
						companyFilterSubQuery.AddToFilter(AccGLHeaderCompanyFilterSchema.ACF_GC_Company, chargeListCompanyPK);
						nonGlobalGLHeaderSubQuery.AddSubQuery(companyFilterSubQuery, JoinCondition.And);
					}
					nonGlobalGLHeaderSubQuery.AddSubQuery(isGlobalFilterSubQuery, JoinCondition.And);
					nonGlobalGLHeaderSubQuery.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_TableName, "GL Account");
					nonGlobalGLHeaderSubQuery.AddToFilter(commonFilter);

					chargeCodeQuery.AddAsUnionQuery(globalGLHeaderSubQuery, true);
					chargeCodeQuery.AddAsUnionQuery(nonGlobalGLHeaderSubQuery, true);

					result.AddSubQuery(chargeCodeQuery, JoinCondition.And);
					chargeList = new AccGenericChargeCollection(CurrentFactory, result);
					chargeList.Load();
				}
				return chargeList;
			}
		}

		#endregion

		public static string ErrorDuplicateCharge
		{
			get { return Res.GetString("FABE6493-D422-4238-8944-D7C2AA11BA03", "There must be only one the same charge code or GL account in the list."); }
		}
	}
}
