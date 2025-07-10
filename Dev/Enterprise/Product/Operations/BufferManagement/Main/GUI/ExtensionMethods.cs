using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public static class ExtensionMethods
	{
		#region TableLayoutPanel

		public static int GetColumn(this TableLayoutPanel table, int leftOffset)
		{
			return GetPosition(leftOffset, table.GetColumnWidths());
		}

		public static int GetRow(this TableLayoutPanel table, int topOffset)
		{
			return GetPosition(topOffset, table.GetRowHeights());
		}

		static int GetPosition(int offset, int[] tableSegments)
		{
			var runningTotal = 0;

			for (int i = 0; i < tableSegments.Length; i++)
			{
				runningTotal += tableSegments[i];
				if (runningTotal >= offset)
				{
					return i;
				}
			}

			return -1;
		}

		#endregion

		#region Control

		// C# renders controls on a bitmap in the reverse order.
		// https://stackoverflow.com/questions/10096195/drawtobitmap-not-taking-screenshots-of-all-items
		public static void DrawToBitmapFixed(this Control control, Bitmap imageBuffer, Rectangle rectangle)
		{
#if !WINZOR
			control.ReverseControlOrder();
			control.DrawToBitmap(imageBuffer, rectangle);
			control.ReverseControlOrder();
#endif
		}

		public static void ReverseControlOrder(this Control control)
		{
			foreach (var innerControl in control.Controls.Cast<Control>().Reverse().ToArray())
			{
				innerControl.SendToBack();
				innerControl.ReverseControlOrder();
			}
		}

		#endregion

		#region MenuItemDescriptor

		public static void ExecuteAndMaybeShowFormForPayload<T>(this MenuItemDescriptor<T> menuItem, object sender, MenuItemClickHandlerEventArgs e)
			where T : BusinessObject
		{
			if (menuItem.ClickHandler != null)
			{
				menuItem.ClickHandler(sender, e);
			}

			if (menuItem.Payload != null && menuItem.ShowFormForPayloadAfterExecute)
			{
				var controller = ZControllerFactory.Create(menuItem.ControllerID);
				controller.ShowChildrenAsDialog = e.ShowPayloadFormModally && !Globals.IsTest;

				if (menuItem.Payload.IsInDatabase)
				{
					controller.ShowEditForm(menuItem.Payload);
				}
				else
				{
					controller.ShowFormForNewEntity(menuItem.Payload);
				}
			}
		}

		#endregion

		#region Tab Pages

		public static ZBindingTabPage GetTabPage(this TabSpec tabSpec)
		{
			var tabPage = tabSpec.TabContentControl as ZBindingTabPage;
			if (tabPage != null)
			{
				return tabPage;
			}

			var control = tabSpec.TabContentControl as Control;
			if (control != null)
			{
				tabPage = tabSpec.IsAlwaysEnabled ? new AlwaysEnabledBindingTabPage() : new BindingTabPageImplementation();
				tabPage.AutoScroll = false;
				tabPage.BackColor = Color.Transparent;
				tabPage.Text = tabSpec.Text;
				tabPage.Name = tabSpec.Name;
				tabPage.Padding = ControlDpiScalingHelper.NewScaledPadding(3);
				tabPage.UseVisualStyleBackColor = true;
				control.Dock = DockStyle.Fill;
				tabPage.Controls.Add(control);

				return tabPage;
			}

			return null;
		}

		public class BindingTabPageImplementation : ZBindingTabPage
		{
		}

		public class AlwaysEnabledBindingTabPage : BindingTabPageImplementation
		{
			[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "Empty setter needed when this property is set via reflection.")]
			public bool ReadOnly
			{
				get { return false; }
				set { }
			}

			protected override void OnEnabledChanged(EventArgs e)
			{
				base.OnEnabledChanged(e);

				Enabled = true;
			}
		}

		#endregion
	}
}
