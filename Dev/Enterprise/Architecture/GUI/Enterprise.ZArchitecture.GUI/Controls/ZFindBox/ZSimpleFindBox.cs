using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZSimpleFindBox : IFindBox
	{
		public ZSimpleFindBox(IFindBoxListProvider provider, IFindBoxPopup popupForm = null)
		{
			ListProvider = provider;
			PopupForm = popupForm;
		}

		public string Code { get; set; }
		public string Description { get; set; }
		public IFindBoxListProvider ListProvider { get; private set; }
		public IFindBoxPopup PopupForm { get; set; }
	}
}
