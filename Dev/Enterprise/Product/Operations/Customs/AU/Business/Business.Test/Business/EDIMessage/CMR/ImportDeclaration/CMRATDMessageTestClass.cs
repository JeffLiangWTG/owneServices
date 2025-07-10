using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class CMRATDMessageTestClass : CMRATDMessage
	{
		public CMRATDMessageTestClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public BusinessObject GetWrappedObjectTestMethod()
		{
			return base.GetWrappedObject();
		}
	}
}
