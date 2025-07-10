using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class GuaranteeForDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryInstructions()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "A";

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "B";

			var list = guarantee.Lookups.EntryInstructions;
			AssertEquals(2, list.Count);
			AssertEquals(list[0].CEI_Style, entryInstruction1.CEI_Style);
			AssertEquals(list[1].CEI_Style, entryInstruction2.CEI_Style);
		}

		public void TestHolderIdentificationList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals(ZString.Empty, guarantee.Lookups.HolderIdentificationList.CodesAsString);

				var declaration = Factory.New<JobDeclaration>();
				guarantee = declaration.Guarantees.AddNew();

				AssertEquals(ZString.Empty, guarantee.Lookups.HolderIdentificationList.CodesAsString);

				var importer = CreateOrgHeader("IMP11111111", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var declarant = CreateOrgHeader("DEC22222222", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var representative = CreateOrgHeader("REP22222222", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var buyer = CreateOrgHeader("BUY22222222", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

				declaration.JE_OH_Importer = importer.PK;
				declaration.Declarant.OA_OH = declarant.PK;
				declaration.RepresentativeDocAddress.OrganisationPK = representative.PK;

				AssertEquals("GBDEC22222222, GBIMP11111111", guarantee.Lookups.HolderIdentificationList.CodesAsString);

				declaration.JE_OA_Representative = representative.MainAddress.PK;
				declaration.JE_OH_Buyer = buyer.PK;

				var holderIds = guarantee.Lookups.HolderIdentificationList;
				holderIds.Sort();

				AssertEquals("GBBUY22222222, GBDEC22222222, GBIMP11111111, GBREP22222222", holderIds.CodesAsString);
			}
		}
		OrgHeader CreateOrgHeader(string code, string type)
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(type, code);
			return org;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			guarantee = declaration.Guarantees.AddNew();
		}
		JobDeclaration declaration;
		GuaranteeForDeclaration guarantee;
	}
}
