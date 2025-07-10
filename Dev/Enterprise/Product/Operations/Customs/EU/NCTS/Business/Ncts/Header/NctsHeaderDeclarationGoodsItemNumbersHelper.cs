using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsHeaderDeclarationGoodsItemNumbersHelper
	{
		public static void AssignUnassignedDeclarationGoodsItemNumbers(NctsHeader header,
			IEnumerable<NctsCommonCargoDesc> goodsItems = null,
			Func<NctsCommonCargoDesc, bool> filter = null)
		{
			Argument.NotNull(header, nameof(header));

			goodsItems = goodsItems ?? header.GetGoodsItems();
			var baseNumber = goodsItems.MaxOrDefault(e => e.BY_DeclarationGoodsItemNumber) + 1;

			var unassignedGoodsItems = goodsItems
				.OrderBy(x => x.Bill?.SequenceNumber)
				.ThenBy(x => x.BY_LineNo)
				.Where(g => g.BY_DeclarationGoodsItemNumber.IsEmpty && (filter is null || filter(g)))
				.Select((g, i) => (GoodsItem: g, Increment: i));

			foreach (var (goodsItem, increment) in unassignedGoodsItems)
			{
				goodsItem.BY_DeclarationGoodsItemNumber = baseNumber + increment;
			}
		}

		public static bool IsDeclarationGoodsItemNumbersNonSequential(NctsHeader header)
		{
			Argument.NotNull(header, nameof(header));
			return header.GetGoodsItems().OrderBy(g => g.BY_DeclarationGoodsItemNumber).Select((g, i) => g.BY_DeclarationGoodsItemNumber != i + 1).Any(g => g);
		}

		public static void SetDeclarationGoodsItemNumbersToZero(NctsHeader header)
		{
			Argument.NotNull(header, nameof(header));
			var goodsItems = header.GetGoodsItems();

			foreach (var goodsItem in goodsItems)
			{
				goodsItem.BY_DeclarationGoodsItemNumber = 0;
			}
		}
	}
}
