using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class FilterTabPageControl : ZUserControl
	{
		public FilterTabPageControl()
			: this(string.Empty, string.Empty)
		{
		}

		public FilterTabPageControl(string bindToProperty, string filterIdentifier, string name = "FilterTabPageControl")
		{
			BindToProperty = bindToProperty;
			FilterIdentifier = filterIdentifier;
			Name = name;
			InitializeComponent();
			CaptionRenderingEnabled = true;
		}

		public string BindToProperty { get; set; }

		string FilterIdentifier { get; }

		public BMFilterStripWrapperControl FilterStripControl => filterStripWrapperControl1;
	}
}
