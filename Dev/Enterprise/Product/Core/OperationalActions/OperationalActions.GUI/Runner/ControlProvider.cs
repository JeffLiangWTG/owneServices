using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using MathByAnotherName = System.Math;

namespace Enterprise.Services.OperationalActions.GUI
{
	internal static class ControlProvider
	{
		#region Positions

		public const int TopGap = 2;
		public const int BottomGap = 2;
		public const int LeftGap = 2;
		public const int RightGap = 16;
		public const int LabelFieldGap = 2;
		public const int TotalWidth = 400;

		public const int LabelLeft = LeftGap;
		public const int LabelWidth = 120;

		public const int FieldLeft = LabelLeft + LabelWidth + LabelFieldGap;
		public const int FieldWidth = TotalWidth - FieldLeft - RightGap;

		#endregion

		public static Control[] GetControls(RunnerField field)
		{
			return new Control[]
			{
				GetLabelControl(field),
				GetFieldControl(field)
			};
		}

		public static Control GetLabelControl(RunnerField field)
		{
			ZLabel label = new ZLabel();
			label.Text = field.Caption;
			label.TextAlign = ContentAlignment.TopLeft;
			label.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.LabelLeft, ControlProvider.TopGap + 2);

			int requiredHeight;
			using (var g = label.CreateGraphics())
			{
				requiredHeight = (int)MathByAnotherName.Ceiling(g.MeasureString(field.Caption, label.Font, LabelWidth).Height);
			}

			label.Size = ControlDpiScalingHelper.NewScaledSize(LabelWidth, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(requiredHeight));

			return label;
		}

		static readonly Dictionary<Type, Func<RunnerField, Control>> GetRunnerFieldControl = new Dictionary<Type, Func<RunnerField, Control>>()
		{
			{ typeof(RunnerTextField), (f) => GetTextControl((RunnerTextField)f) },
			{ typeof(RunnerCodeField), (f) => GetCodeControl((RunnerCodeField)f) },
			{ typeof(RunnerNKModuleField), (f) => GetNKModuleControl((RunnerNKModuleField)f) },
			{ typeof(RunnerPKModuleField), (f) => GetPKModuleControl((RunnerPKModuleField)f) },
			{ typeof(RunnerDateTimeField), (f) => GetDateTimeControl((RunnerDateTimeField)f) },
			{ typeof(RunnerDateField), (f) => GetDateControl((RunnerDateField)f) },
			{ typeof(RunnerDateTimeOffsetField), (f) => GetDateTimeOffsetControl((RunnerDateTimeOffsetField)f) },
			{ typeof(RunnerTimeField), (f) => GetTimeControl((RunnerTimeField)f) },
			{ typeof(RunnerGeographyField), (f) => GetGeographyControl((RunnerGeographyField)f) },
			{ typeof(RunnerAddressField), (f) => GetAddressControl((RunnerAddressField)f) },
			{ typeof(RunnerNumericField), (f) => GetNumericControl((RunnerNumericField)f) },
			{ typeof(RunnerBooleanField), (f) => GetBooleanControl((RunnerBooleanField)f) },
			{ typeof(RunnerErrorField), (f) => GetErrorControl((RunnerErrorField)f) }
		};

