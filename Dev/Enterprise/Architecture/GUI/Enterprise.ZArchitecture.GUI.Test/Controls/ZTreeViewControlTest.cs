namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTreeViewControlTest : ZControlBaseTestCase<ZTreeView>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "ReadOnly" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Text"; }
		}

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}
	}
}
