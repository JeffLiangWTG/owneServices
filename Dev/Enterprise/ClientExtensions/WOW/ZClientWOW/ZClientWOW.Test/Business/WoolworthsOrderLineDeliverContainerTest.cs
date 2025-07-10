using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrderLineDeliverContainer))]
	public class WoolworthsOrderLineDeliverContainerTest : InvoiceOrderLinkTestCase
	{
		#region Related Business Objects
		public void TestDeclarationProperty()
		{
			AssertEquals("Declaration property returns declaration", fDeclaration.PK, fDeliverContainer.Declaration.PK);
		}

		public void TestDeclarationProperty_WhenDeclarationInactive()
		{
			TestWoolworthsOrderLineDeliverContainer deliveryContainer = Factory.Load<TestWoolworthsOrderLineDeliverContainer>(fDeliverContainer.PK);
			AssertEquals("Should have a link to the invoice line initially", fDeclaration.PK, deliveryContainer.Declaration.PK);
			fDeclaration.JE_IsCancelled = true;
			AssertNotNull("Should not a link now that the declaration has been made inactive", deliveryContainer);
		}

		public void TestDeclarationProperty_WhenNoOrderLineDelivery()
		{
			fDeliverContainer.J5_J4 = ZGuid.Empty;
			AssertNull("No declaration is available if there is no OrderLineDelivery, for example when removing relationships", fDeliverContainer.Declaration);
		}

		public void TestRelatedContainerIsTypeDecided()
		{
			Type relatedContainersType = Container.RelatedContainers.AddNew().GetType();
			AssertEquals("Related containers should be type decided correctly otherwise we lose critical wow functionality", typeof(WoolworthsOrderLineDeliverContainer), relatedContainersType);
		}

		#endregion
		#region Property Overrides
		public void TestQuantityInvoicedCalculatedFromPackCount()
		{
			Container.OrderLineDelivery.OrderLine.JO_OuterPacks = 2;
			Container.J5_PackCount = 10;
			AssertEquals("J5_QuantityInvoiced calculated when PackCount changes", 20m, Container.J5_QuantityInvoiced);
			Container.OrderLineDelivery.OrderLine.JO_OuterPacks = 4;
			AssertEquals("J5_QuantityInvoiced calculated when OuterPacks changes", 40m, Container.J5_QuantityInvoiced);
		}

		public void TestJ5_MasterBill_MandatoryValidation()
		{
			Container.J5_MasterBill = "MasterBill";
			AssertNoErrors(Container.J5_MasterBillInfo);
			Container.J5_MasterBill = "";
			AssertHasError(Container.J5_MasterBillInfo, "Please enter a " + Container.J5_MasterBillInfo.Description + ".");
		}

		#endregion
		#region Bindable Declaration Event Dates
		public void TestLastContainerFromWarfToDepot()
		{
			TestJobContainerDate(JobContainerSchema.JC_FCLWharfGateOut.Name, fDeliverContainer.LastContainerFromWarfToDepotInfo);
		}

		public void TestLastContainerUnpack()
		{
			TestJobContainerDate(JobContainerSchema.JC_ArrivalCartageAdvised.Name, fDeliverContainer.LastContainerUnpackInfo);
		}

		public void TestLastDeliveredToWarehouse()
		{
			TestJobContainerDate(JobContainerSchema.JC_ArrivalCartageComplete.Name, fDeliverContainer.LastDeliveredToWarehouseInfo);
		}

		public void TestLastDeliveredToWarehouseReference()
		{
			WoolworthsOrderLineDeliverContainer wowContainer = fDeliverContainer;
			AssertEquals("Cartage Ref is  empty", true, wowContainer.LastDeliveredToWarehouseReference.IsEmpty);
			AssertEquals("no of containers ", 1, wowContainer.Declaration.CusContainers.Count);
			wowContainer.Declaration.CusContainers[0].JobContainer.JC_ArrivalCartageRef = "FTL";
			AssertEquals("Log description populated", "FTL", wowContainer.LastDeliveredToWarehouseReference);
		}

		#endregion
		#region Test Classes
		class TestWoolworthsOrderLineDeliverContainer : WoolworthsOrderLineDeliverContainer
		{
			public TestWoolworthsOrderLineDeliverContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new JobDeclaration Declaration
			{
				get
				{
					return base.Declaration;
				}
			}
		}

		#endregion
		#region Implementation
		OrderLineDeliverContainer Container
		{
			get
			{
				if (container == null)
				{
					container = (OrderLineDeliverContainer)GetNewBusinessObject();
				}

				return container;
			}
		}

		OrderLineDeliverContainer container;
		protected override BusinessObject GetNewBusinessObject()
		{
			Order order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			OrderLineDeliverContainer deliverContainer = delivery.Containers.AddNew();
			deliverContainer.J5_ContainerNum = "TLU2182821";
			return deliverContainer;
		}

		protected void TestJobContainerDate(ZString fieldName, ZPropertyInfo dateAccessorProperty)
		{
			WoolworthsOrderLineDeliverContainer wowContainer = fDeliverContainer;
			AssertEquals("Date empty at first", true, dateAccessorProperty.Value.IsEmpty);
			wowContainer.Declaration.CusContainers[0].JobContainer[fieldName] = ZDateTime.Now;
			AssertEquals("Date populated", false, dateAccessorProperty.Value.IsEmpty);
		}
		#endregion
	}
}
