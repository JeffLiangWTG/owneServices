#if !WINZOR
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ModuleButtonGridForRegistryTest : TestCaseWithFactory
	{
		#region TestAttach

		[RequiresSTA]
		public void TestAttach()
		{
			using (var form = new ZForm(new DummyProxyMaster()))
			{
				var userControl = new RegistryZUserControl();
				var grid = new ModuleButtonGridForRegistry<RecordAttacherForRegistryTest.DummyProxy>();
				grid.ModuleID = DummyModuleIDs.Dummy;
				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(DummyProxyMaster);
				grid.BindToFindBoxList = "FindBoxCollection";
				grid.BindToGridList = "Items";

				// grid must have at least one column
				var calcEditColumn = new ZTextBoxColumnStyleInfo();
				calcEditColumn.ColumnName = "Name";
				grid.ColumnStyles.Add(calcEditColumn);
				userControl.Controls.Add(grid);
				form.Controls.Add(userControl);

				form.Show();

				var toolStrip = (ZToolStrip)grid.Controls.Find("toolStrip", true)[0];
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true)[0];
				AssertNoExceptionThrown(attachButton.PerformClick);
			}
		}

		#endregion

		#region TestDetach

		public void TestDetach()
		{
			var dummyBizO = Factory.New<CargoWise.EntityFramework.Testing.DummyBusinessObject>();
			Factory.Save();

			var master = new DummyProxyMaster();
			master.Items.Add(new RecordAttacherForRegistryTest.DummyProxy { ProxyPK = dummyBizO.PK });

			using (var form = new ZForm(master))
			{
				var userControl = new RegistryZUserControl();
				var grid = new ModuleButtonGridForRegistry<RecordAttacherForRegistryTest.DummyProxy>();
				grid.ModuleID = DummyModuleIDs.Dummy;
				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(DummyProxyMaster);
				grid.BindToFindBoxList = "FindBoxCollection";
				grid.BindToGridList = "Items";

				// grid must have at least one column
				var calcEditColumn = new ZTextBoxColumnStyleInfo();
				calcEditColumn.ColumnName = "Name";
				grid.ColumnStyles.Add(calcEditColumn);
				userControl.Controls.Add(grid);
				var randomControlToFocus = new RegistryZUserControl();
				form.Controls.Add(randomControlToFocus);
				form.Controls.Add(userControl);

				form.Show();

				randomControlToFocus.Focus();
				AssertEquals("Precondition: Control was focused.", true, randomControlToFocus.Focused);

				var toolStrip = (ZToolStrip)grid.Controls.Find("toolStrip", true)[0];
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true)[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(detachButton.PerformClick);
				AssertEquals("Parent of grid should be focused after detach.", userControl, form.ActiveControl);
			}
		}

		#endregion

		#region Implementation

		class DummyProxyMaster : RegistryProxyBusinessObjectMaster<RecordAttacherForRegistryTest.DummyProxy>
		{
			protected override RegistryProxyBusinessObjectCollection<RecordAttacherForRegistryTest.DummyProxy> GetNewCollection()
			{
				return new RecordAttacherForRegistryTest.DummyProxyCollection();
			}

			protected override IBusinessObjectCollection GetNewFindBoxCollection()
			{
				return new DummyBusinessObjectCollection(Factory);
			}

			protected override RegistryProxyBusinessObjectMaster<RecordAttacherForRegistryTest.DummyProxy> GetNewMaster()
			{
				return new DummyProxyMaster();
			}
		}

		#endregion
	}
}
#endif
