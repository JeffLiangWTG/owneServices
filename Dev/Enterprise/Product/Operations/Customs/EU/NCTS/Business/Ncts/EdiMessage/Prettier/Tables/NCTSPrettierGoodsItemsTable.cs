using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierGoodsItemsTable : NCTSPrettierTableBase
	{
		readonly IReadOnlyCollection<INCTSPrettierGoodsItemData> goodsItems;

		public NCTSPrettierGoodsItemsTable(IReadOnlyCollection<INCTSPrettierGoodsItemData> goodsItems)
		{
			this.goodsItems = Argument.NotNull(goodsItems, nameof(goodsItems));
		}

		public override ZString Caption => default;

		public override ZString Border => "0";

		public override IReadOnlyCollection<(ZString Key, ZString Value)> AdditionalInfo => Array.Empty<(ZString Key, ZString Value)>();

		public override IReadOnlyCollection<(ZString Caption, ZString Attributes)> Columns => new (ZString Caption, ZString Attributes)[] {
			(Res.GetString("56F1361E-52F7-4723-BB54-441B6955F9AF", "Item Number"), (NoResString)"class=tdGoodsItemsTitle"),
			(Res.GetString("918398BD-9C42-4B9A-9E26-6C4463C58AAF", "UCR Reference"), (NoResString)"class=tdGoodsItemsTitle"),
			(Res.GetString("1C7C5E79-58C6-4D73-8C19-CA1F4CC80FCE", "Description"), (NoResString)"class=tdGoodsItemsTitle"),
			(Res.GetString("D8792ACA-7920-4FC2-BEDB-86DAB3289CB7", "Code"), (NoResString)"class=tdGoodsItemsTitle"),
		};

		protected override IEnumerable<object[]> Rows => goodsItems.Select(x => new object[] { x.ItemNumber, x.UCRReference, x.Description, x.HarmonizedSubHeadingCode });
	}
}
