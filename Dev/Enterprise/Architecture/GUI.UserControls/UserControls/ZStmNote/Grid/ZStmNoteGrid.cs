using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public class ZStmNoteGrid : ZGrid
	{
		#region Bind

		public override void SetDataBinding(object dataSource, string dataMember, string tableName)
		{
			if (dataSource == null)
			{
				if (Collection != null)
				{
					Collection.OnAttemptedToDeleteRelatedNote -= Collection_OnAttemptedToDeleteRelatedNote;
					Collection.OnAttemptedToDeleteNonDeletableNote -= Collection_OnAttemptedToDeleteNonDeletableNote;
				}

				ColourDeciding -= ZStmNoteGrid_ColourDeciding;

				if (ListManager != null)
				{
					ListManager.CurrentChanged -= ListManager_CurrentChanged;
				}
				if (ParentZTabControl != null)
				{
					ParentZTabControl.SelectedIndexChanged -= ParentZTabControl_SelectedIndexChanged;
				}
			}

			base.SetDataBinding(dataSource, dataMember, tableName);

			if (dataSource != null)
			{
				Collection.OnAttemptedToDeleteRelatedNote += Collection_OnAttemptedToDeleteRelatedNote;
				Collection.OnAttemptedToDeleteNonDeletableNote += Collection_OnAttemptedToDeleteNonDeletableNote;

				ColourDeciding += ZStmNoteGrid_ColourDeciding;
				ListManager.CurrentChanged += ListManager_CurrentChanged;
				if (ParentZTabControl != null)
				{
					ParentZTabControl.SelectedIndexChanged += ParentZTabControl_SelectedIndexChanged;
				}

				FireCurrentChanged(); // in case first row is a related note
			}
		}

		// Based on KUserControl::CurrentDataItem
		public BusinessObject CurrentDataItem
		{
			get
			{
				object result = null;
				if (DataSource != null && ListManager != null)
				{
					result = ListManager != null && ListManager.Position != -1 ? ListManager.GetCurrent() : null;
				}
				return (BusinessObject)result;
			}
		}

		#endregion

		#region Attempting to delete Related & Non-Deletable Notes

		void Collection_OnAttemptedToDeleteRelatedNote(object sender, EventArgs e) => Globals.Message.ShowError(Res.GetString("9f9b2f24-743c-41bc-aa54-0a7bd5fdcef3", "Related notes cannot be deleted."));

		void Collection_OnAttemptedToDeleteNonDeletableNote(object sender, NonDeletableNoteEventArgs e)
		{
			var message = Res.GetString("10f3d879-67f1-4790-bd4a-1bb6dca8a2f8", "A '{0}' note cannot be edited or deleted after it has been saved.", e.NoteDescription);
			Globals.Message.ShowError(message);
		}

		#endregion

		#region Has User Read Related Notes?

		protected void ZStmNoteGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var note = (IStmNoteInternals)e.ObjectAtRow;
			if (note != null && !note.IsNoteRead && note.IsRelatedInParentView)
			{
				var bizoToUseForColour = BizO is IStmALogParent ? BizO : CurrentDataItem;
				e.Colour = bizoToUseForColour.GetLogs().HasUserReadNotes()
					? SystemColors.Control
					: Color.FromArgb(235, 155, 155);
			}
		}

		void FireCurrentChanged()
		{
			if (ListManager.Position != -1)
			{
				ListManager_CurrentChanged(this, EventArgs.Empty);
			}
		}

		void ParentZTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ParentZTabControl.SelectedTab == ParentZStmNoteTab && Visible)
			{
				FireCurrentChanged(); // in case first row is a related note
			}
			else if (fUnreadRelatedNoteTimer != null && UnreadRelatedNoteTimer.Enabled)
			{
				UnreadRelatedNoteTimer.Stop();
			}
		}

#if DEBUG
		public
