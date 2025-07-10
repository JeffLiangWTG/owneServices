using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(CAMessageChooserNonPersistent))]
	sealed class CAMessageChooserNonPersistentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageManagersToSend()
		{
			AssertEquals("Count", 2, Chooser.MessageManagersToSend.Count);
			SetAllIsShouldSend(false);
			foreach (var value in Chooser.MessageManagersToSend.Values)
			{
				AssertEquals("False", false, value);
			}
			SetAllIsShouldSend(true);
			foreach (var value in Chooser.MessageManagersToSend.Values)
			{
				AssertEquals("True", true, value);
			}
		}

		public void TestSelectedManagers()
		{
			SetAllIsShouldSend(false);
			AssertEquals("Length", 0, Chooser.SelectedManagers.Length);
			SetAllIsShouldSend(true);
			AssertEquals("Length", 2, Chooser.SelectedManagers.Length);
		}

		#region Implementation

		const string ccn = "ccn";
		CusCAeMHMaster cusCAeMHMaster;
		CusCAeMHHouse cusCAeMHHouse;
		CusCAeMHMasterCloseWrapper closeWrapper;
		ACIForwarderCloseMessageManager closeMessageManager;
		ACIHouseBillMessageManager houseBillMessageManager;

		protected override void SetUp()
		{
			base.SetUp();
			cusCAeMHMaster = Factory.New<CusCAeMHMaster>();
			cusCAeMHHouse = cusCAeMHMaster.HouseBills.AddNew();
			cusCAeMHHouse.BW_HouseCCN = ccn;
			closeWrapper = new CusCAeMHMasterCloseWrapper(cusCAeMHMaster);
			closeMessageManager = new ACIForwarderCloseMessageManager(closeWrapper, null);
			houseBillMessageManager = new ACIHouseBillMessageManager(cusCAeMHHouse, null);
			chooser = new CAMessageChooserNonPersistent(new SingleMessageManager[] { closeMessageManager, houseBillMessageManager }, "question", "Action", ActionPurpose.Origin);
		}

		void SetAllIsShouldSend(ZBool isShouldSend)
		{
			Chooser.MessageManagersToSend[closeMessageManager] = isShouldSend;
			Chooser.MessageManagersToSend[houseBillMessageManager] = isShouldSend;
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Chooser;
		}

		CAMessageChooserNonPersistent chooser;
		CAMessageChooserNonPersistent Chooser
		{
			get
			{
				if (chooser == null)
				{
					chooser = new CAMessageChooserNonPersistent(new SingleMessageManager[] { closeMessageManager, houseBillMessageManager }, "question", "Action", ActionPurpose.Origin);
				}
				return chooser;
			}
		}
	}
}
