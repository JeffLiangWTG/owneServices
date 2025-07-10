namespace Enterprise.Registry.GUI.Testing
{
	sealed class DpsWebServiceItemControlForTest : DpsWebServiceItemControl
	{
		public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);
	}
}
