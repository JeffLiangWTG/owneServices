using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNJobDeclarationDataObjectWriter : DeclarationDataObjectWriter
	{
		public CNJobDeclarationDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);
			if (declarationBO is JobDeclaration declaration)
			{
				declarationData.AddOrgAddress(writeManager, declaration.Declarant, AddressTypes.Declarant);
			}
		}

		protected override void PopulateBuyer(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var buyer = declarationBO.Buyer;
			if (buyer != null && buyer != declarationBO.BuyerDocAddress.Organisation)
			{
				declarationData.AddOrgAddress(writeManager, buyer, Constants.DocumentaryAddressTypes.Buyer);
			}
		}

		protected override CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriter()
		{
			return new CNEntryInstructionDataObjectWriter(writeManager, helper);
		}

		protected override UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return new CNDataObjectWriterHelper(declarationBO.Factory);
		}

		protected override CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CNEntryHeaderDataObjectWriter(writeManager, helper);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetJobComInvoiceHeaderRelatedFetchHints(row))
			{
				yield return hint;
			}
			foreach (var fetchHint in GetFetchHintsIfNotEmpty(row, JobComInvoiceHeaderSchema.JZ_OH_Manufacturer, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH))
			{
				yield return fetchHint;
			}
			yield return new FetchHint(JobComInvoiceHeaderRefsSchema.J2_JZ, row.GetValue(JobComInvoiceHeaderSchema.PK));
		}

		protected override void PopulateCountrySpecificContainerValue(Container containerData, BaseCusContainer containerBO, BaseJobDeclaration declarationBO)
		{
			if (containerBO is CusContainer cnCusContainer)
			{
				containerData.CustomsContainerSize = new CodeDescriptionPair2Char
				{
					Code = cnCusContainer.ContainerCode
				};
			}
		}

		protected override CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		{
			return new CNInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return hint;
			}
			var pk = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, pk);
			yield return new FetchHint(CusSupportingInfoSchema.CSI_ParentID, pk);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, pk);
		}

		protected override DataObjectList<CommercialInvoiceHeader> GetCommercialInvoiceCollection(BaseJobComInvoiceHeader[] invoiceBOs, CommercialInvoiceHeaderDataObjectWriter commercialInvoiceHeaderDataObjectWriter)
		{
			var factory = invoiceBOs.FirstOrDefault()?.Factory;
			if (factory != null)
			{
				var invoiceLines = invoiceBOs.SelectMany(invoice => invoice.InvoiceLines).Cast<JobComInvoiceLine>();

				foreach (var invoiceLine in invoiceLines.Where(line => !line.JI_CIQTariff.IsEmpty))
				{
					factory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.China, Business.Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff, invoiceLine.JI_CIQTariff, invoiceLine.EffectiveAssessmentDate));
				}
				foreach (var ciqTariffPK in invoiceLines.Select(line => line.CIQTariff).Where(x => x != null).Select(tariff => tariff.PK).Distinct())
				{
					factory.AddFetchHint(CusRefTariffLanguageViewSchema.ZX7_ZZ1_Tariff, ciqTariffPK);
				}
			}

			return base.GetCommercialInvoiceCollection(invoiceBOs, commercialInvoiceHeaderDataObjectWriter);
		}

		protected override IEnumerable<IFetchHint> GetEntryInstructionRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetEntryInstructionRelatedFetchHints(row))
			{
				yield return hint;
			}
			var pk = row.GetValue(CusEntryInstructionSchema.PK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, pk);
			yield return new FetchHint(CusSupportingInfoSchema.CSI_ParentID, pk);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, pk);
			yield return new FetchHint(CusStorageDocPivotSchema.CSD_ParentID, pk);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, pk);
			yield return new FetchHint(CusEntryNumSchema.CE_ParentID, pk);
		}

		protected override void PopulateManufacturer(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var manufacturerDocAddress = ((JobDeclaration)declarationBO).ManufacturerDocumentaryAddress;
			if (manufacturerDocAddress != null)
			{
				declarationData.AddOrgAddress(writeManager, manufacturerDocAddress);
			}
		}

		protected override IEnumerable<IeDoc> GeteDocsToPopulate(BaseJobDeclaration declarationBO)
		{
			var entries = GetEntryHeadersToPopulate(declarationBO);
			return entries.Select(x => x.EntryInstruction).Cast<Business.CusEntryInstruction>().Where(x => x != null)
				.SelectMany(x => x.Attachments.Cast<EntryInstructionAttachment>())
				.Select(x => x.Document)
				.Where(x => x != null).Distinct()
				.OrderBy(x => x.FileName);
		}

		protected override void PopulateDeclarationAddInfoFromRealFieldCore(BaseJobDeclaration declarationBO, List<UniversalDataBuss.DataObjects.Universal.AddInfo> addInfos)
		{
			base.PopulateDeclarationAddInfoFromRealFieldCore(declarationBO, addInfos);
			if (declarationBO is JobDeclaration cnDeclaration && cnDeclaration.CusAgent != null)
			{
				addInfos.AddIfMissing(Constants.AddInfoKeys.JobDeclaration.BrokerNumber, cnDeclaration.BrokerCertificateNumber);
				addInfos.AddIfMissing(Constants.AddInfoKeys.JobDeclaration.OperatorCardID, cnDeclaration.OperatorCardID);

				if (!cnDeclaration.BrokerCertificateNumber.IsEmpty)
				{
					addInfos.AddIfMissing(Constants.AddInfoKeys.JobDeclaration.BrokerName, cnDeclaration.NameOnBrokerCertificate);
				}

				if (!cnDeclaration.OperatorCardID.IsEmpty)
				{
					addInfos.AddIfMissing(Constants.AddInfoKeys.JobDeclaration.OperatorName, cnDeclaration.NameOnOperatorCard);
				}
			}

			var destination = CNCustomsDataRegistry.Instance.CNSWClientSetting.GetFallBackValueAtAllLevels(declarationBO.RegistryCompanyPK, declarationBO.RegistryBranchPK, Guid.Empty)?.EHubClientID ?? ZString.Empty;
			addInfos.AddIfMissing(Constants.AddInfoKeys.JobDeclaration.DestinationParty, destination);
		}
	}
}
