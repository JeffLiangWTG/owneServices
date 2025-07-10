using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class SendsMessagesToCustomsGUI : Customs.GUI.SendsMessagesToCustomsGUI, ISendsMessagesToCustoms
	{
		public bool ContinueWithDoCPDecsAndRemergeEntry()
		{
			return GetConfirmation("Answer CPDec Questions?", "Because certain parts of the declaration have changed since the last merge/save, in order to perform this operation, the current merged lines will have to be discarded.  This means that old messages will be moved to the discarded messages tab and answered CPDec questions will be lost.  Are you sure you want to continue?") == DialogResult.OK;
		}

		public EXIT1MessageType GetExit1MessageType()
		{
			EXIT1MessageType result = new EXIT1MessageType();
			if (ZFormModaliser.ShowDialogAndDispose(new EXIT1MessageTypeForm(result)) == DialogResult.Cancel)
			{
				result = null;
			}
			return result;
		}

		public virtual ContinueWithSave GetAmendmentWithdrawalReason(CMRAmendmentWithdrawalReason amendmentWithdrawalReason)
		{
			if (!Globals.IsTest)
			{
				ZFormModaliser.ShowDialogAndDispose(new AmendmentReasonForm(amendmentWithdrawalReason));
			}

			return amendmentWithdrawalReason.IsCancelled ? ContinueWithSave.No : ContinueWithSave.Yes;
		}

		public virtual ContinueWithSave GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(JobDeclaration declaration, IEnumerable<CusEntryHeader> entries)
		{
			bool isOKToProceed = true;

			//Refactoring -> Joo: As we know which entry requires amendment, do not generate questions when merging. Generate them before they are presented to users
			CallGenerateQuestionsForOriginalOrAmendment(declaration);

			CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders = GetFilteredEntryHeaders(declaration, new List<CusEntryHeader>(entries).ToArray());

			if (!filteredEntryHeaders.AreAllCPDecQuestionsAnswered())
			{
				isOKToProceed = ShowCPQAForm(filteredEntryHeaders);
			}
			return isOKToProceed ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		public virtual ContinueWithSave GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(JobDeclaration declaration, Customs.Business.EntryMessageStatusFilterType filterType, bool forceDeclarationQuestionRegeneration, bool showCPQAFormAlways)
		{
			bool oKToProceed = true;
			if (declaration.MergeManager.RequiresMerge || !declaration.IsMergeDone)
			{
				oKToProceed = declaration.DoMerge();//Generate questions once Merge is done.
			}
			else if (forceDeclarationQuestionRegeneration)
			{
				CallGenerateQuestionsForOriginalOrAmendment(declaration);
			}

			CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders = GetFilteredEntryHeaders(declaration, filterType);

			if (oKToProceed && (showCPQAFormAlways || !filteredEntryHeaders.AreAllCPDecQuestionsAnswered()))
			{
				oKToProceed = ShowCPQAForm(filteredEntryHeaders);
			}
			return oKToProceed ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		public virtual ContinueWithSave GenerateWithdrawDecQuestionAndShowCPQAForm(JobDeclaration declaration)
		{
			declaration.CPQAManager.GenerateQuestionsForWithdrawal();
			CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders = GetFilteredEntryHeaders(declaration, Customs.Business.EntryMessageStatusFilterType.CanSendWithdraw);

			return ShowCPQAForm(filteredEntryHeaders) ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		public virtual ContinueWithSave GenerateWithdrawDecQuestionAndShowCPQAForm(JobDeclaration declaration, CusEntryHeader[] entriesToWithdraw)
		{
			declaration.CPQAManager.GenerateQuestionsForWithdrawal();
			CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders = GetFilteredEntryHeaders(declaration, entriesToWithdraw);

			return ShowCPQAForm(filteredEntryHeaders) ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		public virtual ContinueWithSave ShowCPQAFormForConsolidatedDeclaration(ConsolidatedDeclaration consolidatedDeclaration, JobDeclaration aggregatedDeclaration, bool showCPQAFormAlways)
		{
			bool isOKToProceed = true;
			var consolidatedEntry = aggregatedDeclaration.EntryHeader;
			using ((consolidatedEntry as IBusinessObjectInternals).ResumeValidationForAllDescendantsTemporarily())
			{
				var filteredEntryHeaders = new CusEntryHeaderMessageStatusFilteredCollection(new CusEntryHeader[] { consolidatedEntry }, aggregatedDeclaration.Factory);

				if (showCPQAFormAlways || !filteredEntryHeaders.AreAllCPDecQuestionsAnswered())
				{
					isOKToProceed = ShowCPQAForm(filteredEntryHeaders);
					if (isOKToProceed)
					{
						// write back answers
						consolidatedDeclaration.UpdateQuestionsFromAggregateDeclaration(aggregatedDeclaration);
					}
				}
			}

			return isOKToProceed ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		#region Implementation

		protected virtual CusEntryHeaderMessageStatusFilteredCollection GetFilteredEntryHeaders(JobDeclaration declaration, CusEntryHeader[] entries)
		{
			return new CusEntryHeaderMessageStatusFilteredCollection(entries, declaration.Factory);
		}

		protected virtual CusEntryHeaderMessageStatusFilteredCollection GetFilteredEntryHeaders(JobDeclaration declaration, Customs.Business.EntryMessageStatusFilterType filterType)
		{
			CusEntryHeaderMessageStatusFilteredCollection result = new CusEntryHeaderMessageStatusFilteredCollection(declaration.ActiveEntryHeaders);
			result.MessageStatusFilter = filterType;
			return result;
		}

		protected virtual void CallGenerateQuestionsForOriginalOrAmendment(JobDeclaration declaration)
		{
			declaration.CPQAManager.GenerateQuestionsForOriginalOrAmendment();
		}

		protected internal virtual bool ShowCPQAForm(CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders)
		{
			bool isOKToProceed = true;
			using (CPQAForm cPQAForm = GetCPQAForm(filteredEntryHeaders))
			{
				if (!Globals.IsTest)
				{
					ZFormModaliser.ShowDialogWithoutDispose(cPQAForm);
				}

				isOKToProceed = cPQAForm.IsOKToProceed;
			}
			return isOKToProceed;
		}

		protected virtual CPQAForm GetCPQAForm(CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders)
		{
			return new CPQAForm(filteredEntryHeaders);
		}

		protected DialogResult GetConfirmation(string caption, string message)
		{
			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
		}

		protected override ZForm GetBackDoorForSavingForm(Customs.Business.RequiredMessagesInformation detectionResult, Customs.Business.IDeferredAmendmentSavingOptions savingOptions)
		{
			var outturnSavingOptions = savingOptions as DeferredCusOutturnHeaderSavingOptions;
			if (outturnSavingOptions != null)
			{
				return new SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm(outturnSavingOptions);
			}
			return base.GetBackDoorForSavingForm(detectionResult, savingOptions);
		}

		protected override Customs.Business.SupervisorOverrides GetSupervisorOverrides(IBusiness businessEntity, string context)
		{
			return new IMDSupervisorOverrides(businessEntity, context);
		}

		#endregion
	}
}
