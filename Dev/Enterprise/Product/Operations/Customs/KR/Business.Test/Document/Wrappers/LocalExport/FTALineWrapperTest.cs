using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FTALineWrapper))]
	sealed class FTALineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			FTALineWrapper wrapper = new FTALineWrapper(Factory);
			wrapper.FirstLine = new ImportFTALine();
			wrapper.SecondLine = new ImportFTALine();
			wrapper.ThirdLine = new ImportFTALine();
			return new FTALineWrapper(Factory);
		}
	}
}
