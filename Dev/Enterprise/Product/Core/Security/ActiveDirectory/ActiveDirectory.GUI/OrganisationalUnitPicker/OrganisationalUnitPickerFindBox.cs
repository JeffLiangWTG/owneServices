using System.Collections;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	[SuppressCheckControlModuleId]
	public partial class OrganisationalUnitPickerFindBox : ZGridFindBox
	{
		public OrganisationalUnitPickerFindBox()
		{
			InitializeComponent();
			this.SetReadOnly(true);
			PopupButtonReadonlyCanBeDifferent = true;
			PopupButton.ReadOnly = false;
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keyData)
		{
			if (keyData == Keys.Delete)
			{
				Code = ZString.Empty;
			}

			return base.ProcessCmdKey(ref message, keyData);
		}

		protected override IFindBoxPopup GetNewPopupForm() => new OrganisationalUnitPickerPopup(() => (DomainCredentials)CurrentItem);

		protected override IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting) => null;
	}
}
