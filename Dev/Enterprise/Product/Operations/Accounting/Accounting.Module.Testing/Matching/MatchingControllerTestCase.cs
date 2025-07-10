using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class MatchingControllerTestCase : ZControllerBasherTest
	{
		protected MatchingController fMatchingController;

		public void TestNotSubscribeFactory_SavedForNewMatchGroupForm()
		{
			MatchingBase aRBase = new ARMatchingBase(Factory);
			var executeCount = 0;

			using (ZArchitecture.GUI.IZForm newForm = fMatchingController.GetForm_ForTestOnly(aRBase))
			{
				fMatchingController.RefreshGrid += FMatchingController_RefreshGrid;
				AssertEquals("Pre-condition: Should return NewMatchGroupForm for MatchingBase.", true, newForm is NewMatchGroupForm);
				AssertEquals("Pre-condition: executeCount should be 0 before factory saved.", 0, executeCount);
				fMatchingController.Factory.Save();
				aRBase.Factory.Save();
				AssertEquals("Pre-condition: executeCount should still be 0 after factory saved.", 0, executeCount);
			}

			void FMatchingController_RefreshGrid(object sender, EventArgs e)
			{
				executeCount++;
			}
		}

		public void TestFactory_Save()
		{
			MatchingBase aRBase = new ARMatchingBase(Factory);
			try
			{
				using (ZArchitecture.GUI.IZForm newForm = fMatchingController.GetForm_ForTestOnly(aRBase))
				{
					fMatchingController.Factory.Save();
					aRBase.Factory.Save();
					Assert("Should not have null reference exception", true);
				}
			}
			catch (Exception)
			{
				Fail("Should not throw exception on Factory Saving.");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			fMatchingController = (MatchingController)ZControllerFactory.Create(GetControllerID());
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new UnmatchingRow(Factory);
		}

		#region TestGetLoadedBusinessEntity

		public void TestGetLoadedBusinessEntity()
		{
			ARInvoice testARINV = Factory.NewWithValidTestData<ARInvoice>();
			TransactionMatchLink matchlinkA = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
			matchlinkA.AP_AH = testARINV.PK;
			matchlinkA.AP_MatchGroupNum = "M00002000";

			TransactionMatchLink matchlinkB = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
			APInvoice testAPINV = Factory.NewWithValidTestData<APInvoice>();
			matchlinkB.AP_AH = testAPINV.PK;
			matchlinkB.AP_MatchGroupNum = "M00002000";
			TestObjectCreator.SetupMatchLinkMatchDate(testARINV);

			UnmatchingRow testUnmatch = new UnmatchingRow(Factory);
			testUnmatch.MatchGroupNum = "M00002000";

			IBusiness newUnmatch = fMatchingController.GetLoadedBusinessEntityInLocalFactory_ForTestOnly(testUnmatch);
			Assert("BizO is UnmatchingRow", newUnmatch is UnmatchingRow);
			AssertEquals("UnmatchingRow should not be associated with matchlinks", 0,
				((UnmatchingRow)newUnmatch).MatchLinks.Count);

			Factory.Save();

			IBusiness loadedUnmatch = fMatchingController.GetLoadedBusinessEntityInLocalFactory_ForTestOnly(testUnmatch);
			Assert("BizO is UnmatchingRow", loadedUnmatch is UnmatchingRow);
			AssertEquals("UnmatchingRow should be associated with 2 matchlinks", 2,
				((UnmatchingRow)loadedUnmatch).MatchLinks.Count);

			TransactionMatchLink matchlinkC = ((IMatching)testARINV).CurrentMatchGroup.AddNew();
			matchlinkC.AP_AH = testAPINV.PK;
			matchlinkC.AP_MatchGroupNum = "M00002000";
			TestObjectCreator.SetupMatchLinkMatchDate(testARINV);

			Factory.Save();

			fMatchingController = (MatchingController)ZControllerFactory.Create(GetControllerID());
			IBusiness reloadedUnmatch = fMatchingController.GetLoadedBusinessEntityInLocalFactory_ForTestOnly(testUnmatch);
			Assert("BizO is Unmatching Row", loadedUnmatch is UnmatchingRow);
			AssertEquals("Unmatching Row should be associated with 3 matchlinks", 3,
				((UnmatchingRow)reloadedUnmatch).MatchLinks.Count);
		}

		#endregion

		#region TestGetForm

		public void TestGetForm()
		{
			UnmatchingRow matchGroup = new UnmatchingRow(Factory);
			using (ZArchitecture.GUI.IZForm viewForm = fMatchingController.GetForm_ForTestOnly(matchGroup))
			{
				Assert("Form should be View MatchGroup form", viewForm is ViewMatchGroupForm);
			}

			MatchingBase aRBase = new ARMatchingBase(Factory);
			using (ZArchitecture.GUI.IZForm newForm = fMatchingController.GetForm_ForTestOnly(aRBase))
			{
				Assert("Form should be NewMatchGroup form", newForm is NewMatchGroupForm);
			}
		}

		#endregion

		public void TestModuleID()
		{
			AssertEquals(GetExpectedModuleID(), fMatchingController.ModuleID);
		}

		protected abstract ModuleIdentifier GetExpectedModuleID();

		public void TestDeleteMultipleIfMultipleReversingIsNotSupported()
		{
			BusinessObject formBusinessEntity = GetBusinessObjectThatIsInTheDatabase();
			fMatchingController.DeleteMultiple(new BusinessObject[] { formBusinessEntity });
			AssertEquals("A massage must be shown.", "Please select only one Match Session. Match Sessions must be Unmatched individually.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
