using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KElementHost : System.Windows.Forms.Integration.ElementHost
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal String is safe to use in this context.")]
		protected override void WndProc(ref Message m)
		{
			try
			{
				base.WndProc(ref m);
			}
			catch (ArgumentException ex) when (ex.Message.Equals("Width and Height must be non-negative."))
			{
				// Do nothing, This exception seems like is invisible for users.
				// Eating this exception to prevent the issue created and waiting for the incidents from the user who inflected by it.
			}
		}
	}
}
