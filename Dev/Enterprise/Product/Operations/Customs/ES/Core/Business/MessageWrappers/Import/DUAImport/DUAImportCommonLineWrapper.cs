using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using PreviousDocumentCollection = Enterprise.Customs.ES.Business.Declaration.PreviousDocumentCollection;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DUAImportCommonLineWrapper : ImportCommonLineWrapper, IDUAImportCommonLine
{
	public DUAImportCommonLineWrapper(CusEntryLine cusEntryLine, ZBool isCanary) : base(cusEntryLine)
	{
		randomLine = entryLine.RandomLine;
		previousDocuments = randomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
		isCanaryBool = isCanary;
	}
	readonly JobComInvoiceLine randomLine;
	readonly ZBool isCanaryBool;
	readonly PreviousDocumentCollection previousDocuments;

	const string EmptyExciseExemptionCode = "0";

	public ZString ExternalPackagingType => ContainerHelper.GetFirstESContainerPackagingType(entryLine);

	public IReadOnlyCollection<IPackageCommonNumbers> InternalPackages => internalPackages ?? (internalPackages = PackageWrapper.GetPackagesList(entryLine));
	IReadOnlyCollection<PackageWrapper> internalPackages;

	public IReadOnlyCollection<IVehicleCommon> Vehicles
	{
		get
		{
			if (vehicles == null)
			{
				var vehi = new List<VehicleCommonWrapper>();

				entryLine.InvoiceLinesWithVehicles.ForEach(line => line.Vehicles.Cast<CusVehicle>().ForEach(vehicle => vehi.Add(new VehicleCommonWrapper(vehicle.CVH_VehicleIdentificationNumber, vehicle.CVH_BrandName, vehicle.CVH_ModelName))));
				vehicles = vehi.AsReadOnly();
			}
			return vehicles;
		}
	}
	ReadOnlyCollection<VehicleCommonWrapper> vehicles;

	public ZString OtherMeasurementUnitsCode => ThirdUnitQtyIsGFOrPK ? ZString.Empty : entryLine.ThirdUQ.ConvertCargoWiseToES(entryLine.Factory);

	public ZDecimal OtherMeasurementUnitsNumber => ThirdUnitQtyIsGFOrPK ? ZDecimal.Zero : entryLine.ThirdQuantity;

	public IReadOnlyCollection<ZString> TariffSupplementaryCodes
	{
		get
		{
			if (tariffSupplementaryCodes == null)
			{
				var tariffSupplementaryCodesList = new List<ZString>();

				AddSupplementaryCodeIfNotEmpty(entryLine.SupplementaryCode1, tariffSupplementaryCodesList);
				AddSupplementaryCodeIfNotEmpty(entryLine.SupplementaryCode2, tariffSupplementaryCodesList);

				tariffSupplementaryCodes = tariffSupplementaryCodesList.AsReadOnly();
			}
			return tariffSupplementaryCodes;

			void AddSupplementaryCodeIfNotEmpty(ZString supplementaryCode, List<ZString> tariffSupplementaryCodesList)
			{
				if (!supplementaryCode.IsEmpty)
				{
					tariffSupplementaryCodesList.Add(supplementaryCode);
				}
			}
		}
	}
	IReadOnlyCollection<ZString> tariffSupplementaryCodes;

	public ZString ProductTitleForSpecialTaxes => ExciseCodeStartsWith1 ? ZString.Empty : randomLine.ZG_ExciseCode;

	public ZString SpecialTaxesIndicator
	{
		get
		{
			if (randomLine.ZG_ExciseCode.IsEmpty || ExciseCodeStartsWith1)
			{
				return ZString.Empty;
			}
			else
			{
				var exciseExemption = randomLine.ZG_ExciseExemption;
				return exciseExemption.IsEmpty ? (ZString)EmptyExciseExemptionCode : exciseExemption;
			}
		}
	}

	public ZDecimal GrossWeightInKG => entryLine.GrossWeightInKGForImport;

	public ZString PreferenceCode => randomLine.JI_PrimaryPreference.Left(1);

	public ZString ReductionCode => randomLine.JI_PrimaryPreference.SubstringSafe(1, 2);

	public IReadOnlyCollection<ZString> ConcessionsCPC
	{
		get
		{
			if (concessionsCPC == null)
			{
				concessionsCPC = randomLine.GetAdditionalProcedureCodesList().ToList().AsReadOnly();
			}
			return concessionsCPC;
		}
	}
	IReadOnlyCollection<ZString> concessionsCPC;

	public ZDecimal NetWeightInKG => entryLine.EffectiveCustomsWeight.InKilogramsSafe.Round(3);

	public ZString Contingency => randomLine.JI_ConcessionOrder;

	public ZString PrecedentDocumentType => previousDocuments != null && previousDocuments.Count > 0 ? previousDocuments[0].CSI_SubType : ZString.Empty;

	public ZString PrecedentDocumentClass => previousDocuments != null && previousDocuments.Count > 0 ? previousDocuments[0].CSI_Code : ZString.Empty;

	public ZString PrecedentDocumentReference => previousDocuments != null && previousDocuments.Count > 0 ? PreviousDocumentHelper.GetSUMReferenceNumberToSend(previousDocuments[0]) : ZString.Empty;

	public ZString SupplementaryUnitsCode => entryLine.SupplementaryUQ.ConvertCargoWiseToES(entryLine.Factory);

	public ZDecimal SupplementaryUnitsNumber => entryLine.SupplementaryQuantity;

	public ZDecimal InvoiceValue
	{
		get
		{
			if (invoiceValue == null)
			{
				invoiceValue = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
				{
					var amount = ZDecimal.Zero;
					amount += entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(line => line.JI_LinePrice);

					return amount;
				});
			}
			return invoiceValue.Value;
		}
	}
	CachedProperty<ZDecimal> invoiceValue;

	public IReadOnlyCollection<IImportCommonC44CertificateDocument> DocumentsAndCertificates
	{
		get
		{
			if (documentsAndCertificates == null)
			{
				var documentsAndCertificatesList = new List<ImportCommonC44CertificateDocumentWrapper>();

				documentsAndCertificatesList.AddRange(entryLine.SupportingDocuments.Cast<SupportingDocument>()
																					.Select(doc => new ImportCommonC44CertificateDocumentWrapper(doc)));

				documentsAndCertificatesList.AddRange(entryLine.Header.SupportingDocuments.Cast<SupportingDocument>()
																					.Select(doc => new ImportCommonC44CertificateDocumentWrapper(doc)));

				documentsAndCertificatesList.AddRange(entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType == SupportingDocumentSubType.LIQ && x.CSI_Status != DocumentStatus.Accepted)
																														.Select(doc => new ImportCommonC44CertificateDocumentWrapper(doc)));
				documentsAndCertificates = documentsAndCertificatesList.AsReadOnly();
			}
			return documentsAndCertificates;
		}
	}
	IReadOnlyCollection<ImportCommonC44CertificateDocumentWrapper> documentsAndCertificates;

	public IReadOnlyCollection<ZString> SpecialInstructions
	{
		get
		{
			if (specialInstructions == null)
			{
				var instructions = new HashSet<ZString>();
				AddAdditionalInfoCode(entryLine.AdditionalInfos, instructions);
				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					AddAdditionalInfoCode(invoiceLine.InvoiceHeader.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>(), instructions);
				}
				AddAdditionalInfoCode(entryLine.Declaration.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>(), instructions);
				AddAdditionalInfoCode(entryLine.Header.AdditionalInfos, instructions);

				specialInstructions = instructions.ToList().AsReadOnly();
			}
			return specialInstructions;

			void AddAdditionalInfoCode(IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> additionalInfoCollection, HashSet<ZString> instructions)
			{
				foreach (var additionalInfo in additionalInfoCollection)
				{
					instructions.Add(additionalInfo.CSI_Code);
				}
			}
		}
	}
	IReadOnlyCollection<ZString> specialInstructions;

	public IReadOnlyCollection<IDUAImportDeclaredTax> DeclaredTaxes => declaredTaxes ?? (declaredTaxes = entryLine.Fees.Cast<CusEntryLineFee>().Select(x => new DUAImportDeclaredTaxWrapper(x, isCanaryBool)).ToList().AsReadOnly());
	IReadOnlyCollection<DUAImportDeclaredTaxWrapper> declaredTaxes;

	public ZDecimal TotalValue
	{
		get
		{
			if (totalValue == null)
			{
				totalValue = new CachedProperty<ZDecimal>(entryLine.Factory, () =>
				{
					var amount = ZDecimal.Zero;
					amount += entryLine.Fees.Cast<CusEntryLineFee>().Sum(fee => fee.CF_ChargeAmount);

					return amount;
				});
			}
			return totalValue.Value;
		}
	}
	CachedProperty<ZDecimal> totalValue;

	const string exciseCode1 = "1";
	ZBool ExciseCodeStartsWith1 => randomLine.ZG_ExciseCode.StartsWith(exciseCode1);

	ZBool ThirdUnitQtyIsGFOrPK => randomLine.JI_CustomsThirdUnitQty == ESConstants.UOM.GF
								|| randomLine.JI_CustomsThirdUnitQty == ESConstants.UOM.PK;
}
