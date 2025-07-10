using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryHeaderTestHelper : CusEntryHeader
	{
		public CusEntryHeaderTestHelper(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDecimal CustomsValueExposed { get; set; }
		public override ZDecimal CustomsValue
		{
			get { return CustomsValueExposed; }
		}
	}
}
