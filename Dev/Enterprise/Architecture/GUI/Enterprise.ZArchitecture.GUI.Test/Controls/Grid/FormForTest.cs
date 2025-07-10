using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class FormForTest : ZForm
	{
		public FormForTest(BusinessObjectFactory factory)
			: base(factory.New<DummyBusinessObject>())
		{
			DummyChild1 = Dummy.Collection.AddNew();
			DummyChild2 = Dummy.Collection.AddNew();

			Grid = new ZGrid();
			Grid.BindTo = "Collection";
			Grid.Columns.AddTextColumn("Z0_Code", 20);
			Controls.Add(Grid);
		}

		public DummyBusinessObject Dummy
		{
			get { return (DummyBusinessObject)BusinessEntity; }
		}

		public readonly DummyChildBusinessObject DummyChild1;
		public readonly DummyChildBusinessObject DummyChild2;
		public readonly ZGrid Grid;
	}
}
