using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDLineWrapper : IDeclarationDVDLine
	{
		public DeclarationDVDLineWrapper(CusEntryLine entryLine, bool shouldDeclareUCRInLine = false, bool shouldDeclareAddSupplyActorsInLine = false, bool shouldDeclareCountryOfDestinationInLine = false, bool shouldDeclareCountryOfExportInLine = false, bool shouldDeclarePreviousDocumentsInLine = false)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
			randomLine = entryLine.RandomLine;
			entryInstruction = randomLine.EntryInstruction;

			this.shouldDeclareUCRInLine = shouldDeclareUCRInLine;
			this.shouldDeclareAddSupplyActorsInLine = shouldDeclareAddSupplyActorsInLine;
			this.shouldDeclareCountryOfDestinationInLine = shouldDeclareCountryOfDestinationInLine;
			this.shouldDeclareCountryOfExportInLine = shouldDeclareCountryOfExportInLine;
			this.shouldDeclarePreviousDocumentsInLine = shouldDeclarePreviousDocumentsInLine;
		}
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine randomLine;
		readonly CusEntryInstruction entryInstruction;

		readonly ZBool shouldDeclareUCRInLine;
		readonly ZBool shouldDeclareAddSupplyActorsInLine;
		readonly ZBool shouldDeclareCountryOfDestinationInLine;
		readonly ZBool shouldDeclareCountryOfExportInLine;
		readonly ZBool shouldDeclarePreviousDocumentsInLine;

		public ZString LineNumber => entryLine.CL_LineNumber.ToString();

		public IReadOnlyCollection<IDeclarationDVDSupportingDocumentForLine> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					var supportingDocumentsList = new List<DeclarationDVDSupportingDocumentForLineWrapper>();

					var docs = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()).ToList();
					docs.AddRange(entryLine.Header.InvoiceHeaders.Cast<JobComInvoiceHeader>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()));
					var distinctOnes = IEnumerableExtensions.DistinctBy(docs,doc => new { doc.CSI_Code, doc.CSI_ReferenceNumber, doc.CSI_DateOfIssue, doc.CSI_DateOfExpiry, doc.CSI_Procedure, doc.CSI_AdditionalDescription, doc.CSI_ItemNumber });
					distinctOnes.ForEach(doc => supportingDocumentsList.Add(new DeclarationDVDSupportingDocumentForLineWrapper(doc)));

					entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType == SupportingDocumentSubType.LIQ && x.CSI_Status != DocumentStatus.Accepted)
																	.ForEach(doc => supportingDocumentsList.Add(new DeclarationDVDSupportingDocumentForLineWrapper(doc)));
					supportingDocuments = supportingDocumentsList.AsReadOnly();
				}
				return supportingDocuments;
			}
		}
		IReadOnlyCollection<DeclarationDVDSupportingDocumentForLineWrapper> supportingDocuments;

		public IReadOnlyCollection<IDeclarationDVDAdditionalInfo> AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					var additionalInfosList = new List<DeclarationDVDAdditionalInfoWrapper>();

					entryLine.AdditionalInfos.Cast<AdditionalInfo>().ForEach(doc => additionalInfosList.Add(new DeclarationDVDAdditionalInfoWrapper(doc)));
					randomLine.InvoiceHeader.AdditionalInfos.Cast<AdditionalInfo>().ForEach(doc => additionalInfosList.Add(new DeclarationDVDAdditionalInfoWrapper(doc)));
					entryLine.Declaration.AdditionalInfos.Cast<AdditionalInfo>().ForEach(doc => additionalInfosList.Add(new DeclarationDVDAdditionalInfoWrapper(doc)));
					additionalInfos = additionalInfosList.AsReadOnly();
				}
				return additionalInfos;
			}
		}
		IReadOnlyCollection<DeclarationDVDAdditionalInfoWrapper> additionalInfos;

		public IReadOnlyCollection<IAdditionalSupplyChainActorCommon> AdditionalSupplyActors
		{
			get
			{
				if (additionalSupplyActors == null)
				{
					var additionalSupplyActorsList = new List<AdditionalSupplyChainActorCommonWrapper>();

					if (shouldDeclareAddSupplyActorsInLine)
					{
						entryInstruction.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().ForEach(x => additionalSupplyActorsList.Add(new AdditionalSupplyChainActorCommonWrapper(x)));
						entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(i => i.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().ForEach(x => additionalSupplyActorsList.Add(new AdditionalSupplyChainActorCommonWrapper(x))));
					}

					additionalSupplyActors = additionalSupplyActorsList.AsReadOnly();
				}
				return additionalSupplyActors;
			}
		}
		IReadOnlyCollection<AdditionalSupplyChainActorCommonWrapper> additionalSupplyActors;

		public ZString Description => randomLine.JI_Description;

		public ZString CusCode => randomLine.ZG_CusNumber;

		public ZString TariffCode => randomLine.JI_Tariff.SubstringSafe(0, (entryLine.RandomLine.JI_Tariff.Length < 8 ? entryLine.RandomLine.JI_Tariff.Length : 8));

		public ZString TariffCodeCombined => randomLine.JI_Tariff.SubstringSafe(8, 2);

		public IReadOnlyCollection<ZString> TariffAdditionalCodes
		{
			get
			{
				if (tariffAdditionalCodes == null)
				{
					var tariffAdditionalCodesList = new List<ZString>();

					AddSupplementaryCodeIfNotEmpty(entryLine.SupplementaryCode1);
					AddSupplementaryCodeIfNotEmpty(entryLine.SupplementaryCode2);
					randomLine.JI_AdditionalSupplements.Split(",").ForEach(x => AddSupplementaryCodeIfNotEmpty(x));

					void AddSupplementaryCodeIfNotEmpty(ZString supplementaryCode)
					{
						if (!supplementaryCode.IsEmpty && tariffAdditionalCodesList.Count < 2)
						{
							tariffAdditionalCodesList.Add(supplementaryCode);
						}
					}
					tariffAdditionalCodes = tariffAdditionalCodesList.AsReadOnly();
				}
				return tariffAdditionalCodes;
			}
		}
		IReadOnlyCollection<ZString> tariffAdditionalCodes;

		public IReadOnlyCollection<ZString> NationalAdditionalCodes
		{
			get
			{
				if (nationalAdditionalCodes == null)
				{
					var nationalAdditionalCodesList = new List<ZString>();

					nationalAdditionalCodesList.Add(randomLine.ZG_ExciseCode + randomLine.ZG_ExciseExemption);

					nationalAdditionalCodes = nationalAdditionalCodesList.AsReadOnly();
				}
				return nationalAdditionalCodes;
			}
		}
		IReadOnlyCollection<ZString> nationalAdditionalCodes;

		public ZString PreferenceCode => randomLine.JI_PrimaryPreference.Left(1);

		public ZString ReductionCode => randomLine.JI_PrimaryPreference.SubstringSafe(1, 2);

		public IReadOnlyCollection<IDeclarationDVDTax> Taxes => taxes ?? (taxes = DeclarationDVDTaxWrapper.GetTaxesList(entryLine));
		IReadOnlyCollection<DeclarationDVDTaxWrapper> taxes;

		public ZDecimal NetMass => entryLine.EffectiveCustomsWeight.InKilogramsSafe;

		public ZDecimal GrossMass => entryLine.EffectiveGrossWeight.InUnroundedKilogramsSafe;

		public ZDecimal SupplementaryUnitsQty => entryLine.SupplementaryQuantity;

		public IReadOnlyCollection<ZString> Containers => containers ?? (containers = entryLine.Containers.ToList().AsReadOnly());
		IReadOnlyCollection<ZString> containers;

		public ZString CountryOfDestination => shouldDeclareCountryOfDestinationInLine ? randomLine.ZG_CountryOfDestination : ZString.Empty;

		public ZString CountryOfExport => shouldDeclareCountryOfExportInLine ? randomLine.ZG_CountryOfSupply : ZString.Empty;

		public ZString RequestedCPC => entryLine.ProcedureCode.Left(2);

		public ZString PreviousCPC => entryLine.ProcedureCode.SubstringSafe(2, 2);

		public IReadOnlyCollection<IDeclarationDVDEUAndNationalCodes> AdditionalProcedures
		{
			get
			{
				if (additionalProcedures == null)
				{
					var additionalProceduresList = new List<DeclarationDVDEUAndNationalCodesWrapper>();

					var additionalCodesList = randomLine.GetAdditionalProcedureCodesList();

					ZShort seqNum = 1;
					foreach (var addCode in additionalCodesList)
					{
						if (seqNum > 5)
						{
							break;
						}

						additionalProceduresList.Add(new DeclarationDVDEUAndNationalCodesWrapper(addCode));
						seqNum++;
					}

					additionalProcedures = additionalProceduresList.AsReadOnly();
				}
				return additionalProcedures;
			}
		}
		IReadOnlyCollection<DeclarationDVDEUAndNationalCodesWrapper> additionalProcedures;

		public ZString CountryOfOrigin => randomLine.JI_CountryOfOrigin;

		public IReadOnlyCollection<IDVDCommonPackage> Packages => packages ?? (packages = DVDCommonPackageWrapper.GetPackagesList(entryLine));
		IReadOnlyCollection<DVDCommonPackageWrapper> packages;

		public IReadOnlyCollection<IDeclarationDVDVehicle> Vehicles
		{
			get
			{
				if (vehicles == null)
				{
					var vehiclesList = new List<DeclarationDVDVehicleWrapper>();
					entryLine.InvoiceLinesWithVehicles.ForEach(line => line.Vehicles.Cast<CusVehicle>().ForEach(vehicle => vehiclesList.Add(new DeclarationDVDVehicleWrapper(vehicle.CVH_VehicleIdentificationNumber, vehicle.CVH_BrandName, vehicle.CVH_ModelName))));
					vehicles = vehiclesList.AsReadOnly();
				}
				return vehicles;
			}
		}
		IReadOnlyCollection<DeclarationDVDVehicleWrapper> vehicles;

		public IReadOnlyCollection<IDeclarationDVDPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					var previousDocumentsList = new List<DeclarationDVDPreviousDocumentWrapper>();

					if (shouldDeclarePreviousDocumentsInLine)
					{
						entryLine.PreviousDocuments.ForEach(doc => previousDocumentsList.Add(new DeclarationDVDPreviousDocumentWrapper((PreviousDocument)doc)));
					}

					previousDocuments = previousDocumentsList.AsReadOnly();
				}
				return previousDocuments;
			}
		}
		IReadOnlyCollection<DeclarationDVDPreviousDocumentWrapper> previousDocuments;

		public ZString UCRReferenceNumber => shouldDeclareUCRInLine ? randomLine.ZG_CommercialReference : ZString.Empty;
	}
}
