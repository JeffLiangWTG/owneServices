using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine
{
	public delegate void RatingDocPackBuilderCalllback(RatingDocPackBuilder builder);

	public sealed class RatingDocPackBuilder
	{
		public RatingDocPackBuilder(DocumentCommand command, IDocumentSupportable parent, RatingDocPackBuilderCalllback callback)
		{
			Argument.NotNull(command, "command");
			Argument.NotNull(parent, "parent");

			this.command = command;
			this.client = GetClient(command, parent.DocumentSupporter);

			this.DocPack = new RatingDocumentPack(command, this);
			this.DocPack.DocumentSupporter = parent.DocumentSupporter;
			this.DocPack.ForceBusinessObjectToLogAgainst((BusinessObject)parent);

			this.callback = callback;

			userControlProviderList = GetUserControlProviderList(parent);
			RebuildAll();
		}

		readonly UserControlProviderList userControlProviderList;

		static UserControlProviderList GetUserControlProviderList(IDocumentSupportable documentSupportableBizO)
		{
			if (documentSupportableBizO is IStmNoteParent noteParent)
			{
				var note = DocumentNote.RetrieveNote(noteParent);
				if (note != null)
				{
					var systemList = note.GetSystemDefinedFieldList();
					return new UserControlProviderList(systemList, note.UserDefinedFieldList);
				}
			}

			return null;
		}

		public void AddPages(StmMenuTemplatePivot pivot, IEnumerable<DocumentWrapper> wrappers)
		{
			Argument.NotNull(pivot, "pivot");
			Argument.NotNull(wrappers, "wrappers");

			var generator = new TemplateGenerator(pivot, client, DocPack.Language);
			var contact = ContactType.Find(command.SU_ContactType);
			var direction = GetDirection(command);

			foreach (var wrapper in wrappers)
			{
				try
				{
					IBODocDataProvider topLevelDataProvider = BODocDataProvider.Get((BusinessObject)Command.Parent);
					if (ZExpressionEvaluator.Evaluate(pivot.SI_MenuTemplateFilter, null, wrapper, topLevelDataProvider))
					{
						var evaluator = new FilterEvaluator(wrapper, topLevelDataProvider);
						var excelTemplate = generator.Generate(evaluator);

						var dataProviders = new DataProviderList(wrapper, topLevelDataProvider);

						var document = new Report(DocPack, excelTemplate, dataProviders, pivot.SI_DocumentTitle, contact, userControlProviderList, direction, pivot.SI_IsPasswordProtected, pivot.SI_IsPasswordProtectedForOpening, pivot.PK)
						{
							StTemplate = pivot.Template,
							DocumentDeliveredEventCode = pivot.DocType == null || pivot.DocType.RT_SE_NKDocumentReceivedEvent.IsEmpty ? (ZString)Events.DocumentDeliveredCode : pivot.DocType.RT_SE_NKDocumentReceivedEvent,
							DocTypeCode = pivot.DocType == null ? ZString.Empty : pivot.DocType.RT_DocType,
							MenuItem = pivot.MenuItem
						};

						// This builder's AddPages is called differently: when showing the documents in the Delivery Documents window,
						// and when printing the documents. In the first case, the list `wasIncludedInPrint` is empty.
						// So, the value of the tick box "Include in print" is decided by the pivot's default `Print` value.
						// In the 2nd case, `wasIncludedInPrint` contains the documents being shown in the 1st case.
						// We then use whatever the value from the UI.
						document.IncludedInPrint = (wasIncludedInPrint?.Count ?? 0) == 0
							? pivot.SI_PrintByDefault
							: WasIncludedInPrint(document);

						DocPack.Add(document);
					}
				}
				catch (ExpressionEvaluationException)
				{
				}
			}
		}

		public void AddPage(StmMenuTemplatePivot pivot, DocumentWrapper wrapper)
		{
			Argument.NotNull(pivot, "pivot");

			if (wrapper != null)
			{
				AddPages(pivot, new[] { wrapper });
			}
		}

		public void RebuildAll()
		{
			RecordWhichItemsWereIncludedInPrint();
			DocPack.RemoveAndDisposeAll();
			DocPack.LastTemplateGeneratorLanguage = DocPack.Language;
			callback(this);
		}

		void RecordWhichItemsWereIncludedInPrint()
		{
			wasIncludedInPrint = new Dictionary<Tuple<ZGuid, ZGuid>, ZBool>();
			foreach (IDeliverable doc in DocPack)
			{
				var key = GetIncludeInPrintKey(doc);
				if (!wasIncludedInPrint.ContainsKey(key))
				{
					wasIncludedInPrint.Add(key, doc.IncludedInPrint);
				}
			}
		}

		/// <summary>
		/// Returns true if the item was included in print before, or if it is a new item
		/// </summary>
		ZBool WasIncludedInPrint(IDeliverable document)
		{
			if (wasIncludedInPrint == null || !wasIncludedInPrint.TryGetValue(GetIncludeInPrintKey(document), out var itemWasIncludedInPrint))
			{
				itemWasIncludedInPrint = true;
			}
			return itemWasIncludedInPrint;
		}

		Tuple<ZGuid, ZGuid> GetIncludeInPrintKey(IDeliverable document)
		{
			ZGuid documentKey;
			if (document is Report report)
			{
				var parent = report.BODocDataProvider.ParentBusinessObject;
				documentKey = parent != null ? parent.PK : ZGuid.Empty;
			}
			else if (document is IeDoc ieDoc)
			{
				documentKey = ieDoc.UniqueKey;
			}
			else
			{
				documentKey = ZGuid.Empty;
			}

			return new Tuple<ZGuid, ZGuid>(document.MenuTemplatePivotPK, documentKey);
		}

		Dictionary<Tuple<ZGuid, ZGuid>, ZBool> wasIncludedInPrint;

		public void AddEDocsForMainMenuItem()
		{
			DocPack.AddEDocsToPack(command);
		}
		public void AddEDocsForSubMenuItem(DocumentCommand subCommand)
		{
			DocPack.AddEDocsToPack(subCommand);
		}

		public DocumentPack DocPack { get; private set; }

		static OrgHeader GetClient(IStmMenuItem menuItem, DocumentSupporter supporter)
		{
			IDocumentDeliveryContact contact = supporter.GetContactOrganisation(menuItem.SU_MenuName, ContactType.Find(menuItem.SU_ContactType), GetDirection(menuItem));
			return contact == null ? null : contact.OrgHeader as OrgHeader;
		}

		static DocumentDirection GetDirection(IStmMenuItem menuItem)
		{
			try
			{
				return (DocumentDirection)Enum.Parse(typeof(DocumentDirection), menuItem.SU_DocumentDirection);
			}
			catch (ArgumentException)
			{
				return DocumentDirection.ANY;
			}
		}

		public DocumentCommand Command { get { return command; } }
		readonly DocumentCommand command;

		public OrgHeader Client { get { return client; } }
		readonly OrgHeader client;

		readonly RatingDocPackBuilderCalllback callback;
	}
}
