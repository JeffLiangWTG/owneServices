using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class CMRDOCSMessageTestClass : CMRDOCSMessage
	{
		public CMRDOCSMessageTestClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public BusinessObject GetWrappedObjectTestMethod()
		{
			return base.GetWrappedObject();
		}
	}
}
