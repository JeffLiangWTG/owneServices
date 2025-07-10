using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EdiCommissionAgreementCustomizationForm))]
	class EdiCommissionAgreementCustomizationFormTest : ZFormBasherTest
	{
		#region Form Caption
		public void TestFormCaption()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			using (var form = new EdiCommissionAgreementCustomizationForm(customization))
			{
				AssertEquals("Customer Filters", form.FormCaption);
			}
		}

		#endregion
		#region ReadOnly
		public void TestReadOnly()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			using (var form = new EdiCommissionAgreementCustomizationForm(customization))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.Browse;
				form.Show();
				var treeControl = (EdiCommissionAgreementCustomizationTreeControl)form.Controls.Find("customizationTreeControl", true)[0];
				AssertEquals(false, treeControl.ReadOnly);
				form.ReadOnly = true;
				AssertEquals(true, treeControl.ReadOnly);
			}
		}

		#endregion
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var commissionAgreement = Factory.New<EdiCommissionAgreement>();
			var customization = commissionAgreement.GetOrCreateCustomization();
			return new EdiCommissionAgreementCustomizationForm(customization);
		}
		#endregion
	}
}
