using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	/// <summary>
	/// A ZPlugIn that allows users to enter all User Defined Fields for all templates within context
	/// and also provides the Document menu for a form to run documents. 
	/// </summary>
	public class DocumentUDFPlugIn : DocumentPlugin
	{
		public DocumentUDFPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			MainTabControl = new ZTemplateTabControl();
			MainTabControl.Dock = DockStyle.Fill;

			MainUserControl = new DocumentContainerControl();
			MainUserControl.Controls.Add(MainTabControl);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (MainUserControl != null)
				{
					MainUserControl.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return false; }
		}

		public override string Name
		{
			get { return Res.GetString("c8ca600e-de87-4a30-addd-1dc7ae96d1ff", "User Defined Data"); }
		}

		protected override Control GetNewUserControl()
		{
			return MainUserControl;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Note;
		}

		public override void Delete()
		{
			object noteLoaded = Note;
			base.Delete();
		}

		public override BusinessObjectFactory Factory
		{
			get { return (InputBusinessObject != null) ? InputBusinessObject.Factory : null; }
		}

		#region Implementation

		const int TopMargin = 16;
		const int LeftMargin = 8;

		protected readonly ZUserControl MainUserControl;
		protected readonly ZTemplateTabControl MainTabControl;
		protected bool IsSynchronised;

		#region HintLabel

		bool shouldBeReadOnly;

		public void SetHintLabelVisibility(bool shouldBeReadOnly)
		{
			if (this.shouldBeReadOnly != shouldBeReadOnly)
			{
				this.shouldBeReadOnly = shouldBeReadOnly;
				MainTabControl.TabPages.OfType<UDFTabPage>().ForEach(t => t.SetHintLabelVisibility(shouldBeReadOnly));
			}
		}

		#endregion

		protected override void Note_HasChangesChanged(object sender, EventArgs e)
		{
			if (Note.HasChanges)
			{
				InputBusinessObject.RegisterEditableChildObject(Note);
				Note.HasChangesChanged -= Note_HasChangesChanged;
			}
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return (InputBusinessObject != null && InputBusinessObject.IsInDatabase && Note != null);
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				if (Note != null)
				{
					return Res.GetString("DocumentUDFPlugIn|NotDisplayedMessage", "Please save the new record before using this feature.");
				}
				else
				{
					return Res.GetString("2fd6552f-b67e-4644-896a-13f74ccdc6a2", "An error occurred when loading the plug-in; another user may have tried to load it simultaneously. Please close the form and try again.");
				}
			}
		}

		public override void OnUserControlShown()
		{
			if (ShouldPlugInGUIAndBusinessEntityBeCreated())
			{
				MainTabControl.Visible = true;

				if (!IsSynchronised)
				{
					IsSynchronised = true;

					using (Note.SuspendSettingHasChanges())
					{
						PutUDFsAsControlsOnTabs();
					}
				}
				using (Note.SuspendSettingHasChanges())
				{
					UpdateUDFDefaults();
				}

				Note.RemoveUnnecessaryUDFValidators();
			}
			else
			{
				MainTabControl.Visible = false;
			}
		}

		public override void RefreshData()
		{
			if (MainTabControl.Visible)
			{
				using (Note.SuspendSettingHasChanges())
				{
					UpdateUDFDefaults();
				}
			}
		}

		public void UpdateUDFDefaults()
		{
			if (IsSynchronised)
			{
				Note.UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate();
			}
		}

		ZTabPage MiscTabPage;

		void PutUDFsAsControlsOnTabs()
		{
			MiscTabPage = new ZTabPage();
			MiscTabPage.AutoScroll = true;
			MiscTabPage.Name = "Miscellaneous";
			MiscTabPage.CaptionResourceString = Res.GetData("DocumentContainerControl|2BC1BF7F-E82C-4387-A450-CC1C415C0D9D", "Miscellaneous");
			MainTabControl.TabPages.Add(MiscTabPage);
			MiscTabPage.Size = MainTabControl.Size;

			Hashtable tabPagesTable = new Hashtable();
			Hashtable maxControlWidth = new Hashtable();
			Hashtable nextYTable = new Hashtable();
			Hashtable nextXTable = new Hashtable();

			foreach (FilterField field in Note.UserDefinedFieldList)
			{
				ArrayList tabPagesToAddTo = new ArrayList();
				if (field.TabNames.Count > 0)
				{
					tabPagesToAddTo.AddRange(GetTabPagesToAddTo(field, tabPagesTable));
				}
				else
				{
					tabPagesToAddTo.Add(MiscTabPage);
				}

				PlaceControlOnTabs(field, nextYTable, nextXTable, maxControlWidth, tabPagesToAddTo);
			}

			//reposition it at the end.
			MainTabControl.TabPages.Remove(MiscTabPage);
			MainTabControl.TabPages.Add(MiscTabPage);
		}

		ArrayList GetTabPagesToAddTo(FilterField field, Hashtable tabPagesTable)
		{
			ArrayList tabPagesToAddTo = new ArrayList();

			foreach (string tabName in field.TabNames)
			{
				ZTabPage targetPage;

				if (tabPagesTable.ContainsKey(tabName))
				{
					targetPage = (ZTabPage)tabPagesTable[tabName];
				}
				else
				{
					targetPage = new UDFTabPage(shouldBeReadOnly);
					targetPage.AutoScroll = true;
					targetPage.Size = MainTabControl.Size;
					targetPage.Text = tabName;
					MainTabControl.TabPages.Add(targetPage);
					tabPagesTable.Add(tabName, targetPage);
				}
				tabPagesToAddTo.Add(targetPage);
			}

			return tabPagesToAddTo;
		}

		void PlaceControlOnTabs(FilterField field, Hashtable nextYTable, Hashtable nextXTable, Hashtable maxControlWidth, ArrayList tabPagesToAddTo)
		{
			foreach (TabPage targetPage in tabPagesToAddTo)
			{
				RuntimeOptionUserControl runtimeControl = RuntimeOptionUserControlFactory.New(field);
				runtimeControl.SetFilter(field);

				int nextY = nextYTable.ContainsKey(targetPage) ? (int)nextYTable[targetPage] : ControlDpiScalingHelper.ScaleToCurrentDpiY(TopMargin);
				int nextX = nextXTable.ContainsKey(targetPage) ? (int)nextXTable[targetPage] : ControlDpiScalingHelper.ScaleToCurrentDpiX(LeftMargin);
				if (!maxControlWidth.ContainsKey(targetPage))
				{
					maxControlWidth.Add(targetPage, runtimeControl.Width);
				}
				else
				{
					if ((int)maxControlWidth[targetPage] < runtimeControl.Width)
					{
						maxControlWidth[targetPage] = runtimeControl.Width;
					}
				}

				if ((nextY + runtimeControl.Height) > (targetPage.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(25)))
				{
					nextY = ControlDpiScalingHelper.ScaleToCurrentDpiY(TopMargin);
					nextX += (int)maxControlWidth[targetPage];
					maxControlWidth[targetPage] = 0;
				}
				runtimeControl.Location = ControlDpiScalingHelper.NewScaledPoint(nextX, nextY, false);
				targetPage.Controls.Add(runtimeControl);

				nextYTable[targetPage] = runtimeControl.Bottom;
				nextXTable[targetPage] = nextX;
			}
		}

		public override void OnMenuShown()
		{
			if (InputBusinessObject is IDocumentSupportable)
			{
				object loadedNote = Note;
			}
		}

		#endregion

		#region UDFTabPage

		public class UDFTabPage : ZTabPage
		{
			public UDFTabPage(bool shouldBeReadOnly)
			{
				this.Controls.Add(HintLabel);
				SetHintLabelVisibility(shouldBeReadOnly);
			}

			public void SetHintLabelVisibility(bool shouldBeReadOnly) => HintLabel.Visible = shouldBeReadOnly;

			#region HintLabel

			ZLabel hintLabel;
			ZLabel HintLabel => hintLabel ?? (hintLabel = DocDataPlugInHelper.GetHintLabel());

			#endregion
		}

		#endregion
	}
}
