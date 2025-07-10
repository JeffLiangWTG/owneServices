using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.Rohlig.Testing
{
	public class RohForwardingShipmentDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetMenuTemplateFilterValue()
		{
			RohForwardingShipment shipment = Factory.New<RohForwardingShipment>();
			Assembly assembly = Assembly.Load("DocumentWrappers");
			Type docShipmentType = assembly.GetType("Enterprise.DocumentWrappers.DocForwardingShipment");
			MethodInfo newMethod = docShipmentType.GetMethod("New", new Type[] { typeof(ForwardingShipment), typeof(BusinessObjectFactory) });
			DocumentWrapper docWrapper = (DocumentWrapper)newMethod.Invoke(null, new object[] { shipment, Factory });
			RohForwardingShipmentDocumentSupporter documentSupporter = (RohForwardingShipmentDocumentSupporter)shipment.DocumentSupporter;
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Import Cartage Advice With Receipt";
			documentSupporter.CurrentCommand = menuItem;
			AssertEquals("Print Standard", ZBool.False.ToString(), documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandard, docWrapper));
		}
	}
}
