using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Testing
{
	public class GBFECChallengeUserControlTest : TestCaseWithFactory
	{
		public void TestFieldVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			declaration.InvoiceLines.AddNew().JI_CL = entryLine.PK;

			using (var form = new ZForm(entryHeader))
			{
				entryHeader.FECChallenges.RemoveAndDeleteAll();
				var dsp = entryHeader.FECChallenges.AddNew();
				dsp.CY_Code = FECChallengeFields.Codes.JE_DSP;
				dsp.CY_ParentTableCode = "CH";
				dsp.CY_ParentID = entryHeader.PK;
				dsp.CY_IsOverridden = true;
				using (var control = new GBFECChallengeUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.FECChallengesGrid.ListManager.Refresh();

					AssertEquals("OriginalValueCodeFindBox", true, control.FindSingle<ZCodeFindBox>(x => x.Name == "OriginalValueCodeFindBox").Visible);
					AssertEquals("NewValueCodeFindBox", true, control.FindSingle<ZCodeFindBox>(x => x.Name == "NewValueCodeFindBox").Visible);
					AssertEquals("OriginalValueDropEdit", false, control.FindSingle<ZDropEdit>(x => x.Name == "NewValueDropEdit").Visible);
					AssertEquals("NewValueDropEdit", false, control.FindSingle<ZDropEdit>(x => x.Name == "OriginalValueDropEdit").Visible);
					AssertEquals("OriginalValueCalcEdit", false, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "OriginalValueCalcEdit").Visible);
					AssertEquals("NewValueCalcEdit", false, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "NewValueCalcEdit").Visible);
				}
			}
			using (var form = new ZForm(entryHeader))
			{
				entryHeader.FECChallenges.RemoveAndDeleteAll();
				var nettMass = entryHeader.FECChallenges.AddNew();
				nettMass.CY_Code = FECChallengeFields.Codes.JI_NettMass;
				nettMass.CY_ParentTableCode = "CL";
				nettMass.CY_ParentID = entryLine.PK;
				nettMass.CY_IsOverridden = true;
				using (var control = new GBFECChallengeUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.FECChallengesGrid.ListManager.Refresh();

					AssertEquals("OriginalValueCodeFindBox", false, control.FindSingle<ZCodeFindBox>(x => x.Name == "OriginalValueCodeFindBox").Visible);
					AssertEquals("NewValueCodeFindBox", false, control.FindSingle<ZCodeFindBox>(x => x.Name == "NewValueCodeFindBox").Visible);
					AssertEquals("OriginalValueDropEdit", false, control.FindSingle<ZDropEdit>(x => x.Name == "NewValueDropEdit").Visible);
					AssertEquals("NewValueDropEdit", false, control.FindSingle<ZDropEdit>(x => x.Name == "OriginalValueDropEdit").Visible);
					AssertEquals("OriginalValueCalcEdit", true, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "OriginalValueCalcEdit").Visible);
					AssertEquals("NewValueCalcEdit", true, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "NewValueCalcEdit").Visible);
				}
			}
			using (var form = new ZForm(entryHeader))
			{
				entryHeader.FECChallenges.RemoveAndDeleteAll();
				var nettMass = entryHeader.FECChallenges.AddNew();
				nettMass.CY_Code = FECChallengeFields.Codes.JI_Price;
				nettMass.CY_ParentTableCode = "CL";
				nettMass.CY_ParentID = entryLine.PK;
				nettMass.CY_IsOverridden = true;
				using (var control = new GBFECChallengeUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.FECChallengesGrid.ListManager.Refresh();

					AssertEquals("OriginalValueCodeFindBox", false, control.FindSingle<ZCodeFindBox>(x => x.Name == "OriginalValueCodeFindBox").Visible);
					AssertEquals("NewValueCodeFindBox", false, control.FindSingle<ZCodeFindBox>(x => x.Name == "NewValueCodeFindBox").Visible);
					AssertEquals("OriginalValueDropEdit", false, control.FindSingle<ZDropEdit>(x => x.Name == "NewValueDropEdit").Visible);
					AssertEquals("NewValueDropEdit", false, control.FindSingle<ZDropEdit>(x => x.Name == "OriginalValueDropEdit").Visible);
					AssertEquals("OriginalValueCalcEdit", true, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "OriginalValueCalcEdit").Visible);
					AssertEquals("NewValueCalcEdit", true, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "NewValueCalcEdit").Visible);
				}
			}
			using (var form = new ZForm(entryHeader))
			{
				entryHeader.FECChallenges.RemoveAndDeleteAll();
				var nettMassUQ = entryHeader.FECChallenges.AddNew();
				nettMassUQ.CY_Code = FECChallengeFields.Codes.JI_NettMassUQ;
				nettMassUQ.CY_ParentTableCode = "CL";
				nettMassUQ.CY_ParentID = entryLine.PK;
				nettMassUQ.CY_IsOverridden = true;
				using (var control = new GBFECChallengeUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.FECChallengesGrid.ListManager.Refresh();

					AssertEquals("OriginalValueCodeFindBox", false, control.FindSingle<ZCodeFindBox>(x => x.Name == "OriginalValueCodeFindBox").Visible);
					AssertEquals("NewValueCodeFindBox", false, control.FindSingle<ZCodeFindBox>(x => x.Name == "NewValueCodeFindBox").Visible);
					AssertEquals("OriginalValueDropEdit", true, control.FindSingle<ZDropEdit>(x => x.Name == "NewValueDropEdit").Visible);
					AssertEquals("NewValueDropEdit", true, control.FindSingle<ZDropEdit>(x => x.Name == "OriginalValueDropEdit").Visible);
					AssertEquals("OriginalValueCalcEdit", false, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "OriginalValueCalcEdit").Visible);
					AssertEquals("NewValueCalcEdit", false, control.FindSingle<ZArchitecture.ZCalcEdit>(x => x.Name == "NewValueCalcEdit").Visible);
				}
			}
		}
	}
}
