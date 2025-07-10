using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Main.Navigation
{
	public class ModuleMenuItem : System.Windows.Forms.MenuItem
	{
		public ModuleMenuItem()
			: base("-")
		{
		}

		public ModuleMenuItem(string text)
			: base(text)
		{
		}

		public ModuleMenuItem(MainFormModule module, EventHandler onClick)
			: this(module, onClick, null)
		{
		}

		public ModuleMenuItem(MainFormModule module, EventHandler onClick, StmModuleFilter filter)
			: base("", onClick)
		{
			Filter = filter;
			Module = module;
			Text = GetMenuItemText();
		}

		public ModuleMenuItem(string text, FontStyle style, Color color, float size, bool enabled = false)
			: base(text)
		{
			this.color = color;
			Enabled = enabled;
			OwnerDraw = true;
			font = new Font(SystemFonts.MenuFont.FontFamily.Name, size, style);
		}

		string GetMenuItemText()
		{
			if (TileNavigationBar.ModuleSupportsOpeningInNewWindow(Module))
			{
				if (!Globals.IsWinzor && GlbStaff.CurrentUser.CheckWinzorEnabledForUser()
					&& !GlbStaff.CurrentUser.CheckFeatureEnabledForUser(StmFeatureTest.WinzorMainFormFeatureCode)
					&& (GlbStaff.CurrentUser.CheckFeatureEnabledForUser(StmFeatureTest.WinzorAllFeaturesCode) || GlbStaff.CurrentUser.CheckFeatureEnabledForUser(Module.ID)))
				{
					return Res.GetString("a76f1135-610a-4c91-a6d2-02f5435bfcbb", "Open {0} i&n CargoWise Classic", string.IsNullOrEmpty(Module.RecordUrl) ? Module.Description : Module.RecordDescription);
				}
				else
				{
					return Res.GetString("8269de52-d5fa-4685-a5c5-a17836f50f52", "Open {0} in a &New Window", string.IsNullOrEmpty(Module.RecordUrl) ? Module.Description : Module.RecordDescription);
				}
			}
			else if (Filter != null)
			{
				return Res.GetString("cd044d26-b599-4663-abca-3a0fdd9111fd", "Open {0} filter layout", Filter.S9_FilterNameMultilingual);
			}

			return Res.GetString("e7f7bf37-ac96-409e-a3e5-5582f6c75fc1", "Open {0}", string.IsNullOrEmpty(Module.RecordUrl) ? Module.Description : Module.RecordDescription);
		}

		public readonly MainFormModule Module;
		public readonly StmModuleFilter Filter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Still used by non Winzor, will be fixed by the move to .Net 8")]
		readonly Font font;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Still used by non Winzor, will be fixed by the move to .Net 8")]
		readonly Color color;

		#if !WINZOR

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			var stringSize = TextRenderer.MeasureText(Text, font);
			e.ItemWidth = stringSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(16);
			e.ItemHeight = stringSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			e.Graphics.FillRectangle(SystemBrushes.MenuBar, e.Bounds);
			using (var brush = new SolidBrush(color))
			{
				e.Graphics.DrawString(Text, font, brush, e.Bounds.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(16), e.Bounds.Y);
			}
		}

		#endif
	}
}
