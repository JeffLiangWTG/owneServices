using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierConsignmentsTable : NCTSPrettierTableBase
	{
		readonly IReadOnlyCollection<INCTSPrettierConsignmentData> consignments;

		public NCTSPrettierConsignmentsTable(IReadOnlyCollection<INCTSPrettierConsignmentData> consignments)
		{
			this.consignments = Argument.NotNull(consignments, nameof(consignments));
		}

		public override ZString Caption => Res.GetString("00A1B527-66DC-48C5-920B-AD1B4A5E5540", "Consignments");

		public override IReadOnlyCollection<(ZString Key, ZString Value)> AdditionalInfo => new (ZString Key, ZString Value)[] {
			(Res.GetString("CAD7FAE3-1C68-489E-872F-E6678D35E0EC", "Number of Consignments"), consignments.Count.ToString()),
			(Res.GetString("C71FBBC3-7D93-4708-8E62-2727004DC26A", "Number of Consignment Items"), consignments.SelectMany(x => x.GoodsItems).Count().ToString()),
		};

		public override IReadOnlyCollection<(ZString Caption, ZString Attributes)> Columns => new (ZString Caption, ZString Attributes)[] {
			(Res.GetString("47373791-610C-46CA-BAF9-B5E146C5E2C8", "UCR Reference"), (NoResString)"width=25%"),
			(Res.GetString("2CF8E0AB-E56C-465F-A16B-3D9E7F0F1922", "Goods Items"), (NoResString)"width=75%"),
		};

		protected override IEnumerable<object[]> Rows => consignments.Select(x => new object[] { x.UCRReference, new NCTSPrettierGoodsItemsTable(x.GoodsItems) });
	}
}
