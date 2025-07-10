using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestCoreInitialisers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceLine invoiceline = Factory.New<JobComInvoiceLine>();
			CusContainer container = Factory.New<CusContainer>();
			DeliveryInstructions delvInstr = new DeliveryInstructions();
			Order order = Factory.New<Order>();
			if (ClientOverride.Instance.IsInitialised)
			{
				ClientOverride.Instance.Uninitialise();
			}

			AssertEquals(typeof(DocDeclaration), DocDeclaration.New(declaration, Factory).GetType());
			AssertEquals(typeof(DocJobComInvoiceLine), DocJobComInvoiceLine.New(invoiceline, Factory).GetType());
			AssertEquals(typeof(MessageSendingValidation), MessageSendingValidation.New(null, null).GetType());
			AssertEquals(typeof(DocCusContainer), DocCusContainer.New(container, Factory).GetType());
			AssertEquals(typeof(DeliveryInstructionsHelper), DeliveryInstructionsHelper.New(delvInstr).GetType());
			AssertEquals(typeof(DocOrder), DocOrder.New(order, Factory).GetType());
			ClientOverride.Instance.Initialise();
			AssertEquals(typeof(WowDocDeclaration), DocDeclaration.New(declaration, Factory).GetType());
			AssertEquals(typeof(WowDocJobComInvoiceLine), DocJobComInvoiceLine.New(invoiceline, Factory).GetType());
			AssertEquals(typeof(WowMessageSendingValidation), MessageSendingValidation.New(null, null).GetType());
			AssertEquals(typeof(WowDocCusContainer), DocCusContainer.New(container, Factory).GetType());
			AssertEquals(typeof(WowDeliveryInstructionsHelper), DeliveryInstructionsHelper.New(delvInstr).GetType());
			AssertEquals(typeof(WowDocOrder), DocOrder.New(order, Factory).GetType());
		}

		public void TestBizOverrides()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			AssertEquals(typeof(WoolworthsProduct).FullName, typeof(WoolworthsProduct), part.GetType());
			Order order = Factory.New<Order>();
			AssertEquals(typeof(WoolworthsOrder).FullName, typeof(WoolworthsOrder), order.GetType());
			OrderLine line = Factory.New<OrderLine>();
			AssertEquals(typeof(WoolworthsOrderLine).FullName, typeof(WoolworthsOrderLine), line.GetType());
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory());
			}
		}

		BusinessObjectFactory factory;
	}
}
