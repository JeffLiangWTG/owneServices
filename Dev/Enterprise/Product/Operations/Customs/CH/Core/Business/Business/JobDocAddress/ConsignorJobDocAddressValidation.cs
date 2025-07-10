using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

sealed class ConsignorJobDocAddressValidation : JobDocAddressValidation
{
	public ConsignorJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration)
		: base(parent)
	{
		this.declaration = declaration;
	}

	readonly JobDeclaration declaration;

	protected override void CheckE2_OA_Address()
	{
		base.CheckE2_OA_Address();
		var parent = Parent;

		var plausiValidation = PlausiValidation.New(declaration);
		plausiValidation.CheckNP70065(parent.E2_OA_AddressInfo, parent.Address?.Postcode, PassarValidationMessages.MessageNP70065_Consignor);
	}
}
