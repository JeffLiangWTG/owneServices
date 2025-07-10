using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public interface IAsycudaPackedItemDataObjectWriter
	{
		PackingLine GetDataObject(AsycudaPackedItem sourceBO);
	}

	public class AsycudaPackedItemDataObjectWriter<TCountry> : DataObjectWriter<TCountry, PackingLine>, IAsycudaPackedItemDataObjectWriter
		where TCountry : AsycudaPackedItem
	{
		public AsycudaPackedItemDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = helper;
		}

		readonly AsycudaManifestHeaderDataObjectWriterHelper helper;

		protected override PackingLine PopulateDataObject(TCountry sourceCountry)
		{
			var packingLine = new PackingLine(writeManager.WriterStrategy)
			{
				CountryOfOrigin = Country.New(sourceCountry.GoodsOrigin),
				GoodsDescription = sourceCountry.API_GoodsDescription,
				HarmonisedCode = sourceCountry.API_Tariff,
				PackQty = GetLongQty(sourceCountry.API_CustomsQty),
				PackType = ListHelper.GetWithDescription<PackageType>(sourceCountry.API_CustomsUQ, sourceCountry.Lookups.CustomsUQList)
			};
			PopulateAddInfos(packingLine, sourceCountry);

			return packingLine;
		}

		ZLong GetLongQty(ZDecimal data)
		{
			ZLong result;
			if (data > long.MaxValue)
			{
				result = long.MaxValue;
			}
			else if (data < long.MinValue)
			{
				result = long.MinValue;
			}
			else
			{
				result = data.ToZLong();
			}
			return result;
		}

		void PopulateAddInfos(PackingLine packingLine, TCountry sourceCountry)
		{
			packingLine.SetAddInfoGroupCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>();
				var countryCode = sourceCountry.CountryCode;
				if (!countryCode.IsEmpty)
				{
					addInfoCollection.Add(new AddInfo
					{
						Key = AddInfoConstants.PackedItem.Country,
						Value = countryCode,
					});
				}

				foreach (var pair in helper.GetPackedItemAdditionalAddInfos(sourceCountry))
				{
					if (!pair.Value.IsEmpty)
					{
						addInfoCollection.Add(AddInfo.New(pair.Key, pair.Value));
					}
				}

				PopulateEntryNumbers(sourceCountry, addInfoCollection);

				addInfoCollection.Add(new AddInfo
				{
					Key = AddInfoConstants.PackedItem.CustomsValue,
					Value = sourceCountry.API_CustomsValue.ToString(),
				});

				addInfoCollection.Add(new AddInfo
				{
					Key = AddInfoConstants.PackedItem.CustomsQty,
					Value = sourceCountry.API_CustomsQty.ToString(),
				});

				addInfoCollection.Add(new AddInfo
				{
					Key = AddInfoConstants.PackedItem.CustomsUQ,
					Value = sourceCountry.API_CustomsUQ.ToString(),
				});

				addInfoCollection.Add(new AddInfo
				{
					Key = AddInfoConstants.PackedItem.DutyValue,
					Value = sourceCountry.API_DutyAmount.ToString(),
				});

				addInfoCollection.Add(new AddInfo
				{
					Key = AddInfoConstants.PackedItem.TaxValue,
					Value = sourceCountry.API_TaxAmount.ToString(),
				});
				var countryOfDestination = sourceCountry?.Pack?.Bill?.ABL_RL_NKFinalDestination.Left(2) ?? ZString.Empty;
				if (!countryOfDestination.IsEmpty)
				{
					addInfoCollection.Add(new AddInfo
					{
						Key = AddInfoConstants.PackedItem.CountryOfDestination,
						Value = countryOfDestination,
					});
				}
				return new List<AddInfoGroup>(new[]
				{
					new AddInfoGroup
					{
						Type = new CodeDescriptionPair() { Code = AddInfoConstants.PackedItem.PackingItemAddInfoType, Description = AddInfoConstants.PackedItem.PackingItemAddInfoTypeDescription },
						AddInfoCollection = addInfoCollection,
					}
				});
			});
		}

		void PopulateEntryNumbers(TCountry sourceCountry, List<AddInfo> addInfoCollection)
		{
			var mapping = helper.GetPackedItemEntryNumberMapping().ToArray();
			if (mapping.Length > 0)
			{
				var entryNumbers = sourceCountry.CustomsEntryNumbers
					.Cast<AsycudaPackedItemEntryNum>()
					.Where(c => !c.CE_EntryNum.IsEmpty)
					.OrderByDescending(c => c.CE_SystemCreateTimeUtc);
				foreach (var pair in mapping)
				{
					var entryNumber = entryNumbers.FirstOrDefault(c => c.CE_EntryType == pair.Value);
					if (entryNumber != null)
					{
						addInfoCollection.Add(new AddInfo()
						{
							Key = pair.Key,
							Value = entryNumber.CE_EntryNum,
						});
					}
				}
			}
		}

		PackingLine IAsycudaPackedItemDataObjectWriter.GetDataObject(AsycudaPackedItem sourceBO) => GetDataObject(sourceBO as TCountry);
	}
}
