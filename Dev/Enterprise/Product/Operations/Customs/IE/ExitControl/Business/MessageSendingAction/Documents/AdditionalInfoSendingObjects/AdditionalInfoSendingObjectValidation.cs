using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AdditionalInfoSendingObjectValidation : ZValidation
	{
		public AdditionalInfoSendingObjectValidation(AdditionalInfoSendingObject parent) : base(parent)
		{
			Parent = parent;
		}

		AdditionalInfoSendingObject Parent { get; }

		public override Type AutoValidationType => typeof(AdditionalInfoSendingObjectValidation);

		public override void ValidateAll()
		{
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateDocumentType();
				ValidateDocumentInformation();
			}
		}

		public void ValidateDocumentType()
		{
			ValidateCalculatedProperty(Parent.DocumentTypeInfo);
		}

		protected void CheckDocumentType()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
		}

		public void ValidateDocumentInformation()
		{
			ValidateCalculatedProperty(Parent.DocumentInformationInfo);
		}

		protected void CheckDocumentInformation()
		{
			MandatoryValidation.CheckEntered(Parent.DocumentInformationInfo);
		}
	}
}
