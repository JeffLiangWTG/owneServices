using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine
{
	public class DocumentRunner
	{
		public DocumentRunner(IDocumentRunnerParentForm parentForm = null, ModuleIdentifier moduleIDForSecurity = null, IDocumentEventsForMenu documentEventsForMenu = null, UserControlProviderList parentUserFieldList = null, UserControlProviderList parentSystemDefinedFieldList = null)
		{
			this.parentForm = parentForm;
			this.moduleIDForSecurity = moduleIDForSecurity;
			this.documentEventsForMenu = documentEventsForMenu;
			this.parentUserFieldList = parentUserFieldList;
			this.parentSystemDefinedFieldList = parentSystemDefinedFieldList;
		}

		readonly IDocumentRunnerParentForm parentForm;
#if DEBUG
		public
#endif
		ModuleIdentifier moduleIDForSecurity;
		readonly IDocumentEventsForMenu documentEventsForMenu;
		readonly UserControlProviderList parentUserFieldList;
		readonly UserControlProviderList parentSystemDefinedFieldList;
		internal static bool IsRunningBackgroundDelivery => documentDelivery != null;
		internal static StmDocumentDelivery DocumentDelivery => documentDelivery;
		[ThreadStatic]
		static StmDocumentDelivery documentDelivery;
		public static IDisposable BackgroundDelivery(StmDocumentDelivery backgroundDocumentDelivery)
		{
			return new DisposableAction(
				() => documentDelivery = backgroundDocumentDelivery,
				() => documentDelivery = null);
		}

		#region Run

		public bool Run(DocumentCommand command)
		{
			try
			{
				return RunCore(command);
			}
			catch (ExternalStorageException ex)
			{
				ex.ReportExceptionForDeveloper();

				var caption = Res.GetString("7107B599-2A6B-4D8C-B00F-AAC63F59A871", "Failed to access eDocs");
				Globals.Message.ShowError(ex.UnableToAccessStorageFriendlyMessage, caption);
			}
			catch (DataContextIsInvalidException ex)
			{
				var caption = Res.GetString("D0F435B6-0E3C-44EE-8550-E263E6289A84", "DataContext is invalid");
				Globals.Message.ShowError(ex.Message, caption);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var rethrow = true;

				if (parentForm != null)
				{
					try
					{
						rethrow = !parentForm.HandleSaveException(ex);
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
				}

				if (rethrow)
				{
					var message = $"Could not deliver the following document menu item:\r\n\tMenu Path: {command.SU_MenuPath}\r\n\tMenu Name: {command.SU_MenuName}\r\n\tSystem Defined: {command.SU_IsSystemDefined.ToYN()}\r\n\r\nThe following error occurred:\r\n{ex.Message}\r\nSee inner exception for details.";
					throw new DocumentEngineException(message, ex);
				}
			}

			return false;
		}

		bool RunCore(DocumentCommand command)
		{
			using (command.Parent?.DocumentSupporter.InitialiseFetchStrategy())
			using (var guiManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
			using (IsRunningBackgroundDelivery ? null : command.FromMenu())
			{
				guiManager.Initialise(command.Parent as ICreditControlledBusinessObject);
				command.CreditControlledDocumentDeliveryGUIManager = guiManager;
				var continueRunning = CheckDataState(command) && CheckDocumentPrintRequested(command);
				if (continueRunning)
				{
					SetCursorWait();
					try
					{
						using (var documentPrintSet = GetDocumentPrintSet(command))
						{
							SetCursorPrevious();

							if (!documentPrintSet.UseStreamMode)
							{
								continueRunning = documentPrintSet.ContainsReports();
							}
							if (continueRunning)
							{
								SecurityCheckpoint runDocumentCheckpoint;
								continueRunning = CheckIfPrintingIsAllowed(command, out runDocumentCheckpoint) && BeginRun(documentPrintSet, runDocumentCheckpoint);
							}
							else
							{
								ShowReasonsForNotPrinting(documentPrintSet);
							}
						}
					}
					catch
					{
						continueRunning = false;
						SetCursorPrevious();
						throw;
					}
				}
				return continueRunning;
			}
		}

		bool BeginRun(DocumentPrintSet documentPrintSet, SecurityCheckpoint runDocumentCheckpoint)
		{
			var continueToPrint = true;

			DocumentPrintedEventHandler notifyPrePreviewed = null;
			DocumentPrintedEventHandler notifyPrePrinted = null;

			try
			{
				if (documentEventsForMenu != null)
				{
					notifyPrePreviewed = (object sender, DocumentPrintedEventArgs e) => documentEventsForMenu.NotifyDocumentPrePreviewed(e);
					documentPrintSet.DocumentPrePreviewed += notifyPrePreviewed;

					notifyPrePrinted = (object sender, DocumentPrintedEventArgs e) => documentEventsForMenu.NotifyDocumentPrePrinted(e);
					documentPrintSet.DocumentPrePrinted += notifyPrePrinted;
				}

				continueToPrint = CheckUserMessages(documentPrintSet);
				if (continueToPrint)
				{
					RunDocumentPrintSet(documentPrintSet, runDocumentCheckpoint);
				}
			}
			finally
			{
				if (notifyPrePreviewed != null)
				{
					documentPrintSet.DocumentPrePreviewed -= notifyPrePreviewed;
				}

				if (notifyPrePrinted != null)
				{
					documentPrintSet.DocumentPrePrinted -= notifyPrePrinted;
				}
			}

			return continueToPrint;
		}

		DocumentPrintSet GetDocumentPrintSet(DocumentCommand item)
		{
#if DEBUG
			ThrowExceptionInGetDocumentPrintSetForTesting?.Invoke();
#endif

			if (item.Parent != null)
			{
				var iSupportCustomizedDocumentPrintSet = item.ParentDocumentSupporter as ISupportCustomizedDocumentPrintSet;
				if (iSupportCustomizedDocumentPrintSet != null && iSupportCustomizedDocumentPrintSet.ShouldCustomizedDocumentPrintSet(item))
				{
					return (DocumentPrintSet)iSupportCustomizedDocumentPrintSet.GetCustomizedDocumentPrintSet(item);
				}
			}
			return new DocumentPrintSet(item, new UserControlProviderList(parentSystemDefinedFieldList, parentUserFieldList));
		}

		void RunDocumentPrintSet(DocumentPrintSet documentPrintSet, SecurityCheckpoint runDocumentCheckpoint)
		{
			var modifyDocumentCheckPoint = GetModifyCheckPoint(documentPrintSet.ParentMenuCommand, runDocumentCheckpoint);
			var result = DeliveryInstructionDestination.None;
#if DEBUG
			if (ShouldActuallyRunDocumentSetForTesting)
#endif
			{
				result = documentPrintSet.Run(modifyDocumentCheckPoint);
			}

			NotifyDocumentPrinted(documentPrintSet, result);

#if DEBUG
			if (Globals.IsTest && !documentPrintSet.UseStreamMode)
			{
				for (int index = 0; index < documentPrintSet.Count; index++)
				{
					DocumentPack documentPack = documentPrintSet[index];

					foreach (IDeliverable deliverable in documentPack)
					{
						Report report = deliverable as Report;

						if (report != null)
						{
							string pivotTitle = report.Name;
							string templateName = report.Template.TemplateName;
							UserControlProviderList userDefinedFieldValueList = report.UserDefinedFieldValueList;

							LastRunReportInfosForTesting.Add(new Testing.ReportRunInfoForTesting(report.Name, report.Template.TemplateName, userDefinedFieldValueList));
						}
					}
				}
			}
#endif
		}

		void NotifyDocumentPrinted(DocumentPrintSet documentPrintSet, DeliveryInstructionDestination destination)
		{
			if (documentEventsForMenu != null)
			{
				if (documentPrintSet.DeliveryInstructionsForDeliveryForm == null)
				{
					documentEventsForMenu.NotifyDocumentPrinted(new DocumentPrintedEventArgs(destination, documentPrintSet.ParentMenuCommand, false, documentPrintSet));
				}
				else
				{
					documentEventsForMenu.NotifyDocumentPrinted(new DocumentPrintedEventArgs(destination, documentPrintSet.ParentMenuCommand, documentPrintSet.DeliveryInstructionsForDeliveryForm.IsDraft, documentPrintSet));
				}
			}
		}

		#endregion

		#region Cursor

		void SetCursorWait()
		{
			parentForm?.SetCursorWait();
		}

		void SetCursorPrevious()
		{
			parentForm?.SetCursorPrevious();
		}

		#endregion

		#region Checks

		public static bool CheckDataState(DocumentCommand command)
		{
			var continueRunning = true;

			if (command.Parent != null)
			{
				var dataState = command.Parent.DocumentSupporter.GetDataStateBeforeRun(command);
				if (dataState != null && !dataState.IsValid)
				{
					if (!string.IsNullOrEmpty(dataState.ErrorMessage) && !dataState.ErrorMessage.Contains(SecurityLogin.CancelledText))
					{
						Globals.Message.ShowError(dataState.ErrorMessage, Res.GetString("8a577c8c-fc99-4dea-adfb-d70ef103f37e", "Unable To Run This Document"));
					}

					continueRunning = false;
				}
			}

			return continueRunning;
		}

		bool CheckDocumentPrintRequested(DocumentCommand command)
		{
			var continueRunning = true;

			if (documentEventsForMenu != null)
			{
				documentEventsForMenu.NotifyDocumentPrintRequested(command);
				continueRunning = !documentEventsForMenu.CancelPrintRequest;
			}

			return continueRunning;
		}

		internal bool CheckIfPrintingIsAllowed(DocumentCommand command, out SecurityCheckpoint runDocumentCheckpoint)
		{
			var continueRunning = true;

			runDocumentCheckpoint = GetRunDocumentCheckPoint(command);
			if (runDocumentCheckpoint != Env.Security.None && !runDocumentCheckpoint.IsAllowed)
			{
				continueRunning = false;
				runDocumentCheckpoint.ShowError();
			}

			return continueRunning;
		}

		bool CheckUserMessages(DocumentPrintSet set)
		{
			var continueRunningIfNoneOrAtLeastOneQuestionIsAnsweredYes = true;

			foreach (DocumentSupporterQuestion question in set.GenerateQuestionsToAskUsersBeforeRunningDocument())
			{
				continueRunningIfNoneOrAtLeastOneQuestionIsAnsweredYes = false;

				var defaultRespose = question.DefaultResponse == AnswerType.No ? ZDialogResult.No : ZDialogResult.Yes;
				var icon = question.QuestionType == QuestionType.Warning ? ZMessageBoxIcon.Warning : ZMessageBoxIcon.Question;

				var dialogResult = Globals.Message.Show(question.QuestionText, question.Title, ZMessageBoxButtons.YesNo, icon, defaultRespose);
				if (dialogResult == ZDialogResult.Yes)
				{
					continueRunningIfNoneOrAtLeastOneQuestionIsAnsweredYes = true;
					break;
				}
			}

			return continueRunningIfNoneOrAtLeastOneQuestionIsAnsweredYes;
		}

		internal void ShowReasonsForNotPrinting(DocumentPrintSet documentPrintSet)
		{
			if (documentPrintSet.ReasonsForEmptyPacks.Count > 0)
			{
				var builder = new ZStringBuilder();
				foreach (var item in documentPrintSet.ReasonsForEmptyPacks)
				{
					if (item != null)
					{
						builder.Append(item);
					}
				}
				Globals.Message.ShowWarning(builder.ToStringWithNewLineBetweenAppends(), Res.GetString("63b573df-a85b-42f9-ae87-97f9a38a69ed", "No Document to Print"));
			}
		}

		#endregion

		#region SecurityCheckpoints

		SecurityCheckpoint GetRunDocumentCheckPoint(DocumentCommand command)
		{
			var result = Env.Security.None;

			if (command.SU_IsPublished && moduleIDForSecurity != null)
			{
				using (var module = ObjectFactory.Get<IModuleFactory>().Create(moduleIDForSecurity))
				{
					if (module != null && module.SecurityCheckpoint != Env.Security.None)
					{
						var parentSecurityCheckpoint = Env.Security.FindOrCreateDocumentsCheckpoint(moduleIDForSecurity, module.SecurityCheckpoint);
						result = Env.Security.FindOrCreateDocumentCheckpoint(command.PK.ToGuid(), command.SU_MenuNameMultilingual, moduleIDForSecurity, parentSecurityCheckpoint);
					}
				}
			}

			return result;
		}

		SecurityCheckpoint GetModifyCheckPoint(DocumentCommand command, SecurityCheckpoint runDocumentCheckpoint)
		{
			var result = Env.Security.None;

			if (command.SU_IsPublished && moduleIDForSecurity != null)
			{
				using (var module = ObjectFactory.Get<IModuleFactory>().Create(moduleIDForSecurity))
				{
					if (module != null && module.SecurityCheckpoint != Env.Security.None)
					{
						result = Env.Security.FindOrCreateDocumentOverrideCheckpoint(command.PK.ToGuid(), ResString.GetMultilingualString("AABFF7D7-1671-42BC-8838-E9F0C791FD7D", "Modify"), moduleIDForSecurity, runDocumentCheckpoint);
					}
				}
			}

			return result;
		}

		#endregion

		#region Testing

		// Currently tested in ZDocumentMenuItem

#if DEBUG
		public DocumentPrintSet GetDocumentPrintSetForTesting(DocumentCommand item) { return GetDocumentPrintSet(item); }
		public bool ShouldActuallyRunDocumentSetForTesting = true;
		public Action ThrowExceptionInGetDocumentPrintSetForTesting;
		public readonly List<Testing.ReportRunInfoForTesting> LastRunReportInfosForTesting = new List<Testing.ReportRunInfoForTesting>();
#endif
		#endregion
	}
}
