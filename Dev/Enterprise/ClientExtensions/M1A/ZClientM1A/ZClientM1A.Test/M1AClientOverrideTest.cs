using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.M1A;
using Enterprise.Client.M1A.Business;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ZClientM1A.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class M1AClientOverrideTest : ClientOverrideTest
	{
		public void TestInitialiseAndUnitialise()
		{
			InvoicingBase invoice = Factory.New<ARInvoice>();
			if (ClientOverride.Instance.IsInitialised)
			{
				ClientOverride.Instance.Uninitialise();
			}

			AssertEquals("Testing overrides", typeof(DocARInvoice), DocARInvoice.New(invoice, Factory).GetType());
			AssertEquals("Testing overrides", typeof(InvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(invoice).GetType());
			ClientOverride.Instance.Initialise();
			AssertEquals("Testing overrides", typeof(M1ADocARInvoice), DocARInvoice.New(invoice, Factory).GetType());
			AssertEquals("Testing overrides", typeof(M1AInvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(invoice).GetType());
		}

		public void TestDbSchemaUpgradeInfo()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertNotNull(overrideForClient.DbSchemaExtensionObjects);
			AssertEquals(typeof(M1AClientDbSchemaUpgradeInfo), overrideForClient.DbSchemaExtensionObjects.GetType());
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
				return (factory) ?? (factory = new BusinessObjectFactory());
			}
		}

		BusinessObjectFactory factory;
	}
}
