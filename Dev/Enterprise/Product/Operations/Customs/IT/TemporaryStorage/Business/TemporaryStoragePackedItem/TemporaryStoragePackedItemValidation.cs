using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStoragePackedItemValidation : EU.Business.CusTempStorage.TemporaryStoragePackedItemValidation
{
	public TemporaryStoragePackedItemValidation(TemporaryStoragePackedItem parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckPreviousDocumentIsEmpty();
		CheckSupportingDocumentIsEmpty();
	}

	protected override void CheckAPI_Tariff()
	{
		base.CheckAPI_Tariff();
		var parent = Parent;

		if (parent.Bill.ABL_GoodsDescription.IsEmpty && parent.API_FormattedTariff.IsEmpty)
		{
			parent.API_FormattedTariffInfo.AddMessageError(ValidationCaptions.TemporaryStoragePackedItem.TariffCodeOrGoodsDescriptionRequired);
		}
	}

	protected override void CheckAPI_GrossWeightUQ()
	{
		base.CheckAPI_GrossWeightUQ();
		ListValidation.MessageErrorIfInvalidCode(Parent.API_GrossWeightUQInfo);
	}

	protected override void CheckAPI_CustomsUQ2()
	{
		base.CheckAPI_CustomsUQ2();
		ListValidation.MessageErrorIfInvalidCode(Parent.API_CustomsUQ2Info);
	}

	protected override void CheckAPI_CustomsQty2()
	{
		base.CheckAPI_CustomsQty2();
		const decimal maxAllowedValue = 9999999999.999999m;
		var warningMessage = Res.GetString("1ef47676-ec6d-4b58-ba19-2a2fda0f9804", "The number {0} is too large, the maximum value allowed for Supplementary quantity is 9,999,999,999.999999", Parent.API_CustomsQty2);

		if (Parent.API_CustomsQty2 > maxAllowedValue)
		{
			Parent.API_CustomsQty2Info.AddWarning(warningMessage);
		}
	}

	protected override void CheckAPI_ChemicalSubstanceCode()
	{
		base.CheckAPI_ChemicalSubstanceCode();
		ListValidation.MessageErrorIfInvalidCode(Parent.API_ChemicalSubstanceCodeInfo);
	}

	void CheckSupportingDocumentIsEmpty()
	{
		var parent = Parent;

		parent.ClearRowNotificationsContaining(ValidationCaptions.TemporaryStoragePackedItem.HaveToEnterAtleastOneRow);
		if (parent.SupportingDocuments.Count == 0)
		{
			parent.AddRowMessageError(ValidationCaptions.TemporaryStoragePackedItem.HaveToEnterAtleastOneRow);
		}
	}

	void CheckPreviousDocumentIsEmpty()
	{
		var parent = Parent;

		parent.ClearRowNotificationsContaining(ValidationCaptions.TemporaryStoragePackedItem.PreviousDocumentsRequired);
		if (parent.PreviousDocuments.Count == 0)
		{
			parent.AddRowMessageError(ValidationCaptions.TemporaryStoragePackedItem.PreviousDocumentsRequired);
		}
	}
}
