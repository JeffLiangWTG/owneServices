using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class IE507GoodsItemProvider : IE507And590CommonGoodsItemProvider, IIE507GoodsItem
	{
		public IE507GoodsItemProvider(CusExitConsignmentItem consignmentItem, ZDecimal grossMass, ZDecimal netMass, IEnumerable<(ZString packageType, int? packageQuantity, ZString shippingMarks)> packageData)
			: base(consignmentItem, grossMass, netMass, packageData)
		{
			this.consignmentItem = consignmentItem;
		}

		readonly CusExitConsignmentItem consignmentItem;

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = consignmentItem.CusAuthorizationUsages.Select(x => new AuthorisationProvider(x)).ToArray());
		IReadOnlyCollection<IAuthorisation> authorisations;
	}
}
