using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class ImportMethodFiveToSixUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobComInvoiceHeader), control.BindingSource.DataSourceType);
			AssertEquals("AmountAgreedUponWithCustomsKRWCalcEdit Binding", "AmountAgreedUponWithCustomsKRW", control.FindSingle<ZCalcEdit>("AmountAgreedUponWithCustomsKRWCalcEdit").BindTo);
			AssertEquals("AdditionalCostFreightToArrivalPortCalcEdit Binding", "AdditionalCostFreightToArrivalPort", control.FindSingle<ZCalcEdit>("AdditionalCostFreightToArrivalPortCalcEdit").BindTo);
			AssertEquals("AdditionalCostFreightToDeparturePortCalcEdit Binding", "AdditionalCostFreightToDeparturePort", control.FindSingle<ZCalcEdit>("AdditionalCostFreightToDeparturePortCalcEdit").BindTo);
			AssertEquals("AdditionalCostInsuranceCalcEdit Binding", "AdditionalCostInsurance", control.FindSingle<ZCalcEdit>("AdditionalCostInsuranceCalcEdit").BindTo);
			AssertEquals("AdditionalCostTotalAdditionalAmountCalcEdit Binding", "AdditionalCostTotalAdditionalAmount", control.FindSingle<ZCalcEdit>("AdditionalCostTotalAdditionalAmountCalcEdit").BindTo);

			control.BindToMessageSendingObject();
			AssertEquals(typeof(Business.ValuationDeclarationMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("AmountAgreedUponWithCustomsKRWCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AmountAgreedUponWithCustomsKRW", control.FindSingle<ZCalcEdit>("AmountAgreedUponWithCustomsKRWCalcEdit").BindTo);
			AssertEquals("AdditionalCostFreightToArrivalPortCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalCostFreightToArrivalPort", control.FindSingle<ZCalcEdit>("AdditionalCostFreightToArrivalPortCalcEdit").BindTo);
			AssertEquals("AdditionalCostFreightToDeparturePortCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalCostFreightToDeparturePort", control.FindSingle<ZCalcEdit>("AdditionalCostFreightToDeparturePortCalcEdit").BindTo);
			AssertEquals("AdditionalCostInsuranceCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalCostInsurance", control.FindSingle<ZCalcEdit>("AdditionalCostInsuranceCalcEdit").BindTo);
			AssertEquals("AdditionalCostTotalAdditionalAmountCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalCostTotalAdditionalAmount", control.FindSingle<ZCalcEdit>("AdditionalCostTotalAdditionalAmountCalcEdit").BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ImportMethodFiveToSixUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ImportMethodFiveToSixUserControl control;
	}
}
