using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business;

class DocumentWrapperHelper
{
	internal static DocumentWrapper GetFRSpecificDocumentWrapper(DataContext dataContext, BusinessObject businessObjectToWrap)
	{
		var wrapperStrongName = string.Empty;
		DocumentWrapper wrapper = null;
		if (dataContext == DataContext.IDD)
		{
			var entryHeader = businessObjectToWrap as CusEntryHeader;
			SupportedWrappersForIDD.TryGetValue(entryHeader?.IDDMessageType ?? ZString.Empty, out wrapperStrongName);
			if (!string.IsNullOrEmpty(wrapperStrongName))
			{
				wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(wrapperStrongName, entryHeader.IDDMessage, FRDocumentWrapperNamespace);
			}
		}
		else
		{
			DataContextToWrapperStrongName.TryGetValue(dataContext, out wrapperStrongName);
			wrapper = DocumentWrapperFactory.CreateWrapperWithoutException(wrapperStrongName, businessObjectToWrap, FRDocumentWrapperNamespace);
		}
		return wrapper;
	}

	readonly static ImmutableDictionary<DataContext, string> DataContextToWrapperStrongName = new Dictionary<DataContext, string>()
	{
		{ DataContext.FRSADH, $"{FRDocumentWrapperNamespace}.SADH.DocSADH" },
		{ DataContext.SADH, $"{FRDocumentWrapperNamespace}.SADH.DocSADH" },
		{ DataContext.LiquidationDetails, $"{FRDocumentWrapperNamespace}.LiquidationDetails.LiquidationDetailsWrapper" },
		{ DataContext.Statement, $"{FRDocumentWrapperNamespace}.Statement.DocStatement" },
		{ DataContext.TempStorageHeader, $"{FRDocumentWrapperNamespace}.TempStorageHeader.DocTempStorageHeader" },
		{ DataContext.EuNcts, $"{FRDocumentWrapperNamespace}.NCTS.NctsHeaderDocumentWrapper" },
		{ DataContext.T2L, $"{FRDocumentWrapperNamespace}.Transit.T2LDocWrapper" },
		{ DataContext.T2LF, $"{FRDocumentWrapperNamespace}.Transit.T2LFDocWrapper" },
	}.ToImmutableDictionary();

	readonly static ImmutableDictionary<ZString, string> SupportedWrappersForIDD = new Dictionary<ZString, string>()
		{
			{ DeltaIEResponseMessageSubTypeList.Codes.PrelodgedDeclarationAcceptance, $"{FRDocumentWrapperNamespace}.ImportDeclarationDocument.FRIDD426Wrapper" },
			{ DeltaIEResponseMessageSubTypeList.Codes.DeclarationAcceptance, $"{FRDocumentWrapperNamespace}.ImportDeclarationDocument.FRIDD428Wrapper" },
			{ DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification, $"{FRDocumentWrapperNamespace}.ImportDeclarationDocument.FRIDD429Wrapper" }
		}.ToImmutableDictionary();

	const string FRDocumentWrapperNamespace = "Enterprise.Customs.FR.DocumentWrappers";
}
