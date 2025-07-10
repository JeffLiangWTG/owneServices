using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DataExportBatchPluginController))]
	internal class DataExportBatchPluginControllerBasherTest : ZControllerBasherTest
	{
		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DataExportBatchPlugin;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new NotSupportedException("this is a plugin and deals with different types of business objects");
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;

		#endregion

		public void TestPlugIn()
		{
			var source = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
			using (var form = new ZForm())
			using (var plugins = new PlugIns(source, form))
			{
				plugins.Add(ControllerIDs.PackingPlugIn);
				var plugin = plugins.GetPlugIn(ControllerIDs.PackingPlugIn);
				AssertNotNull(plugin);
			}
		}

		public void TestPlugInTabPageCaption()
		{
			AssertEquals("Data Export Batch Numbers", new DataExportBatchPluginController().PluginTabPageCaption.Caption);
		}

		public override void TestTemplateCopyForm()
		{
			Assert("this is a plugin", true);
		}

		public override void TestViewForm()
		{
			Assert("this is a plugin", true);
		}

		public override void TestNewForm()
		{
			Assert("this is a plugin", true);
		}

		public override void TestDeleteForm()
		{
			Assert("this is a plugin", true);
		}

		public override void TestEditForm()
		{
			Assert("this is a plugin", true);
		}
	}
}
