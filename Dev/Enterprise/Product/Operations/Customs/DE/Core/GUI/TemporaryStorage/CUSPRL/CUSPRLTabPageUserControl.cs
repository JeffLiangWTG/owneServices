using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI
{
	public partial class CUSPRLTabPageUserControl : TemporaryStorageMutexTabPageUserControl
	{
		public CUSPRLTabPageUserControl()
		{
			InitializeComponent();
		}

		protected override CusTempStorageDec CreateNewCusTempStorageDec()
		{
			return CUSPRLCusTempStorageDec.New(CurrentDataItem);
		}

		protected override ZUserControl GetCusTempStorageDecControl()
		{
			var result = new CUSPRLDeclarationUserControl
			{
				Dock = DockStyle.Fill,
				Visible = false,
				Name = "CUSPRLDecControl",
			};

			result.SetDataBinding(CurrentDataItem, "");//SetDataBinding gets called before a dec is created. Needs to call again after a dec is created
			return result;
		}

		protected override ZString DeclarationType => TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger;

		protected override MutexID MutexID => MutexIDs.CUSPRLTempStorageDec;

		protected override ZString DeclarationTypeDescription => TemporaryStorageApplicationCodeList.Descriptions.SumA;
	}
}


