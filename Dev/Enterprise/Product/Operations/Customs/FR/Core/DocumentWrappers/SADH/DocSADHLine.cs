using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using PreviousDocument = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

public class DocSADHLine : Enterprise.DocumentWrappers.Customs.EU.DocSADHLine
{
	public static DocSADHLine New(CusEntryLine entryLine, BusinessObjectFactory factory, bool isUsedForFirstEntryLine = true)
	{
		return entryLine == null ? null : new DocSADHLine(entryLine, factory, isUsedForFirstEntryLine);
	}

	DocSADHLine(CusEntryLine entryLine, BusinessObjectFactory factory, bool isUsedUnderTripleBox44Page = false)
		: base(entryLine, factory)
	{
		IsUsedForFirstEntryLine = isUsedUnderTripleBox44Page;
	}

	bool IsUsedForFirstEntryLine { get; set; }

	protected new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

	protected override ZString Box41SupplementaryUQDescriptionCore => EntryLine.SupplementaryQuantity.IsEmpty ? ZString.Empty : EntryLine.SupplementaryUQ;

	protected override ZString ContainerNumberText => DocumentWrapperConstants.Delimiters.CarriageReturn + (NoResString)"Numéro conteneurs : ";

	protected override ZString Box38NetWeightInKGCore
	{
		get
		{
			var effectiveNetWeight = EntryLine.CustomsQuantity.Round(3);// Obtained from JI_CustomsQuantity always in KGs
			return effectiveNetWeight == 0 ? ZString.Empty : FormatDecimalDefault(effectiveNetWeight);
		}
	}

	protected override ZString FormatDecimalDefault(ZDecimal number) => number.ToString("F3");

	protected override ZString Box33ECSupplementCore
	{
		get
		{
			var result = ZString.Empty;
			if (EntryLine.RandomLine.CEAdditionalCodes.Any())
			{
				result = EntryLine.RandomLine.CEAdditionalCodes.First();
			}
			return result;
		}
	}

	protected override ZString Box31_1NumberOfPackagesPiecesMarksAndNumbersCore => new SADHEntryLinePacksMarksAndNumbersBuilder(EntryLine).Build();

	protected override ZString Box33ECSupplement2Core
	{
		get
		{
			var result = ZString.Empty;
			if (EntryLine.RandomLine.CEAdditionalCodes.Count() > 1)
			{
				result = EntryLine.RandomLine.CEAdditionalCodes.ElementAt(1);
			}
			return result;
		}
	}

	protected override ZString Box34CountryOfOriginCore => (bool)EntryLine.Header.Declaration.IsExport ? ZString.Empty : base.Box34CountryOfOriginCore;

	protected override ZString Box34StateOfOriginCore => (bool)EntryLine.Header.Declaration.IsExport ? ZString.Empty : base.Box34StateOfOriginCore;

	public override ZString Box37Procedure
	{
		get
		{
			var result = EntryLine.ProcedureCode;
			if (!result.IsEmpty && result.Length >= 7)
			{
				result = result.SubstringSafe(0, 4) + " " + result.SubstringSafe(4, result.Length - 4);
			}
			return result;
		}
	}

	public (ZString Regular44box, ZString Enlarged44Box) Box44GlobalContent
	{
		get
		{
			if (box44GlobalContent == null)
			{
				box44GlobalContent = Box44AddInfoAndDocumentsHelper.GetBox44AddInfoAndDocumentsForSADH(EntryLine, true);
			}
			return box44GlobalContent.Value;
		}
	}
	(ZString Regular44box, ZString Enlarged44Box)? box44GlobalContent;

	public override ZString Box44AddInfoAndDocuments => Box44GlobalContent.Regular44box;

	public ZString Box44AddInfoAndDocumentsEnlargedBox => Box44GlobalContent.Enlarged44Box;

	public ZBool SwitchFiscalReferenceToEnlargedBox44 => IsUsedForFirstEntryLine && (!Box44AddInfoAndDocumentsEnlargedBox.IsEmpty || Box44AddInfoAndDocuments.Length + Box44FiscalReferenceNumberSummary.Length > Box44AddInfoAndDocumentsHelper.Box44MaxLength);
	public ZBool SwitchVatInfoCalculationToEnlargedBox44 => SwitchFiscalReferenceToEnlargedBox44 || Box44AddInfoAndDocuments.Length + Box44FiscalReferenceNumberSummary.Length + Box44VatInfoCalculation.Length > Box44AddInfoAndDocumentsHelper.Box44MaxLength;

	public ZBool SwitchVatInfoCalculationToEnlargedBox44ForTripleBox44Page => !Box44AddInfoAndDocumentsEnlargedBox.IsEmpty || Box44AddInfoAndDocuments.Length + Box44VatInfoCalculation.Length > Box44AddInfoAndDocumentsHelper.Box44MaxLength;

