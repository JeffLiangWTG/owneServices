using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsCusContainer : CusContainer
	{
		public WoolworthsCusContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void MarkOrderContainersAsDelivered()
		{
			foreach (OrderLineDeliverContainer container in OrderDeliveryContainers)
			{
				container.J5_QuantityInStore = container.J5_QuantityInvoiced;
			}
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			fValidCO_JE = CO_JE;
		}

		public override void Delete()
		{
			DeleteOrderLineDeliverContainers();
			base.Delete();
		}

		public override void OnLoaded()
		{
			fOriginalContainerNum = CO_ContainerNumber;
			fValidCO_JE = CO_JE;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				fOriginalContainerNum = CO_ContainerNumber;
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!Declaration.JE_IsCancelled && (CO_ContainerNumberInfo.HasChanges || CO_SealInfo.HasChanges))
			{
				SyncOrderLineDeliverContainers();
			}
		}

		#endregion

		#region Property Overrides

		[BusinessObjectTestExclude]
		public override ZGuid CO_JE
		{
			get { return base.CO_JE; }
			set
			{
				base.CO_JE = value;
				if (value.IsValid)
				{
					fValidCO_JE = value;
				}
			}
		}

		public override ZString CO_ContainerNumber
		{
			get { return base.CO_ContainerNumber; }
			set
			{
				if (base.CO_ContainerNumber != value)
				{
					if (!IsInDatabase ||
						((WoolworthsJobDeclaration)this.Declaration).OnUpdatingCusContainerNumber())
					{
						base.CO_ContainerNumber = value;
					}
					CO_ContainerNumberInfo.RefreshBinding();

					if (value.IsEmpty)
					{
						DeleteOrderLineDeliverContainers();
					}
				}
			}
		}

		#endregion

		#region Implementation

		ZString fOriginalContainerNum;
		ZGuid fValidCO_JE;

		void DeleteOrderLineDeliverContainers()
		{
			foreach (OrderLineDeliverContainer container in GetOrderDeliveryContainers(true))
			{
				container.Delete();
			}
		}

		internal OrderLineDeliverContainer[] OrderDeliveryContainers
		{
			get { return GetOrderDeliveryContainers(false); }
		}

		OrderLineDeliverContainer[] GetOrderDeliveryContainers(bool useOriginalContainerNumberValue)
		{
			List<OrderLineDeliverContainer> result = new List<OrderLineDeliverContainer>();
			JobDeclaration declaration = Factory.Load<JobDeclaration>(fValidCO_JE);

			if (declaration != null)
			{
				foreach (WoolworthsJobComInvoiceHeader invoiceHeader in declaration.Invoices)
				{
					foreach (WoolworthsJobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
					{
						foreach (OrderLineDelivery delivery in invoiceLine.OrderLineDeliveries)
						{
							foreach (OrderLineDeliverContainer nextDeliverContainer in delivery.Containers)
							{
								if (nextDeliverContainer.J5_ContainerNum.ToUpper() == (useOriginalContainerNumberValue ? fOriginalContainerNum : CO_ContainerNumber).ToUpper())
								{
									result.Add(nextDeliverContainer);
								}
							}
						}
					}
				}
			}
			return result.ToArray();
		}

		void SyncOrderLineDeliverContainers()
		{
			JobDeclaration declaration = base.Declaration as JobDeclaration;
			foreach (WoolworthsJobComInvoiceHeader invoiceHeader in declaration?.Invoices)
			{
				foreach (WoolworthsJobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
				{
					if (!CO_ContainerNumber.IsEmpty)
					{
						if (invoiceLine.JI_CustomAttrib4 == fOriginalContainerNum)
						{
							invoiceLine.JI_CustomAttrib4 = CO_ContainerNumber;
						}

						foreach (OrderLineDelivery delivery in invoiceLine.OrderLineDeliveries)
						{
							if (delivery.J4_RL_NKDestinationPort.ToUpper() == declaration.JE_RL_NKFinalDestination.ToUpper())
							{
								OrderLineDeliverContainer deliverContainerToUpdate = null;

								foreach (OrderLineDeliverContainer nextDeliverContainer in delivery.Containers)
								{
									if (nextDeliverContainer.J5_ContainerNum.ToUpper() == fOriginalContainerNum.ToUpper() ||
										nextDeliverContainer.J5_ContainerNum.ToUpper() == CO_ContainerNumber.ToUpper())
									{
										deliverContainerToUpdate = nextDeliverContainer;
										break;
									}
								}

								if (deliverContainerToUpdate != null)
								{
									deliverContainerToUpdate.SetContainerKey(CO_ContainerNumber, declaration.JE_VesselName, declaration.JE_VoyageFlightNo);
									deliverContainerToUpdate.J5_ContainerSeal = CO_Seal;
									if (Container != null)
									{
										deliverContainerToUpdate.J5_RC_NKContainerType = this.Container.RC_Code;
									}

									deliverContainerToUpdate.J5_ETA = declaration.JE_DateOfArrival;
									deliverContainerToUpdate.J5_ETD = declaration.JE_DateAtOrigin;
								}
							}
						}
					}
				}
			}
			fOriginalContainerNum = CO_ContainerNumber;
		}

		#endregion
	}
}
