using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(RSFMessageCollection))]
	sealed class RSFMessageCollectionTest : EDIMessageCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RSFMessageCollection(RSF);
		}

		CusStatementHeader RSF
		{
			get
			{
				if (rsf == null)
				{
					rsf = Factory.New<CusStatementHeader>();
				}
				return rsf;
			}
		}
		CusStatementHeader rsf;
	}
}
