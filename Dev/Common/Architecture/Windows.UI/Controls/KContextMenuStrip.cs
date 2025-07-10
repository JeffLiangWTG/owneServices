using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Interop;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KContextMenuStrip : ContextMenuStrip
	{
		public KContextMenuStrip() : base() { }
		public KContextMenuStrip(IContainer container) : base(container) { }

#if !WINZOR
		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			SendF1KeyDownToSourceControlForm(keyData);

			return base.ProcessCmdKey(ref m, keyData);
		}

		void SendF1KeyDownToSourceControlForm(Keys keyData)
		{
			if (SourceControl != null && keyData == Keys.F1)
			{
				var form = SourceControl.FindForm();
				if (form != null)
				{
					var m = Message.Create(form.Handle, WindowsMessage.WM_KEYDOWN, new IntPtr((int)keyData), new IntPtr(0));
					form.PreProcessMessage(ref m);
				}
			}
		}
#endif
	}
}
