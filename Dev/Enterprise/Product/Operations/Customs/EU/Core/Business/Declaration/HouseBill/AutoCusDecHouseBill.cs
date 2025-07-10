using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class AutoCusDecHouseBill : Customs.Business.Bill
	{
		protected AutoCusDecHouseBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
