using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(BusinessObject.Loader))]
	class CusStatementHeaderLoaderTest : LoaderTestCase
	{
		public void TestNotLoadStatementTypeRSF()
		{
			GlbCompany otherCACompany = Factory.New<GlbCompany>();
			otherCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			GlbBranch otherCABranch = otherCACompany.Branches.AddNew();
			otherCABranch.GB_Code = "OTT";
			otherCABranch.GB_RL_NKHomePort = "CAOTT";

			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_EntryFilerCode = "12345";
			header1.B2_ProcessPort = "123";
			header1.B2_ProcessDate = new ZDateTime(2011, 08, 01);
			header1.B2_PaymentType = "BRK";
			header1.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			Factory.Save();

			AssertNull(loader.Load(null, "12345", "123", new ZDateTime(2011, 08, 01), "BRK"));
		}

		public void TestCreateNewCusStatementHeaderFilter()
		{
			AssertNotNull(loader.CreateNewCusStatementHeaderFilter(null, "12345", "123", new ZDateTime(2011, 08, 01), "IMP"));
		}

		public void TestLoadCusStatementHeader()
		{
			GlbCompany otherCACompany = Factory.New<GlbCompany>();
			otherCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			GlbBranch otherCABranch = otherCACompany.Branches.AddNew();
			otherCABranch.GB_Code = "OTT";
			otherCABranch.GB_RL_NKHomePort = "CAOTT";

			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_EntryFilerCode = "12345";
			header1.B2_ProcessPort = "123";
			header1.B2_ProcessDate = new ZDateTime(2011, 08, 01);
			header1.B2_PaymentType = "BRK";
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var header2 = Factory.New<CusStatementHeader>();
			header2.B2_OH_Importer = importer.PK;
			header2.B2_EntryFilerCode = "12345";
			header2.B2_ProcessPort = "123";
			header2.B2_ProcessDate = new ZDateTime(2011, 08, 01);
			header2.B2_PaymentType = "IMP";
			header2.B2_GC = otherCACompany.PK;
			Factory.Save();

			AssertEquals(header1.PK, loader.Load(null, "12345", "123", new ZDateTime(2011, 08, 01), "BRK").PK);
			AssertEquals(header2.PK, loader.Load(importer, "12345", "123", new ZDateTime(2011, 08, 01), "IMP").PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new CusStatementHeaderLoader(Factory);
		}
		CusStatementHeaderLoader loader;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusStatementHeaderLoader(Factory);
		}
	}
}
