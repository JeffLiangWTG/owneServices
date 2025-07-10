using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Codes = Enterprise.Customs.ES.Manifest.H7.Business.ESH7AdditionalProcedureCodeList.Codes;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

[TestedType(typeof(SupportingDocumentValidation))]
sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestSupportingDocumentType_AltaH7VxEntForESH7_AcceptedSupportingDocumentType()
	{
		CombineAssertions(() =>
		{
			AssertAcceptedTypeError(procedures07, acceptedTypeForProcedure07, acceptedTypeMessageForProcedure07);
			AssertAcceptedTypeError([Codes.C08], acceptedTypeForProcedure08, acceptedTypeMessageForProcedure08);
			AssertAcceptedTypeError(procedures163536, acceptedTypeForProcedure163536, acceptedTypeMessageForProcedure163536);
		});
	}

	void AssertAcceptedTypeError(List<string> procedureCodes, List<string> acceptedTypes, string erroMsg)
	{
		foreach(var procedureCode in procedureCodes)
		{
			bill.ABL_Procedure = procedureCode;
			document.CSI_Code = "1234";
			AssertHasMessageError(document.CSI_CodeInfo, erroMsg);
			foreach (var documentType in acceptedTypes)
			{
				document.CSI_Code = documentType;
				AssertNoMessageError(document.CSI_CodeInfo, erroMsg);
			}
		}
	}

	public void TestSupportingDocumentType_AltaH7VxEntForESH7_RequireSupportingDocumentType1018()
	{
		CombineAssertions(() =>
		{
			Assert1018TypeIsRequired(procedures0708163536, agentTypesDIRICA, () => { });
			Assert1018TypeIsRequired(procedures070816, agentTypesINDDCA, ValidateWithCustomsOffice);
		});

		void ValidateWithCustomsOffice()
		{
			bill.Header.AMA_CustomsOffice = "ES005511";
			document.Validation.ValidateCSI_Code();
			AssertNoMessageError("no need to validate 1018 code type given customsoffice starts with ES0055", document.CSI_CodeInfo, requireSupportingDocumentType1018Message);

			bill.Header.AMA_CustomsOffice = "ES005611";
			document.Validation.ValidateCSI_Code();
			AssertNoMessageError("no need to validate 1018 code type given customsoffice starts with ES0055", document.CSI_CodeInfo, requireSupportingDocumentType1018Message);

			bill.Header.AMA_CustomsOffice = "ES006511";
			document.Validation.ValidateCSI_Code();
		}
	}

	void Assert1018TypeIsRequired(List<string> procedures, List<string> agentTypes, Action validateWithCustomsOffice)
	{
		foreach (var procedure in procedures)
		{
			bill.ABL_Procedure = procedure;
			foreach (var agentType in agentTypes)
			{
				bill.Header.AMA_AgentType = agentType;
				bill.ABL_ConsigneeRegNo = "123456789";
				document.CSI_Code = "1234";
				validateWithCustomsOffice();
				AssertHasMessageError("A supporting document with 1018 CSI_Code is required", document.CSI_CodeInfo, requireSupportingDocumentType1018Message);

				document.CSI_Code = "1018";
				AssertNoMessageError("no message error given CSI_Code is 1018", document.CSI_CodeInfo, requireSupportingDocumentType1018Message);

				bill.ABL_ConsigneeRegNo = "89890029A";
				document.CSI_Code = "1234";
				AssertNoMessageError("no need to validate 1018 code type given consigneeRegNo is 89890029A", document.CSI_CodeInfo, requireSupportingDocumentType1018Message);
			}

			bill.Header.AMA_AgentType = "ABC";
			bill.ABL_ConsigneeRegNo = "123456789";
			document.Validation.ValidateCSI_Code();
			AssertNoMessageError("no need to validate 1018 code type given agentType is not DIR or ICA", document.CSI_CodeInfo, requireSupportingDocumentType1018Message);
		}

		bill.ABL_Procedure = "123";
		document.Validation.ValidateCSI_Code();
		AssertNoMessageError("no need to validate 1018 code type given procedure code is not in the list", document.CSI_CodeInfo, requireSupportingDocumentType1018Message);
	}

	public void TestSupportingDocumentType_AltaH7VxEntForESH7_RequiredTypesForProcedure07()
	{
		foreach (var procedure in procedures07)
		{
			bill.ABL_Procedure = procedure;
			document.CSI_Code = "1014";
			AssertHasMessageError(document.CSI_CodeInfo, requiredTypeMessageForProcedureC07);

			document.CSI_Code = "N325";
			AssertNoMessageError(document.CSI_CodeInfo, requiredTypeMessageForProcedureC07);

			document.CSI_Code = "N380";
			AssertNoMessageError(document.CSI_CodeInfo, requiredTypeMessageForProcedureC07);
		}
	}

	public void TestSupportingDocumentType_AltaH7VxEntForESH7_RequireAtLeast2SupportingDocumentType()
	{
		foreach (var procedure in procedures07)
		{
			bill.ABL_Procedure = procedure;
			document.CSI_Code = "N325";
			AssertHasMessageError(document.CSI_CodeInfo, requireAtLeast2SupportingDocumentsMessage);

			var anotherDocument = bill.SupportingDocuments.AddNew();
			anotherDocument.CSI_Code = "1316";
			document.Validation.ValidateCSI_Code();
			AssertNoMessageError(document.CSI_CodeInfo, requireAtLeast2SupportingDocumentsMessage);
			bill.SupportingDocuments.Delete(anotherDocument);
		}
	}

	public void TestSupportingDocumentType_AltaH7VxEntForESH7_NoRepeatedSupportingDocumentType()
	{
		var anotherDocument = bill.SupportingDocuments.AddNew();
		document.CSI_Code = "1230";
		anotherDocument.CSI_Code = "1230";

		AssertHasMessageError(anotherDocument.CSI_CodeInfo, noRepeatedSupportingDocumentTypesMessage);

		anotherDocument.CSI_Code = "N380";

		AssertNoMessageError(anotherDocument.CSI_CodeInfo, noRepeatedSupportingDocumentTypesMessage);

		bill.SupportingDocuments.Delete(anotherDocument);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		document = (SupportingDocument)bill.SupportingDocuments.AddNew();
	}

	readonly string acceptedTypeMessageForProcedure07 = "Supporting Documents Type can only be '1014', '1018', '1230', '1315', '1316', '1317', '1318', '1319', '1320', 'N325' and/or 'N380'.";

	readonly string acceptedTypeMessageForProcedure08 = "Supporting Documents Type can only be '1003', '1018', '1315', '1316', '1317', '1318', '1319' and/or '1320'.";

	readonly string acceptedTypeMessageForProcedure163536 = "Supporting Documents Type can only be '1018', '1315', '1316', '1317', '1318', '1319', '1320' and/or 'N325'.";

	readonly string requireSupportingDocumentType1018Message = "Please enter a Supporting Documents Reference Number with Type '1018'.";

	readonly string requiredTypeMessageForProcedureC07 = "Please enter a Supporting Documents Reference Number with Type 'N325' and/or 'N380'.";

	readonly string requireAtLeast2SupportingDocumentsMessage = "Please enter at least two Supporting Documents Reference Numbers.";

	readonly string noRepeatedSupportingDocumentTypesMessage = "Selected Supporting Documents Type has already been provided.";

	readonly List<string> acceptedTypeForProcedure07 = ["1014", "1018", "1230", "1315", "1316", "1317", "1318", "1319", "1320", "N325", "N380"];

	readonly List<string> acceptedTypeForProcedure08 = ["1003", "1018", "1315", "1316", "1317", "1318", "1319", "1320"];

	readonly List<string> acceptedTypeForProcedure163536 = ["1018", "1315", "1316", "1317", "1318", "1319", "1320", "N325"];

	readonly List<string> procedures0708163536 = [Codes.C07, Codes.C08, Codes.C16, Codes.C35, Codes.C36];

	readonly List<string> procedures070816 = [Codes.C07, Codes.C08, Codes.C16];

	readonly List<string> procedures07 = [Codes.C07, Codes.C07F48, Codes.C07F49];

	readonly List<string> procedures163536 = [Codes.C16, Codes.C35, Codes.C36];

	readonly List<string> agentTypesDIRICA = ["DIR", "ICA"];

	readonly List<string> agentTypesINDDCA = ["IND", "DCA"];

	AsycudaBill bill;
	SupportingDocument document;
}
