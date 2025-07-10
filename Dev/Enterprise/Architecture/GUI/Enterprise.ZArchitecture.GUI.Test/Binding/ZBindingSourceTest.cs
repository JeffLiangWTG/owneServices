using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZBindingSourceTest : TestCaseWithDummy
	{
		public void TestFetchHintGeneratorsInvoked()
		{
			OperationIdentifier = 1;

			using (var control1 = new ControlWithFetchHintGenerator())
			using (var control2 = new ControlWithFetchHintGenerator())
			{
				Form.Controls.Add(control1);
				Form.Controls.Add(control2);
				Form.Show();
				Application.DoEvents();

				BindingSource.SetBindingMember(control1, DummyBizoSchema.Z0_Code.Name);
				BindingSource.SetBindingMember(control2, DummyBizoSchema.Z0_Code.Name);

				BindingSource.SetDataBinding(Factory.New<DummyDependantBusinessObject>(), "Parent");
				AssertEquals("All AddFetchHints called before SetDataBinding", 1, control1.AddFetchHintsIndex);
				AssertEquals("All AddFetchHints called before SetDataBinding", 2, control2.AddFetchHintsIndex);
				AssertEquals("SetDataBinding called after all AddFetchHints", 3, control1.SetDataBindingIndex);
				AssertEquals("SetDataBinding called after all AddFetchHints", 4, control2.SetDataBindingIndex);
			}
		}

		public void TestSetDefaultBindingMembers_AtDesignTime()
		{
			using (var containerControl = new ZUserControl())
			using (var userControl = new ZUserControl())
			using (var userControlExplicitlySetToNotBind = new ZUserControl())
			using (ComponentExtensions.SwitchToDesignMode())
			{
				containerControl.Controls.Add(userControl);
				containerControl.Controls.Add(userControlExplicitlySetToNotBind);
				((ICompositeControlBindingSourceProvider)containerControl).BindingSource.SetBindingMember(userControlExplicitlySetToNotBind, "");

				AssertEquals("Container control shouldn't be defaulted", "", ((ICompositeControlBindingSourceProvider)containerControl).BindingSource.GetBindingMember(containerControl));
				AssertEquals("userControl should be defaulted to '.'", ".", ((ICompositeControlBindingSourceProvider)containerControl).BindingSource.GetBindingMember(userControl));
				AssertEquals("userControlExplicitlySetToNotBind should not be defaulted", "", ((ICompositeControlBindingSourceProvider)containerControl).BindingSource.GetBindingMember(userControlExplicitlySetToNotBind));
			}
		}

		public void TestSetDefaultBindingMembers_AtRuntimeIfNotPerformedAtDesignTime_ThisShouldBeTemporary()
		{
			using (var containerControl = new ZForm())
			using (var userControl = new ZUserControl())
			{
				containerControl.Controls.Add(userControl);
				containerControl.SetDataBinding(Dummy, "");
				AssertEquals("BindingMember defaulted to '.' for all ZUserControls", ".", ((ICompositeControlBindingSourceProvider)containerControl).BindingSource.GetBindingMember(userControl));
			}
		}

		[ThreadStatic]
		static int OperationIdentifier;

		#region Test Classes

		[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
		class ControlWithFetchHintGenerator : ZUserControl, IDataBoundControl, IFetchHintGenerator
		{
			public int SetDataBindingIndex;
			public int AddFetchHintsIndex;

			void IFetchHintGenerator.AddFetchHint(object dataSource, string dataMember)
			{
				AddFetchHintsIndex = OperationIdentifier++;
			}

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				SetDataBindingIndex = OperationIdentifier++;
			}

			#region PropertyDescriptors

			public static PropertyDescriptor[] GetPropertyDescriptors()
			{
				return new ControlPropertyDescriptorBuilder<ZModuleButtonGrid>().Result;
			}

			#endregion
		}

		#endregion

		#region Implementation

		ZBindingSource BindingSource
		{
			get { return bindingSource ?? (bindingSource = new ZBindingSource()); }
		}
		ZBindingSource bindingSource;

		ZForm Form
		{
			get { return form ?? (form = new ZForm()); }
		}
		ZForm form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
