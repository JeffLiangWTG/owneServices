using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	public static class CNCusEntryHeaderHelper
	{
		public static CNEntryHeaderTestData SetupCusEntryHeader(BusinessObjectFactory factory, Action funcFinish = null)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader.CH_CEI_Instruction = instruction.PK;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			funcFinish?.Invoke();

			return new CNEntryHeaderTestData(declaration, cusEntryHeader, entryLine, invoiceHeader, invoiceLine, instruction);
		}

		public static OrgAddress CreateNewAddress(BusinessObjectFactory factory, string orgName, string customsCode, string socalCreditCode, string ciqRegCode, string addressCompanyName = "", string orgHeaderCode = "")
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var result = orgHeader.MainAddress ?? orgHeader.Addresses.AddNew();
			result.CompanyName = addressCompanyName;
			result.Header.OH_FullName = orgName;
			result.Header.OH_Code = string.IsNullOrEmpty(orgHeaderCode) ? "COM" : orgHeaderCode;

			var ccdCode = result.Header.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = "CCD";
			ccdCode.OK_CustomsRegNo = customsCode;

			var cacCode = result.Header.CustomsCodes.AddNew();
			cacCode.OK_CodeType = "USC";
			cacCode.OK_CustomsRegNo = socalCreditCode;

			var ciqCode = result.Header.CustomsCodes.AddNew();
			ciqCode.OK_CodeType = "CIQ";
			ciqCode.OK_CustomsRegNo = ciqRegCode;

			return result;
		}

		public static RefCusCodeList CreateAndSaveNewRefCusCode(BusinessObjectFactory factory, ZString type, ZString code, ZString description)
		{
			return CreateAndSaveNewRefCusCode(factory, type, code, description, Array.Empty<(string, string)>());
		}

		public static RefCusCodeList CreateAndSaveNewRefCusCode(BusinessObjectFactory factory, ZString type, ZString code, ZString description, params (string name, string value)[] attributes)
		{
			return CreateAndSaveNewRefCusCode(factory, type, code, description, attributes.Select(a => (new ZString(a.name), new ZString(a.value))).ToArray());
		}

		public static RefCusCodeList CreateAndSaveNewRefCusCode(BusinessObjectFactory factory, ZString type, ZString code, ZString description, params (ZString name, ZString value)[] attributes)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(type, "Description");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China);

			var result = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, type, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			if (attributes != null)
			{
				foreach (var attribute in attributes)
				{
					helper.CreateNewOrGetExistingCusCodeListAttribute(result.PK, attribute.name, attribute.value);
				}
			}

			factory.Save();

			return result;
		}

		public static RefCusMap CreateAndSaveNewRefCusMap(BusinessObjectFactory factory, ZString type, ZString customsValue, ZString cw1orCommercialValue)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateCusMapType(type, "BTH", "XXX", true);
			var result = helper.CreateCusMap(type, cw1orCommercialValue, customsValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CN");

			factory.Save();
			return result;
		}

		public static void SetDeclarationAndEntry(CusEntryHeader header, bool isEntering, ZString entryType)
		{
			header.Declaration.JE_MessageType = isEntering ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;
			header.Declaration.JE_MessageSubType = entryType;
			header.CH_MessageType = entryType;
		}

		public static OrgSupplierPart CreateNewProduct(BusinessObjectFactory factory, OrgHeader supplier, ZString productNo, ZString productDesc)
		{
			var result = factory.New<OrgSupplierPart>();

			result.OP_StockKeepingUnit = "BAG";
			result.OP_PartNum = productNo;
			result.OP_Desc = productDesc;

			var relatedOrganization = result.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			return result;
		}

		public static void AssertEqualsIgnoreLineBreaksAndIndent(string message, string expected, string actuall)
		{
			var ignoreRegex = @"[\r\n]{1,2}\s+";
			Assertion.AssertMultilineASCIIEquals(message, Regex.Replace(expected, ignoreRegex, ""), Regex.Replace(actuall, ignoreRegex, ""));
		}
	}
}
