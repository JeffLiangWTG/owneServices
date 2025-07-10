using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class ShipmentTypeUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestEntryStyleDropEdit()
		{
			control.AssertContainsControl<ZDropEdit>(nameof(control.EntryStyleDropEdit), x => x
				.WithBindTo(nameof(JobDeclaration.JE_EntryStyle))
			);
		}

		public void TestCTStatusIDDropEdit()
		{
			control.AssertContainsControl<ZDropEdit>(nameof(control.CTStatusIDDropEdit), x => x
				.WithBindTo(nameof(JobDeclaration.ZG_CTStatusID))
			);
		}

		public void TestSecurityDropEdit()
		{
			control.AssertContainsControl<ZDropEdit>(nameof(control.SecurityDropEdit), x => x
				.WithBindTo(nameof(JobDeclaration.ZG_TypeOfSecurity))
			);
		}

		public void TestSpecificCircumstanceDropEdit()
		{
			control.AssertContainsControl<ZDropEdit>(nameof(control.SpecificCircumstanceDropEdit), x => x
				.WithBindTo(nameof(JobDeclaration.ZG_SpecificCircumstanceIndicator))
			);
		}

		public void TestIsHighValueOvrdCheckBox()
		{
			control.AssertContainsControl<ZCheckBox>(nameof(control.IsHighValueOvrdCheckBox),x => x
				.WithBindTo(nameof(JobDeclaration.ZG_IsHighValueOvrd))
			);
		}

		public void TestBorderTransportMeansDropEdit()
		{
			control.AssertContainsControl<ZDropEdit>(nameof(control.BorderTransportMeansDropEdit), x => x
				.WithBindTo(nameof(JobDeclaration.ZG_BorderTransportMeans))
			);
		}

		public void TestIsSecurityDeclarationCheckBox()
		{
			var isSecurityDeclarationCheckBox = control.IsSecurityDeclarationCheckBox;
			CombineAssertions(() =>
			{
				control.AssertContainsControl<ZCheckBox>(nameof(control.IsSecurityDeclarationCheckBox), x => x
					.WithBindTo(nameof(JobDeclaration.ZG_IsSecurityDeclaration))
				);
				AssertEquals("CheckAlign", ZContentAlignment.Right, isSecurityDeclarationCheckBox.CheckAlign);
				AssertEquals("TextAlign", System.Drawing.ContentAlignment.MiddleRight, isSecurityDeclarationCheckBox.TextAlign);
			});
		}

		ShipmentTypeUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentTypeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
