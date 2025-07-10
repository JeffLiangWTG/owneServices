using CargoWise.ComponentModel;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class DownloadGoodsCatalogForm : ZChildForm
	{
		public DownloadGoodsCatalogForm(GoodsCatalogDownloadObject goodsCatalogDownloadObject) : base(goodsCatalogDownloadObject)
		{
			InitializeComponent();
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		GoodsCatalogDownloadObject GoodsCatalogDownloadObject => DataSource as GoodsCatalogDownloadObject;

		void DownloadButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				if (MessageSendingEnviromentChecker.CheckIsOKToSend())
				{
					var goodsCatalogDownloadObject = new GoodsCatalogDownloadMessageSender(GoodsCatalogDownloadObject);
					if (goodsCatalogDownloadObject.CanSendMessage())
					{
						var countOfMessages = goodsCatalogDownloadObject.SendMessagesAndSave();
						Globals.Message.Show(Res.GetString("22546817-75DB-4A32-B872-F05D4D8293C4", "{0} Download message(s) have been sent.", countOfMessages));
						Close();
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("16032ACD-8DA7-45EA-A2E5-46025D31E554", "A second download request is not allowed for this Owner when the first one was not replied."));
					}
				}
			}
		}
	}
}
