using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(CusPermitHeader))]
	public class CusPermitHeaderTest : EU.Business.Testing.CusPermitHeaderTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var permit = Factory.NewWithValidTestData<CusPermitHeader>();
			permit.CPH_QtyValIndicator = ZString.Empty;
			return permit;
		}
	}
}
