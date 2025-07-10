using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocumentCollection<NctsSupportingDocument>))]
	sealed class NctsSupportingDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCountValidation_TR0029_Header()
		{
			AssertMaxCountValidationForParent("NctsHeader");
		}

		public void TestMaxCountValidation_TR0029_Bill()
		{
			AssertMaxCountValidationForParent("NctsBill");
		}

		public void TestMaxCountValidation_TR0029_CargoDesc()
		{
			AssertMaxCountValidationForParent("NctsCargoDesc");
		}

		public void TestMaxCountValidation_ArrivalMovementHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var collection = (ISupportMaxCountValidation)header.ArrivalMovementHeader.SupportingDocuments;
			CombineAssertions(() =>
			{
				AssertEquals("Max Count", 99, collection.MaxCountValidator.MaxCount);
				AssertNull("Message override not set for Arrival movement", collection.MaxCountValidator.Notification);
			});
		}

		public void TestMaxCountValidation_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItems = header.MovementHeader.GoodsItems.AddNew();
			var collection = (ISupportMaxCountValidation)goodsItems.SupportingDocuments;
			CombineAssertions(() =>
			{
				AssertEquals("Max Count", 99, collection.MaxCountValidator.MaxCount);
				AssertNull("Message override not set for Phase4", collection.MaxCountValidator.Notification);
			});
		}

		public void TestAddNewWithCodeAndReferenceNumber()
		{
			var nctsSupportingDocumentCollection = (NctsSupportingDocumentCollection<NctsSupportingDocument>)GetCollectionToTest();
			AssertEquals("PRE-CONDITION", 0, nctsSupportingDocumentCollection.Count);

			nctsSupportingDocumentCollection.AddNew("XYZ", "1234");
			AssertEquals("POST-CONDITION", 1, nctsSupportingDocumentCollection.Count);
			CombineAssertions("CSI_Code and CSI_Description", () =>
			{
				var singleSupportingDocument = nctsSupportingDocumentCollection[0];
				AssertEquals(nameof(singleSupportingDocument.CSI_Code), "XYZ", singleSupportingDocument.CSI_Code);
				AssertEquals(nameof(singleSupportingDocument.CSI_ReferenceNumber), "1234", singleSupportingDocument.CSI_ReferenceNumber);
			});
		}

		public void TestISequenceNumberHeader()
		{
			var collection = GetCollectionToTest();
			AssertSame(collection, ((ISequenceNumberHeader)collection).Lines);
		}

		public void TestShortSequenceNumberGenerator()
		{
			var collection = (NctsSupportingDocumentCollection<NctsSupportingDocument>)GetCollectionToTest();
			AssertType<ShortSequenceNumberGenerator>("Collection should have a ShortSequenceNumberGenerator", collection.SequenceGenerator);
		}

		public void TestSetDefaultsForNewChild_SetCSI_Status_NotArrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var document = header.MovementHeader.SupportingDocuments.AddNew();
			AssertEquals(string.Empty, document.CSI_Status);
		}

		public void TestSetDefaultsForNewChild_SetCSI_Status_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			var document = bill.SupportingDocuments.AddNew();
			AssertEquals(SupportingDocumentStatusList.Codes.NEW, document.CSI_Status);
		}

		public void TestRemoveAndDeleteAllDoesNotRenumber()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			var collection = header.ArrivalMovementHeader.SupportingDocuments;
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
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			return new NctsSupportingDocumentCollection<NctsSupportingDocument>(goodsItem);
		}

		void AssertMaxCountValidationForParent(string parent)
		{
			var expectedErrorMessage = "[TR0029] The maximum number of 99 Supporting Documents has been exceeded.";
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0029Active));

					var supportingDocuments = GetSupportingDocumentCollectionForDeparture(parent);
					var maxCountValidator = ((ISupportMaxCountValidation)supportingDocuments).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount for Departure movement", 99, maxCountValidator.MaxCount);
					AssertEquals("Notification Type for Departure movement", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", expectedErrorMessage, notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0029Active));

					supportingDocuments = GetSupportingDocumentCollectionForDeparture(parent);
					maxCountValidator = ((ISupportMaxCountValidation)supportingDocuments).MaxCountValidator;

					AssertEquals("MaxCount when RuleTR0029 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		INctsSupportingDocumentCollection<NctsSupportingDocument> GetSupportingDocumentCollectionForDeparture(string parent)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			switch (parent)
			{
				case "NctsCargoDesc":
					return header.Bills.AddNew().GoodsItems.AddNew().SupportingDocuments;
				case "NctsBill":
					return header.Bills.AddNew().SupportingDocuments;
				case "NctsHeader":
				default:
					return header.MovementHeader.SupportingDocuments;
			}
		}
	}
}
