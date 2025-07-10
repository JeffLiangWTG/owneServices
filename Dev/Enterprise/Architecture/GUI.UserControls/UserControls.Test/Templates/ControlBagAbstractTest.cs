using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestsSubclassesOf(typeof(ControlBag))]
	public abstract class ControlBagAbstractTest : TestCaseWithFactory
	{
		public void TestControlCount()
		{
			AssertEquals("Missing Registered Controls", RegisteredControlNames.Count(), ControlBagForTesting.Controls.Count);
		}

		public void TestControlRegistered()
		{
			CombineAssertions(() =>
			{
				foreach (var registeredControl in RegisteredControlNames)
				{
					AssertEquals($"ContainsControl ({registeredControl})", true, ControlBagForTesting.ContainsControl(registeredControl));
				}
			});
		}

		public void TestControlNames()
		{
			var validControlSuffixes = new[]
			{
				"OrganisationControl", "AddressControl", "Button", "CalcEdit", "CalcFindBox", "CheckBox", "CodeFindBox", "DateEdit", "YearEdit", "DropEdit", "DropEditWithFixedWidth", "Grid", "GroupBox", "GuidFindBox", "IntEdit", "NotePopupEdit",
				"LocalCurrencyControl", "LongTextControl", "OrganisationFindBox", "TariffFindBox", "TextBox", "UserControl", "TimeEdit", "LinkLabel", "Label", "DateTimeOffsetEdit", "AddressWithContactControl", "TabControl", "Panel", "Grid"
			};
			CombineAssertions(() =>
			{
				foreach (var controlName in RegisteredControlNames)
				{
					AssertEquals($"{controlName} contains the field name e.g JE_ApplicationCodeBoundDropEdit. This is an older style, please update the name to represent the control usage and type e.g. ApplicationCodeDropEdit", false, controlName.Contains('_'));
					AssertEquals($"{controlName} does not have a suffix of the the control type, such as Button, CalcEdit, CheckBox, etc. e.g. ApplicationCodeDropEdit", true, validControlSuffixes.Any(x => controlName.EndsWith(x)));
				}
			});
		}

		[RequiresSTA]
		public void TestNoBindingMemberDuplicates()
		{
			Assert(true); //In case there is only one control

			var execptions = new HashSet<string>
			{
				"CustomsOfficeShortCodeLengthDropEdit", //Different PreBoundMaxLength
				"FixedMaxLengthInvoiceNumberDropEdit", //Different PreBoundMaxLength
				"ManifestTypeShortCodeLengthDropEdit", //Different PreBoundMaxLength
				"RegistrationDateTimeDateEdit",//Uses a different DataTypePiker
				"ShortEstArrivalDateEdit", //Uses a different DataTypePiker
				"DestinationUsingZZRefCusCodeListCodeFindBox", //Different ModuleID
				"DescriptionMultilineTextBox", //Has multiline set to true
				"VesselNationalityDeclaredValueTextBox", //Different visibility setting
				"VesselNationalityUnloadedValueCodeFindBox", //Different visibility setting
				"OriginStateDropEdit", //Different PreBoundMaxLength
				"WithDescriptionPrimaryPreferenceDropEdit", //Contains description
				"WithDescriptionTariffFindBox", //Contains description
				"VoyageNumberTextBox", //Uses differnt resource string
				"FormattedDateTimeDateEdit", //Uses a different DataTypePiker
			};

			using (var templateControl = GetControlBagForTesting().TemplateControl)
			{
				var controls = templateControl.Controls.Cast<Control>().Where(x => !execptions.Contains(x.Name)).OrderBy(x => x.Name).ToArray();
				CombineAssertions(() =>
				{
					var index = 1;
					foreach (var controlToTestForDuplicate in controls)
					{
						//Pedro suggestion for performance, no need to test controls we have already tested again.
						var remainingControls = controls.Skip(index);
						index++;
						foreach (var remainingControl in remainingControls)
						{
							AssertEquals("Control " + controlToTestForDuplicate.Name + " and control " + remainingControl.Name + " both use the same BindingMember: " + controlToTestForDuplicate.GetBindingMember(), false, CheckIfControlTypeAndBindingAreEqual(controlToTestForDuplicate, remainingControl));
						}
					}
				});
			}
		}

		protected abstract IEnumerable<string> RegisteredControlNames { get; }

		protected abstract ControlBag GetControlBagForTesting();

		ControlBag ControlBagForTesting => controlBagForTesting ?? (controlBagForTesting = GetControlBagForTesting());
		ControlBag controlBagForTesting;

		bool CheckIfControlTypeAndBindingAreEqual(Control control1, Control control2)
		{
			var bindingMember1 = control1.GetBindingMember();
			var bindingMember2 = control2.GetBindingMember();

			if (!string.IsNullOrEmpty(bindingMember1) && !bindingMember1.Equals(".") && !string.IsNullOrEmpty(bindingMember2) && !bindingMember2.Equals("."))
			{
				return control1.GetType().Equals(control2.GetType()) && bindingMember1.Equals(bindingMember2);
			}

			return false;
		}
	}
}
