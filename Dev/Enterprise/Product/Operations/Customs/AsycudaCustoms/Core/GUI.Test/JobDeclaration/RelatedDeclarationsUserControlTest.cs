using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AsycudaCustoms.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class RelatedDeclarationsUserControlTest : TestCaseWithFactory
	{
		public void TestJE_MessageSubTypeAvailability_Default()
		{
			using (var control = new RelatedDeclarationsUserControl())
			{
				AssertEquals(true, control.RelatedDeclarationsGrid.GetColumnStyle(Customs.Business.AutoJobDeclaration.Schema.JE_MessageSubType).IsUnavailable);
			}
		}

		public void TestJE_DeclarationType_Default()
		{
			using (var control = new RelatedDeclarationsUserControl())
			{
				AssertEquals(true, control.RelatedDeclarationsGrid.GetColumnStyle(JobDeclaration.Schema.JE_DeclarationType).IsUnavailable);
			}
		}

		public void TestJE_DeclarationTypeAvailability_AreMultipleEntryInstructionsAllowed_False()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Setup(m => m.AreMultipleEntryInstructionsAllowed).Returns(false);
			using (var control = new RelatedDeclarationsUserControl())
			{
				control.JobDeclaration = declarationMock.Object;
				CombineAssertions(() =>
				{
					AssertEquals("IsUnavailable", false, control.RelatedDeclarationsGrid.GetColumnStyle(JobDeclaration.Schema.JE_DeclarationType).IsUnavailable);
					AssertEquals("IsVisible", true, control.RelatedDeclarationsGrid.GetColumnStyle(JobDeclaration.Schema.JE_DeclarationType).IsVisible);
				});
			}
		}

		public void TestJE_DeclarationTypeAvailability_AreMultipleEntryInstructionsAllowed_True()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Setup(m => m.AreMultipleEntryInstructionsAllowed).Returns(true);

			using (var control = new RelatedDeclarationsUserControl())
			{
				control.JobDeclaration = declarationMock.Object;
				AssertEquals(true, control.RelatedDeclarationsGrid.GetColumnStyle(JobDeclaration.Schema.JE_DeclarationType).IsUnavailable);
			}
		}

		public void TestJE_DeclarationType_ColumnDetails()
		{
			using (var control = new RelatedDeclarationsUserControl())
			{
				var declarationTypeColumnStyle = control.RelatedDeclarationsGrid.GetColumnStyle(JobDeclaration.Schema.JE_DeclarationType);
				CombineAssertions(() =>
				{
					AssertEquals("Width", 105, declarationTypeColumnStyle.Width);
					AssertEquals("Caption", "Declaration Type", declarationTypeColumnStyle.CaptionResourceString.Caption);
					AssertEquals("IsReadOnly", true, declarationTypeColumnStyle.IsReadOnly);
				});
			}
		}
	}
}
