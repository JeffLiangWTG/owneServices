using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common.Enumeration;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Forms
{
	public class StmNoteControl : IStmNoteControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Baseline")]
		public string GetActiveControlInfo()
		{
			var rootControl = ZApplication.GetOpenForms().FirstOrDefault(f => f.ContainsFocus)?.ActiveControl;

			if (rootControl == null)
			{
				return "No active control found";
			}

			Control GetActiveChild(Control c)
				=> c.Controls.Cast<Control>().FirstOrDefault(child => child.ContainsFocus);

			var sb = new StringBuilder();
			sb.Append("Current tab : " + rootControl.Parent?.Text ?? "No Parent");
			foreach (var control in ZEnumerable.Iterate(rootControl, GetActiveChild, null))
			{
				var isTextOnlyForBindingValue = "Not available";
				BusinessObject currentDataItem = null;

				if (control.GetType().Name.Contains("ZStmNoteRichTextBox"))
				{
					var bindingProperty = (ZBool)control.GetType().GetProperty("IsTextOnlyForBinding", BindingFlags.Public | BindingFlags.Instance).GetValue(control, null);
					isTextOnlyForBindingValue = bindingProperty.ToString();
					currentDataItem = (BusinessObject)control.GetType().GetProperty("CurrentDataItem", BindingFlags.Public | BindingFlags.Instance).GetValue(control, null);
				}

				sb.Append(FormattableString.Invariant($@"
Type: {control.GetType().Name}
Name: '{control.Name}'
Visible: {control.Visible}
Enabled: {control.Enabled}
IsTextOnlyForBinding: {isTextOnlyForBindingValue}
CurrentDataItem: {currentDataItem?.PK.ToString() ?? "null"}
BindingMember: {(control as IDataBoundControl)?.DataMember ?? "Not bound"}"));
			}

			return sb.ToString();
		}
	}
}


