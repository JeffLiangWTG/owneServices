using System.Linq;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Loader.Common;

namespace Enterprise.Loader
{
	public partial class ConfigurationDialog : Form
	{
		public ConfigurationDialog(Installation installation)
		{
			this.installation = installation;

			InitializeComponent();

			this.Icon = BrandingFactory.Instance.ProductIcon;
			this.pictureBox1.Image = BrandingFactory.Instance.ProductIcon.ToBitmap();

			this.serverNameTextBox.Text = Configuration.ServerName ?? string.Empty;
			this.databaseNameTextBox.Text = Configuration.DatabaseName ?? string.Empty;

			list = new InstanceBindingList();
			InitializeInstanceList();
		}

		void InitializeInstanceList()
		{
			if (installation.Configuration.Services.CargoWiseOneInstanceClass.ExistsInCurrentSchema())
			{
				foreach (var instance in installation.Configuration.Services.CargoWiseOneInstanceClass.FindAllInstances())
				{
					list.Add(new InstanceBindingEntry(instance));
				}
			}
			RefreshComboBox();
		}

		void RefreshComboBox()
		{
			this.comboBoxInstance.Items.Clear();
			this.comboBoxInstance.Items.AddRange(list.OrderBy(instance => instance.Name).Select(item => item).ToArray());
			this.comboBoxInstance.Items.Add(new ConnectToDatabaseItem());
			this.comboBoxInstance.Items.Add(new ManageInstancesItem());
		}

		void conntectButton_Click(object sender, System.EventArgs e)
		{
			if (this.comboBoxInstance.SelectedItem is ConnectToDatabaseItem)
			{
				Configuration.ServerName = this.serverNameTextBox.Text.Trim();
				Configuration.DatabaseName = this.databaseNameTextBox.Text.Trim();
				if (string.IsNullOrEmpty(Configuration.ServerName) || string.IsNullOrEmpty(Configuration.DatabaseName))
				{
					Configuration.Notifier.ShowError("Server and database name values must be provided");
				}
				else
				{
					Close();
				}
			}
			else
			{
				var instanceItem = this.comboBoxInstance.SelectedItem as InstanceBindingEntry;
				if (instanceItem == null)
				{
					Configuration.Notifier.ShowError("Select a valid instance");
				}
				else
				{
					Configuration.InstanceName = instanceItem.Name;
					Configuration.ServerName = instanceItem.ServerName;
					Configuration.DatabaseName = instanceItem.DatabaseName;
					Close();
				}
			}
		}

		void comboBoxInstance_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.comboBoxInstance.SelectedItem is ConnectToDatabaseItem)
			{
				this.databaseNameLabel.Visible = true;
				this.databaseNameTextBox.Visible = true;
				this.serverNameLabel.Visible = true;
				this.serverNameTextBox.Visible = true;
			}
			else
			{
				this.databaseNameLabel.Visible = false;
				this.databaseNameTextBox.Visible = false;
				this.serverNameLabel.Visible = false;
				this.serverNameTextBox.Visible = false;
			}
			if (this.comboBoxInstance.SelectedItem is ManageInstancesItem)
			{
				using (var form = new InstanceManagerForm(installation.Configuration, list))
				{
					Configuration.Services.MessageBox.ShowDialog(form);
					RefreshComboBox();
				}
			}
		}

		class ConnectToDatabaseItem
		{
			public override string ToString()
			{
				return "Connect to Database...";
			}
		}

		class ManageInstancesItem
		{
			public override string ToString()
			{
				return "Manage Instances...";
			}
		}

		EnterpriseConfiguration Configuration
		{
			get { return ((EnterpriseConfiguration)installation.Configuration); }
		}

		readonly Installation installation;
		readonly InstanceBindingList list;
	}
}
