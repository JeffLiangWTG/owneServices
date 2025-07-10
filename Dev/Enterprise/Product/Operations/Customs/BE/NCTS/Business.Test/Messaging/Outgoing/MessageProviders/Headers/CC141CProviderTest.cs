using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC141CProvider))]
	sealed class CC141CProviderTest : NctsHeaderProviderAbstractTest<CC141CProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC141CProvider(null));
		}

		public void TestCustomsOfficeOfDestination() => CombineAssertions(() =>
		{
			var action = new MessageSendingAction(nctsHeader.MovementHeader);
			var provider = new CC141CProvider(action);

			action.ActualOfficeOfDestination = "BE101000";
			AssertNull("Query Information is empty", provider.CustomsOfficeOfDestination);
			action.QueryInformation = "queryinformation";
			AssertEquals("Query Information is not empty", "BE101000", provider.CustomsOfficeOfDestination);
		});

		public void TestCustomsOfficeOfEnquiryAtDeparture()
		{
			var movementHeader = nctsHeader.MovementHeader;
			var customsOfficeOfDestination = movementHeader.CustomsOffices.AddNew();
			customsOfficeOfDestination.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry;
			customsOfficeOfDestination.CY_Data = "EnqID";

			Factory.CreateJobDocAddress(AutoDocAddressTypes.Codes.ConsigneeAddress, parent: nctsHeader);

			AssertEquals("EnqID", Provider.CustomsOfficeOfEnquiryAtDeparture);
		}

		[TestDate]
		public void TestEnquiryTC11DeliveryDate()
		{
			CombineAssertions(() =>
			{
				var action = new MessageSendingAction(nctsHeader.MovementHeader);

				AssertEquals("No TCI Document", null, Provider.EnquiryTC11DeliveryDate);

				var provider1 = new CC141CProvider(action);
				action.TCI11 = ZDateTime.Empty;
				AssertEquals("TCI Document has empty date received", null, provider1.EnquiryTC11DeliveryDate);

				var provider2 = new CC141CProvider(action);
				action.TCI11 = new ZDateTime(DateTime.MinValue);
				AssertNull("TCI Document empty date received from MinValue", provider2.EnquiryTC11DeliveryDate);

				var newDateReceived = ZDateTime.Now.AddHours(1);
				var provider3 = new CC141CProvider(action);
				action.TCI11 = newDateReceived;
				AssertEquals("TCI Document has date received", newDateReceived.ToDateTime(), provider3.EnquiryTC11DeliveryDate);
			});
		}

		public void TestEnquiryText()
		{
			var action = new MessageSendingAction(nctsHeader.MovementHeader);

			CombineAssertions(() =>
			{
				AssertNull("No TCI Document", Provider.EnquiryText);

				var provider1 = new CC141CProvider(action);
				action.QueryInformation = ZString.Empty;
				AssertNull("TCI Document has empty notes", provider1.EnquiryText);

				var enquiryText = "123";
				action.QueryInformation = enquiryText;
				AssertEquals("TCI Document has notes", enquiryText, provider1.EnquiryText);
			});
		}

		public void TestHolderOfTheTransitProcedure()
		{
			AssertNotNull(Provider.HolderOfTheTransitProcedure);
		}

		public void TestConsignmentConsignee()
		{
			CombineAssertions(() =>
			{
				var action = new MessageSendingAction(nctsHeader.MovementHeader);
				var provider = new CC141CProvider(action);
				action.ActualConsignee.E2_CompanyName = "test";
				AssertNull("Query Information empty", provider.ConsignmentConsignee);

				action.QueryInformation = "123";
				provider = new CC141CProvider(action);
				AssertType<CC141CConsigneeProvider>(provider.ConsignmentConsignee);
				AssertNotNull("Query Information filled", provider.ConsignmentConsignee);
			});
		}

		protected override string MessageType => Constants.MessageTypes.CC141C;

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool HasSendingActionParameter => true;
	}
}
