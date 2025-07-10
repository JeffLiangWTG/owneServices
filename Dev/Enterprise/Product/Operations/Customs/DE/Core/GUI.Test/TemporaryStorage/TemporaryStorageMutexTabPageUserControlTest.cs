using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public abstract class TemporaryStorageTabPageUserControlTest : TestCaseWithFactory
	{
		protected abstract TemporaryStorageMutexTabPageUserControl GetUserControlToTest();
		protected abstract ZString DeclarationTypeDescription { get; }
		protected abstract ZString DecControlName { get; }
		protected abstract MutexID MutexID { get; }

		public void TestNoCurrentDataItem()
		{
			using (var userControl = GetUserControlToTest())
			{
				var coveringLabel = userControl.Controls["CoveringLabel"];
				Assert(!coveringLabel.Visible);
				userControl.LoadUserControl();
				Assert(coveringLabel.Visible);
				AssertEquals(string.Format("You have chosen not to create a {0} Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a {0} Declaration.", DeclarationTypeDescription), coveringLabel.Text);
			}
		}

		public void DecControlExists()
		{
			using (var userControl = GetUserControlToTest())
			{
				userControl.SetDataBinding(header, ZString.Empty);
				userControl.LoadUserControl();
				Assert(!userControl.Controls["CoveringLabel"].Visible);
				var decControl = userControl.Controls[DecControlName];
				AssertNotNull(decControl);
				Assert(decControl.Visible);
			}
		}

		public void TestMutexLockAndRelease()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			using (var anotherUserMutex = new ZGlobalMutex(MutexID, header.PK.ToString()))
			{
				anotherUserMutex.Lock();
				using (var userControl = GetUserControlToTest())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					userControl.SetDataBinding(header, ZString.Empty);
					userControl.LoadUserControl();
					var coveringLabel = userControl.Controls["CoveringLabel"];
					Assert(coveringLabel.Visible);
					AssertEquals(string.Format("Someone else is already in the process of creating a {0} Declaration for this {0} job.\r\nYou should be able to access the {0} Declaration when the person has saved or canceled. Please try later.", DeclarationTypeDescription), coveringLabel.Text);
					anotherUserMutex.Unlock();
					userControl.LoadUserControl();
					AssertEquals(string.Format("Are you sure you want to create a {0} Declaration now?", DeclarationTypeDescription), UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(coveringLabel.Visible);
					AssertEquals(string.Format("You have chosen not to create a {0} Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a {0} Declaration.", DeclarationTypeDescription), coveringLabel.Text);
				}
			}
		}

		public void TestCreateCusTempStorageAndSaveReleasesMutex()
		{
			var customerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_OH_Customer = customerOrg.PK;
			AssertNull(LoadCusTemStorageDec(header));
			using (var anotherUserMutex = new ZGlobalMutex(MutexID, header.PK.ToString()))
			{
				Assert(!anotherUserMutex.IsLocked);
				using (var userControl = GetUserControlToTest())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					userControl.SetDataBinding(header, ZString.Empty);
					userControl.LoadUserControl();
					Assert(!userControl.Controls["CoveringLabel"].Visible);
					var decControl = userControl.Controls[DecControlName];
					AssertNotNull(decControl);
					Assert(decControl.Visible);
					AssertNotNull(LoadCusTemStorageDec(header));
					Assert(!anotherUserMutex.Lock());
					Factory.Save();
					Assert(anotherUserMutex.Lock());
					anotherUserMutex.Unlock();
				}
			}
		}

		public void TestCurrentDataItemChangeRemovesMutex()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var header2 = Factory.New<CusTempStorageJobHeader>();
			using (var anotherMutexWithSameKey = new ZGlobalMutex(MutexID, header.PK.ToString()))
			{
				Assert(!anotherMutexWithSameKey.IsLocked);
				using (var userControl = GetUserControlToTest())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					userControl.SetDataBinding(header, ZString.Empty);
					userControl.LoadUserControl();
					Assert(!userControl.Controls["CoveringLabel"].Visible);
					var decControl = userControl.Controls[DecControlName];
					AssertNotNull(decControl);
					Assert(decControl.Visible);
					Assert(!anotherMutexWithSameKey.Lock());
					userControl.SetDataBinding(header2, ZString.Empty);
					Assert(anotherMutexWithSameKey.Lock());
					anotherMutexWithSameKey.Unlock();
				}
			}
		}

		protected virtual CusTempStorageDec LoadCusTemStorageDec(CusTempStorageJobHeader header) => null;

		protected override void SetUp()
		{
			base.SetUp();
			var customerOrg = Factory.NewWithValidTestData<OrgHeader>();
			header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_OH_Customer = customerOrg.PK;
		}
		protected CusTempStorageJobHeader header;
	}
}
