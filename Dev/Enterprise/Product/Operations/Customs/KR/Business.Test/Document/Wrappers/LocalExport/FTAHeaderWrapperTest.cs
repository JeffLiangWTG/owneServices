using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FTAHeaderWrapper))]
	sealed class FTAHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = new ImportFTAHeader();
			return new FTAHeaderWrapper(entry, Factory);
		}
	}
}
