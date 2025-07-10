using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5WNLineDutyTaxDetailMessageDataCollection))]
	sealed class GOVCBR5WNLineDutyTaxDetailMessageDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GOVCBR5WNLineDutyTaxDetailMessageDataCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new GOVCBR5WNLineDutyTaxDetailMessageData(Factory);

		protected override GOVCBR5WNLineDutyTaxDetailMessageDataCollection GetCollectionToTest() => new GOVCBR5WNLineDutyTaxDetailMessageDataCollection(Factory);

		public void TestStringIndexer()
		{
			var messageData5TV = new GOVCBR5WNMessageData(Factory);
			var lineMessageData = messageData5TV.Lines.AddNew();

			var detailMessageDataCUD1 = lineMessageData.DutyTaxDetails.AddNew();
			detailMessageDataCUD1.DutyTaxType = "CUD";
			detailMessageDataCUD1.BaseValue = 10m;
			detailMessageDataCUD1.DutyTaxRate = 20m;
			detailMessageDataCUD1.ReducedOrExemptAmount = 40m;
			detailMessageDataCUD1.DutyTaxAmount = 50m;
			detailMessageDataCUD1.BeforeOrAfterAmendmentIndicator = BeforeOrAfterAmendment.Before;

			var detailMessageDataCUD2 = lineMessageData.DutyTaxDetails.AddNew();
			detailMessageDataCUD2.DutyTaxType = "CUD";
			detailMessageDataCUD2.BaseValue = 11m;
			detailMessageDataCUD2.DutyTaxRate = 21m;
			detailMessageDataCUD2.ReducedOrExemptAmount = 41m;
			detailMessageDataCUD2.DutyTaxAmount = 51m;
			detailMessageDataCUD2.BeforeOrAfterAmendmentIndicator = BeforeOrAfterAmendment.After;

			var detailMessageDataVAT1 = lineMessageData.DutyTaxDetails.AddNew();
			detailMessageDataVAT1.DutyTaxType = "VAT";
			detailMessageDataVAT1.BaseValue = 100m;
			detailMessageDataVAT1.DutyTaxRate = 200m;
			detailMessageDataVAT1.ReducedOrExemptAmount = 300m;
			detailMessageDataVAT1.DutyTaxAmount = 400m;
			detailMessageDataVAT1.BeforeOrAfterAmendmentIndicator = BeforeOrAfterAmendment.Before;

			var detailMessageDataVAT2 = lineMessageData.DutyTaxDetails.AddNew();
			detailMessageDataVAT2.DutyTaxType = "VAT";
			detailMessageDataVAT2.BaseValue = 101m;
			detailMessageDataVAT2.DutyTaxRate = 201m;
			detailMessageDataVAT2.ReducedOrExemptAmount = 301m;
			detailMessageDataVAT2.DutyTaxAmount = 401m;
			detailMessageDataVAT2.BeforeOrAfterAmendmentIndicator = BeforeOrAfterAmendment.After;

			AssertEquals("CUD", lineMessageData.DutyTaxDetails["\"CUD, 1\""].DutyTaxType);
			AssertEquals(10m, lineMessageData.DutyTaxDetails["\"CUD, 1\""].BaseValue);
			AssertEquals(20m, lineMessageData.DutyTaxDetails["\"CUD, 1\""].DutyTaxRate);
			AssertEquals(40m, lineMessageData.DutyTaxDetails["\"CUD, 1\""].ReducedOrExemptAmount);
			AssertEquals(50m, lineMessageData.DutyTaxDetails["\"CUD, 1\""].DutyTaxAmount);
			AssertEquals(BeforeOrAfterAmendment.Before, lineMessageData.DutyTaxDetails["\"CUD, 1\""].BeforeOrAfterAmendmentIndicator);

			AssertEquals("CUD", lineMessageData.DutyTaxDetails["\"CUD, 2\""].DutyTaxType);
			AssertEquals(11m, lineMessageData.DutyTaxDetails["\"CUD, 2\""].BaseValue);
			AssertEquals(21m, lineMessageData.DutyTaxDetails["\"CUD, 2\""].DutyTaxRate);
			AssertEquals(41m, lineMessageData.DutyTaxDetails["\"CUD, 2\""].ReducedOrExemptAmount);
			AssertEquals(51m, lineMessageData.DutyTaxDetails["\"CUD, 2\""].DutyTaxAmount);
			AssertEquals(BeforeOrAfterAmendment.After, lineMessageData.DutyTaxDetails["\"CUD, 2\""].BeforeOrAfterAmendmentIndicator);

			AssertEquals("VAT", lineMessageData.DutyTaxDetails["\"VAT, 1\""].DutyTaxType);
			AssertEquals(100m, lineMessageData.DutyTaxDetails["\"VAT, 1\""].BaseValue);
			AssertEquals(200m, lineMessageData.DutyTaxDetails["\"VAT, 1\""].DutyTaxRate);
			AssertEquals(300m, lineMessageData.DutyTaxDetails["\"VAT, 1\""].ReducedOrExemptAmount);
			AssertEquals(400m, lineMessageData.DutyTaxDetails["\"VAT, 1\""].DutyTaxAmount);
			AssertEquals(BeforeOrAfterAmendment.Before, lineMessageData.DutyTaxDetails["\"VAT, 1\""].BeforeOrAfterAmendmentIndicator);

			AssertEquals("VAT", lineMessageData.DutyTaxDetails["\"VAT, 2\""].DutyTaxType);
			AssertEquals(101m, lineMessageData.DutyTaxDetails["\"VAT, 2\""].BaseValue);
			AssertEquals(201m, lineMessageData.DutyTaxDetails["\"VAT, 2\""].DutyTaxRate);
			AssertEquals(301m, lineMessageData.DutyTaxDetails["\"VAT, 2\""].ReducedOrExemptAmount);
			AssertEquals(401m, lineMessageData.DutyTaxDetails["\"VAT, 2\""].DutyTaxAmount);
			AssertEquals(BeforeOrAfterAmendment.After, lineMessageData.DutyTaxDetails["\"VAT, 2\""].BeforeOrAfterAmendmentIndicator);

			AssertNull(lineMessageData.DutyTaxDetails["\"IND, 1\""]);
		}
	}
}
