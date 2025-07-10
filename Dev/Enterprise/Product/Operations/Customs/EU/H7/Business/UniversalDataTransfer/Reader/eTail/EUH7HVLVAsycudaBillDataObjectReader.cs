using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgCusCode;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer
{
	public class EUH7HVLVAsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public EUH7HVLVAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override void FillPackingLines(ASYCUDA.Business.AsycudaBill bill)
		{
			if (!(header.FeatureProvider?.SupportsAsycudaPacks ?? ((ZBool)false)) || dataObject.PackingLineCollection == null)
			{
				return;
			}

			helper.PacksReaderHelper.MarkUnprocessedExistingObjectFor(factory, bill);
			bill.PackedItems.RemoveAndDeleteAll();

			if (dataObject.PackingLineCollection != null)
			{
				foreach (var packingLineDataObject in dataObject.PackingLineCollection)
				{
					var pack = new EUH7HVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, factory, (AsycudaBill)bill, dataObject).ReadIntoBusinessObject();
					helper.PacksReaderHelper.MarkProcessed(pack);
				}
			}

			helper.PacksReaderHelper.DeleteUnprocessedObjectsFor(bill, logger);
		}

		protected override void PopulateBillForSpecificRules(ASYCUDA.Business.AsycudaBill bill)
		{
			base.PopulateBillForSpecificRules(bill);
			var billRow = GetColumnIndexer(bill);

			var exporterAdditionalReferenceNumber = GetAdditionalReferenceNumber(CustomsAdditionalReferenceNumbersCodes.ExporterEORINumber);
			var importerAdditionalReferenceNumber = GetAdditionalReferenceNumber(CustomsAdditionalReferenceNumbersCodes.ImporterEORINumber);
			var ucrAdditionalReferenceNumber = GetAdditionalReferenceNumber(CustomsAdditionalReferenceNumbersCodes.UniqueConsignmentReference);

			SetValue(billRow, AsycudaBillSchema.ABL_ShipperRegNo, exporterAdditionalReferenceNumber);
			SetValue(billRow, AsycudaBillSchema.ABL_ShipperRegNoType, !string.IsNullOrEmpty(exporterAdditionalReferenceNumber) ? EuropeanUnionSharedCodeTypes.Eori : string.Empty);

			SetValue(billRow, AsycudaBillSchema.ABL_ConsigneeRegNo, importerAdditionalReferenceNumber);
			SetValue(billRow, AsycudaBillSchema.ABL_ConsigneeRegNoType, !string.IsNullOrEmpty(importerAdditionalReferenceNumber) ? EuropeanUnionSharedCodeTypes.Eori : string.Empty);

			SetValue(billRow, AsycudaBillSchema.ABL_UCRNumber, ucrAdditionalReferenceNumber);

			SetValue(billRow, AsycudaBillSchema.ABL_TransportValue, dataObject.TransportValue);
			SetValue(billRow, AsycudaBillSchema.ABL_RX_NKTransportValueCurrency, dataObject.GoodsValueCurrency);

			SetValue(billRow, AsycudaBillSchema.ABL_InsuranceValue, dataObject.InsuranceValue);
			SetValue(billRow, AsycudaBillSchema.ABL_RX_NKInsuranceValueCurrency, dataObject.GoodsValueCurrency);

			SetValue(billRow, AsycudaBillSchema.ABL_Incoterm, dataObject.ShipmentIncoTerm);

			SetValue(billRow, AsycudaBillSchema.ABL_SellerRegNo, dataObject.VendorIdentifier);
		}

		protected override ASYCUDA.Business.AsycudaBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return header.IsInDatabase ? base.GetExistingBusinessObjectUsingModuleSpecificBusinessRules() : null;
		}

		string GetAdditionalReferenceNumber(string type)
		{
			return dataObject.AdditionalReferenceCollection?.FirstOrDefault(reference => reference.Type.GetCodeAsUpperCase() == type)?.ReferenceNumber;
		}
	}
}
