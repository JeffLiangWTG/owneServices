using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Client.UPE.Business.Testing
{
	sealed class UPEShipmentHeldLetterAutoDeliveryTest : UPEDocumentAutoDeliveryTest
	{
		public void TestPrintBatchType()
		{
			AssertEquals("Allow queue for batch print if delivered to the consignee", UPEPrintBatchTypes.Codes.ShipmentHeldLetter, new TestUPEShipmentHeldLetterAutoDelivery(CusHAWB, ShipmentHeldLetterRecipient.Consignee).PrintBatchType);
			AssertEquals("Don't queue for batch print if delivered to the consignor", "", new TestUPEShipmentHeldLetterAutoDelivery(CusHAWB, ShipmentHeldLetterRecipient.Consignor).PrintBatchType);
		}

		protected override UPEDocumentAutoDelivery NewDocumentAutoDelivery()
		{
			return new UPEShipmentHeldLetterAutoDelivery(CusHAWB, ShipmentHeldLetterRecipient.Consignee);
		}

		protected override IUPEDocumentSupportable NewDocumentSupportable()
		{
			UPECusHAWB result = Factory.NewWithValidTestData<UPECusHAWB>();
			result.CS_HAWB = "HAWB";
			result.CS_OA_ConsigneeAddress = DeliveryOrganisation.MainAddress.PK;
			return result;
		}

		protected override DocumentCommand ExpectedDocumentCommand
		{
			get
			{
				return DocumentLoader.LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			}
		}

		protected override ZString ExpectedPrintBatchType
		{
			get
			{
				return UPEPrintBatchTypes.Codes.ShipmentHeldLetter;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailSubject
		{
			get
			{
				return "Delivery instructions incomplete for Consignee Customer Notification - HAWB";
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailBody
		{
			get
			{
				return @"
Delivery instructions incomplete for Consignee Customer Notification; Generated 11-Nov-05 00:00:00

HAWB : HAWB

Error: DeliveryAddress: Please enter a Fax Number.
";
			}
		}

		UPECusHAWB CusHAWB
		{
			get
			{
				return (UPECusHAWB)DocumentSupportable;
			}
		}

		#region Test Classes
		class TestUPEShipmentHeldLetterAutoDelivery : UPEShipmentHeldLetterAutoDelivery
		{
			public TestUPEShipmentHeldLetterAutoDelivery(UPECusHAWB cusHAWB, ShipmentHeldLetterRecipient recipient) : base(cusHAWB, recipient)
			{
			}

			public new ZString PrintBatchType
			{
				get
				{
					return base.PrintBatchType;
				}
			}
		}
		#endregion
	}
}
