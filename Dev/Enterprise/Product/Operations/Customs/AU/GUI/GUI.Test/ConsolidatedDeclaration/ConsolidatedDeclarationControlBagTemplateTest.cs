using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class ConsolidatedDeclarationControlBagTemplateTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ConsolidatedDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestEntryStyleDropEdit()
		{
			CombineAssertions(() =>
			{
				var entryStyleControl = control.EntryStyleDropEdit;
				AssertEquals("Caption", "Entry Style", entryStyleControl.CaptionResourceString.Caption);
				AssertEquals("Binding", "EntryStyle", entryStyleControl.GetBindingMember());
				AssertType<ZDropEdit>("Type", entryStyleControl);
			});
		}

		public void TestVesselCodeFindBox()
		{
			CombineAssertions(() =>
			{
				var vesselCodeControl = control.VesselCodeFindBox;
				AssertEquals("Caption", "Vessel", vesselCodeControl.CaptionResourceString.Caption);
				AssertEquals("Binding", "VesselName", vesselCodeControl.GetBindingMember());
				AssertType<ZCodeFindBox>("Type", vesselCodeControl);
			});
		}

		public void TestVoyageFlightNoTextBox()
		{
			CombineAssertions(() =>
			{
				var voyageFlightControl = control.VoyageFlightNoTextBox;
				AssertEquals("Caption", "Arrival Flight", voyageFlightControl.CaptionResourceString.Caption);
				AssertEquals("Binding", "VoyageFlightNo", voyageFlightControl.GetBindingMember());
				AssertType<ZTextBox>("Type", voyageFlightControl);
			});
		}

		public void TestPaymentStatusTextBox()
		{
			CombineAssertions(() =>
			{
				var paymentStatusControl = control.PaymentStatusTextBox;
				AssertEquals("Caption", "Customs Pay", paymentStatusControl.CaptionResourceString.Caption);
				AssertEquals("Binding", "PaymentStatus", paymentStatusControl.GetBindingMember());
				AssertType<ZTextBox>("Type", paymentStatusControl);
			});
		}

		public void TestConsolidatedDeclarationDetailsUserControl()
		{
			var consolidatedDeclarationDetailsControl = control.ConsolidatedDeclarationDetailsUserControl;
			AssertEquals("Binding", ".", consolidatedDeclarationDetailsControl.GetBindingMember());
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ConsolidatedDeclarationControlBagTemplate();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ConsolidatedDeclarationControlBagTemplate control;
	}
}
