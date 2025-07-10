namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridLayoutConfigurableFalse : ZGrid
	{
		public GridLayoutConfigurableFalse(object dataSource)
			: base()
		{
			this.DataSource = dataSource;
			OnAfterDataBound();
		}

		protected internal override bool IsGridLayoutConfigurable
		{
			get { return false; }
		}

		public override GridColourScheme GetLastUsedColourSchemeForCurrentUser => SchemeForTest;

		public GridColourScheme SchemeForTest { get; set; }
	}
}
