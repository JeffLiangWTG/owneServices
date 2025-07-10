using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	class WoolworthsReportUserControlTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateOrderStatusesForReport_CalledOnIndentVendorOrderReports()
		{
			using (var module = (WoolworthsOrdersReportModuleOverride)ZModuleFactory.Instance.Create(ModuleIDs.OrdersReport))
			{
				var documentsXmlFile = BaseSourcePath + @"Enterprise\ClientExtensions\WOW\Documents\WOWDocuments.xml";
				AssertEquals("Wow documents file exists", true, File.Exists(documentsXmlFile));
				var task = new DbUpgrader.Data.ClientDocumentsUpgradeTask(documentsXmlFile);
				task.Run();
				TestCaseHelper.RunClientDbCreateScripts();
				var factory = new BusinessObjectFactory();
				var userControl = (WoolworthsReportUserControl)module.EmbeddedControl;
				var indentOrderCommand = LoadReportCommandByMenuNameContains(factory, "indent");
				AssertNotNull("Could not find the indent order for test!", indentOrderCommand);
				using (var printSet = new ReportPrintSet(indentOrderCommand))
				{
					WoolworthsOrder.WasUpdateOrderStatusesForReportCalled = false;
					try
					{
						userControl.PrintReportSet(printSet, new EmptyDeliveryInstructions());
						AssertEquals("The order status should be updated for the indent order report", true, WoolworthsOrder.WasUpdateOrderStatusesForReportCalled);
					}
					finally
					{
						WoolworthsOrder.WasUpdateOrderStatusesForReportCalled = false;
					}
				}
			}
		}

		[Serializable]
		protected class EmptyDeliveryInstructions : DeliveryInstructions
		{
			public EmptyDeliveryInstructions()
			{
				Destination = DeliveryInstructionDestination.None;
			}
		}

		ReportCommand LoadReportCommandByMenuNameContains(BusinessObjectFactory factory, string partCommandName)
		{
			var collection = new ReportCommandCollection(factory, "RepOrdersReport");
			collection.Load();
			ReportCommand result = null;
			foreach (ReportCommand command in collection)
			{
				if (command.SU_IsClientSpecific && command.SU_MenuName.ToLower().IndexOf(partCommandName) != -1)
				{
					result = command;
					break;
				}
			}

			return result;
		}
	}
}
