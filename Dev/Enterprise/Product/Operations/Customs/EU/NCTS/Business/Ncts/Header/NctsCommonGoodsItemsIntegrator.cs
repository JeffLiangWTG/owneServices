using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCommonGoodsItemsIntegrator : ICommonGoodsItemsIntegrator
	{
		public NctsCommonGoodsItemsIntegrator(NctsHeader header)
		{
			this.header = header;
		}

		public ICommonGoodsItem ConvertToCommonGoodsItem(BusinessObject item)
		{
			ICommonGoodsItem result = null;
			if (item is NctsDepartureCargoDesc source)
			{
				var packages = source.Packages.Select(p => new CommonPackage
				{
					BillOrReferenceNumber = source.Bill?.B0_ReferenceID ?? header.BH_JobReference,
					PackageType = p.B5_UnitType,
					PackageCount = p.B5_UnitCount.ToZInt(),
					MarksAndNumbers = p.B5_MarksAndNumbers
				});
				result = new CommonGoodsItem
				{
					GoodsDescription = source.BY_Description,
					GrossMass = source.BY_GrossWeight,
					NetMass = source.BY_NetWeight,
					GrossMassUnit = source.BY_GrossWeightUnit,
					NetMassUnit = source.BY_NetWeightUnit,
					CommodityCode = source.BY_FormattedHarmonisedTariff,
					Value = source.BY_MonetaryValue,
					JobReference = source.Header.BH_JobReference,
					DispatchCountry = source.BY_RN_NKCountryOfDispatch,
					DestinationCountry = source.BY_RN_NKCountryOfDestination,
					EntryNumber = source.GetEntryNumberFormatter().FormatEntryNumber(source.Header.MovementReferenceEntryNumber, source.MoveHeader.BM_InBondEntryType),
					Packages = packages.ToList<ICommonPackage>()
				};
			}

			return result;
		}

		public void CopyCommonGoodsItems(IEnumerable<ICommonGoodsItem> goodsItemsForIntegration, int targetIndex)
		{
			if (header.IsDepartureMovement)
			{
				var targetCollection = header.MovementHeader.GoodsItems;
				var oldValue = false;
				if (header.IsPhase5)
				{
					targetCollection = header.Bills[targetIndex].GoodsItems;

					oldValue = targetCollection.CopyLastGoodsItemToNewLines;
					targetCollection.CopyLastGoodsItemToNewLines = false;
				}

				foreach (var sourceItem in goodsItemsForIntegration)
				{
					var targetItem = targetCollection.AddNew();
					CopyFromCommonGoodsItem(sourceItem, targetItem);
				}

				if (header.IsPhase5)
				{
					targetCollection.CopyLastGoodsItemToNewLines = oldValue;
				}
			}
		}

		public void CopyFromCommonGoodsItem(ICommonGoodsItem source, BusinessObject targetItem) => CopyFromCommonGoodsItemCore(source, targetItem);

		protected virtual void CopyFromCommonGoodsItemCore(ICommonGoodsItem source, BusinessObject targetItem)
		{
			if (targetItem is NctsDepartureCargoDesc target)
			{
				target.BY_Description = source.GoodsDescription.SubstringSafe(0, target.BY_DescriptionInfo.MaxLength);
				target.BY_FormattedHarmonisedTariff = source.CommodityCode.SubstringSafe(0, target.BY_FormattedHarmonisedTariffInfo.MaxLength);
				target.BY_GrossWeight = source.GrossMass;
				target.BY_GrossWeightUnit = source.GrossMassUnit.SubstringSafe(0, target.BY_GrossWeightUnitInfo.MaxLength);
				target.BY_NetWeightUnit = source.NetMassUnit.SubstringSafe(0, target.BY_NetWeightUnitInfo.MaxLength);
				target.BY_NetWeight = source.NetMass;
				target.BY_RN_NKCountryOfDestination = source.DestinationCountry.SubstringSafe(0, target.BY_RN_NKCountryOfDestinationInfo.MaxLength);
				target.BY_RN_NKCountryOfDispatch = source.DispatchCountry.SubstringSafe(0, target.BY_RN_NKCountryOfDispatchInfo.MaxLength);
				target.BY_Description = source.GoodsDescription.SubstringSafe(0, target.BY_DescriptionInfo.MaxLength);
				target.BY_MonetaryValue = source.Value;

				foreach (var package in source.Packages)
				{
					var targetPackage = target.Packages.AddNew();
					targetPackage.B5_UnitType = package.PackageType;
					targetPackage.B5_UnitCount = (long)package.PackageCount;
					targetPackage.B5_MarksAndNumbers = package.MarksAndNumbers;
				}

				CopySupplementaryQuantityFromCommonGoodsItem(source, target);
				CopyEntryNumberFromCommonGoodsItem(source, target);
			}
		}

		public IEnumerable<ICommonGoodsItem> GetCommonGoodsItemsForIntegration()
		{
			foreach (var item in header.DepartureGoodsItems)
			{
				yield return ConvertToCommonGoodsItem(item);
			}
		}

		public IBusinessObjectCollection TheOtherCollectionToAttach()
		{
			return new CusEntryHeadersToAttachCollection(header);
		}

		protected virtual void CopySupplementaryQuantityFromCommonGoodsItem(ICommonGoodsItem source, NctsDepartureCargoDesc target)
		{
			target.BY_CustomsSecondQuantity = source.SupplementaryQuantity;
			target.BY_CustomsSecondUnitQty = source.SupplementaryQuantityUnit;
		}

		void CopyEntryNumberFromCommonGoodsItem(ICommonGoodsItem source, NctsDepartureCargoDesc target)
		{
			var (entryNumberClass, entryNumberType, entryReferenceNumber, entryLineNumber) = source.EntryNumber;
			if (!entryReferenceNumber.IsEmpty)
			{
				var isInPhase5 = header?.IsInPhase5TransitionPeriod ?? false;
				var targetPreviousDocument = isInPhase5
					? (EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)target.PreviousDocuments.AddNew()
					: target.Bill?.PreviousDocuments.AddNew();

				if (targetPreviousDocument != null)
				{
					if (isInPhase5 && entryLineNumber.HasValue)
					{
						targetPreviousDocument.CSI_ItemNumber = entryLineNumber.Value;
					}

					targetPreviousDocument.CSI_Code = entryNumberType;
					targetPreviousDocument.CSI_SubType = entryNumberClass;
					targetPreviousDocument.CSI_ReferenceNumber = entryReferenceNumber;
					targetPreviousDocument.CSI_Description = entryReferenceNumber;
				}
			}
		}

		readonly NctsHeader header;
	}
}
