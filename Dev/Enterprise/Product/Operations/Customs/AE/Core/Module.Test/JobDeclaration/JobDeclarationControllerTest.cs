using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(JobDeclarationController))]
class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
{
	public override Type ControllerToBashType
	{
		get
		{
			return typeof(JobDeclarationController);
		}
	}

	public void TestBrokeragePlugIn()
	{
		var controller = new JobDeclarationControllerForTesting();
		using (var plugin = controller.GetPlugIn(Factory.New<ForwardingShipment>()))
		{
			AssertType<BrokeragePlugIn>("GetPlugIn should return an object of AE CustomsBrokerageUserControl.", plugin);
		}
	}
}

class JobDeclarationControllerForTesting : JobDeclarationController
{
	public new ZPlugIn GetPlugIn(IBusiness businessEntity)
	{
		return base.GetPlugIn(businessEntity);
	}
}
