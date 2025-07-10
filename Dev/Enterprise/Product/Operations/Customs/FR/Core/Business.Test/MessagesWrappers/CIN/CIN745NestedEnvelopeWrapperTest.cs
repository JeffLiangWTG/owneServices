using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN.Testing
{
	public class CIN745NestedEnvelopeWrapperTest : TestCaseWithFactory
	{
		public void TestWrapperValues()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.Add(cusEntryHeader);
			var nestedEnvelopeWrapper = new CIN745NestedEnvelopeWrapper(cusEntryHeader);
			AssertEquals(CIN745NestedEnvelopeWrapper.oaciCode, nestedEnvelopeWrapper.OACI);
			AssertEquals(CIN745NestedEnvelopeWrapper.oaciDest, nestedEnvelopeWrapper.DEST_OACI);
			var cusOffice = cusEntryHeader.Declaration.CustomsOffices.AddNew();
			cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			cusOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
			cusOffice.CY_Data = "Test";
			var cusEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			AssertEquals(cusEntryHeader.CustomOffice, nestedEnvelopeWrapper.BUR_DOUANE);
			AssertEquals(cusEntryNumber.CE_EntryNum, nestedEnvelopeWrapper.MRN_ECS);

			ZString result = ZString.Empty;
			ZString partQuant = ZDateTime.Now.DayOfYear.ToString("000", CultureInfo.InvariantCulture);
			ZString counter = FRConstants.MessageSpecialCharacter.MessageID;
			result = string.Format(CultureInfo.InvariantCulture, "{0}{1}", partQuant, counter);

			AssertEquals(result, nestedEnvelopeWrapper.REFERENCE);
			var supplier = Factory.New<OrgHeader>();
			cusEntryHeader.Declaration.JE_OH_Supplier = supplier.PK;

			var cTO = Factory.New<OrgHeader>();
			var addressCTO = cTO.Addresses.AddNew();
			cusEntryHeader.Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = addressCTO.PK;

			var customCodeCTO = cTO.CustomsCodes.AddNew();
			customCodeCTO.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodeCTO.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodeCTO.OK_CustomsRegNo = "FR123456800";
			customCodeCTO.OK_OA_PremisesAddress = addressCTO.PK;

			AssertEquals("only CTO => MAGASIN should be CTO OK_CustomsRegNo value link to OK_OA_PremisesAddress", "FR123456800", nestedEnvelopeWrapper.MAGASIN);

			customCodeCTO.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertEquals("only CTO => MAGASIN should be CTO OK_CustomsRegNo value link to first CIN", "FR123456800", nestedEnvelopeWrapper.MAGASIN);

			customCodeCTO.OK_OA_PremisesAddress = addressCTO.PK;

			var depot = Factory.New<OrgHeader>();
			var addressDepot = depot.Addresses.AddNew();
			cusEntryHeader.Declaration.DepotDocAddress.E2_OA_Address = addressDepot.PK;

			var customCodedepot = depot.CustomsCodes.AddNew();
			customCodedepot.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodedepot.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodedepot.OK_CustomsRegNo = "FR123456799";
			customCodedepot.OK_OA_PremisesAddress = addressDepot.PK;
			AssertEquals("Depot is present => MAGASIN should be depot OK_CustomsRegNo value link to OK_OA_PremisesAddress", "FR123456799", nestedEnvelopeWrapper.MAGASIN);

			var customCode = cusEntryHeader.Declaration.Supplier.CustomsCodes.AddNew();
			customCode.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCode.OK_CustomsRegNo = "FR123456798";
			AssertEquals(cusEntryHeader.Declaration.Supplier.CustomsCodes.GetCustomsRegNo(CusEntryHeader.Schema.CINOACICode, Core.Constants.CountryCodes.France), nestedEnvelopeWrapper.OACI_Shipper);

			var shipper = Factory.New<OrgHeader>();
			cusEntryHeader.Declaration.JE_OH_ShippingLine = shipper.PK;

			var customCode1 = cusEntryHeader.Declaration.ShippingLine.CustomsCodes.AddNew();
			customCode1.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCode1.OK_CustomsRegNo = "FR123456798";
			AssertEquals(cusEntryHeader.Declaration.ShippingLine.CustomsCodes.GetCustomsRegNo(CusEntryHeader.Schema.CINOACICode, Core.Constants.CountryCodes.France), nestedEnvelopeWrapper.OACI_Carrier);
		}

		public void TestMagasin()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.Add(cusEntryHeader);
			var nestedEnvelopeWrapper = new CIN745NestedEnvelopeWrapper(cusEntryHeader);

			var cTO2 = Factory.New<OrgHeader>();
			var addressCTO2 = cTO2.Addresses.AddNew();
			cusEntryHeader.Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = addressCTO2.PK;

			var customCodeCTO2 = cTO2.CustomsCodes.AddNew();
			customCodeCTO2.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodeCTO2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodeCTO2.OK_CustomsRegNo = "FR123456800";
			customCodeCTO2.OK_OA_PremisesAddress = addressCTO2.PK;

			var depot2 = Factory.New<OrgHeader>();
			var addressDepot2 = depot2.Addresses.AddNew();
			cusEntryHeader.Declaration.DepotDocAddress.E2_OA_Address = addressDepot2.PK;
			var customCodedepot2 = depot2.CustomsCodes.AddNew();
			customCodedepot2.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodedepot2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodedepot2.OK_CustomsRegNo = "FR123456801";
			customCodedepot2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertEquals("Depot is present but no premise adress, CTO is present with premise => MAGASIN should be CTO OK_CustomsRegNo value link to OK_OA_PremisesAddress", "FR123456800", nestedEnvelopeWrapper.MAGASIN);

			customCodeCTO2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertEquals("Depot is present but no premise adress, CTO is present but no premise address => MAGASIN should be depot OK_CustomsRegNo value link to first CIN", "FR123456801", nestedEnvelopeWrapper.MAGASIN);
		}
	}
}
