using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(GlbExternalPasswordNMC))]
	sealed class GlbExternalPasswordNMCTest : GlbExternalPasswordWithPasswordTypeTest<GlbExternalPasswordNMC>
	{
		public override void TestPasswordTypeCodeAndDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals(JPPasswordType.Codes.NMC, GlbExternalPassword.PasswordTypeCode);
				AssertEquals(JPPasswordType.Descriptions.NMC, GlbExternalPassword.PasswordTypeDescription);
			});
		}

		public void TestSetDefaultValues()
		{
			var credential = Factory.New<GlbExternalPasswordNMC>();
			AssertEquals(true, credential.ShouldReceive);
		}

		public void TestShouldReceive()
		{
			var credential = Factory.New<GlbExternalPasswordNMC>();
			Assert(credential.ShouldReceiveInfo.ReadOnly);

			credential.GP_MailBoxID = "TEST@TEST.COM";
			Assert(!credential.ShouldReceiveInfo.ReadOnly);

			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, GlbExternalPasswordNMC.GenAddOnColumnConstants.ShouldReceiveColumnName);
			var persistedObj = Factory.LoadTop1<GenAddOnColumn>(query);

			AssertNotNull(persistedObj);
			AssertEquals("Y", persistedObj.XA_Data);
		}

		public void TestGP_PasswordStatus()
		{
			var credential = Factory.New<GlbExternalPasswordNMC>();
			Assert(credential.GP_PasswordStatusInfo.ReadOnly);
		}

		public void TestNACCSMailDomain()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var configType1 = helper.CreateOrGetExistingRefSysConfigType("NACCSMailP", "NACCS Mail Prod", "NACCS Mail Prod");
			var configType2 = helper.CreateOrGetExistingRefSysConfigType("NACCSMailT", "NACCS Mail Test", "NACCS Mail Test");
			helper.CreateOrUpdateExistingRefSysConfig(configType1.ZRT_ConfigCode, "NACCS@Mail.Prod.NACCS6", ZDateTime.Now.AddMonths(-1), ZDateTime.Now.AddMonths(1));
			helper.CreateOrUpdateExistingRefSysConfig(configType2.ZRT_ConfigCode, "NACCS@Mail.Test.NACCS6", ZDateTime.Now.AddMonths(-1), ZDateTime.Now.AddMonths(1));

			var credential = Factory.New<GlbExternalPasswordNMC>();
			AssertEquals("@Mail.Test.NACCS6", credential.MailboxDomain);

			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			AssertEquals("@Mail.Prod.NACCS6", credential.MailboxDomain);
		}

		public void TestValidationType()
		{
			AssertType<GlbExternalPasswordNMCValidation>(GlbExternalPassword.Validation);
		}

		public void TestLookupsType()
		{
			AssertType<GlbExternalPasswordNMCLookups>(GlbExternalPassword.Lookups);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<GlbExternalPasswordNMC>();
		}
	}
}
