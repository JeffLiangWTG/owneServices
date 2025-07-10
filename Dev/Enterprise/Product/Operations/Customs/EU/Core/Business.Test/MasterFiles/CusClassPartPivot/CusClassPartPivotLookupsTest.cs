using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	public class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		class CusClassPartPivotForTest : CusClassPartPivot
		{
			public CusClassPartPivotForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString DataGroupingCodeForAdditionalProcedures => "CDS";
		}

		public void TestFilteredCPCCollection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = org.PK;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.Both;

			var currentCountry = GlbCompany.CurrentCompany.Country.Code;  // Latvia for base EU tests. 
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cpc1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP");
			var cpc2 = helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP");
			var cpc3 = helper.CreateRefCusProcedure(currentCountry, "A", "33", "33", "333", "Three", "EXP");
			pivot.CI_ChildType = ClassificationType.IMP;
			AssertEquals(2, pivot.Lookups.RefCusProcedureCollection.Count);
			Assert(pivot.Lookups.RefCusProcedureCollection.Contains(cpc1));
			Assert(pivot.Lookups.RefCusProcedureCollection.Contains(cpc2));
			Assert(!pivot.Lookups.RefCusProcedureCollection.Contains(cpc3));
			pivot.CI_ChildType = ClassificationType.EXP;
			AssertEquals(1, pivot.Lookups.RefCusProcedureCollection.Count);
			Assert(!pivot.Lookups.RefCusProcedureCollection.Contains(cpc1));
			Assert(!pivot.Lookups.RefCusProcedureCollection.Contains(cpc2));
			Assert(pivot.Lookups.RefCusProcedureCollection.Contains(cpc3));
			pivot.CI_ChildType = ClassificationType.Both;
			AssertEquals(0, pivot.Lookups.RefCusProcedureCollection.Count);
		}

		public void TestAdditionalCDCs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "A", "11", "11", "111", "One", "IMP", group: "CDS");
			helper.CreateRefCusProcedure("CDS", "A", "11", "11", "112", "Two", "EXP", group: "CDS");
			helper.CreateRefCusProcedure("CDS", "A", "11", "11", "113", "Two", "EXP", group: "CCC");
			helper.CreateRefCusProcedure("CDS", "A", "33", "33", "333", "Three", "IMP", group: "CDS");

			var pivot = Factory.New<CusClassPartPivotForTest>();
			pivot.CI_ChildType = "IMP";
			pivot.CI_CPC = "111";
			var additionalCPCs = pivot.Lookups.AdditionalCPCs;
			AssertEquals(0, additionalCPCs.Count);

			pivot.CI_CPC = "1111111";
			additionalCPCs = pivot.Lookups.AdditionalCPCs;
			AssertEquals(1, additionalCPCs.Count);
			Assert(additionalCPCs.ContainsCode("1111111"));
		}
	}
}
