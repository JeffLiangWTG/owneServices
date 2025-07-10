using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	[TestedType(typeof(BorderWiseReturnHookNonPersistentBizo))]
	class BorderWiseReturnHookNonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestPK()
		{
			var bizo = new BorderWiseReturnHookNonPersistentBizo();

			NUnit.Framework.Assert.That(bizo.PK, Is.Not.EqualTo(ZGuid.Empty));

			bizo = new BorderWiseReturnHookNonPersistentBizo(ZGuid.BrettsGuid);

			NUnit.Framework.Assert.That(bizo.PK, Is.EqualTo(ZGuid.BrettsGuid));
		}
	}
}
