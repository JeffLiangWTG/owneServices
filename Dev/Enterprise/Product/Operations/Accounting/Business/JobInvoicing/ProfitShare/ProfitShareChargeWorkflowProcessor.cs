using System;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class ProfitShareChargeWorkflowProcessor : IProcessor
	{
		public ProfitShareChargeWorkflowProcessor(IJobInvoicingPlugIn plugIn, ProfitShareShipmentChargeProcessor.IObjectProvider objectProvider = null)
		{
			Argument.NotNull(plugIn, "plugIn");
			PlugIn = plugIn;
			Factory = plugIn.Factory;
			ObjectProvider = objectProvider ?? new ProfitShareShipmentChargeProcessor.ObjectProviderImplementation();
		}

		public ProfitShareChargeWorkflowProcessor(IJobCostingPlugIn consol, ProfitShareShipmentChargeProcessor.IObjectProvider objectProvider = null)
		{
			Argument.NotNull(consol, "consol");
			Consol = consol;
			Factory = consol.Factory;
			ObjectProvider = objectProvider ?? new ProfitShareShipmentChargeProcessor.ObjectProviderImplementation();
		}

		IJobInvoicingPlugIn PlugIn;
		readonly IJobCostingPlugIn Consol;
		readonly BusinessObjectFactory Factory;
		readonly ProfitShareShipmentChargeProcessor.IObjectProvider ObjectProvider;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			this.notifications = new NotificationCollection();
			var shouldSaveFactory = false;

			try
			{
				if (!AccountingUtils.ValidateProfitShareRegistry(Factory, GlbCompany.CurrentCompany.PK, out string registryErrorMessage))
				{
					this.notifications.AddError(registryErrorMessage);
				}
				else
				{
					if (Consol != null)
					{
						var isFirstIteration = true;
						foreach (var shipmentPK in Consol.CostSupporter.ShipmentsListPKs)
						{
							PlugIn = Factory.Load<ForwardingShipment>(shipmentPK);
							shouldSaveFactory = ProcessShipment(PlugIn, this.notifications) && (shouldSaveFactory || isFirstIteration);
							isFirstIteration = false;
						}
					}
					else if (PlugIn != null)
					{
						shouldSaveFactory = ProcessShipment(PlugIn, this.notifications);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AddErrorToNotification(ex.Message);
			}
			shouldSaveFactory = shouldSaveFactory && !this.notifications.HasErrors();

			var email = this.notifications.HasErrors()
						? new ProfitShareChargeWorkflowProcessorErrorEmail(PlugIn, Consol, this.notifications).Render()
						: null;
			if (!shouldSaveFactory)
			{
				throw new LogSubscriberToAbortLogGroupProcessingSilentlyException("Profit share charge processing is not successful.", email);
			}
		}

		void AddErrorToNotification(ZString errorMessage)
		{
			StringBuilder message = new StringBuilder();
			if (IsReportingForConsol)
			{
				message.Append(PlugIn.JobNumber).Append(" - ");
			}

			message.Append(errorMessage);
			notifications.AddError(message.ToString());
		}

		bool ProcessShipment(IJobInvoicingPlugIn plugIn, NotificationCollection notifications)
		{
			bool result = false;
			var loader = new Job.Loader(Factory, plugIn);
			var job = loader.Load(setParent: true, setJobDefaults: false);

			if (job != null)
			{
				var processor = new ProfitShareShipmentChargeProcessor(Factory, plugIn, job, saveCharges: false, objectProvider: ObjectProvider);
				processor.OnErrorOccurred += ProfitShareShipmentChargeProcessor_OnErrorOccurred;
				try
				{
					result = processor.Process();

					if (job.HasErrors())
					{
						foreach (var errorMessage in job.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
						{
							AddErrorToNotification(errorMessage);
						}
					}
				}
				finally
				{
					processor.OnErrorOccurred -= ProfitShareShipmentChargeProcessor_OnErrorOccurred;
				}
			}

			return result;
		}

		bool IsReportingForConsol
		{
			get { return Consol != null; }
		}

		void ProfitShareShipmentChargeProcessor_OnErrorOccurred(object sender, ProfitShareChargeCreationEventArgs e)
		{
			AddErrorToNotification(e.Message);
		}

		NotificationCollection notifications;
	}
}
