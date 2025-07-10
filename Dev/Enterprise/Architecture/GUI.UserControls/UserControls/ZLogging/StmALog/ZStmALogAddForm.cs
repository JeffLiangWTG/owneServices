using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmALogAddForm : ZChildForm
	{
		public ZStmALogAddForm(StmALogCollectionView logCollection, bool parentHasChanges)
			: this(new BusinessObjectFactory().New<StmALogAsAddedByUser>(), parentHasChanges)
		{
			this.parent = logCollection.Parent;
		}

		public ZStmALogAddForm(StmALogCollectionView logCollection, bool parentHasChanges, string stringToAppendToReference)
			: this(logCollection, parentHasChanges)
		{
			this.stringToAppendToReference = stringToAppendToReference;
		}

		public ZStmALogAddForm(StmALogCollectionView logCollection, bool parentHasChanges, bool isEventAndReferenceReadOnly)
			: this(logCollection, parentHasChanges)
		{
			EventAddUserControl.SetEventAndReferenceToReadOnly(isEventAndReferenceReadOnly);
		}

		internal ZStmALogAddForm(StmALogAsAddedByUser tempLog, IStmALogParent parent)
			: this(tempLog, parent.HasChanges)
		{
			this.parent = parent;
		}

		internal ZStmALogAddForm(StmALogAsAddedByUser tempLog, bool parentHasChanges)
			: base(tempLog)
		{
			InitializeComponent();
			this.Icon = Icons.GetIcon(IconTypes.Events);

			TempLog.IsEditing = true; // the log collection is readonly but we need to edit this log
		}

		#region Adding / Cancelling the Log

		internal Action<StmALog> EventAddedAction;

		void AddButton_Click(object sender, EventArgs e)
		{
			if (parent != null && TempLog.SL_Parent.IsEmpty)
			{
				TempLog.SL_Parent = parent.LogsParentPK;
			}
			TempLog.RunPreSaveValidation();

			if (TempLog.HasErrors)
			{
				base.ShowErrorsDialog();

				return;
			}

			TempLog.IsEditing = false;

			if (!Business.Events.All.Contains(TempLog.SL_SE_NKEvent))
			{
				ErrorReporter.ReportOnce("EventWithCodeNotFound", "Event with code '" + TempLog.SL_SE_NKEvent + "' was not found in the 'Business.Events.All' dictionary.");
				UserNotification.Instance.ShowError(Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("DB47FA76-A52E-4844-92CB-EA41475D9788", "Could not add an event with the specified code"));

				Close();
				return;
			}

			var eventType = Business.Events.All[TempLog.SL_SE_NKEvent];
			var eventValue = new EventValue(eventType,
				eventTime: TempLog.SL_EventTimeOffset,
				isEstimate: TempLog.SL_IsEstimate,
				reference: StmALog.GetFreeTextFromReference(ReferenceWithStringAppended),
				parameters: StmALog.GetParametersFromReference(ReferenceWithStringAppended));

			var stmALog = parent.Logs.AddNew(eventValue);
			EventAddedAction?.Invoke(stmALog);

			Close();
		}

		#region ReferenceWithStringAppended

		ZString ReferenceWithStringAppended
		{
			get
			{
				ZString referenceWithStringAppended;

				if (stringToAppendToReference.IsEmpty || TempLog.SL_Reference.Contains(stringToAppendToReference, StringComparison.CurrentCultureIgnoreCase))
				{
					referenceWithStringAppended = TempLog.SL_Reference;
				}
				else
				{
					referenceWithStringAppended = TempLog.SL_Reference.IsEmpty
					? stringToAppendToReference
					: (ZString)(TempLog.SL_Reference + " " + stringToAppendToReference);
				}

				return referenceWithStringAppended;
			}
		}

		#endregion

		void CancelAddButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		BaseStmALog TempLog
		{
			get { return BusinessEntity as BaseStmALog; }
		}

		readonly IStmALogParent parent;
		readonly ZString stringToAppendToReference;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
