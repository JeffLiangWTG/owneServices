using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.GUI.ExportService;
using Enterprise.UniversalCopy.GUI.ImportService;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.GUI
{
	public partial class UniversalCopyTemplateForm : ZTemplateForm
	{
		public UniversalCopyTemplateForm(UniversalCopyTemplate copyTemplate, UniversalCopyManager copyManager)
			: base(copyTemplate)
		{
			InitializeComponent();

			CopyManager = copyManager;
			ucTemplateUserControl.CopyManager = copyManager;
		}

		public UniversalCopyManager CopyManager { get; private set; }

		public UniversalCopyTemplate Template
		{
			get { return (UniversalCopyTemplate)BusinessEntity; }
		}

		public override string FormCaption
		{
			get
			{
				var template = Template;
				if (template != null && !template.FilterNameWithoutHotkeys.IsEmpty)
				{
					return Res.GetString("b863916b-a232-4cbf-93d7-409278099ef8", "Universal Copy Template {0}", template.FilterNameWithoutHotkeys);
				}
				else
				{
					return base.FormCaption;
				}
			}
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Template.S9_IsPublished)
			{
				if (Template.IsPublishedGlobal && !EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowedForAllBranches)
				{
					Globals.Message.ShowWarning(Res.GetString("c52e3a4b-57ae-446c-ab80-310fb343aca6", "You don't have permission to edit Universal Copy templates published across all companies."));
					Template.SetReadOnlyIncludingChildren(true);
					DisplayMode = ODisplayMode.ReadOnly;
				}

				if (!EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed || !CopyManager.Security.CanEditPublic)
				{
					Globals.Message.ShowWarning(Res.GetString("556d9a0b-bceb-4f4e-96e9-902044f3a493", "You don't have permission to edit published Universal Copy templates."));
					Template.SetReadOnlyIncludingChildren(true);
					DisplayMode = ODisplayMode.ReadOnly;
				}
			}

			if (DisplayMode != ODisplayMode.ReadOnly)
			{
				PrepareDeleteMenu();

				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("676845d0-a48c-4335-8452-6ce84b66bc94", "Default all to Do Not Copy"), DefaultAllToDoNotCopy);
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("c1c4ac2d-01aa-4bac-b080-fac7d2f845e4", "Default all to Copy/Link"), DefaultAllToCopy);

				ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem("-"));
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("398cb0fe-a6c3-4ad2-bc6b-f4f048b808f6", "Export to File"), ExportTemplateMenuClicked);
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("e5dfe68d-15c9-415e-9290-947997e17d56", "Import from File"), ImportTemplateMenuClicked);
			}
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}
#if DEBUG
		internal
