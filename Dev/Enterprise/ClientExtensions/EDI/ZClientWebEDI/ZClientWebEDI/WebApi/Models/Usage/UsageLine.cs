using System;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UsageLine
	{
		public Guid DatabasePK { get; set; }
		public ZString Product { get; set; }
		public ZString DatabaseServerCode { get; set; }
		public ZString EnterpriseCode { get; set; }
		public ZGuid PriceItemPK { get; set; }
		public ZString PriceItemDescription { get; set; }
		public ZShort PriceItemOrder { get; set; }
		public ZString CompanyCode { get; set; }
		public ZString SystemCode { get; set; }
		public ZGuid OrgPK { get; set; }
		public ZString OrgName { get; set; }
		public ZGuid ClientCompanyPK { get; set; }
		public ZGuid LicenceCompanyPK { get; set; }
		public ZInt UnitCount { get; set; }
	}
}