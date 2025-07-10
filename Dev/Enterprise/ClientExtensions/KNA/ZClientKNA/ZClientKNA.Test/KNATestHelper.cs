using CargoWise.EntityFramework;
using Enterprise.ClientSharedComponents;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.KNA
{
	internal class KNATestHelper : SharedTestHelper
	{
		internal KNATestHelper() : base() { }

		internal KNATestHelper(BusinessObjectFactory factory) : base(factory) { }

	  public void TestCheckPartsThatAlreadyExistAreExcluded()
	  {
			ClearCustomsRecordsBeforeTesting();

			OrgHeader testBuyer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));

			OrgSupplierPart testPart1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart1.OP_PartNum = "Part with Owner";
			OrgSupplierPart testPart2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart2.OP_PartNum = "Part with Owner and Supplier";
			OrgSupplierPart testPart3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart3.OP_PartNum = "Part with Supplier";
			OrgSupplierPart testPart4 = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart4.OP_PartNum = "Part with Both";

			OrgPartRelation relation1 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation1.OU_OP = testPart1.PK;
			relation1.OU_OH = testBuyer.PK;
			relation1.OU_Relationship = "OWN";

			OrgPartRelation relation2 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation2.OU_OP = testPart2.PK;
			relation2.OU_OH = testBuyer.PK;
			relation2.OU_Relationship = "OWN";

			OrgPartRelation relation3 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation3.OU_OP = testPart2.PK;
			relation3.OU_OH = testSupplier.PK;
			relation3.OU_Relationship = "SUP";

			OrgPartRelation relation4 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation4.OU_OP = testPart3.PK;
			relation4.OU_OH = testSupplier.PK;
			relation4.OU_Relationship = "SUP";

			OrgPartRelation relation5 = Factory.NewWithValidTestData<OrgPartRelation>();
			relation5.OU_OP = testPart4.PK;
			relation5.OU_OH = testSupplier.PK;
			relation5.OU_Relationship = "BTH";

		 Factory.Save();
		}

		internal void ClearCustomsRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
			TestCaseHelper.ClearTable("OrgPartRelation");
			TestCaseHelper.ClearTable("OrgSupplierPart");
		}

		internal void SetupEnvironment()
		{
			Classification classification = Factory.New<Classification>();
			classification.CC_LookupCode = "9505A";
			classification.CC_ClassificationType = "IMP";

			OrgHeader testBuyer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "S"));
			testBuyer.OH_Code = "SUPGENSYD";

			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			testSupplier.OH_Code = "199POPSYD";

			Factory.Save();
		}
	}
}
