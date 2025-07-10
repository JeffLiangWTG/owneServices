using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public partial class MapTreeRightPanel : ZUserControl
	{
		public MapTreeRightPanel(DataReflectorValueProviderWrapper wrapper)
		{
			InitializeComponent();
			this.Wrapper = wrapper;
		}

		internal readonly DataReflectorValueProviderWrapper Wrapper;
	}
}
