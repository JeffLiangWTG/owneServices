using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyForEventDatePropertyTesting : DummyEnterpriseBusinessObject
	{
		public DummyForEventDatePropertyTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual)]
		public override ZDateTime Z0_Date
		{
			get { return base.Z0_Date; }
			set
			{
				base.Z0_Date = value;
				var asOffset = new ZDateTimeOffset(value);
				Logs.CreateOrRecreateEventLog(Events.GateIn, EstimateActual.Actual, asOffset);
				Logs.CreateOrRecreateEventLog(Events.Dehire, EstimateActual.Actual, asOffset);
				Logs.CreateRecreateOrUpdateEventLog(Events.GateIn, EstimateActual.Actual, asOffset);
				Logs.CreateRecreateOrUpdateEventLog(Events.Dehire, EstimateActual.Actual, asOffset);
				Logs.CreateRecreateOrUpdateEventLog(Events.GateOut, EstimateActual.Actual, asOffset);
			}
		}
	}
}
