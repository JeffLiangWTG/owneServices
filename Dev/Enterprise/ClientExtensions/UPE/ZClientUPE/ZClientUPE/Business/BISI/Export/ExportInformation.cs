
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ExportInformation
	{
		public ExportInformation(int batchNumber, ZDateTime completedStartDate, ZDateTime completedEndDate)
		{
			this.BatchNumber = batchNumber;
			this.CompletedStartDate = completedStartDate;
			this.CompletedEndDate = completedEndDate;
		}

		public int BatchNumber;
		public ZDateTime CompletedStartDate;
		public ZDateTime CompletedEndDate;

		#region Every Day Export

		public bool RunEveryDayExport;
		public ZDateTime EveryDayStartDate;
		public ZDateTime EveryDayEndDate;

		#endregion

		#region Every Day Date of Arrival Export

		public bool RunEveryDayDateOfArrivalExport;

		public ZDateTime EveryDayDateOfArrivalStartDate
		{
			get { return fEveryDayDateOfArrivalStartDate.Date; }
			set { fEveryDayDateOfArrivalStartDate = value; }
		}
		ZDateTime fEveryDayDateOfArrivalStartDate;

		public ZDateTime EveryDayDateOfArrivalEndDate
		{
			get { return fEveryDayDateOfArrivalEndDate.Date; }
			set { fEveryDayDateOfArrivalEndDate = value; }
		}
		ZDateTime fEveryDayDateOfArrivalEndDate;

		#endregion

		#region Working Day Metro Export

		public bool RunWorkingDayMetroExport;
		public ZDateTime WorkingDayMetroStartDate;
		public ZDateTime WorkingDayMetroEndDate;

		#endregion

		#region Working Day Date of Arrival Metro Export

		public bool RunWorkingDayDateOfArrivalMetroExport;

		public ZDateTime WorkingDayDateOfArrivalMetroStartDate
		{
			get { return fWorkingDayDateOfArrivalMetroStartDate.Date; }
			set { fWorkingDayDateOfArrivalMetroStartDate = value; }
		}
		ZDateTime fWorkingDayDateOfArrivalMetroStartDate;

		public ZDateTime WorkingDayDateOfArrivalMetroEndDate
		{
			get { return fWorkingDayDateOfArrivalMetroEndDate.Date; }
			set { fWorkingDayDateOfArrivalMetroEndDate = value; }
		}
		ZDateTime fWorkingDayDateOfArrivalMetroEndDate;

		#endregion

		#region Working Day Other Export

		public bool RunWorkingDayOtherExport;
		public ZDateTime WorkingDayOtherStartDate;
		public ZDateTime WorkingDayOtherEndDate;

		#endregion

		#region Working Day Date of Arrival Other Export

		public bool RunWorkingDayDateOfArrivalOtherExport;

		public ZDateTime WorkingDayDateOfArrivalOtherStartDate
		{
			get { return fWorkingDayDateOfArrivalOtherStartDate.Date; }
			set { fWorkingDayDateOfArrivalOtherStartDate = value; }
		}
		ZDateTime fWorkingDayDateOfArrivalOtherStartDate;

		public ZDateTime WorkingDayDateOfArrivalOtherEndDate
		{
			get { return fWorkingDayDateOfArrivalOtherEndDate.Date; }
			set { fWorkingDayDateOfArrivalOtherEndDate = value; }
		}
		ZDateTime fWorkingDayDateOfArrivalOtherEndDate;

		#endregion
	}
}
