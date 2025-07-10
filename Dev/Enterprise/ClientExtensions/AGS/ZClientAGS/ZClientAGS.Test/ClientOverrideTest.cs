using System;
using CargoWise.EntityFramework;
using Enterprise.Client.AGS.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Client.ZClientAGS.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInitialiseAndUninitialised()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ForwardingShipment shipment = factory.New<ForwardingShipment>();
			AssertEquals("StmALogValueObjectDataAdapter instance type should be AGSStmALogValueObjectDataAdapter", typeof(AGSStmALogValueObjectDataAdapter), StmALogValueObjectDataAdapter.New(shipment, "").GetType());
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
