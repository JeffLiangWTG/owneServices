using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	[DependentBusinessObject(typeof(BMReleaseSequence), "Items")]
	public class BMReleaseSequenceItem : AutoBMReleaseSequenceItem, IBMReleaseSequenceItem, IDataVersionLoggingSupported
	{
		public BMReleaseSequenceItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDecimal Duration
		{
			get
			{
				if (LinkedWorkflow == null)
				{
					return 0;
				}

				return LinkedWorkflow.TotalNonCancelledEstimatedHoursIncludingChildren;
			}
		}

		public ZDecimal Score
		{
			get
			{
				try
				{
					return (ZDecimal)BMI_Value / (ZDecimal)BMI_Investment * 100m;
				}
				catch (OverflowException)
				{
					return decimal.MaxValue;
				}
			}
		}

		public ZString Status
		{
			get
			{
				return LinkedWorkflow == null ? ZString.Empty : LinkedWorkflow.FH_Status;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryLoad", Justification = "Baseline")]
		public virtual ProcessHeader LinkedWorkflow
		{
			get { return (ProcessHeader)Factory.Load(typeof(ProcessHeader), BMI_FH_ProcessHeader); }
		}

		public BMReleaseSequence Sequence
		{
			get { return Factory.Load<BMReleaseSequence>(BMI_BMR_Sequence); }
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!IsInDatabase)
			{
				Sequence.Logs.AddNew(AutoEvents.ItemAdded, ZString.Format("Item added at position {0}", BMI_Position));
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!saveSucceeded)
			{
				var log = Sequence.Logs.MostRecentLogByEventTime(AutoEvents.ItemAdded);
				if (log != null)
				{
					log.Delete();
				}
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();
			Sequence.Logs.AddNew(AutoEvents.ItemRemoved, ZString.Format("Item removed at position {0}", BMI_Position));
		}
	}
}
