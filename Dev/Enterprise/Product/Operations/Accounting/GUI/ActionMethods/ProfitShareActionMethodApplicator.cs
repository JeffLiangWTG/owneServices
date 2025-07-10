using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class ProfitShareActionMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public ProfitShareActionMethodApplicator(Type actionSupporterType)
			: base(Res.GetString("5282ec30-f413-4466-aecf-6afc57925ecb", "Create Profit Share Charges"))
		{
			this.actionSupporterType = actionSupporterType;
		}

		readonly Type actionSupporterType;
		BusinessObjectFactoryProvider factoryProvider;
		bool ErrorTracked;

		new BusinessObjectFactory Factory
		{
			get { return factoryProvider.Current; }
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				return;
			}

			this.log = log;

			if (!AccountingUtils.ValidateProfitShareRegistry(targets.First().Factory, GlbCompany.CurrentCompany.PK, out string registryErrorMessage))
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, registryErrorMessage);
			}
			else
			{
				log.SetSectionProgressMax(targets.Length);

				foreach (BusinessObject target in targets)
				{
					factoryProvider = new BusinessObjectFactoryProvider(target.Factory);
					if (actionSupporterType.Name == "ForwardingConsolActionSupporter")
					{
						var consol = target as IJobCostingPlugIn;
						if (consol != null)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0}", ActionMethodsHelper.TargetReference(target));

							foreach (var shipmentPK in consol.CostSupporter.ShipmentsListPKs)
							{
								var shipment = Factory.Load<ForwardingShipment>(shipmentPK);

								ProcessShipment(shipment, log, ActionMethodsHelper.TargetReference(shipment));

								if (ErrorTracked)
								{
									factoryProvider.CreateNewWithoutSave();
								}
							}
						}
					}
					else if (actionSupporterType.Name == "QuotedBookingSupporter")
					{
						var shipment = (target is IQuotedBooking booking
							&& booking.ForwardingShipment != null
								? booking.ForwardingShipment
								: null) as IJobInvoicingPlugIn;
						if (shipment != null)
						{
							ProcessShipment(shipment, log, ActionMethodsHelper.TargetReference(target));
						}
					}
					else
					{
						var shipment = target as IJobInvoicingPlugIn;
						if (shipment != null)
						{
							ProcessShipment(shipment, log, ActionMethodsHelper.TargetReference(shipment as BusinessObject));
						}
					}

					log.BumpSectionProgress();
				}
			}
		}

		void ProcessShipment(IJobInvoicingPlugIn shipment, IOperationalActionSectionLog log, object targetReference)
		{
			ErrorTracked = false;

			bool result = false;
			log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0}", targetReference);
			var job = new Job.Loader(Factory, shipment).Load(true);

			if (job == null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("e831a68b-ef1e-4fb6-ae83-621aa143b3e1", "No job found."));
			}
			else
			{
				var processor = new ProfitShareShipmentChargeProcessor(Factory, shipment, job);
				processor.OnErrorOccurred += ProfitShareShipmentChargeProcessor_OnErrorOccurred;
				try
				{
					result = processor.Process();

					if (job.HasErrors())
					{
						ErrorTracked = true;
						foreach (var errorMessages in job.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Error, errorMessages);
						}
					}

					if (!ErrorTracked)
					{
						if (result)
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational
								, Res.GetString("2c2a5d5f-aa3c-4904-add2-cf687ae962a7", "Profit Share Charge found for creating or updating."));
						}
						else
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational
								, Res.GetString("d8de568a-a08d-43a0-88cc-04acc7d14606", "No Profit Share Charge found for creating or updating."));
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorTracked = true;
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, ex.Message);
				}
				finally
				{
					processor.OnErrorOccurred -= ProfitShareShipmentChargeProcessor_OnErrorOccurred;
				}
			}
		}

		IOperationalActionSectionLog log;

		void ProfitShareShipmentChargeProcessor_OnErrorOccurred(object sender, ProfitShareChargeCreationEventArgs e)
		{
			ErrorTracked = true;
			log.NotifyFormat(OperationalActionLogErrorLevel.Error, e.Message);
		}
	}
}
