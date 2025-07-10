using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosLineBizOTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			ZGuid testGuid = ZGuid.NewZGuid();
			DynamicBusinessObjectCollection<CognosLineBizO> collection = new DynamicBusinessObjectCollection<CognosLineBizO>(Factory);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@AccountPK", testGuid, DummyBizoSchema.Z0_Guid);
			@params.Add("@AccountName", "MEH MEH", DummyBizoSchema.Z0_Description);
			@params.Add("@AccountCode", "MEH", DummyBizoSchema.Z0_Code);
			@params.Add("@CounterCompany", "USCOR", DummyBizoSchema.Z0_Code);
			@params.Add("@Mode", "AE", DummyBizoSchema.Z0_Code);
			@params.Add("@Branch", "MEL", DummyBizoSchema.Z0_Code);
			@params.Add("@Business", "OTH", DummyBizoSchema.Z0_Code);
			@params.Add("@Amount", 250m, DummyBizoSchema.Z0_Decimal);
			@params.Add("@TransactionCurrency", "JPY", DummyBizoSchema.Z0_Code);
			@params.Add("@TransactionAmount", 1000m, DummyBizoSchema.Z0_Decimal);
			@params.Add("@Geographical", "SEA", DummyBizoSchema.Z0_Code);
			collection.Load(@"
SELECT 
    @AccountPK AS AccountPK,
    @AccountName AS AccountName,
    @AccountCode AS AccountCode,
    @CounterCompany AS CounterCompany,
    @Mode AS Mode,
    @Branch AS Branch,
    @Business AS Business,
    @Amount AS Amount,
    @TransactionCurrency AS TransactionCurrency,
    @TransactionAmount AS TransactionAmount,
    @Geographical AS Geographical", @params);
			AssertEquals(testGuid, collection[0].AccountPK);
			AssertEquals("MEH", collection[0].AccountCode);
			AssertEquals("MEH MEH", collection[0].AccountName);
			AssertEquals("USCOR", collection[0].CounterCompany);
			AssertEquals("AE", collection[0].Mode);
			AssertEquals("MEL", collection[0].Branch);
			AssertEquals("OTH", collection[0].Business);
			AssertEquals(250m, collection[0].Amount);
			AssertEquals("JPY", collection[0].TransactionCurrency);
			AssertEquals(1000m, collection[0].TransactionAmount);
			AssertEquals("SEA", collection[0].Geographical);
		}
	}
}
