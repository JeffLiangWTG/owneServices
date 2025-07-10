using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class CustomPropertiesControl : ZUserControl
	{
		public CustomPropertiesControl()
		{
			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(nothingSetupMessageLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		public string NothingSetupMessageLabelText
		{
			get { return nothingSetupMessageLabel.Text; }
			set { nothingSetupMessageLabel.Text = value; }
		}

		protected override bool ShouldRegisterToBeBoundOnPreSaveValidation
		{
			get { return true; }
		}

#if !WINZOR

		int drawingSuspendCount;
		void SuspendDrawing()
		{
			if (this.IsDisposed || this.Disposing || !this.IsHandleCreated)
			{ return; }
			if (drawingSuspendCount == 0)
			{
				CargoWise.Interop.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), WM_SETREDRAW, DISABLE_DRAWING, 0);
			}
			drawingSuspendCount++;
		}

		void ResumeDrawing()
		{
			if (this.IsDisposed || this.Disposing || !this.IsHandleCreated)
			{ return; }
			--drawingSuspendCount;
			if (drawingSuspendCount == 0)
			{
				CargoWise.Interop.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), WM_SETREDRAW, ENABLE_DRAWING, 0);
				Invalidate(true);
			}
		}

		const int DISABLE_DRAWING = 0;
		const int ENABLE_DRAWING = 1;
		const int WM_SETREDRAW = 0xB;

