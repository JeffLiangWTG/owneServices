using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.SWL.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.SWL.ServiceTasks
{
	internal sealed class ShipnetServiceTaskProcessor : ShipnetProcessor
	{
		public ShipnetServiceTaskProcessor(ILogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}
			this.logger = logger;

			CanContinue = true;

			shipnetCarriers = new OrgHeaderCollection(new BusinessObjectFactory(), new ShipnetCarrierFilter().GetFilter());
			shipnetCarriers.Load();
		}

		protected override void Notify(INotification @event)
		{
			LogProcess(@event.Message);
		}

		#region Implementation

		protected override ZDateTime EndDateTimeUTC
		{
			get { return ZDateTime.UtcNow; }
		}

		protected override ZDateTime StartDateTimeUTC
		{
			get
			{
				ZDateTime regoDateTime = new ZDateTime(SWLDataRegistry.Instance.ShipnetHighWaterMarkItem.Value);
				return regoDateTime.IsValid ? regoDateTime : ZDateTime.UtcNow.AddDays(-1);
			}
			set
			{
				if (value.IsValid)
				{
					SWLDataRegistry.Instance.ShipnetHighWaterMarkItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToDateTime());
				}
			}
		}

		protected override OrgHeaderCollection ShipnetCarriers
		{
			get { return shipnetCarriers; }
		}
		readonly OrgHeaderCollection shipnetCarriers;

		protected override bool IsEnvironmentValid()
		{
			bool result = false;
			if (IsShipnetEmailGroupValid)
			{
				result = base.IsEnvironmentValid();
			}
			return result;
		}

		bool IsShipnetEmailGroupValid
		{
			get
			{
				Guid emailGroupPK = SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.Value;

				if (emailGroupPK == Guid.Empty || !FactoryProvider.Current.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(GlbGroup)), new ZQuery(GlbGroupSchema.PK, emailGroupPK)))
				{
					logger.Log(LogType.Warning, "Shipnet Notification Email Group has not been properly setup; data cannot be exported.");
					return false;
				}

				return true;
			}
		}

		readonly ILogger logger;

		#endregion

		void LogProcess(string message)
		{
			try
			{
				logger.Information(message + " process " + System.Diagnostics.Process.GetCurrentProcess().Id);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Information(ex.Message + ": " + message);
			}
		}
	}
}
