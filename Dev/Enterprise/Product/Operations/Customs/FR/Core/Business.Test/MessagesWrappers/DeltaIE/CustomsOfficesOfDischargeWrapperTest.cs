using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CustomsOfficeOfDischargeWrapperTest : DataProviderTestCase<CustomsOfficesOfDischargeWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal declaration customsOffice with role = DIS.", "FR000001", Provider.ReferenceNumber);
		}

		protected override CustomsOfficesOfDischargeWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			var customsOfficeOfDischarge = declaration.CustomsOffices.AddNew();
			customsOfficeOfDischarge.CY_Code = FrOfficeCodesTypes.Codes.OfficeOfDischarge;
			customsOfficeOfDischarge.CY_Data = "FR000001";
			return CustomsOfficesOfDischargeWrapper.New(declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == FrOfficeCodesTypes.Codes.OfficeOfDischarge));
		}
	}
}
