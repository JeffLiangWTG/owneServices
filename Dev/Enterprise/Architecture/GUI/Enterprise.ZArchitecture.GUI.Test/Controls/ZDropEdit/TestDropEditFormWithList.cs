using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestDropEditFormWithList : TestDropEditForm
	{
		public TestDropEditFormWithList(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		protected override string BindToPrefix
		{
			get { return "DummiesWithList."; }
		}
	}
}
