using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyFindBox : IFindBox
	{
		public DummyFindBox()
			: this("Dummy", "It's dummy")
		{ }

		public DummyFindBox(string code, string description)
		{
			this.Code = Code;
			this.Description = description;
		}

		public string Code { get; set; }

		public string Description { get; set; }

		public IFindBoxListProvider ListProvider { get; private set; }

		public IFindBoxPopup PopupForm { get; private set; }
	}
}
