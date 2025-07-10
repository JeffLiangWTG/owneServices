using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaGJobDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestDeltaModeListWhenJE_ApplicationCodeIsDeltaG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", ZString.Empty, ZString.Empty, ZString.Empty, "5DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "B", ZString.Empty, ZString.Empty, ZString.Empty, "3E93054B");
			declaration.SetupSupplier().SetupAccount(OrgCusAccountCodeList.Codes.DGE, "C", ZString.Empty, ZString.Empty, ZString.Empty, "6DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGE, "D", ZString.Empty, ZString.Empty, ZString.Empty, "4E93054B");
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaG();
			AssertEquals("The DeltaG mode list for import should contain account types of Importer and Declarant DGI configuration.", "A, B", declaration.Lookups.DeltaModeList.CodesAsString);
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Export).WithDeltaG();
			AssertEquals("The DeltaG mode list for export should contain account types of Supplier and Declarant DGE configuration.", "C, D", declaration.Lookups.DeltaModeList.CodesAsString);
		}

		public void TestProfileList()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaG();
			declaration.Importer.OH_FullName = "Importer_Full_Name";
			declaration.Importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", "CUSOF001", ZString.Empty, "604D8AFA");
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "Declarant_Full_Name";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", "CUSOF003", ZString.Empty, "C447912A");
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G2, "DGE002", "CUSOF004", ZString.Empty, "C447912A");
			declaration.SetupDeclarant(declarant.MainAddress);
			var list = declaration.Lookups.ProfileList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "DGI001, DGI002", list.CodesAsString);
				AssertEquals("Importer Importer_Full_Name, Delta G1, CUSOF001", list["DGI001"].Description);
				AssertEquals("Declarant Declarant_Full_Name, Delta G2, CUSOF003", list["DGI002"].Description);
				AssertSame("Cached", list, declaration.Lookups.ProfileList);
			});

			declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Export).WithDeltaG();
			declaration.Supplier.OH_FullName = "Supplier_Full_Name";
			declaration.Supplier.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", "CUSOF002", ZString.Empty, "604D8AFA");
			declaration.SetupDeclarant(declarant.MainAddress);
			list = declaration.Lookups.ProfileList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "DGE001, DGE002", list.CodesAsString);
				AssertEquals("Supplier Supplier_Full_Name, Delta G1, CUSOF002", list["DGE001"].Description);
				AssertEquals("Declarant Declarant_Full_Name, Delta G2, CUSOF004", list["DGE002"].Description);
				AssertSame("Cached", list, declaration.Lookups.ProfileList);
			});
		}

		public void TestDeltaGDataGroupingForVATCANA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var lookups = declaration.Lookups;
			AssertEquals("Data grouping for DeltaG VAT CANA code should be FR", "FR", lookups.DataGroupingForVATCANA);
		}
	}
}
