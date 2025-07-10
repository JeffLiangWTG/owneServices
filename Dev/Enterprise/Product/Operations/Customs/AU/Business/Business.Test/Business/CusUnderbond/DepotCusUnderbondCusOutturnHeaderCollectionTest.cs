using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DepotCusUnderbondCusOutturnHeaderCollection))]
	public class DepotCusUnderbondCusOutturnHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DepotCusUnderbondCusOutturnHeaderCollection(Header);
		}

		#region Implementation

		protected CusOutturnHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<CusOutturnHeader>();
				}
				return header;
			}
		}
		CusOutturnHeader header;

		#endregion
	}
}
