using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class GBGuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryInstructions()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;

			var list = guarantee.Lookups.EntryInstructions;
			AssertEquals(2, list.Count);
			AssertEquals(1, list.Cast<CusEntryInstruction>().Count(x => x.CEI_Style == ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse));
			AssertEquals(1, list.Cast<CusEntryInstruction>().Count(x => x.CEI_Style == ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing));
		}

		public void TestBondTypeList()
		{
			AssertEquals("G", guarantee.Lookups.BondTypeList.CodesAsString);
			AssertSame(Factory.New<GBGuarantee>().Lookups.BondTypeList, guarantee.Lookups.BondTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			guarantee = declaration.Guarantees.AddNew();
		}
		JobDeclaration declaration;
		GBGuarantee guarantee;
	}
}
