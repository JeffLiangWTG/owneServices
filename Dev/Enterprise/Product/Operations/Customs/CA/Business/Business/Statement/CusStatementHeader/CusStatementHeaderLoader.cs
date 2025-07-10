using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusStatementHeaderLoader : BusinessObject.Loader
	{
		public CusStatementHeaderLoader(BusinessObjectFactory factory)
				: base(factory)
		{
		}

		public CusStatementHeader Load(ZString statementType, ZString statementNumber)
		{
			var query = new ZDBOnlyQuery(typeof(CusStatementHeader));
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
			return Factory.LoadTop1<CusStatementHeader>(query);
		}

		public CusStatementHeader Load(OrgHeader importer, ZString accountSecurityNumber, ZString accountingOffice, ZDateTime accountingDate, ZString paymentType)
		{
			var cusStatementHeaderFilter = CreateNewCusStatementHeaderFilter(importer, accountSecurityNumber, accountingOffice, accountingDate, paymentType);
			var result = Factory.LoadTop1<CusStatementHeader>(cusStatementHeaderFilter);
			if (result == null)
			{
				cusStatementHeaderFilter = CreateNewCusStatementHeaderInAllCACompaniesFilter(importer, accountSecurityNumber, accountingOffice, accountingDate, paymentType);
				result = Factory.LoadTop1<CusStatementHeader>(cusStatementHeaderFilter);
			}
			return result;
		}

		public CusStatementHeader Load(OrgHeader importer, ZString accountSecurityNumber, ZString accountingOffice, ZDateTime accountingDate, ZString paymentType, ZString status, ZString accountNo)
		{
			var cusStatementHeaderFilter = CreateNewCusStatementHeaderFilter(importer, accountSecurityNumber, accountingOffice, accountingDate, paymentType);
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_Status, status);
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_AccountNo, accountNo);
			var result = Factory.LoadTop1<CusStatementHeader>(cusStatementHeaderFilter);
			if (result == null)
			{
				cusStatementHeaderFilter = CreateNewCusStatementHeaderInAllCACompaniesFilter(importer, accountSecurityNumber, accountingOffice, accountingDate, paymentType);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_Status, status);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_AccountNo, accountNo);
				result = Factory.LoadTop1<CusStatementHeader>(cusStatementHeaderFilter);
			}
			return result;
		}

		internal ZQuery CreateNewCusStatementHeaderFilter(OrgHeader importer, ZString accountSecurityNumber, ZString accountingOffice, ZDateTime accountingDate, ZString paymentType)
		{
			var cusStatementHeaderFilter = new ZQuery();
			AddFiltersForCreateNewCusStatementHeaderQuery(cusStatementHeaderFilter, importer, accountSecurityNumber, accountingOffice, accountingDate, paymentType);
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
			return cusStatementHeaderFilter;
		}

		internal ZQuery CreateNewCusStatementHeaderInAllCACompaniesFilter(OrgHeader importer, ZString accountSecurityNumber, ZString accountingOffice, ZDateTime accountingDate, ZString paymentType)
		{
			var cusStatementHeaderFilter = new ZDBOnlyQuery(typeof(CusStatementHeader));
			AddFiltersForCreateNewCusStatementHeaderQuery(cusStatementHeaderFilter, importer, accountSecurityNumber, accountingOffice, accountingDate, paymentType);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), CusStatementHeaderSchema.B2_GC);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
			cusStatementHeaderFilter.AddSubQuery(companyQuery, JoinCondition.And);
			return cusStatementHeaderFilter;
		}

		void AddFiltersForCreateNewCusStatementHeaderQuery(ZQuery cusStatementHeaderFilter, OrgHeader importer, ZString accountSecurityNumber, ZString accountingOffice, ZDateTime accountingDate, ZString paymentType)
		{
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_ProcessDate, accountingDate);
			if (importer == null)
			{
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_OH_Importer, null);
			}
			else
			{
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_OH_Importer, importer.PK);
			}
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, accountingOffice);
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_PaymentType, paymentType);
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_EntryFilerCode, accountSecurityNumber);
			cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_StatementType, SQLComparisonOperator.NotEqual, CusStatementHeaderTypes.Codes.RSF);
		}

		protected override Type GetTypeOfBusinessObjectToLoad()
		{
			return typeof(CusStatementHeader);
		}
	}
}
