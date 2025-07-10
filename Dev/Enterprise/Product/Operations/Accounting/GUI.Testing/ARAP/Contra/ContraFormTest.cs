using System;
using System.Windows.Forms;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Contra.Testing
{
	[TestedType(typeof(ContraForm))]
	class ContraFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ContraForm(Business.ARAP.Contra.New(Factory));
		}

		protected override bool AllowFormSizeFixed => true;

		public void TestShowSetsFactoryContext()
		{
			var contra = Business.ARAP.Contra.New(Factory);
			contra.IsReverseTransaction = true;
			using (ContraForm contraForm = new ContraForm(contra))
			{
				contraForm.DisplayMode = ODisplayMode.Delete;
				contraForm.Show();
				contraForm.OnShown_ForTestOnly(new EventArgs());
				Assert("The Factory should contain BusinessContext.ReverseDateForm", contraForm.BusinessEntity.Factory.HasContext(BusinessContext.ReverseDateForm));
			}
		}

		public void TestFormBorderStyle()
		{
			var contra = Business.ARAP.Contra.New(Factory);
			contra.IsReverseTransaction = true;
			using (ContraForm contraForm = new ContraForm(contra))
			{
				AssertEquals("Should be the default value",FormBorderStyle.Sizable, contraForm.FormBorderStyle);
			}
		}

		protected override bool ShouldHaveAuditPlugIn => true;
	}
}
