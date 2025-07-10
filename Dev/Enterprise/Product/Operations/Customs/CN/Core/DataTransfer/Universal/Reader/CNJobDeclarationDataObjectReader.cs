using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNJobDeclarationDataObjectReader : JobDeclarationDataObjectReader
	{
		public CNJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null) : base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected override CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, BaseJobDeclaration declaration)
		{
			return new CNEntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);
		}

		protected override CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CNInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}

		protected override CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, BaseJobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CNContainerDataObjectReader(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override IEnumerable<ZString> GetSettingOrder(BaseJobDeclaration declaration)
		{
			foreach (var setting in base.GetSettingOrder(declaration))
			{
				yield return setting;
			}
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Buyer);
			yield return ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Manufacturer);
		}

		protected override void FillDeclarant(List<OrganizationAddress> organizationAddressCollection, BaseJobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
		}

		protected override void FillBuyer(List<OrganizationAddress> organizationAddressCollection, BaseJobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declarationRow = GetColumnIndexer(declaration);
			var buyerData =
				organizationAddressCollection.FirstOrDefault(Constants.DocumentaryAddressTypes.Buyer)
				?? organizationAddressCollection.FirstOrDefault(nameof(DocAddressType.BuyerDocumentaryAddress));

			if (buyerData != null)
			{
				var buyerDataReader = new OrganisationDataObjectReader(buyerData, logger, factory);
				var buyerOrgAddress = buyerDataReader.GetMatched(declaration, OrganisationTypes.Consignee);

				if (buyerOrgAddress != null)
				{
					SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Buyer, buyerOrgAddress, delaySetters);
				}
			}
		}

		protected override void FillManufacturer(List<OrganizationAddress> orgAddresses, BaseJobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declarationRow = GetColumnIndexer(declaration);

			var manufacturerDocAddress = orgAddresses.FirstOrDefault(nameof(DocAddressType.Manufacturer));
			if (manufacturerDocAddress != null)
			{
				var manufacturerReader = new OrganisationDataObjectReader(manufacturerDocAddress, logger, factory);
				SetOrganisationPK(declarationRow, JobDeclarationSchema.JE_OH_Manufacturer, manufacturerReader.GetMatched(declaration, OrganisationTypes.Consignor), delaySetters);
			}
		}

		protected override void AddFetchHintsRelatedToCommerialInvoiceLineTariff(BaseJobDeclaration declaration, List<ZString> tariffCodes)
		{
			base.AddFetchHintsRelatedToCommerialInvoiceLineTariff(declaration, tariffCodes);

			var fetchHintFactory = factory.BOFactory;

			var loader = new TariffView.Loader(fetchHintFactory);
			var dateOfValuation = declaration.DateOfValuation;

			foreach (var tariffCode in tariffCodes)
			{
				fetchHintFactory.AddFetchHint(typeof(TariffView), loader.GetEffectiveTariffFilter(Core.Constants.CountryCodes.China, Customs.Universal.Constants.TariffTypes.HarmonizedSystem, tariffCode, dateOfValuation, null, ZString.Empty));
			}

			foreach (var tariffCode in tariffCodes)
			{
				var tariff = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Customs.Universal.Constants.TariffTypes.HarmonizedSystem, tariffCode, declaration.DateForDutyRate);
				if (tariff != null)
				{
					tariff.FetchForLoadChildEditableObjectsIfNeeded();
				}
			}
		}
	}
}
