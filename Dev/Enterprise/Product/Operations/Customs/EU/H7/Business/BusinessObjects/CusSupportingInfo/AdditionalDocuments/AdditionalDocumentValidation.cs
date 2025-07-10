using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalDocumentValidation : CusSupportingInfoValidation
	{
		public AdditionalDocumentValidation(AdditionalDocument parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var codeValue = Parent.CSI_Code;
			var referenceNumberValue = Parent.CSI_ReferenceNumber;
			var propertyInfo = Parent.CSI_CodeInfo;
			if (codeValue.IsEmpty && !referenceNumberValue.IsEmpty)
			{
				var notEnteredMessageError = Res.GetString("FAF364D3-834B-459F-ACE0-A358C5301FE3",
					"You have not entered a type.");
				propertyInfo.AddMessageError(notEnteredMessageError);
				return;
			}
			if (codeValue.Length != 4)
			{
				var notAlphanumericMessageError = Res.GetString("5B3C517C-C536-442C-82CB-5E28217A1AA8",
					"Type must be a 4-character alphanumeric code.");
				propertyInfo.AddMessageError(notAlphanumericMessageError);
				return;
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var codeValue = Parent.CSI_Code;
			var referenceNumberValue = Parent.CSI_ReferenceNumber;
			var propertyInfo = Parent.CSI_ReferenceNumberInfo;
			if (referenceNumberValue.IsEmpty && !codeValue.IsEmpty)
			{
				var notEnteredMessageError = Res.GetString("08DC5A94-07D2-4590-8FDC-10EE32C7F424",
					"You have not entered a reference number.");
				propertyInfo.AddMessageError(notEnteredMessageError);
				return;
			}
			if (referenceNumberValue.Length > 70)
			{
				var notAlphanumericMessageError = Res.GetString("FE719CC6-B0C1-442F-9B02-59375FC8A448",
					"Reference number length cannot exceed 70 alphanumeric characters.");
				propertyInfo.AddMessageError(notAlphanumericMessageError);
			}
		}
	}
}
