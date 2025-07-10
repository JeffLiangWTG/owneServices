using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using EuAdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLineEadAttachmentPrintingSupporter : CusEntryLineSadAttachmentPrintingSupporter
, ICusEntryLineAttachmentPrintingSupporter
{
	public CusEntryLineEadAttachmentPrintingSupporter(CusEntryLine entryLine)
		: base(entryLine)
	{
	}

	protected override ZBool RequiresContainersSectionCore
	{
		get
		{
			if (!requiresContainersSectionCore.HasValue)
			{
				var threshold = entryLine.CL_LineNumber == 1 ? ThresholdForContainersPrintingFirstEntry : ThresholdForContainersPrintingGenericEntry;
				requiresContainersSectionCore = entryLine.Containers.Skip(threshold).Any();
			}
			return requiresContainersSectionCore.Value;
		}
	}
	ZBool? requiresContainersSectionCore;

	protected override ZBool RequiresSupportingDocumentsSectionCore
	{
		get
		{
			if (!requiresSupportingDocumentsSection.HasValue)
			{
				var threshold = entryLine.CL_LineNumber == 1 ? ThresholdForSupportingDocumentsPrintingFirstEntry : ThresholdForSupportingDocumentsPrintingGenericEntry;

				requiresSupportingDocumentsSection =
					SupportingDocumentsFormatted.Sum(x => x.Length) + SupportingDocumentsFormatted.Count()
					+ AdditionalInfosFormatted.Sum(y => y.Length) + AdditionalInfosFormatted.Count()
					> threshold;
			}
			return requiresSupportingDocumentsSection.Value;
		}
	}
	ZBool? requiresSupportingDocumentsSection;

	protected override ZBool RequiresFeesSectionCore => false;

	protected override IEnumerable<ZString> LoadAdditionalInfosSectionContentCore => LoadAdditionalInfosSectionContent();

	ZString[] LoadAdditionalInfosSectionContent()
	{
		return entryLine.Header.AdditionalInfos.Union(entryLine.AdditionalInfos)
			.Cast<EuAdditionalInfo>()
			.Select(additionalInfo => FormatAdditionalInfo(additionalInfo))
			.ToArray();
	}

	ZString FormatAdditionalInfo(EuAdditionalInfo additionalInfo) => new ZStringBuilder()
			.AppendIfNotEmpty(additionalInfo.CSI_Code)
			.AppendIfNotEmpty(additionalInfo.CSI_Description)
			.AppendIfNotEmpty(additionalInfo.CSI_ReferenceNumber)
			.ToStringWithDelimiterBetweenAppends(Dash);

	const int ThresholdForContainersPrintingFirstEntry = 12;
	const int ThresholdForContainersPrintingGenericEntry = 3;

	const int ThresholdForSupportingDocumentsPrintingFirstEntry = 350;
	const int ThresholdForSupportingDocumentsPrintingGenericEntry = 80;
}
