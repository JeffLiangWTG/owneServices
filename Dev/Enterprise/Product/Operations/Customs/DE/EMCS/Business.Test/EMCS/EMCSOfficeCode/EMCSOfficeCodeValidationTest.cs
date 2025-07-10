using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data_IfOfficeOfDispatchAndDeclarationIsConsolidatedDocument()
		{
			const string messageError = "The entered Office Of Dispatch (DIS) has to be a German Customs Office";
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.ZG_DeferredSubmission = EmcsDeferredSubmissionList.Codes.Nein;
			var office = declaration.CustomsOffices.AddNew();
			CombineAssertions(() =>
			{
				office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDispatch;
				office.CY_Data = Core.Constants.CountryCodes.France + "123456";
				AssertNoMessageError("OfficeOfDispatch only: French office", office.CY_DataInfo, messageError);

				declaration.SetConsolidatedDocument();
				office.Validation.ValidateCY_Data();
				AssertHasMessageError("OfficeOfDispatch AND Is consolidated document: French office", office.CY_DataInfo, messageError);

				office.CY_Data = Core.Constants.CountryCodes.Germany + "123456";
				office.Validation.ValidateCY_Data();
				AssertNoMessageError("OfficeOfDispatch AND Is consolidated document: German office", office.CY_DataInfo, messageError);
			});
		}
	}
}
