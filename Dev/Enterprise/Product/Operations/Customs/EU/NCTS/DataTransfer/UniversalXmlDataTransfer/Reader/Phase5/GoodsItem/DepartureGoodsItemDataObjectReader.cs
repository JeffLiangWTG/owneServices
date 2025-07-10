using System.Linq;
using CargoWise.Common;
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
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	public class DepartureGoodsItemDataObjectReader : DataObjectReader<InBondMoveLineItem, NctsDepartureCargoDesc>
	{
		public DepartureGoodsItemDataObjectReader(Shipment moveHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalDataObjectReaderHelper helper, NctsHeader header, NctsBill bill, InBondMoveLineItem lineItem, ZInt? itemLink)
			: base(lineItem, logger, factory)
		{
			this.moveHeaderDataObject = Argument.NotNull(moveHeaderDataObject, nameof(moveHeaderDataObject));
			this.header = Argument.NotNull(header, nameof(header));
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.itemLink = itemLink;
			this.helper = helper;
		}

		readonly Shipment moveHeaderDataObject;
		readonly NctsHeader header;
		readonly NctsBill bill;
		readonly ZInt? itemLink;
		readonly UniversalDataObjectReaderHelper helper;

		protected override NctsDepartureCargoDesc GetNewBusinessObject()
		{
			return bill.GoodsItems.AddNew();
		}

		protected override NctsDepartureCargoDesc GetExistingBusinessObject() => null;

		protected override void PopulateBusinessObject(NctsDepartureCargoDesc goodsItemBO)
		{
			var goodsItemRow = GetColumnIndexer(goodsItemBO);

			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_ParentID, bill.PK);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_ParentTableCode, bill.TablePrefix);

			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_LineNo, dataObject.LineNumber);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_Description, dataObject.DescriptionAndQuantityOfMerchandise);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_NetWeight, dataObject.NetWeight);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_NetWeightUnit, dataObject.NetWeightUnit);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_GrossWeight, dataObject.Weight);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_GrossWeightUnit, dataObject.WeightUnit);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_HarmonisedTariff, dataObject.TariffCode);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfOrigin, dataObject.CountryOfOrigin);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfDispatch, dataObject.CountryOfDispatch);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfDestination, dataObject.CountryOfDestination);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsSecondQuantity, dataObject.CustomsSecondQuantity);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsSecondUnitQty, dataObject.CustomsSecondQuantityUnit);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_Type, dataObject.DeclarationType);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CommercialReferenceNumber, dataObject.ReferenceNumber);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_TransportChargesMethodOfPayment, dataObject.TransportPaymentMethod);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CusC4Number, dataObject.HazardousMaterial?.Code);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsQuantity, dataObject.CustomsFirstQuantity);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsUnitQty, dataObject.CustomsFirstQuantityUnit);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsThirdQuantity, dataObject.CustomsThirdQuantity);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsThirdUnitQty, dataObject.CustomsThirdQuantityUnit);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsFourthQuantity, dataObject.CustomsFourthQuantity);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_CustomsFourthUnitQty, dataObject.CustomsFourthQuantityUnit);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_MonetaryValue, dataObject.MonetaryValue);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RX_NKCurrency, dataObject.MonetaryValueCurrency);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_ZZF_NKTaxType, dataObject.TaxType?.Code);
			using (goodsItemBO.SuspendUpdateMonetaryValue())
			{
				SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_LinePrice, dataObject.LinePrice);
				SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_RX_NKLinePriceCurrency, dataObject.LinePriceCurrency);
			}

			FillJobDocAddresses(goodsItemBO);
			FillHazardousMaterial(goodsItemBO);
			FillCustomsReferenceCollection(goodsItemBO);
			FillPackagesAndContainers(goodsItemBO);
			FillRelatedDocuments(goodsItemBO);
		}

		void FillHazardousMaterial(NctsDepartureCargoDesc goodsItemBO)
		{
			if (dataObject.HazardousMaterial?.UNDGCollection != null)
			{
				var uNDangerousGoodsCodeValue = dataObject.HazardousMaterial.UNDGCollection[0].UNDGCode;
				if (!uNDangerousGoodsCodeValue.ToString().IsNullOrEmpty())
				{
					var uNDangerousGoodsCode = goodsItemBO.UNDGs.Count > 0 ? goodsItemBO.UNDGs.FirstOrDefault() : goodsItemBO.UNDGs.AddNew();
					var standard = dataObject.HazardousMaterial.UNDGCollection[0].Standard;
					var substances = UNDGSubstanceLoader.LoadSubstances(factory.BOFactory, uNDangerousGoodsCodeValue.Value.SubstringSafe(0, 4), uNDangerousGoodsCodeValue.Value.SubstringSafe(4, 2), standard);
					var uNDangerousGoodsCodeRow = GetColumnIndexer(uNDangerousGoodsCode);
					SetValue(uNDangerousGoodsCodeRow, UNDGDataItemSchema.DI_DG, substances?.FirstOrDefault()?.PK ?? ZGuid.Empty);
				}
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

		void FillRelatedDocuments(NctsDepartureCargoDesc goodsItemBO)
		{
			if (goodsItemBO is Integration.Customs.ICusSupportingInfoTypeSupporter)
			{
				new CustomsSupportingInformationCollectionDataObjectReader(logger, helper).ReadIntoDataRows(goodsItemBO.PK, goodsItemBO.TablePrefix, goodsItemBO.IsInDatabase, dataObject);
			}
		}

		void FillCustomsReferenceCollection(NctsDepartureCargoDesc goodsItemBO)
		{
			new EU.DataTransfer.Universal.CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(goodsItemBO.PK, goodsItemBO.TablePrefix, goodsItemBO.IsInDatabase, dataObject);
		}

		void ClearPackagesAndContainers(NctsDepartureCargoDesc goodsItemBO)
		{
			goodsItemBO.Packages.RemoveAndDeleteAll();
			goodsItemBO.ContainersPivots.RemoveAndDeleteAll();
		}

		void FillPackagesAndContainers(NctsDepartureCargoDesc goodsItemBO)
		{
			if (moveHeaderDataObject.PackingLineCollection != null)
			{
				ClearPackagesAndContainers(goodsItemBO);
				var existingContainers = header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().ToDictionary(c => c.BC_ContainerNum.ToUpperInvariant(), c => c);
				var packedItems = moveHeaderDataObject.PackingLineCollection
					.Where(line => line.PackedItemCollection != null)
					.SelectMany(line => line.PackedItemCollection.Where(item => item.InBondMoveLineItemLink == itemLink).Select(item => new
					{
						ContainerNumber = line.ContainerNumber.GetValueOrDefault().ToUpperInvariant(),
						MarksAndNumbers = line.MarksAndNos,
						PackType = line.PackType?.Code,
						PackedQuantity = item.PackedQuantity?.ToZLong()
					}))
					.ToArray();
				foreach (var packedItem in packedItems)
				{
					var packageBO = goodsItemBO.Packages.AddNew();
					var packageRow = GetColumnIndexer(packageBO);
					if (packedItem.PackType.HasValue)
					{
						SetValue(packageRow, CusInvPackSchema.B5_MarksAndNumbers, packedItem.MarksAndNumbers);
						SetValue(packageRow, CusInvPackSchema.B5_UnitType, packedItem.PackType);
						SetValue(packageRow, CusInvPackSchema.B5_UnitCount, packedItem.PackedQuantity);

						if (existingContainers.TryGetValue(packedItem.ContainerNumber, out var container))
						{
							packageBO.ContainersPivot.AddPivotFor(container);
						}
					}
				}
			}
		}
	}
}
