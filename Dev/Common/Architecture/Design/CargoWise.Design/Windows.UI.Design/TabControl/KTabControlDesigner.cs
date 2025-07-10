using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms.Design.Ripped;

namespace CargoWise.Windows.UI.Design
{
	public class KTabControlDesigner : TabControlDesigner
	{
		protected internal Type ExternalTabPageType => TabPageType;
		protected override Type TabPageType
		{ get { return (Type)typeof(KTabControl).InvokeMember("TabPageType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, Control, null, CultureInfo.InvariantCulture); } }

		new KTabControl Control
		{ get { return (KTabControl)base.Control; } }
	}
}
