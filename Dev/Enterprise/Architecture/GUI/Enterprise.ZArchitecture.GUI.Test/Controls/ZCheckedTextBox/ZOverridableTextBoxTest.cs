using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZOverridableTextBoxTest : ZControlBaseTestCase<ZOverridableTextBox>
	{
		public void TestBindingBetweenControlAndBusinessLayer()
		{
			var dummy = Factory.New<DummyToBind>();
			dummy.Z0_Description = string.Empty;
			dummy.Z0_Bool = false;
			dummy.TextOverride = string.Empty;
			dummy.PlaceholderText = string.Empty;
			dummy.TextIsOverridden = false;
			using (var form = new ZForm(dummy))
			{
				var box = new ZOverridableTextBox();
				box.BindTo = nameof(dummy.TextOverride);
				box.BindToForPlaceholderText = nameof(dummy.PlaceholderText);
				box.BindToForTextIsOverridden = nameof(dummy.TextIsOverridden);

				var controlTextOverriddenChangedFired = false;
				var controlPlaceholderTextChangedFired = false;
				var controlTextIsOverriddenChangedFired = false;
				box.TextOverrideChanged += delegate
				{ controlTextOverriddenChangedFired = true; };
				box.PlaceholderTextChanged += delegate
				{ controlPlaceholderTextChangedFired = true; };
				box.TextIsOverriddenChanged += delegate
				{ controlTextIsOverriddenChangedFired = true; };

				form.Controls.Add(box);
				form.Show();

				dummy.TextOverride = "Text Override";
				AssertEquals("TextOverrideChanged should be fired", true, controlTextOverriddenChangedFired);
				AssertEquals("TextOverride should reflect the property bound to TextOverride", "Text Override", box.TextOverride);

				dummy.PlaceholderText = "Placeholder Text";
				AssertEquals("PlaceholderTextChanged should be fired", true, controlPlaceholderTextChangedFired);
				AssertEquals("PlaceholderText should reflect the property bound to PlaceholderText", "Placeholder Text", box.PlaceholderText);

				dummy.TextIsOverridden = true;
				AssertEquals("TextIsOverriddenChanged should be fired", true, controlTextIsOverriddenChangedFired);
				AssertEquals("TextIsOverridden should reflect the property bound to TextIsOverridden", true, box.TextIsOverridden);

				var bizoTextOverriddenChangedFired = false;
				var bizoPlaceholderTextChangedFired = false;
				var bizoTextIsOverriddenChangedFired = false;
				dummy.TextOverrideInfo.ValueChanged += delegate
				{ bizoTextOverriddenChangedFired = true; };
				dummy.PlaceholderTextInfo.ValueChanged += delegate
				{ bizoPlaceholderTextChangedFired = true; };
				dummy.TextIsOverriddenInfo.ValueChanged += delegate
				{ bizoTextIsOverriddenChangedFired = true; };

				box.TextOverride = "I'll override you!";
				AssertEquals("Dummy TextOverrideChanged event should be fired", true, bizoTextOverriddenChangedFired);
				AssertEquals("TextOverride should reflect the property bound to TextOverride", "I'LL OVERRIDE YOU!", dummy.TextOverride);

				box.PlaceholderText = "Another Placeholder Text";
				AssertEquals("Dummy PlaceholderTextChanged event should be fired", true, bizoPlaceholderTextChangedFired);
				AssertEquals("PlaceholderText should reflect the property bound to PlaceholderText", "Another Placeholder Text", dummy.PlaceholderText);

				box.TextIsOverridden = false;
				AssertEquals("Dummy TextIsOverriddenChanged event should be fired", true, bizoTextIsOverriddenChangedFired);
				AssertEquals("TextIsOverridden should reflect the property bound to TextIsOverridden", false, dummy.TextIsOverridden);
			}
		}

		public void TestBindingTextAndChecked()
		{
			var dummy = Factory.New<DummyToBind>();
			dummy.Z0_Description = string.Empty;
			dummy.Z0_Bool = false;
			using (var form = new ZForm(dummy))
			{
				var box = new ZOverridableTextBox();
				box.BindTo = nameof(dummy.TextOverride);
				box.BindToForText = DummyBizoSchema.Z0_Description.Name;
				box.BindToForChecked = DummyBizoSchema.Z0_Bool.Name;

				var controlTextChangedFired = false;
				var controlCheckedChangedFired = false;
				box.TextChanged += delegate
				{ controlTextChangedFired = true; };
				box.CheckedChanged += delegate
				{ controlCheckedChangedFired = true; };

				form.Controls.Add(box);
				form.Show();

				dummy.Z0_Description = "bound";
				AssertEquals("TextChanged should be fired", true, controlTextChangedFired);
				AssertEquals("Text should reflect the property bound to Text", "BOUND", box.Text);

				dummy.Z0_Bool = true;
				AssertEquals("CheckedChanged should be fired", true, controlCheckedChangedFired);
				AssertEquals("Checked should reflect the property bound to Checked", true, box.Checked);

				var bizoTextChanged = false;
				var bizoCheckedChanged = false;
				dummy.Z0_DescriptionInfo.ValueChanged += delegate
				{ bizoTextChanged = true; };
				dummy.Z0_BoolInfo.ValueChanged += delegate
				{ bizoCheckedChanged = true; };

				box.Text = "Some new text";
				AssertEquals("Dummy TextChanged event should be fired", true, bizoTextChanged);
				AssertEquals("Text should reflect the property bound to Text", "SOME NEW TEXT", dummy.Z0_Description);

				box.Checked = false;
				box.Checked = true;
				AssertEquals("Dummy CheckedChanged event should be fired", true, bizoCheckedChanged);
				AssertEquals("Checked should reflect the property bound to Checked", true, dummy.Z0_Bool);
			}
		}

		public void TestNotifications()
		{
			var dummy = Factory.New<DummyWithValidationErrors>();
			using (var form = new ZForm(dummy))
			{
				var box = new ZCheckedTextBox();
				box.BindTo = nameof(dummy.PropertyWithErrors);
				form.Controls.Add(box);
				form.Show();

				form.Validate();
				AssertEquals("TextBox should render a notification error", 1, box.TextBox.GetExtension<NotificationExtension>().Notifications.Count());
			}
		}

		#region Implementation

		protected override void BindControl()
		{
			base.BindControl();
			Control.BindTo = DummyBizoSchema.Z0_Description.Name;
			DataBoundControl.Get(Control).SetDataBinding(Dummy, Control.BindTo);
		}

		class DummyToBind : DummyBusinessObject
		{
			public DummyToBind(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			public virtual ZString TextOverride
			{
				get { return textOverride; }
				set
				{
					textOverride = value;
					TextOverrideInfo.RefreshBinding();
				}
			}
			ZString textOverride;
			public virtual ZPropertyInfo TextOverrideInfo { get { return GetZPropertyInfo(nameof(TextOverride)); } }

			public virtual ZString PlaceholderText
			{
				get { return placeholderText; }
				set
				{
					placeholderText = value;
					PlaceholderTextInfo.RefreshBinding();
				}
			}
			ZString placeholderText;

			public virtual ZPropertyInfo PlaceholderTextInfo { get { return GetZPropertyInfo(nameof(PlaceholderText)); } }

			public virtual ZBool TextIsOverridden
			{
				get { return textIsOverridden; }
				set
				{
					textIsOverridden = value;
					TextIsOverriddenInfo.RefreshBinding();
				}
			}
			ZBool textIsOverridden;
			public virtual ZPropertyInfo TextIsOverriddenInfo { get { return GetZPropertyInfo(nameof(TextIsOverridden)); } }
		}

		#endregion
	}
}
