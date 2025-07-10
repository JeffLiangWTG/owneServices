using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI.DevTools
{
	internal sealed class FilterDevTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic Information")]
		public const string Name = "Examine Filter";

		#region IDevTool Members

		bool IDevTool.AddAsButton
		{
			get { return false; }
		}

		string IDevTool.Name
		{
			get { return Name; }
		}

		public void Show(Form form)
		{
			OperationalActionCustomizationForm customisationForm = form as OperationalActionCustomizationForm;
			OperationalAction action;

			if (customisationForm == null)
			{
				Globals.Message.Show((NoResString)"unable to find the form", Name, MessageBoxButtons.OK, DialogResult.OK);
			}
			else if ((action = customisationForm.SelectedAction) == null)
			{
				Globals.Message.Show((NoResString)"unable to find the selected action", Name, MessageBoxButtons.OK, DialogResult.OK);
			}
			else
			{
				TreeNode rootNode;

				try
				{
					IFilterExpression expression = FilterParser.Parse(action.SU_FilterList);
					rootNode = NodeFromExpression(expression, EnvironmentFilterProvider.Instance);
				}
				catch (ParseException ex)
				{
					rootNode = new TreeNode(ex.Message);
				}

				KForm dialog = CreateTreeForm(rootNode);
				dialog.Show();
			}
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1109:UseZForm", Justification = "Baseline")]
		static KForm CreateTreeForm(TreeNode rootNode)
		{
			TreeView view = new TreeView();
			view.Dock = DockStyle.Fill;
			view.ShowRootLines = false;
			view.Nodes.Add(rootNode);
			view.ExpandAll();
			view.DrawMode = TreeViewDrawMode.OwnerDrawText;
			#if !WINZOR
			view.DrawNode += new DrawTreeNodeEventHandler(view_DrawNode);
			#endif

			KButton close = new KButton();
			close.Text = (NoResString)"Close";
			close.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5);
			close.Anchor = AnchorStyles.Right | AnchorStyles.Top;

			KPanel bottom = new KPanel();
			bottom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(close.Right, close.Bottom);
			bottom.Dock = DockStyle.Bottom;
			bottom.Controls.Add(close);

			KForm dialog = new KForm();
			dialog.Text = Name;
			dialog.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 600);
			dialog.StartPosition = FormStartPosition.CenterScreen;
			dialog.CancelButton = close;
			dialog.Controls.Add(view);
			dialog.Controls.Add(bottom);

			close.Click += delegate(object sender, EventArgs e)
			{
				dialog.Close();
			};

			close.Focus();

			return dialog;
		}

		static TreeNode NodeFromExpression(IFilterExpression expression, IFilterValueProvider provider)
		{
			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}

			TreeNode node = new TreeNode();

			IFilterExpression[] subExpressions = GetCollapsedSubExpressions(expression);

			if (subExpressions.Length == 0)
			{
				node.Text = expression.ToString();
				node.ForeColor = (expression.Evaluate(provider) ? Color.Green : Color.Red);

				foreach (string constraintName in expression.GetConstraints())
				{
					IFilterConstraint constraint = provider.GetConstraint(constraintName);
					if (constraint != null)
					{
						node.Nodes.Add(new TreeNode(string.Format("{0}: {1}", constraintName, constraint.GetValue())));
					}
					else
					{
						node.Nodes.Add(new TreeNode(string.Format((NoResString)"'{0}' is not a supported constraint.", constraintName)));
					}
				}
			}
			else
			{
				node.Text = expression.Precidence.ToString();
				node.ForeColor = (expression.Evaluate(provider) ? Color.DarkGreen : Color.DarkRed);

				foreach (IFilterExpression sub in subExpressions)
				{
					node.Nodes.Add(NodeFromExpression(sub, provider));
				}
			}

			return node;
		}

		static IFilterExpression[] GetCollapsedSubExpressions(IFilterExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			List<IFilterExpression> result = new List<IFilterExpression>();
			Stack<IFilterExpression> expressions = new Stack<IFilterExpression>();
			expressions.Push(expression);

			while (expressions.Count > 0)
			{
				IFilterExpression thisExpression = expressions.Pop();

				if (thisExpression.Precidence != expression.Precidence)
				{
					result.Add(thisExpression);
				}
				else
				{
					IFilterExpression[] subExpressions = thisExpression.GetSubExpressions();

					for (int i = subExpressions.Length - 1; i >= 0; i--)
					{
						expressions.Push(subExpressions[i]);
					}
				}
			}

			return result.ToArray();
		}

		#if !WINZOR

		static void view_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			Graphics g = e.Graphics;
			Rectangle bounds = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(
				ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.Bounds.X),
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Bounds.Y),
				ControlDpiScalingHelper.UnscaleFromCurrentDpiX((int)Math.Ceiling(e.Bounds.Width * 1.1)),
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Bounds.Height));
			TreeNode node = e.Node;
			TreeView view = node.TreeView;
			Color foreColor = node.ForeColor;

			g.FillRectangle(SystemBrushes.Window, bounds);

			if (node.IsSelected)
			{
				g.DrawRectangle(SystemPens.Highlight, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);
			}

			if (foreColor.IsEmpty)
			{
				TextRendererHelper.DrawText(g, node.Text, view.Font, bounds, SystemBrushes.WindowText);
			}
			else
			{
				TextRendererHelper.DrawText(g, node.Text, view.Font, bounds, BrushProvider.FromColor(foreColor));
			}
		}

		#endif

		#endregion
	}
}
