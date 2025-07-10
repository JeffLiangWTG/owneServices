using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public partial class G3DeclarationFilterControl : ZFilterStripControl
	{
		public G3DeclarationFilterControl(IBusinessObjectCollection gridCollection, G3DeclarationFilterBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
