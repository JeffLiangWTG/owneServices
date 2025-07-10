using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.BR.GUI
{
	public partial class MultiCodesSelectForm : ZChildForm, IFindBoxPopup
	{
		public MultiCodesSelectForm(CodeDescriptionPairList list)
		{
			List = GetBoolDescriptionPairList(list);
			BindingSource.SetDataBinding(List, ".");
			InitializeComponent();
		}
		ZBoolDescriptionPairList List { get; set; }

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK)
		{
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.findBox = findBox;
			var selectedCodes = findBox.Code.Split(',');
			List.Cast<ZBoolCodeDescriptionPair>().Where(x => selectedCodes.Contains(x.Code)).ForEach(x => x.Value = true);
			ZFormModaliser.Show(this, parentForm);
		}
		IFindBox findBox;

		ZBoolDescriptionPairList GetBoolDescriptionPairList(CodeDescriptionPairList list)
		{
			var boolDescriptionPairList = new ZBoolDescriptionPairList();
			foreach (CodeDescriptionPair pair in list)
			{
				boolDescriptionPairList.Add(new ZBoolCodeDescriptionPair(pair.Code, pair.CodeAndDescription, false));
			}
			return boolDescriptionPairList;
		}

		class ZBoolCodeDescriptionPair : ZBoolDescriptionPair
		{
			public ZBoolCodeDescriptionPair(string code, string description, ZBool value) : base(description, value)
			{
				Code = code;
			}

			public string Code { get; private set; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			findBox.Code = string.Join(",", List.Cast<ZBoolCodeDescriptionPair>().Where(x => x.Value).Select(x => x.Code));
			Close();
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
