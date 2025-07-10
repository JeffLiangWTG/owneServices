using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class CMREXDRMessageTestClass : CMREXDRMessage
	{
		public CMREXDRMessageTestClass(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BusinessObject GetWrappedBizoExposed()
		{
			return base.GetWrappedObject();
		}
	}
}
