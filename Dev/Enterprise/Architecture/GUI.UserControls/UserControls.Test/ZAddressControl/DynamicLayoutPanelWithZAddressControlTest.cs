using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DynamicLayoutPanelWithZAddressControlTest : TestCaseWithFactory
	{
		public void TestZAddressControl_SetDataBinding()
		{
			var bo = Factory.New<DummyWithZAddress>();
			var childBo = bo.Dummies.AddNew();
			childBo.Z0_NVarChar = "A";

			using (var form = new FormForTest(bo))
			{
				var panel = form.PanelForChildBusinessObject;
				panel.SetDataBinding(bo, nameof(DummyWithZAddress.Dummies));
				panel.UpdateLayout(new ZAddressControlLayoutProviderForTest());
				form.Show();

				AssertEquals("Dummies.Lookups.DummyWithZAddressList", panel.FindSingle<ZAddressControl>(nameof(BagWithZAddressControlForTest.AddressControl)).BindToOrgList);
			}
		}

		public void TestZAddressControl_CorrectBindToOrgList()
		{
			var bo = Factory.New<DummyWithZAddress>();
			var childBo = bo.Dummies.AddNew();
			childBo.Z0_NVarChar = "A";

			using (var form = new FormWithDynamicControlCreationUserControlForTest(bo))
			{
				form.PanelForChildBusinessObject.UserControlType = typeof(ControlWithDynamicLayoutPanelForTest);
				form.Show();

				AssertEquals("Dummies.Lookups.DummyWithZAddressList", form.PanelForChildBusinessObject.FindSingle<ZAddressControl>(nameof(BagWithZAddressControlForTest.AddressControl)).BindToOrgList);
			}
		}

		sealed class ZAddressControlLayoutProviderForTest : IPanelLayoutProvider
		{
			public PanelLayout Layout => CreateLayout();

			PanelLayout CreateLayout()
			{
				var common = BagWithZAddressControlForTest.Instance;
				var layout = new PanelLayout();
				layout.RegisterControlBag(common);

				layout.Include(common.AddressControl);
				layout.SetVisibility<DummyWithZAddress>(common.AddressControl, d => d.Z0_NVarChar == "A", d => d.Z0_NVarCharInfo);

				return layout;
			}
		}

		sealed class BagWithZAddressControlForTest : ControlBag
		{
			public static BagWithZAddressControlForTest Instance { get; } = new BagWithZAddressControlForTest();

			BagWithZAddressControlForTest()
			{
				AddressControl = RegisterControl(nameof(AddressControl));
			}

			public ControlReference AddressControl { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZUserControl();
				template.BindingSource.DataSourceType = typeof(DummyWithZAddress);
				var addressControl = new ZAddressControl { Name = nameof(AddressControl), BindToOrgList = "Lookups.DummyWithZAddressList" };
				template.Controls.Add(addressControl);
				template.BindingSource.SetBindingMember(addressControl, "Z0_Guid");
				return template;
			}
		}

		sealed class FormForTest : ZForm
		{
			public FormForTest(DummyWithZAddress bo) : base(bo)
			{
				Controls.Add(PanelForChildBusinessObject);
				BindingSource.SetBindingMember(PanelForChildBusinessObject, nameof(DummyWithZAddress.Dummies));
				SetDataBinding(bo, "");
			}

			public readonly DynamicLayoutPanel PanelForChildBusinessObject = new DynamicLayoutPanel();
		}

		sealed class FormWithDynamicControlCreationUserControlForTest : ZForm
		{
			public FormWithDynamicControlCreationUserControlForTest(DummyWithZAddress bo) : base(bo)
			{
				Controls.Add(PanelForChildBusinessObject);
				BindingSource.SetBindingMember(PanelForChildBusinessObject, nameof(DummyWithZAddress.Dummies));
				SetDataBinding(bo, "");
			}

			public readonly ZDynamicControlCreationUserControl PanelForChildBusinessObject = new ZDynamicControlCreationUserControl();
		}

		sealed class ControlWithDynamicLayoutPanelForTest : ZUserControl
		{
			public ControlWithDynamicLayoutPanelForTest() : base()
			{
				Controls.Add(panel);
			}

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				base.SetDataBinding(dataSource, dataMember);
				panel.UpdateLayout(new ZAddressControlLayoutProviderForTest());
			}

			readonly DynamicLayoutPanel panel = new DynamicLayoutPanel();
		}
	}
}
