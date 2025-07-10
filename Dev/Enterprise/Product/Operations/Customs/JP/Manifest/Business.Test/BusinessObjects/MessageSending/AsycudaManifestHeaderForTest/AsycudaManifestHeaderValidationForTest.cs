using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderValidationForTest : BaseAsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidationForTest(AsycudaManifestHeaderForTest parent) : base(parent)
		{
		}

		AsycudaManifestHeaderForTest Header => (AsycudaManifestHeaderForTest)Parent;

		public override void ValidateAll()
		{
			ValidateAMA_ManifestType();
			ValidateAMA_CustomsOffice();
		}

		protected override void CheckAMA_ManifestType()
		{
			if (Header.CreateErrorForTest)
			{
				Parent.AMA_ManifestTypeInfo.AddError("Test error on AMA_ManifestType.");
			}

			if (Header.CreateMessageErrorForTest)
			{
				Parent.AMA_ManifestTypeInfo.AddMessageError("Test message error on AMA_ManifestType.");

				if (Header.IsNVC01SendingInProgress)
				{
					Parent.AMA_ManifestTypeInfo.AddError("Test message error on AMA_ManifestType when sending NVC01 message.");
				}
			}

			if (Header.CreateWarningForTest)
			{
				Parent.AMA_ManifestTypeInfo.AddWarning("Test warning on AMA_ManifestType.");
			}
		}

		protected override void CheckAMA_CustomsOffice()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.AMA_CustomsOfficeInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_CustomsOfficeInfo);
		}

		protected override void CheckAMA_ManifestDescription()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.AMA_ManifestDescriptionInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_ManifestDescriptionInfo);
		}
	}
}
