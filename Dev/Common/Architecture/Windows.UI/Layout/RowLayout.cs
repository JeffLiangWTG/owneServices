using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Windows.Forms.VisualStyles;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI.Layout
{
	public interface RowLayoutInternal
	{
		void DoLayout();
	}

	/// <summary>
	/// A layout engine for controls that are organized into fixed height rows.
	/// </summary>
	public class RowLayout : LayoutEngine, RowLayoutInternal
	{
		public RowLayout(RowLayoutPanel container, LayoutEngine baseLayoutEngine)
		{
			this.Container = container;
			this.Container.ControlAdded += new ControlEventHandler(Container_ControlAdded);
			this.Container.VisibleChanged += Container_VisibleChanged;
			this.baseLayoutEngine = baseLayoutEngine;
			this.AutoTabOrder = true;
		}

		/// <summary>
		/// The default value of the RowHeight property.
		/// </summary>
		/// 
		[DpiState(DpiState.Unscaled)]
		public const int DefaultRowHeight = 23;

		/// <summary>
		/// Get the hosting Control.
		/// </summary>
		public RowLayoutPanel Container { get; private set; }

		/// <summary>
		/// Get or set the fixed height for each row.
		/// </summary>
		[Category(DesignerConstants.Category)]
		[Description("Get or set the fixed height for each row.")]
		[DpiState(DpiState.ScaleY)]
		public int RowHeight
		{
			get { return rowHeight; }
			set
			{
				if (rowHeight != value)
				{
					Argument.GreaterThanZero(value, "rowHeight");
					rowHeight = value;
					DoLayout();
				}
			}
		}
		int rowHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(DefaultRowHeight);

		/// <summary>
		/// The default value of the Alignment property.
		/// </summary>
		public const VerticalAlignment DefaultAlignment = VerticalAlignment.Top;

		[Category(DesignerConstants.Category)]
		[Description("Vertical alignment of controls within each row.")]
		[DefaultValue(RowLayout.DefaultAlignment)]
		public VerticalAlignment Alignment
		{
			get { return alignment; }
			set
			{
				if (alignment != value)
				{
					alignment = value;
					DoLayout();
				}
			}
		}
		VerticalAlignment alignment = DefaultAlignment;

		[Category(DesignerConstants.Category)]
		[Description("Get or set whether to automatically update the tab order at design time.")]
		[DefaultValue(true)]
		public bool AutoTabOrder { get; set; }

		#region DoLayout

		bool IsLayoutSuspended
		{
			get { return (bool)IsLayoutSuspendedProperty.GetValue(Container, null); }
		}

		static PropertyInfo IsLayoutSuspendedProperty
		{
			get { return isLayoutSuspendedProperty ?? (isLayoutSuspendedProperty = typeof(Control).GetProperty("IsLayoutSuspended", BindingFlags.NonPublic | BindingFlags.Instance)); }
		}
		[ThreadSafe]
		static PropertyInfo isLayoutSuspendedProperty;

		void RowLayoutInternal.DoLayout()
		{
			DoLayout();
		}

		internal void DoLayout()
		{
			if (IsLayoutSuspended)
			{
				Container.Layout -= new LayoutEventHandler(Container_Layout);
				Container.Layout += new LayoutEventHandler(Container_Layout);
			}
			else
			{
				DoLayoutCore();
			}
		}

		void Container_Layout(object sender, LayoutEventArgs e)
		{
			Container.Layout -= new LayoutEventHandler(Container_Layout);
			DoLayoutCore();
		}

		void Container_VisibleChanged(object sender, EventArgs e)
		{
			if (pendingLayout)
			{
				DoLayoutCore();
				pendingLayout = false;
			}
		}

		void DoLayoutCore()
		{
			if (Container.Visible)
			{
				Container.SuspendLayout();
				try
				{
					int containerHeight = 0;
					for (int i = 0; i < Rows.Count; i++)
					{
						RowLayoutRow row = Rows[i];
						foreach (Control control in new ArrayList(row.Controls))
						{
							UpdateControlTop(control, i);
							containerHeight = Math.Max(containerHeight, control.Bottom);
						}
					}
					if (!IsDesigning && Container.AutoSize && containerHeight != Container.Height)
					{
						ControlDpiScalingHelper.SetHeight(Container, containerHeight, false);
					}

					UpdateTabOrder();
				}
				finally
				{
					Container.ResumeLayout();
				}
			}
			else
			{
				pendingLayout = true;
			}
		}

		void UpdateControlTop(Control control, int row)
		{
			int newTop = GetControlTop(row, control);
			if (newTop != control.Top)
			{
				ControlDpiScalingHelper.SetTop(ref control, newTop, false);
			}
		}

		void UpdateTabOrder()
		{
			int tabIndex = 1;
			foreach (RowLayoutRow row in Rows)
			{
				List<Control> controls = new List<Control>(row.Controls);
				controls.Sort((lhs, rhs) => lhs.Left - rhs.Left);
				foreach (Control control in controls)
				{
					control.TabIndex = tabIndex++;
				}
			}
		}

		#endregion

		#region GetRow / SetRow

		/// <summary>
		/// Get the row a control belongs to.
		/// </summary>
		public int GetRow(Control control)
		{
			return Rows.GetRowNumberFromControl(control);
		}

		[Category(DesignerConstants.Category)]
		[Description("Get or set whether the row of a control is fixed or is automatically updated if its layout changes. Set this to true if you call SetRow yourself and the row should not subsequently change.")]
		[DefaultValue(false)]
		public bool FixedRows { get; set; }

		/// <summary>
		/// Set the row a control belongs to.
		/// </summary>
		public void SetRow(Control control, int row)
		{
			if (control != null && control.Name == "IsHighRisk" && row == 0)
			{
				//Remove this after issue fixed
				IsHighRiskRowSetTo0CallStack.AppendLine("control.Top:" + control.Top);
				IsHighRiskRowSetTo0CallStack.AppendLine("control.Text:" + control.Text);
				IsHighRiskRowSetTo0CallStack.AppendLine(Environment.StackTrace);
			}
			Argument.GreaterThanOrEqual(row, -1, (NoResString)"row of control:" + control?.Name);
			int currentRow = Rows.GetRowNumberFromControl(control);
			if (currentRow != -1 && currentRow != row)
			{
				Rows[currentRow].RemoveControl(control);
			}
			if (row != -1)
			{
				if (currentRow != row)
				{
					Rows[row].AddControl(control);
				}
				UpdateControlTop(control, row);
			}
		}

		public readonly StringBuilder IsHighRiskRowSetTo0CallStack = new ();

		#endregion

		#region Visible Row Count

		public int VisibleRowCount
		{
			get
			{
				int result = 0;
				foreach (RowLayoutRow row in Rows)
				{
					if (row.Visible)
					{
						result++;
					}
				}

				return result;
			}
		}

		#endregion

		#region LayoutEngine Overrides

		public override void InitLayout(object child, BoundsSpecified specified)
		{
			baseLayoutEngine.InitLayout(child, specified);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
		{
			bool result = false;
			try
			{
				result = baseLayoutEngine.Layout(container, layoutEventArgs);
			}
			catch (NullReferenceException)
			{
				// happens in the designer only - in DefaultLayout.GetAnchorDestination for some reason
				if (!IsDesigning)
				{
					throw;
				}
			}
			if (layoutEventArgs.AffectedControl != null && (IsDesigning || layoutEventArgs.AffectedProperty != "Visible") && layoutEventArgs.AffectedControl.Parent != null && layoutEventArgs.AffectedControl.Parent == container)
			{
				if (SelectionService != null && SelectionService.GetSelectedComponents().Count > 1)
				{
					foreach (Control affectedControl in SelectionService.GetSelectedComponents())
					{
						Layout(affectedControl);
					}
				}
				else
				{
					Layout(layoutEventArgs.AffectedControl);
				}

				if (AutoTabOrder)
				{
					UpdateTabOrder();
				}
			}
			return result;
		}

		void Layout(Control affectedControl)
		{
			if (affectedControl.Visible)
			{
				int previousRowNumber = Rows.GetRowNumberFromControl(affectedControl);
				int newRowNumber;

				if (!FixedRows)
				{
					int topDiff = (affectedControl.Top + RowHeight / 2) % RowHeight - RowHeight / 2;
					if (-topDiff == RowHeight)
					{
						topDiff = 0;
					}
					ControlDpiScalingHelper.SetTop(affectedControl, affectedControl.Top - topDiff, false);

					int visibleRow = affectedControl.Top / RowHeight;
					newRowNumber = Rows.GetRowNumber(visibleRow);
				}
				else
				{
					newRowNumber = previousRowNumber;
					if (newRowNumber == -1)
					{
						newRowNumber = Math.Max(affectedControl.Top / RowHeight, 0);
					}
				}

				SetRow(affectedControl, newRowNumber);

				if (IsDesigning)
				{
					EnsureHasLeftMostControlInTransaction(previousRowNumber, newRowNumber);
				}
			}
		}

		ISelectionService SelectionService
		{
			get { return Container.Site == null ? null : (ISelectionService)Container.Site.GetService(typeof(ISelectionService)); }
		}

		void EnsureHasLeftMostControlInTransaction(int previousRowNumber, int newRowNumber)
		{
			IDesignerHost designerHost = this.DesignerHost;
			if (designerHost != null && designerHost.InTransaction)
			{
				ValueHolder<DesignerTransactionCloseEventHandler> handler = new ValueHolder<DesignerTransactionCloseEventHandler>();
				handler.Value = delegate
				{
					designerHost.TransactionClosing -= handler.Value;
					EnsureHasLeftMostControl(previousRowNumber, newRowNumber);
				};
				designerHost.TransactionClosing += handler.Value;
			}
			else
			{
				EnsureHasLeftMostControl(previousRowNumber, newRowNumber);
			}
		}

		class ValueHolder<T>
		{
			public T Value;
		}

		void EnsureHasLeftMostControl(int previousRowNumber, int newRowNumber)
		{
			if (previousRowNumber != -1)
			{
				Rows[previousRowNumber].EnsureHasLeftMostControl();
			}
			if (newRowNumber != -1)
			{
				Rows[newRowNumber].EnsureHasLeftMostControl();
			}
		}

		IDesignerHost DesignerHost
		{
			get { return (Container == null || Container.Site == null) ? null : (IDesignerHost)Container.Site.GetService(typeof(IDesignerHost)); }
		}

		#endregion

		#region Implementation

		readonly LayoutEngine baseLayoutEngine;

		bool IsDesigning
		{
			get { return Container != null && Container.Site != null && Container.Site.DesignMode; }
		}

		internal RowLayoutRowCollection Rows
		{
			get { return rows ?? (rows = new RowLayoutRowCollection(this)); }
		}
		RowLayoutRowCollection rows;

		int GetControlTop(int row, Control control)
		{
			int result = Rows.GetVisibleRowNumber(row) * RowHeight;

			if (alignment == VerticalAlignment.Top)
			{
				if ((control is Label || control is CheckBox) && control.AutoSize)
				{
					result += ((RowHeight - control.Height) / 2);
				}
			}
			else if (alignment == VerticalAlignment.Center)
			{
				result += ((RowHeight - control.Height) / 2);
			}
			else
			{
				result += RowHeight - control.Height;
			}

			result -= Container.VerticalScroll.Value;

			return result;
		}

		void Container_ControlAdded(object sender, ControlEventArgs e)
		{
			SetRow(e.Control, e.Control.Top / RowHeight);

			if (!Container.Visible)
			{
				pendingLayout = true;
			}
		}

		bool pendingLayout;

		#endregion
	}
}
