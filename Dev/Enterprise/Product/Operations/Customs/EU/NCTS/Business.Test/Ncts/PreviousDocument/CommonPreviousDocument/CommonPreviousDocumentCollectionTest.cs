using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CommonPreviousDocumentCollection<CommonPreviousDocument>))]
	class CommonPreviousDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestShortSequenceNumberGenerator()
		{
			var collection = (CommonPreviousDocumentCollection<CommonPreviousDocument>)GetCollectionToTest();
			AssertType<ShortSequenceNumberGenerator>("Collection should have a ShortSequenceNumberGenerator", collection.SequenceGenerator);
		}

		public void TestMaxCountValidation_TR0028()
		{
			AssertMaxCountValidationForParent("NctsHeader", expectedMaxCountWhenEnable: 9999, "TR0028", nameof(ValidationRuleConfiguration.IsRuleTR0028Active), expectedMaxCountWhenDisable: -1);
		}

		public void TestMaxCountValidation_TR0030()
		{
			AssertMaxCountValidationForParent("NctsBill", expectedMaxCountWhenEnable: 99, "TR0030", nameof(ValidationRuleConfiguration.IsRuleTR0030Active), expectedMaxCountWhenDisable: -1);
		}

		public void TestMaxCountValidation_G0026_1() => CombineAssertions(() =>
		{
			const int maxCountForBillWhenTR0030 = 99;
			const int maxCountForBillWhenG0026_1 = 1;
			var parent = "NctsBill";
			var expectedErrorMessage = $"[G0026-1] The maximum number of {maxCountForBillWhenG0026_1} Previous Documents has been exceeded.";

			using var ruleTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			ruleTestContext.EnableRule(x => x.IsRuleG0026_1Active);
			var previousDocuments = GetPreviousDocumentCollectionForParent(parent);
			var maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;
			var notification = maxCountValidator.Notification;
			AssertEquals("MaxCount for Departure movement", maxCountForBillWhenG0026_1, maxCountValidator.MaxCount);
			AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
			AssertEquals("Notification Message", expectedErrorMessage, notification.Message);
			AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

			ruleTestContext.DisableRule(x => x.IsRuleG0026_1Active);
			previousDocuments = GetPreviousDocumentCollectionForParent(parent);
			maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;
			AssertEquals($"MaxCount when G0026-1 is disabled", maxCountValidator.MaxCount, maxCountForBillWhenTR0030);
		});

		void AssertMaxCountValidationForParent(string parent, int expectedMaxCountWhenEnable, string ruleNumber, string publicPropertyName, int expectedMaxCountWhenDisable)
		{
			var expectedErrorMessage = $"[{ruleNumber}] The maximum number of {expectedMaxCountWhenEnable} Previous Documents has been exceeded.";
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(publicPropertyName);

					var previousDocuments = GetPreviousDocumentCollectionForParent(parent);
					var maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount for Departure movement", expectedMaxCountWhenEnable, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", expectedErrorMessage, notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					ruleTestContext.DisableRule(publicPropertyName);

					previousDocuments = GetPreviousDocumentCollectionForParent(parent);
					maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;

					AssertEquals($"MaxCount when {ruleNumber} is disabled", maxCountValidator.MaxCount, expectedMaxCountWhenDisable);
				}
			});
		}

		public void TestRemoveAndDeleteAllDoesNotRenumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();

			var collection = bill.PreviousDocuments;
			collection.AddNew();
			var second = collection.AddNew();
			collection.AddNew();

			var lineNoChanged = false;
			second.CSI_LineNoInfo.ValueChanged += (sender, args) => { lineNoChanged = true; };

			collection.RemoveAndDeleteAll();

			AssertEquals("Line number not changed , so no renumbering", false, lineNoChanged);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			return new CommonPreviousDocumentCollection<CommonPreviousDocument>(bill);
		}

		ICommonPreviousDocumentCollection<CommonPreviousDocument> GetPreviousDocumentCollectionForParent(string parent)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			switch (parent)
			{
				case "NctsBill":
					return header.Bills.AddNew().PreviousDocuments;
				case "NctsHeader":
				default:
					return header.PreviousDocuments;
			}
		}
	}
}
