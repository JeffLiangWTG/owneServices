using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderEntryInstructionDataObjectReader : DataObjectReader<EntryInstruction, AsycudaManifestHeader>
	{
		public AsycudaManifestHeaderEntryInstructionDataObjectReader(EntryInstruction dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper)
			: base(dataObject, logger, factory)
		{
			this.header = Argument.NotNull(header, "header");
			this.helper = Argument.NotNull(helper, "helper");
		}
		readonly AsycudaManifestHeader header;
		readonly AsycudaManifestDataObjectReaderHelper helper;

		protected override AsycudaManifestHeader GetExistingBusinessObject()
		{
			return header;
		}

		protected sealed override void PopulateBusinessObject(AsycudaManifestHeader headerBO)
		{
			var headerRow = GetColumnIndexer(headerBO);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_ManifestType, dataObject.Style);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_DateAtCustomsOffice, dataObject.DateAtCustomsOffice);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival, dataObject.FirstArrival);
			SetValue(headerRow, AsycudaManifestHeaderSchema.AMA_Nature, dataObject.AddInfoCollection.GetZStringValue(AHC_Nature, logger));

			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(dataObject.AddInfoCollection, helper.GetAsycudaManifestHeaderGenAddOnColumnList(headerBO), headerBO);

			FillCarrier(headerRow);
			FillShippingAgent(headerRow);
		}

		internal const string AHC_Nature = "AHC_Nature";

		void FillCarrier(IColumnIndexer countryRow)
		{
			var carrierAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Carrier));
			if (carrierAddress != null)
			{
				var targetCarrierCCode = carrierAddress.RegistrationNumberCollection?.FirstOrDefault(x => x.CountryOfIssue.GetCodeAsUpperCase() == helper.CountryCode && x.Type.GetCodeAsUpperCase() == OrgCusCode.CodeTypes.CarrierCode)?.Value;
				if (targetCarrierCCode.HasValue)
				{
					SetValue(countryRow, AsycudaManifestHeaderSchema.AMA_CarrierCode, targetCarrierCCode.Value);
				}
			}
		}

		void FillShippingAgent(IColumnIndexer countryRow)
		{
			var shippingAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ControllingAgent));

			if (shippingAddress != null)
			{
				var shippingAddressBO = new OrganisationDataObjectReader(shippingAddress, logger, factory).GetMatched();
				if (shippingAddressBO != null)
				{
					SetValue(countryRow, AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent, shippingAddressBO.PK);
				}
			}
		}
	}
}
