using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;

namespace Enterprise.DocumentEngine
{
	public class DocumentCommandValidation : StmMenuItemBaseValidation
	{
		public DocumentCommandValidation(DocumentCommand parent)
			: base(parent)
		{
		}

		#region Parent

		new DocumentCommand Parent
		{
			get { return (DocumentCommand)base.Parent; }
		}

		#endregion

		#region CheckSU_EmailSubjectLine

		protected override void CheckSU_EmailSubjectLine()
		{
			base.CheckSU_EmailSubjectLine();

			if (Parent.EmailSubjectAndMenuDataContextReadOnlyBasedOnDocumentSetup && !Parent.SU_IsSystemDefined)
			{
				Parent.SU_EmailSubjectLineInfo.AddMessageError(Res.GetString("b3ca1bed-48c3-4afe-8860-6b83c675d954", "This field is only used when the document has at least one template or is flagged as a Doc Pack."));
			}
		}

		#endregion

		#region CheckSU_FilterList

		protected override void CheckSU_FilterList()
		{
			base.CheckSU_FilterList();

			if (!Parent.SU_FilterList.IsEmpty)
			{
				string message;
				if (!ZExpressionEvaluator.IsValidFilter(Parent.SU_FilterList, false, out message))
				{
					Parent.SU_FilterListInfo.AddError(message);
				}
			}
		}

		#endregion

		#region CheckSU_IsModifiable

		protected override void CheckSU_IsModifiable()
		{
			base.CheckSU_IsModifiable();
			if (!Parent.SU_SupportsVisualisation && Parent.SU_IsModifiable)
			{
				Parent.SU_IsModifiableInfo.AddError(Res.GetString("8d95f216-3134-4afa-acf1-bb8a8442efba", "This menu item doesn't support visualization."));
			}
		}

		#endregion

		#region CheckSU_IncludeDocInArchive

		protected override void CheckSU_IncludeDocInArchive()
		{
			base.CheckSU_IncludeDocInArchive();
			ListValidation.ErrorIfInvalidCode(Parent.SU_IncludeDocInArchiveInfo, Parent.SU_IncludeDocInArchiveList);

			if (!Parent.SU_IncludeDocInArchiveInfo.HasErrors())
			{
				if (Parent.SU_IsDocPack && (Parent.SU_IncludeDocInArchive == ArchiveConstants.IncludeDocInArchiveCodes.Yes))
				{
					Parent.SU_IncludeDocInArchiveInfo.AddError(Res.GetString("20dd4d8c-c047-4bdb-8b6a-4c99a5179979", "A Doc Pack cannot be selected for archiving. Please choose individual documents instead."));
				}
			}
		}

		#endregion

		#region CheckSU_MenuDataContext

		protected override void CheckSU_MenuDataContext()
		{
			base.CheckSU_MenuDataContext();

			if (Parent.EmailSubjectAndMenuDataContextReadOnlyBasedOnDocumentSetup && !Parent.SU_IsSystemDefined)
			{
				Parent.SU_MenuDataContextInfo.AddMessageError(Res.GetString("b3ca1bed-48c3-4afe-8860-6b83c675d954", "This field is only used when the document has at least one template or is flagged as a Doc Pack."));
			}
			else if (!Parent.SU_EmailSubjectLine.IsEmpty && RegexProvider.OutermostMacroRegex.IsMatch(Parent.SU_EmailSubjectLine) && Parent.SU_MenuDataContext.IsEmpty)
			{
				if (Parent.Documents.Count < 1)
				{
					Parent.SU_MenuDataContextInfo.AddError(Res.GetString("210fbe9d-8a4d-454a-860d-4618ca95d5d8", "Please select an Email Subject Data Context for your customized Email Subject Line replacements."));
				}
			}
			else if (!Parent.SU_MenuDataContext.IsEmpty && !Parent.SU_MenuDataContextInfo.ReadOnly && (!Parent.IsInDatabase || Parent.SU_MenuDataContextInfo.HasChanges))
			{
				if (Parent.SU_MenuDataContextList.IndexOfCode(Parent.SU_MenuDataContext) < 0)
				{
					Parent.SU_MenuDataContextInfo.AddError(Res.GetString("e4a844de-c026-4909-9aa9-d4fa37bd05e5", "Please select a valid Email Subject Data Context for your customized Email Subject Line replacements."));
				}
			}
		}

		#endregion

		#region CheckSU_MenuShortcut

		protected override void CheckSU_MenuShortcut()
		{
			base.CheckSU_MenuShortcut();

			if (!Parent.SU_MenuShortcut.IsEmpty && !ObjectFactory.Get<IShortcuts>().IsValidShortcut(Parent.SU_MenuShortcut))
			{
				Parent.SU_MenuShortcutInfo.AddError(Res.GetString("08df3fc9-cddc-48c5-a879-683879d39e94", "Invalid short cut. Please select one from the list."));
			}
		}

		#endregion

		#region CheckSU_DefaultAttachmentType

		protected override void CheckSU_DefaultAttachmentType()
		{
			base.CheckSU_DefaultAttachmentType();

			ListValidation.ErrorIfInvalidCode(Parent.SU_DefaultAttachmentTypeInfo, Parent.AttachmentTypes, ResString.GetMultilingualString("2DDCE67E-A88F-4CDA-ADD8-9E4FA3D77249", "Default Attachment Type"));
		}

		#endregion

	}
}
