using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2AsycudaManifestHeaderDataObjectWriter : AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>
	{
		public ICS2AsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateManifestSpecificData(AsycudaManifestHeader headerBO, Shipment headerData, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			PopulateHeaderGenAddOnColumns(headerBO, headerData);
			PopulateMasterBillGenAddOnColumns(headerBO, headerData);
			PopulateMasterBillScreenings(headerBO, headerData, headerHelper);

			var customsReferencesList = GetCustomsReferencesList(headerBO);
			headerData.SetCustomsReferenceCollection(() => customsReferencesList);
		}

		void PopulateHeaderGenAddOnColumns(AsycudaManifestHeader headerBO, Shipment headerData)
		{
			var addInfoCollection = headerData.AddInfoCollection;
			addInfoCollection.Add(new AddInfo { Key = AddInfoConstants.Header.SpecificCircumstanceIndicator, Value = headerBO.SpecificCircumstanceIndicator });
			addInfoCollection.Add(new AddInfo { Key = AddInfoConstants.Header.ReEntryIndicator, Value = headerBO.ReEntryIndicator.ToString() });
			addInfoCollection.Add(new AddInfo { Key = AddInfoConstants.Header.SplitConsignmentIndicator, Value = headerBO.SplitConsignmentIndicator.ToString() });
		}

		void PopulateMasterBillGenAddOnColumns(AsycudaManifestHeader headerBO, Shipment headerData)
		{
			var masterBillBO = headerBO.MasterBill;
			if (masterBillBO != null)
			{
				var addInfoCollection = headerData.AddInfoCollection;
				addInfoCollection.Add(new AddInfo { Key = AddInfoConstants.Bill.TransportDocumentType, Value = masterBillBO.TransportDocumentType });
			}
		}

		void PopulateMasterBillScreenings(AsycudaManifestHeader headerBO, Shipment headerData, AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			var masterBillBO = headerBO.MasterBill;
			if (masterBillBO != null)
			{
				var writer = new ICS2AsycudaBillDataObjectWriter(writeManager, headerHelper);
				writer.PopulateBillScreens(masterBillBO, headerData);
			}
		}

		List<CustomsReference> GetCustomsReferencesList(AsycudaManifestHeader headerBO)
		{
			var customsReferencesList = new List<CustomsReference>();
			var cusSupplyChainActorReferences = headerBO.CusSupplyChainActorReferences;

			foreach (CusSupplyChainActorReference cusSupplyChainActorReference in cusSupplyChainActorReferences)
			{
				var customsReference = new CustomsReference
				{
					Type = new CodeDescriptionPair { Code = CusReferenceTypeList.Codes.SupplyChainActor },
					SubType = new CodeDescriptionPair35Char { Code = cusSupplyChainActorReference.CFR_Code },
					Reference = cusSupplyChainActorReference.CFR_Reference,
					Owner = new OrganizationDataObjectWriter(writeManager, AddressTypes.Owner).GetDataObject(cusSupplyChainActorReference.Owner)
				};

				customsReferencesList.Add(customsReference);
			}

			return customsReferencesList;
		}

		protected override IAsycudaBillDataObjectWriter CreateNewAsycudaBillDataObjectWriter(AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			return new ICS2AsycudaBillDataObjectWriter(writeManager, headerHelper);
		}
	}
}
