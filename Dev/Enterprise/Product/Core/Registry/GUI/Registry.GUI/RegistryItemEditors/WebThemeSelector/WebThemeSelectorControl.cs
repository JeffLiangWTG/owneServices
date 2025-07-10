using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebThemeSelectorControl : RegistryZUserControl
	{
		public WebThemeSelectorControl()
		{
			InitializeComponent();
			ThemeListGrid.AllowSorting = false;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			CreateThemeButton.ReadOnly = readOnly;
			CopyThemeButton.ReadOnly = readOnly;
			DeleteThemeButton.ReadOnly = readOnly;
			ThemeListGrid.ReadOnly = readOnly;
			webThemeChildControl.SetReadOnly(readOnly);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			kSplitContainer1.FixedPanel = FixedPanel.Panel2;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!DesignModeFinder.IsDesigning && ThemeListGrid.ListManager != null)
			{
				ThemeListGrid.ListManager.PositionChanged += ListManager_PositionChanged;
				SetReadOnlyOnGridPositionChanged();
			}
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			SetReadOnlyOnGridPositionChanged();
		}

		void SetReadOnlyOnGridPositionChanged()
		{
			DeleteThemeButton.ReadOnly = ThemeListGrid.ListManager.Position == 0;
			webThemeChildControl.SetReadOnly(ThemeListGrid.ListManager.Position == 0);
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public new WebThemeCustomObjectCollection DataSource => (WebThemeCustomObjectCollection)base.DataSource;

		void CreateNewTheme(UserResponseArgument args)
		{
			var name = Globals.Message.QueryUserResponse(args);
			if (!string.IsNullOrEmpty(name))
			{
				var newTheme = new WebThemeCustomObject(DataSource.CurrentFallbackLevel, DataSource.Factory);
				newTheme.ThemeName = name;
				DataSource.Add(newTheme);
			}
		}

		string EnterNewThemeNameMessage
		{
			get { return Res.GetString("cb5c1b2e-0313-492f-ba64-6c2be27a465b", "Please enter a name for the new theme."); }
		}

		void CreateThemeButton_Click(object sender, EventArgs e)
		{
			CreateTheme();
		}

		internal void CreateTheme()
		{
			var args = new UserResponseArgument();
			args.Caption = Res.GetString("a322242e-a8b3-47ee-beb4-af78cbaf5b76", "New Theme");
			args.Message = EnterNewThemeNameMessage;
			args.MinimumResponseLength = 1;
			args.Buttons = ZMessageBoxButtons.OKCancel;
			args.DefaultButton = ZMessageBoxDefaultButton.Button1;

			CreateNewTheme(args);
			NotifyChanges();
		}

		void CopyThemeButton_Click(object sender, EventArgs e)
		{
			CopyTheme();
		}

		internal void CopyTheme()
		{
			if (DataSource != null && ThemeListGrid.ListManager != null)
			{
				var current = ThemeListGrid.ListManager.GetCurrent() as WebThemeCustomObject;
				if (current != null)
				{
					var clone = new WebThemeCustomObject(current.CurrentFallbackLevel, current.Factory);
					clone.ThemeName = (NoResString)"New Theme Name";
					clone.CSS = current.CSS;

					foreach (WebCustomThemeImageBusinessObject item in current.ImageCollection)
					{
						var newItem = new WebCustomThemeImageBusinessObject();
						newItem.ImageName = item.ImageName;
						newItem.Data = item.Data;
						clone.ImageCollection.Add(newItem);
					}

					DataSource.Add(clone);
					NotifyChanges();
				}
			}
		}

		void DeleteThemeButton_Click(object sender, EventArgs e)
		{
			DeleteTheme();
		}

		internal void DeleteTheme()
		{
			if (DataSource != null && ThemeListGrid.ListManager != null)
			{
				var current = ThemeListGrid.ListManager.GetCurrent() as WebThemeCustomObject;
				if (current != null)
				{
					DataSource.Remove(current);
					NotifyChanges();
				}
			}
		}
	}
}
