using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing
{
	[TestedType(typeof(CusTempStorageRegPremises))]
	sealed class CusTempStorageRegPremisesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusTempStorageRegPremisesLookups>(premises.Lookups);
		}

		public void TestAuthorizationType()
		{
			var cusTempStorageRegPremisesForTest = Factory.New<CusTempStorageRegPremisesForTest>();
			AssertType<ES.Business.CusAuthorizationUsage>(cusTempStorageRegPremisesForTest.AuthorizationExposed);
		}

		public void TestDefaultAuthorizationCode()
		{
			var cusTempStorageRegPremisesForTest = Factory.New<CusTempStorageRegPremisesForTest>();
			CombineAssertions(() =>
			{
				cusTempStorageRegPremisesForTest.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
				AssertEquals("When SRP_Type is LAM, DefaultAuthorizationCode = LAME", ESCusAuthorisationHeaderTypeList.Codes.PremisesAuthorizedForExport, cusTempStorageRegPremisesForTest.DefaultAuthorizationCodeCoreExposed);
				cusTempStorageRegPremisesForTest.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
				AssertEquals("When SRP_Type it not LAME,  DefaultAuthorizationCode = TST", AuthorizationTypeList.Codes.TST, cusTempStorageRegPremisesForTest.DefaultAuthorizationCodeCoreExposed);
			});
		}

		public void TestDefaultSRP_Types()
		{
			var cusTempStorageRegPremisesForTest = Factory.New<CusTempStorageRegPremisesForTest>();
			CombineAssertions(() =>
			{
				cusTempStorageRegPremisesForTest.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
				AssertEquals("When SRP_Type is LAM, authorization.AGC_Code = LAME", ESCusAuthorisationHeaderTypeList.Codes.PremisesAuthorizedForExport, cusTempStorageRegPremisesForTest.AuthorizationExposed.AGC_Code);
				cusTempStorageRegPremisesForTest.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
				AssertEquals("When SRP_Type it not LAME,  authorization.AGC_Code = TST", AuthorizationTypeList.Codes.TST, cusTempStorageRegPremisesForTest.AuthorizationExposed.AGC_Code);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			premises = Factory.New<CusTempStorageRegPremises>();
		}

		CusTempStorageRegPremises premises;

		class CusTempStorageRegPremisesForTest : CusTempStorageRegPremises
		{
			public CusTempStorageRegPremisesForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public EU.Business.CusAuthorizationUsage AuthorizationExposed => Authorization;

			public ZString DefaultAuthorizationCodeCoreExposed => DefaultAuthorizationCodeCore;
		}
	}
}
