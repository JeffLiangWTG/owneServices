using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class IM432HeaderProviderTest : DataProviderTestCase<IM432HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Message sending object missing", () => new IM432HeaderProvider(null));
		}

		public void TestImportOperation()
		{
			AssertSame("IM432MessageProvider is IIM432Operation", Provider, Provider.ImportOperation);
		}

		public void TestLRN()
		{
			AssertEquals("LRN", "TestLRN", Provider.LRN);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "TestMRN", Provider.MRN);
		}

		public void TestFallbackProcedure()
		{
			AssertNull("Fallback procedure", Provider.FallbackProcedure);
		}

		public void TestCustomsOfficeOfPresentation()
		{
			AssertEquals("Customs office presentation", "NJOffice", Provider.CustomsOfficeOfPresentation);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("Customs office lodgement", "TestOffice", Provider.CustomsOfficeLodgement);
		}

		public void TestDeclarant()
		{
			var declarant = Provider.Declarant;
			CombineAssertions("Declarant", () =>
			{
				Assert("Declarant is MDeclarantProvider", declarant is MDeclarantProvider);
				AssertSame("Is cached", declarant, Provider.Declarant);
			});
		}

		public void TestRepresentative()
		{
			var representative = Provider.Representative;
			CombineAssertions("Representative", () =>
			{
				Assert("Representative is MRepresentativeProvider", representative is MRepresentativeProvider);
				AssertSame("Is cached", representative, Provider.Representative);
			});
		}

		public void TestGoodsShipment()
		{
			var goodsShipment = Provider.GoodsShipment;
			CombineAssertions("Goods shipment", () =>
			{
				Assert("Goods shipment is IM432GoodsShipmentProvider", goodsShipment is IM432GoodsShipmentProvider);
				AssertSame("Is cached", goodsShipment, Provider.GoodsShipment);
			});
		}

		[TestDate(2023, 12, 28, 15, 55, 01)]
		public void TestPreparationDateAndTime()
		{
			AssertEquals("Preparation date and time", new DateTime(2023, 12, 28, 15, 55, 01), Provider.PreparationDateAndTime);
		}

		protected override IM432HeaderProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "TestOffice";
			header.PresentationOffice = "NJOffice";

			var declarantHeader = Factory.New<OrgHeader>();
			header.AMA_OA_Declarant = declarantHeader.MainAddress.PK;

			var representativeHeader = Factory.New<OrgHeader>();
			header.AMA_OA_Representative = representativeHeader.MainAddress.PK;

			var bill = header.Bills.AddNew();

			bill.LocalReferenceNumber = "TestLRN";
			bill.MovementReferenceNumber = "TestMRN";

			var sendingObject = new MessageSendingObject(bill);
			return new IM432HeaderProvider(sendingObject);
		}
	}
}