#endif

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// When data binding a record of a grid collection:
			// 1. Remove all controls incase the binding details have changed
			// 2. Bind this with the currently selected record
			// 3. Add controls linked to the currently selected record
			// In that order for the bindings to be updated successfully (i.e. no KDataBinding exceptions)	

			try
			{
#if !WINZOR
				rowLayoutPanel.SuspendDrawing();
				SuspendDrawing();
#endif
				rowLayoutPanel.SuspendLayout();
				SuspendLayout();

				RemoveControls();

				bool isNothing = true;
				IDynamicBusinessObject obj = dataSource as IDynamicBusinessObject;
				if (obj != null)
				{
					string[] usedProperties = obj.GetOrderedCustomProperties();
					if (usedProperties.Length > 0)
					{
						isNothing = false;
						nothingSetupMessageLabel.Visible = false;
						rowLayoutPanel.Visible = true;
						base.SetDataBinding(obj, "");
						AddControls(obj, usedProperties);
					}
				}
				if (isNothing)
				{
					rowLayoutPanel.Visible = false;
					nothingSetupMessageLabel.Visible = true;
					base.SetDataBinding(obj, "");
				}
			}
			finally
			{
				rowLayoutPanel.ResumeLayout(true);
				ResumeLayout(true);
#if !WINZOR
				rowLayoutPanel.ResumeDrawing();
				ResumeDrawing();
#endif
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1091:Do Not Set CausesValidation to false", Justification = "the disposed control's validation causes unexcepted things, can't find other better solution to solve it. So add this statement.")]
		void RemoveControls()
		{
			var form = ParentForm as ZForm;
			if (form?.IsDisposing ?? false)
			{
				return;
			}

			var controls = rowLayoutPanel.Controls.Cast<Control>().ToArray();
			for (int i = controls.Length - 1; i >= 0; i--)
			{
				// removing from the bottom is much quicker as we don't have to move everything else up in the grid
				var control = controls[i];
				control.CausesValidation = false;   // the disposed control's validation causes unexcepted things, can't find other better solution to solve it. So add this statement.
				control.Dispose();
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);

			ResizeTextboxes();
		}

		void AddControls(IDynamicBusinessObject obj, string[] usedProperties)
		{
			int row = 0;
			foreach (var properties in GroupPropertiesByParts(usedProperties))
			{
				var control = properties.Count > 1 ?
					GetCombinationControl(obj, properties) :
					GetControl(obj, properties.FirstOrDefault());

				if (control != null)
				{
					rowLayoutPanel.Controls.Add(control);
					rowLayoutPanel.SetRow(control, row++);
				}
			}

			var labeledControls = from ctrl in rowLayoutPanel.Controls.OfType<Control>() where !(ctrl is CheckBox) select ctrl;
			float maxCaption;
			using (Graphics gr = Graphics.FromHwnd(Handle))
			{
				maxCaption = (
					from ctrl in labeledControls
					let render = ctrl.GetExtension<LabelCaptionRenderer>()
					select gr.MeasureString(render.Caption + render.LabelSeparator, render.Font).Width).DefaultIfEmpty().Max();
			}

			foreach (Control ctrl in rowLayoutPanel.Controls)
			{
				ControlDpiScalingHelper.SetLeft(ctrl, (int)Math.Ceiling(maxCaption) + ControlDpiScalingHelper.ScaleToCurrentDpiX(ControlClearanceOnLeft), false);
			}

			ResizeTextboxes();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic Constant")]
		IEnumerable<ICollection<string>> GroupPropertiesByParts(string[] usedProperties)
		{
			var usedPropertiesList = new List<string>(usedProperties);
			var propertiesGrouped = new List<ICollection<string>>(usedProperties.Length);

			//PRECONDITION: PART1 must always appear before PART2, PART3, etc. in the list, so that they don't get erroneously added on their lonesome.
			for (var i = 0; i < usedPropertiesList.Count; ++i)
			{
				var propertyName = usedPropertiesList[i];
				if (propertyName.EndsWith("Info", StringComparison.Ordinal))
				{ continue; }

				if (propertyName.Contains(AddOnColumnDataType.PartIdentifier + "1"))
				{
					var parts = new List<string>();
					do
					{
						parts.Add(propertyName);
						propertyName = propertyName.Replace(AddOnColumnDataType.PartIdentifier + parts.Count,
							AddOnColumnDataType.PartIdentifier + (parts.Count + 1));
					} while (usedPropertiesList.Remove(propertyName));

					yield return parts;
				}
				else
				{
					yield return new[] { propertyName };
				}
			}
		}

		internal const int ControlClearanceOnLeft = 7;

		void ResizeTextboxes()
		{
			rowLayoutPanel.SuspendLayout();

			foreach (Control control in rowLayoutPanel.Controls)
			{
				if (control.GetType() == typeof(ZTextBox) || (control is HorizontalStackControl))
				{
					ControlDpiScalingHelper.SetWidth(control, rowLayoutPanel.Width - control.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(ControlClearanceOnLeft) * 3, false);
				}
			}

			rowLayoutPanel.ResumeLayout(false);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			((CargoWise.Windows.UI.Layout.RowLayoutInternal)rowLayoutPanel.LayoutEngine).DoLayout();
		}

		class HorizontalStackControl : ZUserControl, IExtendedControl
		{
			public HorizontalStackControl() : base()
			{
				Extensions = new DefaultControlExtensionCollection(this);
			}

			Control IExtendedControl.Host => this;
			public IControlExtensionCollection Extensions { get; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
			protected override void Dispose(bool disposing)
			{
				Extensions.Dispose();
				base.Dispose(disposing);
			}

			protected override void OnLayout(LayoutEventArgs e)
			{
				base.OnLayout(e);

				if (Controls.Count > 0)
				{
					//Align controls so that each one starts just after the previous one's right (plus clearance pixels)
					var nextLeft = 0;
					foreach (Control ctrl in Controls)
					{
						ControlDpiScalingHelper.SetLeft(ctrl, nextLeft, false);
						nextLeft = ctrl.Right;
					}

					var lastControl = Controls[Controls.Count - 1];
					ControlDpiScalingHelper.SetWidth(lastControl, Width - lastControl.Left, false);

					var heightScaled = this.Controls.Cast<Control>().Select(p => p.Height).Max();
					var dummy = this;
					ControlDpiScalingHelper.SetHeight(ref dummy, heightScaled, false);
				}
			}
		}

		class CustomFieldsNotSetupLabel : ZLabel
		{
		}

		Control GetCombinationControl(IDynamicBusinessObject obj, IEnumerable<string> propertyNames)
		{
			var controls = propertyNames.Select(name => GetControl(obj, name, isPart: true)).WhereNotNull().ToArray();
			var heightScaled = controls.Select(p => p.Height).Max();

			var parentControl = new HorizontalStackControl();
			ControlDpiScalingHelper.SetWidth(ref parentControl, rowLayoutPanel.Width, false);
			ControlDpiScalingHelper.SetHeight(ref parentControl, heightScaled, false);

			parentControl.Controls.AddRange(controls);

			var renderer = parentControl.GetExtension<LabelCaptionRenderer>();
			var propertyName1 = propertyNames.FirstOrDefault();
			var property = obj.GetProperty(propertyName1);
			renderer.Caption = (property.GetCaption() ?? propertyName1).Replace(AddOnColumnDataType.PartIdentifier + "1", "");

			return parentControl;
		}

		Control GetControl(IDynamicBusinessObject obj, string propertyName, bool isPart = false)
		{
			DynamicBusinessObjectProperty property = obj.GetProperty(propertyName);
			Type propertyType = property.Type;

			Control control;
			if (IsType(propertyType, typeof(ZByte)) ||
				IsType(propertyType, typeof(ZShort)) ||
				IsType(propertyType, typeof(ZInt)) ||
				IsType(propertyType, typeof(ZDecimal)))
			{
				ZCalcEdit calcEdit = new ZCalcEdit();
				if (!IsType(propertyType, typeof(ZDecimal)))
				{
					calcEdit.Decimals = 0;
				}
				else
				{
					calcEdit.Decimals = DynamicBusinessObjectProperty.DefaultDecimalScale;
				}
				control = calcEdit;
			}
			else if (IsType(propertyType, typeof(ZString)))
			{
				if (property.GetMetaData(MetaDataTypes.ListDataSource) != null)
				{
					control = new ZDropEdit();
					//First textbox of a combo box custom field type doesn't have the description.
					if (propertyName.Contains(AddOnColumnDataType.PartIdentifier + "1") // Programmatic Constant
						&& (string)property.GetMetaData(MetaDataTypes.ParentCustomFieldType)?.Value == AddOnColumnDataType.Codes.ComboBox)
					{
						((ZDropEdit)control).ShowDescriptionBox = false;
					}
				}
				else
				{
					ZTextBox textBox = new ZTextBox();
					int maxLength = GetMaxLength(property);
					textBox.CharacterCasing = CharacterCasing.Normal;

					if (maxLength >= 0 && maxLength < 10000)
					{
						textBox.MaximumSize = ControlDpiScalingHelper.NewScaledSize(TextRenderer.MeasureText(new String('Q', maxLength), Font).Width, 0, false);
					}
					control = textBox;
				}
			}
			else if (IsType(propertyType, typeof(ZDateTime)))
			{
				ZDateEdit dateEdit = new ZDateEdit();
				DynamicMetaData metaData = property.GetMetaData(MetaDataTypes.DateTimeFormat);
				if (metaData != null)
				{
					switch ((KDateTimeFormat)metaData.Value)
					{
						case KDateTimeFormat.Long:
							dateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
							break;

						case KDateTimeFormat.Short:
							dateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
							break;

						case KDateTimeFormat.Time:
							dateEdit.DateTimeFormat = ZDateTimePickerFormat.Time;
							break;
					}
				}
				control = dateEdit;
			}
			else if (IsType(propertyType, typeof(ZBool)))
			{
				int captionLength = 0;

				ZCheckBox checkBox = new ZCheckBox();

				using (Graphics gr = Graphics.FromHwnd(Handle))
				{
					var caption = property.GetCaption() ?? propertyName;
					var render = checkBox.GetExtension<LabelCaptionRenderer>();
					captionLength = (int)Math.Ceiling(gr.MeasureString(caption + render.LabelSeparator, render.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(32));
				}

				checkBox.Size = ControlDpiScalingHelper.NewScaledSize(captionLength, rowLayoutPanel.RowHeight, false);
				ControlDpiScalingHelper.SetHeight(ref checkBox, rowLayoutPanel.RowHeight, false);
				control = checkBox;
			}
			else if (IsType(propertyType, typeof(ZPropertyInfo)))
			{
				return null;
			}
			else
			{
				throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Cannot determine control type for {0} type.", propertyType.Name));
			}

			control.SetBindingMember(propertyName);

			if (!isPart)
			{
				var renderer = control.GetExtension<LabelCaptionRenderer>();
				renderer.Caption = property.GetCaption() ?? propertyName;
			}
			return control;
		}

		static int GetMaxLength(DynamicBusinessObjectProperty property)
		{
			DynamicMetaData metadata = property.GetMetaData(MetaDataTypes.MaxLength);
			if (metadata != null)
			{
				return (int)metadata.Value;
			}

			return -1;
		}

		static bool IsType(Type type, Type parentType)
		{
			return parentType == type || type.IsSubclassOf(parentType);
		}
	}
}
