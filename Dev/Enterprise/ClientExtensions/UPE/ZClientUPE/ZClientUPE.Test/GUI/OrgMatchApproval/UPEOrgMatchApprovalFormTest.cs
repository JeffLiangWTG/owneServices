using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Tests;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(UPEOrgMatchApprovalForm))]
	public class UPEOrgMatchApprovalFormTest : OrgMatchApprovalFormBasherTest
	{
		public void TestOwnerCodeLabelAndGridColumnRenamedToAccountNum()
		{
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyBusinessObject dummyParent = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyOrgMatchApproval matchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);
			matchApproval.IsCurrentUserSupervisorOverride = true;
			using (TestUPEOrgMatchApprovalForm form = new TestUPEOrgMatchApprovalForm(matchApproval))
			{
				form.Show();
				Application.DoEvents();
				ZGridColumnInfo ownerCodeColumn = form.SimilarOrgMatchesModuleButtonGrid.InnerGrid.GetColumnStyle(SimilarOrgMatchForApproval.Schema.OwnerCode);
				AssertEquals("Label caption should be renamed", "Account #", form.OwnerCodeBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Grid column caption should be renamed", "Account #", ownerCodeColumn.Caption);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override Form GetFormToBashCore()
		{
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyBusinessObject dummyParent = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyOrgMatchApproval matchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);
			matchApproval.IsCurrentUserSupervisorOverride = true;
			// in real life, the entity is saved and the form is shown for editing only
			Factory.Save();
			return new UPEOrgMatchApprovalForm(matchApproval);
		}

		class TestUPEOrgMatchApprovalForm : UPEOrgMatchApprovalForm
		{
			public TestUPEOrgMatchApprovalForm(OrgMatchApproval businessEntity) : base(businessEntity)
			{
			}

			public new ZTextBox OwnerCodeBoundTextBox
			{
				get
				{
					return base.OwnerCodeBoundTextBox;
				}
			}

			public new ZModuleButtonGrid SimilarOrgMatchesModuleButtonGrid
			{
				get
				{
					return base.SimilarOrgMatchesModuleButtonGrid;
				}
			}
		}
	}
}
