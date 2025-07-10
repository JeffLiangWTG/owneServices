using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportAdditionalInfoValidation : AdditionalInfoValidation
	{
		public ExportAdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;
			var targetInfo = parent.CSI_ReferenceNumberInfo;

			if (parent.IsRoroRoroUnaccompaniedTrailer)
			{
				if (!referenceNumber.IsLettersAndNumbersOnlyOrEmpty || referenceNumber.Length < 4 || referenceNumber.Length > 32)
				{
					targetInfo.AddMessageError(Res.GetString("252F84B3-B521-4F62-8F46-78CEBA69602C", "Format for registration number must be alphanumeric with minimum 4 characters and max 32 characters."));
				}
			}
			else if (parent.IsRoRoShipID)
			{
				var vesselNumberValidation = new LloydsNumberValidation();
				vesselNumberValidation.Validate(referenceNumber);
				if (!vesselNumberValidation.IsValid)
				{
					targetInfo.AddMessageError(vesselNumberValidation.ErrorText);
				}
			}
			else if (referenceNumber.Length > 35 && parent.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod)
			{
				targetInfo.AddMessageError(Res.GetString("D327E508-E623-483C-88FF-216C32928FDD", "Reference Number of Additional Reference can have up to 35 alpha numeric characters."));
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			if (Parent.IsRoroRoroUnaccompaniedTrailer)
			{
				if (Parent.Declaration is JobDeclaration declaration && !declaration.IsSea && Parent.IsAdditionalReferenceOnDeclaration)
				{
					Parent.CSI_CodeInfo.AddMessageError(Res.GetString("693BB127-2056-4B87-AD01-86255CF039B0", "1D95 for Ro-Ro unaccompanied trailer can only be used when mode of transport at the border is Sea."));
				}
			}
			else if (Parent.IsRoRoShipID)
			{
				if (Parent.Declaration is JobDeclaration declaration && !declaration.IsRoad && Parent.IsAdditionalReferenceOnDeclaration)
				{
					Parent.CSI_CodeInfo.AddMessageError(Res.GetString("7ADDCFFD-76EF-4182-BAD8-9E9AC45C3083", "1D94 for Ro-Ro accompanied can only be used when mode of transport at the border is Road."));
				}
			}
		}
	}
}
