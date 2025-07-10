using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business;

public abstract class SADLineCommonWrapper : ILineCommon
{
	public SADLineCommonWrapper(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		entryHeader = entryLine.Header;
	}
	protected readonly CusEntryLine entryLine;
	protected readonly CusEntryHeader entryHeader;

	public IEnumerable<ZString> Containers => entryLine.Containers;

	public ZString GoodsDescription => SADWrapperHelper.RemoveBlackListChars(entryLine.EffectiveDescription).Left(CustomsFieldMaxLength.EntryLine.DescriptionForImportDeclarations);

	public ZInt ItemNumber => entryLine.CL_LineNumber;

	public ZString CombinedNomenclature => entryLine.CL_AdValoremTariff;

	public IEnumerable<ZString> AdditionalCodes => entryLine.AdditionalCodes;

	public ZDecimal GrossMass => entryLine.EffectiveGrossWeight.InKilogramsSafe;

	public ZString Procedure => entryLine.ProcedureCodeWithoutConcession;

	public IEnumerable<ZString> NationalProcedures => NationalProceduresCore;

	protected virtual IEnumerable<ZString> NationalProceduresCore
	{
		get
		{
			var code = entryLine.NationalProcedureCode;
			return code.IsEmpty ? Array.Empty<ZString>() : new ZString[] { code };
		}
	}

	public ZDecimal? NetMass => entryLine.EffectiveCustomsWeight.InKilogramsSafe;

	public IPreviousDocument PreviousAdministrativeDocument
	{
		get
		{
			var mergedPreviousDocuments = entryLine.MergedPreviousDocuments;
			if (!mergedPreviousDocuments.Any())
			{
				return new SADEmptyPreviousDocumentWrapper();
			}
			else if (!mergedPreviousDocuments.Skip(1).Any())
			{
				return new SADPreviousDocumentWrapper(mergedPreviousDocuments.Single());
			}
			else if (((INBWrappableBusinessObject)entryLine).IsImport && !mergedPreviousDocuments.Skip(2).Any() && mergedPreviousDocuments.Any(x => x.IsSummaryDeclarationDocument) && mergedPreviousDocuments.Any(x => x.IsPreviousProcedureDocument))
			{
				return new SADPreviousDocumentWrapper(mergedPreviousDocuments.Single(x => x.IsSummaryDeclarationDocument));
			}
			else
			{
				return new SADPreviousDocumentM2IndicatorWrapper();
			}
		}
	}

	public ZDecimal? SupplementaryUnit => entryLine.SupplementaryUnit;

	public IEnumerable<ICertificate> Certificates => certificates ?? (certificates = GetCertificates());

	public ZString Notes => SADWrapperHelper.RemoveBlackListChars(entryLine.LineNotes).Left(CustomsFieldMaxLength.EntryLine.Notes);

	public ZDecimal? StatisticalValueAmount => entryLine.CL_StatisticalValue;

	public virtual IEnumerable<IDutyTaxFee> Duties
	{
		get
		{
			foreach (var lineFee in EntryLineFeesIncludedInMessageSending.InCustomsCompliantOrder())
			{
				yield return new SADDutyTaxFeeWrapper(lineFee);
			}
		}
	}

	IEnumerable<CusEntryLineFee> EntryLineFeesIncludedInMessageSending => entryLineFeesIncludedInMessageSending ?? (entryLineFeesIncludedInMessageSending = entryLine.Fees.IncludedInMessageSending());
	IEnumerable<CusEntryLineFee> entryLineFeesIncludedInMessageSending;

	public ZDecimal? TotalItemTaxedAmount => EntryLineFeesIncludedInMessageSending.GetTotalTaxedAmount();

	public ZDecimal? GrandTotalTaxedAmount => GetGrandTotalTaxedAmount();

	public ZString CountryOfOrigin => CountryOfOriginCore;

	protected virtual ZString CountryOfOriginCore => entryLine.CountryOfOriginCode;

	#region Implementation

	IEnumerable<ICertificate> GetCertificates()
	{
		const int MaxCertificatesCount = 99;
		var supportingDocuments = entryLine.SupportingDocuments.Cast<SupportingDocument>();
		var certificate10YY = GetCertificate10YYIfNeeded();

		if (supportingDocuments.Count() + (certificate10YY == null ? 0 : 1) > MaxCertificatesCount)
		{
			throw new InvalidOperationException(FormattableString.Invariant($"The maximum length for certificates collection has been exceeded ({MaxCertificatesCount})"));
		}

		if (certificate10YY != null)
		{
			yield return certificate10YY;
		}

		foreach (var supportingDocument in supportingDocuments)
		{
			yield return new SADCertificateWrapper(supportingDocument);
		}
	}

	ICertificate GetCertificate10YYIfNeeded()
	{
		ICertificate certificate10YY = null;
		var invoiceLinesWithThirdQtySet = entryLine.InvoiceLines.Where(x => x.JI_CustomsThirdQuantity > 0);
		if (invoiceLinesWithThirdQtySet.Any())
		{
			certificate10YY = new SADCertificate10YYWrapper(invoiceLinesWithThirdQtySet.Sum(x => x.JI_CustomsThirdQuantity));
		}
		return certificate10YY;
	}

	IEnumerable<ICertificate> certificates;

	ZDecimal? GetGrandTotalTaxedAmount()
	{
		var mergedLines = entryHeader.MergedLines?.Cast<CusEntryLine>();

		var result = ZDecimal.Zero;
		if (IsLastLine())
		{
			result = mergedLines.Sum(entryLine => entryLine.Fees.IncludedInMessageSending().GetTotalTaxedAmount().Round(2));
		}
		return result == ZDecimal.Zero ? null : result;

		bool IsLastLine() => mergedLines != null && mergedLines.Last().CL_LineNumber == entryLine.CL_LineNumber;
	}

	#endregion
}
