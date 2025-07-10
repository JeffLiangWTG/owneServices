using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentSendingObjectValidation : CommonDocumentSendingObjectValidation
	{
		public DocumentSendingObjectValidation(SupportingDocSendingObject parent) : base(parent)
		{
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateFileDescription();
		}

		public override ZInt MaxEDocFileSizeInBytes => 10_000_000;

		public void ValidateFileDescription()
		{
			ValidateCalculatedProperty(Parent.FileDescriptionInfo);
		}

		protected void CheckFileDescription()
		{
			var parent = Parent;
			if (parent.ParentSendingObject is AdditionalInfoSendingObject sendingObject && sendingObject.Action is { } action && action.ShouldSend)
			{
				MandatoryValidation.CheckEntered(parent.FileDescriptionInfo);
			}
		}

		protected override void CheckEDoc()
		{
			var parent = Parent;
			if (parent.ParentSendingObject is AdditionalInfoSendingObject sendingObject && sendingObject.Action is { } action && action.ShouldSend)
			{
				base.CheckEDoc();
			}
			if (parent.EDoc.IsValid && Parent.Document?.FileName is ZString fileName && !allowedTypes.Any(ext => fileName.EndsWith(ext, System.StringComparison.InvariantCultureIgnoreCase)))
			{
				parent.EDocInfo.AddError(Res.GetString("E1E26F71-2738-45EA-845C-7A4B993F03DC", "The file type is not supported. Please convert document to one of the supported types - pdf, png, gif, jpeg, zip, 7z, docx, xlsx or pptx."));
			}
		}

		readonly ZString[] allowedTypes = [".pdf", ".png", ".gif", ".jpeg", ".zip", ".7z", ".docx", ".xlsx", ".pptx"];

		protected override void CheckEDocIsValidZGuid()
		{
			var parent = Parent;
			if (parent.ParentSendingObject is AdditionalInfoSendingObject sendingObject && sendingObject.Action is { } action && action.ShouldSend)
			{
				base.CheckEDocIsValidZGuid();
			}
		}

		new DocumentSendingObject Parent => (DocumentSendingObject)base.Parent;
	}
}
