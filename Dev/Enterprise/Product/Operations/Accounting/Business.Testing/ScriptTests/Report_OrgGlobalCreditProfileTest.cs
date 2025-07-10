using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class Report_OrgGlobalCreditProfileTest : ScriptTest
	{
		public void TestOrgGlobalCreditProfileScript()
		{
			var currencyCode = Env.CurrentCompany.LocalCurrency.Code;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";
			org1.OH_FullName = "TestOrgFullName1";
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.OB_ARCreditLimit = 50m;
			org1.MiscServ.OM_ARGlobalCreditApproved = true;
			org1.MiscServ.OM_RX_NKARGlobalCreditCurrency = currencyCode;
			org1.MiscServ.OM_ARGlobalCreditLimit = 500m;
			org1.MiscServ.OM_ARGlobalOnCreditHold = true;

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currencyCode));
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			exchangeRate.RE_SellRate = 1.1m;
			exchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
			exchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_FullName = "TestOrgFullName2";
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_ARCreditLimit = 75m;
			org2.MiscServ.OM_OH_ARGlobalCreditGroup = org1.PK;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TestOrg3";
			org3.OH_FullName = "TestOrgFullName3";
			org3.CompanyData.OB_IsDebtor = true;
			org3.CompanyData.OB_ARCreditLimit = 0m;
			org3.MiscServ.OM_ARGlobalCreditApproved = true;
			org3.MiscServ.OM_RX_NKARGlobalCreditCurrency = currencyCode;
			org3.MiscServ.OM_ARGlobalCreditLimit = 1500m;
			org3.MiscServ.OM_ARGlobalOnCreditHold = false;
			org3.CompanyData.OB_ARCreditRating = "CR2";

			TestObjectCreator.CreateRelatedParty(org3, org2, RelatedPartyTypeList.Codes.ARSettlementGroup);

			Factory.Save();

			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var tempFactory = new BusinessObjectFactory();
				var tempOrg = tempFactory.Load<OrgHeader>(org1.PK);
				tempOrg.CompanyData.OB_IsDebtor = true;
				tempOrg.CompanyData.OB_ARCreditLimit = 200m;

				var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currencyCode));
				var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
				tempExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
				tempExchangeRate.RE_SellRate = 2m;
				tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
				tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

				tempFactory.Save();
			}

			var resultOrg = RunScript(org1.PK);
			AssertEquals("Result Rows", 3, resultOrg.Rows.Count);

			var headers = new[]
			{
				"GC_Code","GC_Name","OH_Code","OH_FullName","GlobalCreditChildOrgCode","GlobalCreditChildOrgName","SettlementGroupOrgCode",
				"OB_ARUseSettlementGroupCreditLimit","GC_RX_NKLocalCurrency","OB_ARCreditLimit","OB_ARCreditApproved","OB_AROnCreditHold",
				"ARTemporaryCreditLimit","OB_ARTemporaryCreditLimitIncreaseExpiry","OB_ARTemporaryCreditLimitIncrease","OM_ARGlobalCreditLimit",
				"OM_ARGlobalCreditApproved","OM_ARGlobalOnCreditHold","OM_RX_NKARGlobalCreditCurrency","ExRate","ARCreditLimitInGlobalCreditCurrency",
				"OB_ARCreditRating"
			};

			AssertDataRow(resultOrg.Rows[0], headers, new object[]
			{
				"EDI", "Eagle Datamation International", "TestOrg1", "TestOrgFullName1", "TestOrg1", "TestOrgFullName1", null,
				"No", "AUD", 50m, "Yes", "No", 50m, null, 0m, 500m, "Yes", "Yes", "AUD", 1m, 50m, ""
			});

			AssertDataRow(resultOrg.Rows[1], headers, new object[]
			{
				"EDI", "Eagle Datamation International", "TestOrg1", "TestOrgFullName1", "TestOrg2", "TestOrgFullName2", null,
				"No", "AUD", 75m, "Yes", "No", 75m, null, 0m, 500m, "Yes", "Yes", "AUD", 1m, 75m, ""
			});

			AssertDataRow(resultOrg.Rows[2], headers, new object[]
			{
				"SIN", "Eagle Datamation International Pte Ltd", "TestOrg1", "TestOrgFullName1", "TestOrg1", "TestOrgFullName1",
				null, "No", "SGD", 200m, "Yes", "No", 200m, null, 0m, 500m, "Yes", "Yes", "AUD", 2m, 100m, ""
			});

			var resultOrg3 = RunScript(org3.PK);
			AssertEquals("Result Rows", 1, resultOrg3.Rows.Count);

			AssertDataRow(resultOrg3.Rows[0], headers, new object[]
			{
				"EDI", "Eagle Datamation International", "TestOrg3", "TestOrgFullName3", "TestOrg3", "TestOrgFullName3",
				"TestOrg2", "No", "AUD", 0m, "Yes", "No", 0m, null, 0m, 1500m, "Yes", "No", "AUD", 1m, 0m, "CR2"
			});
		}

		DataTable RunScript(ZGuid orgPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"SELECT * FROM Report_OrgGlobalCreditProfile('{0}') ORDER BY GC_Code,OB_ARCreditLimit", orgPK));
		}
	}
}
