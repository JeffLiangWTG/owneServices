using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class SadDocumentSupporterConfigurator
{
	public SadDocumentSupporterConfigurator(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	readonly CusEntryHeader entryHeader;

	public DocumentSupporterDataState ConfigureDataState(DocumentSupporterDataState dataState, IStmMenuItem menuItem)
	{
		if (dataState.IsValid && menuItem != null)
		{
			var sadDocumentSupporter = entryHeader.Declaration.DocumentSupporter.SadDocumentSupporter;

			sadDocumentSupporter.BGMReferenceToPrint = entryHeader.CH_BGMReference;
			var menuName = menuItem.SU_MenuName;

			dataState = ConfigureSadhC88(dataState, sadDocumentSupporter, menuName);
		}

		return dataState;
	}

	#region Implementation

	DocumentSupporterDataState ConfigureSadhC88(DocumentSupporterDataState dataState, JobDeclarationSadDocumentSupporter sadDocumentSupporter, ZString menuName)
	{
		if (menuName == DocumentWrapperConstants.MenuItemName.SADHC88.SADH)
		{
			return ConfigureUsingExternalConfigurator(dataState, sadDocumentSupporter);
		}

		if (sadHLayoutStyleDictionary.ContainsKey(menuName))
		{
			sadDocumentSupporter.LayoutStyle = sadHLayoutStyleDictionary[menuName];
		}

		return dataState;
	}

	DocumentSupporterDataState ConfigureUsingExternalConfigurator(DocumentSupporterDataState documentSupporterDataState, JobDeclarationSadDocumentSupporter sadDocumentSupporter)
	{
		var entryHeaderDocumentSupporterConfigurator = ObjectFactory.Get<IITCusEntryHeaderDocumentSupporterConfigurator>();

		var cancelEventArgs = entryHeaderDocumentSupporterConfigurator.Configure(sadDocumentSupporter);
		documentSupporterDataState.IsValid = !cancelEventArgs.Cancel;

		return documentSupporterDataState;
	}

	readonly ImmutableDictionary<string, string> sadHLayoutStyleDictionary = new Dictionary<string, string>()
	{
		{ DocumentWrapperConstants.MenuItemName.SADHC88.CopyC,  SADExportCopyNumberList.Codes.EsemplareNonValidoAiFiniFiscali },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy1, SADExportCopyNumberList.Codes.EsemplarePerIlPaeseDiSpedizioneEsportaz },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy3, SADExportCopyNumberList.Codes.EsemplarePerLoSpeditoreEsportatore },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy3A, SADExportCopyNumberList.Codes.EsemplarePerLaRestituzioneDeiDiritti },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy3B, SADExportCopyNumberList.Codes.EsemplarePerAbbuonoImposte },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy6, SADImportCopyNumberList.Codes.EsemplarePerIlPaeseDiDestinazione },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy7, SADImportCopyNumberList.Codes.EsemplarePerLaStatisticaPaeseDiDestinaz },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy8, SADImportCopyNumberList.Codes.EsemplarePerIlDestinatario },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.Copy8R, SADImportCopyNumberList.Codes.EsemplarePerIlRiscontro },
		{ DocumentWrapperConstants.MenuItemName.SADHC88.CopyI, SADExportCopyNumberList.Codes.UsoInterno },
	}.ToImmutableDictionary();

	#endregion
}
