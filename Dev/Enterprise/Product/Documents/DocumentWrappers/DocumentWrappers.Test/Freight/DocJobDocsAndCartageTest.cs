using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobDocsAndCartage))]
	sealed class DocJobDocsAndCartageTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocJobDocsAndCartage.New(GenDocsAndCartage, Factory)
			};
		}

		#region ZDateTime Fields

		public void TestFumigationCompletionEventTime()
		{
			AssertEquals("FumigationCompletionEventTime", DocsAndCartage.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Fumigation), DocsAndCartageWrapper.FumigationCompletionEventTime);
		}

		public void TestQuarantineInspectionCompletionEventTime()
		{
			AssertEquals("QuarantineInspectionCompletionEventTime", DocsAndCartage.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.QuarantineInspection), DocsAndCartageWrapper.QuarantineInspectionCompletionEventTime);
		}

		public void TestCustomsHoldCompletionEventTime()
		{
			AssertEquals("CustomsHoldCompletionEventTime", DocsAndCartage.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.CustomsHold), DocsAndCartageWrapper.CustomsHoldCompletionEventTime);
		}

		public void TestDeliveryCartageAdvised()
		{
			DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2004, 04, 04);
			AssertEquals("DeliveryCartageAdvised", DocsAndCartage.JP_DeliveryCartageAdvised, DocsAndCartageWrapper.DeliveryCartageAdvised);
		}

		public void TestDeliveryCartageCompleted()
		{
			DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2004, 04, 04);
			AssertEquals("DeliveryCartageCompleted", DocsAndCartage.JP_DeliveryCartageCompleted, DocsAndCartageWrapper.DeliveryCartageCompleted);
		}

		public void TestEstimatedDelivery()
		{
			DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2004, 04, 04);
			AssertEquals("EstimatedDelivery", DocsAndCartage.JP_EstimatedDelivery, DocsAndCartageWrapper.EstimatedDelivery);
		}

		public void TestEstimatedPickup()
		{
			DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2004, 04, 04);
			AssertEquals("EstimatedPickup", DocsAndCartage.JP_EstimatedPickup, DocsAndCartageWrapper.EstimatedPickup);
		}

		public void TestPickupCartageAdvised()
		{
			DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2004, 04, 04);
			AssertEquals("PickupCartageAdvised", DocsAndCartage.JP_PickupCartageAdvised, DocsAndCartageWrapper.PickupCartageAdvised);
		}

		public void TestPickupCartageCompleted()
		{
			DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2004, 04, 04);
			AssertEquals("PickupCartageCompleted", DocsAndCartage.JP_PickupCartageCompleted, DocsAndCartageWrapper.PickupCartageCompleted);
		}

		public void TestPickupRequiredBy()
		{
			DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2004, 04, 04);
			AssertEquals("PickupRequiredBy", DocsAndCartage.JP_PickupRequiredBy, DocsAndCartageWrapper.PickupRequiredBy);
		}

		public void TestDeliveryRequiredBy()
		{
			DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2004, 04, 04);
			AssertEquals("DeliveryRequiredBy", DocsAndCartage.JP_DeliveryRequiredBy, DocsAndCartageWrapper.DeliveryRequiredBy);
		}
		public void TestFCLAvailable()
		{
			DocsAndCartage.JP_FCLAvailable = new ZDateTime(2004, 04, 04);
			AssertEquals("FCLAvailable", DocsAndCartage.JP_FCLAvailable, DocsAndCartageWrapper.FCLAvailable);
		}

		public void TestLCLAvailable()
		{
			DocsAndCartage.JP_LCLAvailable = new ZDateTime(2004, 04, 04);
			AssertEquals("LCLAvailable", DocsAndCartage.JP_LCLAvailable, DocsAndCartageWrapper.LCLAvailable);
		}

		public void TestFCLStorageCommences()
		{
			DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2004, 04, 04);
			AssertEquals("FCLStorageCommences", DocsAndCartage.JP_FCLStorageCommences, DocsAndCartageWrapper.FCLStorageCommences);
		}

		public void TestLCLStorageCommences()
		{
			DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2004, 04, 04);
			AssertEquals("LCLStorageCommences", DocsAndCartage.JP_LCLStorageCommences, DocsAndCartageWrapper.LCLStorageCommences);
		}

		#endregion

		#region ZBool Fields

		public void TestIsFumigationCompleted()
		{
			Assert("!IsFumigationCompleted", !DocsAndCartageWrapper.IsFumigationCompleted);

			JobService fumigation = DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("!IsFumigationCompleted", !DocsAndCartageWrapper.IsFumigationCompleted);

			fumigation.ES_Completed = ZDateTime.Now;
			Assert("IsFumigationCompleted", DocsAndCartageWrapper.IsFumigationCompleted);
		}

		public void TestIsQuarantineCompleted()
		{
			Assert("!IsQuarantineCompleted", !DocsAndCartageWrapper.IsQuarantineCompleted);

			JobService quarantine = DocsAndCartage.Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			Assert("!IsQuarantineCompleted", !DocsAndCartageWrapper.IsQuarantineCompleted);

			quarantine.ES_Completed = ZDateTime.Now;
			Assert("IsQuarantineCompleted", DocsAndCartageWrapper.IsQuarantineCompleted);
		}

		public void TestIsCustomsHoldCompleted()
		{
			Assert("!IsCustomsHoldCompleted", !DocsAndCartageWrapper.IsCustomsHoldCompleted);

			JobService customsHold = DocsAndCartage.Services.AddNew();
			customsHold.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.CustomsHold;
			Assert("!IsCustomsHoldCompleted", !DocsAndCartageWrapper.IsCustomsHoldCompleted);

			customsHold.ES_Completed = ZDateTime.Now;
			Assert("IsCustomsHoldCompleted", DocsAndCartageWrapper.IsCustomsHoldCompleted);
		}

		public void TestHasProhibitedPackaging()
		{
			DocsAndCartage.JP_HasProhibitedPackaging = ZBool.False;
			Assert("!HasProhibitedPackaging", !DocsAndCartageWrapper.HasProhibitedPackaging);

			DocsAndCartage.JP_HasProhibitedPackaging = ZBool.True;
			Assert("HasProhibitedPackaging", DocsAndCartageWrapper.HasProhibitedPackaging);
		}

		public void TestInsuranceRequired()
		{
			DocsAndCartage.JP_InsuranceRequired = ZBool.False;
			Assert("!InsuranceRequired", !DocsAndCartageWrapper.InsuranceRequired);

			DocsAndCartage.JP_InsuranceRequired = ZBool.True;
			Assert("InsuranceRequired", DocsAndCartageWrapper.InsuranceRequired);
		}

		#endregion

		#region ZString Fields

		public void TestOrderItemsAsString()
		{
			DocsAndCartage.JP_OrderItemsAsString = "OrderItemsAsString";
			AssertEquals("OrderItemsAsString", DocsAndCartage.JP_OrderItemsAsString, DocsAndCartageWrapper.OrderItemsAsString);
		}

		public void TestStorageTimeUnits()
		{
			AssertEquals("StorageTimeUnits", DocsAndCartage.JP_StorageTimeUnits, DocsAndCartageWrapper.StorageTimeUnits);
		}

		public void TestFCLDeliveryEquipmentNeeded()
		{
			DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "FCL";
			AssertEquals("FCLDeliveryEquipmentNeeded", DocsAndCartage.JP_FCLDeliveryEquipmentNeeded, DocsAndCartageWrapper.FCLDeliveryEquipmentNeeded);
		}

		public void TestFCLPickupEquipmentNeeded()
		{
			DocsAndCartage.JP_FCLPickupEquipmentNeeded = "FCL";
			AssertEquals("FCLPickupEquipmentNeeded", DocsAndCartage.JP_FCLPickupEquipmentNeeded, DocsAndCartageWrapper.FCLPickupEquipmentNeeded);
		}

		#endregion

		#region ZDecimal Fields

		public void TestDeliveryLabourCharge()
		{
			DocsAndCartage.JP_DeliveryLabourCharge = 12.34M;
			AssertEquals("DeliveryLabourCharge", DocsAndCartage.JP_DeliveryLabourCharge, DocsAndCartageWrapper.DeliveryLabourCharge);
		}

		public void TestDeliveryLabourTime()
		{
			DocsAndCartage.JP_DeliveryLabourTime = new ZDateTime(2006, 1, 1, 5, 45, 0);
			AssertEquals("DeliveryLabourTime", DocsAndCartage.JP_DeliveryLabourTime, DocsAndCartageWrapper.DeliveryLabourTime);
		}

		public void TestDemurrageOnDeliveryCharge()
		{
			DocsAndCartage.JP_DeliveryTruckWaitCharge = 12.34M;
			AssertEquals("DemurrageOnDeliveryCharge", DocsAndCartage.JP_DeliveryTruckWaitCharge, DocsAndCartageWrapper.DemurrageOnDeliveryCharge);
		}

		public void TestDemurrageOnPickupCharge()
		{
			DocsAndCartage.JP_PickupTruckWaitCharge = 12.34M;
			AssertEquals("DemurrageOnPickupCharge", DocsAndCartage.JP_PickupTruckWaitCharge, DocsAndCartageWrapper.DemurrageOnPickupCharge);
		}

		public void TestLCLAirStorageCharge()
		{
			DocsAndCartage.JP_LCLAirStorageCharge = 12.34M;
			AssertEquals("LCLAirStorageCharge", DocsAndCartage.JP_LCLAirStorageCharge, DocsAndCartageWrapper.LCLAirStorageCharge);
		}

		public void TestPickupLabourCharge()
		{
			DocsAndCartage.JP_PickupLabourCharge = 12.34M;
			AssertEquals("PickupLabourCharge", DocsAndCartage.JP_PickupLabourCharge, DocsAndCartageWrapper.PickupLabourCharge);
		}

		public void TestPickupLabourTime()
		{
			DocsAndCartage.JP_PickupLabourTime = new ZDateTime(2006, 1, 1, 5, 43, 0);
			AssertEquals("PickupLabourHours", DocsAndCartage.JP_PickupLabourTime, DocsAndCartageWrapper.PickupLabourTime);
		}

		#endregion

		#region ZByte Fields

		public void TestDemurrageOnDeliveryTime()
		{
			DocsAndCartage.JP_DeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 5, 45, 0);
			AssertEquals("DemurrageOnDeliveryTime", DocsAndCartage.JP_DeliveryTruckWaitTime, DocsAndCartageWrapper.DemurrageOnDeliveryTime);
		}

		public void TestDemurrageOnPickupTime()
		{
			DocsAndCartage.JP_PickupTruckWaitTime = new ZDateTime(2006, 1, 1, 5, 45, 0);
			AssertEquals("DemurrageOnPickupTime", DocsAndCartage.JP_PickupTruckWaitTime, DocsAndCartageWrapper.DemurrageOnPickupTime);
		}

		public void TestLCLAirStorageDaysOrHours()
		{
			DocsAndCartage.JP_LCLAirStorageDaysOrHours = Convert.ToByte(1);
			AssertEquals("LCLAirStorageDaysOrHours", DocsAndCartage.JP_LCLAirStorageDaysOrHours, DocsAndCartageWrapper.LCLAirStorageDaysOrHours);
		}

		#endregion

		#region Wrapper Fields

		public void TestDeliveryAddress()
		{
			AssertNull("DeliveryAddress", DocsAndCartageWrapper.DeliveryAddress);
			var address = Factory.New<OrgAddress>();
			address.OA_Address1 = "Original Street";
			DocsAndCartageShipment.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;

			AssertNotNull("DeliveryAddress", DocsAndCartageWrapper.DeliveryAddress);
			AssertEquals("DeliveryAddress is of type DocDocAddress", typeof(DocDocAddress), DocsAndCartageWrapper.DeliveryAddress.GetType());
			AssertEquals("Original Street", DocsAndCartageWrapper.DeliveryAddress.Address1);

			JobDocAddress deliveryAddress = DocsAndCartageShipment.ConsigneeDeliveryAddress;
			deliveryAddress.E2_AddressOverride = true;
			deliveryAddress.E2_Address1 = "Some Street";
			deliveryAddress.E2_Address2 = "Some Other Street";
			deliveryAddress.E2_City = "Some Suburb";
			deliveryAddress.E2_Postcode = "1133";
			deliveryAddress.E2_State = "QLD";

			DocsAndCartageWrapper = DocJobDocsAndCartage.New(DocsAndCartage, Factory);
			AssertNotNull("DeliveryAddress", DocsAndCartageWrapper.DeliveryAddress);
			AssertEquals("Some Street", DocsAndCartageWrapper.DeliveryAddress.Address1);

			deliveryAddress.E2_AddressOverride = false;
			DocsAndCartageWrapper = DocJobDocsAndCartage.New(DocsAndCartage, Factory);
			AssertEquals("Don't override", "Original Street", DocsAndCartageWrapper.DeliveryAddress.Address1);
		}

		public void TestPickupAddress()
		{
			AssertNull("PickupAddress", DocsAndCartageWrapper.PickupAddress);
			var address = Factory.New<OrgAddress>();
			address.OA_Address1 = "Original Street";
			DocsAndCartageShipment.ConsignorPickupAddress.E2_OA_Address = address.PK;

			AssertNotNull("PickupAddress", DocsAndCartageWrapper.PickupAddress);
			AssertEquals("PickupAddress is of type DocDocAddress", typeof(DocDocAddress), DocsAndCartageWrapper.PickupAddress.GetType());
			AssertEquals("Original Street", DocsAndCartageWrapper.PickupAddress.Address1);

			JobDocAddress pickupAddress = DocsAndCartageShipment.ConsignorPickupAddress;
			pickupAddress.E2_AddressOverride = true;
			pickupAddress.E2_Address1 = "Zubin Street";
			pickupAddress.E2_Address2 = "Some Other Street";
			pickupAddress.E2_City = "Some Suburb";
			pickupAddress.E2_Postcode = "1133";
			pickupAddress.E2_State = "QLD";

			DocsAndCartageWrapper = DocJobDocsAndCartage.New(DocsAndCartage, Factory);
			AssertNotNull("PickupAddress", DocsAndCartageWrapper.PickupAddress);
			AssertEquals("Zubin Street", DocsAndCartageWrapper.PickupAddress.Address1);

			pickupAddress.E2_AddressOverride = false;
			DocsAndCartageWrapper = DocJobDocsAndCartage.New(DocsAndCartage, Factory);
			AssertEquals("Don't override", "Original Street", DocsAndCartageWrapper.PickupAddress.Address1);
		}

		public void TestDeliveryCartageCo()
		{
			AssertNull("DeliveryCartageCo", DocsAndCartageWrapper.DeliveryCartageCo);

			DocsAndCartage.DeliveryCartageCoPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNotNull("DeliveryCartageCo", DocsAndCartageWrapper.DeliveryCartageCo);
			AssertEquals("DeliveryCartageCo is of type DocOrganisation", typeof(DocOrganisation), DocsAndCartageWrapper.DeliveryCartageCo.GetType());
		}

		public void TestPickupCartageCo()
		{
			AssertNull("PickupCartageCo", DocsAndCartageWrapper.PickupCartageCo);

			DocsAndCartage.PickupCartageCoPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNotNull("PickupCartageCo", DocsAndCartageWrapper.PickupCartageCo);
			AssertEquals("PickupCartageCo is of type DocOrganisation", typeof(DocOrganisation), DocsAndCartageWrapper.PickupCartageCo.GetType());
		}

		public void TestConsignorDocumentaryAddress()
		{
			OrgHeader header = OrgHeader.New(Factory);
			header.Addresses[0].OA_Address1 = "Test";
			DocsAndCartage.Parent.ConsignorDocumentaryAddress.E2_OA_Address = header.Addresses[0].PK;
			AssertEquals("ConsignorDocumentaryAddress should not be null", "Test", DocsAndCartageWrapper.ConsignorDocumentaryAddress.Address1);
		}

		public void TestConsigneeDocumentaryAddress()
		{
			OrgHeader header = OrgHeader.New(Factory);
			header.Addresses[0].OA_Address1 = "Test";
			DocsAndCartage.Parent.ConsigneeDocumentaryAddress.E2_OA_Address = header.Addresses[0].PK;
			AssertEquals("ConsigneeDocumentaryAddress should not be null", "Test", DocsAndCartageWrapper.ConsigneeDocumentaryAddress.Address1);
		}

		public void TestJobDocumentsRequired()
		{
			AssertEquals("Precondition - no job documents required", 0, DocsAndCartageWrapper.JobDocumentsRequired.Count);
			JobRequiredDocument doc1 = DocsAndCartage.RequiredDocuments.AddNew();
			AssertEquals(1, DocsAndCartageWrapper.JobDocumentsRequired.Count);
			JobRequiredDocument doc2 = DocsAndCartage.RequiredDocuments.AddNew();
			doc2.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			AssertEquals(2, DocsAndCartageWrapper.JobDocumentsRequired.Count);
			DocsAndCartage.RequiredDocuments.RemoveAndDelete(doc1);
			AssertEquals(1, DocsAndCartageWrapper.JobDocumentsRequired.Count);
			AssertEquals(Core.Constants.RefDocTypes.CommercialInvoice, DocsAndCartageWrapper.JobDocumentsRequired[0].Type);
		}

		#endregion

		#region Implementation

		JobDocsAndCartage DocsAndCartage;
		DocJobDocsAndCartage DocsAndCartageWrapper;
		CommonShipment DocsAndCartageShipment;

		protected override void SetUp()
		{
			DocsAndCartageShipment = Factory.NewWithValidTestData<CommonShipment>();
			DocsAndCartage = DocsAndCartageShipment.DocsAndCartage;
			DocsAndCartageWrapper = DocJobDocsAndCartage.New(DocsAndCartage, Factory);
			base.SetUp();
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocJobDocsAndCartage.New(DocsAndCartage, Factory);
		}

		#region General/Reflection Test Implementation

		CommonShipment GenDocsAndCartageShipment
		{
			get
			{
				if (fGenDocsAndCartageShipment == null)
				{
					fGenDocsAndCartageShipment = Factory.NewWithValidTestData<CommonShipment>();
				}
				return fGenDocsAndCartageShipment;
			}
		}

		JobDocsAndCartage GenDocsAndCartage
		{
			get { return GenDocsAndCartageShipment.DocsAndCartage; }
		}

		CommonShipment fGenDocsAndCartageShipment;

		#endregion

		#endregion
	}
}
