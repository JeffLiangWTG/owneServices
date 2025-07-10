using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLineSadAttachmentPrintingSupporter : ICusEntryLineAttachmentPrintingSupporter
{
	public CusEntryLineSadAttachmentPrintingSupporter(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}

	protected readonly CusEntryLine entryLine;

	public ZBool RequiresAttachment => RequiresContainersSection || RequiresSupportingDocumentsSection || RequiresFeesSection;

	public ZBool RequiresContainersSection => RequiresContainersSectionCore;

	protected virtual ZBool RequiresContainersSectionCore
	{
		get
		{
			if (!requiresContainersSectionCore.HasValue)
			{
				requiresContainersSectionCore = entryLine.Containers.Skip(ThresholdForContainersPrinting).Any();
			}
			return requiresContainersSectionCore.Value;
		}
	}
	ZBool? requiresContainersSectionCore;

	public ZBool RequiresSupportingDocumentsSection => RequiresSupportingDocumentsSectionCore;

	protected virtual ZBool RequiresSupportingDocumentsSectionCore
	{
		get
		{
			if (!requiresSupportingDocumentsSection.HasValue)
			{
				requiresSupportingDocumentsSection = SupportingDocumentsFormatted.Sum(x => x.Length) + SupportingDocumentsFormatted.Count() > ThresholdForSupportingDocumentsPrinting;
			}
			return requiresSupportingDocumentsSection.Value;
		}
	}
	ZBool? requiresSupportingDocumentsSection;

	public IEnumerable<ZString> SupportingDocumentsFormatted => supportingDocumentsFormatted ?? (supportingDocumentsFormatted = LoadSupportingDocumentsSectionContentCore());
	ZString[] supportingDocumentsFormatted;

	public ZBool RequiresFeesSection => RequiresFeesSectionCore;

	public IEnumerable<ZString> AdditionalInfosFormatted => additionalInfosFormattedFormatted ?? (additionalInfosFormattedFormatted = LoadAdditionalInfosSectionContentCore);
	IEnumerable<ZString> additionalInfosFormattedFormatted;

	protected virtual IEnumerable<ZString> LoadAdditionalInfosSectionContentCore => Enumerable.Empty<ZString>();

	protected virtual ZBool RequiresFeesSectionCore
	{
		get
		{
			if (!requiresFeesSection.HasValue)
			{
				requiresFeesSection = entryLine.Fees.Count > ThresholdForFeesPrinting;
			}
			return requiresFeesSection.Value;
		}
	}
	ZBool? requiresFeesSection;

	#region Implementation

	protected virtual ZString[] LoadSupportingDocumentsSectionContentCore()
	{
		var formattedSupportingDocumentInfoList = new List<ZString>();
		foreach (SupportingDocument headerDocument in Header.SupportingDocuments)
		{
			formattedSupportingDocumentInfoList.Add(FormatSupportingDocumentInfoCore(headerDocument));
		}
		foreach (SupportingDocument lineSupportingDocument in entryLine.SupportingDocuments)
		{
			formattedSupportingDocumentInfoList.Add(FormatSupportingDocumentInfoCore(lineSupportingDocument));
		}
		return formattedSupportingDocumentInfoList.ToArray();
	}

	protected virtual ZString FormatSupportingDocumentInfoCore(SupportingDocument supportingDocument)
	{
		return new ZStringBuilder()
			.AppendIfNotEmpty(supportingDocument.CSI_Code)
			.AppendIfNotEmpty(supportingDocument.CSI_RN_NKCountryCode)
			.AppendIfNotEmpty(supportingDocument.CSI_YearOfIssue)
			.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber)
			.AppendIfNotEmpty(GetUnitWithQuantitySection(supportingDocument.CSI_UnitOfQuantity, supportingDocument.CSI_Quantity, "#0.######"))
			.AppendIfNotEmpty(GetUnitWithQuantitySection(supportingDocument.CSI_RX_NKCurrency, supportingDocument.CSI_Value, "#0.00"))
			.ToStringWithDelimiterBetweenAppends(Dash);

		string GetUnitWithQuantitySection(ZString unitValue, ZDecimal quantityValue, ZString format)
		{
			if (unitValue.IsEmpty)
			{
				return ZString.Empty;
			}

			return new ZStringBuilder()
				.Append(unitValue)
				.Append(quantityValue.ToString(format, CultureInfo.InvariantCulture))
				.ToStringWithDelimiterBetweenAppends(Dash);
		}
	}

	CusEntryHeader Header => entryLine.Header;

	const int ThresholdForContainersPrinting = 5;
	const int ThresholdForSupportingDocumentsPrinting = 150;
	const int ThresholdForFeesPrinting = 8;

	protected const string Dash = "-";

	#endregion
}
