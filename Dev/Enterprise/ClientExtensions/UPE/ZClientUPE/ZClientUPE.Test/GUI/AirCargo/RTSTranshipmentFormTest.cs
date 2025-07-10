using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class RTSTranshipmentFormTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestOnValidOKButtonClick()
		{
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			RTSDetails flagDetails = new RTSDetails(uPECusHAWB);
			using (RTSTranshipmentForm form = new RTSTranshipmentForm(flagDetails))
			{
				uPECusHAWB.CS_ConsigneeCity = "MILAN";
				flagDetails.City = "SPARTA PRAGUE";
				using (flagDetails.GetValidationSuspender())
				{
					form.Show();
					form.DialogResult = DialogResult.OK;
					form.Close();
					AssertEquals("Consignee City should be changed", "SPARTA PRAGUE", uPECusHAWB.CS_ConsigneeCity);
				}
			}
		}
	}
}
