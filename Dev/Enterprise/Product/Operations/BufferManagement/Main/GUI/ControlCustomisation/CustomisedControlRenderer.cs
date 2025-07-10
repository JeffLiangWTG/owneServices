using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public static class CustomisedControlRenderer
	{
		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		public static ZUserControl Render(ControlCustomisationViewModel viewModel)
		{
			var customisation = viewModel.Customisation;

			var baseControl = new ZUserControl
			{
				CaptionRenderingEnabled = true,
				Size = ControlDpiScalingHelper.NewScaledSize(customisation.Width, customisation.Height, true),
			};

			try
			{
				if (!customisation.BackgroundColor.IsEmpty)
				{
					baseControl.BackColor = customisation.BackgroundColorValue;
				}
				if (!customisation.BackgroundImage.IsEmpty)
				{
					baseControl.BackgroundImage = customisation.BackgroundImageValue;
				}

				AddCustomisedControls(baseControl, customisation, viewModel);
				AddFields(baseControl, customisation, viewModel);

				foreach (Control control in baseControl.Controls)
				{
					control.ForceBindingIncludingParents();
					var customisationLine = control.Tag as ControlCustomisationBase;
					if (customisationLine != null && customisationLine.BringToFront)
					{
						control.BringToFront();
					}
				}
			}
#pragma warning disable ENT0001
			catch
			{
				try
				{
					baseControl.Dispose();
				}
				catch
#pragma warning restore ENT0001
				{
				} // Correct pattern for disposing in factory methods.
				throw;
			}

			return baseControl;
		}

		#region RenderForPreview

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		public static ZUserControl RenderForPreview(BMControlCustomisation customisation)
		{
			var task = GetDummyTaskForPreview();
			var viewModel = new ControlCustomisationViewModel(customisation, new TaskCardContent(task, null));

			var control = CustomisedControlRenderer.Render(viewModel);
			try
			{
				if (control != null)
				{
					control.Tag = viewModel;
					control.SetDataBinding(task, string.Empty);
				}
			}
			catch (KDataBindingException)
			{
				try
				{
					control.Dispose();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				} // Correct pattern for disposing in factory methods.
				throw;
			}

			return control;
		}

		static ProcessTask GetDummyTaskForPreview()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "CustomisedControlRenderer.GetDummyTaskForPreview", RefreshEnabled = false };
			factory.SuspendValidation();
			var job = factory.New<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, factory).ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			SetDummyValuesForPersistentProperties(job, workflow, task);

			var staff = factory.New<GlbStaff>();
			staff.GS_Code = "DUM";
			staff.GS_FullName = (NoResString)"Preview User"; // Dummy staff record for preview

			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartGraduatedFinish;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			return task;
		}

		static void SetDummyValuesForPersistentProperties(params BusinessObject[] bizos)
		{
			foreach (var bizo in bizos)
			{
				foreach (ZPropertyInfo propertyInfo in bizo.ZPropertyInfoHash)
				{
					if (propertyInfo.HasSetter && !propertyInfo.ReadOnly && propertyInfo.PropertyType != typeof(ZBool) && !propertyInfo.PropertyDescriptor.Attributes.OfType<CustomisedControlExcludeAttribute>().Any())
					{
						var dummyValue = BusinessObjectHelper.GetNonDefaultValueForZType(propertyInfo);

						if (dummyValue != null)
						{
							propertyInfo.Value = dummyValue;
						}
					}
				}
			}
		}

		#endregion

		#region CustomisedControls

		static void AddCustomisedControls(ZUserControl baseControl, BMControlCustomisation customisation, ControlCustomisationViewModel viewModel)
		{
			foreach (var customisedControl in customisation.CustomisedControls.Cast<StaticControlCustomisation>().ToArray().OrderBy(c => c.DisplaySequence))
			{
				var control = GetControl(baseControl, customisedControl, viewModel);
				if (control == null)
				{
					return;
				}

				baseControl.Controls.Add(control);
				control.BringToFront();

				if (viewModel.IsPreview)
				{
					HookupControlForClickForPreview(control, viewModel);
				}
			}
		}

		static Control GetControl(ZUserControl baseControl, StaticControlCustomisation customisedControl, ControlCustomisationViewModel viewModel)
		{
			var control = GetControl(customisedControl, viewModel);

			if (control != null)
			{
				SetControlParameters(baseControl, control, customisedControl, viewModel);
			}

			return control;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static Control GetControl(StaticControlCustomisation customisedControl, ITaskCardComponentParent parent)
		{
			switch (customisedControl.ControlType)
			{
				case StaticControlTypeList.Codes.AttachedTagsIndicator:
					return new AttachedTagsIndicator(parent, customisedControl.OrientationValue);

				case StaticControlTypeList.Codes.Label:
					return new DirectionalLabel(isVertical: customisedControl.OrientationValue == BMBoardSectionOrientation.Vertical) { UseMnemonic = false };

				case StaticControlTypeList.Codes.CloseButton:
					return new CloseCardButton(parent);

				case StaticControlTypeList.Codes.OpenJobButton:
					return new OpenJobButton(parent);

				case StaticControlTypeList.Codes.SaveButton:
					return new SaveButton(parent);

				case StaticControlTypeList.Codes.CapabilityAssignmentButton:
					return new CapabilityAssignmentButton(parent);

				case StaticControlTypeList.Codes.NudgeControls:
					return new NudgeControls(parent);

				case StaticControlTypeList.Codes.StatusButtons:
					return new TaskStatusControl(parent, customisedControl.PlayButtonBehavior, customisedControl.SuspendButtonBehavior, customisedControl.CloseTaskButtonBehavior);

				case StaticControlTypeList.Codes.TaskStatusIndicator:
					return new StatusIndicatorControl(parent, customisedControl.OrientationValue);

				case StaticControlTypeList.Codes.DateAcceptabilityPicture:
					return new DateAcceptabilityPicture(parent);

				case StaticControlTypeList.Codes.WorkingStatusButton:
					return GenericStatusChangeButtonProvider.GetButtonForCode(StaticControlTypeList.Codes.WorkingStatusButton, parent, null, null, customisedControl.PlayButtonBehavior);

				case StaticControlTypeList.Codes.SuspendStatusButton:
					return GenericStatusChangeButtonProvider.GetButtonForCode(StaticControlTypeList.Codes.SuspendStatusButton, parent, null, null, customisedControl.SuspendButtonBehavior);

				case StaticControlTypeList.Codes.CompletedStatusButton:
					return GenericStatusChangeButtonProvider.GetButtonForCode(StaticControlTypeList.Codes.CompletedStatusButton, parent, null, null, customisedControl.CloseTaskButtonBehavior);

				default:
					return new ZLabel();
			}
		}

		#endregion

		#region Fields

		static void AddFields(ZUserControl baseControl, BMControlCustomisation customisation, ControlCustomisationViewModel viewModel)
		{
			foreach (var fieldLine in customisation.CustomisationLines.Cast<BMControlCustomisationLine>().ToArray().OrderBy(l => l.DisplaySequence))
			{
				var control = GetControlForField(baseControl, fieldLine, viewModel);
				if (control == null)
				{
					return;
				}

				baseControl.Controls.Add(control);
				control.BringToFront();

				if (viewModel.IsPreview)
				{
					HookupControlForClickForPreview(control, viewModel);
				}

				CheckControlCaption(baseControl, control);
			}
		}

		static void CheckControlCaption(ZUserControl baseControl, Control control)
		{
			var label = control as ZLabel;
			if (label != null)
			{
				SetLabelCaption(baseControl, label);
			}

			SetCaptionVisibility(control);
		}

		static void SetLabelCaption(ZUserControl baseControl, ZLabel label)
		{
			label.UseMnemonic = false;

			if (!string.IsNullOrEmpty(label.CaptionResourceString.Caption))
			{
				var captionLabel = new ZLabel
				{
					AutoSize = true,
					BackColor = Color.Transparent,
					Text = label.CaptionResourceString.Caption + ": ",
				};

				baseControl.Controls.Add(captionLabel);
				ControlDpiScalingHelper.SetLeft(ref captionLabel, label.Left - captionLabel.Width, false);
				ControlDpiScalingHelper.SetTop(ref captionLabel, label.Top + label.Height / 2 - captionLabel.Height / 2, false);
			}
		}

		static void SetCaptionVisibility(Control control)
		{
			var captionControl = control as IResCaptionedControl;

			if (captionControl != null && string.IsNullOrEmpty(captionControl.CaptionResourceString.Caption))
			{
				new LabelCaptionRenderProvider().SetLabelCaptionVisible(control, false);
			}
		}

		static bool IsCustomFieldProperty(BMControlCustomisationLine fieldLine)
		{
			var isDetailedControlType = fieldLine.Parent.FM_ControlType.In<ZString>(CustomisedControlTypeList.Codes.DetailedCard, CustomisedControlTypeList.Codes.WorkflowDetailedCard);
			return isDetailedControlType && Utilities.TrimExpression(fieldLine.PropertyName).StartsWith(MacroHelper.GetCustomFieldMacroName, StringComparison.CurrentCultureIgnoreCase);
		}

		static Control GetControlForField(ZUserControl baseControl, BMControlCustomisationLine fieldLine, ControlCustomisationViewModel viewModel)
		{
			var control = GetControlForField(fieldLine);

			if (control == null)
			{
				return null;
			}

			SetControlParameters(baseControl, control, fieldLine, viewModel);

			if (fieldLine.AutoSize && control is Label label)
			{
				label.AutoSize = true;
			}

			if (viewModel.IsPreview && fieldLine.PropertySource == PropertySourceList.Codes.Job)
			{
				SetControlFieldsForPreview(control, fieldLine);
			}
			else
			{
				var bindingPath = fieldLine.GetBindingPath(viewModel?.CardContent, viewModel.ViewModel != null && viewModel.ViewModel.ShowJobCards);

				if (!string.IsNullOrEmpty(bindingPath))
				{
					var isCustomField = IsCustomFieldProperty(fieldLine);
					if (isCustomField)
					{
						BindCustomField(viewModel, control, bindingPath);
					}
					else
					{
						baseControl.BindingSource.SetBindingMember(control, bindingPath);
					}
				}
			}

			return control;
		}

		static void BindCustomField(ControlCustomisationViewModel viewModel, Control control, string bindingPath)
		{
			var parent = viewModel?.Task?.Parent;
			if (parent != null)
			{
				var customBizo = ((ICustomFieldProvider)parent).GetCustomBusinessObject();
				if (customBizo != null)
				{
					if (control is ZCheckBox checkBox)
					{
						checkBox.DataBindings.Add(new KBinding("Checked", customBizo, bindingPath, true, DataSourceUpdateMode.OnPropertyChanged));
					}
					else if (control is ZDateEdit dateEdit)
					{
						dateEdit.DataBindings.Add(new KBinding("DateTimeValue", customBizo, bindingPath, true, DataSourceUpdateMode.OnPropertyChanged));
					}
					else
					{
						control.DataBindings.Add(new KBinding("Text", customBizo, bindingPath, true, DataSourceUpdateMode.OnPropertyChanged));
					}
				}
			}
		}

		static void SetControlFieldsForPreview(Control control, BMControlCustomisationLine fieldLine)
		{
			var propertyName = fieldLine.PropertyName;
			var index = propertyName.IndexOf('_');

			if (index > 0 && index < 3 && index < propertyName.Length - 1) // Only remove the table prefix - the property name may contain an underscore elsewhere.
			{
				propertyName = propertyName.Substring(index + 1);
			}

			if (control is ZCodeFindBox codeFindBox)
			{
				codeFindBox.CodeBox.Text = propertyName;
			}
			else if (fieldLine.IsCustomField && control is ZCheckBox checkBox)
			{
				checkBox.Text = fieldLine.Label;
			}
			else
			{
				control.Text = propertyName;
			}
		}

		static Control GetControlForField(BMControlCustomisationLine fieldLine)
		{
			var isReadOnly = fieldLine.IsReadOnly;
			switch (fieldLine.ControlType)
			{
				case PropertyTypeList.Codes.Text:
					return GetControlForTextType(fieldLine);

				case PropertyTypeList.Codes.Number:
					return new ZCalcEdit { ReadOnly = isReadOnly };

				case PropertyTypeList.Codes.Date:
					return new ZDateEdit { ReadOnly = isReadOnly, DateTimeFormat = ZDateTimePickerFormat.Short };

				case PropertyTypeList.Codes.DateTime:
					return new ZDateEdit { ReadOnly = isReadOnly, DateTimeFormat = ZDateTimePickerFormat.Long };

				case PropertyTypeList.Codes.Duration:
					return new ZTimeEditEx { ReadOnly = isReadOnly };

				case PropertyTypeList.Codes.Boolean:
					return new ZCheckBox { ReadOnly = isReadOnly };
			}

			return null;
		}

		static Control GetControlForTextType(BMControlCustomisationLine fieldLine)
		{
			var isReadOnly = fieldLine.IsReadOnly;
			var property = fieldLine.Property;

			if (!isReadOnly && property != null && Attribute.IsDefined(property, typeof(ListAttribute)))
			{
				if (Attribute.IsDefined(property, typeof(RelatedBusinessObjectAttribute)))
				{
					return new CustomisedLayoutCodeFindBox();
				}

				return new ZDropEdit
				{
					CharacterCasing = CharacterCasing.Normal,
					ReadOnly = false,
					ShowDescriptionBox = false,
					ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
					BindToForDescription = "CustomisedZDropEditControl"
				};
			}

			if (isReadOnly && (fieldLine.Label.IsEmpty || fieldLine.BackgroundColor == Color.Transparent.Name))
			{
				return new DirectionalLabel(isVertical: fieldLine.OrientationValue == BMBoardSectionOrientation.Vertical);
			}

			return new ZTextBox
			{
				CharacterCasing = CharacterCasing.Normal,
				ReadOnly = isReadOnly
			};
		}

		#endregion

		#region Shared

		static void HookupControlForClickForPreview(Control control, ControlCustomisationViewModel viewModel)
		{
			if (control is ZCodeFindBox codeFindBox)
			{
				codeFindBox.PopupButton.Enabled = false;
			}

			HookupControlForClick(control, viewModel);
		}

		static void HookupControlForClick(Control control, ControlCustomisationViewModel viewModel)
		{
			control.Click += (s, e) => viewModel.SelectControl(control, (ControlCustomisationBase)control.Tag);

			foreach (Control childControl in control.Controls)
			{
				childControl.Tag = control.Tag;
				HookupControlForClick(childControl, viewModel);
			}
		}

		static void SetControlParameters(ZUserControl baseControl, Control control, ControlCustomisationBase customisedControl, ControlCustomisationViewModel viewModel)
		{
			try
			{
				control.Tag = customisedControl;
				control.Location = ControlDpiScalingHelper.NewScaledPoint(customisedControl.Left, customisedControl.Top, true);

				SetControlSize(baseControl, control, customisedControl);
				SetControlColor(control, customisedControl, viewModel);
				SetControlFont(control, customisedControl, viewModel);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				control.Dispose();
				throw;
			}
		}

		static void SetControlSize(ZUserControl baseControl, Control control, ControlCustomisationBase customisedControl)
		{
			if (!customisedControl.ManagesOwnSize)
			{
				control.Size = ControlDpiScalingHelper.NewScaledSize(customisedControl.Width, customisedControl.Height, true);
			}
			else if (customisedControl.Alignment == ControlAlignmentList.Codes.Right &&
				control.Width > ControlDpiScalingHelper.ScaleToCurrentDpiX(customisedControl.Width) && customisedControl.Width > 0)
			{
				ControlDpiScalingHelper.SetLeft(control, customisedControl.Left, true);
			}

			if (control is ZDropEdit)
			{
				baseControl.Load += (s, e) => ((ZDropEditInternals)control).SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(customisedControl.Width));
			}
			else if (control is ZCodeFindBox codeFindBox)
			{
				codeFindBox.Load += (s, e) => codeFindBox.CodeBox.Size = ControlDpiScalingHelper.NewScaledSize(customisedControl.Width - codeFindBox.PopupButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), customisedControl.Height, true);
			}
		}

		static void SetControlColor(Control control, ControlCustomisationBase customisedControl, ControlCustomisationViewModel viewModel)
		{
			if (!customisedControl.BackgroundColor.IsEmpty)
			{
				var defaultColor = control.BackColor;
				try
				{
					control.BackColor = customisedControl.BackgroundColorValue;
				}
				catch (ArgumentException)
				{
					if (viewModel.IsPreview)
					{
						throw;
					}
					else
					{
						// This color was forbidden, so don't set it.
						control.BackColor = defaultColor;
					}
				}
			}
			if (!customisedControl.ForegroundColor.IsEmpty)
			{
				control.ForeColor = customisedControl.ForegroundColorValue;
			}
		}

		static bool IsFontInstalled(string font, ControlCustomisationViewModel viewModel)
		{
#if WINZOR
			return TextRenderer.IsFontSupportedInWinzor(font);
#else
			return viewModel.InstalledFonts.Families.Select(f => f.Name).Contains(font);
#endif
		}

		static void SetControlFont(Control control, ControlCustomisationBase customisedControl, ControlCustomisationViewModel viewModel)
		{
			var fontFamily = customisedControl.Font.IsEmpty || !IsFontInstalled(customisedControl.Font, viewModel) ? control.Font.FontFamily : customisedControl.FontFamily;
			control.Font = new Font(fontFamily, customisedControl.FontSize);

			if (customisedControl.IsBold)
			{
				control.Font = new Font(control.Font, FontStyle.Bold);
			}

			var resStringControl = control as IResCaptionedControl;
			if (resStringControl != null && !customisedControl.Label.IsEmpty)
			{
				resStringControl.CaptionResourceString = new ResourceStringData(string.Empty, customisedControl.Label);
			}

			if (customisedControl.Alignment == ControlAlignmentList.Codes.Right)
			{
				control.Anchor = AnchorStyles.Top | AnchorStyles.Right;

				switch (control)
				{
					case Label label:
						label.TextAlign = ContentAlignment.MiddleRight;
						break;
					case ZTextBox textBox:
						textBox.TextAlign = HorizontalAlignment.Right;
						break;
					case ZCodeFindBox codeFindBox:
						codeFindBox.CodeBox.TextAlign = HorizontalAlignment.Right;
						break;
				}
			}
		}

		#endregion
	}
}
