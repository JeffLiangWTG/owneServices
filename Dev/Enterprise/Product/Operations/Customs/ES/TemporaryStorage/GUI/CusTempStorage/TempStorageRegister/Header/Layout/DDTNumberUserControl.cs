using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using CusTempStorageRegPremisesTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class DDTNumberUserControl : ZUserControl
	{
		public DDTNumberUserControl()
		{
			InitializeComponent();
			SetupGoToUrlButton();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetupGoToUrlButtonVisibility();
		}

		void SetupGoToUrlButtonVisibility()
		{
			if (DataSource is CusTempStorageRegHeader cusTempStorageRegHeader)
			{
				GoToUrlButton.Visible = (cusTempStorageRegHeader.Premises?.SRP_Type ?? ZString.Empty) == CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			}
		}

		void SetupGoToUrlButton()
		{
			GoToUrlButton.FlatStyle = FlatStyle.Standard;
			GoToUrlButton.BackgroundImage = Icons.GetImage(IconTypes.Globe20x16);
		}

		void GoToUrlButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is CusTempStorageRegHeader header)
			{
				var url = header.TSDStatusURL;
				if (!url.IsEmpty)
				{
					WebUrlLauncher.Launch(url);
				}
			}
		}
	}
}
