using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class SupportingDocSendingObjectValidation : Customs.Business.SupportingDocSendingObjectValidation
{
	public SupportingDocSendingObjectValidation(AutoSupportingDocSendingObject parent) : base(parent)
	{
	}

	new SupportingDocSendingObject Parent => (SupportingDocSendingObject)base.Parent;

	public override ZInt MaxEDocFileSizeInBytes => 10_000_000;

	public override ZString EDocTooLargeError => Res.GetString("556a9b7d-d21b-415e-aab8-86d6f7dff9c4", "The file size is too large, you can submit up to a 10MB file. You may split the file and submit multiple files to NAIC.");

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateCUSRES();
		ValidateCUSCAR();
	}

	protected override void CheckDocumentType()
	{
		if (!allowedTypes.Contains(Parent.DocumentType.ToLower()))
		{
			Parent.DocumentTypeInfo.AddError(Res.GetString("fc3ddd47-06bc-404d-95d1-2ddb45fdba91", "The file type is not supported. Please convert document to one of the supported types - jpeg, jpg, png, pdf."));
		}
	}

	readonly ZString[] allowedTypes = [".jpeg", ".jpg", ".png", ".pdf"];

	protected override void CheckLocalReferenceNumber()
	{
	}

	public void ValidateCUSRES()
	{
		ValidateCalculatedProperty(Parent.CUSRESInfo);
	}

	internal void CheckCUSRES()
	{
		MandatoryValidation.CheckEntered(Parent.CUSRESInfo);
		ListValidation.ErrorIfInvalidCode(Parent.CUSRESInfo);
	}

	public void ValidateCUSCAR()
	{
		ValidateCalculatedProperty(Parent.CUSCARInfo);
	}

	internal void CheckCUSCAR()
	{
		MandatoryValidation.CheckEntered(Parent.CUSCARInfo);
	}
}
