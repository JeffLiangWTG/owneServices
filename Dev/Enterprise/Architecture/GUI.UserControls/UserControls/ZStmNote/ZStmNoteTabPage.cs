using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressResourceStringContainerControlNameKeyPrefix]
	public class ZStmNoteTabPage : ZBindingTabPage, IStmNoteControl, INotesTabPage
	{
		public ZStmNoteTabPage()
		{
			InitialiseComponent();
			CaptionResourceString = Res.GetData("4fb3cb92-63a8-4c91-b7a4-04ad7ad6a34d", "Notes", "The Notes tab.");
		}

		#region Initialization

		void InitialiseComponent()
		{
			if (ZStmNoteUserControl == null && DesignModeFinder.IsDesigning)
			{
				CreateZStmNoteUserControl();
			}
		}

		protected virtual void CreateZStmNoteUserControl()
		{
			ZStmNoteUserControl = new ZStmNoteUserControl();
			ZStmNoteUserControl.SetBindingMember(".");
			AddStmNoteUserControlToControls();
		}

		protected void AddStmNoteUserControlToControls()
		{
			if (RemoveDescriptionColumn)
			{
				ZStmNoteUserControl.RemoveDescriptionColumn();
			}

			if (!DesignModeFinder.IsDesigning && DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				ZStmNoteUserControl.RemoveNoteContext();
			}

			SuspendLayout();
			Controls.Add(ZStmNoteUserControl);
			ResumeLayout(true);
		}

		protected ZStmNoteUserControl ZStmNoteUserControl;

		public void HideNoteText()
		{
			if (ZStmNoteUserControl != null)
			{
				ZStmNoteUserControl.NoteGrid.SetAvailability(false, StmNote.Schema.ST_NoteDataAsTextConcatenatedAndTrimmed);
				ZStmNoteUserControl.NoteRichTextBox.Visible = false;
			}
		}

		#endregion

		#region Description Column

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool RemoveDescriptionColumn
		{
			get => fRemoveDescriptionColumn;
			set
			{
				fRemoveDescriptionColumn = value;

				if (value && ZStmNoteUserControl != null)
				{
					ZStmNoteUserControl.RemoveDescriptionColumn();
				}
			}
		}
		bool fRemoveDescriptionColumn;

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed && disposing)
			{
				if (noteParent != null)
				{
					noteParent.CustomNoteTypesDelegate = null;
					noteParent = null;
				}

				if (Form != null)
				{
					Form.Shown -= ZStmNoteTabPage_Shown;
					_form = null;
				}

				fBusinessEntity = null;
			}
			base.Dispose(disposing);
		}

		#region Unread Notes

		protected override void OnAdded()
		{
			base.OnAdded();

			SynchronizationContext.Current.Post(delegate
			{ HookUpValidatingForSave(); }, null);
		}

		protected virtual void HookUpValidatingForSave()
		{
			if (FindForm() is ZForm form)
			{
				form.ValidatingForSave -= Form_ValidatingForSave;
				form.ValidatingForSave += Form_ValidatingForSave;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging only")]
		void Form_ValidatingForSave(object sender, ValidatingForSaveEventArgs e)
		{
			if (UserNeedsToReadRelatedNotes)
			{
				this.UpdateNoteImage();

				var debugLogForCS01154849 = (ZStringBuilder)BusinessEntity.GetField("debugLogForCS01154849");
				debugLogForCS01154849?.AppendLine("Before reading notes.");
				debugLogForCS01154849?.AppendLine((string)BusinessEntity.GetProperty("DebugInfoForCS01154849"));

				e.ContinueWithSave = ShowUnreadRelatedNotesDialog();
				if (e.ContinueWithSave == ContinueWithSave.No)
				{
					FocusOnTabPage();

					debugLogForCS01154849?.AppendLine("After reading notes.");
					debugLogForCS01154849?.AppendLine((string)BusinessEntity.GetProperty("DebugInfoForCS01154849"));
				}
			}
		}

		internal bool UserNeedsToReadRelatedNotes => BusinessEntity != null && BusinessEntity.GetNotes().HasVisibleRelatedNotes && !BusinessEntity.GetLogs().HasUserReadNotes() && !BusinessEntityIsTemplateRecord;

		bool BusinessEntityIsTemplateRecord => BusinessEntity is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord;

		ContinueWithSave ShowUnreadRelatedNotesDialog()
		{
			var message = Res.GetString("7fb59fe2-db59-4395-b8ac-363c0d6edfac", "Unread notes exist for this {0}. Read these notes before saving?", BusinessEntity.HumanReadableName);

			using (var msgBox = new ZMessageBox(message, Res.GetString("6bc0548e-0870-4be6-bfd3-0b830355d255", "Important!"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
			{
				msgBox.SetCustomIcon(Icons.GetIcon(IconTypes.StmNoteLarge));
				var result = ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);

				if (result == DialogResult.Yes)
				{
					return ContinueWithSave.No;
				}
				else
				{
					_ = BusinessEntity.GetLogs().AddNew(ZArchitecture.Business.Events.RelatedNotesNotRead, string.Format(CultureInfo.InvariantCulture, "|NAM={0}", EnvProxy.Instance.CurrentUser.FullName)); // not constant always changes plus its a parameter
					return ContinueWithSave.Yes;
				}
			}
		}

		BusinessObject BusinessEntity
		{
			get
			{
				if (fBusinessEntity == null)
				{
					if (Form != null)
					{
						if (Form.BusinessEntity is BusinessObjectCollection)
						{
							if (Form.BusinessEntity.Count > 0)
							{
								fBusinessEntity = (BusinessObject)Form.BusinessEntity[0];
							}
						}
						else
						{
							fBusinessEntity = (BusinessObject)Form.BusinessEntity;
						}
					}
				}
				return fBusinessEntity;
			}
		}

		protected BusinessObject fBusinessEntity;

		ZForm Form => _form ?? (_form = FindForm() as ZForm);
		ZForm _form;

		#endregion

		#region Note Icon

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			if (Form != null)
			{
				Form.Shown -= ZStmNoteTabPage_Shown;
				Form.Shown += ZStmNoteTabPage_Shown;
			}
			UpdateInitialNoteImageViaOnLayout(); // this is necessary in case the user instantiates the form, adds a note, shows the form - in that order. test in place for this [Geoff].
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Tab Page Name")]
		void ZStmNoteTabPage_Shown(object sender, EventArgs e)
		{
			if (Form != null && !CaptionRenderingSupport.IsCaptionRenderingEnabled(this))
			{
				Text = "Notes";
			}
		}

		public override void ClearNotificationImage()
		{
			base.ClearNotificationImage();
			this.UpdateNoteImage();
		}

		protected override void UpdateInitialTabImageCore()
		{
			base.UpdateInitialTabImageCore();
			if (BusinessEntity != null)
			{
				this.UpdateNoteImage();
			}
		}

		void UpdateInitialNoteImageViaOnLayout()
		{
			if (!InitialImageUpdateViaOnLayoutDone && BusinessEntity != null)
			{
				_ = UserIdleWorker.QueueWorkItem(this, new MethodInvoker(() => this.UpdateNoteImage()));
				InitialImageUpdateViaOnLayoutDone = true;
			}
		}

		bool InitialImageUpdateViaOnLayoutDone;

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get => ZStmNoteUserControl != null && ZStmNoteUserControl.ReadOnly;
			set
			{
				if (ZStmNoteUserControl == null)
				{
					CreateZStmNoteUserControl();
				}
				ZStmNoteUserControl.ReadOnly = value;
			}
		}

		#endregion

		#region IStmNoteControl Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StmNote SelectedNote
		{
			get => ZStmNoteUserControl?.SelectedNote;
			set
			{
				if (ZStmNoteUserControl == null)
				{
					CreateZStmNoteUserControl();
				}
				ZStmNoteUserControl.SelectedNote = value;
			}
		}

		public void FocusOnTabPage()
		{
			if (ZStmNoteUserControl == null)
			{
				CreateZStmNoteUserControl();
			}
			ZStmNoteUserControl.FocusOnTabPage();
		}

		public virtual void ShowMessage(string caption, string message)
		{
			if (ZStmNoteUserControl == null)
			{
				CreateZStmNoteUserControl();
			}
			ZStmNoteUserControl.ShowMessage(caption, message);
		}

		#endregion

		#region Hiding Properties

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get => base.Text;
			set => base.Text = value;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int ImageIndex
		{
			get => base.ImageIndex;
			set => base.ImageIndex = value;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override ResourceStringData CaptionResourceString
		{
			get => base.CaptionResourceString;
			set => base.CaptionResourceString = value;
		}

		#endregion

		#region Auto Sized

		protected sealed override bool IsAutoSized => true;

		#endregion

		#region Custom Notes

		NoteTypeCollection GetCustomNoteTypeListForThisModule()
		{
			NoteTypeCollection result = null;
			var controllerID = Form == null ? null : ((IZForm)Form).ControllerID;

			if (controllerID != null)
			{
				var moduleID = ZControllerFactory.Create(controllerID).ModuleID;
				if (moduleID != null)
				{
					result = CustomNotesProvider.Instance.CustomNoteTypesForModuleAndThisCountry(moduleID.Name);
				}
			}

			return result;
		}

		#endregion

		#region Binding

		IStmNoteParent noteParent;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (dataSource is BusinessObject businessObject)
			{
				noteParent = businessObject.GetNotes().Parent;
				noteParent.CustomNoteTypesDelegate = new GetValueDelegate<NoteTypeCollection>(GetCustomNoteTypeListForThisModule);
				businessObject.GetNotes().AddFetchHintForVisibleNotes();
			}

			if (ZStmNoteUserControl == null)
			{
				CreateZStmNoteUserControl();
			}

			base.SetDataBindingCore(dataSource, dataMember);
		}

		//this used to be a performance save, but now that Country Validation Rules exist, we do need to bind it to show those errors/warnings.
		[DefaultValue(false)]
		public override bool ExcludeFromBindingOnSave => false;

		#endregion

		#region INotesTabPage Members

		bool INotesTabPage.HasRelatedNotes => BusinessEntity != null && !BusinessEntity.IsDeleted && BusinessEntity.GetNotes().HasRelatedNotes;

		bool INotesTabPage.HasNotes => BusinessEntity != null && BusinessEntity.GetNotes().HasNotes;

		IBusiness INotesTabPage.BusinessEntity => BusinessEntity;

		#endregion

		#region Implementation
		public void UpdateNoteImageOnRelatedChanges() => BusinessEntity?.GetNotes()?.LoadRelatedElements();
		#endregion
	}
}
