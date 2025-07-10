using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class MiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestFieldVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			var controlNames = new[]
			{
					"LCPInspectDateEdit",
					"LCPDepartDateEdit",
					"JE_EntryAuthorisationDateDateEdit",
					"JE_RouteFRequestedCheckBox",
					"CheckBoxTraining",
			};

			using (var form = new ZForm(declaration))
			using (var userControl = new MiscOptionsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				foreach (var controlName in controlNames)
				{
					var control = userControl.FindSingle<Control>(controlName);
					AssertNotNull(controlName, control);
					Assert("Should hide in DE export for " + controlName, !control.Visible);
				}

				AssertEquals("Should hide RepresentationDropEdit when the declaration is not Import.", false, userControl.FindSingle<ZDropEdit>("RepresentationDropEdit").Visible);
				AssertEquals("Should hide JE_StatisticStatusDropEdit when the declaration is not Import.", false, userControl.FindSingle<ZDropEdit>("JE_StatisticStatusDropEdit").Visible);
				AssertEquals("Should hide VATClaimBack when !Import", false, userControl.FindSingle<ZDropEdit>("VATClaimBackDropEdit").Visible);

				var supportingInformationTabControl = userControl.FindSingle<ZTabControl>("SupportingInformationTabControl");
				AssertEquals("Should not display additional info tab", false, supportingInformationTabControl.TabPages.ContainsKey("AdditionalInfoTabPage"));
				AssertEquals("Should display supporting document tab", false, supportingInformationTabControl.TabPages.ContainsKey("SupportingDocumentTabPage"));
				AssertEquals("Should display previous document tab", false, supportingInformationTabControl.TabPages.ContainsKey("PreviousDocumentTabPage"));
				AssertEquals("SupportingInformationTabControl visible", false, supportingInformationTabControl.Visible);
				AssertEquals("DeferralGroupBox", false, userControl.FindSingle<ZGroupBox>("DeferralGroupBox").Visible);
			}

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var userControl = new MiscOptionsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				foreach (var controlName in controlNames)
				{
					var control = userControl.FindSingle<Control>(controlName);
					AssertNotNull(controlName, control);
					Assert("Should hide in DE import for " + controlName, !control.Visible);
				}

				AssertEquals("Should display RepresentationDropEdit when the declaration is Import.", true, userControl.FindSingle<ZDropEdit>("RepresentationDropEdit").Visible);
				AssertEquals("Should display JE_StatisticStatusDropEdit when the declaration is Import.", true, userControl.FindSingle<ZDropEdit>("JE_StatisticStatusDropEdit").Visible);
				AssertEquals("Should show VATClaimBack when Import", true, userControl.FindSingle<ZDropEdit>("VATClaimBackDropEdit").Visible);

				var supportingInformationTabControl = userControl.FindSingle<ZTabControl>("SupportingInformationTabControl");
				AssertEquals("Should not display additional info tab", false, supportingInformationTabControl.TabPages.ContainsKey("AdditionalInfoTabPage"));
				AssertEquals("Should display supporting document tab", false, supportingInformationTabControl.TabPages.ContainsKey("SupportingDocumentTabPage"));
				AssertEquals("Should display previous document tab", false, supportingInformationTabControl.TabPages.ContainsKey("PreviousDocumentTabPage"));
				AssertEquals("SupportingInformationTabControl visible", false, supportingInformationTabControl.Visible);
				AssertEquals("DeferralGroupBox", true, userControl.FindSingle<ZGroupBox>("DeferralGroupBox").Visible);
			}
		}
	}
}
