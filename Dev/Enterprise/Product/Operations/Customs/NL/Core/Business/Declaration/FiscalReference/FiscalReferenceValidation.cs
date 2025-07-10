using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business.Declaration;

public class FiscalReferenceValidation : Customs.Business.CusSupportingInfoValidation
{
	public FiscalReferenceValidation(FiscalReference parent)
		: base(parent)
	{
	}

	protected new FiscalReference Parent => (FiscalReference)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateEntryInstructionID();
	}

	public void ValidateEntryInstructionID() => ValidateCalculatedProperty(Parent.EntryInstructionIDInfo);

	public static string CSI_CodeInfoEmpty => Res.GetString("B3C4E769-0A39-4CFA-884F-33DFFAE94D69", "Please enter a Type Code.");

	public static string CSI_ReferenceInfoEmpty => Res.GetString("DE4E0C99-AF92-40E2-B51E-6E3DD2D5C54F", "Please enter a Holder EORI.");

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		if (Parent.CSI_Code.IsEmpty)
		{
			Parent.CSI_CodeInfo.AddMessageError(CSI_CodeInfoEmpty);
		}
		else
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		if (Parent.CSI_ReferenceNumber.IsEmpty)
		{
			Parent.CSI_ReferenceNumberInfo.AddMessageError(CSI_ReferenceInfoEmpty);
		}
	}
}