	internal ZBool NeedASecondPageForBox44 => IsUsedForFirstEntryLine ? SwitchVatInfoCalculationToEnlargedBox44 : SwitchVatInfoCalculationToEnlargedBox44ForTripleBox44Page;

	Box44AddInfoAndDocumentsHelper Box44AddInfoAndDocumentsHelper => box44AddInfoAndDocumentsHelper ?? (box44AddInfoAndDocumentsHelper = new Box44AddInfoAndDocumentsHelper());
	Box44AddInfoAndDocumentsHelper box44AddInfoAndDocumentsHelper;

	protected override OrganisationWrapper GetBox44FiscalReferenceCore()
	{
		var fiscalRepresentative = EntryLine.Header.EntryInstruction?.FiscalReferences?.Cast<EU.Business.Declaration.CusFiscalReference>().FirstOrDefault(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
		return new OrganisationWrapper(OrganisationUsageType.Consignee, fiscalRepresentative?.Owner, ContactType.NoContactType, Factory);
	}

	protected override ZString GetBox44FiscalReferenceNumberCore()
	{
		var fiscalRepresentative = EntryLine.Header.EntryInstruction?.FiscalReferences?.Cast<EU.Business.Declaration.CusFiscalReference>().FirstOrDefault(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
		return fiscalRepresentative?.CFR_Reference ?? ZString.Empty;
	}

	protected override ZBool ShowBox44FiscalReferenceCore => Box44FiscalReference?.MainAddress != null && !Box44FiscalReference.MainAddress.Address.IsEmpty;

	protected override ZString GetBox44FiscalReferenceNumberLabelCore() => (NoResString)"Représentant Fiscal ou Mandataire:";

	protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHLineTaxCollection GetBox47TaxesCore()
	{
		var lineTaxCollection = new DocSADHLineTaxCollection(GetTaxBoxSupporterListCore(), Factory);
		SortBox47TaxesCore(lineTaxCollection);
		return lineTaxCollection;
	}

	protected override Enterprise.DocumentWrappers.Customs.EU.ProducedDocumentsCertificatesBuilder GetProducedDocumentsCertificatesBuilder() => new ProducedDocumentsCertificatesBuilder();

	protected override ZString FormatPreviousDocument(PreviousDocument prevDoc) => prevDoc.CSI_Code + "-" + prevDoc.CSI_SubType + "-" + prevDoc.CSI_ReferenceNumber + (prevDoc.CSI_DateOfIssue.IsValid ? "-" + prevDoc.DateOfIssueInFormat : string.Empty);

	protected override ZDecimal Box46StatisticalValueCore => EntryLine.CL_ConfirmedOrCalculatedStatisticalValue.Round(0);

	protected override ZString Box44VatInfoCalculationCore
	{
		get
		{
			var charges = new Dictionary<ZString, Money>();
			if (EntryLine.CL_LineNumber == 1 && EntryLine.Header.IsImport)
			{
				foreach (var entryLine in EntryLine.Header.MergedLines.Cast<CusEntryLine>())
				{
					foreach (var invoiceLine in entryLine.InvoiceLines.Cast<JobComInvoiceLine>())
					{
						foreach (var charge in invoiceLine.Charges.Cast<JobComInvCharge>()
																  .Concat(invoiceLine.ApportionedCharges.Cast<JobComInvCharge>())
																  .Where(x => x.J7_Amount > 0))
						{
							if (charges.TryGetValue(charge.J7_ChargeType, out var amount))
							{
								charges[charge.J7_ChargeType] = CurrencyConverter.Add(amount, charge.Money);
							}
							else
							{
								charges.Add(charge.J7_ChargeType, charge.Money);
							}
						}
					}
				}
			}

			var result = new ZStringBuilder();
			foreach (var charge in charges)
			{
				result.Append(FormattableString.Invariant($"{charge.Key}:{charge.Value.Amount.Round(2)}{charge.Value.Currency.Code} "));
			}

			return result.ToString();
		}
	}

	public ZString TotalLineLiquidation => GetTotalLineLiquidation();

	ZString GetTotalLineLiquidation() => EntryLine.ConfirmedFees.Cast<CusEntryLineFee>().Where(fee => fee.G4_MethodOfPayment == "1" || fee.G4_MethodOfPayment == "2").Sum(fee => fee.CF_ChargeAmount).ToString("0");

	CurrencyConverter CurrencyConverter => currencyConverter ?? (currencyConverter = EntryLine.Header.CurrencyConverter);
	CurrencyConverter currencyConverter;
}
