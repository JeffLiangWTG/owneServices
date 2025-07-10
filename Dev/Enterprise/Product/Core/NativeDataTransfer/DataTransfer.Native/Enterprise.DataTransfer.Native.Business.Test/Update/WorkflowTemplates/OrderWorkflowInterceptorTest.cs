using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.WorkflowTemplates
{
	public class OrderWorkflowInterceptorTest : TransactionedTestCase
	{
		public void TestInvoke_CreateWorkflowTemplate()
		{
			var buyerPk = TestUtil.PrepareOrgHeaderTableData();
			var buyerAddressPk = TestUtil.PrepareOrgAddressTableData(buyerPk);
			var orderPk = TestUtil.PrepareJobOrderHeaderData(buyerAddressPk);

			// Change date and apply template
			var entity = PrepareOrderEntity(orderPk, buyerAddressPk);
			entity.Action = EntityAction.MERGE;
			entity["OrderNumber"] = "O0002";
			var entitySet = new EntitySet("Order");
			entitySet.Root = entity;

			context.Update(entitySet);

			var order = factory.Load<Order>(orderPk);
			AssertNotEquals("Milestone should be added to order", 0, order.WorkflowItems.Count);
		}

		public void TestInvoke_FindEventLogs()
		{
			var startTime = DateTime.UtcNow;

			Thread.Sleep(100);

			var buyerPk = TestUtil.PrepareOrgHeaderTableData();
			var buyerAddressPk = TestUtil.PrepareOrgAddressTableData(buyerPk);
			var orderPk = TestUtil.PrepareJobOrderHeaderData(buyerAddressPk);

			// Change date and apply template
			var entity = PrepareOrderEntity(orderPk, buyerAddressPk);
			entity.Action = EntityAction.INSERT;
			entity["OrderNumber"] = "O***";
			var entitySet = new EntitySet("Order");
			entitySet.Root = entity;

			context.Update(entitySet);

			Thread.Sleep(100);

			var logs = interceptor.FindEventLogs(entitySet.Root.InternalPK, startTime);
			AssertEquals(0, logs.Count());
		}

		IEntity PrepareOrderEntity(Guid orderPk, Guid buyerAddressPk)
		{
			var buyerAddressDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader.BuyerAddress");
			var buyerAddress = new Entity(buyerAddressDefinition, sessionServices);
			buyerAddress.InternalPK = buyerAddressPk;
			var orderDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader");
			var order = new Entity(orderDefinition, sessionServices);
			order.InternalPK = orderPk;
			order.ParentCollection.Add(buyerAddress);
			return order;
		}

		protected override void SetUp()
		{
			base.SetUp();
			sessionServices = new AncillaryImportServices();
			factory = new BusinessObjectFactory();
			context = new UpdateContext(sessionServices, new FactoryProvider());

			var setting = new OrderWorkflowTemplateSetting();
			setting.Enable = true;
			setting.Context = context;

			interceptor = new OrderWorkflowTemplateInterceptor(setting, sessionServices);
			setting.Interceptor = interceptor;

			context.InterceptorSettings.Add(setting);
		}

		AncillaryImportServices sessionServices;
		OrderWorkflowTemplateInterceptor interceptor;
		BusinessObjectFactory factory;
		IUpdateContext context;
	}
}
