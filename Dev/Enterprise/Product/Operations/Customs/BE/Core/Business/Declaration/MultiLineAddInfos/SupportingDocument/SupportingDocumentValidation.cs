using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
{
	public SupportingDocumentValidation(SupportingDocument parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateValidationOffice();
		ValidateArchiveSupport();
		ValidateArchiveLocationIndicator();
	}

	protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		var csiCode = Parent.CSI_Code;
		var targetInfo = Parent.CSI_ReferenceNumberInfo;

		if (!csiCode.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			var sourceValue = Parent.CSI_ReferenceNumber;
			var expectedAuthType = AuthorisationHelper.GetAuthorisationCode(csiCode);
			var authTypeEntered = sourceValue.SubstringSafe(2, 3);
			if (!expectedAuthType.IsEmpty && !authTypeEntered.IsEmpty)
			{
				if (expectedAuthType != authTypeEntered)
				{
					targetInfo.AddMessageError(Res.GetString("c455824a-9b83-481b-8e49-16012938c585", "The Authorization Type {0} of this document doesn't match the selected Document Type, {1} is expected.", authTypeEntered, expectedAuthType));
				}
				else if (Parent.Parent is JobComInvoiceLine invLine)
				{
					var warehouseAddress = AuthorisationHelper.GetAuthoristationTarget(invLine);
					if (warehouseAddress != null)
					{
						var availableAuthList = CusAuthorisationHelper.GetCachedAuthorizationNumbersForAddresses(new BusinessObjectFactory(), Core.Constants.CountryCodes.Belgium, new[] { expectedAuthType }, new[] { warehouseAddress.PK }, invLine.EffectiveAssessmentDate.Date);
						if (availableAuthList.Count == 0)
						{
							targetInfo.AddMessageError(Res.GetString("F754BDB9-FF59-43E6-B1D4-28828C6A6520", "No Authorization of type '{0}' found for the selected warehouse address.", expectedAuthType));
						}
						else if (!availableAuthList.ContainsCode(Parent.CSI_ReferenceNumber))
						{
							targetInfo.AddMessageError(Res.GetString("46CBBEC1-6060-4002-AF51-E62B19438318", "No Authorization of type '{0}' with number {1} found for the selected warehouse address. Please checked the entered Authorization Number is correct.", expectedAuthType, sourceValue));
						}
					}
				}
			}
		}
	}

	protected override void CheckCSI_QuantityIsValidZDecimal()
	{
		TypeValidation.CheckValidDecimal(Parent.CSI_QuantityInfo, SupportingDocument.Schema.QuantityDecimalPlaces, SupportingDocument.Schema.QuantityDecimalPrecision);
	}

	public void ValidateValidationOffice()
	{
		ValidateCalculatedProperty(Parent.ValidationOfficeInfo);
	}

	protected virtual void CheckValidationOffice()
	{
		EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.ValidationOfficeInfo);
	}

	public void ValidateArchiveSupport()
	{
		ValidateCalculatedProperty(Parent.ArchiveSupportInfo);
	}

	protected virtual void CheckArchiveSupport()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.ArchiveSupportInfo);
	}

	public void ValidateArchiveLocationIndicator()
	{
		ValidateCalculatedProperty(Parent.ArchiveLocationIndicatorInfo);
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_UnitOfQuantityInfo, Parent.CSI_QuantityInfo);

		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		MandatoryValidation.CheckEntered(Parent.CSI_CodeInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_QuantityInfo, Parent.CSI_UnitOfQuantityInfo);
	}

	protected override void CheckCSI_Value()
	{
		base.CheckCSI_Value();

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_ValueInfo, Parent.CSI_RX_NKCurrencyInfo);
	}

	protected override void CheckCSI_RX_NKCurrency()
	{
		base.CheckCSI_RX_NKCurrency();

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_RX_NKCurrencyInfo, Parent.CSI_ValueInfo);
	}

	protected virtual void CheckArchiveLocationIndicator()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.ArchiveLocationIndicatorInfo);
	}
}

