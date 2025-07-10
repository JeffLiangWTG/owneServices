using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	/// <summary>
	/// A set of document packs for printing.
	/// </summary>
	public class DocumentPrintSet : PrintTask
	{
		public DocumentPrintSet(DocumentCommand command, UserControlProviderList userFieldList)
			: this(command, userFieldList, null)
		{
		}

		public DocumentPrintSet(DocumentCommand command, UserControlProviderList userFieldList, Guid[] documentSuppressionList)
			: base(command)
		{
			using (PrintTaskUIProvider.GetNewProgressNotificationUI(this.TaskSettings))
			{
				TaskSettings.OnStartDocumentGeneration();

				loader = new PrintTaskDocumentPackLoader(this, command, userFieldList, documentSuppressionList);
				loader.LoadAll();

				TaskSettings.OnEndDocumentGeneration();
			}
		}

		public DocumentPrintSet(DocumentCommand command)
			: base(command)
		{
		}

		protected DocumentPrintSet(DocumentCommand command, IEnumerable<DocumentPack> docPacks)
			: base(command, docPacks)
		{
		}

		readonly PrintTaskDocumentPackLoader loader;

		public new DocumentCommand ParentMenuCommand
		{
			get { return (DocumentCommand)base.ParentMenuCommand; }
		}

		#region Implementation

		public bool ContainsReports()
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i].Any(IsBizoAReport))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsBizoAReport(BusinessObject bizo)
			=> bizo is Report
			|| bizo is IStorageDocs
			|| bizo is IStorageDocsUnallocated
			|| bizo is IStorageFile // Without the IStorageFile check, we get the popup saying "cannot print as the required data is not present" when we try to deliver a DocPack that contains pre-existing (i.e. not being re-rendered on demand) eDocs.
			|| (bizo is IDeliverable deliverable
				&& deliverable.MenuItem != null
				&& deliverable.MenuItem.SU_MenuType == Core.Constants.StmMenuItemTypes.Forms);

		public List<string> ReasonsForEmptyPacks
		{
			get { return loader.ReasonsForEmptyPacks; }
		}

		internal UserControlProviderList UserFieldList
		{
			get { return loader.UserFieldList; }
		}

		protected override ReportSubjectLineMapping GenerateSubjectLineMappingForPrintTask(DocumentPack docPack)
		{
			ReportSubjectLineMapping reportSubjectMapping = null;
			ReportWithDeliverables replacementProvider = null;

			var command = docPack.StmMenuCommand as DocumentCommand;
			if (ParentMenuCommand.SU_IsDocPack || command == null)
			{
				command = ParentMenuCommand;
			}

			bool shouldUseJobNumber = !docPack.DeliverDocumentsInOneEmail || docPack.IsSingleAttachment;

			using (Res.TemporarilySwitchLanguage(docPack.Language))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(docPack.Language) ?? Culture.Default))
			{
				if (!command.SU_EmailSubjectLine.IsEmpty && !RegexProvider.OutermostMacroRegex.IsMatch(command.SU_EmailSubjectLine)) // No replacements needed.
				{
					replacementProvider = GetDefaultReplacementProvider(docPack);
					reportSubjectMapping = new ReportSubjectLineMapping(replacementProvider, command.SU_EmailSubjectLine);
				}
				else if (!command.SU_EmailSubjectLine.IsEmpty)
				{
					if (!command.SU_MenuDataContext.IsEmpty)
					{
						var userFieldList = docPack.DeliverDocumentsInOneEmail && loader == null ? docPack.UserFieldList : UserFieldList;
						Report report = command.GetReportForMenuDataContextReplacements(userFieldList);
						if (report != null)
						{
							replacementProvider = new ReportWithDeliverables(report, Enumerable.Empty<IDeliverable>());
						}
					}
					else
					{
						replacementProvider = GetDefaultReplacementProvider(docPack);
					}

					if (replacementProvider != null && shouldUseJobNumber)
					{
						string subjectLine = replacementProvider.Report.TranslateMacros(command.SU_EmailSubjectLine);
						reportSubjectMapping = new ReportSubjectLineMapping(replacementProvider, subjectLine);
					}
					else
					{
						reportSubjectMapping = new ReportSubjectLineMapping(null, RegexProvider.OutermostMacroRegex.Replace(command.SU_EmailSubjectLine, string.Empty));
					}
				}
				else if (!command.SU_MenuDataContext.IsEmpty)
				{
					IBODocDataProvider boDocDataProvider = command.GetDocWrapperForMenuDataContext();
					replacementProvider = GetDefaultReplacementProvider(docPack);
					reportSubjectMapping = new ReportSubjectLineMapping(replacementProvider, GlbCompany.CurrentCompany.GC_Name.ToString() + " (" + GlbBranch.CurrentBranch.GB_BranchName.ToString() + ") - " + command.SU_MenuName + (boDocDataProvider != null && shouldUseJobNumber ? " - " + boDocDataProvider.ToString() : string.Empty));
				}
				else if (docPack.DeliverDocumentsInOneEmail && !docPack.IsSingleAttachment)
				{
					reportSubjectMapping = new ReportSubjectLineMapping(replacementProvider, GlbCompany.CurrentCompany.GC_Name.ToString() + " (" + GlbBranch.CurrentBranch.GB_BranchName.ToString() + ") - " + command.SU_MenuName);
				}
				else
				{
					replacementProvider = GetDefaultReplacementProvider(docPack);
					reportSubjectMapping = new ReportSubjectLineMapping(replacementProvider, command.SU_EmailSubjectLine);
				}

				ZString subject;
				if (docPack.NumberOfDocumentNeedToBeConsolidated > 1 && !string.IsNullOrWhiteSpace(docPack.EmailSubjectForConsolidateReports))
				{
					subject = docPack.EmailSubjectForConsolidateReports;
				}
				else
				{
					subject = reportSubjectMapping.SubjectLine;
					subject = subject.Replace("\n", " ");
				}

				subject = subject.SubstringSafe(0, StmDeliveryGroupSchema.SB_EmailSubjectLine.MaxLength);
				reportSubjectMapping.SubjectLine = subject;

				return reportSubjectMapping;
			}
		}

		ReportWithDeliverables GetDefaultReplacementProvider(DocumentPack pack)
		{
			ReportWithDeliverables replacementProvider = null;
			var report = pack.GetFirstNonCoverSheetReport();

			if (report != null)
			{
				var eDocs = pack.OfType<IDeliverable>();
				replacementProvider = new ReportWithDeliverables(report, eDocs);
			}
			return replacementProvider;
		}

		public List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocument()
		{
			return ParentMenuCommand.Parent.DocumentSupporter.GenerateQuestionsToAskUsersBeforeRunningDocument(ParentMenuCommand);
		}

		protected override void CheckAvailableDeliveryOptions(DeliveryInstructions instructions)
		{
			base.CheckAvailableDeliveryOptions(instructions);

			if (ParentMenuCommand != null)
			{
				var command = ParentMenuCommand;
				if (command.Documents.Count > 0)
				{
					SetAllowedPrintTypes(instructions, false);

					foreach (StmMenuTemplatePivot template in command.Documents.ToArray()) //Use ToArray() to prevent changing collection during iteration.
					{
						PrintCopyType copyType = GetPrintCopyType(template.SI_PrintCopyType);
						switch (copyType)
						{
							case PrintCopyType.ALL:
								SetAllowedPrintTypes(instructions, true);
								break;

							case PrintCopyType.EML:
								instructions.AllowEmail = true;
								break;

							case PrintCopyType.FAX:
								instructions.AllowFax = true;
								break;

							case PrintCopyType.PRN:
								instructions.AllowPrint = true;
								break;

							default:
								break;
						}
					}
				}
			}
		}

		void SetAllowedPrintTypes(DeliveryInstructions instructions, bool allowed)
		{
			instructions.AllowEmail = allowed;
			instructions.AllowFax = allowed;
			instructions.AllowPrint = allowed;
		}

		PrintCopyType GetPrintCopyType(ZString typeString)
		{
			PrintCopyType copyType = PrintCopyType.ALL;
			if (!typeString.IsEmpty)
			{
				copyType = (PrintCopyType)Enum.Parse(typeof(PrintCopyType), typeString);
			}

			return copyType;
		}

		#endregion
	}
}
