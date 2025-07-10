using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_GuarantorTypeIsConsolidatedDocument()
		{
			const string messageError = "If a guarantee has been lodged, code '1' must be entered.";
			CombineAssertions(() =>
			{
				declaration.ZG_GuarantorType = EmcsGuarantorTypeList.Codes.Transporter;
				AssertNoWarning("No consolidated document: GuarantorType = 2", declaration.ZG_GuarantorTypeInfo, messageError);

				declaration.SetConsolidatedDocument();
				declaration.AddInfoValidation.ValidateZG_GuarantorType();
				AssertHasWarning("Is consolidated document: GuarantorType = 2", declaration.ZG_GuarantorTypeInfo, messageError);

				declaration.ZG_GuarantorType = EmcsGuarantorTypeList.Codes.Consignor;
				AssertNoWarning("Is consolidated document: GuarantorType = 1", declaration.ZG_GuarantorTypeInfo, messageError);
			});
		}

		public void TestCheckZG_TransportArrangementIsConsolidatedDocument()
		{
			const string messageError = "Transport Arrangement must be 1-Consignor for deferred consolidated declarations.";
			CombineAssertions(() =>
			{
				declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignee;
				AssertNoMessageError("No consolidated document: TransportArrangement = 2", declaration.ZG_TransportArrangementInfo, messageError);

				declaration.SetConsolidatedDocument();
				declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignee;
				AssertHasMessageError("Is consolidated document: Transport Arrangement = 2", declaration.ZG_TransportArrangementInfo, messageError);

				declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
				AssertNoMessageError("Is consolidated document: Transport Arrangement = 1", declaration.ZG_TransportArrangementInfo, messageError);
			});
		}

		public void TestCheckZG_DispatchReference()
		{
			declaration.AddInfoValidation.ValidateZG_DispatchReference();
			AssertNoMessageErrors("No message errors", declaration.ZG_DispatchReferenceInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
