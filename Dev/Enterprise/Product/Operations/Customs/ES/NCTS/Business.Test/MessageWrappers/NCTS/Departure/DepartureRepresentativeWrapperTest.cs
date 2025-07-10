using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class DepartureRepresentativeWrapperTest : WrapperHelperTest<DepartureRepresentativeWrapper>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				NctsHeader header = null;
				AssertNull("Header null => representative is null", DepartureRepresentativeWrapper.New(header));
				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header.DeclarantAddressPK = ZGuid.Empty;
				AssertNull("Header with null declarant => representative is null", DepartureRepresentativeWrapper.New(header));

				var orgHeaderDeclarant = Factory.NewWithValidTestData<OrgHeader>();
				header.DeclarantAddressPK = orgHeaderDeclarant.MainAddress.PK;
				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222223");
				orgHeaderDeclarant.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertNull("Header with declarant specified but representative not specified => representative is null", DepartureRepresentativeWrapper.New(header));

				var orgHeaderRepresentative = Factory.NewWithValidTestData<OrgHeader>();
				header.MovementHeader.Representative.OrganisationPK = orgHeaderRepresentative.PK;
				orgHeaderRepresentative.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderRepresentative.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertNotNull("Header with representative specified => representative is not null", DepartureRepresentativeWrapper.New(header));

				var orgHeaderPrincipal = Factory.NewWithValidTestData<OrgHeader>();
				header.Principal.OrganisationPK = orgHeaderPrincipal.PK;
				orgHeaderPrincipal.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderPrincipal.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				AssertNull("Header with declarant specified but with the same ID as Principal => representative is null", DepartureRepresentativeWrapper.New(header));

				orgHeaderPrincipal.OH_Category = OrgConstants.Category.Business;

				AssertNotNull("Header with declarant specified but with differenct ID as Principal => representative is not null", DepartureRepresentativeWrapper.New(header));
			});
		}

		protected override DepartureRepresentativeWrapper GetProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var orgHeaderRepresentative = Factory.NewWithValidTestData<OrgHeader>();
			header.MovementHeader.Representative.OrganisationPK = orgHeaderRepresentative.PK;
			orgHeaderRepresentative.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeaderRepresentative.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			return DepartureRepresentativeWrapper.New(header);
		}
	}
}