#endif
		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (fUnreadRelatedNoteTimer != null)
			{
				UnreadRelatedNoteTimer.Stop();
			}

			if (CurrentNote != null
				&& BizO is IStmALogParent
				&& !((IStmALogParent)BizO).Logs.HasUserReadNotes()
				&& CurrentNoteInternals.IsRelatedInParentView
				&& !CurrentNoteInternals.IsNoteRead)
			{
				UnreadRelatedNoteTimer.Start();
			}
		}

		protected void UnreadRelatedNoteTimer_Tick(object sender, EventArgs e)
		{
			UnreadRelatedNoteTimer.Stop();
			if (CurrentNoteInternals != null)
			{
				CurrentNoteInternals.IsNoteRead = true;
			}

			Invalidate();

			var bizo = BizO;
			if (bizo != null && AllRelatedNotesRead)
			{
				bizo.GetLogs().MarkNotesAsRead();
			}
		}

		Timer UnreadRelatedNoteTimer
		{
			get
			{
				if (fUnreadRelatedNoteTimer == null)
				{
					fUnreadRelatedNoteTimer = new Timer();
					var milliseconds = (Env.Registry.RelatedNoteReadDelay <= 0) ? 1 : Env.Registry.RelatedNoteReadDelay * 1000;
					fUnreadRelatedNoteTimer.Interval = milliseconds;
					fUnreadRelatedNoteTimer.Tick += new EventHandler(UnreadRelatedNoteTimer_Tick);
				}
				return fUnreadRelatedNoteTimer;
			}
		}
		Timer fUnreadRelatedNoteTimer;

		bool AllRelatedNotesRead
		{
			get
			{
				if (List != null)
				{
					foreach (IStmNoteInternals note in List)
					{
						if (note.IsRelatedInParentView && !note.IsNoteRead)
						{
							return false;
						}
					}
				}

				return true;
			}
		}

		protected StmNote CurrentNote => (StmNote)ListManager.GetCurrent();

		protected IStmNoteInternals CurrentNoteInternals => CurrentNote;

		BusinessObject BizO
		{
			get
			{
				if (DataSource is BusinessObjectCollection)
				{
					var collection = DataSource as IBusiness;
					return (BusinessObject)collection[0];
				}
				return (BusinessObject)DataSource;
			}
		}

		#endregion

		#region Parent Tab / Parent TabControl

		ZTabControl ParentZTabControl
		{
			get
			{
				if (fParentZTabControl == null)
				{
					var parentControl = Parent;
					while (parentControl != null && !(parentControl is ZTabControl))
					{
						parentControl = parentControl.Parent;
					}

					fParentZTabControl = parentControl as ZTabControl;
				}
				return fParentZTabControl;
			}
		}

		ZStmNoteTabPage ParentZStmNoteTab
		{
			get
			{
				if (fParentZStmNoteTab == null)
				{
					var parentControl = Parent;
					while (parentControl != null && !(parentControl is ZStmNoteTabPage))
					{
						parentControl = parentControl.Parent;
					}

					fParentZStmNoteTab = parentControl as ZStmNoteTabPage;
				}

				return fParentZStmNoteTab;
			}
		}

		ZTabControl fParentZTabControl;
		ZStmNoteTabPage fParentZStmNoteTab;

		#endregion

		#region List

		StmNoteCollectionView Collection => (StmNoteCollectionView)base.List;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Collection != null)
				{
					Collection.OnAttemptedToDeleteRelatedNote -= new EventHandler(Collection_OnAttemptedToDeleteRelatedNote);
					Collection.OnAttemptedToDeleteNonDeletableNote -= new NonDeletableNoteEventHandler(Collection_OnAttemptedToDeleteNonDeletableNote);
				}

				if (ListManager != null)
				{
					ListManager.CurrentChanged -= new EventHandler(ListManager_CurrentChanged);
				}

				if (fUnreadRelatedNoteTimer != null)
				{
					fUnreadRelatedNoteTimer.Dispose();
				}

				fParentZStmNoteTab = null;
				fParentZTabControl = null;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
