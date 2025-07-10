using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class StandAloneFsrEnquiryFormForNew : ZChildForm
	{
		public StandAloneFsrEnquiryFormForNew(NewStandAloneFsrEnquiryManager manager)
			: base(manager.NonPersistentBO)
		{
			this.manager = manager;
			manager.NonPersistentBO.CcsukLicenceLoginHandler += new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
		}

		void CreateButton_Click(object sender, System.EventArgs e)
		{
			if (manager.NonPersistentBO.IsValid)
			{
				manager.ExecuteMakingRealEdiMessageFromNonPersistentHelper();
				LicenceAndPimaHelper.RecordLicenceLoginBasedOnProfileOfAncillaryJob(manager.NonPersistentBO.PIMA, manager.NonPersistentBO.PIMAInfo, manager.NonPersistentBO);
				Close();
			}
			else
			{
				Globals.Message.Show("Please fix the validation errors first");
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void HandleShedLicenceLogin(object sender, Customs.Business.LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			licenceLoginResult = e.LicenceCheckPoint.Login(this);
		}
		LicenceLoginResponse? licenceLoginResult;

#if DEBUG
		public LicenceLoginResponse? GetLicenceLoginResultForTest()
		{
			return licenceLoginResult;
		}
#endif

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			if (manager != null && manager.NonPersistentBO != null)
			{
				manager.NonPersistentBO.CcsukLicenceLoginHandler -= new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			}
			base.Dispose(disposing);
		}
		readonly NewStandAloneFsrEnquiryManager manager;
	}
}
