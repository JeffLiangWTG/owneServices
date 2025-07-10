using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZCheckBox : KCheckBox, IBindTo, IExtendedControl, IBackColorMutable, IResCaptionedControl, IDisposeStackProvider, IEditableInViewMode
	{
		protected override bool ReadOnlyAttributeValue => ReadOnly;
	}
}
