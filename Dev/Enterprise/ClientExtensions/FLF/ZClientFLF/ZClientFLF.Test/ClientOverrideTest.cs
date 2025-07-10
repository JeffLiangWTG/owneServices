using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.FLF;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.ZClientFLF.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : Enterprise.ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestBizOverrides()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals(typeof(FLFForwardingConsol).FullName, typeof(FLFForwardingConsol), consol.GetType());
		}

		public void TestInitialiseAndUnitialise()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			ClientOverride.Instance.Uninitialise();
			AssertEquals(typeof(Statement), Statement.New(GlbBranch.CurrentBranch).GetType());
			AssertEquals(typeof(DocARInvoice), DocARInvoice.New(invoice, Factory).GetType());
			ClientOverride.Instance.Initialise();
			AssertEquals(typeof(FLFStatement), Statement.New(GlbBranch.CurrentBranch).GetType());
			AssertEquals(typeof(DocFLFARInvoice), DocARInvoice.New(invoice, Factory).GetType());
			ClientOverride.Instance.Uninitialise();
			AssertEquals(typeof(Statement), Statement.New(GlbBranch.CurrentBranch).GetType());
			AssertEquals(typeof(DocARInvoice), DocARInvoice.New(invoice, Factory).GetType());
		}

		#region Implementation
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		#region Factory
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}

		BusinessObjectFactory factory;
		#endregion
		#endregion
	}
}
