using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class RNSPlugInSupportShipmentWrapperTest : TestCaseWithFactory
	{
		[TestDate(2011, 3, 25)]
		public void TestIRNSPlugInSupportProperties()
		{
			AssertIRNSPlugInSupportProperties(Factory.New<ForwardingShipment>());
			AssertIRNSPlugInSupportProperties(Factory.New<CFSShipment>());
		}

		void AssertIRNSPlugInSupportProperties(Freight.Business.CommonShipment shipment)
		{
			shipment.JS_HouseBill = "TEST1234567";
			var entryNumber = shipment.Numbers.AddNew();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			IRNSPlugInSupport wrapper = new RNSPlugInSupportShipmentWrapper(shipment);
			var visibilityChangedCount = 0;
			wrapper.PlugInVisibilityDataChanged += (s, e) => visibilityChangedCount++;

			AssertEquals("DateOfArrival", ZDateTime.Now, wrapper.DateOfArrival);
			AssertEquals("HouseBillNumber", "TEST1234567", wrapper.HouseBillNumber);
			AssertEquals("CargoControlNumber", "123", wrapper.CargoControlNumber);
			AssertEquals("TransactionNumber", ZString.Empty, wrapper.TransactionNumber);
			AssertEquals("OfficeCode", ZString.Empty, wrapper.OfficeCode);
			AssertEquals("SubLocationCode", ZString.Empty, wrapper.SubLocationCode);
			AssertEquals("Master", shipment, wrapper.Master);
			AssertEquals("PlugInVisible", false, wrapper.PlugInVisible);
			if (shipment is ForwardingShipment forwardingShipment)
			{
				AssertEquals("ReleaseStatusEventsSupported", true, wrapper.ReleaseStatusEventsSupported);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = forwardingShipment.PK;
				AssertNotNull(forwardingShipment.GetDeclaration());
				AssertEquals("ReleaseStatusEventsSupported", false, wrapper.ReleaseStatusEventsSupported);
			}
			else
			{
				AssertEquals("ReleaseStatusEventsSupported", true, wrapper.ReleaseStatusEventsSupported);
			}

			shipment.JS_RL_NKOrigin = "USAAA";
			shipment.JS_RL_NKDestination = "CABBB";

			AssertEquals("PlugInVisible", true, wrapper.PlugInVisible);
			AssertEquals("PlugInVisibilityDataChanged", 2, visibilityChangedCount);
		}
	}
}
