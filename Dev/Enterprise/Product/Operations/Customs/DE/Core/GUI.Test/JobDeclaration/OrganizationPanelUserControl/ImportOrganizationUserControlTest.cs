using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class ImportOrganizationUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibility()
		{
			using (var control = new ImportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("ManufacturerAddressControl", false, control.FindSingle<Control>("ManufacturerAddressControl").Visible);
					AssertEquals("BuyingAgentAddressControl", true, control.FindSingle<Control>("BuyingAgentAddressControl").Visible);
					AssertEquals("BuyerAddressControl", true, control.FindSingle<Control>("BuyerAddressControl").Visible);
					AssertEquals("AcquirerDocAddress", true, control.FindSingle<Control>("AcquirerDocAddress").Visible);
				});
			}
		}

		public void TestControlNames()
		{
			using (var control = new ImportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("RepresentativeAddressControl", "[14] Representative", control.FindSingle<ZAddressControl>("RepresentativeAddressControl").CaptionResourceString.Caption);
					AssertEquals("BuyingAgentAddressControl", "Represented Party", control.FindSingle<ZAddressControl>("BuyingAgentAddressControl").CaptionResourceString.Caption);
					AssertEquals("BuyerAddressControl", "Buyer", control.FindSingle<ZAddressControl>("BuyerAddressControl").CaptionResourceString.Caption);
					AssertEquals("AcquirerDocAddress", "Acquirer", control.FindSingle<ZDocAddressControl>("AcquirerDocAddress").CaptionResourceString.Caption);
				});
			}
		}

		public void TestDefermentPartyAddressControlCaption()
		{
			using (var control = new ImportOrganizationUserControl())
			{
				var defermentAddressControlLabelRenderer = control.FindSingle<ZDocAddressControl>("DefermentPartyDocAddressControl").GetExtension<ILabelCaptionRenderer>();
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("correctly initialized", "Payment Party", defermentAddressControlLabelRenderer.Caption);

					declaration.ZG_MethodOfPayment = Business.UniversalReferenceConstants.MethodOfPaymentTypes.E;
					AssertEquals("deferment", "Deferment Party", defermentAddressControlLabelRenderer.Caption);

					declaration.ZG_MethodOfPayment = Business.UniversalReferenceConstants.MethodOfPaymentTypes.A;
					AssertEquals("no deferment", "Payment Party", defermentAddressControlLabelRenderer.Caption);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
