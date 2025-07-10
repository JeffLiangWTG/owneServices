using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Loader.Common;
using Enterprise.Client.Common;
using Moq;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	class InstanceManagerFormTest : TestCase
	{
		public void TestNewSystem()
		{
			var list = new InstanceBindingList();
			using (var form = new InstanceManagerForm(config, list))
			{
				form.Show();
				AssertEquals("Register New Instance", form.Controls["itemGroupBox"].Text);
				AssertEquals("", form.Controls["itemGroupBox"].Controls["textBoxName"].Text);
				AssertEquals("", form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text);
				AssertEquals("", form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text);
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["textBoxName"].Enabled);
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonDeleteItem"].Enabled);

				form.Controls["itemGroupBox"].Controls["textBoxName"].Text = "theone";
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "aserver";
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text = "thedatabase";
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "";
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "theserver";
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);

				Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
				Mock.Get(services.MessageBox).Setup(m => m.Show(CargoWiseOneInstanceManager.SchemaModificationWarning + "\r\n\r\nWould you like to continue?", "Warning: Active Directory Schema Modification", MessageBoxButtons.YesNo, MessageBoxIcon.Question)).Returns(DialogResult.No);
				((Button)form.Controls["itemGroupBox"].Controls["buttonSaveItem"]).PerformClick();
				mocker.VerifyAll();

				Mock.Get(services.MessageBox).Setup(m => m.Show(CargoWiseOneInstanceManager.SchemaModificationWarning + "\r\n\r\nWould you like to continue?", "Warning: Active Directory Schema Modification", MessageBoxButtons.YesNo, MessageBoxIcon.Question)).Returns(DialogResult.Yes);
				Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema());
				var newItem = new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict);
				newItem.SetupGet(item => item.Name).Returns("theone");
				newItem.As<ICargoWiseOneInstanceSearchResult>().SetupGet(item => item.ServerName).Returns("theserver");
				newItem.As<ICargoWiseOneInstanceSearchResult>().SetupGet(item => item.DatabaseName).Returns("thedatabase");
				Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.AddNewInstance("theone", "theserver", "thedatabase", null)).Returns(newItem.Object);
				((Button)form.Controls["itemGroupBox"].Controls["buttonSaveItem"]).PerformClick();
				mocker.VerifyAll();
				AssertEquals(1, list.Count);
				AssertEquals(newItem.Object, list[0].Entry);
			}
		}

		public void TestEditExisting()
		{
			var existingItem = new Mock<ICargoWiseOneInstanceSearchResult>();
			existingItem.SetupGet(item => item.Name).Returns("existing");
			existingItem.SetupGet(item => item.ServerName).Returns("oldserver");
			existingItem.SetupGet(item => item.DatabaseName).Returns("olddatabase");
			var list = new InstanceBindingList();
			list.Add(new InstanceBindingEntry(existingItem.Object));
			using (var form = new InstanceManagerForm(config, list))
			{
				form.Show();
				AssertEquals("Edit Instance", form.Controls["itemGroupBox"].Text);
				AssertEquals("existing", form.Controls["itemGroupBox"].Controls["textBoxName"].Text);
				AssertEquals("oldserver", form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text);
				AssertEquals("olddatabase", form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text);
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["textBoxName"].Enabled);
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonDeleteItem"].Enabled);

				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "aserver";
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text = "";
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text = "adatabase";
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "";
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "oldserver";
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text = "olddatabase";
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text = "newdatabase";
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "newserver";
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);

				var entry = new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict);
				existingItem.Setup(x => x.GetDirectoryEntry()).Returns(entry.Object);
				entry.SetupSet(x => x.ServerName = "newserver");
				entry.SetupSet(x => x.DatabaseName = "newdatabase");
				entry.Setup(x => x.CommitChanges());
				entry.Setup(x => x.Dispose());
				existingItem.SetupGet(item => item.Name).Returns("existing");
				existingItem.SetupGet(item => item.ServerName).Returns("newserver");
				existingItem.SetupGet(item => item.DatabaseName).Returns("newdatabase");
				((Button)form.Controls["itemGroupBox"].Controls["buttonSaveItem"]).PerformClick();
				mocker.VerifyAll();
				AssertEquals(false, form.Controls["itemGroupBox"].Controls["buttonSaveItem"].Enabled);
			}
		}

		public void TestNewWithExisting()
		{
			var existingItem = new Mock<ICargoWiseOneInstanceSearchResult>();
			existingItem.SetupGet(item => item.Name).Returns("existing");
			existingItem.SetupGet(item => item.ServerName).Returns("oldserver");
			existingItem.SetupGet(item => item.DatabaseName).Returns("olddatabase");

			var list = new InstanceBindingList();
			list.Add(new InstanceBindingEntry(existingItem.Object));
			using (var form = new InstanceManagerForm(config, list))
			{
				form.Show();
				((DataGridView)form.Controls["dataGridView"]).CurrentCell = ((DataGridView)form.Controls["dataGridView"])[0, 1];
				((DataGridView)form.Controls["dataGridView"]).Rows[0].Selected = false;
				((DataGridView)form.Controls["dataGridView"]).Rows[1].Selected = true;
				AssertEquals("Register New Instance", form.Controls["itemGroupBox"].Text);

				AssertEquals("", form.Controls["itemGroupBox"].Controls["textBoxName"].Text);
				AssertEquals("", form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text);
				AssertEquals("", form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text);
				form.Controls["itemGroupBox"].Controls["textBoxName"].Text = "new";
				form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "newserver";
				form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text = "newdatabase";

				Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
				var newItem = new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict);
				newItem.SetupGet(item => item.Name).Returns("new");
				newItem.SetupGet(item => item.ServerName).Returns("newserver");
				newItem.SetupGet(item => item.DatabaseName).Returns("newdatabase");
				Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.AddNewInstance("new", "newserver", "newdatabase", null)).Returns(newItem.Object);
				existingItem.SetupGet(item => item.Name).Returns("existing");
				existingItem.SetupGet(item => item.ServerName).Returns("oldserver");
				existingItem.SetupGet(item => item.DatabaseName).Returns("olddatabase");

				((Button)form.Controls["itemGroupBox"].Controls["buttonSaveItem"]).PerformClick();
				mocker.VerifyAll();
				AssertEquals(2, list.Count);
				AssertEquals(existingItem.Object, list[0].Entry);
				AssertEquals(newItem.Object, list[1].Entry);
			}
		}

		public void TestDeleteExisting()
		{
			var existingItem = new Mock<ICargoWiseOneInstanceSearchResult>();
			existingItem.SetupGet(item => item.Name).Returns("existing");
			existingItem.SetupGet(item => item.ServerName).Returns("oldserver");
			existingItem.SetupGet(item => item.DatabaseName).Returns("olddatabase");
			var list = new InstanceBindingList();
			list.Add(new InstanceBindingEntry(existingItem.Object));
			using (var form = new InstanceManagerForm(config, list))
			{
				form.Show();
				AssertEquals("Edit Instance", form.Controls["itemGroupBox"].Text);
				AssertEquals(true, form.Controls["itemGroupBox"].Controls["buttonDeleteItem"].Enabled);

				Mock.Get(services.MessageBox).Setup(m => m.Show("Are you sure you want to delete this instance, any existing shortcuts will stop functioning?", "Delete Instance", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)).Returns(DialogResult.No);

				((Button)form.Controls["itemGroupBox"].Controls["buttonDeleteItem"]).PerformClick();
				mocker.VerifyAll();
				AssertEquals(1, list.Count);

				Mock.Get(services.MessageBox).Setup(m => m.Show("Are you sure you want to delete this instance, any existing shortcuts will stop functioning?", "Delete Instance", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)).Returns(DialogResult.Yes);
				var mockEntry = new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict);
				existingItem.Setup(x => x.GetDirectoryEntry()).Returns(mockEntry.Object);
				mockEntry.Setup(x => x.Delete());
				mockEntry.Setup(x => x.Dispose());
				((Button)form.Controls["itemGroupBox"].Controls["buttonDeleteItem"]).PerformClick();
				mocker.VerifyAll();

				AssertEquals(0, list.Count);
				AssertEquals("Register New Instance", form.Controls["itemGroupBox"].Text);
			}
		}

		public void TestGridFieldsBinding()
		{
			var existingItem = new Mock<ICargoWiseOneInstanceSearchResult>();
			existingItem.SetupGet(item => item.Name).Returns("thename");
			existingItem.SetupGet(item => item.ServerName).Returns("theserver");
			existingItem.SetupGet(item => item.DatabaseName).Returns("thedatabase");
			var list = new InstanceBindingList();
			list.Add(new InstanceBindingEntry(existingItem.Object));
			using (var form = new InstanceManagerForm(config, list))
			{
				form.Show();
				var dataGridView = form.Controls["dataGridView"] as DataGridView;
				AssertEquals("thename", dataGridView[0, 0].Value);
				AssertEquals("theserver", dataGridView[1, 0].Value);
				AssertEquals("thedatabase", dataGridView[2, 0].Value);
			}
		}

		public void TestGridSorting()
		{
			var list = new InstanceBindingList();
			list.Add(CreateBindingEntry("abc", "abc", "abc"));
			list.Add(CreateBindingEntry("xyz", "xyz", "xyz"));
			list.Add(CreateBindingEntry("lmn", "lmn", "lmn"));

			DataGridView dataGridView;

			void TestSort(string columnName, Func<InstanceBindingEntry, string> sortFunction, bool ascending)
			{
				dataGridView.Columns[columnName].HeaderCell.AccessibilityObject.DoDefaultAction();
				var sortedItems = (ascending ? list.OrderBy(sortFunction) : list.OrderByDescending(sortFunction)).ToArray();

				for (int i = 0; i < sortedItems.Length; i++)
				{
					var boundItem = dataGridView.Rows[i].DataBoundItem;
					AssertEquals("Grid items should be ordered correctly", sortedItems[i], boundItem);
				}
			}

			using (var form = new InstanceManagerForm(config, list))
			{
				form.Show();
				dataGridView = form.Controls["dataGridView"] as DataGridView;
				TestSort("nameDataGridViewTextBoxColumn", item => item.Name, true);
			}
		}

		InstanceBindingEntry CreateBindingEntry(string name, string serverName, string databaseName)
		{
			var item = new Mock<ICargoWiseOneInstanceSearchResult>();
			item.SetupGet(i => i.Name).Returns(name);
			item.SetupGet(i => i.ServerName).Returns(serverName);
			item.SetupGet(i => i.DatabaseName).Returns(databaseName);
			return new InstanceBindingEntry(item.Object);
		}

		protected override void SetUp()
		{
			mocker = new MockRepository(MockBehavior.Loose);
			services = new MockServiceContainer(mocker)
			{
				MessageBox = mocker.Create<IMessageBoxProxy>(MockBehavior.Strict).Object,
				CargoWiseOneInstanceClass = new Mock<ICargoWiseOneInstanceClass>(MockBehavior.Strict).Object
			};
			config = new EnterpriseConfiguration { Services = services };
			base.SetUp();
		}

		MockRepository mocker;
		MockServiceContainer services;
		EnterpriseConfiguration config;
	}
}
