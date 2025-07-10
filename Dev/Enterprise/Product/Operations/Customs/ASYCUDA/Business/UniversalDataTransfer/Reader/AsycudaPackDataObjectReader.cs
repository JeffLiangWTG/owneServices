using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaPackDataObjectReader : DataObjectReader<PackingLine, AsycudaPack>
	{
		public AsycudaPackDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory)
		{
			this.bill = Argument.NotNull(bill, "bill");
			this.helper = Argument.NotNull(helper, "helper");
			this.packAddInfoGroupCollection = dataObject.AddInfoGroupCollection?.Where(x => x.Type.Code.HasValue && x.Type.Code.Value == AddInfoConstants.Pack.PackAddInfoType);
			this.packAddInfoCollection = packAddInfoGroupCollection?.FirstOrDefault()?.AddInfoCollection;
			this.isUpdateEnabled = isUpdateEnabled;
		}

		readonly AsycudaBill bill;
		protected readonly AsycudaManifestDataObjectReaderHelper helper;
		readonly IEnumerable<AddInfoGroup> packAddInfoGroupCollection;
		readonly List<AddInfo> packAddInfoCollection;
		readonly bool isUpdateEnabled;
		AsycudaPack packingLine;

		protected override AsycudaPack GetExistingBusinessObject()
		{
			ZInt? consignmentReference = 0;
			AsycudaPack result = null;
			consignmentReference = packAddInfoCollection?.GetZIntValue(AddInfoConstants.Pack.ConsignmentReference, logger);
			if (consignmentReference.HasValue && consignmentReference.Value != 0)
			{
				result = bill.Packs.OfType<AsycudaPack>().FirstOrDefault(x => x.ConsignmentReference == consignmentReference.Value);
			}
			if (result == null)
			{
				var matchingReference = packAddInfoCollection?.GetZStringValue(AddInfoConstants.Pack.MatchingReference, logger);
				if (matchingReference.HasValue && !matchingReference.Value.IsEmpty)
				{
					result = bill.Packs.OfType<AsycudaPack>().FirstOrDefault(x => x.MatchingReference == matchingReference.Value);
				}
			}
			if (result == null && dataObject.ItemNo.HasValue)
			{
				result = bill.Packs.OfType<AsycudaPack>().FirstOrDefault(x => x.APA_LineNo == dataObject.ItemNo);
			}
			return result;
		}

		protected override AsycudaPack GetNewBusinessObject()
		{
			return bill.Packs.AddNew();
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(AsycudaPack packBO)
		{
			var builder = new ZStringBuilder();
			if (packAddInfoGroupCollection != null && packAddInfoGroupCollection.Count() > 1)
			{
				builder.Append(Res.GetString("CD2CE451-D780-40B0-825D-7871E1F03F4F", "{0} cannot have more than one {1} which Type is PAC.", "AddInfoGroupCollection", "AddInfoGroup"));
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected sealed override void PopulateBusinessObject(AsycudaPack packingLine)
		{
			this.packingLine = packingLine;
			var packingLineRow = GetColumnIndexer(packingLine);
			if (!isUpdateEnabled || !packingLine.HasManifestBeenSubmittedToCustomsIncludingChildren)
			{
				SetValue(packingLineRow, AsycudaPackSchema.APA_CommodityCode, dataObject.Commodity);
				SetValue(packingLineRow, AsycudaPackSchema.APA_GoodsDescription, dataObject.GetCleanSingleLineGoodsDescription());
				SetValue(packingLineRow, AsycudaPackSchema.APA_MarksAndNumbers, dataObject.MarksAndNos);
				SetValue(packingLineRow, AsycudaPackSchema.APA_PackQty, dataObject.PackQty);
				SetValue(packingLineRow, AsycudaPackSchema.APA_PackUQ, dataObject.PackType);
				SetValue(packingLineRow, AsycudaPackSchema.APA_Weight, dataObject.Weight);
				SetValue(packingLineRow, AsycudaPackSchema.APA_WeightUQ, dataObject.WeightUnit);
				SetValue(packingLineRow, AsycudaPackSchema.APA_Volume, dataObject.Volume);
				SetValue(packingLineRow, AsycudaPackSchema.APA_VolumeUQ, dataObject.VolumeUnit);

				var isDefaultingEnabled = IsDefaultingEnabled;
				var linePriceCurrency = dataObject.LinePriceCurrency;
				if (linePriceCurrency != null)
				{
					var linePriceCurrencyDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaPack.Schema.LinePriceCurrency, PropertyName = AsycudaPack.Schema.LinePriceCurrency, Value = linePriceCurrency.GetCodeAsUpperCase() };
					linePriceCurrencyDetail.ReadIntoBusinessObject(isDefaultingEnabled, packingLine);
				}
				var linePrice = dataObject.LinePrice;
				if (linePrice.HasValue)
				{
					var linePriceDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaPack.Schema.LinePrice, PropertyName = AsycudaPack.Schema.LinePrice, Value = linePrice.Value };
					linePriceDetail.ReadIntoBusinessObject(isDefaultingEnabled, packingLine);
				}

				LinkToContainer(packingLineRow);
				FillUNDGs(packingLineRow);
				ReadAddInfo(packingLine);
				FillPackedItem(packingLine);
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("{2DE8D9F8-BECF-4130-BAD9-ABD847E28AFB}", "Pack '{1}' on Bill '{0}' cannot be updated as this pack has been submitted to Customs.", bill.ABL_BillNumber, packingLine.CodeProperty));
			}
		}

		void ReadAddInfo(AsycudaPack packingLine)
		{
			if (packAddInfoCollection != null)
			{
				new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(packAddInfoCollection, GetGenAddOnColumnList(packingLine), packingLine);
			}
		}

		IEnumerable<GenAddOnDetail> GetGenAddOnColumnList(AsycudaPack packingLine)
		{
			yield return new GenAddOnDetail() { TypeCode = AddOnColumnDataType.GetCodeFromObject(packingLine.MatchingReference), AddInfoKey = AddInfoConstants.Pack.MatchingReference, GenAddOnColumnName = AsycudaPack.Schema.MatchingReference, PropertyName = AsycudaPack.Schema.MatchingReference };
		}

		void LinkToContainer(IColumnIndexer packingLineRow)
		{
			if (dataObject.ContainerNumber.HasValue && packingLine != null)
			{
				var packingLinePK = packingLineRow.GetValue(AsycudaPackSchema.PK);
				var containerNumber = dataObject.ContainerNumber;
				var query = new ZQuery(AsycudaContainerSchema.ACN_AMA_Manifest, bill.ABL_AMA);
				query.AddToFilter(AsycudaContainerSchema.ACN_ContainerNumber, containerNumber);
				query.FetchOnlyFromLocalCache = !(bill?.Header?.IsInDatabase ?? false);
				var container = factory.Load<AsycudaContainer>(query).OrderBy(x => x.PK).FirstOrDefault();
				if (container != null)
				{
					var link = packingLine.Pivot ?? factory.New<AsycudaContainerBillOrPackageLink>();
					var linkRow = GetColumnIndexer(link);
					SetValue(linkRow, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, packingLinePK);
					SetValue(linkRow, AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, container.PK);
					SetValue(linkRow, AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, container.ACN_ClusterKey);
				}
			}
		}

		void FillUNDGs(IColumnIndexer packingLineRow)
		{
			if (dataObject.UNDGCollection != null)
			{
				var packingLinePK = packingLineRow.GetValue(AsycudaPackSchema.PK);
				var query = new ZQuery(UNDGDataItemSchema.DI_ParentID, packingLinePK);
				query.FetchOnlyFromLocalCache = true;
				factory.Load<UNDGDataItem>(query).DeleteAll();
				foreach (var undgData in dataObject.UNDGCollection)
				{
					var undgBO = GetUNDGDataObjectReader(undgData, logger, factory).ReadIntoBusinessObject();
					var undgRow = GetColumnIndexer(undgBO);
					SetValue(undgRow, UNDGDataItemSchema.DI_ParentID, packingLinePK);
					SetValue(undgRow, UNDGDataItemSchema.DI_ParentTableCode, AsycudaPackSchema.Constants.Prefix);
				}
			}
		}

		protected virtual UNDGDataObjectReader GetUNDGDataObjectReader(UNDG undgData, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new UNDGDataObjectReader(undgData, logger, factory);
		}

		protected virtual void FillPackedItem(AsycudaPack packingLine)
		{
			if (!packingLine.IsNonePackedItemRelationship)
			{
				var packingLineCollection = dataObject.PackingLineCollection;
				if (packingLineCollection != null)
				{
					var existingPackedItems = packingLine.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => x.PackedItem).ToList();
					var foundValidData = false;
					var isOnePackedItemRelationship = packingLine.IsOnePackedItemRelationship;
					if (isOnePackedItemRelationship)
					{
						existingPackedItems.Remove(packingLine.PackedItem);
					}
					foreach (var packingLineDataObject in packingLineCollection)
					{
						foundValidData |= FillPackedItem(packingLine, packingLineDataObject);
						if (isOnePackedItemRelationship && foundValidData)
						{
							break;
						}
					}
					if (foundValidData)
					{
						existingPackedItems.DeleteAll();
					}
				}
			}
		}

		bool FillPackedItem(AsycudaPack packingLine, PackingLine packingLineDataObject)
		{
			var packedItemAddInfoGroupCollection = packingLineDataObject.AddInfoGroupCollection?.Where(x => x.Type.GetCodeAsUpperCase() == AddInfoConstants.PackedItem.PackingItemAddInfoType);
			if (packedItemAddInfoGroupCollection != null)
			{
				if (packedItemAddInfoGroupCollection.Count() > 1)
				{
					logger.Log(LogType.Error, Res.GetString("C2CC32AB-DBCD-435A-9245-761AEB733F8A", "Packing Line '{3}' Sub Line '{4}' {0} cannot have more than one {1} which Type is {2}.", "AddInfoGroupCollection", "AddInfoGroup", AddInfoConstants.PackedItem.PackingItemAddInfoType, dataObject.Link.GetValueOrDefault(), packingLineDataObject.Link.GetValueOrDefault()));
				}
				else
				{
					var packedItemAddInfoCollection = packedItemAddInfoGroupCollection.FirstOrDefault()?.AddInfoCollection;
					var countryCode = packedItemAddInfoCollection?.GetZStringValue(AddInfoConstants.PackedItem.Country, logger).GetValueOrDefault() ?? ZString.Empty;
					if (countryCode == helper.CountryCode)
					{
						new AsycudaPackedItemObjectReader(packingLineDataObject, packedItemAddInfoCollection, logger, factory, packingLine, helper).ReadIntoBusinessObject();
						return true;
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("01D838C9-E877-4823-9448-50C2B7D7F26C", "Packing Line '{0}' Sub Line '{1}' {2} is not '{3}'; this data will be ignored.", dataObject.Link.GetValueOrDefault(), packingLineDataObject.Link.GetValueOrDefault(), "AddInfoGroupCollection.AddInfoGroup.AddInfoCollection.AddInfo.Key.Country", helper.CountryCode));
					}
				}
			}
			return false;
		}
	}
}
