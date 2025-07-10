using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class SpecialProceduresUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		[RequiresSTA]
		public void TestDynamicSpecialProceduresPanel()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AutoScroll", true, control.DynamicSpecialProceduresPanel.AutoScroll);
				AssertEquals("Dock", DockStyle.Fill, control.DynamicSpecialProceduresPanel.Dock);
			});
		}

		SpecialProceduresUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new SpecialProceduresUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
