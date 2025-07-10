using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.Rohlig;
using Enterprise.Client.Rohlig.DocWrappers;
using Enterprise.Client.Rohlig.GUI;
using Enterprise.Client.Rohlig.Module;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		public void TestModuleOverride()
		{
			ModuleOverrides moduleOverrides = ClientOverride.Instance.ModuleOverrides;
			AssertNotNull("Client Module Override should not be null", moduleOverrides);
			AssertNotNull("Module Override should contain APTransaction", moduleOverrides[ModuleIDs.APTransaction, Env.CurrentCompany.Country.Code]);
			AssertEquals("Module Override should be BellinModule", typeof(BellinModule).FullName, moduleOverrides[ModuleIDs.APTransaction, Env.CurrentCompany.Country.Code].TypePath.Split(',')[0]);
		}

		public void TestClientTypeDeciders()
		{
			ITypeDeciderDictionary typeDeciders = ClientOverride.Instance.ClientTypeDeciders;
			AssertNotNull("ClientTypeDeciders should not be null", typeDeciders);
			AssertEquals("ClientTypeDeciders count", 2, new List<KeyValuePair<Type, ITypeDecider>>(typeDeciders).Count);
			TypeDeciderImpl typeDeciderImpl = (TypeDeciderImpl)typeDeciders[typeof(ForwardingShipment)];
			AssertNotNull("TypeDeciderImpl should not be null", typeDeciderImpl);
			AssertEquals("Client Type", typeof(RohForwardingShipment), typeDeciderImpl.ClientType);
			typeDeciderImpl = (TypeDeciderImpl)typeDeciders[typeof(JobDeclaration)];
			AssertNotNull("TypeDeciderImpl should not be null", typeDeciderImpl);
			AssertEquals("Client Type", typeof(RohJobDeclaration), typeDeciderImpl.ClientType);
		}

		public void TestInitialiseAndUninitialise()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ClientOverride.Instance.Uninitialise();
			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("Menu should be unintialised; normal action data menu item", typeof(EDIMenu), menu.GetType());
			}

			AssertEquals("Testing overrides", typeof(DocARInvoice), DocARInvoice.New(invoice, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocForwardingShipment), DocForwardingShipment.New(shipment, Factory).GetType());
			ClientOverride.Instance.Initialise();
			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("Menu should be initialised; return ROH action data menu item", typeof(ROHMenuItem), menu.GetType());
			}

			AssertEquals("Testing overrides", typeof(DocROHARInvoice), DocARInvoice.New(invoice, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocROHForwardingShipment), DocForwardingShipment.New(shipment, Factory).GetType());
			ClientOverride.Instance.Uninitialise();
			using (EDIMenu menu = EDIMenu.New())
			{
				AssertEquals("Menu should be unintialised; should be back to normal action data menu item", typeof(EDIMenu), menu.GetType());
			}

			AssertEquals("Testing overrides", typeof(DocARInvoice), DocARInvoice.New(invoice, Factory).GetType());
			AssertEquals("Testing overrides", typeof(DocForwardingShipment), DocForwardingShipment.New(shipment, Factory).GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory = new BusinessObjectFactory();
		}

		BusinessObjectFactory Factory;
	}
}
