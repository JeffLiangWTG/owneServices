using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgHeaderCollection))]
	public class EDIOrgHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EDIOrgHeader>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIOrgHeaderCollection(Factory);
		}
	}
}
