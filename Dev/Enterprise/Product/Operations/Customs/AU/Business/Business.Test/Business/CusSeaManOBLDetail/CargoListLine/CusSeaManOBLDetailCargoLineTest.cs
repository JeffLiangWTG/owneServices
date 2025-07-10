using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLDetailCargoLine))]
	public class CusSeaManOBLDetailCargoLineTest : BaseCusSeaManOBLDetailTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusSeaManOBLHeaderCargoLine header = Factory.New<CusSeaManOBLHeaderCargoLine>();
			return header.Detail;
		}
	}
}
