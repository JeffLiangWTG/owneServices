using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business.Asycuda;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	public class IShipmentDataExtensionTest : TestCaseWithFactory
	{
		public void TestLogBISIEventAfterUploaded()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var manifestBill = manifestHeader.Bills.AddNew();
			AssertEquals(0, manifestBill.Logs.GetAllLogs().Count);
			var asycudaShipmentData = new AsycudaIShipmentDataProxy(manifestBill);
			asycudaShipmentData.LogBISIEventAfterUploaded();
			AssertEquals(1, manifestBill.Logs.GetAllLogs().Count);
			var mostRecentLog = manifestBill.Logs.MostRecentLog;
			AssertNotNull(mostRecentLog);
			AssertEquals(Events.DataExport.Code, mostRecentLog.SL_SE_NKEvent);
			AssertEquals(IShipmentDataExtension.BISIReference, mostRecentLog.SL_Reference);
		}
	}
}
