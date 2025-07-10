using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public class FRDocAddressControl : ZDocAddressControl, Integration.Customs.FR.IFRDocAddressControl
	{
		protected override Func<BusinessObjectFactory, OrgAddress, AddressFormatter> GetAddressFormatter()
		{
			return (factory, orgAddress) => new FRAddressFormatter(factory, orgAddress);
		}
	}
}
