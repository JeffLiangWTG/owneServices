using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(BorderTransport))]
class BorderTransportValidationTest : CusCodeDataTest<BorderTransport>
{
	public void TestCheckCY_Code_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(borderTransport.CY_CodeInfo, "~", TransportTypeIdList.Codes._10);
	}

	public void TestCheckCY_Code_ListValidationFiltered()
	{
		var dec = Factory.New<JobDeclaration>();
		var borderTransport = dec.BorderTransports.AddNew();

		dec.JE_TransportModeInland = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertInvalidCodeMessageError(borderTransport.CY_CodeInfo, TransportTypeIdList.Codes._20, TransportTypeIdList.Codes._10);

		dec.JE_TransportModeInland = TransportTypeList.Codes.Rail;
		ValidationTestHelper.AssertInvalidCodeMessageError(borderTransport.CY_CodeInfo, TransportTypeIdList.Codes._10, TransportTypeIdList.Codes._20);

		dec.JE_TransportModeInland = TransportTypeList.Codes.Road;
		ValidationTestHelper.AssertInvalidCodeMessageError(borderTransport.CY_CodeInfo, TransportTypeIdList.Codes._10, TransportTypeIdList.Codes._30);

		dec.JE_TransportModeInland = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertInvalidCodeMessageError(borderTransport.CY_CodeInfo, TransportTypeIdList.Codes._10, TransportTypeIdList.Codes._40);

		dec.JE_TransportModeInland = TransportTypeList.Codes.InlandWaterwayTransport;
		ValidationTestHelper.AssertInvalidCodeMessageError(borderTransport.CY_CodeInfo, TransportTypeIdList.Codes._10, TransportTypeIdList.Codes._80);
	}

	public void TestCheckCY_Code()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(borderTransport.CY_CodeInfo);
	}

	public void TestCheckCY_Data()
	{
		var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "1000";

		dec.JE_TransportMode = TransportTypeList.Codes.Mail;
		borderTransport.CY_Code = TransportTypeIdList.Codes._21;
		borderTransport.CY_Data = "Border Transport";
		AssertHasMessageErrorContaining(borderTransport.CY_DataInfo, "[21] Transport ID must be left empty");
		dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
		AssertHasMessageErrorContaining(borderTransport.CY_DataInfo, "[21] Transport ID must be left empty");
		dec.JE_TransportMode = TransportTypeList.Codes.Air;
		borderTransport.CY_Code = TransportTypeIdList.Codes._20;
		borderTransport.CY_Data = "Border Transport 1";
		AssertNoMessageError(borderTransport.CY_DataInfo, "[21] Transport ID must be left empty");
		dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
		borderTransport.CY_Data = ZString.Empty;
		AssertNoMessageError(borderTransport.CY_DataInfo, "[21] Transport ID must be left empty");

		invoiceLine.JI_Procedure = "2100";

		dec.JE_TransportMode = TransportTypeList.Codes.Mail;
		borderTransport.CY_Code = TransportTypeIdList.Codes._21;
		borderTransport.CY_Data = "ABC";
		AssertHasMessageErrorContaining(borderTransport.CY_DataInfo, "[21] Transport type must be left empty");
		dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
		AssertHasMessageErrorContaining(borderTransport.CY_DataInfo, "[21] Transport type must be left empty");
		dec.JE_TransportMode = TransportTypeList.Codes.Air;
		borderTransport.CY_Code = TransportTypeIdList.Codes._20;
		borderTransport.CY_Data = "Border Transport 1";
		AssertNoMessageError(borderTransport.CY_DataInfo, "[21] Transport type must be left empty");
		dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
		borderTransport.CY_Data = ZString.Empty;
		AssertNoMessageError(borderTransport.CY_DataInfo, "[21] Transport type must be left empty");
	}

	public void TestCheckNationality_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(borderTransport.NationalityInfo, "~", Core.Constants.CountryCodes.Netherlands);
	}

	public void TestCheckNationality()
	{
		borderTransport.CY_Data = "Border transport";
		ValidationTestHelper.AssertErrorIfNotEntered(borderTransport.NationalityInfo);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		return declaration.BorderTransports.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		dec = Factory.New<JobDeclaration>();
		borderTransport = dec.BorderTransports.AddNew();
	}
	JobDeclaration dec;
	BorderTransport borderTransport;
}
