using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	internal interface IWarningMessageProvider
	{
		ZString GetWarningMessage();
	}

	public class GenericChargeFilterBusinessObject : AccountingFilterStripBusinessObject, IWarningMessageProvider
	{
		public GenericChargeFilterBusinessObject(ElementType elementType = ElementType.None) : base()
		{
			this.elementType = elementType;
		}

		public GenericChargeFilterBusinessObject()
		{
		}

		#region Element Type

		public enum ElementType { None, ChargeCode, GeneralLedger, All }

		readonly ElementType elementType;

		#endregion

		#region IWarningMessageProvider

		ZString IWarningMessageProvider.GetWarningMessage()
		{
			switch (elementType)
			{
				case ElementType.GeneralLedger:
					return ResString.GetMultilingualString("Accounting|IGenericChargeTypeProvider|WarningMessage|ChargeCode", "Note: Charge Codes are not listed as you do not have security right to Maintain -> Account -> Charge Codes");
				case ElementType.ChargeCode:
					return ResString.GetMultilingualString("Accounting|IGenericChargeTypeProvider|WarningMessage|GeneralLedger", "Note: GL Accounts are not listed as you do not have security right to Maintain -> Account -> GL Accounts");
				case ElementType.None:
					return ResString.GetMultilingualString("Accounting|IGenericChargeTypeProvider|WarningMessage|None", "No Charge Codes nor GL Accounts are listed as you do not have security right to Maintain -> Account -> Charge Codes or Maintain -> Account -> GL Accounts");
				default:
					return null;
			}
		}

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddTextFilter("Account Type", ViewGenericChargeSchema.VC_TableName, AccountChargeTypeList).MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|AccountType", "Account Type");
			filters.AddTextFilter("Charge Type", ViewGenericChargeSchema.VC_Type, ChargeTypeList).MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|ChargeType", "Charge Type");

			filters.AddTextFilter("Charge Code", ViewGenericChargeSchema.VC_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|ChargeCode", "Charge Code");
			filters.AddTextFilter("Description", ViewGenericChargeSchema.VC_Description).MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|Description", "Description");
			filters.AddTextFilter("Local Language Description", ViewGenericChargeSchema.VC_LocalLanguageDescription).MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|LocalLanguageDescription", "Local Language Description");

			filters.AddTextFilter("Local Account Code", GetLocalAccountCode)
				.WithMaxLengthOf(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|LocalAccountCode", "Local Account Code");
			filters.AddTextFilter("Local Account Description", GetLocalAccountDescription)
				.WithMaxLengthOf(AccGLAccountDescriptorSchema.AJ_AccountDescription)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|LocalAccountDescription", "Local Account Description");

			if (AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
			{
				filters.AddTextFilter("Alternate Account", GetAlternateAccount)
					.WithMaxLengthOf(AccAlternateGLAccountSchema.AGA_AccountNum)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|AlternateAccount", "Alternate Account");
				filters.AddTextFilter("Alternate Account Name", GetAlternateAccountName)
					.WithMaxLengthOf(AccAlternateGLAccountSchema.AGA_Description)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|AlternateAccountName", "Alternate Account Name");
			}

			ModuleFlagsFilter flagsFilter = filters.AddFlagsFilter("Active", new string[] { Res.GetString("Accounting|GenericChargeFilter|ActiveOnly", "Active Only") }, new SchemaBoolColumn[] { ViewGenericChargeSchema.VC_IsActive });
			flagsFilter.Property0 = true;
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GenericChargeFilter|Active", "Active");

			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;
				var companyFilter = new ZQuery(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
				companyFilter.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_GC, null);
				query.AddToFilter(companyFilter);

				if (elementType == ElementType.None)
				{
					query.IsNoResultQuery = true;
				}
				else if (elementType != ElementType.All)
				{
					var elementFilter = new ZQuery(ViewGenericChargeSchema.VC_IsGLAccount, elementType == ElementType.GeneralLedger);
					query.AddToFilter(elementFilter);
				}

				return query;
			}
		}

		ZQuery GetLocalAccountCode(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetLocalAccountCodeAndDescription(comparisonOperator, value, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);
		}

		ZQuery GetLocalAccountDescription(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetLocalAccountCodeAndDescription(comparisonOperator, value, AccGLAccountDescriptorSchema.AJ_AccountDescription);
		}

		ZQuery GetLocalAccountCodeAndDescription(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn column)
		{
			if (value.IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				var descriptionQuery = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
				descriptionQuery.AddToFilter(column, comparisonOperator, value);
				descriptionQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, GlbStaff.CurrentUser.GS_WorkingLanguage);
				descriptionQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				descriptionQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
				var descriptorPKs = Factory.Load<AccGLAccountDescriptor>(descriptionQuery).Select(x => x.ParentGLHeaderPK);

				if (descriptorPKs.Any())
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GenericCharge));
					query.AddToFilter(ViewGenericChargeSchema.PK, descriptorPKs);

					return query;
				}
				else
				{
					return ZQuery.NoResultQuery;
				}
			}
		}

		ZQuery GetAlternateAccount(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAlternateAccountAndName(comparisonOperator, value, AccAlternateGLAccountSchema.AGA_AccountNum);
		}

		ZQuery GetAlternateAccountName(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAlternateAccountAndName(comparisonOperator, value, AccAlternateGLAccountSchema.AGA_Description);
		}

		ZQuery GetAlternateAccountAndName(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn column)
		{
			if (!value.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
				query.AddToFilter(column, comparisonOperator, value);
				var alternateGLAccounts = Factory.Load<AccAlternateGLAccount>(query);
				var glHeaderPKs = alternateGLAccounts
					.Where(x => x.AGA_AAC_AlternateChart == AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
					.SelectMany(x => x.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>().Select(y => y.AAA_AG_GLHeader));

				if (glHeaderPKs.Any())
				{
					query = new ZDBOnlyQuery(typeof(GenericCharge));
					query.AddToFilter(ViewGenericChargeSchema.PK, glHeaderPKs);

					return query;
				}
			}

			return ZQuery.NoResultQuery;
		}

		#endregion

		#region Lookups

		CodeDescriptionPairList AccountChargeTypeList
		{
			get
			{
				if (fAccountChargeTypeList == null)
				{
					fAccountChargeTypeList = new CodeDescriptionPairList();
					fAccountChargeTypeList.AddPair((NoResString)"GL Account", Res.GetString("Accounting|GenericChargeFilter|GLAccountsOnly", "GL Accounts Only"));
					fAccountChargeTypeList.AddPair((NoResString)"Charge Code", Res.GetString("Accounting|GenericChargeFilter|ChargeCodesOnly", "Charge Codes Only"));
				}
				return fAccountChargeTypeList;
			}
		}

		CodeDescriptionPairList fAccountChargeTypeList;

		CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				if (fChargeTypeList == null)
				{
					fChargeTypeList = new CodeDescriptionPairList(OLookUpEditType.ChargeTypes);
					fChargeTypeList.AddRange(new CodeDescriptionPairList(OLookUpEditType.GLAccountType));
				}
				return fChargeTypeList;
			}
		}

		CodeDescriptionPairList fChargeTypeList;

		#endregion
	}
}
