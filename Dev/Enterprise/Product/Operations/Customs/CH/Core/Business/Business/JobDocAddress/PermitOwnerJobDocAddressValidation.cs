using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

sealed class PermitOwnerJobDocAddressValidation : JobDocAddressValidation
{
	public PermitOwnerJobDocAddressValidation(AutoJobDocAddress parent, Restriction restriction)
		: base(parent)
	{
		this.restriction = restriction;
	}

	readonly Restriction restriction;

	protected override void CheckE2_OA_Address()
	{
		base.CheckE2_OA_Address();
		var parent = Parent;

		var plausiValidation = PlausiValidation.New(((JobComInvoiceLine)restriction.Parent).Declaration);
		plausiValidation.CheckNS30103(parent.E2_OA_AddressInfo, restriction);
	}
}
