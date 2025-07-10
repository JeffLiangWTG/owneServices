using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7StandAloneDeclarationUserControl))]
	sealed class EUH7StandAloneDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestStandAloneDeclarationTextBox()
		{
			using (var control = new EUH7StandAloneDeclarationUserControl())
			{
				control.Show();

				var standAloneDeclarationTextBox = control.FindSingle<ZTextBox>("StandAloneDeclarationTextBox");

				AssertEquals("BindingMember", "EntrySummaryReferenceNumber", standAloneDeclarationTextBox.GetBindingMember());
			}
		}

		public void TestButtonIsVisibleForBinding()
		{
			using (var control = new EUH7StandAloneDeclarationUserControl())
			{
				control.Show();
				control.SetDataBinding(bill, "");

				bill.EntrySummaryReferenceNumber = "B00001234";

				var editButton = control.FindSingle<ZButton>("EditButton");
				var convertButton = control.FindSingle<ZButton>("ConvertButton");

				AssertIsVisibleForBindingZBinding(editButton.DataBindings, bill, "CanEditStandAloneDeclaration");
				AssertIsVisibleForBindingZBinding(convertButton.DataBindings, bill, "CanConvertToStandAloneDeclaration");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.New<AsycudaBill>();
		}

		void AssertIsVisibleForBindingZBinding(ControlBindingsCollection bindingCollection, object expectedDataSource, string expectedDataMember)
		{
			foreach (Binding binding in bindingCollection)
			{
				if (binding is KBinding && binding.PropertyName == "IsVisibleForBinding")
				{
					AssertEquals("IsVisibleForBinding", binding.PropertyName);
					AssertEquals(expectedDataSource, binding.DataSource);
					AssertEquals(expectedDataMember, binding.BindingMemberInfo.BindingMember);
					return;
				}
			}

			Fail("IsVisibleForBinding property is not bound");
		}

		AsycudaBill bill;
	}
}
