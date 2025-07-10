using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationDocumentSupporter : EU.Business.Declaration.JobDeclarationDocumentSupporter
{
	public JobDeclarationDocumentSupporter(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override DataContext[] GetSupportedDataContexts()
	{
		var result = new List<DataContext>(base.GetSupportedDataContexts())
		{
			DataContext.ITSadAttachment
		};
		return result.ToArray();
	}

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		var entryHeaders = EntryHeaders;
		switch (dataContext)
		{
			case DataContext.SADH when commandBeingRun.SU_MenuName == DocumentWrapperConstants.MenuItemName.SADHC88.SADH:
				entryHeaders = new[] { SadDocumentSupporter.GetSelectedEntryHeader() }.WhereNotNull();
				return GetWrappers(dataContext, entryHeaders);

			case DataContext.SADH:
				return GetWrappers(dataContext, entryHeaders);

			case DataContext.ITSadAttachment:
				ConfigureAttachmentPrintingSupporter(commandBeingRun);
				return GetWrappers(dataContext, entryHeaders.Where(entryHeader => entryHeader.MergedLines.Cast<CusEntryLine>().Any(entryLine => entryLine.AttachmentPrintingSupporter.RequiresAttachment)));
		}
		return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}

	protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
	{
		var documentSupporterDataState = base.GetDataStateBeforeRunCore(commandAboutToBeRun);

		if (documentSupporterDataState.IsValid && commandAboutToBeRun != null && commandAboutToBeRun.SU_MenuName == DocumentWrapperConstants.MenuItemName.SADHC88.SADH)
		{
			if (EntryHeaders.Any())
			{
				sadDocumentSupporter = null;

				var cancelEventArgs = Declaration.PerformOnGetEntryToPrintSadHC88();
				documentSupporterDataState.IsValid = !cancelEventArgs.Cancel;
			}
			else
			{
				documentSupporterDataState = new DocumentSupporterDataState(false, Res.GetString("027A7693-11D7-4272-B6A5-4188833487DF", "No entries are available"));
			}
		}

		return documentSupporterDataState;
	}

	DocumentWrapper[] GetWrappers(DataContext dataContext, IEnumerable<CusEntryHeader> entryHeaders)
	{
		var wrapperStrongName = supportedWrappers[dataContext];
		var documentWrappers = new List<DocumentWrapper>();
		foreach (var entry in entryHeaders)
		{
			var wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(wrapperStrongName, entry);
			if (wrapper != null)
			{
				documentWrappers.Add(wrapper);
			}
		}
		return documentWrappers.ToArray();
	}

	readonly ImmutableDictionary<DataContext, ZString> supportedWrappers = new Dictionary<DataContext, ZString>()
	{
		{ DataContext.SADH, DocumentWrapperConstants.FullNames.SADH },
		{ DataContext.ITSadAttachment, DocumentWrapperConstants.FullNames.ITSadAttachment },
	}.ToImmutableDictionary();

	void ConfigureAttachmentPrintingSupporter(IStmMenuItem commandBeingRun)
	{
		var entryHeaders = EntryHeaders;
		entryHeaders.SelectMany(x => x.MergedLines).ForEach(x => x.UseEadAttachmentPrintingSupporter = commandBeingRun.SU_MenuName == DocumentWrapperConstants.MenuItemName.EAD);
	}

	IEnumerable<CusEntryHeader> EntryHeaders => Declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();

	public JobDeclarationSadDocumentSupporter SadDocumentSupporter => sadDocumentSupporter ?? (sadDocumentSupporter = new JobDeclarationSadDocumentSupporter(Declaration));
	JobDeclarationSadDocumentSupporter sadDocumentSupporter;
}
