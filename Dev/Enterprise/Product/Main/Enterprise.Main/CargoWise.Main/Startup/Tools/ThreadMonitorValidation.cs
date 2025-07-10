using System;
using CargoWise.EntityFramework;
using ResString = CargoWise.Main.ResString;

namespace Enterprise.Startup.Tools
{
	public class ThreadMonitorValidation : ZValidation
	{
		public ThreadMonitorValidation(ThreadMonitor parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ThreadMonitor parent;

		#region IntervalNumber

		public void ValidateIntervalNumber()
		{
			ValidateCalculatedProperty(parent.IntervalNumberInfo);
		}

		protected void CheckIntervalNumber()
		{
			CompareValidation.CheckNumberGreaterThanZero(parent.IntervalNumberInfo);

			switch (parent.IntervalType)
			{
				case IntervalTypeConstant.Second:
					CompareValidation.CheckLessThanOrEqualTo(parent.IntervalNumberInfo, parent.MaxSeconds);
					break;

				case IntervalTypeConstant.Minute:
					CompareValidation.CheckLessThanOrEqualTo(parent.IntervalNumberInfo, parent.MaxMinutes);
					break;
			}
		}

		#endregion

		#region IntervalType

		public void ValidateIntervalType()
		{
			ValidateCalculatedProperty(parent.IntervalTypeInfo);
		}

		protected void CheckIntervalType()
		{
			MandatoryValidation.CheckEntered(parent.IntervalTypeInfo);
			ListValidation.ErrorIfInvalidCode(parent.IntervalTypeInfo, parent.IntervalTypes);
		}

		#endregion

		#region SelectedThreadId

		public void ValidateSelectedThread()
		{
			ValidateCalculatedProperty(parent.SelectedThreadIdInfo);
		}

		protected void CheckSelectedThreadId()
		{
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("a75077ef-427e-4f67-bdd8-cdf3345c816d", "The selected thread ID does not correspond to an active thread. Please select an active thread."), parent.SelectedThreadIdInfo);
			CompareValidation.CheckNumberGreaterThanZero(parent.SelectedThreadIdInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateIntervalNumber();
			ValidateIntervalType();
			ValidateSelectedThread();
		}

		public override Type AutoValidationType
		{
			get { return typeof(ThreadMonitorValidation); }
		}
	}
}
