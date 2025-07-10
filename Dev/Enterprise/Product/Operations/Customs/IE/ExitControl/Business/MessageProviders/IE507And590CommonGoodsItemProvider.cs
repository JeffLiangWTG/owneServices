using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class IE507And590CommonGoodsItemProvider : IIE507And590CommonGoodsItem
	{
		public IE507And590CommonGoodsItemProvider(CusExitConsignmentItem consignmentItem, ZDecimal grossMass, ZDecimal netMass, IEnumerable<(ZString packageType, int? packageQuantity, ZString shippingMarks)> packageData)
		{
			GoodsItemNumber = consignmentItem.CCI_LineNumber;
			GrossMass = grossMass;
			NetMass = netMass;
			this.packageData = Argument.NotNull(packageData, nameof(packageData));
		}
		readonly IEnumerable<(ZString packageType, int? packageQuantity, ZString shippingMarks)> packageData;

		public short GoodsItemNumber { get; }
		short IIE507And590CommonGoodsItem.GoodsItemNumber => GoodsItemNumber;

		public decimal GrossMass { get; }
		decimal IIE507And590CommonGoodsItem.GrossMass => GrossMass;

		public decimal NetMass { get; }
		decimal IIE507And590CommonGoodsItem.NetMass => NetMass;

		public IReadOnlyCollection<IPackaging> Packages => cachedIPackages ?? (cachedIPackages = packageData.Select(p => new PackagingProvider(p.packageType, p.packageQuantity ?? 0, p.shippingMarks)).ToArray());
		IPackaging[] cachedIPackages;
		IReadOnlyCollection<IPackaging> IIE507And590CommonGoodsItem.Packages => Packages;
	}
}
