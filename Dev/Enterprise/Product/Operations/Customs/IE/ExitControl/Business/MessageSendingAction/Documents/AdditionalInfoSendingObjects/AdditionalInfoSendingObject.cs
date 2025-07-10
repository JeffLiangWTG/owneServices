using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AdditionalInfoSendingObject : NonPersistentBusinessObject<AdditionalInfoSendingObjectValidation>
	{
		public AdditionalInfoSendingObject(string documentType)
		{
			DocumentType = documentType;
		}

		[MaxLength(512)]
		[ResourceStringData("6A1C2B07-7976-4790-A2D5-83AA33E5EC59", Caption = "Document Type", ShortCaption = "Type")]
		public ZString DocumentType
		{
			get => documentType;
			set
			{
				if (documentType != value)
				{
					SetNonPersistentPropertyValue(DocumentTypeInfo, ref documentType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDocumentType();
					}
				}
				DocumentTypeInfo.RefreshBinding();
			}
		}
		ZString documentType;
		public ZPropertyInfo DocumentTypeInfo => GetZPropertyInfo(nameof(DocumentType));

		[MaxLength(512)]
		[ResourceStringData("41A2C0AF-F00C-4B3E-9141-78E560B037A3", Caption = "Document Information", ShortCaption = "Info.")]
		public ZString DocumentInformation
		{
			get => documentInformation;
			set
			{
				if (documentInformation != value)
				{
					SetNonPersistentPropertyValue(DocumentInformationInfo, ref documentInformation, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDocumentInformation();
					}
				}
				DocumentInformationInfo.RefreshBinding();
			}
		}
		ZString documentInformation;
		public ZPropertyInfo DocumentInformationInfo => GetZPropertyInfo(nameof(DocumentInformation));

		#region Validations

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("0F0B1D33-4065-4E25-9DC1-25D22CEF3993", "Additional Info");

		public override AdditionalInfoSendingObjectValidation GetNewValidation() => new AdditionalInfoSendingObjectValidation(this);

		#endregion
	}
}
