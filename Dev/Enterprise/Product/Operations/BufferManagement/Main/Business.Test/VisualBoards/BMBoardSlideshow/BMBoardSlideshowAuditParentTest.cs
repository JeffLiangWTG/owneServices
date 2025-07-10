using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSlideshow))]
	public class BMBoardSlideshowAuditParentTest : AuditParentTest<BMBoardSlideshow>
	{
		protected override BMBoardSlideshow NewTestAuditParent()
		{
			return Factory.New<BMBoardSlideshow>();
		}
	}
}
