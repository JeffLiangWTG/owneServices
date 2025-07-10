using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using FreightBiz = Enterprise.Freight.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobDeclaration : JobDeclaration
	{
		public WoolworthsJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString JE_RL_NKFinalDestination
		{
			get { return base.JE_RL_NKFinalDestination; }
			set
			{
				if (IsCopying || !HasLinkToOneOrMoreOrders)
				{
					base.JE_RL_NKFinalDestination = value;
				}
				else if (OnOrderInvoiceMismatching())
				{
					base.JE_RL_NKFinalDestination = value;
				}
				else
				{
					JE_RL_NKFinalDestinationInfo.RefreshBinding();
				}
			}
		}

		public override Type DocsAndCartageType
		{
			get { return typeof(WoolworthsJobDocsAndCartage); }
		}

		public override Type DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		public event CancelEventHandler OrderInvoiceMismatching;
		internal bool OnOrderInvoiceMismatching()
		{
			CancelEventArgs e = new CancelEventArgs(false);
			if (OrderInvoiceMismatching != null)
			{
				OrderInvoiceMismatching(this, e);
			}
			return !e.Cancel;
		}

		public void RunOrderInvoiceMismatchValidation()
		{
			new WoolworthsJobDeclarationValidation(this).RunOrderInvoiceMismatchValidation();
		}

		#region BusinessObject Overrides

		protected override Enterprise.Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			Enterprise.Customs.Business.JobDeclarationValidation result = base.GetNewValidation();
			result.Add(new WoolworthsJobDeclarationValidation(this));
			return result;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			StmALog containerUnpackLog = Logs.MostRecentLogByEventTime(Events.ContainerUnpack);

			if (containerUnpackLog != null && !containerUnpackLog.SL_IsCancelled && !containerUnpackLog.IsInDatabase && !JE_IsCancelled)
			{
				foreach (WoolworthsCusContainer container in CusContainers)
				{
					container.MarkOrderContainersAsDelivered();
				}
			}
		}

		#endregion

		#region Events

		public event CancelEventHandler UpdatingCusContainerNumber;
		internal bool OnUpdatingCusContainerNumber()
		{
			CancelEventArgs e = new CancelEventArgs(false);
			if (UpdatingCusContainerNumber != null)
			{
				UpdatingCusContainerNumber(this, e);
			}
			return !e.Cancel;
		}

		public event CancelEventHandler DeletingCusContainer;
		internal bool OnDeletingCusContainer()
		{
			CancelEventArgs e = new CancelEventArgs(false);
			if (DeletingCusContainer != null)
			{
				DeletingCusContainer(this, e);
			}
			return !e.Cancel;
		}

		#endregion

		#region Implementation

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
		{
			return new WoolworthsCusContainerCollection(this, Factory);
		}

		protected bool HasLinkToOneOrMoreOrders
		{
			get
			{
				bool result = false;
				foreach (WoolworthsJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (OrderLineDelivery delivery in invoiceLine.OrderLineDeliveries)
					{
						if (delivery.J4_RL_NKDestinationPort.ToUpper() == JE_RL_NKFinalDestination.ToUpper())
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (Shipment == null)
			{
				new Enterprise.MasterFiles.Business.ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
				if (HasChanges)
				{
					DefaultDeliveryOrPickupDatesFromContainers();
				}
			}
		}

		void DefaultDeliveryOrPickupDatesFromContainers()
		{
			if (CusContainers.Count > 0)
			{
				var propertiesToDefault = new[] { JE_EstimatedDeliveryOrPickupInfo, JP_Calc_CartageAdvisedInfo, JE_CartageCompletedInfo };
				var containerPropertiesToDefaultFrom = IsImport ? new[] { FreightBiz.CommonContainer.Schema.JC_ArrivalEstimatedDelivery, FreightBiz.CommonContainer.Schema.JC_ArrivalCartageAdvised, FreightBiz.CommonContainer.Schema.JC_ArrivalCartageComplete }
														: new[] { FreightBiz.CommonContainer.Schema.JC_DepartureEstimatedPickup, FreightBiz.CommonContainer.Schema.JC_DepartureCartageAdvised, FreightBiz.CommonContainer.Schema.JC_DepartureCartageComplete };

				for (var i = 0; i < propertiesToDefault.Length; i++)
				{
					DefaultDateFromLastDeliveredOrPickedUpContainer(propertiesToDefault[i], containerPropertiesToDefaultFrom[i]);
				}
			}
		}

		void DefaultDateFromLastDeliveredOrPickedUpContainer(IZPropertyInfo dateInfoToUpdate, string containerPropertyToUpdateFrom)
		{
			if (dateInfoToUpdate.Value.IsEmpty
				&& CusContainers.All<CusContainer>(cont => !((ZDateTime)cont.JobContainer[containerPropertyToUpdateFrom]).IsEmpty)
				&& containerPropertyToUpdateFrom != Enterprise.Freight.Business.CommonContainer.Schema.JC_ArrivalCartageAdvised)
			{
				var latestDate = (from Enterprise.Customs.Business.BaseCusContainer cusContainer in CusContainers
								  select (ZDateTime)cusContainer.JobContainer[containerPropertyToUpdateFrom]).Max();

				if (!latestDate.IsEmpty)
				{
					dateInfoToUpdate.Value = latestDate;
				}
			}
		}

		#endregion
	}
}
