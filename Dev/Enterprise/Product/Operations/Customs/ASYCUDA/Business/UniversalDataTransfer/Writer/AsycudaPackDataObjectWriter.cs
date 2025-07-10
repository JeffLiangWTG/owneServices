using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public interface IAsycudaPackDataObjectWriter
	{
		PackingLine GetDataObject(AsycudaPack sourceBO);
	}

	public class AsycudaPackDataObjectWriter<TPack> : DataObjectWriter<TPack, PackingLine>, IAsycudaPackDataObjectWriter
		where TPack : AsycudaPack
	{
		public AsycudaPackDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = helper;
		}

		protected readonly AsycudaManifestHeaderDataObjectWriterHelper helper;
		protected override PackingLine PopulateDataObject(TPack sourcePack)
		{
			var bill = sourcePack.Bill;
			var packQty = sourcePack.APA_PackQty;
			var packingLineData = new PackingLine(writeManager.WriterStrategy)
			{
				Commodity = new Commodity() { Code = sourcePack.APA_CommodityCode }, // TODO: Add description
				GoodsDescription = sourcePack.APA_GoodsDescription,
				MarksAndNos = sourcePack.APA_MarksAndNumbers,
				CustomsOuterPacks = packQty,
				PackQty = new ZLong(packQty),
				PackType = ListHelper.GetWithDescription<PackageType>(sourcePack.APA_PackUQ, bill.Lookups.PackageTypeList),
				CustomsPackType = new PackageType() { Code = GetCustomsUQ(bill, sourcePack.APA_PackUQ) },
				Weight = new ZWeight(sourcePack.APA_Weight, sourcePack.APA_WeightUQ).InKilogramsSafe,
				WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms, Description = "kilogram" },  // lower case singular is correct
				Volume = new ZVolume(sourcePack.APA_Volume, sourcePack.APA_VolumeUQ).InCubicMetres,
				VolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres, Description = "Cubic metre" },
				ContainerNumber = sourcePack.Container?.ACN_ContainerNumber ?? ZString.Empty,
				LinePrice = sourcePack.LinePrice,
				LinePriceCurrency = Currency.New(sourcePack.RefLinePriceCurrency),
			};
			PopulateAddInfos(packingLineData, sourcePack);
			packingLineData.SetUNDGCollection(() => GetUNDGs(sourcePack));
			packingLineData.SetPackingLineCollection(() => CreatePackingLineCollection(sourcePack));
			return packingLineData;
		}

		protected List<PackingLine> CreatePackingLineCollection(TPack sourcePack)
		{
			var list = new List<PackingLine>();
			var packedItems = new List<AsycudaPackedItem>();
			if (sourcePack.IsOnePackedItemRelationship)
			{
				packedItems.Add(sourcePack.PackedItem);
			}
			else if (sourcePack.IsManyPackedItemRelationship)
			{
				packedItems.AddRange(sourcePack.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => (AsycudaPackedItem)x.PackedItem).OrderBy(x => x.API_Tariff).ThenBy(x => x.API_RN_NKGoodsOrigin));
			}
			if (packedItems.Count > 0)
			{
				var writer = CreateNewAsycudaPackedItemDataObjectWriter();
				packedItems.ForEach(x => list.Add(writer.GetDataObject(x)));
			}
			return list;
		}

		protected virtual IAsycudaPackedItemDataObjectWriter CreateNewAsycudaPackedItemDataObjectWriter() => new AsycudaPackedItemDataObjectWriter<AsycudaPackedItem>(writeManager, helper);

		void PopulateAddInfos(PackingLine packingLine, TPack sourcePack)
		{
			packingLine.SetAddInfoGroupCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>(new[]
				{
					new AddInfo()
					{
						Key = AddInfoConstants.Pack.ConsignmentReference,
						Value = sourcePack.ConsignmentReference.ToString(),
					},
					new AddInfo()
					{
						Key = AddInfoConstants.Pack.MatchingReference,
						Value = sourcePack.MatchingReference,
					}
				});
				var addInfoGroupCollection = new List<AddInfoGroup>(new[]
				{
					new AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = AddInfoConstants.Pack.PackAddInfoType, Description = AddInfoConstants.Pack.PackAddInfoTypeDescription },
						AddInfoCollection = addInfoCollection,
					}
				});
				return addInfoGroupCollection;
			});
		}

		List<UNDG> GetUNDGs(TPack packSource)
		{
			List<UNDG> list = null;
			if (packSource.UNDGs?.Count > 0)
			{
				var packageUNDG = packSource.UNDGs[0];
				var writer = new UNDGDataObjectWriter(writeManager);
				var undgData = writer.GetDataObject(packageUNDG);
				list = new List<UNDG> { undgData };
			}
			return list;
		}

		ZString GetCustomsUQ(AsycudaBill sourceBill, ZString uqToConvert)
		{
			var country = helper.CountryCode;
			var acceptableCustomsUQs = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(sourceBill.Factory,
																										country,
																										Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);
			var refPack = new CusRefPacks.Loader(sourceBill.Factory).Load(uqToConvert, country, RPTypeList.Codes.GlobalManifestLine, acceptableCustomsUQs.GetAllCodes());

			return refPack == null ? sourceBill.ABL_ManifestUQ : refPack.RP_CustomsPack;
		}

		PackingLine IAsycudaPackDataObjectWriter.GetDataObject(AsycudaPack sourceBO) => GetDataObject(sourceBO as TPack);
	}
}
