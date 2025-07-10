using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AppDomainWrappers.Net;
using CargoWise.Windows.UI.Design;
using CargoWise.Windows.UI.Layout;
using static System.Windows.Forms.Orientation;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(KSplitContainerSerializer), typeof(CodeDomSerializer))]
	public partial class KSplitContainer : SplitContainer, IChooseHowMyPropertiesScale, ISplitterLayoutSaveProvider
	{
		public KSplitContainer()
			: base()
		{
			SplitterMoving += KSplitContainer_SplitterMoving;
			SplitterMoved += KSplitContainer_SplitterMoved;
			SplitterWidth = base.SplitterWidth;

#if DEBUG
			// These attributes need to be excepted from the DPI awareness basher, since they are inherited from SplitContainer.
			var appDomainWrapper = new AppDomainWrapper();
			var architectureAssembly = appDomainWrapper.GetAssemblies().First(assembly => assembly.FullName.StartsWith("Enterprise.ZArchitecture.GUI,"));
			var suppressAttributeType = architectureAssembly.GetType("Enterprise.ZArchitecture.GUI.SuppressDpiAwareBasherAttribute");
			var suppressAttribute = (Attribute)Activator.CreateInstance(suppressAttributeType);
			TypeDescriptor.AddAttributes(Panel1, new[] { suppressAttribute });
			TypeDescriptor.AddAttributes(Panel2, new[] { suppressAttribute });
#endif // DEBUG

		}

		public new int SplitterWidth
		{
			get => base.SplitterWidth;
			set => base.SplitterWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(value);
		}

#if !WINZOR
		Color originalBackColor;
		Color originalPanel1BackColor;
		Color originalPanel2BackColor;

		protected override void OnMouseEnter(EventArgs e)
		{
			SetNewBackColor();

			base.OnMouseEnter(e);
		}
		protected override void WndProc(ref Message msg)
		{
			const int WM_SETCURSOR = 0x20;

			base.WndProc(ref msg);
			// Change the cursor set by WM_SETCURSOR
			if (msg.Msg == WM_SETCURSOR)
			{
				if (Cursor.Current == Cursors.HSplit)
				{
					Cursor.Current = Cursors.SizeNS;
				}
				else if (Cursor.Current == Cursors.VSplit)
				{
					Cursor.Current = Cursors.SizeWE;
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			RevertOriginalBackColor();

			base.OnMouseLeave(e);
		}

		void SetNewBackColor()
		{
			originalBackColor = BackColor;
			originalPanel1BackColor = Panel1.BackColor;
			originalPanel2BackColor = Panel2.BackColor;

			BackColor = Color.DarkGray;
			Panel1.BackColor = originalPanel1BackColor;
			Panel2.BackColor = originalPanel2BackColor;
		}

		void RevertOriginalBackColor()
		{
			if (BackColor != originalBackColor)
			{
				BackColor = originalBackColor;
			}

			if (Panel1.BackColor != originalPanel1BackColor)
			{
				Panel1.BackColor = originalPanel1BackColor;
			}

			if (Panel2.BackColor != originalPanel2BackColor)
			{
				Panel2.BackColor = originalPanel2BackColor;
			}
		}
#endif

		bool moving;
		Rectangle oldRect;

		void KSplitContainer_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			if (!moving)
			{
				oldRect = SplitterRectangle;
			}
#if !WINZOR
			RevertOriginalBackColor();
#endif
			moving = true;
		}

		void KSplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (moving)
			{
				oldRect.Inflate(ControlDpiScalingHelper.ScaleToCurrentDpiX(2), ControlDpiScalingHelper.ScaleToCurrentDpiY(2));
				Invalidate(oldRect, true);
			}
			moving = false;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Up || e.KeyData == Keys.Down)
			{
				KSplitContainer_SplitterMoving(this, new SplitterCancelEventArgs(this.SplitterRectangle.X, this.SplitterRectangle.Y, this.SplitterRectangle.X, this.SplitterRectangle.Y));
			}
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyData == Keys.Left || e.KeyData == Keys.Right || e.KeyData == Keys.Up || e.KeyData == Keys.Down)
			{
				KSplitContainer_SplitterMoved(this, new SplitterEventArgs(this.SplitterRectangle.X, this.SplitterRectangle.Y, this.SplitterRectangle.X, this.SplitterRectangle.Y));
			}
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(e);
		}

		#region Tab Control

		protected sealed override bool ProcessTabKey(bool forward)
		{
			bool result;
			var active = this.GetFrontMostActiveControl();

			if (active == null || active.GetReadOnly())
			{
				result = base.ProcessTabKey(forward);
			}
			else
			{
				result = this.SelectNextControlNonTabStopNonReadOnly(ActiveControl, forward, true, false);
			}

			return result;
		}

		#endregion

		DpiState IChooseHowMyPropertiesScale.GetDpiState(MemberInfo method)
		{
			switch (method.Name)
			{
				case "Panel1MinSize":
				case "Panel2MinSize":
				case "SplitterDistance":
					return Orientation == Horizontal ? DpiState.ScaleY : DpiState.ScaleX; //Orientation means 'which direction the splitter bar extends in'. So this is correct.
				default:
					return DpiState.Unknown;
			}
		}

		int ISplitterLayoutSaveProvider.SplitterPosition
		{
			get => SplitterDistance;
			set
			{
				var minValue = this.Panel1MinSize;
				var maxValue = 0;
				if (Orientation == Orientation.Vertical)
				{
					maxValue = this.Width - Panel2MinSize - SplitterWidth;
				}
				else
				{
					maxValue = this.Height - Panel2MinSize - SplitterWidth;
				}

				if (value < minValue)
				{
					value = minValue;
				}
				else if (value > maxValue)
				{
					value = maxValue;
				}

				if (value >= minValue && value <= maxValue)
				{
					SplitterDistance = value;
				}
			}
		}

		int ISplitterLayoutSaveProvider.ContainerSize
		{
			get { return Orientation == Orientation.Horizontal ? this.Height : this.Width; }
		}

		bool ISplitterLayoutSaveProvider.IsSplitterFixed
		{
			get => IsSplitterFixed;
		}

		bool ISplitterLayoutSaveProvider.IsLayoutRestored
		{
			get;
			set;
		}

#if WINZOR
		public override bool SplitContainerTabStopFunc(SplitContainer c)
		{
			bool tabStop;
			var active = this.GetFrontMostActiveControl();

			if (active == null || active.GetReadOnly())
			{
				tabStop = true;
			}
			else
			{
				if (ActiveControlPresent == null)
				{
					ActiveControlPresent = ActiveControl != null;
				}

				tabStop = !(bool)ActiveControlPresent;
			}

			return tabStop && base.SplitContainerTabStopFunc(c);
		}

		bool? ActiveControlPresent;
#endif
	}
}
