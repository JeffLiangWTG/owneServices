using System.Linq;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	class SectionDescriptorProviderTest : TestCase
	{
		public void TestGetDescriptors_TypesShouldBeUnique()
		{
			var types = SectionDescriptorProvider.GetSupportedSectionTypes();
			AssertNoExceptionThrown(() => types.ToDictionary(x => x));
		}
	}
}
