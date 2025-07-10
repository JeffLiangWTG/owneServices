using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class BusinessObjectIsBindingTest : TestCaseWithFactory
	{
		public void TestGrid()
		{
			var rootDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var collection = rootDummy.Collection;

			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm_Testo())
			using (var grid = new ZGrid())
			{
				form.Controls.Add(grid);
				var columnStyle = new ZTextBoxColumnStyleInfo();
				columnStyle.Caption = "Description";
				columnStyle.ColumnName = "Z0_Description";
				grid.ColumnStyles.Add(columnStyle);

				form.BindingSource_Exposed.SetBindingMember(grid, "Collection");
				form.SetDataBinding(rootDummy, "");

				form.Show();
				Application.DoEvents();

				Assert(dummy1.IsBound);
				Assert(dummy2.IsBound);
				Assert(!dummy3.IsBound);
			}
		}

		public void TestGrid_ActiveBusinessObjectCollection()
		{
			var rootDummy = Factory.NewWithValidTestData<DummyBusinessObjectWithActiveCollection>();
			var collection = rootDummy.ActiveCollection;

			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = dummy2.Z0_Code = "SPK";

			using (var form = new ZForm_Testo())
			using (var grid = new ZGrid())
			{
				form.Controls.Add(grid);
				var columnStyle = new ZTextBoxColumnStyleInfo();
				columnStyle.Caption = "Description";
				columnStyle.ColumnName = "Z0_Description";
				grid.ColumnStyles.Add(columnStyle);

				form.BindingSource_Exposed.SetBindingMember(grid, "ActiveCollection");
				form.SetDataBinding(rootDummy, "");

				form.Show();
				Application.DoEvents();

				Assert(dummy1.IsBound);
				Assert(dummy2.IsBound);
				Assert(!dummy3.IsBound);
			}
		}

		public void TestTextBox()
		{
			var collection = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "SPZ"));
			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = collection.AddNew();
			dummy1.Z0_Code = dummy2.Z0_Code = "SPZ";
			dummy3.Z0_Code = "GAN";

			using (var form = new ZForm())
			using (var control = new ZTextBox())
			{
				form.Controls.Add(control);
				control.SetDataBinding(dummy1, "Z0_Description");

				form.Show();
				Application.DoEvents();

				Assert(dummy1.IsBound);
				Assert(!dummy2.IsBound);
				Assert(!dummy3.IsBound);
			}
		}

		public void TestDateBox()
		{
			var collection = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "SPZ"));
			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = collection.AddNew();
			dummy1.Z0_Code = dummy2.Z0_Code = "SPZ";
			dummy3.Z0_Code = "GAN";

			using (var form = new ZForm())
			using (var control = new ZDateEdit())
			{
				form.Controls.Add(control);
				control.SetDataBinding(dummy1, nameof(dummy1.Z0_SmallDateTime));

				form.Show();
				Application.DoEvents();

				Assert(dummy1.IsBound);
				Assert(!dummy2.IsBound);
				Assert(!dummy3.IsBound);
			}
		}

		public void TestCalcBox()
		{
			var collection = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "SPZ"));
			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = collection.AddNew();
			dummy1.Z0_Code = dummy2.Z0_Code = "SPZ";
			dummy3.Z0_Code = "GAN";

			using (var form = new ZForm())
			using (var control = new ZCalcEdit())
			{
				form.Controls.Add(control);
				control.SetDataBinding(dummy1, nameof(dummy1.Z0_Number));

				form.Show();
				Application.DoEvents();

				Assert(dummy1.IsBound);
				Assert(!dummy2.IsBound);
				Assert(!dummy3.IsBound);
			}
		}

		public void TestCheckbox()
		{
			var collection = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "SPZ"));
			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();
			var dummy3 = collection.AddNew();
			dummy1.Z0_Code = dummy2.Z0_Code = "SPZ";
			dummy3.Z0_Code = "GAN";

			using (var form = new ZForm_Testo())
			using (var control = new ZCheckBox())
			{
				form.Controls.Add(control);
				form.BindingSource_Exposed.SetBindingMember(control, "Z0_Bool");
				form.SetDataBinding(dummy1, "");

				form.Show();
				Application.DoEvents();

				Assert(dummy1.IsBound);
				Assert(!dummy2.IsBound);
				Assert(!dummy3.IsBound);
			}
		}

		class ZForm_Testo : ZForm
		{
			internal KBindingSource BindingSource_Exposed => BindingSource;
		}

		class DummyBusinessObjectWithActiveCollection : DummyBusinessObject
		{
			public DummyBusinessObjectWithActiveCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ActiveBusinessObjectCollection<DummyBusinessObject> ActiveCollection
			{
				get => activeCollection ?? (activeCollection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "SPK")));
			}

			ActiveBusinessObjectCollection<DummyBusinessObject> activeCollection;
		}
	}
}
