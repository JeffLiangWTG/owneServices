#region Testing
#if DEBUG

using System;
using DataTransfer.Common.GUI.MenuItems;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Common.GUI.MenuItems
{
	public class ExportToXmlMenuItemTest : TransactionedTestCase
	{
		public void TestVerboseHandler()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			EventHandler handler = (s, e) =>
							{
								var value = SystemDataRegistry.Instance.SimpleXMLExportFormat.Value;
								AssertEquals(false, value);
							};
			var testHandler = ExportToXmlMenuItem.VerboseHandler(handler);
			testHandler(null, null);
			AssertEquals(true, SystemDataRegistry.Instance.SimpleXMLExportFormat.Value);
		}

		public void TestLightWeightHandler()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			EventHandler handler = (s, e) =>
			{
				var value = SystemDataRegistry.Instance.SimpleXMLExportFormat.Value;
				AssertEquals(true, value);
			};
			var testHandler = ExportToXmlMenuItem.LightWeighHandler(handler);
			testHandler(null, null);
			AssertEquals(false, SystemDataRegistry.Instance.SimpleXMLExportFormat.Value);
		}
	}
}

#endif
#endregion