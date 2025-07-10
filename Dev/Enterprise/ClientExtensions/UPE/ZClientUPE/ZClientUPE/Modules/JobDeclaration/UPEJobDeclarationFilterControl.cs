using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.UPE.Module
{
	public partial class UPEJobDeclarationFilterControl : ZFilterStripControl<UPEZFilterStrip>
	{
		public UPEJobDeclarationFilterControl()
		{
			InitializeComponent();
		}

		public UPEJobDeclarationFilterControl(IBusinessObjectCollection gridCollection, UPEJobDeclarationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public new UPEJobDeclarationFilterBusinessObject FilterBusinessObject
		{
			get { return (UPEJobDeclarationFilterBusinessObject)base.FilterBusinessObject; }
		}
	}
}
