using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	partial class GLAccountDescriptorDataAdapter : ValueObjectDataAdapter<AccGLAccountDescriptor, Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "GLHeaderMultiLanguageMappings"; }
		}

		public override string RootElementName
		{
			get { return "GLHeaderMultiLanguageMapping"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeaderMultiLanguageMappingSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeaderMultiLanguageMappingsSchema; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(AccGLAccountDescriptor glAccDescriptorBizObj, Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting GL Mapping is currently not supported");
		}
		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(AccGLAccountDescriptor glAccDescriptorBizObj, Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping value, IValueObjectImportContext context)
		{
			if (value != null)
			{
				ProcessHeader(glAccDescriptorBizObj, value, context);
			}
		}

		ZGuid GetGLAccDescriptorPKByCode(ZString glAccountCodeValue, ZString language, ZString country, BusinessObjectFactory factory)
		{
			ZQuery glAccountDescQuery = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, glAccountCodeValue);
			glAccountDescQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, language);
			glAccountDescQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, country);
			AccGLAccountDescriptor glAccountDesc = factory.LoadTop1<AccGLAccountDescriptor>(glAccountDescQuery);
			return (glAccountDesc != null) ? glAccountDesc.PK : ZGuid.Empty;
		}

		ZGuid GetGLHeaderPKByCode(ZString glAccountCodeValue, BusinessObjectFactory factory)
		{
			ZQuery glAccountQuery = new ZQuery(AccGLHeaderSchema.AG_AccountNum, glAccountCodeValue);
			AccGLHeader glAccount = factory.LoadTop1<AccGLHeader>(glAccountQuery);
			return (glAccount != null) ? glAccount.PK : ZGuid.Empty;
		}

		public void SetReferencesToGLAccounts(AccGLAccountDescriptor glAccDescriptor, Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping value, IValueObjectImportContext context)
		{
			if (value.ReportCategory != Core.Constants.AccountType.Note)
			{
				glAccDescriptor.AJ_AJ_PercentNum = GetGLAccDescriptorPKByCode(value.PercentNum, value.Language, value.CountryOfCompliance, glAccDescriptor.Factory);
				glAccDescriptor.AJ_AJ_ConsolidationNum = GetGLAccDescriptorPKByCode(value.ConsolidationNum, value.Language, value.CountryOfCompliance, glAccDescriptor.Factory);
			}

			if (value.ReportCategory == Core.Constants.AccountType.BalanceSheetAccount)
			{
				glAccDescriptor.AJ_AJ_AlternativeNum = GetGLAccDescriptorPKByCode(value.AlternativeNum, value.Language, value.CountryOfCompliance, glAccDescriptor.Factory);
			}

			if (value.ReportCategory == Core.Constants.AccountType.Header)
			{
				glAccDescriptor.AJ_AJ_HeaderDependsOnTotal = GetGLAccDescriptorPKByCode(value.HeaderDependsOnTotal, value.Language, value.CountryOfCompliance, glAccDescriptor.Factory);
			}

			if (value.ReportCategory == AccountTypeComboBoxConstants.CarriedForwardAccount)
			{
				glAccDescriptor.AJ_AJ_CarriedForwardAccount = GetGLAccDescriptorPKByCode(value.CarriedForwardAccount, value.Language, value.CountryOfCompliance, glAccDescriptor.Factory);
			}
		}

		void ProcessHeader(AccGLAccountDescriptor glAccDescriptor, Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping value, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(glAccDescriptor.AJ_LanguageInfo, value.Language, value.LanguageSpecified);
			context.SetPropertyInfoValue(glAccDescriptor.AJ_LocalAccountNumberInfo, value.LocalAccountNumber, value.LocalAccountNumberSpecified);
			context.SetPropertyInfoValue(glAccDescriptor.AJ_AccountDescriptionInfo, value.Description, value.DescriptionSpecified);
			context.SetPropertyInfoValue(glAccDescriptor.AJ_DebitCreditInfo, value.DebitCredit, value.DebitCreditSpecified);
			context.SetPropertyInfoValue(glAccDescriptor.AJ_RN_NKCountryOfComplianceInfo, value.CountryOfCompliance, value.CountryOfComplianceSpecified);
			context.SetPropertyInfoValue(glAccDescriptor.AJ_ReportTypeInfo, value.ReportType, value.ReportTypeSpecified);
			context.SetPropertyInfoValue(glAccDescriptor.AJ_ReportCategoryInfo, value.ReportCategory, value.ReportCategorySpecified);
			if (value.ReportType.IsEmpty)
			{
				glAccDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			}

			if (value.ReportCategory == Core.Constants.AccountType.Total)
			{
				glAccDescriptor.AJ_TotalLevel = (ZShort)value.TotalLevel;
			}

			glAccDescriptor.AJ_PrintSequence = (ZShort)value.PrintSequence;

			glAccDescriptor.ParentGLHeaderPK = GetGLHeaderPKByCode(value.ParentAccount, glAccDescriptor.Factory);

			AddErrorsToNotifications(glAccDescriptor, context);
		}

		void AddErrorsToNotifications(AccGLAccountDescriptor glAccDescriptor, IValueObjectImportContext context)
		{
			foreach (string errorString in glAccDescriptor.Notifications.GetErrors().GetUniqueMessageList())
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(glAccDescriptor, errorString)));
			}

			foreach (string warningString in glAccDescriptor.Notifications.GetWarnings().GetUniqueMessageList())
			{
				context.Notify(new ErrorNotification(ErrorType.Warning, GetErrorMessageFormatted(glAccDescriptor, warningString)));
			}
		}

		string GetErrorMessageFormatted(AccGLAccountDescriptor glAccDescriptor, string message)
		{
			return Res.GetString("29E21B91-569E-4B93-B258-51AB26A31CBE}", "GL Mapping Code {0} - {1}", glAccDescriptor.AJ_LocalAccountNumber, message);
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#endregion
	}
}