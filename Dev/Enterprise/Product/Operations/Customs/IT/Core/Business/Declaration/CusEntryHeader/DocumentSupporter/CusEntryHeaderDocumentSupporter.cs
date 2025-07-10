using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryHeaderDocumentSupporter : EU.Business.Declaration.CusEntryHeaderDocumentSupporter
{
	public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	public override CargoWise.Definitions.BusinessContext BusinessContext => CargoWise.Definitions.BusinessContext.CusEntryHeader;

	public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		ConfigureAttachmentPrintingSupporter(commandBeingRun);

		switch (dataContext)
		{
			case DataContext.SADH:
				return GetWrappers(dataContext);

			case DataContext.ITSadAttachment when RequiresAttachment:
				return GetWrappers(dataContext);

			default:
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun)
					.WhereNotNull()
					.ToArray();
		}
	}

	public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
	{
		var documentSupporterDataState = base.GetDataStateBeforeRun(commandAboutToBeRun);

		return new SadDocumentSupporterConfigurator(EntryHeader)
			.ConfigureDataState(documentSupporterDataState, commandAboutToBeRun);
	}

	protected override DataContext[] GetSupportedDataContexts()
	{
		var result = new List<DataContext>(base.GetSupportedDataContexts())
		{
			DataContext.ITSadAttachment
		};
		return result.ToArray();
	}

	#region Implementation

	DocumentWrapper[] GetWrappers(DataContext dataContext)
	{
		var wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(supportedWrappers[dataContext], EntryHeader);

		return new DocumentWrapper[] { wrapper }.WhereNotNull().ToArray();
	}

	bool RequiresAttachment => EntryHeader.MergedLines.Any(x => x.AttachmentPrintingSupporter.RequiresAttachment);

	void ConfigureAttachmentPrintingSupporter(IStmMenuItem commandBeingRun)
	{
		EntryHeader.MergedLines.ForEach(line => line.UseEadAttachmentPrintingSupporter = commandBeingRun.SU_MenuName == DocumentWrapperConstants.MenuItemName.EAD);
	}

	readonly ImmutableDictionary<DataContext, ZString> supportedWrappers = new Dictionary<DataContext, ZString>()
	{
		{ DataContext.SADH, DocumentWrapperConstants.FullNames.SADH },
		{ DataContext.ITSadAttachment, DocumentWrapperConstants.FullNames.ITSadAttachment },
	}.ToImmutableDictionary();

	#endregion
}
