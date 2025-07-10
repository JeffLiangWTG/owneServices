using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FTALineWrapperCollection))]
	sealed class FTALineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FTALineWrapperCollection>
	{
		protected override FTALineWrapperCollection GetCollectionToTest() => new FTALineWrapperCollection(Enumerable.Empty<ImportFTALine>(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			FTALineWrapper wrapper = new FTALineWrapper(Factory);
			wrapper.FirstLine = new ImportFTALine();
			wrapper.SecondLine = new ImportFTALine();
			wrapper.ThirdLine = new ImportFTALine();

			return new FTALineWrapper(Factory);
		}
	}
}
