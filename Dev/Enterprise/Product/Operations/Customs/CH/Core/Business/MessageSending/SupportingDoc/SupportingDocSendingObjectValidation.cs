using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

internal class SupportingDocSendingObjectValidation : Customs.Business.SupportingDocSendingObjectValidation
{
	public SupportingDocSendingObjectValidation(SupportingDocSendingObject parent) : base(parent)
	{
	}

	new SupportingDocSendingObject Parent => (SupportingDocSendingObject)base.Parent;

	CHCustomsDataRegistry Registry => CHCustomsDataRegistry.Instance;

	protected override void CheckEDoc()
	{
		base.CheckEDoc();

		if (Parent.EDoc.IsValid)
		{
			var fileName = Parent.Document?.FileName.ToString();
			if (fileName != null)
			{
				var allowedFileExtensions = Registry.AllowedFileExtensions.Value;
				if (!allowedFileExtensions.Any(ext => fileName.EndsWith("." + ext, System.StringComparison.InvariantCultureIgnoreCase)))
				{
					Parent.EDocInfo.AddError(ValidationMessages.SupportingDocSendingObject.InvalidFileExtension(allowedFileExtensions));
				}
			}
		}
	}

	public override ZInt MaxEDocFileSizeInBytes => Registry.MaximumFileSize.Value * 1_048_576;

	public override ZString EDocTooLargeError => ValidationMessages.SupportingDocSendingObject.MaxAllowedFileSizeExceeded(Registry.MaximumFileSize.Value);
}
