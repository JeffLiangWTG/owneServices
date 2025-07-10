using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyWithRecursiveEventBadness : DummyEnterpriseBusinessObject
	{
		public DummyWithRecursiveEventBadness(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		[EventDateProperty(AutoEvents.CustomisableEvent00Code, EstimateActual.Actual)]
		public override ZDateTime Z0_Date
		{
			get { return base.Z0_Date; }
			set
			{
				base.Z0_Date = value;
				if (!value.IsEmpty)
				{
					Logs.AddNew(Events.CustomisableEvent00, value.ToOffset());
				}
			}
		}

		[EventDateProperty(AutoEvents.CustomisableEvent01Code, EstimateActual.Actual)]
		public override ZDateTime Z0_AnotherDate
		{
			get { return base.Z0_AnotherDate; }
			set
			{
				base.Z0_AnotherDate = value;
				Logs.CreateOrRecreateEventLog(Events.CustomisableEvent01, EstimateActual.Actual, value.ToOffset());
			}
		}

		[EventDateProperty(AutoEvents.CustomisableEvent02Code, EstimateActual.Actual)]
		public override ZDateTime Z0_SmallDateTime
		{
			get { return base.Z0_SmallDateTime; }
			set
			{
				base.Z0_SmallDateTime = value;
				Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent02, EstimateActual.Actual, value.ToOffset());
			}
		}
	}
}
