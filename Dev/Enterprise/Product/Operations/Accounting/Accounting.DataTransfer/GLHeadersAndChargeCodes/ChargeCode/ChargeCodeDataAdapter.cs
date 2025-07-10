using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	class ChargeCodeDataAdapter : BaseAccountingDataAdapter<AccChargeCode, Xsd.ChargeCodesChargeCode>
	{
		public static string InvalidXmlFileErrorMessage
		{
			get { return Res.GetString("234f27a5-9ec8-4121-8c55-309ed061e139", "The journal XML file you tried to import was invalid."); }
		}
		public static string MoreThanOneJournalErrorMsg
		{
			get { return Res.GetString("b06838ae-4554-4967-9737-b98d609a4bb4", "There is more than one Journal transaction in the XML file."); }
		}
		public static string SetMarginPercentageErrorMessage
		{
			get { return Res.GetString("4eaa991b-e95e-41bb-aaeb-0759b858843b", "You must specify a Margin Percentage for {0} or {1} charge code types", Core.Constants.ChargeType.Margin, Core.Constants.ChargeType.Disbursement); }
		}
		public static string SetRevenueAccountErrorMessage
		{
			get { return Res.GetString("5cfe8462-657b-4923-b92b-1165add2176f", "You must specify a Revenue Account for {0}, {1}, {2}, {3} and {4} charge types", Core.Constants.ChargeType.Margin, Core.Constants.ChargeType.Disbursement, Core.Constants.ChargeType.Revenue, Core.Constants.ChargeType.NonAccrual, Core.Constants.ChargeType.ManualJobAccrual); }
		}
		public static string SetWIPAccountErrorMessage
		{
			get { return Res.GetString("deb549ca-9f0a-4f73-9d41-408d7f8fa36e", "You must specify a WIP for {0}, {1}, {2} and {3} charge types", Core.Constants.ChargeType.Margin, Core.Constants.ChargeType.Disbursement, Core.Constants.ChargeType.Revenue, Core.Constants.ChargeType.ManualJobAccrual); }
		}
		public static string SetCostAccountErrorMessage
		{
			get { return Res.GetString("208dab26-cb4b-41ed-91eb-bd1b1dd6641c", "You must specify a Cost Account for {0}, {1}, {2}, {3} and {4} charge types", Core.Constants.ChargeType.Margin, Core.Constants.ChargeType.Disbursement, Core.Constants.ChargeType.Overhead, Core.Constants.ChargeType.NonAccrual, Core.Constants.ChargeType.ManualJobAccrual); }
		}
		public static string SetAccrualAccountErrorMessage
		{
			get { return Res.GetString("1030bf4c-f2ae-445f-a0e0-cf090ad79f8c", "You must specify an Accrual Account for {0}, {1} and {2} charge types", Core.Constants.ChargeType.Margin, Core.Constants.ChargeType.Disbursement, Core.Constants.ChargeType.ManualJobAccrual); }
		}
		public static string SetGLAccountTypeErrorMsg(ZString accountNum)
		{
			return Res.GetString("2293d322-395e-4470-8a46-79cd1bd72d99", "The GL Account Number '{0}' is an Alternate Account and cannot be selected", accountNum);
		}

		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "ChargeCodes"; }
		}

		public override string RootElementName
		{
			get { return "ChargeCode"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.ChargeCodeSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.ChargeCodesSchema; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(AccChargeCode bizObj, Xsd.ChargeCodesChargeCode constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting Charge Code is currently not supported");
		}

		#endregion

		#region Import

		public void ImportChargeCodeFromValueObject(AccChargeCode chargeCodeBizObj, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(chargeCodeBizObj, value, context);
		}

		protected override void ImportFromValueObjectCore(AccChargeCode chargeCodeBizObj, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			if (value != null)
			{
				ProcessChargeCode(chargeCodeBizObj, value, context);
			}
		}

		ZGuid GetGstRatePKByRateCode(ZString gstRateCodeValue, BusinessObjectFactory factory)
		{
			ZString rateCodeValue = gstRateCodeValue;
			ZQuery gstRateQuery = new ZQuery(AccTaxRateSchema.AT_Code, rateCodeValue);
			gstRateQuery.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AccTaxRate taxRate = factory.LoadTop1<AccTaxRate>(gstRateQuery);
			return (taxRate != null) ? taxRate.PK : ZGuid.Empty;
		}

		ZGuid GetWithholdingTaxRatePKByCode(ZString wHTRateCodeValue, BusinessObjectFactory factory)
		{
			ZString rateCodeValue = wHTRateCodeValue;
			ZQuery wHTRateQuery = new ZQuery(AccWithholdingSchema.AW_Code, rateCodeValue);
			wHTRateQuery.AddToFilter(AccWithholdingSchema.AW_GC, GlbCompany.CurrentCompany.PK);
			AccWithholding wHTRate = factory.LoadTop1<AccWithholding>(wHTRateQuery);
			return (wHTRate != null) ? wHTRate.PK : ZGuid.Empty;
		}

		ZBool IsGLAccountTypeAlternate(ZString gLAccountNumber, BusinessObjectFactory factory)
		{
			ZQuery gLHeaderQuery = new ZQuery(AccGLHeaderSchema.AG_AccountNum, gLAccountNumber);
			AccGLHeader gLHeader = factory.LoadTop1<AccGLHeader>(gLHeaderQuery);
			return (gLHeader != null) && gLHeader.AG_AccountType == Core.Constants.AccountType.Alternate;
		}

		ZGuid GetGLHeaderPKByAccountNumber(ZString gLAccountNumber, BusinessObjectFactory factory)
		{
			ZString accountNumber = gLAccountNumber;
			ZQuery gLHeaderQuery = new ZQuery(AccGLHeaderSchema.AG_AccountNum, accountNumber);
			AccGLHeader gLHeader = factory.LoadTop1<AccGLHeader>(gLHeaderQuery);
			return (gLHeader != null) ? gLHeader.PK : ZGuid.Empty;
		}

		ZGuid GetAccGroupsPKByGroupCode(ZString aCCGroupCode, BusinessObjectFactory factory)
		{
			ZString groupCode = aCCGroupCode;
			ZQuery accGroupQuery = new ZQuery(AccGroupsSchema.AR_Code, groupCode);
			AccGroups accGroup = factory.LoadTop1<AccGroups>(accGroupQuery);
			return (accGroup != null) ? accGroup.PK : ZGuid.Empty;
		}

		void ProcessChargeCode(AccChargeCode chargeCode, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			if (value.IsGlobal == Core.Constants.BooleanTrueString)
			{
				chargeCode.AC_GC = ZGuid.Empty;
			}
			else
			{
				chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			}
			chargeCode.Validation.ValidateAC_GC();

			context.SetPropertyInfoValue(chargeCode.AC_CodeInfo, value.Code, value.CodeSpecified);
			chargeCode.Validation.ValidateAC_Code();

			context.SetPropertyInfoValue(chargeCode.AC_DescInfo, value.Description, value.DescriptionSpecified);
			chargeCode.Validation.ValidateAC_Desc();

			context.SetPropertyInfoValue(chargeCode.AC_DepartmentFilterListInfo, value.DepartmentFilterList, value.DepartmentFilterListSpecified);
			chargeCode.Validation.ValidateAC_DepartmentFilterList();

			context.SetPropertyInfoValue(chargeCode.AC_ChargeTypeInfo, value.ChargeType, value.ChargeTypeSpecified);
			chargeCode.Validation.ValidateAC_ChargeType();

			SetMarginPercentage(chargeCode, value, context);
			SetRevenueAccount(chargeCode, value, context);
			SetWIPAccount(chargeCode, value, context);
			SetCostAccount(chargeCode, value, context);
			SetAccrualAccount(chargeCode, value, context);

			chargeCode.AC_AT_GSTRate = GetGstRatePKByRateCode(value.GSTRate, chargeCode.Factory);
			chargeCode.Validation.ValidateAC_AT_GSTRate();

			chargeCode.AC_AW_WithholdingTaxRate = GetWithholdingTaxRatePKByCode(value.WithholdingTaxRate, chargeCode.Factory);
			chargeCode.Validation.ValidateAC_AW_WithholdingTaxRate();

			chargeCode.AC_AR_SalesGroup = GetAccGroupsPKByGroupCode(value.SalesGroup, chargeCode.Factory);
			chargeCode.Validation.ValidateAC_AR_SalesGroup();

			chargeCode.AC_AR_ExpenseGroup = GetAccGroupsPKByGroupCode(value.ExpenseGroup, chargeCode.Factory);
			chargeCode.Validation.ValidateAC_AR_ExpenseGroup();

			context.SetPropertyInfoValue(chargeCode.AC_ChargeGroupInfo, value.ChargeGroup, value.ChargeGroupSpecified);
			chargeCode.Validation.ValidateAC_ChargeGroup();

			context.SetPropertyInfoValue(chargeCode.AC_ChargeSubGroupInfo, value.SubGroup, value.SubGroupSpecified);
			chargeCode.Validation.ValidateAC_ChargeSubGroup();

			chargeCode.AC_IsGroupageCharge = (value.IsGroupageCharge == Core.Constants.BooleanTrueString);
			chargeCode.Validation.ValidateAC_IsGroupageCharge();

			context.SetPropertyInfoValue(chargeCode.AC_RateCalculatorInfo, value.RateCalculator, value.RateCalculatorSpecified);
			chargeCode.Validation.ValidateAC_RateCalculator();

			chargeCode.AC_ShowOnQuotation = (value.ShowOnQuotation == Core.Constants.BooleanTrueString);
			chargeCode.Validation.ValidateAC_ShowOnQuotation();

			chargeCode.AC_SuppressOnQuoteIfZero = (value.SuppressOnQuoteIfZero == Core.Constants.BooleanTrueString);
			chargeCode.Validation.ValidateAC_SuppressOnQuoteIfZero();

			context.SetPropertyInfoValue(chargeCode.AC_IATA_ChargeCodeMapInfo, value.IATA_ChargeCodeMap, value.IATA_ChargeCodeMapSpecified);
			chargeCode.Validation.ValidateAC_IATA_ChargeCodeMap();

			if ((AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value && !value.GovtChargeCodeSpecified) || chargeCode.IsGlobal)
			{
				context.SetPropertyInfoValue(chargeCode.AC_GovtChargeCodeInfo, ZString.Empty);
			}
			else
			{
				context.SetPropertyInfoValue(chargeCode.AC_GovtChargeCodeInfo, value.GovtChargeCode);
			}

			chargeCode.Validation.ValidateAC_GovtChargeCode();

			AddErrorsToNotifications(chargeCode, value, context);
		}

		void SetMarginPercentage(AccChargeCode chargeCode, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			if (value.MarginPercentage.Length > 0)
			{
				chargeCode.AC_MarginPercentage = ZDecimal.ParseSafe(value.MarginPercentage, 0);
			}
			else if (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement)
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetMarginPercentageErrorMessage)));
			}

			chargeCode.Validation.ValidateAC_MarginPercentage();
		}

		void SetRevenueAccount(AccChargeCode chargeCode, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			if (value.RevenueAccount.Length > 0)
			{
				if (IsGLAccountTypeAlternate(value.RevenueAccount, chargeCode.Factory))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetGLAccountTypeErrorMsg(value.RevenueAccount))));
				}
				chargeCode.AC_AG_RevenueAccount = GetGLHeaderPKByAccountNumber(value.RevenueAccount, chargeCode.Factory);
				if ((chargeCode.AC_AG_RevenueAccount == ZGuid.Empty) && (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.Revenue || value.ChargeType == Core.Constants.ChargeType.NonAccrual || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetRevenueAccountErrorMessage)));
				}
			}
			else if (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.Revenue || value.ChargeType == Core.Constants.ChargeType.NonAccrual || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual)
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetRevenueAccountErrorMessage)));
			}
			chargeCode.Validation.ValidateAC_AG_RevenueAccount();
		}

		void SetWIPAccount(AccChargeCode chargeCode, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			if (value.WIPAccount.Length > 0)
			{
				if (IsGLAccountTypeAlternate(value.WIPAccount, chargeCode.Factory))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetGLAccountTypeErrorMsg(value.WIPAccount))));
				}

				chargeCode.AC_AG_WIPAccount = GetGLHeaderPKByAccountNumber(value.WIPAccount, chargeCode.Factory);

				if ((chargeCode.AC_AG_WIPAccount == ZGuid.Empty) && (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.Revenue || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetWIPAccountErrorMessage)));
				}
			}
			else if (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.Revenue || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual)
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetWIPAccountErrorMessage)));
			}
			chargeCode.Validation.ValidateAC_AG_WIPAccount();
		}

		void SetCostAccount(AccChargeCode chargeCode, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			if (value.CostAccount.Length > 0)
			{
				if (IsGLAccountTypeAlternate(value.CostAccount, chargeCode.Factory))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetGLAccountTypeErrorMsg(value.CostAccount))));
				}
				chargeCode.AC_AG_CostAccount = GetGLHeaderPKByAccountNumber(value.CostAccount, chargeCode.Factory);
				if ((chargeCode.AC_AG_CostAccount == ZGuid.Empty) && (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.Overhead || value.ChargeType == Core.Constants.ChargeType.NonAccrual || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetCostAccountErrorMessage)));
				}
			}
			else if (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.Overhead || value.ChargeType == Core.Constants.ChargeType.NonAccrual || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual)
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetCostAccountErrorMessage)));
			}
			chargeCode.Validation.ValidateAC_AG_CostAccount();
		}

		void SetAccrualAccount(AccChargeCode chargeCode, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			if (IsGLAccountTypeAlternate(value.AccrualAccount, chargeCode.Factory))
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetGLAccountTypeErrorMsg(value.AccrualAccount))));
			}
			if (value.AccrualAccount.Length > 0)
			{
				chargeCode.AC_AG_AccrualAccount = GetGLHeaderPKByAccountNumber(value.AccrualAccount, chargeCode.Factory);
				if ((chargeCode.AC_AG_AccrualAccount == ZGuid.Empty) && (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetAccrualAccountErrorMessage)));
				}
			}
			else if (value.ChargeType == Core.Constants.ChargeType.Margin || value.ChargeType == Core.Constants.ChargeType.Disbursement || value.ChargeType == Core.Constants.ChargeType.ManualJobAccrual)
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, SetAccrualAccountErrorMessage)));
			}
			chargeCode.Validation.ValidateAC_AG_AccrualAccount();
		}

		void AddErrorsToNotifications(AccChargeCode chargeCode, Xsd.ChargeCodesChargeCode value, IValueObjectImportContext context)
		{
			foreach (INotification errorString in chargeCode.Notifications.GetErrors())
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(chargeCode, errorString.Message)));
			}

			foreach (INotification warningString in chargeCode.Notifications.GetWarnings())
			{
				context.Notify(new WarningNotification(WarningType.Warning, GetErrorMessageFormatted(chargeCode, warningString.Message)));
			}
		}

		string GetErrorMessageFormatted(AccChargeCode chargeCode, string message)
		{
			return Res.GetString("177dce21-351a-4940-a256-1f35dd84c0dc", "Charge Code {0} - {1}", chargeCode.AC_Code, message);
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#endregion

		#region Test metheds/property wrapper
		public void NotifyBizObjCreatedOrUpdated_ForTestOnly(INotifications notifications, BusinessObject bizObj)
		{
			NotifyBizObjCreatedOrUpdated(notifications, bizObj);
		}
		#endregion
	}
}
