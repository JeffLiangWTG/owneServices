using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStoragePreviousDocumentValidation : EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentValidation
{
	public TemporaryStoragePreviousDocumentValidation(TemporaryStoragePreviousDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		var parent = Parent;

		MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);

		if (PreviousDocuments?.Any(x => x.CSI_ReferenceNumber != parent.CSI_ReferenceNumber) ?? false)
		{
			parent.CSI_ReferenceNumberInfo.AddWarning(ValidationCaptions.TemporaryStoragePreviousDocument.ShouldHaveSameReferenceNumberMessage);
		}
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		var parent = Parent;
		if (PreviousDocuments?.Any(x => x.CSI_Code != parent.CSI_Code) ?? false)
		{
			parent.CSI_CodeInfo.AddWarning(ValidationCaptions.TemporaryStoragePreviousDocument.ShouldHaveSameType);
		}
	}

	protected override void CheckCSI_LineNo()
	{
		base.CheckCSI_LineNo();

		var parent = Parent;
		if (PreviousDocuments != null && PreviousDocuments.Any(x => x.CSI_LineNo == parent.CSI_LineNo && x.PK != parent.PK))
		{
			parent.CSI_LineNoInfo.AddWarning(ValidationCaptions.TemporaryStoragePreviousDocument.ShouldHaveDiffLineNumber);
		}
	}

	protected override void CheckCSI_PackType()
	{
		base.CheckCSI_PackType();

		var parent = Parent;

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.CSI_PackTypeInfo, parent.CSI_PackQtyInfo);
		ListValidation.MessageErrorIfInvalidCode(parent.CSI_PackTypeInfo);
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();

		var parent = Parent;
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(parent.CSI_UnitOfQuantityInfo, parent.CSI_QuantityInfo);
		ListValidation.MessageErrorIfInvalidCode(parent.CSI_UnitOfQuantityInfo);
	}

	protected override void CheckRuleBR_PN_TS_053(EU.Business.CusTempStorage.TemporaryStoragePreviousDocument previousDocument) { }

	new TemporaryStoragePreviousDocument Parent => (TemporaryStoragePreviousDocument)base.Parent;

	EU.Business.CusTempStorage.ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments => Parent.Parent switch
	{
		TemporaryStorageBill bill => bill.PreviousDocuments,
		TemporaryStoragePackedItem packedItem => packedItem.PreviousDocuments,
		_ => null,
	};
}