#endif
		protected override void SaveInternal()
		{
			if (ucTemplateUserControl.ElementDetails != null)
			{
				ucTemplateUserControl.ElementDetails.SaveFilterLayout();
			}
			Template.PrepareForSave();

			base.SaveInternal();

			if (CopyManager != null)
			{
				CopyManager.IsCopyMenuItemPopulated = false;
			}
		}

		#region Delete

		void PrepareDeleteMenu()
		{
			if (FileMenuItem != null)
			{
				var deleteMenuItem = FileMenuItem.MenuItems.FindByName(ZFormMenuStrategy.FileDeleteMenuItemName);
				if (deleteMenuItem != null && !deleteMenuItem.Enabled)
				{
					switchToDeleteModeMenuItem = new ZMenuItem(ResString.GetMultilingualString("430ad53b-8b0f-4441-8f92-2c8893e9cd2b", "Switch to Delete mode"));
					switchToDeleteModeMenuItem.Click += SwitchToDeleteMode;
					FileMenuItem.MenuItems.Add(deleteMenuItem.Index, switchToDeleteModeMenuItem);
				}
			}
		}

		ZMenuItem switchToDeleteModeMenuItem;

		void SwitchToDeleteMode(object sender, EventArgs e)
		{
			if (Template.S9_IsPublished && !CopyManager.Security.CanDeletePublic)
			{
				Globals.Message.ShowError(Res.GetString("165DCA14-A839-4584-B0D6-E5D73A238829", "You don't have permission to delete published Universal Copy templates."));
				return;
			}

			if (switchToDeleteModeMenuItem != null)
			{
				switchToDeleteModeMenuItem.Enabled = false;
			}
			Template.CancelChanges();
			Template.SetReadOnlyIncludingChildren(true);
			DisplayMode = ODisplayMode.Delete;
		}

		#endregion

		#region Default Copy Method

		void DefaultAllToDoNotCopy(object sender, EventArgs eventArgs)
		{
			Template.CopyTemplateTree.DefaultToDoNotCopy();
		}

		protected int QueryDefaultDepth(int defaultDepth = 4)
		{
			var resultAsString = Globals.InteractiveNotification.QueryDefaultValue(defaultDepth.ToString(System.Globalization.CultureInfo.InvariantCulture),
				Res.GetString("455267e3-1b32-453a-a755-b0d4c9559369", "Please enter the depth to which copying should be the default.\r\n(Accepted values: 1-9. Values higher than 4 may cause a long delay.)"),
				Res.GetString("2e24b855-d648-42a2-9994-61b3962be205", "Defaulting Depth"),
				1, 1, true);
			if (int.TryParse(resultAsString, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var resultAsNumber))
			{
				return resultAsNumber;
			}
			else
			{
				return -1;
			}
		}

		void DefaultAllToCopy(object sender, EventArgs eventArgs)
		{
			var amount = QueryDefaultDepth();
			Cursor = Cursors.WaitCursor;
			try
			{
				if (amount >= 0)
				{
					Template.CopyTemplateTree.DefaultToCopy(amount);
				}
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		#endregion

		#region Export and Import

		void ExportTemplateMenuClicked(object sender, EventArgs e)
		{
			var service = new UniversalCopyXmlExportService();
			if (string.IsNullOrEmpty(Template.CopyTemplateTree.ConfigurationName))
			{
				Globals.Message.ShowWarning(Res.GetString("44DBEE64-8DCF-43D5-B03D-FA744B2291B9", "Please specify Template Name before export."));
			}
			else
			{
				service.Export(Template.CopyTemplateTree.CopyTemplateNode);
			}
		}

		void ImportTemplateMenuClicked(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				var service = new UniversalCopyXmlImportService();
				var copyTemplateTree = service.Import(true);
				if (copyTemplateTree != null)
				{
					if (copyTemplateTree.Name != Template.CopyTemplateTree.Name)
					{
						Globals.Message.ShowWarning(Res.GetString("B08C3C98-7BA1-44BC-B161-0B9AF2CFEFB3", "Selected file contains copy template configuration for module {0}, and cannot be applied into current Copy Template. Please select other file, or import it in correct module.", copyTemplateTree.TableName));
					}
					else
					{
						var importedCopyTemplateTree = CopyManager.GetNewCopyTemplateFromImport(copyTemplateTree).CopyTemplateTree;
						if (string.IsNullOrEmpty(Template.CopyTemplateTree.ConfigurationName))
						{
							Template.CopyTemplateTree.ConfigurationName = importedCopyTemplateTree.ConfigurationName;
						}

						Template.CopyTemplateTree = importedCopyTemplateTree;
						ucTemplateUserControl.templateTreeView.Unbind();
						ucTemplateUserControl.templateTreeView.Bind(Template.CopyTemplateTree);
					}
				}
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		#endregion

		#region Implementation
#if DEBUG
		internal
#endif
		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (Template.S9_IsPublished && !CopyManager.Security.CanEditPublic)
			{
				Globals.Message.ShowError(Res.GetString("71D6D0C5-B769-431D-8DF3-3C85EEC37AB6", "You don't have permission to publish Universal Copy templates."));
				result = ContinueWithSave.No;
			}
			return result;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
