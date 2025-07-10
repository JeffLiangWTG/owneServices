using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSlideshowCollection : ActiveBusinessObjectCollection<BMBoardSlideshow>
	{
		public BMBoardSlideshowCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
