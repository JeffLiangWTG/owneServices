using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(ReportEdw))]
	sealed class ReportEdwTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReportEdw(new DocumentPack(), null);
		}
	}
}
