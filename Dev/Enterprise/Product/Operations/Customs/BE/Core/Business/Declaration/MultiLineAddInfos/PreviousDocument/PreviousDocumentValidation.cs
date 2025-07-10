using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
{
	public PreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_ItemNumber()
	{
		base.CheckCSI_ItemNumber();
		CompareValidation.CheckLessThanOrEqualTo(Parent.CSI_ItemNumberInfo, 9999);
		MandatoryValidation.MessageErrorIfIsNegative(Parent.CSI_ItemNumberInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		if (!Parent.CSI_Code.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}

		if (!Parent.CSI_ReferenceNumber.KeepAlphanumericCharacters().Equals(Parent.CSI_ReferenceNumber))
		{
			var error = Res.GetString("6DC6FAAF-F4C0-4E24-8D5A-83C29E9325D9", "{0} should only consist alphabetical and numeric characters.", Parent.CSI_ReferenceNumberInfo.Description);
			Parent.CSI_ReferenceNumberInfo.AddMessageError(error);
		}
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		if (!Parent.CSI_ReferenceNumber2.KeepAlphanumericCharacters().Equals(Parent.CSI_ReferenceNumber2))
		{
			var error = Res.GetString("6DC6FAAF-F4C0-4E24-8D5A-83C29E9325D9", "{0} should only consist alphabetical and numeric characters.", Parent.CSI_ReferenceNumber2Info.Description);
			Parent.CSI_ReferenceNumber2Info.AddMessageError(error);
		}
	}

	protected override void CheckCSI_CustomsOffice()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CustomsOfficeInfo, (Parent as PreviousDocument).Lookups.CustomsOffices);
	}

	protected override void CheckCSI_QuantityIsValidZDecimal()
	{
		TypeValidation.CheckValidDecimal(Parent.CSI_QuantityInfo, PreviousDocument.Schema.CSI_QuantityDecimalPlaces, PreviousDocument.Schema.CSI_QuantityDecimalPrecision);
	}

	protected override void CheckCSI_Quantity2IsValidZDecimal()
	{
		TypeValidation.CheckValidDecimal(Parent.CSI_Quantity2Info, PreviousDocument.Schema.CSI_Quantity2DecimalPlaces, PreviousDocument.Schema.CSI_Quantity2DecimalPrecision);
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();

		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
	}

	protected override void CheckCSI_UnitOfQuantity2()
	{
		base.CheckCSI_UnitOfQuantity2();

		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantity2Info);
	}

	protected override void CheckCSI_PackType()
	{
		base.CheckCSI_PackType();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_PackTypeInfo);
	}

	protected override bool IsSubTypeMandatory => false;
}
