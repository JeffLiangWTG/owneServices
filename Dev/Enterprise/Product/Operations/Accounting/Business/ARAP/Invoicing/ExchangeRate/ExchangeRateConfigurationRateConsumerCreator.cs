using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class ExchangeRateConfigurationRateConsumerCreator
	{
		internal static IAccExchangeRateConfigurationRateConsumer CreateExchangeRateConfigurationRateConsumerForJob(Job rateConsumerJob, GlbCompany apInvoiceCompany = null)
		{
			return new ExchangeRateConfigurationRateConsumerForJob(rateConsumerJob, apInvoiceCompany);
		}

		internal static IAccExchangeRateConfigurationRateConsumer CreateExchangeRateConfigurationRateConsumerForNonJob(GlbCompany apInvoiceCompany)
		{
			return new ExchangeRateConfigurationRateConsumerForNonJob(apInvoiceCompany);
		}

		internal static IAccExchangeRateConfigurationRateConsumer CreateExchangeRateConfigurationRateConsumerForConsolCost(IJobCostingPlugIn rateConsumerConsol, JobConsolCost consolCost, GlbCompany consolCostCompany)
		{
			return new ExchangeRateConfigurationRateConsumerForConsolCost(rateConsumerConsol, consolCostCompany, consolCost);
		}

		class ExchangeRateConfigurationRateConsumerForJob : IAccExchangeRateConfigurationRateConsumer
		{
			readonly Job job;
			readonly GlbCompany company;

			public ExchangeRateConfigurationRateConsumerForJob(Job rateConsumerJob, GlbCompany apInvoiceCompany)
			{
				job = rateConsumerJob;
				company = apInvoiceCompany;
			}

			#region IAccExchangeRateConfigurationRateConsumer

			ZString IAccExchangeRateConfigurationRateConsumer.JobType => job.JobType?.Code ?? ZString.Empty;

			ZString IAccExchangeRateConfigurationRateConsumer.Direction => job.Direction;

			ZString IAccExchangeRateConfigurationRateConsumer.TransportMode => job.TransportMode;

			ZGuid IAccExchangeRateConfigurationRateConsumer.LocalClientPK => job.LocalChargesPK;

			GlbCompany IAccExchangeRateConfigurationRateConsumer.Company => company ?? job.Company;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ConsolExchangeRateDate => ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualArrivalDate => job.PlugInData?.InvoicingSupporter?.ATA ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualDepartureDate => job.PlugInData?.InvoicingSupporter?.ATD ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedArrivalDate => job.PlugInData?.InvoicingSupporter?.ETA ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedDepartureDate => job.PlugInData?.InvoicingSupporter?.ETD ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedArrivalAtLoadPortDate => job.PlugInData?.InvoicingSupporter?.EstimatedArrivalAtLoadPort ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualArrivalAtLoadPortDate => job.PlugInData?.InvoicingSupporter?.ArrivalAtLoadPort ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.PickupDate => job.PlugInData?.InvoicingSupporter?.ESP ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.DeliveryDate => job.PlugInData?.InvoicingSupporter?.ESD ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.RequiredDate => job.PlugInData?.InvoicingSupporter?.REQ.ToZDateTime() ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.FinalizedDate => job.PlugInData?.InvoicingSupporter?.FIN.ToZDateTime() ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HouseBillIssueDate => job.JH_JS_HouseBillIssueDate;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ShipmentOrBrokeragePickupDate => job.PlugInData?.InvoicingSupporter?.GetOperationsSignificantDateByDirection(InvoiceDateConfigurationLookups.SignificantDateCodes.PickupDate, job.Direction) ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ShipmentOrBrokerageDeliveryDate => job.PlugInData?.InvoicingSupporter?.GetOperationsSignificantDateByDirection(InvoiceDateConfigurationLookups.SignificantDateCodes.DeliveryDate, job.Direction) ?? ZDateTime.Invalid;

			ZDecimal IAccExchangeRateConfigurationRateConsumer.GetExchangeRateForConsolExchangeRatePreference(RefCurrency currency)
			{
				var supporter = job.PlugInData?.InvoicingSupporter;
				return (supporter?.ConsolRateCurrency?.RX_Code ?? ZString.Empty) == currency.RX_Code
									? supporter.ConsolExchangeRate
									: supporter.GetConsolExchangeRate(currency.RX_Code);
			}

			#endregion
		}

		class ExchangeRateConfigurationRateConsumerForNonJob : IAccExchangeRateConfigurationRateConsumer
		{
			readonly GlbCompany company;

			public ExchangeRateConfigurationRateConsumerForNonJob(GlbCompany apInvoiceCompany)
			{
				company = apInvoiceCompany;
			}

			#region IAccExchangeRateConfigurationRateConsumer

			ZString IAccExchangeRateConfigurationRateConsumer.JobType => AccountingMasterFilesConstants.JobTypes.NonJobRelated;
			GlbCompany IAccExchangeRateConfigurationRateConsumer.Company => company;

			ZString IAccExchangeRateConfigurationRateConsumer.Direction => ZString.Empty;
			ZString IAccExchangeRateConfigurationRateConsumer.TransportMode => ZString.Empty;
			ZGuid IAccExchangeRateConfigurationRateConsumer.LocalClientPK => ZGuid.Empty;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.ConsolExchangeRateDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualArrivalDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualDepartureDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedArrivalDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedDepartureDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedArrivalAtLoadPortDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualArrivalAtLoadPortDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.PickupDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.DeliveryDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.RequiredDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.FinalizedDate => ZDateTime.Invalid;
			ZDateTime IAccExchangeRateConfigurationRateConsumer.HouseBillIssueDate => ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ShipmentOrBrokeragePickupDate => ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ShipmentOrBrokerageDeliveryDate => ZDateTime.Invalid;

			ZDecimal IAccExchangeRateConfigurationRateConsumer.GetExchangeRateForConsolExchangeRatePreference(RefCurrency currency) => ZDecimal.Zero;

			#endregion
		}

		class ExchangeRateConfigurationRateConsumerForConsolCost : IAccExchangeRateConfigurationRateConsumer
		{
			readonly IJobCostingPlugIn consol;
			readonly GlbCompany company;
			readonly ZGuid costPK;

			public ExchangeRateConfigurationRateConsumerForConsolCost(IJobCostingPlugIn rateConsumerConsol, GlbCompany consolCostCompany, JobConsolCost consolCost)
			{
				consol = rateConsumerConsol;
				if (consolCost != null
					&& rateConsumerConsol is IGateway gatewayConsol
					&& (gatewayConsol.GatewayBillingSupporter?.IsGatewayBillingEnabled() ?? false))
				{
					jobType = consolCost.IsGatewayConsolCost ? JobInvoicingConsumerTypes.GatewayConsolCode : JobInvoicingConsumerTypes.ForwardingConsolCode;
				}
				else
				{
					jobType = InvoicingPlugIn?.InvoicingSupporter?.ConsumerType.Code ?? ZString.Empty;
				}

				company = consolCostCompany;
				costPK = consolCost?.PK ?? ZGuid.Empty;
			}

			ZString GetConsolDirectionForExchangeRateConfiguration()
			{
				var direction = ZString.Empty;
				if (consol != null && consol.CostSupporter != null)
				{
					if (consol.CostSupporter.Direction == Directions.Import)
					{
						direction = Core.Constants.FreightShipmentDirection.Code.Import;
					}
					else if (consol.CostSupporter.Direction == Directions.Export)
					{
						direction = Core.Constants.FreightShipmentDirection.Code.Export;
					}
					else if (consol.CostSupporter.Direction == Directions.Domestic)
					{
						direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
					}
					else
					{
						direction = Core.Constants.FreightShipmentDirection.Code.Other;
					}
				}
				return direction;
			}

			IJobInvoicingPlugIn InvoicingPlugIn => consol is IJobInvoicingPlugIn plugin ? plugin : null;

			#region IAccExchangeRateConfigurationRateConsumer Members

			ZString IAccExchangeRateConfigurationRateConsumer.JobType => jobType;
			readonly ZString jobType;

			ZString IAccExchangeRateConfigurationRateConsumer.Direction => GetConsolDirectionForExchangeRateConfiguration();

			ZString IAccExchangeRateConfigurationRateConsumer.TransportMode => consol?.CostSupporter?.TransportMode ?? ZString.Empty;

			ZGuid IAccExchangeRateConfigurationRateConsumer.LocalClientPK => ZGuid.Empty;

			GlbCompany IAccExchangeRateConfigurationRateConsumer.Company => company;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ConsolExchangeRateDate => ZDateTime.Today;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualArrivalDate => InvoicingPlugIn?.InvoicingSupporter?.ATA ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualDepartureDate => InvoicingPlugIn?.InvoicingSupporter?.ATD ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedArrivalDate => consol?.CostSupporter?.ETA ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedDepartureDate => consol?.CostSupporter?.ETD ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromEstimatedArrivalAtLoadPortDate => InvoicingPlugIn?.InvoicingSupporter?.EstimatedArrivalAtLoadPort ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HistoricalRateFromActualArrivalAtLoadPortDate => InvoicingPlugIn?.InvoicingSupporter?.ArrivalAtLoadPort ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.PickupDate => InvoicingPlugIn?.InvoicingSupporter?.ESP ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.DeliveryDate => InvoicingPlugIn?.InvoicingSupporter?.ESD ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.RequiredDate => InvoicingPlugIn?.InvoicingSupporter?.REQ.ToZDateTime() ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.FinalizedDate => InvoicingPlugIn?.InvoicingSupporter?.FIN.ToZDateTime() ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.HouseBillIssueDate => ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ShipmentOrBrokeragePickupDate => InvoicingPlugIn?.InvoicingSupporter?.GetOperationsSignificantDateByDirection(InvoiceDateConfigurationLookups.SignificantDateCodes.PickupDate, JobInvoicingSupporterExtensions.GetJobDirection(InvoicingPlugIn?.InvoicingSupporter).ToString()) ?? ZDateTime.Invalid;

			ZDateTime IAccExchangeRateConfigurationRateConsumer.ShipmentOrBrokerageDeliveryDate => InvoicingPlugIn?.InvoicingSupporter?.GetOperationsSignificantDateByDirection(InvoiceDateConfigurationLookups.SignificantDateCodes.DeliveryDate, JobInvoicingSupporterExtensions.GetJobDirection(InvoicingPlugIn?.InvoicingSupporter).ToString()) ?? ZDateTime.Invalid;

			ZDecimal IAccExchangeRateConfigurationRateConsumer.GetExchangeRateForConsolExchangeRatePreference(RefCurrency currency)
			{
				return consol?.ExchangeRateForCurrency(currency, costPK) ?? ZDecimal.Zero;
			}

			#endregion
		}
	}
}