		public static Control GetFieldControl<T>(T field)
			where T : RunnerField
		{
			if (field == null)
			{
				throw new ArgumentNullException(nameof(field));
			}

			if (GetRunnerFieldControl.ContainsKey(field.GetType()))
			{
				return GetRunnerFieldControl[field.GetType()](field);
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(field), field, "unknown field type");
			}
		}

		static Control GetTextControl(RunnerTextField field)
		{
			ZTextBox result = New<ZTextBox>(field, RunnerTextField.Schema.Property);
			result.CharacterCasing = CharacterCasing.Normal;
			result.Location = ControlDpiScalingHelper.NewScaledPoint(FieldLeft, TopGap);

			if (field.FieldSupporter.MaxLength > 50)
			{
				ControlDpiScalingHelper.SetWidth(ref result, FieldWidth, true);
			}
			else
			{
				ControlDpiScalingHelper.SetWidth(ref result, Math.Min(FieldWidth, TextBoxControlSize.GetControlWidth(result, field.FieldSupporter.MaxLength)), false);
			}

			return result;
		}

		static Control GetCodeControl(RunnerCodeField field)
		{
			ZDropEdit result = New<ZDropEdit>(field, RunnerCodeField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(FieldLeft, TopGap);
			ControlDpiScalingHelper.SetWidth(ref result, FieldWidth, true);
			result.BindToList = RunnerCodeField.Schema.Property_List;
			return result;
		}

		static Control GetPKModuleControl(RunnerPKModuleField field)
		{
			ZGuidFindBox result = New<ZGuidFindBox>(field, RunnerPKModuleField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(FieldLeft, TopGap);
			ControlDpiScalingHelper.SetWidth(ref result, FieldWidth, true);
			result.BindToList = RunnerPKModuleField.Schema.Property_List;
			return result;
		}

		static Control GetNKModuleControl(RunnerNKModuleField field)
		{
			ZCodeFindBox result = New<ZCodeFindBox>(field, RunnerNKModuleField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(FieldLeft, TopGap);
			ControlDpiScalingHelper.SetWidth(ref result, FieldWidth, true);
			result.BindToList = RunnerNKModuleField.Schema.Property_List;
			return result;
		}

		static Control GetDateControl(RunnerDateField field)
		{
			ZDateEdit result = New<ZDateEdit>(field, RunnerDateTimeField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.FieldLeft, ControlProvider.TopGap);
			result.DateTimeFormat = ZDateTimePickerFormat.Short;
			return result;
		}

		static Control GetDateTimeControl(RunnerDateTimeField field)
		{
			if (field.FieldSupporter.IsDuration)
			{
				var result = New<ZTimeEditEx>(field, RunnerDateTimeField.Schema.Property);
				result.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.FieldLeft, ControlProvider.TopGap);
				return result;
			}
			else
			{
				ZDateEdit result = New<ZDateEdit>(field, RunnerDateTimeField.Schema.Property);
				result.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.FieldLeft, ControlProvider.TopGap);
				result.DateTimeFormat = field.FieldSupporter.Format;
				return result;
			}
		}

		static Control GetTimeControl(RunnerTimeField field)
		{
			ZTimeTimeEdit result = New<ZTimeTimeEdit>(field, RunnerTimeField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.FieldLeft, ControlProvider.TopGap);
			return result;
		}

		static Control GetDateTimeOffsetControl(RunnerDateTimeOffsetField field)
		{
			ZDateTimeOffsetEdit result = New<ZDateTimeOffsetEdit>(field, RunnerDateTimeOffsetField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.FieldLeft, ControlProvider.TopGap);
			result.DateTimeFormat = ZDateTimePickerFormat.Long;
			return result;
		}

		static Control GetGeographyControl(RunnerGeographyField field)
		{
			ZGeographyEdit result = New<ZGeographyEdit>(field, RunnerGeographyField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.FieldLeft, ControlProvider.TopGap);
			return result;
		}

		static Control GetAddressControl(RunnerAddressField field)
		{
			ZAddressControl result = new ZAddressControl();
			result.Location = ControlDpiScalingHelper.NewScaledPoint(ControlProvider.FieldLeft, ControlProvider.TopGap);
			ControlDpiScalingHelper.SetWidth(ref result, FieldWidth, true);
			result.BindToAddress = RunnerAddressField.Schema.Property;
			result.BindToOrgList = RunnerAddressField.Schema.Organisation_List;
			result.ShowAddress = false;

			foreach (Control subControl in result.Controls)
			{
				IExtendedControl extendedControl;
				IHintExtension hint;

				if ((extendedControl = subControl as IExtendedControl) != null &&
					(hint = extendedControl.Extensions.Get<IHintExtension>()) != null)
				{
					hint.Caption = field.Caption;
					hint.Description = Hint(field.Descriptor.EmptyBehaviour);
				}
			}

			return result;
		}

		static Control GetBooleanControl(RunnerBooleanField field)
		{
			ZDropEdit result = New<ZDropEdit>(field, RunnerCodeField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(FieldLeft, TopGap);
			ControlDpiScalingHelper.SetWidth(ref result, FieldWidth, true);
			result.BindToList = RunnerBooleanField.Schema.Property_List;
			return result;
		}

		static Control GetNumericControl(RunnerNumericField field)
		{
			ZCalcEdit result = New<ZCalcEdit>(field, RunnerNumericField.Schema.Property);
			result.Location = ControlDpiScalingHelper.NewScaledPoint(FieldLeft, TopGap);
			ControlDpiScalingHelper.SetWidth(ref result, FieldWidth, true);
			result.DecimalPlaces = field.FieldSupporter.Scale;

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		static Control GetErrorControl(RunnerErrorField field)
		{
			ZLabel result = new ZLabel();
			result.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(FieldLeft, TopGap);
			result.Text = field.ErrorText;
			result.BorderStyle = BorderStyle.FixedSingle;
			result.BackColor = Color.WhiteSmoke;
			result.ForeColor = Color.Red;
			result.TextAlign = ContentAlignment.TopLeft;
			result.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 2);

			int requiredHeight;
			using (var g = result.CreateGraphics())
			{
				requiredHeight = (int)MathByAnotherName.Round(g.MeasureString(result.Text, result.Font, FieldWidth).Height);
			}

			result.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(FieldWidth, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(requiredHeight) + 4);

			return result;
		}

		static T New<T>(RunnerField field, string bindTo)
			where T : Control, IExtendedControl, new()
		{
			T result = new T();
			result.SetBindingMember(bindTo);

			IHintExtension hint = result.Extensions.Get<IHintExtension>() ?? throw new InvalidOperationException();

			hint.Caption = field.Caption;
			hint.Description = Hint(field.Descriptor.EmptyBehaviour);

			return result;
		}

		static string Hint(ZString emptyBehaviour)
		{
			switch (emptyBehaviour)
			{
				case EmptyBehaviourList.Codes.Apply:
					return Res.GetString("OperationalActionsControlProvider|Apply", "This field will be applied even if it is empty.");
				case EmptyBehaviourList.Codes.Skip:
					return Res.GetString("OperationalActionsControlProvider|Skip", "This field will not be applied if empty.");
				case EmptyBehaviourList.Codes.Mandatory:
					return Res.GetString("OperationalActionsControlProvider|Mandatory", "This field is mandatory.");
				default:
					return "";
			}
		}
	}
}
