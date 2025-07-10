using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class Permit : CusSupportingInfo
	{
		public Permit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
