using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Text;
using System.Windows.Forms;

namespace CargoWise.Windows.UI.Layout
{
	public static class RowLayoutDependentControlManager
	{
		public static void SetRowAndMoveDependentRows(RowLayout layoutEngine, Control control, int rowTarget, Control visibilityDependentControl)
		{
			int rowCurrent = layoutEngine.GetRow(control);
			if (rowCurrent != rowTarget)
			{
				layoutEngine.SetRow(control, rowTarget);

				var controlVisRelationshipProvider = visibilityDependentControl as IControlVisibilityRelationshipProviderSource;
				if (controlVisRelationshipProvider != null)
				{
					foreach (Control otherControl in layoutEngine.Container.Controls)
					{
						if (controlVisRelationshipProvider.VisibilityRelationshipProvider.GetSourceControl(otherControl) == control)
						{
							int rowOther = layoutEngine.GetRow(otherControl);
							try
							{
								SetRowAndMoveDependentRows(layoutEngine, otherControl, rowTarget + (rowOther - rowCurrent), visibilityDependentControl);
							}
							catch (ArgumentOutOfRangeException ex)
							{
								throw new SetRowAndMoveDependentRowsException(control, otherControl, visibilityDependentControl, rowCurrent, rowTarget, rowOther, layoutEngine.Rows, layoutEngine.IsHighRiskRowSetTo0CallStack, ex);
							}
						}
					}
				}
			}

			layoutEngine.DoLayout();
		}

		public static void PrepareSpaceForDependentRows(RowLayout layoutEngine, Control visibilityDependentControl, IDictionary<Control, int> orderList)
		{
			IControlVisibilityRelationshipProviderSource controlVisRelationshipProvider = visibilityDependentControl as IControlVisibilityRelationshipProviderSource;
			if (controlVisRelationshipProvider != null)
			{
				Dictionary<int, int> dependees = new Dictionary<int, int>();
				List<int> allRows = new List<int>();
				int maxRow = -1;

				foreach (Control otherControl in layoutEngine.Container.Controls)
				{
					Control parentControl = controlVisRelationshipProvider.VisibilityRelationshipProvider.GetSourceControl(otherControl);
					int row;

					if (parentControl == null && orderList.TryGetValue(otherControl, out row))
					{
						if (!allRows.Contains(row))
						{
							allRows.Add(row);
						}
					}
					else if (parentControl != null && orderList.TryGetValue(parentControl, out row))
					{
						if (orderList.ContainsKey(otherControl))
						{
							orderList.Remove(otherControl);
						}

						if (row > maxRow)
						{
							maxRow = row;
						}

						if (!dependees.ContainsKey(row))
						{
							dependees[row] = 0;
						}
						int shift = layoutEngine.GetRow(otherControl) - layoutEngine.GetRow(parentControl);
						if (dependees[row] < shift)
						{
							dependees[row] = shift;
						}
					}
				}

				for (int i = maxRow; i >= 0; i--)
				{
					if (dependees.ContainsKey(i) && dependees[i] > 0)
					{
						for (int j = 0; j < dependees[i]; j++)
						{
							if (allRows.Contains(i + j + 1))
							{
								foreach (var key in orderList.Keys.ToArray())
								{
									if (orderList[key] > i)
									{
										orderList[key]++;
									}
								}

								allRows.Remove(i + j + 1);
								if (!allRows.Contains(i + j + 2))
								{
									allRows.Add(i + j + 2);
								}
							}
						}
					}
				}
			}
		}

		[Serializable]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message")]
		public class SetRowAndMoveDependentRowsException : Exception
		{
			internal SetRowAndMoveDependentRowsException(Control control, Control otherControl, Control visibilityDependentControl, int rowCurrent, int rowTarget, int rowOther, RowLayoutRowCollection rows, StringBuilder isHighRiskRowSetTo0CallStack, Exception innerException)
			: base("Could not set row and move dependent rows", innerException)
			{
				this.Control = control;
				this.OtherControl = otherControl;
				this.VisibilityDependentControl = visibilityDependentControl;
				this.RowCurrent = rowCurrent;
				this.RowTarget = rowTarget;
				this.RowOther = rowOther;

				this.IsHighRiskRowSetTo0CallStack = isHighRiskRowSetTo0CallStack.ToString();
				var controlDescription = (Control control, int index) => $"\tIndex: {index} Type: {control.GetType().Name} Name: {control.Name}";
				var controlDescriptions = (ReadOnlyCollection<Control> controls) => string.Join("\r\n", controls.Select((control, controlIndex) => controlDescription(control, controlIndex)));
				this.RowDescriptions = string.Join("\r\n", rows.Select((row, index) => $"Row {index} Visible: {row.Visible} Controls:\r\n{controlDescriptions(row.Controls)}"));
			}

#if NETFRAMEWORK
			protected SetRowAndMoveDependentRowsException(SerializationInfo info, StreamingContext context) : base(info, context)
			{
			}
#endif

			public Control Control { get; private set; }
			public Control OtherControl { get; private set; }
			public Control VisibilityDependentControl { get; private set; }
			public int RowCurrent { get; private set; }
			public int RowTarget { get; private set; }
			public int RowOther { get; private set; }
			public string RowDescriptions { get; private set; }
			public string IsHighRiskRowSetTo0CallStack { get; set; }
		}
	}
}
