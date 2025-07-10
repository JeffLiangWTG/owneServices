using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using MathByAnotherName = System.Math;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class ControlProviderTest : TestCaseWithFactory
	{
		public void TestText()
		{
			OperationalActionTextFieldSupporter supporter = new OperationalActionTextFieldSupporter("Field", false, 15);
			RunnerTextField field = new RunnerTextField(Factory, Descriptor, supporter);
			AssertControls<ZTextBox>(field);

			using (var textBox = (ZTextBox)ControlProvider.GetFieldControl(field))
			{
				AssertEquals(CharacterCasing.Normal, textBox.CharacterCasing);
			}
		}

		public void TestDateTime()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("Field", false, ZDateTimePickerFormat.Long);
			RunnerDateTimeField field = new RunnerDateTimeField(Factory, Descriptor, supporter);
			AssertControls<ZDateEdit>(field);
		}

		public void TestDateTime_Duration()
		{
			OperationalActionDateTimeFieldSupporter supporter = new OperationalActionDateTimeFieldSupporter("Field", false, ZDateTimePickerFormat.Long, isDuration: true);
			RunnerDateTimeField field = new RunnerDateTimeField(Factory, Descriptor, supporter);
			AssertControls<ZTimeEditEx>(field);
		}

		public void TestDate()
		{
			var supporter = new OperationalActionDateFieldSupporter("Field", false);
			var field = new RunnerDateField(Factory, Descriptor, supporter);
			AssertControls<ZDateEdit>(field);

			using (var control = ControlProvider.GetFieldControl(field))
			{
				AssertNotNull("should have returned a control", control);
				AssertType("returned control should be of the correct type.", typeof(ZDateEdit), control);
				AssertEquals(ZDateTimePickerFormat.Short, ((ZDateEdit)control).DateTimeFormat);
			}
		}

		public void TestDateTimeOffset()
		{
			OperationalActionDateTimeOffsetFieldSupporter supporter = new OperationalActionDateTimeOffsetFieldSupporter("Field", false);
			RunnerDateTimeOffsetField field = new RunnerDateTimeOffsetField(Factory, Descriptor, supporter);
			AssertControls<ZDateTimeOffsetEdit>(field);
		}

		public void TestTime()
		{
			OperationalActionTimeFieldSupporter supporter = new OperationalActionTimeFieldSupporter("Field", false);
			RunnerTimeField field = new RunnerTimeField(Factory, Descriptor, supporter);
			AssertControls<ZTimeTimeEdit>(field);
		}

		public void TestGeography()
		{
			OperationalActionGeographyFieldSupporter supporter = new OperationalActionGeographyFieldSupporter("Field", false);
			RunnerGeographyField field = new RunnerGeographyField(Factory, Descriptor, supporter);
			AssertControls<ZGeographyEdit>(field);
		}

		public void TestCode()
		{
			OperationalActionCodeFieldSupporter supporter = new OperationalActionCodeFieldSupporter("Field", false, new CodeDescriptionPairList());
			RunnerCodeField field = new RunnerCodeField(Factory, Descriptor, supporter);
			AssertControls<ZDropEdit>(field);
		}

		public void TestPKModule()
		{
			OperationalActionPKModuleFieldSupporter supporter = new OperationalActionPKModuleFieldSupporter("Field", false, (BusinessObjectFactory factory) => new RefUNLOCOCollection(factory));
			RunnerPKModuleField field = new RunnerPKModuleField(Factory, Descriptor, supporter);
			AssertControls<ZGuidFindBox>(field);
		}

		public void TestNKModule()
		{
			OperationalActionNKModuleFieldSupporter supporter = new OperationalActionNKModuleFieldSupporter("Field", false, 5, (BusinessObjectFactory factory) => new RefUNLOCOCollection(factory));
			RunnerNKModuleField field = new RunnerNKModuleField(Factory, Descriptor, supporter);
			AssertControls<ZCodeFindBox>(field);
		}

		public void TestAddressModule()
		{
			OperationalActionAddressFieldSupporter supporter = new OperationalActionAddressFieldSupporter("Field", false, AddressType.NoDefault, (BusinessObjectFactory factory) => new ConsigneeCollection(factory));
			RunnerAddressField fieldControl = new RunnerAddressField(Factory, Descriptor, supporter);

			using (Control subControl = ControlProvider.GetFieldControl(fieldControl))
			{
				AssertNotNull("should have returned a control", subControl);
				AssertType("returned control should be of the correct type.", typeof(ZAddressControl), subControl);
			}
		}

		public void TestBoolean()
		{
			OperationalActionBooleanFieldSupporter supporter = new OperationalActionBooleanFieldSupporter("Field", false);
			RunnerBooleanField field = new RunnerBooleanField(Factory, Descriptor, supporter);
			AssertControls<ZDropEdit>(field);
		}

		public void TestNumeric()
		{
			OperationalActionNumericFieldSupporter supporter = new OperationalActionNumericFieldSupporter("Field", false, -100, 100, 5, 2);
			RunnerNumericField field = new RunnerNumericField(Factory, Descriptor, supporter);
			AssertControls<ZCalcEdit>(field);
		}

		public void TestError()
		{
			RunnerErrorField field = new RunnerErrorField(Factory, Descriptor, "Error Text");
			AssertLabelControl(field);

			using (Control control = ControlProvider.GetFieldControl(field))
			{
				AssertNotNull("should have returned a control", control);
				AssertEquals("returned control should be of the correct type.", typeof(ZLabel), control.GetType());

				ZLabel label = (ZLabel)control;
				AssertEquals("should have the correct text", field.ErrorText, control.Text);
			}
		}

		public void TestHints()
		{
			OperationalActionTextFieldSupporter supporter = new OperationalActionTextFieldSupporter("Field", false, 15);
			RunnerTextField field = new RunnerTextField(Factory, Descriptor, supporter);

			Descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Apply;
			AssertHint("Apply", field, "This field will be applied even if it is empty.");

			Descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
			AssertHint("Skip", field, "This field will not be applied if empty.");

			Descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Mandatory;
			AssertHint("Mandatory", field, "This field is mandatory.");
		}

		public void TestLabelsAreScaledCorrectly()
		{
			var supporter = new OperationalActionTextFieldSupporter("Abcdefg hijk (lmnop qr stuv)", false, 15);
			var field = new RunnerTextField(Factory, Descriptor, supporter);
			using (var label = ControlProvider.GetLabelControl(field))
			using (var g = label.CreateGraphics())
			{
				var textHeight = (int)MathByAnotherName.Ceiling(g.MeasureString(field.Caption, label.Font, ControlProvider.LabelWidth).Height);
				var expectedHeight = ControlDpiScalingHelper.NewScaledSize(ControlProvider.LabelWidth, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(textHeight)).Height;
				AssertEquals(expectedHeight, label.Height);
			}
		}

		#region Implementation

		void AssertControls<T>(RunnerField field)
			where T : Control, IExtendedControl
		{
			AssertLabelControl(field);
			AssertFieldControl<T>(field);
		}

		void AssertLabelControl(RunnerField field)
		{
			using (Control labelControl = ControlProvider.GetLabelControl(field))
			{
				AssertNotNull("should have returned a control", labelControl);
				AssertEquals("returned control should be of teh correct type.", typeof(ZLabel), labelControl.GetType());
				AssertEquals("label should have the correct text", "Caption", labelControl.Text);
			}
		}

		void AssertFieldControl<T>(RunnerField field)
			where T : Control, IExtendedControl
		{
			using (Control fieldControl = ControlProvider.GetFieldControl(field))
			{
				AssertNotNull("should have returned a control", fieldControl);
				AssertType("returned control should be of the correct type.", typeof(T), fieldControl);

				IHintExtension hint = ((T)fieldControl).Extensions.Get<IHintExtension>();
				AssertNotNull(hint);
				AssertEquals(field.Caption, hint.Caption);
			}
		}

		void AssertHint(string message, RunnerField field, string hintText)
		{
			using (Control fieldControl = ControlProvider.GetFieldControl(field))
			{
				IExtendedControl extendedControl = fieldControl as IExtendedControl;
				IHintExtension hint = extendedControl.Extensions.Get<IHintExtension>();

				AssertEquals(message, hintText, hint.Description);
			}
		}

		public OperationalActionFieldDescriptor Descriptor
		{
			get
			{
				if (descriptor == null)
				{
					descriptor = new OperationalActionFieldDescriptor(Action);
					descriptor.FieldCaption = "Caption";
				}
				return descriptor;
			}
		}
		OperationalActionFieldDescriptor descriptor;

		public OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}
				return action;
			}
		}
		OperationalAction action;

		public OperationalActionContext Context
		{
			get { return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name")); }
		}
		OperationalActionContext context;

		public OperationalActionSupporter ActionSupporter
		{
			get { return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter); }
		}
		OperationalActionSupporter actionSupporter;

		#endregion
	}
}
