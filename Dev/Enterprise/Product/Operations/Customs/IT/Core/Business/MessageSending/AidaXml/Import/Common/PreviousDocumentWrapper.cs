using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

class PreviousDocumentWrapper : IPreviousDocument
{
	public PreviousDocumentWrapper(PreviousDocument previousDocument, IEnumerable<PreviousDocument> groupedPreviousDocuments = null)
	{
		PreviousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		this.groupedPreviousDocuments = groupedPreviousDocuments ?? new[] { PreviousDocument };

		lazyQuantity = new Lazy<decimal?>(GetQuantity);
		lazyReferenceNumber = new Lazy<string>(GetReferenceNumber);
	}

	protected PreviousDocument PreviousDocument { get; }

	int? IPreviousDocument.LineNo => PreviousDocument.CSI_LineNo.NullIfZero();

	int? IPreviousDocument.NumberOfPackages => GetNumberOfPackages();

	string IPreviousDocument.PackageType => PreviousDocument.CSI_PackType;

	decimal? IPreviousDocument.Quantity => Quantity;
	readonly Lazy<decimal?> lazyQuantity;

	decimal? Quantity => lazyQuantity.Value;

	string IPreviousDocument.ReferenceNumber => lazyReferenceNumber.Value;
	readonly Lazy<string> lazyReferenceNumber;

	string IPreviousDocument.DocumentType => PreviousDocument.CSI_Code;

	string IPreviousDocument.UnitOfQuantity
	{
		get
		{
			const string kilogram = "KGM";

			return Quantity.HasValue
				? new ZString(kilogram)
				: ZString.Empty;
		}
	}

	#region Implementation

	protected virtual decimal? GetQuantity()
	{
		if (PreviousDocument.IsPreviousProcedureDocument || IsMR1ProcedureAndParentInvoiceLinePreviousProcedureIsNotH2OrH3())
		{
			return ((ZDecimal)groupedPreviousDocuments.Sum(x => x.EffectiveNetMass.InKilogramsSafe.NullIfZero())).NullIfZero();
		}

		return ((ZDecimal)groupedPreviousDocuments.Sum(x => x.EffectiveGrossMass.InKilogramsSafe.NullIfZero())).NullIfZero();
	}

	ZInt? GetNumberOfPackages()
	{
		return ((ZInt)groupedPreviousDocuments.Sum(x => x.CSI_PackQty.NullIfZero())).NullIfZero();
	}

	bool IsMR1ProcedureAndParentInvoiceLinePreviousProcedureIsNotH2OrH3()
	{
		if (PreviousDocument.CSI_Procedure != ImportPreviousDocumentProcedureList.Codes.DichiarazioneNotificaMrn)
		{
			return false;
		}

		if (!(PreviousDocument.Parent is JobComInvoiceLine invoiceLine))
		{
			return false;
		}

		var procedures = GetProceduresByProcedureCode(invoiceLine.JI_Calc_PreviousProcedure);
		return !procedures.Any(x => IsProcedureH2OrH3(x));

		bool IsProcedureH2OrH3(RefCusProcedure procedure)
		{
			var groups = procedure.Groups.ToArray();
			return
				groups.Contains(ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2) ||
				groups.Contains(ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3);
		}
	}

	RefCusProcedure[] GetProceduresByProcedureCode(ZString previousProcedureCode)
	{
		var query = RefCusProcedure.Loader.GetFullFilter(dataGroupingCode: Core.Constants.CountryCodes.Italy,
			date: ZDateTime.Today,
			declarationType: ZString.Empty,
			shipmentType: EUJobMessageTypeList.Codes.Import);
		query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, previousProcedureCode);
		return PreviousDocument.Factory.Load<RefCusProcedure>(query);
	}

	string GetReferenceNumber()
	{
		var procedure = PreviousDocument.CSI_Procedure;

		switch (procedure)
		{
			case PreviousDocumentProcedureList.Codes.DichiarazioneMeccanizzataDiTransito:
				return GetReferenceNumberCombineProcedureAndMRN(procedure);

			case ImportPreviousDocumentProcedureList.Codes.NumeroLrn:
				return PreviousDocument.CSI_ReferenceNumber;

			case ImportPreviousDocumentProcedureList.Codes.DichiarazioneNotificaMrn:
			case ImportPreviousDocumentProcedureList.Codes.MrnTemporaneaCustodia:
			case ImportPreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3:
				return PreviousDocument.CSI_ReferenceNumber2;

			default:
				return GetReferenceNumberCombineProcedureThenNumberThenYearThenOffice(procedure);
		}
	}

	const string DashDelimiter = "-";

	string GetReferenceNumberCombineProcedureAndMRN(ZString procedure)
	{
		return new ZStringBuilder()
			.AppendIfNotEmpty(procedure)
			.AppendIfNotEmpty(PreviousDocument.CSI_ReferenceNumber2)
			.ToStringWithDelimiterBetweenAppends(DashDelimiter);
	}

	string GetReferenceNumberCombineProcedureThenNumberThenYearThenOffice(ZString procedure)
	{
		return new ZStringBuilder()
			.AppendIfNotEmpty(procedure)
			.AppendIfNotEmpty(PreviousDocument.CSI_ReferenceNumber)
			.AppendIfNotEmpty(GetYearOfIssue(PreviousDocument.CSI_DateOfIssue))
			.AppendIfNotEmpty(XmlWrapperHelper.RemoveIsoCode(PreviousDocument.CSI_CustomsOffice))
			.ToStringWithDelimiterBetweenAppends(DashDelimiter);
	}

	string GetYearOfIssue(ZDateTime dateOfIssue)
	{
		return dateOfIssue.IsValid
			? (ZString)dateOfIssue.Year.ToString()
			: ZString.Empty;
	}

	#endregion

	readonly IEnumerable<PreviousDocument> groupedPreviousDocuments;
}
