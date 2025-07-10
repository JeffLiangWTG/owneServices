namespace Enterprise.Customs.CH.Business;

public class ExportRestrictionValidation : RestrictionValidation
{
	public ExportRestrictionValidation(Restriction parent) : base(parent)
	{
	}

	protected PlausiValidation PlausiValidation => plausiValidation ??= PlausiValidation.New((JobComInvoiceLine)Parent.Parent);
	PlausiValidation plausiValidation;

	new Restriction Parent => (Restriction)base.Parent;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		PlausiValidation.CheckNS30003_CusSupportingInfo(Parent, PassarValidationMessages.MessageNS30003_Restriction);
		PlausiValidation.CheckNS30116(Parent);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		PlausiValidation.CheckNS30120(Parent.CSI_ReferenceNumberInfo, Parent);
		PlausiValidation.CheckNS30001_CSI_ReferenceNumber(Parent.CSI_ReferenceNumberInfo, Parent);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		PlausiValidation.CheckNS30001_PermitExceptionReason(Parent.CSI_DescriptionInfo, Parent);
	}
}
