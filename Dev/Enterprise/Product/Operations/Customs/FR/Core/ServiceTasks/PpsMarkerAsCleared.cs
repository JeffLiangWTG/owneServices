using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Registry;

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class PpsMarkerAsCleared
	{
		readonly BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		const int DefaultFallbackTimerInMinutes = 30;

		public PpsMarkerAsCleared(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void DoEverything(ZString country)
		{
			var ppsEntryNums = ServiceTaskHelper.GetFallbackEntryNumbersInStatus(factory, DeltaGFallbackStatusList.Codes.PPS, country);
			var fallbackTimeInMinutes = ServiceTaskHelper.IfZero(FRCustomsDataRegistry.Instance.FallbackTimerInMinutes.Value, DefaultFallbackTimerInMinutes);

			foreach (var entryNum in ppsEntryNums)
			{
				ProcessPPSEntryNumToMarkItAsPDSAfterFallbackTimer(entryNum, fallbackTimeInMinutes);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		void ProcessPPSEntryNumToMarkItAsPDSAfterFallbackTimer(CusEntryNumber entryNum, int minutes)
		{
			var date = ZDateTime.Now.AddMinutes(-minutes);
			if (entryNum.CE_IssueDate < date)
			{
				entryNum.CE_EntryStatus = DeltaGFallbackStatusList.Codes.PDS;
			}
			entryNum.Factory.Save();
		}
	}
}
