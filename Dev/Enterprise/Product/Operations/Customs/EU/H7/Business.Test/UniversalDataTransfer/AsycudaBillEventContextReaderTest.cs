using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer.Test
{
	public class AsycudaBillEventContextReaderTest : TestCaseWithFactory
	{
		public void TestAddEventContextValues()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.MasterBill.ABL_BillNumber = "MBN1";
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
				var bill = header.Bills.AddNew();
				bill.ABL_BillStatus = "CLR";
				bill.ABL_BillNumber = "1";
				var manager = bill.GetUniversalDataContextManager() as IEventDataContextManager;
				var contextValues = manager.EventContextValues;
				AssertEquals(3, contextValues.Count());
				CombineAssertions(() =>
				{
					AssertEquals("MBN1", contextValues.FirstOrDefault(x => x.Key.Type == nameof(UniversalEvent.ContextTypes.MAWBNumber)).Value);
					AssertEquals("CLR", contextValues.FirstOrDefault(x => x.Key.Type == nameof(UniversalEvent.ContextTypes.ComplianceStatus)).Value);
					AssertEquals("1", contextValues.FirstOrDefault(x => x.Key.Type == nameof(UniversalEvent.ContextTypes.HAWBNumber)).Value);
				});
			}
		}
	}
}
