using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(JASForwardingShipment))]
	internal class JASForwardingShipmentBOTest : ForwardingShipmentBusinessObjectTest
	{
		#region Metadata
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.JASForwardingShipment);
			}
		}

		#endregion
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JASForwardingShipment>();
		}
	}
}
