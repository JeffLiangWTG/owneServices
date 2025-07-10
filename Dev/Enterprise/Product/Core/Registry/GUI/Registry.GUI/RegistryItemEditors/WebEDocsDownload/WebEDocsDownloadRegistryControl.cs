using System.ComponentModel;
using CargoWise.Application;
using Enterprise.Integration.Web;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	public partial class WebEDocsDownloadRegistryControl : RegistryZUserControl
	{
		string lastModuleCode;

		public RefDocTypeEntryCollection CurrentDocTypes { get; set; }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WebEDocsDownloadEntryDictionary ModuleDocTypeList
		{
			get
			{
				SaveModule();
				return moduleDocTypeList;
			}
			set
			{
				if (value != null)
				{
					CurrentDocTypes = new RefDocTypeEntryCollection();

					var newDictionary = new WebEDocsDownloadEntryDictionary();
					foreach (var item in value)
					{
						newDictionary.Add(item.Key, item.Value.Clone());
					}
					moduleDocTypeList = newDictionary;
				}

				if (ModulesComboBox != null)
				{
					ModulesComboBox.SelectedIndex = 0;
				}
			}
		}
		WebEDocsDownloadEntryDictionary moduleDocTypeList;

		IWebEDocsDownloadModulesList WebEDocsDownloadModulesList
		{
			get
			{
				return webEDocsDownloadModulesList ?? (webEDocsDownloadModulesList = ObjectFactory.Get<IWebEDocsDownloadModulesList>());
			}
		}
		IWebEDocsDownloadModulesList webEDocsDownloadModulesList;

		public WebEDocsDownloadRegistryControl()
		{
			InitializeComponent();
			CurrentDocTypes = new RefDocTypeEntryCollection();
			InitializeThemeDropEdit();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
			ModulesComboBox.Enabled = !readOnly;
			AllowAllDocTypesCheckBox.ReadOnly = readOnly;
		}

		void InitializeThemeDropEdit()
		{
			var collection = (CodeDescriptionPairList)WebEDocsDownloadModulesList.ModulesCodeDescriptionPairList;
			collection.SortByDescription();
			collection.Insert(0, WebEDocsDownloadModulesList.AllModules);

			ModulesComboBox.Items.Clear();
			ModulesComboBox.Items.AddRange(collection.ToArray());
		}

		void ModulesComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SaveModule();
			UpdateDocTypes();
		}

		void SaveModule()
		{
			if (!string.IsNullOrEmpty(lastModuleCode))
			{
				moduleDocTypeList[lastModuleCode].UpdateValues(AllowAllDocTypesCheckBox.Checked, (RefDocTypeEntryCollection)this.Grid.List);
			}

			var selectedItem = ModulesComboBox.SelectedItem as CodeDescriptionPair;
			if (selectedItem != null)
			{
				if (!moduleDocTypeList.ContainsKey(selectedItem.Code))
				{
					moduleDocTypeList.Add(selectedItem.Code, new WebEDocsDownloadEntry());
				}

				CurrentDocTypes.RemoveAll();
				CurrentDocTypes.AddRange(moduleDocTypeList[selectedItem.Code].DocTypeCollection);
				AllowAllDocTypesCheckBox.Checked = moduleDocTypeList[selectedItem.Code].AllowAllDocTypes;

				lastModuleCode = selectedItem.Code;
			}
		}

		void UpdateDocTypes()
		{
			this.Grid.SetDataBinding(this, "CurrentDocTypes");
		}
	}
}
