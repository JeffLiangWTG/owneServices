using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class RulerPanel : ZPanel, IHotkeyProvider
	{
		readonly RulerLine[] lines = new[]
		{
			new RulerLine { Location = ControlDpiScalingHelper.ScaleToCurrentDpiX(100), Oreantation = Oreantation.Vertical },
			new RulerLine { Location = ControlDpiScalingHelper.ScaleToCurrentDpiY(100), Oreantation = Oreantation.Horizontal },
		};

		IMessageFilter mouseMessageFilter;

		public RulerPanel()
		{
			InitializeComponent();
			RegisterHotkeys();

			AddEvents(this);

			SetStyle(ControlStyles.Selectable, true);
			TabStop = true;
		}

		bool showLines = true;
		public bool ShowLines
		{
			get { return showLines; }
			set
			{
				showLines = value;
				Refresh();
			}
		}

		#region Hotkeys

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Up, () => MoveLine(Oreantation.Horizontal, -1), Res.GetString("A451F280-BB83-4935-BF99-EA751F1C246C", "Move line up"));
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Down, () => MoveLine(Oreantation.Horizontal, 1), Res.GetString("0D5CF213-E4D2-453E-A857-2AEFEA3C90B4", "Move line down"));
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Left, () => MoveLine(Oreantation.Vertical, -1), Res.GetString("F5BC1F14-6EC1-4A43-8AF0-1ECFC8CB5670", "Move line left"));
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Right, () => MoveLine(Oreantation.Vertical, 1), Res.GetString("E7F5F708-D11B-4A90-A445-851BB44F3D81", "Move line right"));
		}

		public HotkeyRegister Hotkeys { get; } = new HotkeyRegister();
		string IHotkeyProvider.TypeNameForDisplay => Res.GetString("0833B401-7132-4BB8-AE8B-F960FDDED2B3", "Ruler");

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return Hotkeys.ProcessCmdKey(this, keyData) || base.ProcessCmdKey(ref msg, keyData);
		}

		void MoveLine(Oreantation oreantation, int deltaScaled)
		{
			var line = lines.First(l => l.Oreantation == oreantation);
			line.Location += deltaScaled;

			Refresh();
		}

		#endregion

		#region Events for keeping line over children on repaints

		void AddEvents(Control control)
		{
			control.ControlAdded += Control_ControlAdded;
			control.ControlRemoved += Control_ControlRemoved;
			control.Paint += Control_Paint;

			foreach (Control child in control.Controls)
			{
				AddEvents(child);
			}
		}

		void RemoveEvents(Control control)
		{
			control.ControlAdded -= Control_ControlAdded;
			control.ControlRemoved -= Control_ControlRemoved;
			control.Paint -= Control_Paint;

			foreach (Control child in control.Controls)
			{
				RemoveEvents(child);
			}
		}

		void Control_Paint(object sender, PaintEventArgs e)
		{
			if (ShowLines)
			{
				PaintForControl((Control)sender, e.Graphics);
			}
		}

		void Control_ControlAdded(object sender, ControlEventArgs e)
		{
			AddEvents(e.Control);
		}

		void Control_ControlRemoved(object sender, ControlEventArgs e)
		{
			RemoveEvents(e.Control);
		}

		#endregion

		#region Painting

		void PaintForControl(Control c, Graphics g)
		{
			foreach (var line in lines)
			{
				Point start, end;
				if (line.Oreantation == Oreantation.Vertical)
				{
					start = ControlDpiScalingHelper.NewScaledPoint(line.Location, 0, false);
					end = ControlDpiScalingHelper.NewScaledPoint(line.Location, Height, false);
				}
				else
				{
					start = ControlDpiScalingHelper.NewScaledPoint(0, line.Location, false);
					end = ControlDpiScalingHelper.NewScaledPoint(Width, line.Location, false);
				}

				g.DrawLine(Pens.Red, c.PointToClient(PointToScreen(start)), c.PointToClient(PointToScreen(end)));
			}
		}

		#endregion

		#region Cursor / Focus

		void UpdateCursorStyle(Point mousePosition)
		{
			bool isHittingV = false, isHittingH = false;
			foreach (var line in lines)
			{
				if (line.HitTest(mousePosition))
				{
					isHittingH |= line.Oreantation == Oreantation.Horizontal;
					isHittingV |= line.Oreantation == Oreantation.Vertical;
				}
			}

			if (isHittingH && isHittingV)
			{
				Cursor.Current = Cursors.Cross;
			}
			else if (isHittingH)
			{
				Cursor.Current = Cursors.HSplit;
			}
			else if (isHittingV)
			{
				Cursor.Current = Cursors.VSplit;
			}

			if (!Focused && (isHittingV | isHittingH))
			{
				Focus();
			}
		}

		#endregion

		#region Message Filtering and Events

		readonly List<RulerLine> grabbedLines = new List<RulerLine>();
		bool AddNearbyLinesToGrabbedList(Point mousePosition)
		{
			foreach (var line in lines)
			{
				if (line.HitTest(mousePosition))
				{
					grabbedLines.Add(line);
				}
			}

			return grabbedLines.Count > 0;
		}

		bool ClearGrabbedLines()
		{
			if (grabbedLines.Count > 0)
			{
				grabbedLines.Clear();
				Refresh();

				return true;
			}

			return false;
		}

		bool DragGrabbedLines(Point mousePosition)
		{
			if (grabbedLines.Count > 0)
			{
				foreach (var line in grabbedLines)
				{
					line.Location = line.Oreantation == Oreantation.Horizontal ? mousePosition.Y : mousePosition.X;
				}

				Refresh();

				return true;
			}

			return false;
		}

		bool MouseHook(ref Message m, Point mousePosition)
		{
			if (ShowLines)
			{
				switch (m.Msg)
				{
					case SafeNativeMethods.WM_LBUTTONDOWN:
						return AddNearbyLinesToGrabbedList(mousePosition);

					case SafeNativeMethods.WM_LBUTTONUP:
						return ClearGrabbedLines();

					case SafeNativeMethods.WM_MOUSEMOVE:
						UpdateCursorStyle(mousePosition);
						return DragGrabbedLines(mousePosition);
				}
			}

			return false;
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			mouseMessageFilter = new MouseMessageFilter(this, MouseHook);
			Application.AddMessageFilter(mouseMessageFilter);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			Application.RemoveMessageFilter(mouseMessageFilter);
			base.OnHandleDestroyed(e);
		}

		#endregion

		#region Methods for testing
#if DEBUG

		internal void SetRulerLocation(Oreantation oreantation, int locationScaled)
		{
			var rule = lines.First(l => l.Oreantation == oreantation);

			rule.Location = locationScaled;
			Refresh();
		}

		internal int GetRulerLocation(Oreantation oreantation)
		{
			return lines.First(l => l.Oreantation == oreantation).Location;
		}

		internal bool FireMouseHook(ref Message message, Point mousePosition)
		{
			return MouseHook(ref message, mousePosition);
		}

#endif
		#endregion
	}

	#region Helper Classes

	class RulerLine
	{
		[DpiState(DpiState.ScaledVariant)]
		readonly int grabDistancePx = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);

		[DpiState(DpiState.ScaledVariant)]
		public int Location { get; set; }
		public Oreantation Oreantation { get; set; }

		public bool HitTest(Point pt)
		{
			var dimensionScaled = Oreantation == Oreantation.Horizontal ? pt.Y : pt.X;

			return Math.Abs(Location - dimensionScaled) <= grabDistancePx;
		}
	}

	enum Oreantation { Horizontal, Vertical }

	#endregion
}
