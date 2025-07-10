using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_QuantityInteger()
		{
			var errorMessage = string.Format("Only integer values are allowed for this {0} Unit", previousDocument.CSI_QuantityInfo.HasHumanReadableName ? previousDocument.CSI_QuantityInfo.HumanReadableName.ToString() : "Qty.");
			CombineAssertions(() =>
			{
				previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
				previousDocument.CSI_Quantity = 10.1;
				AssertNoMessageError("Decimal qty with non-number unit", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageError("Decimal qty with number unit 'NAR'", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells;
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageError("Decimal qty with number unit 'NCL'", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs;
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageError("Decimal qty with number unit 'NPR'", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_Quantity = 10;
				AssertNoMessageError("Int qty with number unit", previousDocument.CSI_QuantityInfo, errorMessage);
			});
		}

		public void TestCheckCSI_QuantityDecimalRange()
		{
			const string errorMessage = "is too large, the maximum value allowed for";
			CombineAssertions(() =>
			{
				previousDocument.CSI_Quantity = -999999999.999;
				AssertNoErrorContaining("Minimum", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_Quantity = -1000000000;
				AssertHasErrorContaining("Less than Minimum", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_Quantity = 999999999.999;
				AssertNoErrorContaining("Maximum", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_Quantity = 1000000000;
				AssertHasErrorContaining("Greater than maximum", previousDocument.CSI_QuantityInfo, errorMessage);
			});
		}

		public void TestCheckCSI_Quantity2Integer()
		{
			var errorMessage = string.Format("Only integer values are allowed for this {0} Unit", previousDocument.CSI_Quantity2Info.HasHumanReadableName ? previousDocument.CSI_Quantity2Info.HumanReadableName.ToString() : "Qty.");
			CombineAssertions(() =>
			{
				previousDocument.CSI_UnitOfQuantity2 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
				previousDocument.CSI_Quantity2 = 10.1;
				AssertNoMessageError("Decimal qty with non-number unit", previousDocument.CSI_Quantity2Info, errorMessage);

				previousDocument.CSI_UnitOfQuantity2 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
				previousDocument.Validation.ValidateCSI_Quantity2();
				AssertHasMessageError("Decimal qty with number unit 'NAR'", previousDocument.CSI_Quantity2Info, errorMessage);

				previousDocument.CSI_UnitOfQuantity2 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells;
				previousDocument.Validation.ValidateCSI_Quantity2();
				AssertHasMessageError("Decimal qty with number unit 'NCL'", previousDocument.CSI_Quantity2Info, errorMessage);

				previousDocument.CSI_UnitOfQuantity2 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs;
				previousDocument.Validation.ValidateCSI_Quantity2();
				AssertHasMessageError("Decimal qty with number unit 'NPR'", previousDocument.CSI_Quantity2Info, errorMessage);

				previousDocument.CSI_Quantity2 = 10;
				AssertNoMessageError("Int qty with number unit", previousDocument.CSI_Quantity2Info, errorMessage);
			});
		}

		public void TestCheckCSI_Quantity2DecimalRange()
		{
			const string errorMessage = "is too large, the maximum value allowed for";
			CombineAssertions(() =>
			{
				previousDocument.CSI_Quantity = -999999999.999;
				AssertNoErrorContaining("Minimum", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_Quantity = -1000000000;
				AssertHasErrorContaining("Less than Minimum", previousDocument.CSI_QuantityInfo, errorMessage);

				previousDocument.CSI_Quantity2 = 999999999.999;
				AssertNoErrorContaining("Maximum", previousDocument.CSI_Quantity2Info, errorMessage);

				previousDocument.CSI_Quantity2 = 1000000000;
				AssertHasErrorContaining("Greater than maximum", previousDocument.CSI_Quantity2Info, errorMessage);
			});
		}

		public void TestIsSubTypeMandatoryValue()
		{
			var previousDocumentValidationForTest = new NctsPreviousDocumentValidationForTest(previousDocument);

			AssertEquals("IsSubTypeMandatory should be false", expected: false, previousDocumentValidationForTest.IsSubTypeMandatoryFlag);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}
		NctsPreviousDocument previousDocument;

		public class NctsPreviousDocumentValidationForTest : NctsPreviousDocumentValidation
		{
			public NctsPreviousDocumentValidationForTest(NctsPreviousDocument parent) : base(parent)
			{
			}

			public bool IsSubTypeMandatoryFlag => IsSubTypeMandatory;
		}
	}
}
