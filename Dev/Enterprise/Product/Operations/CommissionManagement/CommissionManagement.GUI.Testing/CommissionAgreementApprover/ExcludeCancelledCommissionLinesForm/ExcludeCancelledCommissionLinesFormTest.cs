using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(ExcludeCancelledCommissionLinesForm))]
	class ExcludeCancelledCommissionLinesFormTest : ZFormBasherTest
	{
		public void TestInitializeControlsForAgreements()
		{
			using (var form = new ExcludeCancelledCommissionLinesFormForTest(Factory, GetAgreementAndCancelledLinesForTests()))
			{
				AssertEquals("3 SubControls should have been created for the 3 agreements", 3, form.SubControls_ExposedForTest.Count);
			}
		}

		public void TestContinueButton_OnClick()
		{
			using (var form = new ExcludeCancelledCommissionLinesFormForTest(Factory, GetAgreementAndCancelledLinesForTests()))
			{
				form.Show();
				form.ContinueButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.OK);
			}
		}

		public void TestUntickAllSubcontrols()
		{
			using (var form = new ExcludeCancelledCommissionLinesFormForTest(Factory, GetAgreementAndCancelledLinesForTests()))
			{
				form.Show();

				foreach (var subControl in form.SubControls_ExposedForTest)
				{
					foreach (ExcludeViewCommissionLine line in subControl.CancelledLines)
					{
						Assert("Line should be ticked.", line.IsExcluded);
					}
				}

				form.UntickAllSubcontrols();

				foreach (var subControl in form.SubControls_ExposedForTest)
				{
					foreach (ExcludeViewCommissionLine line in subControl.CancelledLines)
					{
						Assert("Line should be unticked.", !line.IsExcluded);
					}
				}
			}
		}

		public void TestCancelledLinesToNotReinstate_ShouldReinstateCorrectlySet()
		{
			using (var form = new ExcludeCancelledCommissionLinesFormForTest(Factory, GetAgreementAndCancelledLinesForTests()))
			{
				form.Show();
				form.ContinueButton.PerformClick();

				var lines = Factory.Load<AccCommissionLine>(new ZQuery());
				AssertEquals("3 lines should have been created.", 3, lines.Length);

				foreach (var line in lines)
				{
					AssertEquals("CL0_ShouldReinstate should be false.", false, line.CL0_ShouldReinstate);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ExcludeCancelledCommissionLinesForm(new BusinessObjectFactory(), GetAgreementAndCancelledLinesForTests());
		}

		Dictionary<OrgCommissionAgreement, List<ViewCommissionLine>> GetAgreementAndCancelledLinesForTests()
		{
			var agreementAndCancelledLines = new Dictionary<OrgCommissionAgreement, List<ViewCommissionLine>>();

			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var header1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			header1.CH0_CA0 = agreement1.PK;
			header1.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			header1.CH0_GroupingSourceID = ZGuid.NewZGuid();
			header1.Lines.AddNew();

			var header2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			header2.CH0_CA0 = agreement2.PK;
			header2.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			header2.CH0_GroupingSourceID = ZGuid.NewZGuid();
			header2.Lines.AddNew();

			var header3 = Factory.NewWithValidTestData<AccCommissionHeader>();
			header3.CH0_CA0 = agreement3.PK;
			header3.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			header3.CH0_GroupingSourceID = ZGuid.NewZGuid();
			header3.Lines.AddNew();

			Factory.Save();

			var lines = Factory.Load<ViewCommissionLine>(new ZQuery());

			agreementAndCancelledLines.Add(agreement1, lines.Where(line => line.VCL_CA0 == agreement1.PK).ToList());
			agreementAndCancelledLines.Add(agreement2, lines.Where(line => line.VCL_CA0 == agreement2.PK).ToList());
			agreementAndCancelledLines.Add(agreement3, lines.Where(line => line.VCL_CA0 == agreement3.PK).ToList());

			return agreementAndCancelledLines;
		}

		#endregion
	}

	class ExcludeCancelledCommissionLinesFormForTest : ExcludeCancelledCommissionLinesForm
	{
		public ExcludeCancelledCommissionLinesFormForTest(BusinessObjectFactory factory, Dictionary<OrgCommissionAgreement, List<ViewCommissionLine>> agreementsAndCancelledLines)
			: base(factory, agreementsAndCancelledLines) { }

		public List<AgreementCancelledCommissionsControl> SubControls_ExposedForTest => SubControls;
	}
}
