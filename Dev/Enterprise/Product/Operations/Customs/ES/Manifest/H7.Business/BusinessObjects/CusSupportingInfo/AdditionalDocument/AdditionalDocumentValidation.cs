using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AdditionalDocumentValidation : EU.H7.Business.AdditionalDocumentValidation
	{
		public AdditionalDocumentValidation(AdditionalDocument parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			var codeValue = Parent.CSI_Code;
			var referenceNumberValue = Parent.CSI_ReferenceNumber;
			var propertyInfo = Parent.CSI_CodeInfo;
			if (codeValue.IsEmpty && !referenceNumberValue.IsEmpty)
			{
				var notEnteredMessageError = Res.GetString("87d29222-b059-48ad-acdf-474c0fa6a3db",
					"You have not entered a Transport Document Type.");
				propertyInfo.AddMessageError(notEnteredMessageError);
				return;
			}
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			var codeValue = Parent.CSI_Code;
			var referenceNumberValue = Parent.CSI_ReferenceNumber;
			var propertyInfo = Parent.CSI_ReferenceNumberInfo;
			if (referenceNumberValue.IsEmpty && !codeValue.IsEmpty)
			{
				var notEnteredMessageError = Res.GetString("2990cdc0-89c8-4b5c-add7-3dee9c6ab4c2",
					"You have not entered a Transport Document Reference.");
				propertyInfo.AddMessageError(notEnteredMessageError);
				return;
			}
			if (!referenceNumberValue.IsLettersAndNumbersOnlyOrEmpty)
			{
				var notAlphanumericMessageError = Res.GetString("54f6619e-0657-4102-9484-addc4638b6f4",
					"Transport Document Reference must be alphanumeric.");
				propertyInfo.AddMessageError(notAlphanumericMessageError);
			}
		}
	}
}
