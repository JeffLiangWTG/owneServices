using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using EUDocumentWrapperConstants = Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants;
using IDocSADHLineTaxBoxSupporter = Enterprise.Customs.EU.Business.Declaration.IDocSADHLineTaxBoxSupporter;
using ITCusEntryLine = Enterprise.Customs.IT.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.IT.Business;

public class ITDocSADHLine : DocSADHLine
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Italian text, not a code smell")]
	const string SeeAttachments = "VEDI TIPI DOCUMENTI COME DA LISTA ALLEGATA";

	public static ITDocSADHLine New(ITCusEntryLine entryLine, BusinessObjectFactory factory)
	{
		return entryLine == null ? null : new ITDocSADHLine(entryLine, factory);
	}

	protected ITDocSADHLine(ITCusEntryLine entryLine, BusinessObjectFactory factory)
		: base(entryLine, factory)
	{
	}

	public new ITCusEntryLine EntryLine => (ITCusEntryLine)base.EntryLine;

	protected override ZString Box18IdentityOfTransportAtDepartureLineCore => new ITBox18IdentityOfTransportAtDepartureBuilder((JobDeclaration)Declaration).GetBox18IdentityOfTransportAtDepartureFormatted();

	protected override ZString GetFirstLineOfBox44()
	{
		var result = new ZStringBuilder();
		result.AppendIfNotEmpty(GetSteelType());
		result.AppendIfNotEmpty(GetRemarks());
		return result.ToStringWithDelimiterBetweenAppends(Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants.Delimiters.SemiColonAndspace);
	}

	protected override IAdditionalInformationFormatter GetAdditionalInformationFormatter() => new AdditionalInformationFormatter();

	ZString GetRemarks()
	{
		var result = new ZStringBuilder();
		foreach (var item in EntryLine.Remarks)
		{
			result.Append(item);
		}
		return result.ToStringWithDelimiterBetweenAppends(Enterprise.DocumentWrappers.Customs.EU.DocumentWrapperConstants.Delimiters.SemiColonAndspace);
	}

	ZString GetSteelType()
	{
		var result = ZString.Empty;
		var steelType = EntryLine.RandomLine.ZG_SteelType;

		if (!steelType.IsEmpty && steelType != SteelTypeList.Codes._0)
		{
			result = string.Format(Culture.Invariant, "AC={0}", steelType);
		}

		return result;
	}

	protected override ZDecimal Box46StatisticalValueCore => EntryLine.CL_StatisticalValue.Round(2);

	protected override ZString GetBox45AdjustmentCore()
	{
		return EntryLine.ZG_AdjustmentAmount.IsEmpty ? string.Empty : EntryLine.ZG_AdjustmentAmount.ToString();
	}

	public override ZString Box48DeferredPayment => Declaration.JE_DefermentAccountNumber;

	protected override ZString GetBox40PreviousDocumentsCore(IEnumerable<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> previousDocuments) => new ITCompactPreviousDocumentsBuilder().GetPreviousDocumentsFormatted(previousDocuments);

	protected override ZString Box31_3ContainerNumbersCore => !AttachmentSupporter.RequiresContainersSection ? base.Box31_3ContainerNumbersCore : (ZString)FormattableString.Invariant($"VEDI ALLEGATO TOT. CNT {ContainerList.Count()}");

	public ZString AttachmentContainerNumbers => AttachmentSupporter.RequiresContainersSection ? base.Box31_3ContainerNumbersCore : ZString.Empty;

	protected override void AppendBox31CountrySpecificInformation(ZStringBuilder stringBuilder)
	{
		base.AppendBox31CountrySpecificInformation(stringBuilder);
		stringBuilder.AppendIfNotEmpty(GetFreightChargesInfo());
	}

	protected override ZString Box35GrossWeightInKGCore => GetFormattedWeightForDeclaration(EntryLine.EffectiveGrossWeight.InKilogramsSafe);

	protected override ZString Box38NetWeightInKGCore
	{
		get
		{
			var weight = EntryLine.CustomsUnitQty == Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram ? EntryLine.CustomsQuantity : new ZWeight(EntryLine.CustomsQuantity, EntryLine.CustomsUnitQty).InKilogramsSafe;
			return GetFormattedWeightForDeclaration(weight);
		}
	}

	ZString GetFormattedWeightForDeclaration(ZDecimal weight) => new IT5DecimalsWeightFormatter(weight).GetFormattedValue();

	ZString GetFreightChargesInfo()
	{
		var stringBuilder = new ZStringBuilder();
		var extraEUFreightChargesAmount = EntryLine.ExtraEUFreightChargesAmount;
		if (extraEUFreightChargesAmount != 0)
		{
			stringBuilder.Append(FormattableString.Invariant($"Nolo ExtraCee: {extraEUFreightChargesAmount.ToString(2)}"));
		}

		var euFreightChargesAmount = EntryLine.EUFreightChargesAmount;
		if (euFreightChargesAmount != 0)
		{
			stringBuilder.Append(FormattableString.Invariant($"Nolo Cee: {euFreightChargesAmount.ToString(2)}"));
		}

		var domesticFreightChargesAmount = EntryLine.DomesticFreightChargesAmount;
		if (domesticFreightChargesAmount != 0)
		{
			stringBuilder.Append(FormattableString.Invariant($"Nolo Nazionale: {domesticFreightChargesAmount.ToString(2)}"));
		}
		return stringBuilder.ToStringWithDelimiterBetweenAppends(", ");
	}

	/// <summary>
	/// Do not add any Entry Header level supporting documents (don't call base)
	/// </summary>
	protected override void AddEntryHeaderSupportingDocumentsToFirstPage(ZStringBuilder result) { }

	protected override ZString Box44_1ProducedDocumentsCertificatesCore
		=> AttachmentSupporter.RequiresSupportingDocumentsSection
		? SeeAttachments
		: GetDocumentsFormatted(EntryLine);

	protected override void AddEntryLineSupportingDocuments(ZStringBuilder result)
	{
		if (AttachmentSupporter.RequiresSupportingDocumentsSection)
		{
			result.Append(SeeAttachments);
		}
		else if (AttachmentSupporter.SupportingDocumentsFormatted.Any())
		{
			result.Append(ZString.Join(";", AttachmentSupporter.SupportingDocumentsFormatted.ToArray()));
		}
	}

	protected override ZBool ShouldAppendBox44SupportingDocument9DCR => ZBool.False;

	protected override ZBool ShouldAppendBox48PaymentMethod => ZBool.False;

	/// <summary>
	/// For IT taxes come already sorted from the business layer
	/// </summary>
	protected override void SortBox47TaxesCore(DocSADHLineTaxCollection taxCollection) { }

	protected override IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore() => AttachmentSupporter.RequiresFeesSection ? PlaceholderFees : EntryLineFees;

	IEnumerable<IDocSADHLineTaxBoxSupporter> EntryLineFees => entryLineFees ?? (entryLineFees = EntryLine.GetTaxBoxSupporterList());
	IEnumerable<IDocSADHLineTaxBoxSupporter> entryLineFees;

	IEnumerable<IDocSADHLineTaxBoxSupporter> PlaceholderFees => placeholderFees ?? (placeholderFees = LoadPlaceholderFees());
	IEnumerable<IDocSADHLineTaxBoxSupporter> placeholderFees;

	IEnumerable<IDocSADHLineTaxBoxSupporter> LoadPlaceholderFees()
	{
		yield return new PlaceholderFee("VEDI");
		yield return new PlaceholderFee("LISTA");
		yield return new PlaceholderFee("ALLEGATA");
	}

	public DocSADHLineTaxCollection AttachmentFees => attachmentFees ?? (attachmentFees = GetAttachmentFees());
	DocSADHLineTaxCollection attachmentFees;

	DocSADHLineTaxCollection GetAttachmentFees()
	{
		return new DocSADHLineTaxCollection(AttachmentSupporter.RequiresFeesSection ? EntryLineFees : Enumerable.Empty<IDocSADHLineTaxBoxSupporter>(), Factory);
	}

	public ZString AttachmentSupportingDocuments => attachmentSupportingDocuments ?? (attachmentSupportingDocuments = GetAttachmentSupportingDocuments());
	string attachmentSupportingDocuments;

	ZString GetAttachmentSupportingDocuments()
	{
		return AttachmentSupporter.RequiresSupportingDocumentsSection ? ZString.Join(System.Environment.NewLine, AttachmentSupporter.SupportingDocumentsFormatted.ToArray()) : ZString.Empty;
	}

	public ZString AttachmentAdditionalInfos => attachmentAdditionalInfos ?? (attachmentAdditionalInfos = GetAttachmentAdditionalInfos());
	string attachmentAdditionalInfos;

	ZString GetAttachmentAdditionalInfos()
	{
		return AttachmentSupporter.RequiresSupportingDocumentsSection ? ZString.Join(System.Environment.NewLine, AttachmentSupporter.AdditionalInfosFormatted.ToArray()) : ZString.Empty;
	}

	public bool ContainsBox44Attachments => !AttachmentSupportingDocuments.IsEmpty || !AttachmentAdditionalInfos.IsEmpty;

	ICusEntryLineAttachmentPrintingSupporter AttachmentSupporter => EntryLine.AttachmentPrintingSupporter;

	protected override ProducedDocumentsCertificatesBuilder GetProducedDocumentsCertificatesBuilder() => new ITCompactProducedDocumentsCertificatesBuilder();

	protected override ZString Box44_2SpecialMentionsCore => GetBox44_2SpecialMentions();

	ZString GetBox44_2SpecialMentions()
	{
		var previousProcedureDocument = EntryLine.GetUnloadingDataPreviousProcedureDocument();
		if (previousProcedureDocument != null)
		{
			return FormattableString.Invariant($"DS={previousProcedureDocument.CSI_Tariff}-{previousProcedureDocument.NetMass}-{previousProcedureDocument.SupplementaryQuantity}");
		}
		return ZString.Empty;
	}

	protected override ZString Box49WarehouseCore
	{
		get
		{
			if (EntryInstruction is CusEntryInstruction entryInstruction)
			{
				if (Declaration.IsImport)
				{
					var invoiceLines = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>();
					if (invoiceLines.Any(x => x.HasIntoWarehouseProcedure))
					{
						return entryInstruction.WarehouseIDFor27;
					}

					if (invoiceLines.Any(x => x.HasOutOfWarehouseProcedure))
					{
						return entryInstruction.FromWarehouseCode;
					}
				}
				else
				{
					return entryInstruction.WarehouseIDFor27.SubstringSafe(1, 7);
				}
			}

			return ZString.Empty;
		}
	}

	protected override DocSADHLineTaxCollection GetBox47TaxesCore() => new ITDocSADHLineTaxCollection(GetTaxBoxSupporterListCore(), Factory);

	string GetDocumentsFormatted(ITCusEntryLine entryLine)
	{
		var stringBuilder = new ZStringBuilder();
		var attachmentPrintingSupporter = entryLine.AttachmentPrintingSupporter;

		attachmentPrintingSupporter.SupportingDocumentsFormatted
			.Union(attachmentPrintingSupporter.AdditionalInfosFormatted)
			.ForEach(s => stringBuilder.Append(s));

		return stringBuilder.ToStringWithDelimiterBetweenAppends(EUDocumentWrapperConstants.Delimiters.Semicolon);
	}

	#region PlaceholderFee

	class PlaceholderFee : IDocSADHLineTaxBoxSupporter
	{
		public PlaceholderFee(ZString taxBase)
		{
			TaxBase = taxBase;
		}

		public ZString Type => ZString.Empty;

		public ZString TaxBase { get; }

		public ZString Rate => ZString.Empty;

		public ZString RateDuty => ZString.Empty;

		public ZString RateOverride => ZString.Empty;

		public ZString AmountInDeclarationCurrency => ZString.Empty;

		public ZString MethodOfPayment => ZString.Empty;

		public ZString NationalFeeTypeCode => ZString.Empty;

		public ZString DeclarationMethodOfPayment => ZString.Empty;
	}
	#endregion
}
