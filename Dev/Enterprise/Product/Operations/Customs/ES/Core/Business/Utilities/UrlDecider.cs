using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Registry;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business;

public class UrlDecider
{
	public UrlDecider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));

		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
	}

	readonly CusEntryHeader entryHeader;
	readonly CusEntryInstruction entryInstruction;

	public ZString GetUrl()
	{
		ZString mrn = entryHeader.MovementReferenceNumber;
		bool isT2C = entryInstruction.IsT2C;
		bool isT2L = entryInstruction.IsT2L;

		if (entryHeader.IsImport)
		{
			return GetImportUrl(mrn, isT2C, isT2L);
		}
		else if (entryHeader.IsExport)
		{
			return GetExportUrl(mrn, isT2C, isT2L);
		}

		return ZString.Empty;
	}

	string GetImportUrl(ZString mrn, bool isT2C, bool isT2L)
	{
		var t2cMRN = entryHeader.T2CMovementReferenceNumber;
		var djpMRN = entryHeader.ZG_DJPMRN;
		var isPOUS2 = entryHeader.ZG_POUSVersion > POUSVersionCodes.POUS;
		var isH2 = entryInstruction.IsH2;
		var isUCC6 = entryHeader.IsUCC6;

		if (isT2C && !t2cMRN.IsEmpty)
		{
			return ComposeUrl(ESCustomsDataRegistry.Instance.T2cClearanceStatusQueryUrl.Value, t2cMRN);
		}
		else if (isT2L && isPOUS2 && !t2cMRN.IsEmpty)
		{
			return ComposeUrl(ESCustomsDataRegistry.Instance.T2lExpeditionAndReceptionStatusQueryUrl.Value, t2cMRN);
		}
		else if (isT2L && !isPOUS2 && !mrn.IsEmpty)
		{
			return ComposeUrl(ESCustomsDataRegistry.Instance.T2lNonUCCReceptionStatusQueryUrl.Value, mrn);
		}
		else if (!djpMRN.IsEmpty)
		{
			return isUCC6 ? ComposeUrl(ESCustomsDataRegistry.Instance.ImportH1DJPStatusQueryUrl.Value, djpMRN) : ComposeUrl(ESCustomsDataRegistry.Instance.ImportDjpStatusQueryUrl.Value, djpMRN);
		}
		else if (isH2 && !mrn.IsEmpty)
		{
			return ComposeUrl(GetImportH2Url(), mrn);
		}
		else if (!isT2C && !isT2L && !mrn.IsEmpty & !isH2)
		{
			return isUCC6 ? ComposeUrlH1(GetImportH1LUrl(), mrn) : ComposeUrl(GetImportNotT2CNorT2LUrl(), mrn);
		}

		return ZString.Empty;
	}

	string GetImportH1LUrl() => entryHeader.Declaration.DestinationStateIsCanaryIsland
										? ESCustomsDataRegistry.Instance.ImportH1StatusCanaryIslandsQueryUrl.Value
										: ESCustomsDataRegistry.Instance.ImportH1StatusQueryUrl.Value;

	string GetImportNotT2CNorT2LUrl()
	{
		if (entryHeader.CH_EntryStatus == MessageProcessorConstants.EntryStatusCodes.IncompletePreDeclaration)
		{
			return ESCustomsDataRegistry.Instance.ImportPdiStatusQueryUrl.Value;
		}
		else if (entryHeader.Declaration.DestinationStateIsCanaryIsland)
		{
			return ESCustomsDataRegistry.Instance.ImportStatusCanaryIslandsQueryUrl.Value;
		}
		else
		{
			return ESCustomsDataRegistry.Instance.ImportStatusQueryUrl.Value;
		}
	}

	string GetImportH2Url()
	{
		if (entryHeader.Declaration.IsCustomOfficeCanaryIsland)
		{
			return ESCustomsDataRegistry.Instance.DvdStatusCanaryIslandsQueryUrl.Value;
		}
		else
		{
			return ESCustomsDataRegistry.Instance.DvdStatusQueryUrl.Value;
		}
	}

	string GetExportUrl(ZString mrn, bool isT2C, bool isT2L)
	{
		bool isEXS = entryInstruction.IsEXS;
		if (!mrn.IsEmpty)
		{
			if (isT2L)
			{
				return ComposeUrl(ESCustomsDataRegistry.Instance.T2lExpeditionAndReceptionStatusQueryUrl.Value, mrn);
			}
			else if (isEXS)
			{
				return ComposeUrl(ESCustomsDataRegistry.Instance.ExsStatusQueryUrl.Value, mrn);
			}
			else if (!isT2C && !isT2L && mrn.Length > 10)
			{
				return ComposeUrl(ESCustomsDataRegistry.Instance.ExportStatusQueryUrl.Value, mrn);
			}
		}

		return ZString.Empty;
	}

	static string ComposeUrl(ZString url, ZString mrn)
		=> url.Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, mrn);

	static string ComposeUrlH1(ZString url, ZString mrn)
		=> url.Replace(CustomsWebsiteUrlCodes.AnyoInRegistryUrl, mrn.Substring(0,2))
			  .Replace(CustomsWebsiteUrlCodes.PaisInRegistryUrl, mrn.Substring(2,2))
			  .Replace(CustomsWebsiteUrlCodes.RecintoInRegistryUrl, mrn.Substring(4, 6))
			  .Replace(CustomsWebsiteUrlCodes.NumeroInRegistryUrl, mrn.Substring(10, 8));
}
