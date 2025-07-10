using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUEdificeMiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestChangeControlVisibility()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXP";
			using (var testUserControl = new AUEdificeMiscOptionsUserControl())
			{
				testUserControl.JobDeclaration = testDec;
				testDec.JE_MessageType = "IMP";
				AssertEquals("Import Control is visible: MergeByDropEdit", true, testUserControl.FindSingle<ZDropEdit>("MergeByDropEdit").Visible);
				AssertEquals("Import Control is visible: CompilePrintersGroupBox", true, testUserControl.FindSingle<ZGroupBox>("CompilePrintersGroupBox").Visible);
				AssertEquals("Import Control is visible: ForcePrimeEnclosureCheckBox", true, testUserControl.FindSingle<ZCheckBox>("ForcePrimeEnclosureCheckBox").Visible);
				AssertEquals("Import Control is visible: ManifestClientIDTextBox", true, testUserControl.FindSingle<ZTextBox>("ManifestClientIDTextBox").Visible);
				AssertEquals("Export control is invisible: CCANTextBox", false, testUserControl.FindSingle<ZTextBox>("CCANTextBox").Visible);
				testDec.JE_MessageType = "IMP";
				testDec.JE_MessageType = "EXP";
				AssertEquals("EXP control is visible: CCANTextBox", true, testUserControl.FindSingle<ZTextBox>("CCANTextBox").Visible);
			}
		}

		public void TestMergeByDropEditVisibility()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXP";
			using (var testUserControl = new AUEdificeMiscOptionsUserControl())
			{
				testUserControl.JobDeclaration = testDec;
				testDec.JE_MessageType = "IMP";
				AssertEquals("Import Control is visible: MergeByDropEdit", true, testUserControl.FindSingle<ZDropEdit>("MergeByDropEdit").Visible);

				testDec.JE_MessageType = "EXP";
				AssertEquals("Export Control is visible: MergeByDropEdit", true, testUserControl.FindSingle<ZDropEdit>("MergeByDropEdit").Visible);

				var entryNum = CusEntryNumber.New(testDec, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
				entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				testDec.JE_MessageType = "IMP";
				testDec.JE_MessageType = "EXP";
				AssertEquals("Export Control is invisible: MergeByDropEdit", false, testUserControl.FindSingle<ZDropEdit>("MergeByDropEdit").Visible);
			}
		}
	}
}
