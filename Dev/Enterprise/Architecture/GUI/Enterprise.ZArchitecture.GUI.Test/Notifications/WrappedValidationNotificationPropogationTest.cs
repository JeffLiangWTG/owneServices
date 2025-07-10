using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class WrappedValidationNotificationPropogationTest : TestCaseWithFactory
	{
		public void TestTabThroughValidationOnWrappedProperty()
		{
			AssertEquals("Wrapped property should have had an errors set during validation.", 0, Dummy.Wrapped_Z0_DescriptionInfo.GetErrors().Count());
			DummyInternals.Validate(Dummy.Wrapped_Z0_DescriptionInfo.Name);

			AssertEquals("Wrapped property should have had an error set during validation (validation not run).", 1, Dummy.Wrapped_Z0_DescriptionInfo.GetErrors().Count());
		}

		public void TestInfoNotificationsGetPropagatedToTheWrappedPropertyInfo()
		{
			using (var form = new TestInfoNotificationsGetPropagatedToTheWrappedPropertyInfoForm(Dummy))
			{
				form.Show();

				System.Windows.Forms.Application.DoEvents();
				GUI.UserIdleWorker.Flush();

				Assert("Pre-condition", !form.Z0_DescriptionTextBox.Extensions.Get<INotificationExtension>().Notifications.HasErrors());
				Assert("Pre-condition", !form.Wrapped_Z0_DescriptionTextBox.Extensions.Get<INotificationExtension>().Notifications.HasErrors());

				DummyInternals.Validate(Dummy.Wrapped_Z0_DescriptionInfo);
				Assert("Sanity check. Should have error notification", form.Z0_DescriptionTextBox.Extensions.Get<INotificationExtension>().Notifications.HasErrors());
				Assert("Should be propagated", form.Wrapped_Z0_DescriptionTextBox.Extensions.Get<INotificationExtension>().Notifications.HasErrors());
			}
		}

		class TestInfoNotificationsGetPropagatedToTheWrappedPropertyInfoForm : GUI.ZChildForm
		{
			public TestInfoNotificationsGetPropagatedToTheWrappedPropertyInfoForm(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.Controls.Add(Z0_DescriptionTextBox);
				this.Controls.Add(Wrapped_Z0_DescriptionTextBox);
			}

			public ZTextBox Z0_DescriptionTextBox
			{
				get
				{
					if (fZ0_DescriptionTextBox == null)
					{
						fZ0_DescriptionTextBox = new ZTextBox();
						fZ0_DescriptionTextBox.BindTo = "Dummy2.Z0_Description";
					}
					return fZ0_DescriptionTextBox;
				}
			}

			public ZTextBox Wrapped_Z0_DescriptionTextBox
			{
				get
				{
					if (fWrapped_Z0_DescriptionTextBox == null)
					{
						fWrapped_Z0_DescriptionTextBox = new ZTextBox();
						fWrapped_Z0_DescriptionTextBox.Top = 30;
						fWrapped_Z0_DescriptionTextBox.BindTo = "Wrapped_Z0_Description";
					}
					return fWrapped_Z0_DescriptionTextBox;
				}
			}

			ZTextBox fZ0_DescriptionTextBox;
			ZTextBox fWrapped_Z0_DescriptionTextBox;
		}

		#region Setup

		DummyWithWrappedProperty Dummy;
		IBusinessObjectInternals DummyInternals;

		protected override void SetUp()
		{
			base.SetUp();

			Dummy = (DummyWithWrappedProperty)Factory.New(typeof(DummyWithWrappedProperty));
			DummyInternals = Dummy;
		}

		#endregion

		#region DummyWithWrappedProperty

		class DummyWithWrappedProperty : DummyBusinessObject
		{
			public DummyWithWrappedProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString Wrapped_Z0_Description
			{
				get { return Dummy2.Z0_Description; }
				set { Dummy2.Z0_Description = value; }
			}

			public ZPropertyInfo Wrapped_Z0_DescriptionInfo
			{
				get { return GetWrappedZPropertyInfo(nameof(Wrapped_Z0_Description), x => Dummy2.Z0_DescriptionInfo); }
			}

			public Dummy2 Dummy2
			{
				get
				{
					if (fDummy2 == null)
					{
						fDummy2 = (Dummy2)Factory.New(typeof(Dummy2));
					}
					return fDummy2;
				}
			}

			Dummy2 fDummy2;
		}

		class Dummy2 : DummyBusinessObject
		{
			public Dummy2(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return new Dummy2Validation(this);
			}
		}

		class Dummy2Validation : DummyBizoValidation
		{
			public Dummy2Validation(Dummy2 parent)
				: base(parent)
			{
			}

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();
				Parent.Z0_DescriptionInfo.AddError("Description Error");
			}
		}

		#endregion
	}
}
