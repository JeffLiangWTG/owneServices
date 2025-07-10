using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class RNSPlugInSupportConsolWrapperTest : TestCaseWithFactory
	{
		[TestDate(2011, 3, 25)]
		public void TestIRNSPlugInSupportProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			var entryNumber = consol.Numbers.AddNew();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			IRNSPlugInSupport wrapper = new RNSPlugInSupportConsolWrapper(consol);
			var visibilityChangedCount = 0;
			wrapper.PlugInVisibilityDataChanged += (s, e) => visibilityChangedCount++;

			AssertEquals("DateOfArrival", ZDateTime.Now, wrapper.DateOfArrival);
			AssertEquals("HouseBillNumber", ZString.Empty, wrapper.HouseBillNumber);
			AssertEquals("CargoControlNumber", "123", wrapper.CargoControlNumber);
			AssertEquals("TransactionNumber", ZString.Empty, wrapper.TransactionNumber);
			AssertEquals("OfficeCode", ZString.Empty, wrapper.OfficeCode);
			AssertEquals("SubLocationCode", ZString.Empty, wrapper.SubLocationCode);
			AssertEquals("Master", consol, wrapper.Master);
			AssertEquals("PlugInVisible", false, wrapper.PlugInVisible);
			AssertEquals(typeof(RNSMultiMessageManager), wrapper.GetRNSMultiMessageManager().GetType());

			consol.JK_RL_NKLoadPort = "USAAA";
			consol.JK_RL_NKDischargePort = "CABBB";

			AssertEquals("PlugInVisible", true, wrapper.PlugInVisible);
			AssertEquals("PlugInVisibilityDataChanged", 2, visibilityChangedCount);
		}
	}
}
