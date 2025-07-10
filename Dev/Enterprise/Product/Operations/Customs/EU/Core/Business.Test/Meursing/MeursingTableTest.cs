using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Meursing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(MeursingTable))]
	class MeursingTableTest : NonPersistentBusinessObjectTestCase
	{
		// All functional tests are in MeursingTableManager.cs, this is just a reflection test
	}
}
