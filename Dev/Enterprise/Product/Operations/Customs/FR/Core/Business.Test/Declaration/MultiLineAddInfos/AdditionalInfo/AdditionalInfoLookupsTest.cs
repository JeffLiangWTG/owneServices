using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.France;
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "France", eun);

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Additional Information");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", codeType, countryCode);

			var cusCode1 = helper.CreateCusCodeList(countryCode, codeType, "9001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attribute1 = cusCode1.Attributes.AddNew("Direction", "IMPORT");

			var cusCode2 = helper.CreateCusCodeList(countryCode, codeType, "9002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attribute2 = cusCode2.Attributes.AddNew("Direction", "EXPORT");

			var cusCode3 = helper.CreateCusCodeList(countryCode, codeType, "9003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attribute3 = cusCode3.Attributes.AddNew("Direction", "IMPORT");
			var attribute4 = cusCode3.Attributes.AddNew("Direction", "EXPORT");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var additionalInfo = invoiceHeader.AdditionalInfos.AddNew();

			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var list = (CodeDescriptionPairList)additionalInfo.Lookups.CodeList;
			AssertEquals("9001, 9003", list.CodesAsString);

			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			list = (CodeDescriptionPairList)additionalInfo.Lookups.CodeList;
			AssertEquals("9002, 9003", list.CodesAsString);
		}

		public void TestUCC6CodeList()
		{
			var config = new RefDataConfig(
				dataGroupings:
				[
					new(code: "EUN", description: "European Union", parent: ""),
					new(code: "FR", description: "France", parent: "EUN"),
					new(code: "DIE", description: "Delta IE", parent: "EUN")
				],
				cusCodeTypes:
				[
					new(typeCode: "AR44E", description: "Additional Reference Export", attributeTypes: [new(name: "Level", dataGrouping: "EUN"), new(name: "Level", dataGrouping: "FR")],
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "FR", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE2", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE3", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
						]
					),
					new(typeCode: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportAddDocTransportContract, description: "ImportAddDocTransportContract", attributeTypes: [new(name: "Level", dataGrouping: "EUN"), new(name: "Level", dataGrouping: "FR")],
						cusCodes:
						[
							new(code: "CODE4", dataGrouping: "FR", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE5", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE6", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
						]
					),
					new(typeCode: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportAddDocAdditionalReference, description: "ImportAddDocAdditionalReference", attributeTypes: [new(name: "Level", dataGrouping: "EUN"), new(name: "Level", dataGrouping: "DIE")],
						cusCodes:
						[
							new(code: "CODE7", dataGrouping: "DIE", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE8", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
							new(code: "CODE9", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
						]
					)
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var addInfoOfDec = dec.AdditionalInfos.AddNew();
			addInfoOfDec.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var declarationCodeList = addInfoOfDec.Lookups.CodeList;

			var addInfoOfInvoice = invoice.AdditionalInfos.AddNew();
			addInfoOfInvoice.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var invoiceCodeList = addInfoOfInvoice.Lookups.CodeList;

			var addInfoOfInvoiceLine = invoiceLine.AdditionalInfos.AddNew();
			addInfoOfInvoiceLine.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var invoiceLineCodeList = addInfoOfInvoiceLine.Lookups.CodeList;

			CombineAssertions("When additional parent is import and CSI_SubType is REF in FR, we should use AR44I", () =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("Declaration CodeList Type", declarationCodeList);
				AssertContainsExactElementsInAnyOrder("DIE codes with Header attribute should be loaded.", new[] { "CODE7" }, (declarationCodeList as ZZRefCusCodeListCombinedCollection).Select(x => x.ZZD_Code));

				AssertType<ZZRefCusCodeListCombinedCollection>("Invoice CodeList Type", invoiceCodeList);
				AssertContainsExactElementsInAnyOrder("DIE codes with Header attribute should be loaded.", new[] { "CODE7" }, (invoiceCodeList as ZZRefCusCodeListCombinedCollection).Select(x => x.ZZD_Code));

				AssertType<ZZRefCusCodeListCombinedCollection>("InvoiceLine CodeList Type", invoiceLineCodeList);
				AssertContainsExactElementsInAnyOrder("DIE codes with Header attribute should be loaded and Level attribute is omitted.", new[] { "CODE7" }, (invoiceLineCodeList as ZZRefCusCodeListCombinedCollection).Select(x => x.ZZD_Code));
			});
		}
	}
}
