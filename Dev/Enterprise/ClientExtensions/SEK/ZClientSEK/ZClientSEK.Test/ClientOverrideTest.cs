using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.SEK;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.ZClientSEK.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInitialiseAndUnitialise()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			var awbHeader = factory.NewWithValidTestData<ShipmentExportAWBHeader>();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			PrintStatement statement = new PrintStatement(factory, GlbBranch.CurrentBranch);
			ClientOverride.Instance.Uninitialise();
			AssertEquals("DocAWB wraper type ", typeof(DocAWB), DocAWB.New(awbHeader, factory).GetType());
			ClientOverride.Instance.Initialise();
			AssertEquals("DocAWB wraper type ", typeof(SEKDocAWB), DocAWB.New(awbHeader, factory).GetType());
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
