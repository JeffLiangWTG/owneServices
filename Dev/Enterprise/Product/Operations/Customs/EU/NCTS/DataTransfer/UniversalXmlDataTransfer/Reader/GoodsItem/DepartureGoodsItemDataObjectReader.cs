using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class DepartureGoodsItemDataObjectReader : GoodsItemDataObjectReader<NctsDepartureCargoDesc>
	{
		public DepartureGoodsItemDataObjectReader(Shipment moveHeaderDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, NctsHeader header, NctsCommonMovementHeader moveHeader, CommercialInvoiceLine commercialInvoiceLine, ZShort currentLineNumber)
			: base(moveHeaderDataObject, logger, helper, header, moveHeader, commercialInvoiceLine, currentLineNumber)
		{
		}

		protected override void PopulateBusinessObject(NctsDepartureCargoDesc goodsItemBO)
		{
			base.PopulateBusinessObject(goodsItemBO);
			var goodsItemRow = GetColumnIndexer(goodsItemBO);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfOrigin, dataObject.CountryOfOrigin?.Code);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfDispatch, dataObject.CountryOfExport?.Code);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfDestination, dataObject.AddInfoCollection.GetZStringValue(DataObjectWriterConstants.GoodsItem.AddInfo.CountryOfDestination));
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_Type, dataObject.AddInfoCollection.GetZStringValue(DataObjectWriterConstants.GoodsItem.AddInfo.DeclarationType));
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CommercialReferenceNumber, dataObject.AddInfoCollection.GetZStringValue(DataObjectWriterConstants.GoodsItem.AddInfo.CommercialReferenceNumber));
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_TransportChargesMethodOfPayment, dataObject.AddInfoCollection.GetZStringValue(DataObjectWriterConstants.GoodsItem.AddInfo.TransportChargesMoP));
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsSecondQuantity, dataObject.CustomsSecondQuantity);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsSecondUnitQty, dataObject.CustomsSecondQuantityUnit);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_MonetaryValue, dataObject.CustomsValue);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_ZZF_NKTaxType, dataObject.TaxType);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_Procedure, dataObject.Procedure);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RW_NKOriginState, dataObject.StateOfOrigin);

			FillJobDocAddresses(goodsItemBO);
			FillAddInfoCollection(goodsItemBO);
			FillCustomsReferenceCollection(goodsItemBO);
		}

		protected override void ClearPackagesAndContainers(NctsDepartureCargoDesc goodsItemBO)
		{
			base.ClearPackagesAndContainers(goodsItemBO);
			goodsItemBO.ContainersPivots.RemoveAndDeleteAll();
		}

		protected override void FillPackageIfNoPackType(NctsDepartureCargoDesc goodsItemBO, PackingLine packingLine)
		{
			var container = header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().FirstOrDefault(x => packingLine.ContainerNumber.Equals(x.BC_ContainerNum));
			if (container == null)
			{
				container = header.DepartureHeaderContainers.AddNew();
				var containerRow = GetColumnIndexer(container);
				SetValue(containerRow, CusInBondContainerSchema.BC_ContainerNum, packingLine.ContainerNumber);
			}

			var query = new ZQuery(GenPivotSchema.XX_RelationType, "NCT");
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, goodsItemBO.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, container.PK);
			var genPivot = factory.LoadTop1<GenPivot>(query);
			if (genPivot == null)
			{
				var containerPivotItem = goodsItemBO.ContainersPivots.AddNew();
				containerPivotItem.Container = container;
				containerPivotItem.ContainerSelected = true;
			}
		}

		void FillJobDocAddresses(NctsDepartureCargoDesc docAddresses)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var supportedAddressTypes = ((IDocAddresses)docAddresses).SupportedAddressTypes;
				if (supportedAddressTypes != null && supportedAddressTypes.Count > 0)
				{
					foreach (var orgAddressDataObject in dataObject.OrganizationAddressCollection)
					{
						var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
						if (System.Enum.TryParse(addressType, false, out DocAddressType docAddressType) && supportedAddressTypes.Contains(docAddressType))
						{
							OrganisationDataObjectReader.MatchedOrNew(docAddresses, orgAddressDataObject, logger, factory, null);
						}
					}
				}
			}
		}

		void FillAddInfoCollection(NctsDepartureCargoDesc goodsItemBO)
		{
			if (dataObject.AddInfoCollection != null)
			{
				var uNDangerousGoodsCodeValue = dataObject.AddInfoCollection.GetZStringValue(DataObjectWriterConstants.GoodsItem.AddInfo.UNDangerousGoodsCode);
				if (!uNDangerousGoodsCodeValue.ToString().IsNullOrEmpty())
				{
					var uNDangerousGoodsCode = goodsItemBO.UNDGs.Count > 0 ? goodsItemBO.UNDGs.FirstOrDefault() : goodsItemBO.UNDGs.AddNew();
					var standard = dataObject.AddInfoCollection.GetZStringValue(DataObjectWriterConstants.GoodsItem.AddInfo.UNDangerousGoodsStandard);
					var substances = UNDGSubstanceLoader.LoadSubstances(factory.BOFactory, uNDangerousGoodsCodeValue.Value.SubstringSafe(0, 4), uNDangerousGoodsCodeValue.Value.SubstringSafe(4, 2), standard);
					var uNDangerousGoodsCodeRow = GetColumnIndexer(uNDangerousGoodsCode);
					SetValue(uNDangerousGoodsCodeRow, UNDGDataItemSchema.DI_DG, substances?.FirstOrDefault()?.PK ?? ZGuid.Empty);
				}
			}
		}

		void FillCustomsReferenceCollection(NctsDepartureCargoDesc goodsItemBO)
		{
			new CustomsReferenceCollectionDataObjectReader(logger, Helper).ReadIntoDataRows(goodsItemBO.PK, goodsItemBO.TablePrefix, goodsItemBO.IsInDatabase, dataObject);
		}
	}
}
