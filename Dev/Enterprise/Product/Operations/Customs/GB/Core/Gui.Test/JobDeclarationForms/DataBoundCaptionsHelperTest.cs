using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	internal class DataBoundCaptionsHelperTest : TestCaseWithFactory
	{
		class TestUserControl : ZUserControl, ISupportMultipleResourceStringDataSupporter
		{
			public const string TestFieldName = "JE_RL_NKFinalDestination";

			public TestUserControl(JobDeclaration jobDeclaration)
			{
				JobDeclaration = jobDeclaration;
				TestControl = new ZDropEdit();
				Controls.Add(TestControl);
				TestGrid = new ZGridForTest();
				Controls.Add(TestGrid);
				CaptionRenderingEnabled = true;
				BindingSource.SetBindingMember(TestControl, TestFieldName);
				BindingSource.SetDataBinding(JobDeclaration, "");
			}

			public JobDeclaration JobDeclaration { get; }

			public ZDropEdit TestControl { get; }

			public ZGridForTest TestGrid { get; }

			public ISupportMultipleResourceStringData SupportMultipleResourceStringData => JobDeclaration;
		}

		class ZGridForTest : ZGrid
		{
			public bool IsTableStylesRefreshed;

			protected override void RefreshTableStylesCore()
			{
				IsTableStylesRefreshed = true;
				base.RefreshTableStylesCore();
			}
		}

		public void TestRefreshControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new TestUserControl(declaration))
			{
				form.Controls.Add(control);
				var captionCds = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), TestUserControl.TestFieldName, new[] { JobDeclaration.MultipleKeyCdsImport })?.Caption;
				var captionChief = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), TestUserControl.TestFieldName, new[] { JobDeclaration.MultipleKeyChief })?.Caption;
				CombineAssertions(() =>
				{
					AssertNotNull($"Pre-requisite: Test field ({TestUserControl.TestFieldName}) must have caption for CDS", captionCds);
					AssertNotNull($"Pre-requisite: Test field ({TestUserControl.TestFieldName}) must have caption for CHIEF", captionChief);
					AssertNotEquals($"Pre-requisite: Test field ({TestUserControl.TestFieldName}) must have different captions for CDS vs CHIEF", captionCds, captionChief);
				});

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_MessageType = "IMP";
				control.RefreshControlCaptions();
				AssertEquals(captionCds, control.TestControl.GetExtension<ILabelCaptionRenderer>().Caption);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
				control.RefreshControlCaptions();
				AssertEquals(captionChief, control.TestControl.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestRefreshColumnCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new TestUserControl(declaration))
			{
				var column = new ZDropEditColumnStyleInfo(TestUserControl.TestFieldName, 10, 1);
				control.TestGrid.ColumnStyles.Add(column);
				form.Controls.Add(control);
				var captionCds = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), TestUserControl.TestFieldName, new[] { JobDeclaration.MultipleKeyCdsImport })?.Caption;
				var captionChief = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), TestUserControl.TestFieldName, new[] { JobDeclaration.MultipleKeyChief })?.Caption;
				CombineAssertions(() =>
				{
					AssertNotNull($"Pre-requisite: Test field ({TestUserControl.TestFieldName}) must have caption for CDS", captionCds);
					AssertNotNull($"Pre-requisite: Test field ({TestUserControl.TestFieldName}) must have caption for CHIEF", captionChief);
					AssertNotEquals($"Pre-requisite: Test field ({TestUserControl.TestFieldName}) must have different captions for CDS vs CHIEF", captionCds, captionChief);
				});

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_MessageType = "IMP";
				control.TestGrid.RefreshColumnCaptions(typeof(JobDeclaration), declaration.MultipleKeysToUse);
				AssertEquals(captionCds, column.Caption);
				AssertEquals(false, control.TestGrid.IsTableStylesRefreshed);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
				control.TestGrid.RefreshColumnCaptions(typeof(JobDeclaration), declaration.MultipleKeysToUse);
				AssertEquals(captionChief, column.Caption);
				AssertEquals(false, control.TestGrid.IsTableStylesRefreshed);
			}
		}
	}
}
