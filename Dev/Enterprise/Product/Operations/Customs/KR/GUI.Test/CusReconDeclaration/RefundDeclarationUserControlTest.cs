using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusReconDeclaration = Enterprise.Customs.KR.Business.CusReconDeclaration;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundDeclarationUserControl))]
	sealed class RefundDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestDeclarationTab()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			using (var reconDeclarationForm = new CusReconDeclarationFormForTest(reconDeclaration))
			{
				reconDeclarationForm.Show();

				var payerGroupBox = reconDeclarationForm.FindSingle<ZGroupBox>("PayerGroupBox");
				AssertNotNull(payerGroupBox);
				AssertNotNull(payerGroupBox.FindSingle<DynamicLayoutPanel>("PayerPanel"));

				var customsDetailsGroupBox = reconDeclarationForm.FindSingle<ZGroupBox>("CustomsDetailsGroupBox");
				AssertNotNull(customsDetailsGroupBox);
				AssertNotNull(customsDetailsGroupBox.FindSingle<DynamicLayoutPanel>("CustomsDetailsPanel"));

				var refundDeclarationDetailsGroupBox = reconDeclarationForm.FindSingle<ZGroupBox>("RefundDeclarationDetailsGroupBox");
				AssertNotNull(refundDeclarationDetailsGroupBox);
				AssertNotNull(refundDeclarationDetailsGroupBox.FindSingle<DynamicLayoutPanel>("RefundDeclarationDetailsPanel"));
			}
		}

		sealed class CusReconDeclarationFormForTest : RefundDeclarationForm
		{
			public CusReconDeclarationFormForTest(CusReconDeclaration reconDeclaration) : base(reconDeclaration)
			{
			}
		}
	}
}
