using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareShipmentChargeProcessor
	{
		public ProfitShareShipmentChargeProcessor(
			BusinessObjectFactory factory,
			IJobInvoicingPlugIn shipment,
			Job job,
			bool saveCharges = true,
			IObjectProvider objectProvider = null)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(shipment, "shipment");
			Argument.NotNull(job, "job");

			this.factory = factory;
			this.shipment = shipment;
			this.job = job;
			this.saveCharges = saveCharges;
			ObjectProvider = objectProvider ?? new ObjectProviderImplementation();
		}

		readonly BusinessObjectFactory factory;
		readonly IJobInvoicingPlugIn shipment;
		readonly Job job;
		readonly bool saveCharges;
		public IObjectProvider ObjectProvider { get; }

		public bool Process()
		{
			bool result = false;
			try
			{
				var profitShareDetails = ObjectProvider.GetCalculator(factory, null, shipment).CreateProfitShares();
				var chargeCreator = ObjectProvider.GetChargeCreator(profitShareDetails, job, saveCharges);

				chargeCreator.RunPreCreateValidation();
				if (!chargeCreator.ValidationErrors.IsEmpty)
				{
					RaiseOnErrorOccurredEvent(chargeCreator.ValidationErrors);
					return false;
				}

				result = chargeCreator.CreateCharges();
				if (!chargeCreator.ValidationErrors.IsEmpty)
				{
					RaiseOnErrorOccurredEvent(chargeCreator.ValidationErrors);
					return false;
				}
			}
			catch (OnSavingCriticalCheckException ex)
			{
				RaiseOnErrorOccurredEvent(ex.Message);
			}
			catch (RethrownByExceptionHandlerException e) when (e.InnerException is OnSavingCriticalCheckException criticalValidationException)
			{
				RaiseOnErrorOccurredEvent(criticalValidationException.Message);
			}

			return result;
		}

		public event EventHandler<ProfitShareChargeCreationEventArgs> OnErrorOccurred;

		protected void RaiseOnErrorOccurredEvent(ZString message)
		{
			if (OnErrorOccurred != null)
			{
				OnErrorOccurred(this, new ProfitShareChargeCreationEventArgs(message));
			}
		}

		#region IObjectProvider and Implementation

		public interface IObjectProvider
		{
			IProfitShareCalculator GetCalculator(BusinessObjectFactory factory, IJobCostingPlugIn consol, params IJobInvoicingPlugIn[] shipments);

			IProfitShareChargeCreator GetChargeCreator(ProfitShareDetailCollection profitShareDetails, Job job, bool saveCharges);
		}

		[WTG.StaticAnalysis.Annotation.Immutable]
		public class ObjectProviderImplementation : IObjectProvider
		{
			public IProfitShareCalculator GetCalculator(BusinessObjectFactory factory, IJobCostingPlugIn consol, params IJobInvoicingPlugIn[] shipments)
				=> new ProfitShareCalculator(factory, null, shipments);

			public IProfitShareChargeCreator GetChargeCreator(ProfitShareDetailCollection profitShareDetails, Job job, bool saveCharges)
				=> new ProfitShareShipmentChargeCreator(profitShareDetails, job, saveCharges);
		}

		#endregion
	}
}
