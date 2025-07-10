using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public abstract class GoodsItemDataObjectReader<T> : DataObjectReader<CommercialInvoiceLine, T> where T : NctsCommonCargoDesc
	{
		protected GoodsItemDataObjectReader(Shipment moveHeaderDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, NctsHeader header, NctsCommonMovementHeader moveHeader, CommercialInvoiceLine commercialInvoiceLine, ZShort currentLineNumber)
					: base(commercialInvoiceLine, logger, helper.Factory)
		{
			this.moveHeaderDataObject = Argument.NotNull(moveHeaderDataObject, nameof(moveHeaderDataObject));
			this.header = Argument.NotNull(header, nameof(header));
			Helper = Argument.NotNull(helper, nameof(helper));
			this.moveHeader = moveHeader;
			this.currentLineNumber = currentLineNumber;
		}

		protected readonly NctsHeader header;
		protected UniversalDataObjectReaderHelper Helper { get; }
		readonly Shipment moveHeaderDataObject;
		readonly NctsCommonMovementHeader moveHeader;
		readonly ZShort currentLineNumber;

		protected override T GetExistingBusinessObject() => null;

		protected override T GetNewBusinessObject() => (T)factory.New(moveHeader.CusInBondCargoDescType);

		protected override void PopulateBusinessObject(T goodsItemBO)
		{
			var goodsItemRow = GetColumnIndexer(goodsItemBO);
			var goodsItemPK = goodsItemRow.GetValue(CusInBondCargoDescSchema.PK);

			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_ParentID, moveHeader.PK);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_ParentTableCode, moveHeader.TablePrefix);

			if (IsDefaultingEnabled)
			{
				SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_LineNo, currentLineNumber);
			}
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_Description, dataObject.Description);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_NetWeight, dataObject.NetWeight);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_NetWeightUnit, dataObject.NetWeightUnit?.Code);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_GrossWeight, dataObject.Weight);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_GrossWeightUnit, dataObject.WeightUnit?.Code);
			SetValue(goodsItemRow, CusInBondCargoDescSchema.BY_HarmonisedTariff, dataObject.HarmonisedCode);

			FillPackagesAndContainersData(goodsItemBO);

			new AddInfoGroupCollectionDataObjectReader(logger, Helper).ReadIntoDataRows(goodsItemPK, CusInBondCargoDescSchema.Constants.Prefix, goodsItemBO.IsInDatabase, dataObject);
			new CustomsSupportingInformationCollectionDataObjectReader(logger, Helper).ReadIntoDataRows(goodsItemPK, CusInBondCargoDescSchema.Constants.Prefix, goodsItemBO.IsInDatabase, dataObject);
		}

		void FillPackagesAndContainersData(T goodsItemBO)
		{
			if (moveHeaderDataObject.PackingLineCollection != null)
			{
				ClearPackagesAndContainers(goodsItemBO);
				var packingLineCollection = moveHeaderDataObject.PackingLineCollection.Where(x => x.EntryType.Equals(goodsItemBO.MoveHeader.BM_SubApplicationCode) && x.ItemNo == dataObject.LineNo);
				foreach (var packingLine in packingLineCollection)
				{
					FillPackage(goodsItemBO, packingLine);
				}
			}
		}

		protected virtual void ClearPackagesAndContainers(T goodsItemBO)
		{
			goodsItemBO.Packages.RemoveAndDeleteAll();
		}

		protected void FillPackage(T goodsItemBO, PackingLine packingLine)
		{
			if (packingLine.PackType.GetNullableCodeAsUpperCase().HasValue)
			{
				var packageItem = goodsItemBO.Packages.AddNew();
				var packageRow = GetColumnIndexer(packageItem);
				SetValue(packageRow, CusInvPackSchema.B5_UnitType, packingLine.PackType?.Code);
				SetValue(packageRow, CusInvPackSchema.B5_UnitCount, packingLine.PackQty.HasValue ? new ZLong(packingLine.PackQty.Value) : null);
				SetValue(packageRow, CusInvPackSchema.B5_PackageID, packingLine.InBondPackQty?.ToString());
				SetValue(packageRow, CusInvPackSchema.B5_MarksAndNumbers, packingLine.MarksAndNos);
			}
			else
			{
				FillPackageIfNoPackType(goodsItemBO, packingLine);
			}
		}

		protected virtual void FillPackageIfNoPackType(T goodsItemBO, PackingLine packingLine)
		{
		}
	}
}
