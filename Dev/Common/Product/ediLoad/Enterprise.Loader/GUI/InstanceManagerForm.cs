using System;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Loader.Common;
using Enterprise.Client.Common;

namespace Enterprise.Loader
{
	partial class InstanceManagerForm : Form
	{
		internal InstanceManagerForm(Configuration configuration, InstanceBindingList list)
		{
			this.configuration = configuration;
			this.list = list;
			InitializeComponent();
			this.Icon = BrandingFactory.Instance.ProductIcon;
			this.dataGridView.SelectionChanged += dataGridView_SelectionChanged;
			this.dataGridView.DataBindingComplete += dataGridView_SelectionChanged;
			this.textBoxName.TextChanged += OnTextChanged;
			this.textBoxServerName.TextChanged += OnTextChanged;
			this.textBoxDatabaseName.TextChanged += OnTextChanged;
			this.dataGridView.DataSource = list;
		}

		void dataGridView_SelectionChanged(object sender, EventArgs e)
		{
			InstanceBindingEntry current = null;
			if (this.dataGridView.CurrentRow != null)
			{
				current = this.dataGridView.CurrentRow.DataBoundItem as InstanceBindingEntry;
			}
			if (current == null)
			{
				current = editingItem?.Entry is NewCargoWiseOneInstanceEntryPlaceholder ? editingItem : new InstanceBindingEntry(new NewCargoWiseOneInstanceEntryPlaceholder());
			}
			if (editingItem != current)
			{
				editingItem = current;
				if (editingItem.Entry is NewCargoWiseOneInstanceEntryPlaceholder)
				{
					itemGroupBox.Text = "Register New Instance";
					this.textBoxName.Enabled = true;
					this.buttonDeleteItem.Enabled = false;
				}
				else
				{
					itemGroupBox.Text = "Edit Instance";
					this.textBoxName.Enabled = false;
					this.buttonDeleteItem.Enabled = true;
				}
				this.textBoxName.Text = editingItem.Name;
				this.textBoxServerName.Text = editingItem.ServerName;
				this.textBoxDatabaseName.Text = editingItem.DatabaseName;
				OnTextChanged(null, EventArgs.Empty);
			}
		}

		void OnTextChanged(object sender, EventArgs e)
		{
			buttonSaveItem.Enabled =
				!string.IsNullOrWhiteSpace(this.textBoxName.Text) &&
				!string.IsNullOrWhiteSpace(this.textBoxServerName.Text) &&
				!string.IsNullOrWhiteSpace(this.textBoxDatabaseName.Text) &&
				(this.textBoxServerName.Text != editingItem.ServerName ||
				 this.textBoxDatabaseName.Text != editingItem.DatabaseName);
		}

		void buttonSaveItem_Click(object sender, EventArgs e)
		{
			if (buttonSaveItem.Enabled)
			{
				try
				{
					if (editingItem.Entry is NewCargoWiseOneInstanceEntryPlaceholder)
					{
						if (!configuration.Services.CargoWiseOneInstanceClass.ExistsInCurrentSchema())
						{
							if (configuration.Services.MessageBox.Show(CargoWiseOneInstanceManager.SchemaModificationWarning + "\r\n\r\nWould you like to continue?", "Warning: Active Directory Schema Modification", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
							{
								return;
							}
							CargoWiseOneInstanceManager.CreateInCurrentSchemaWithLoginIfRequired(configuration);
						}
						list.Add(new InstanceBindingEntry(configuration.Services.CargoWiseOneInstanceClass.AddNewInstance(this.textBoxName.Text, this.textBoxServerName.Text, this.textBoxDatabaseName.Text)));
					}
					else
					{
						using (var entry = editingItem.Entry.GetDirectoryEntry())
						{
							entry.ServerName = textBoxServerName.Text;
							entry.DatabaseName = textBoxDatabaseName.Text;
							entry.CommitChanges();
						}
						this.dataGridView.Refresh();
						OnTextChanged(null, EventArgs.Empty);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					configuration.Services.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		void buttonDeleteItem_Click(object sender, EventArgs e)
		{
			if (configuration.Services.MessageBox.Show("Are you sure you want to delete this instance, any existing shortcuts will stop functioning?", "Delete Instance", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				try
				{
					using (var entry = editingItem.Entry.GetDirectoryEntry())
					{
						entry.Delete();
					}
					list.Remove(editingItem);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					configuration.Services.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		readonly InstanceBindingList list;
		readonly Configuration configuration;
		InstanceBindingEntry editingItem;
	}

	class InstanceBindingList : SortableBindingList<InstanceBindingEntry>
	{
		public InstanceBindingList()
		{
			AllowNew = true;
		}

		protected override object AddNewCore()
		{
			return new InstanceBindingEntry(new NewCargoWiseOneInstanceEntryPlaceholder());
		}
	}

	class InstanceBindingEntry
	{
		public InstanceBindingEntry(ICargoWiseOneInstanceSearchResult entry)
		{
			Entry = entry;
		}

		public string Name => Entry.Name;
		public string DatabaseName => Entry.DatabaseName;
		public string ServerName => Entry.ServerName;
		public ICargoWiseOneInstanceSearchResult Entry { get; }

		public override string ToString() => Entry.Name;
	}

	class NewCargoWiseOneInstanceEntryPlaceholder : ICargoWiseOneInstanceSearchResult
	{
		public string DatabaseName
		{
			get { return ""; }
		}

		public string Name
		{
			get { return ""; }
		}

		public string ServerName
		{
			get { return ""; }
		}

		public int Flags
		{
			get { return 0; }
		}

		public string BlazorUrlAuthority
		{
			get { return ""; }
		}

		public DateTime LastModified => throw new NotImplementedException();

		public Guid Guid => throw new NotImplementedException();

		public ICargoWiseOneInstanceEntry GetDirectoryEntry()
		{
			throw new NotImplementedException();
		}
	}
}
