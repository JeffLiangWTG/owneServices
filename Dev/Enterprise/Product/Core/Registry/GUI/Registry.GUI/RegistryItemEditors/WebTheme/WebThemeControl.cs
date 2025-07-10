using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class WebThemeControl : RegistryZUserControl
	{
		bool IsFirstLoad = true;

#if DEBUG
		public WebThemeControl()
		{
			CaptionRenderingEnabled = true;
		}
#endif

		public WebThemeControl(IRegistryItem registryItem)
		{
			InitializeComponent();
			RegistryItem = registryItem;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public WebTrackerTheme[] Value
		{
			get
			{
				SaveTheme();
				return ValueList.Where(x => !String.IsNullOrEmpty(x.Code)).ToArray();
			}
			set
			{
				LastUrl = null;
				ValueList = value.ToList();
				InitializeThemeDropEdit();
				InitializeUrlComboBox();
			}
		}
		List<WebTrackerTheme> ValueList;

		public IRegistryItem RegistryItem
		{
			get { return registryItem; }
			set { registryItem = value; }
		}

		IRegistryItem registryItem;

		void UrlComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UrlComboBoxIndexChanged();
		}

		internal void UrlComboBoxIndexChanged()
		{
			SaveTheme();

			if (!IsFirstLoad)
			{
				UpdateTheme();
				NotifyChanges();
			}
		}

		void ThemeDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			ThemeDropEditIndexChanged();
		}

		internal void ThemeDropEditIndexChanged()
		{
			if (!IsFirstLoad)
			{
				NotifyChanges();
			}
		}

		void InitializeUrlComboBox()
		{
			UrlComboBox.Items.Clear();
			UrlComboBox.Items.Add(Res.GetString("d5324f0e-e9fc-4076-987c-ba41c7403c48", "All URLs"));
			var webThemeRegistryItem = RegistryItem as WebThemeRegistryItem;
			var range = webThemeRegistryItem.UrlsRegistryItem.Value.Distinct().ToArray();
			UrlComboBox.Items.AddRange(range);
			UrlComboBox.SelectedIndex = 0;
			IsFirstLoad = false;
		}

		void InitializeThemeDropEdit()
		{
			var businessObject = new DropEditBusinessObject(new ThemeCodeDescriptionPairList());
			ThemeDropEdit.CharacterCasing = CharacterCasing.Normal;
			ThemeDropEdit.BindToList = "List";
			ThemeDropEdit.SetDataBinding(businessObject, "Value");
		}

		void SaveTheme()
		{
			if (LastObject != null)
			{
				LastObject.Code = ThemeDropEdit.Text;
			}
			else
			{
				ValueList.Add(new WebTrackerTheme(LastUrl, ThemeDropEdit.Text));
			}
		}

		void UpdateTheme()
		{
			LastUrl = CurrentUrl;
			if (CurrentObject != null)
			{
				ThemeDropEdit.Text = CurrentObject.Code;
			}
			else
			{
				ThemeDropEdit.Text = ThemeCodeDescriptionPairList.Codes.CUS;
			}
		}

		WebTrackerTheme CurrentObject
		{
			get { return ValueList.FirstOrDefault(x => x.Url == CurrentUrl); }
		}

		WebTrackerTheme LastObject
		{
			get { return ValueList.FirstOrDefault(x => x.Url == LastUrl); }
		}

		string CurrentUrl
		{
			get { return UrlComboBox.SelectedIndex == 0 ? String.Empty : (string)UrlComboBox.SelectedItem; }
		}

		string LastUrl { get; set; }
	}
}
