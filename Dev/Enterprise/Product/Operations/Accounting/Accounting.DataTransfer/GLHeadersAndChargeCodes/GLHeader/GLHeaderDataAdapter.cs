using System;
using System.Linq;
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
	class GLHeaderDataAdapter : BaseAccountingDataAdapter<AccGLHeader, Xsd.GLHeadersGLHeader>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "GLHeaders"; }
		}

		public override string RootElementName
		{
			get { return "GLHeader"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeaderSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeadersSchema; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(AccGLHeader bizObj, Xsd.GLHeadersGLHeader constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting GLHeader is currently not supported");
		}
		#endregion

		#region Import

		public void ImportGLHeaderFromValueObject(AccGLHeader gLHeaderBizObj, Xsd.GLHeadersGLHeader value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(gLHeaderBizObj, value, context);
		}

		protected override void ImportFromValueObjectCore(AccGLHeader gLHeaderBizObj, Xsd.GLHeadersGLHeader value, IValueObjectImportContext context)
		{
			if (value != null && gLHeaderBizObj != null)
			{
				ProcessHeader(gLHeaderBizObj, value, context);
			}
		}

		ZGuid GetGLHeaderPKByCode(ZString gLAccountCodeValue, BusinessObjectFactory factory)
		{
			ZString codeValue = gLAccountCodeValue;
			ZQuery gLAccountQuery = new ZQuery(AccGLHeaderSchema.AG_AccountNum, codeValue);
			AccGLHeader hEADER = factory.LoadTop1<AccGLHeader>(gLAccountQuery);
			return (hEADER != null) ? hEADER.PK : ZGuid.Empty;
		}

		public void SetReferencesToGLAccounts(AccGLHeader gLHeader, Xsd.GLHeadersGLHeader value, IValueObjectImportContext context)
		{
			if (value.AccountType != Core.Constants.AccountType.Note)
			{
				gLHeader.AG_AG_PercentNum = GetGLHeaderPKByCode(value.PercentNum, gLHeader.Factory);

				gLHeader.AG_AG_ConsolidationNum = GetGLHeaderPKByCode(value.ConsolidationNum, gLHeader.Factory);
			}

			if (value.AccountType == Core.Constants.AccountType.BalanceSheetAccount)
			{
				gLHeader.AG_AG_AlternateNum = GetGLHeaderPKByCode(value.AlternateNum, gLHeader.Factory);
			}

			if (value.AccountType == Core.Constants.AccountType.Header)
			{
				gLHeader.AG_AG_HeaderDependsOnTotal = GetGLHeaderPKByCode(value.HeaderDependsOnTotal, gLHeader.Factory);
			}

			AddErrorsToNotifications(gLHeader, value, context);
		}

		void ProcessHeader(AccGLHeader gLHeader, Xsd.GLHeadersGLHeader value, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(gLHeader.AG_ColumnInfo, value.Section, value.SectionSpecified);

			context.SetPropertyInfoValue(gLHeader.AG_AccountNumInfo, value.AccNumber, value.AccNumberSpecified);

			context.SetPropertyInfoValue(gLHeader.AG_DebitCreditInfo, value.DebitCredit, value.DebitCreditSpecified);

			context.SetPropertyInfoValue(gLHeader.AG_DescriptionInfo, value.Description, value.DescriptionSpecified);

			context.SetPropertyInfoValue(gLHeader.AG_AccountTypeInfo, value.AccountType, value.AccountTypeSpecified);

			if (value.AccountType == Core.Constants.AccountType.Total)
			{
				gLHeader.AG_TotalLevel = value.TotalLevel;
			}

			gLHeader.AG_ControlAccount = (value.ControlAccount == Core.Constants.BooleanTrueString);

			gLHeader.AG_DisallowDirectPosting = (value.DisallowDirectPosting == Core.Constants.BooleanTrueString);

			gLHeader.AG_PrintSequence = value.PrintSequence;

			if (value.AccountType == Core.Constants.AccountType.Note)
			{
				if (value.StatisticalUnits.IsEmpty)
				{
					context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("ec0e5e32-3e6a-4fb2-996c-435acbccdb79", "Statistical Units can not be empty when Account Type is NTE.")));
				}
				else
				{
					context.SetPropertyInfoValue(gLHeader.AG_StatisticalUnitsInfo, value.StatisticalUnits, value.StatisticalUnitsSpecified);
				}
			}
			else
			{
				//Legacy GL header XML import does NOT support multiple sub accounts.
				if (value.CashFlowType.IsEmpty && (value.AccountType == Core.Constants.AccountType.BalanceSheetAccount || value.AccountType == Core.Constants.AccountType.ProfitAndLossAccount))
				{
					gLHeader.AG_CashFlowType =  CashFlowCodeLists.Codes.XXX;
				}
				else
				{
					gLHeader.AG_CashFlowType = value.CashFlowType;
				}
			}

			if (value.CompanyFilterList.IsEmpty || value.CompanyFilterList == "ALL")
			{
				gLHeader.AG_IsGlobal = true;
			}
			else
			{
				gLHeader.AG_IsGlobal = false;
				var companyFilterList = value.CompanyFilterList.Split(',').Select(x => x.Trim()).ToArray();
				foreach (var companyFilter in companyFilterList)
				{
					var company = gLHeader.Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, companyFilter));
					if (company == null)
					{
						context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(gLHeader, Res.GetString("c18e2ff9-f936-4cbf-ab5e-2d3e460a90eb", "Company Filter includes invalid companies."))));
					}
					else
					{
						var filter = gLHeader.CompanyFilters.AddNew();
						filter.ACF_AG_Header = gLHeader.PK;
						filter.ACF_GC_Company = company.PK;
					}
				}
			}

			AddErrorsToNotifications(gLHeader, value, context);
		}

		void AddErrorsToNotifications(AccGLHeader gLHeader, Xsd.GLHeadersGLHeader value, IValueObjectImportContext context)
		{
			foreach (string errorString in gLHeader.Notifications.GetErrors().GetUniqueMessageList())
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(gLHeader, errorString)));
			}

			foreach (string warningString in gLHeader.Notifications.GetWarnings().GetUniqueMessageList())
			{
				context.Notify(new WarningNotification(WarningType.Warning, GetErrorMessageFormatted(gLHeader, warningString)));
			}
		}

		string GetErrorMessageFormatted(AccGLHeader gLHeader, string message)
		{
			return Res.GetString("C524EC84-6AEA-4651-8F1F-55DEBC41CFF5", "GL Header Code {0} - {1}", gLHeader.AG_AccountNum, message);
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#endregion

		#region Test metheds/property wrapper
		public void ProcessHeader_ForTestOnly(AccGLHeader gLHeader, Xsd.GLHeadersGLHeader value, IValueObjectImportContext context)
		{
			ProcessHeader(gLHeader, value, context);
		}

		public void ImportFromValueObjectCore_ForTestOnly(AccGLHeader gLHeaderBizObj, Xsd.GLHeadersGLHeader value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(gLHeaderBizObj, value, context);
		}

		public void NotifyBizObjCreatedOrUpdated_ForTestOnly(INotifications notifications, BusinessObject bizObj)
		{
			NotifyBizObjCreatedOrUpdated(notifications, bizObj);
		}
		#endregion
	}
}
