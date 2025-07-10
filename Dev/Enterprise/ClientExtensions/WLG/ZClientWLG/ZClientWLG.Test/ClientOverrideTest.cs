using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.WLG.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInitialiseAndUninitialise()
		{
			ARInvoice arInvoice = Factory.New<ARInvoice>();
			try
			{
				CliOverride.Initialise();
				AssertEquals("Statement override", typeof(WakoStatement), Statement.New(GlbBranch.CurrentBranch).GetType());
				AssertEquals("DocARInvoce override", typeof(DocWLGARInvoice), DocARInvoice.New(arInvoice, Factory).GetType());
				AssertEquals("InvoicingBaseDocumentSupporter override", typeof(WLGInvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(arInvoice).GetType());
				CliOverride.Uninitialise();
				AssertEquals("Statement override", typeof(Statement), Statement.New(GlbBranch.CurrentBranch).GetType());
				AssertEquals("DocARInvoce override", typeof(DocARInvoice), DocARInvoice.New(arInvoice, Factory).GetType());
				AssertEquals("InvoicingBaseDocumentSupporter override", typeof(InvoicingBaseDocumentSupporter), InvoicingBaseDocumentSupporter.New(arInvoice).GetType());
			}
			finally
			{
				if (!CliOverride.IsInitialised)
				{
					CliOverride.Initialise();
				}
			}
		}

		public void TestClientTypeDeciders()
		{
			ITypeDeciderDictionary clientTypeDeciders = ClientOverride.Instance.ClientTypeDeciders;
			AssertNotNull("ClientTypeDeciders", clientTypeDeciders);
			AssertEquals("ClientTypeDeciders.Count", 1, new List<KeyValuePair<Type, ITypeDecider>>(clientTypeDeciders).Count);
			AssertEquals("ARInvoice should map to WLGARInvoice", typeof(WLGARInvoice), ((TypeDeciderImpl)clientTypeDeciders[typeof(ARInvoice)]).ClientType);
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		readonly ClientOverride CliOverride = ClientOverride.Instance;
		BusinessObjectFactory fFactory;
		protected BusinessObjectFactory Factory
		{
			get
			{
				return (fFactory) ?? (fFactory = new BusinessObjectFactory());
			}
		}
	}
}
