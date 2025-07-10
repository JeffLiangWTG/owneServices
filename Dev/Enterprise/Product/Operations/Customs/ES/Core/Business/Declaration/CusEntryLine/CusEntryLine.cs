using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Declaration;

public class CusEntryLine : EU.Business.Declaration.CusEntryLine, Integration.Customs.ES.ICusEntryLine
{
	public CusEntryLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.CusEntryLine.Schema
	{
		public const string TotalGrossWeightInKG = nameof(CusEntryLine.TotalGrossWeightInKG);
		public const string VehiclesOrPackagesQty = nameof(CusEntryLine.VehiclesOrPackagesQty);
	}

	#region TypeSafe

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;
	public new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

	protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);

	public new EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees => (EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;

	public new ReadOnlySupportingDocumentCollection ReadOnlySupportingDocuments => (ReadOnlySupportingDocumentCollection)base.ReadOnlySupportingDocuments;

	protected override void CopySupportingDocumentPropertiesExcludedFromCloning(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument copiedDocument, EU.Business.Declaration.MultiLineAddInfos.SupportingDocument originalDocument)
	{
		base.CopySupportingDocumentPropertiesExcludedFromCloning(copiedDocument, originalDocument);
		copiedDocument.CSI_Procedure = originalDocument.CSI_Procedure;
	}

	protected override EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocumentCollection GetNewReadOnlySupportingDocuments() => new ReadOnlySupportingDocumentCollection(this);

	public ReadOnlyPreviousDocumentCollection ReadOnlyPreviousDocuments
	{
		get
		{
			readOnlyPreviousDocuments ??= GetNewReadOnlyPreviousDocuments();
			if (!IsDeleted && !readOnlyPreviousDocuments.IsLoaded && !IsInProcessOfMerging)
			{
				readOnlyPreviousDocuments.LoadNew();
			}
			return readOnlyPreviousDocuments;
		}
	}
	ReadOnlyPreviousDocumentCollection readOnlyPreviousDocuments;

	ReadOnlyPreviousDocumentCollection GetNewReadOnlyPreviousDocuments() => new ReadOnlyPreviousDocumentCollection(this);

	public void ResetReadOnlyPreviousDocuments()
	{
		readOnlyPreviousDocuments = null;
	}

	public ReadOnlyAdditionalInfoCollection ReadOnlyAdditionalInfos
	{
		get
		{
			if (readOnlyAdditionalInfos == null)
			{
				readOnlyAdditionalInfos = GetNewReadOnlyAdditionalInfos();
			}
			if (!IsDeleted && !readOnlyAdditionalInfos.IsLoaded && !IsInProcessOfMerging)
			{
				readOnlyAdditionalInfos.LoadNew();
			}
			return readOnlyAdditionalInfos;
		}
	}
	ReadOnlyAdditionalInfoCollection readOnlyAdditionalInfos;

	ReadOnlyAdditionalInfoCollection GetNewReadOnlyAdditionalInfos() => new ReadOnlyAdditionalInfoCollection(this);

	public void ResetReadOnlyAdditionalInfos()
	{
		readOnlyAdditionalInfos = null;
	}

	protected override Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

	#endregion

	public override void Delete()
	{
		readOnlyPreviousDocuments?.RemoveAndDeleteAll();
		readOnlyAdditionalInfos?.RemoveAndDeleteAll();
		base.Delete();
	}

	public ZDecimal TotalGrossWeightInKG
	{
		get
		{
			var grossWeight = (ZDecimal)InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.EffectiveGrossWeight.InUnroundedKilograms);
			return Header.IsUCC6 || Header.ZG_POUSVersion > 0 || Header.IsH2Style ? grossWeight : grossWeight.Ceiling(0);
		}
	}

	public NonPersistentCusContainerCollection ContainersPivot
	{
		get
		{
			var collection = new NonPersistentCusContainerCollection(RandomLine);
			collection.RemoveAll();
			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				foreach (CusContainerInvoiceLinePivot container in line.ContainersPivot)
				{
					if (collection.ToList().Where(x => ((CusContainerInvoiceLinePivot)x).ContainerNumber.Equals(container.ContainerNumber)).ToList().Count == 0)
					{
						collection.Add(container);
					}
				}
			}
			return collection;
		}
	}

	public IEnumerable<ZString> SealsFromContainers(string containerCode1 = null)
	{
		IEnumerable<CusContainerInvoiceLinePivot> source = base.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany((JobComInvoiceLine x) => x.ContainersPivot).Cast<CusContainerInvoiceLinePivot>();
		if (!string.IsNullOrEmpty(containerCode1))
		{
			source = source.Where(y => y.ContainerNumber == containerCode1);
		}
		var firstSealsCodes = (from x in source select x.Container.CO_Seal into x where !x.IsEmpty select x).Distinct();
		var secondSealsCodes = (from x in source select x.Container.CO_SecondSeal into x where !x.IsEmpty select x).Distinct();
		var additionalCusSeals = source.SelectMany(x => ((CusContainer)x.Container).AdditionalSeals);
		var additionalSealsCodes = (from x in additionalCusSeals select x.BK_SealNumber into x where !x.IsEmpty select x).Distinct();

		return firstSealsCodes.Concat(secondSealsCodes).Concat(additionalSealsCodes).Distinct();
	}

	public IEnumerable<ZString> SealsFromEquipments
	{
		get
		{
			IEnumerable<InvoiceLinePackagePivot> source = base.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany((JobComInvoiceLine x) => x.PackagesPivot).Cast<InvoiceLinePackagePivot>();

			var equipmentCodes = source.Select(x => x.Package.CW_ContainerNoOrEquipmentNo).Distinct();

			var sealsCodes = Array.Empty<ZString>();
			if (equipmentCodes.Any())
			{
				var additionalCusSeals = Declaration.Equipments.Where(x => equipmentCodes.Contains(x.CEQ_IdentificationNumber)).SelectMany(x => x.Seals); //This line has an error (null reference)
				sealsCodes = (from x in additionalCusSeals select x.BK_SealNumber into x where !x.IsEmpty select x).Distinct().ToArray();
			}

			return sealsCodes;
		}
	}

	public IEnumerable<JobComInvoiceLine> InvoiceLinesWithVehicles => InvoiceLines.Cast<JobComInvoiceLine>()
																				.Where(x => x.IsVehicleDeclared);

	protected override IEnumerable<ZString> GuaranteeDeferredMethodsOfPayment => Factory.GetCachedValue("ES.CusEntryLine.GuaranteeDeferredMethodsOfPayment", () => new ZString[] {
		MethodOfPaymentList.Codes.R });

	protected override IEnumerable<ZString> DeferredMethodsOfPayment => Factory.GetCachedValue("ES.CusEntryLine.DeferredMethodsOfPayment", () => new ZString[] {
		MethodOfPaymentList.Codes.J, MethodOfPaymentList.Codes.R });

	protected override MoPLevel MoPDetailsLevel => MoPLevel.InvoiceLine;

	protected override bool EffectiveGrossWeightIsApplicableCore => true;

	public ZInt Box31MaxLength => 1030;

	public ZInt ImportBox44MaxLength => 900;

	public ZInt ExportBox44MaxLength => 987;

	public ZInt ImportBox44BISPageMaxLength => 800;

	public ZInt ExportBox44BISPageMaxLength => 881;

	public ZString Box31CompleteText
	{
		get
		{
			var box31MisMash = new ZStringBuilder();

			ZString conts = ZString.Empty;
			if (!Header.IsExportUCC6)
			{
				conts = ZString.Join(", ", Containers.ToArray());
			}
			else
			{
				conts = ZString.Empty;
				foreach (var container in Containers)
				{
					conts += conts.IsEmpty ? container.ToString() : ", " + container.ToString();
					if (SealsFromContainers().Any())
					{
						conts += " - " + ZString.Join("/", SealsFromContainers(container).ToArray());
					}
				}
			}
			var numContainers = Containers.Count();
			var contTag = numContainers == 1 ? "CONTENEDOR" : "CONTENEDORES";
			if (!conts.IsEmpty)
			{
				box31MisMash.AppendIfNotEmpty(Box31_PackagesText + string.Format(CultureInfo.InvariantCulture, " {0} {1} ", numContainers, contTag) + conts + ".");
			}
			else
			{
				box31MisMash.AppendIfNotEmpty(Box31_PackagesText);
			}

			box31MisMash.Append("--------------------------------------------------------------------------------");

			box31MisMash.AppendIfNotEmpty(CL_Description);

			return box31MisMash.ToStringWithNewLineBetweenAppends();
		}
	}

	protected ZString Box31_PackagesText
	{
		get
		{
			var invoiceLinesVehicles = InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.IsVehicleDeclared);
			return invoiceLinesVehicles.Any() ? GetVehiclesBox31Text(invoiceLinesVehicles) : GetPackagesBox31Text();
		}
	}

	ZString GetVehiclesBox31Text(IEnumerable<JobComInvoiceLine> invLinesVehicles)
	{
		var packingDetails = new ZStringBuilder();

		packingDetails.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} BASTIDORES", invLinesVehicles.Cast<JobComInvoiceLine>().Sum(x => x.Vehicles.Count)));

		foreach (var line in invLinesVehicles)
		{
			foreach (CusVehicle vehicle in line.Vehicles)
			{
				packingDetails.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", vehicle.CVH_VehicleIdentificationNumber, vehicle.CVH_BrandName, vehicle.CVH_ModelName));
			}
		}
		return packingDetails.ToStringWithDelimiterBetweenAppends(", ") + ".";
	}

	ZString GetPackagesBox31Text()
	{
		var packingDetails = new ZStringBuilder();
		var allPacksInAllLines = PackagingDetails;
		Dictionary<string, int> uniquePacks = new Dictionary<string, int>();
		foreach (InvoiceLinePackagePivot pivot in allPacksInAllLines)
		{
			string key = pivot.Package.CW_PackType + "~" + pivot.Package.CW_MarksAndNos;
			if (uniquePacks.ContainsKey(key))
			{
				uniquePacks[key] += pivot.CHC_NumberOfPacks;
			}
			else
			{
				uniquePacks[key] = pivot.CHC_NumberOfPacks;
			}
		}
		foreach (string key in uniquePacks.Keys)
		{
			string pc = key.Split('~')[0];
			string pm = key.Split('~')[1];
			string pn = uniquePacks[key].ToString(CultureInfo.InvariantCulture);
			packingDetails.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}, {2}", pn, pc, pm));
		}

		return packingDetails.ToStringWithDelimiterBetweenAppends(". ") + ".";
	}

	public ZString Box37ProcedureCompleteText => ProcedureCode.Left(4).InsertSafe(2, ".") + " | " + RandomLine.JI_FormattedProcedure.SubstringSafe(4);

	public ZString Box37_2ProcedureCompleteText
	{
		get
		{
			var additionalCodes = RandomLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>()
														.Where(additionalCode => !additionalCode.CY_Code.IsEmpty)
														.Select(additionalCode => additionalCode.CY_Code.Right(3)).ToArray();
			return ZString.Join(" ", additionalCodes);
		}
	}

	public ZString Box44CompleteText
	{
		get
		{
			var result = new ZStringBuilder();
			AddEntryLineSupportingDocuments(result);
			if (Header.IsExport && !Header.IsT2L && !Header.IsExsSubStyle)
			{
				AddEntryLineAdditionalDocuments(result);
				AddEntryLineAuthorizations(result);
			}
			return result.ToStringWithDelimiterBetweenAppends("; ");
		}
	}

	protected void AddEntryLineSupportingDocuments(ZStringBuilder result)
	{
		var documents = new List<SupportingDocument>();
		var entryStatus = Header.CH_EntryStatus;
		if (entryStatus.IsEmpty || entryStatus == EntryStatusCodes.IncompletePreDeclaration)
		{
			documents.AddRange(Header.SupportingDocuments.Cast<SupportingDocument>());
			documents.AddRange(SupportingDocuments.Cast<SupportingDocument>());

			documents.AddRange(GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType == SupportingDocumentSubType.LIQ));
			documents.AddRange(Header.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType == SupportingDocumentSubType.LIQ));
		}
		else
		{
			documents.AddRange(GetPreviouslySentSupportingDocuments().Where(x => x.CSI_Status == DocumentStatus.Accepted));
			documents.AddRange(Header.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_Status == DocumentStatus.Accepted));
		}

		foreach (var document in documents)
		{
			var lineString = new ZStringBuilder();
			lineString.Append(document.CSI_Code + ":");
			lineString.AppendIfNotEmpty(document.CSI_ReferenceNumber);
			lineString.AppendIfNotEmpty(document.CSI_DateOfExpiry.ToCustomsFormatDateStringddMMyyyyWithDash());
			if (document.CSI_DateOfExpiry.IsEmpty)
			{
				lineString.AppendIfNotEmpty(document.CSI_DateOfIssue.ToCustomsFormatDateStringddMMyyyyWithDash());
			}

			result.Append(lineString.ToStringWithDelimiterBetweenAppends(" "));
		}
	}

	protected void AddEntryLineAdditionalDocuments(ZStringBuilder result)
	{
		var documents = new List<AdditionalInfo>();
		var entryStatus = Header.CH_EntryStatus;
		if (entryStatus.IsEmpty)
		{
			documents.AddRange(Declaration.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments));
			documents.AddRange(Header.EntryInstruction?.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments) ?? Enumerable.Empty<AdditionalInfo>());
			Header.InvoiceHeaders.ForEach(x => documents.AddRange(((JobComInvoiceHeader)x).AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments)));
			InvoiceLines.ForEach(x => documents.AddRange(((JobComInvoiceLine)x).AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments)));
		}
		else
		{
			documents.AddRange(GetPreviouslySentAdditionalInfos().Where(x => x.CSI_Status == DocumentStatus.Accepted && x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments));
		}

		foreach (var document in documents)
		{
			var lineString = new ZStringBuilder();
			lineString.Append(document.CSI_Code + ":");
			lineString.AppendIfNotEmpty(document.CSI_ReferenceNumber);

			result.Append(lineString.ToStringWithDelimiterBetweenAppends(" "));
		}
	}

	protected void AddEntryLineAuthorizations(ZStringBuilder result)
	{
		var authorizations = new List<CusAuthorizationUsage>();
		authorizations.AddRange(Header.EntryInstruction?.CusAuthorizationUsages.Cast<CusAuthorizationUsage>() ?? Enumerable.Empty<CusAuthorizationUsage>());
		InvoiceLines.ForEach(x => authorizations.AddRange(((JobComInvoiceLine)x).CusAuthorizationUsages.Cast<CusAuthorizationUsage>()));

		foreach (var authorization in authorizations)
		{
			var code = authorization.CustomsCode.IsEmpty ? authorization.AGC_Code : authorization.CustomsCode;
			var lineString = new ZStringBuilder();
			lineString.Append(code + ":");
			lineString.AppendIfNotEmpty(authorization.AGC_Number);

			result.Append(lineString.ToStringWithDelimiterBetweenAppends(" "));
		}
	}

	public ZString Box45CompleteText()
	{
		var result = new ZStringBuilder();
		result.Append(string.Format("+{0}", AddPositiveAdjustment()));
		result.Append(string.Format("-{0}", AddNegativeAdjustment()));
		return result.ToStringWithDelimiterBetweenAppends(" ");
	}

	ZString AddPositiveAdjustment()
	{
		var amount = ZDecimal.Zero;
		foreach (JobComInvoiceLine line in InvoiceLines)
		{
			amount += line.JI_PosAdj;
		}
		return amount.ToStringTrimZeros("N2");
	}

	ZString AddNegativeAdjustment()
	{
		var amount = ZDecimal.Zero;
		foreach (JobComInvoiceLine line in InvoiceLines)
		{
			amount += line.JI_NegAdj;
		}
		return amount.ToStringTrimZeros("N2").Replace("-", "");
	}

	public IEnumerable<IESDocSADHLineTaxBoxSupporter> GetESTaxBoxSupporterList() => Fees.Cast<IESDocSADHLineTaxBoxSupporter>();

	#region Package

	public bool HasNonEmptyPackage
	{
		get
		{
			if (hasNonEmptyPackageCached == null)
			{
				hasNonEmptyPackageCached = new CachedProperty<bool>(Factory, () =>
				{
					bool result = false;
					var package = Package;

					if (package != null)
					{
						result = (!package.IsUnpacked && package.PackCount > 0 || package.IsUnpacked && package.ItemsCount > 0);
					}

					return result;
				});
			}
			return hasNonEmptyPackageCached.Value;
		}
	}
	CachedProperty<bool> hasNonEmptyPackageCached;

	public PackingGroup Package => GetPackage();

	PackingGroup GetPackage()
	{
		PackingGroup result = null;

		if (PackagingDetails.Any())
		{
			var firstPackage = PackagingDetails.First().Package;

			bool isfirstPackageUnpacked = firstPackage.CW_PackType == EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked
									   || firstPackage.CW_PackType == EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.UnpackedMultiple
									   || firstPackage.CW_PackType == EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.UnpackedSingle;

			result = new PackingGroup(firstPackage.CW_PackType, 0, 0, firstPackage.CW_MarksAndNos, isfirstPackageUnpacked);

			foreach (var packagePivot in PackagingDetails)
			{
				if (packagePivot.Package.CW_PackType == firstPackage.CW_PackType && packagePivot.Package.CW_MarksAndNos == firstPackage.CW_MarksAndNos)
				{
					result.PackCount += packagePivot.CHC_NumberOfPacks;
					result.ItemsCount += packagePivot.InvoiceLine.JI_InvoiceQuantity;
				}
			}
		}
		return result;
	}

	public ZInt FirstPackQty() => Package?.PackCount ?? ZInt.Zero;

	public ZString FirstPackType() => Package?.Type ?? ZString.Empty;

	ZInt PackagesQty => PackagingDetails.Sum(x => x.CHC_NumberOfPacks);

	[ResourceStringData("Enterprise.Customs.ES.Business.Declaration.CusEntryLine|VehiclesOrPackagesQty", Caption = "Packages", MediumCaption = "Packs", ShortCaption = "Pkg", FullDescription = "Total Packages for Entry Line")]
	public ZInt VehiclesOrPackagesQty => VehiclesQty > 0 ? VehiclesQty : PackagesQty;
	#endregion

	public bool HasSupportingDocumentsToSend()
	{
		var previouslySentDocs = GetPreviouslySentSupportingDocuments();

		return SupportingDocuments.Cast<SupportingDocument>().Any(doc => !doc.MatchesAnyPreviouslySentDocument(previouslySentDocs))
				|| Header.SupportingDocuments.Cast<SupportingDocument>().Any(doc => !doc.MatchesAnyPreviouslySentDocument(previouslySentDocs));
	}

	public SupportingDocument[] GetPreviouslySentSupportingDocuments()
	{
		var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, TablePrefix);
		query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.SupportingDocument);
		return (SupportingDocument[])Factory.Load(typeof(SupportingDocument), query);
	}

	public PreviousDocument[] GetPreviouslySentPreviousDocuments()
	{
		var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, TablePrefix);
		query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.PreviousDocument);
		return (PreviousDocument[])Factory.Load(typeof(PreviousDocument), query);
	}

	public AdditionalInfo[] GetPreviouslySentAdditionalInfos()
	{
		var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, TablePrefix);
		query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.AdditionalInfo);
		return (AdditionalInfo[])Factory.Load(typeof(AdditionalInfo), query);
	}

	public ZInt VehiclesQty => Factory.GetValue(ref vehiclesQty, () =>
	{
		bool hasFRPackageMark = InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.PackagesPivot
			.Cast<InvoiceLinePackagePivot>()
			.Any(p => (p.Package?.CW_PackType ?? ZString.Empty) == PackageType.Frame));

		if (hasFRPackageMark)
		{
			return InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.PackagesPivot
			.Cast<InvoiceLinePackagePivot>()
			.Sum(p => (p.Package?.CW_PackType ?? ZString.Empty) == PackageType.Frame ? p.CHC_NumberOfPacks : ZInt.Zero));
		}
		else
		{
			return InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.Vehicles.Count);
		}
	});
	CachedProperty<ZInt> vehiclesQty;

	public ZBool HasPRECustomsOffice => Declaration.CustomsOffices.Cast<EuOfficeCode>().Any(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation);

	public ZDecimal GrossWeightInKGForImport
	{
		get
		{
			var grossWeight = EffectiveGrossWeight.InKilogramsSafe;
			return grossWeight < 1 ? grossWeight.Round(3) : (ZDecimal)Math.Ceiling(grossWeight);
		}
	}
}
