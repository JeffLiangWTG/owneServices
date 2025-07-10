using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class RNSPlugInSupportTallyWrapperTest : TestCaseWithFactory
	{
		[TestDate(2011, 3, 25)]
		public void TestIRNSPlugInSupportProperties()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var entryNumber = consol.Numbers.AddNew();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			var container = Factory.New<TallyContainer>();
			consol.Containers.Add(container);

			IRNSPlugInSupport wrapper = new RNSPlugInSupportTallyWrapper(container);
			var visibilityChangedCount = 0;
			wrapper.PlugInVisibilityDataChanged += (s, e) => visibilityChangedCount++;

			AssertEquals("DateOfArrival", ZDateTime.Now, wrapper.DateOfArrival);
			AssertEquals("CargoControlNumber", "123", wrapper.CargoControlNumber);
			AssertEquals("TransactionNumber", ZString.Empty, wrapper.TransactionNumber);
			AssertEquals("OfficeCode", ZString.Empty, wrapper.OfficeCode);
			AssertEquals("SubLocationCode", ZString.Empty, wrapper.SubLocationCode);
			AssertEquals("Master", container, wrapper.Master);
			AssertEquals("PlugInVisible", false, wrapper.PlugInVisible);

			container.Consol.JK_RL_NKLoadPort = "USAAA";
			container.Consol.JK_RL_NKDischargePort = "CABBB";

			AssertEquals("PlugInVisible", true, wrapper.PlugInVisible);
			AssertEquals("PlugInVisibilityDataChanged", 2, visibilityChangedCount);
		}
	}
}
