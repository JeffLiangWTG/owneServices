using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentSendingActionValidation : ZValidation
	{
		public DocumentSendingActionValidation(DocumentSendingAction parent) : base(parent)
		{
			Parent = parent;
		}

		public DocumentSendingAction Parent;

		public override Type AutoValidationType => typeof(DocumentSendingActionValidation);

		public override void ValidateAll()
		{
			CheckAdditionalInfos();
			CheckSupportingDocuments();
		}

		void CheckAdditionalInfos()
		{
			if (!Parent.IsValidationSuspended)
			{
				var message = Res.GetString("A72B12BD-3827-4F8D-9379-F77C0BC92066", "Cannot send message without any Additional Informations.");
				Parent.RemoveRowError(message);
				if (!Parent.AddInfoCollection.Any())
				{
					Parent.AddRowError(message);
				}
			}
		}

		void CheckSupportingDocuments()
		{
			if (!Parent.IsValidationSuspended)
			{
				var message = Res.GetString("B5F3C9B5-329D-4730-A670-03013B84448B", "Cannot send message without any eDoc selected.");
				Parent.RemoveRowError(message);
				if (!Parent.SupportingDocuments.Any())
				{
					Parent.AddRowError(message);
				}
			}
		}
	}
}
