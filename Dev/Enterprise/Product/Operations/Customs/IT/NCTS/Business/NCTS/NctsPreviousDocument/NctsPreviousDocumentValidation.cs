using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsPreviousDocumentValidation : EU.NCTS.Business.NctsPreviousDocumentPhase4Validation
{
	public NctsPreviousDocumentValidation(NctsPreviousDocument parent)
		: base(parent)
	{
	}

	protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

	protected override void CheckCSI_Procedure()
	{
		base.CheckCSI_Procedure();
		PreviousDocumentValidationHelper.CheckCSI_Procedure(Parent);
		CheckMoreThanOnePaAndOneOrMoreRpDocuments();
	}

	protected override void CheckCSI_Code()
	{
		PreviousDocumentValidationHelper.CheckCSI_Code(Parent);
	}

	protected override void CheckCSI_SubType()
	{
		PreviousDocumentValidationHelper.CheckCSI_SubType(Parent);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		IPreviousDocumentReferenceNumberValidator previousDocumentReferenceNumberValidator = new PreviousDocumentReferenceNumberValidator(Parent, GetSettings(Parent));
		previousDocumentReferenceNumberValidator.CheckReferenceNumber();
	}

	PreviousDocumentFieldsInfo GetSettings(NctsPreviousDocument parent)
	{
		var combinationsProvider = new PreviousDocumentCombinationsProvider(parent);
		var settingsProvider = new PreviousDocumentSettingsProvider(combinationsProvider);
		return settingsProvider.GetSettings();
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();
		PreviousDocumentValidationHelper.CheckCSI_ReferenceNumber2(Parent);
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		PreviousDocumentValidationHelper.CheckCSI_DateOfIssue(Parent);

		if (!Parent.CSI_DateOfIssue.IsEmpty && Parent.CSI_DateOfIssue.IsInTheFuture())
		{
			Parent.CSI_DateOfIssueInfo.AddMessageError(ValidationCaptions.NctsPreviousDocument.InvalidDateOfIssue);
		}
	}

	protected override void CheckCSI_LineNo()
	{
		base.CheckCSI_LineNo();

		if (IsPreviousProcedureDocumentWithoutLineNoAndTariff)
		{
			Parent.CSI_LineNoInfo.AddWarning(ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
		}
		else if (Parent.CSI_Procedure != PreviousDocumentProcedureList.Codes.DichiarazioneMeccanizzataDiTransito)
		{
			PreviousDocumentValidationHelper.CheckCSI_LineNo(Parent);
		}
	}

	protected override void CheckCSI_Tariff()
	{
		base.CheckCSI_Tariff();

		if (IsPreviousProcedureDocumentWithoutLineNoAndTariff)
		{
			Parent.CSI_TariffInfo.AddWarning(ValidationCaptions.NctsPreviousDocument.RpWithLineNoAndTariffEmpty);
		}
	}

	protected override void CheckCSI_Status()
	{
		base.CheckCSI_Status();
		PreviousDocumentValidationHelper.CheckCSI_Status(Parent);
	}

	protected override void CheckCSI_CustomsOffice()
	{
		base.CheckCSI_CustomsOffice();
		PreviousDocumentValidationHelper.CheckCSI_CustomsOffice(Parent);
	}

	bool IsPreviousProcedureDocumentWithoutLineNoAndTariff => Parent.IsPreviousProcedureDocument && Parent.CSI_LineNo == ZShort.Zero && Parent.CSI_Tariff.IsEmpty;

	protected override void CheckCSI_Quantity3()
	{
		base.CheckCSI_Quantity3();
		ValidateQuantityIfSummaryDeclarationDocument(Parent.CSI_Quantity3Info, previousDocument => previousDocument.CSI_Quantity3, goodsItem => goodsItem.BY_GrossWeight);
	}

	protected override void CheckCSI_PackQty()
	{
		base.CheckCSI_PackQty();
		ValidateQuantityIfSummaryDeclarationDocument(Parent.CSI_PackQtyInfo, previousDocument => previousDocument.CSI_PackQty, goodsItem => goodsItem.Packages.Cast<NctsPackage>().Sum(x => x.B5_UnitCount));
	}

	void ValidateQuantityIfSummaryDeclarationDocument(ZPropertyInfo propertyInfo, Func<NctsPreviousDocument, decimal> actualValueComparisonDelegate, Func<NctsDepartureCargoDesc, decimal> expectedValueComparisonDelegate)
	{
		if (Parent.Parent is NctsDepartureCargoDesc goodsItem && Parent.IsSummaryDeclarationDocument)
		{
			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			CheckPreviousDocumentsQuantitiesAgainsGoodsItemQuantity(propertyInfo, actualValueComparisonDelegate, expectedValueComparisonDelegate, goodsItem);
		}
	}

	void CheckPreviousDocumentsQuantitiesAgainsGoodsItemQuantity(ZPropertyInfo propertyInfo, Func<NctsPreviousDocument, decimal> actualValueComparisonDelegate, Func<NctsDepartureCargoDesc, decimal> expectedValueComparisonDelegate, NctsDepartureCargoDesc goodsItem)
	{
		const string emptyPreviousPrecedure = "00";
		if (goodsItem.BY_Procedure.Right(2) == emptyPreviousPrecedure && goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>().Sum(previousDocument => actualValueComparisonDelegate(previousDocument)) != expectedValueComparisonDelegate(goodsItem))
		{
			propertyInfo.AddMessageError(ValidationCaptions.NctsPreviousDocument.PreviousDocQtyDoesNotMatchGoodsItemQty);
		}
	}

	void CheckMoreThanOnePaAndOneOrMoreRpDocuments()
	{
		if (Parent.Parent is NctsDepartureCargoDesc goodsItem)
		{
			var nctsPreviousDocuments = goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>();
			var moreThanOnePaDoc = nctsPreviousDocuments.Where(x => x.IsSummaryDeclarationDocument).Skip(1).Any();
			var anyRpDoc = nctsPreviousDocuments.Any(x => x.IsPreviousProcedureDocument);

			if (moreThanOnePaDoc && anyRpDoc)
			{
				Parent.CSI_ProcedureInfo.AddWarning(ValidationCaptions.NctsPreviousDocument.MoreThanOnePaAndOneOrMoreRpDocuments);
			}
		}
	}

	protected override void CheckCSI_Quantity2()
	{
		base.CheckCSI_Quantity2();
		Parent.SupplementaryQuantityHandler.ValidateQuantity2();
	}
}
