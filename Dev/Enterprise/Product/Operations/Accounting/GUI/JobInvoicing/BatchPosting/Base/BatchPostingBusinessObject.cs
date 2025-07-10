using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class BatchPostingBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BatchPostingBusinessObject(BaseBatchInvoicingPostManagerGUIWrapper basePostManagerGUIWrapper)
		{
			this.BasePostManagerGUIWrapper = basePostManagerGUIWrapper;
			basePostManagerGUIWrapper.OnObjectPostingFinished += new BaseBatchInvoicingPostManagerGUIWrapper.ObjectPostingFinishedEventHandler(OnObjectPostingFinished);
			basePostManagerGUIWrapper.OnPostingFinished += new EventHandler(BasePostManagerGUIWrapper_OnPostingFinished);
			basePostManagerGUIWrapper.OnStartingToPostNextObject += new BaseBatchInvoicingPostManagerGUIWrapper.StartingToPostNextObjectEventHandler(OnStartingToPostNextObject);
		}

		public delegate void PostingEvent(string message);

		public event PostingEvent OnShowInfoMessage;
		public event EventHandler OnAfterPosted;
		public event EventHandler OnProgress;
		public event EventHandler OnPostingCancelled;

		public void SetFormForPrinting(Form form)
		{
			BasePostManagerGUIWrapper.ParentForm = form;
		}

		public void StartPosting()
		{
			OnBeforePosting();
			BasePostManagerGUIWrapper.Post();
			RaiseOnPostingCancelled();
		}

		public void StopPostingOnNextIteration()
		{
			BasePostManagerGUIWrapper.StopPostingOnNextIteration();
		}

		void OnBeforePosting()
		{
			ShowInfoMessage(PostOperationDescription.IsEmpty ? "" : (PostOperationDescription + "\r\n\r\n"));
		}

		void OnStartingToPostNextObject(ZString objectCode, ZInt objectNumber)
		{
			ShowInfoMessage(GetTextBeforeNewObjectPosting(objectCode, objectNumber));
		}

		void OnObjectPostingFinished(ZString resultMessage, ZBool isObjectPosted)
		{
			ZString message = resultMessage;
			RaiseOnProgress();

			if (isObjectPosted)
			{
				ObjectsPosted++;
				message += System.Environment.NewLine + ProgressMessageForObjectPostedSuccessfully;
			}

			ShowInfoMessage(message.ToString() + System.Environment.NewLine + System.Environment.NewLine);
		}

		void BasePostManagerGUIWrapper_OnPostingFinished(object sender, EventArgs e)
		{
			ShowInfoMessage(ProgressMessageForSuccessfulPosting);
			RaiseOnAfterPosted();
		}

		#region GUI Wrapper Properties

		public ZInt NumberOfObjectsToPost
		{
			get { return BasePostManagerGUIWrapper.NumberOfObjectsToPost; }
		}

		public ZString SelectedPostingOptionName
		{
			get { return BasePostManagerGUIWrapper.SelectedPostingOptionName; }
		}

		protected ZString PostedObjectName
		{
			get { return BasePostManagerGUIWrapper.PostedObjectName; }
		}

		ZString fPostOperationDescription;
		ZString PostOperationDescription
		{
			get
			{
				if (fPostOperationDescription.IsEmpty)
				{
					fPostOperationDescription = Res.GetString("dc5e7175-267f-478a-a5a3-d70eac61d349", "Starting {0} batch posting.", PostedObjectName) + "\r\n";
					fPostOperationDescription += Res.GetString("daf47062-5f1f-4134-9ae2-9764cedf6bc8", "Posting option:{0}", SelectedPostingOptionName) + "\r\n";
					fPostOperationDescription += Res.GetString("2499cc6d-c772-4787-a239-46fd93aab2b0", "Selected {0} count:{1}", Grammar.Instance.Pluralize(PostedObjectName), NumberOfObjectsToPost.ToString());
				}
				return fPostOperationDescription;
			}
		}

		ZString ProgressMessageForObjectPostedSuccessfully
		{
			get { return Res.GetString("ae687f89-28c9-4a14-98b6-8311430fb2df", "{0} posted successfully.", PostedObjectName); }
		}

		ZString ProgressMessageForSuccessfulPosting
		{
			get { return Res.GetString("cd26d0c5-ae6a-4f3a-ba70-9d9fb345a08a", "{0} batch posting completed.", PostedObjectName); }
		}

		ZString GetTextBeforeNewObjectPosting(ZString currentObjectCode, ZInt currentObjectNumber)
		{
			return Res.GetString("df162211-9369-445e-bf6e-f9a84afaa714", "Processing the {0} {1} - {2} of {3}...", BasePostManagerGUIWrapper.PostedObjectName, currentObjectCode, currentObjectNumber.ToString(), NumberOfObjectsToPost.ToString());
		}

		#endregion

		#region Implementation

		[ReadOnly(true)]
		public ZInt ObjectsPosted
		{
			get { return fObjectsPosted; }
			set
			{
				fObjectsPosted = value;
				ObjectsPostedInfo.RefreshBinding();
			}
		}
		ZInt fObjectsPosted;

		public ZPropertyInfo ObjectsPostedInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(ObjectsPosted));
				return result;
			}
		}

		public string FormCaption
		{
			get { return BasePostManagerGUIWrapper.PostedObjectName + " " + Res.GetString("Accounting|BatchPostingFormCaptionSuffix", "Batch Posting"); }
		}

		public string ObjectsPostedLabelText
		{
			get
			{
				return Res.GetString("Accounting|BatchPostingBO|ObjectsPostedLabelTextSuffix", "{0} Posted:", Grammar.Instance.Pluralize(BasePostManagerGUIWrapper.PostedObjectName).ToUpper());
			}
		}

		protected BaseBatchInvoicingPostManagerGUIWrapper BasePostManagerGUIWrapper;

		#endregion
		#region Events

		void RaiseOnAfterPosted()
		{
			if (OnAfterPosted != null)
			{
				OnAfterPosted(this, null);
			}
		}

		void RaiseOnProgress()
		{
			if (OnProgress != null)
			{
				OnProgress(this, null);
			}
		}

		void RaiseOnPostingCancelled()
		{
			if (OnPostingCancelled != null)
			{
				OnPostingCancelled(this, null);
			}
		}

		void ShowInfoMessage(string infoMessage)
		{
			if (OnShowInfoMessage != null)
			{
				if (fSyncInvoke == null)
				{
					OnShowInfoMessage(infoMessage);
				}
				else
				{
					fSyncInvoke.BeginInvoke(new PostingEvent(OnShowInfoMessage), new object[] { infoMessage });
				}
			}
		}

		public ISynchronizeInvoke SyncInvoke
		{
			get { return fSyncInvoke; }
			set { fSyncInvoke = value; }
		}

		ISynchronizeInvoke fSyncInvoke;

		#endregion
	}
}
