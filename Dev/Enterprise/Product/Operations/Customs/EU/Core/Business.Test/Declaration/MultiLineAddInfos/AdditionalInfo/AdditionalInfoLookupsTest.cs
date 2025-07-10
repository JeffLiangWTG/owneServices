using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.BaseCusClassification;
using OrgSupplierPart = Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	sealed class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestList63()
		{
			var list63 = additionalInfo.Lookups.List63ExportFromCountries;
			AssertEquals("AD", list63[0].Code);
			AssertEquals("Andorra", list63[0].Description);
			AssertContains(Core.Constants.CountryCodes.SvalbardAndJanMayen, list63.CodesAsString);
		}

		public void TestStatusList()
		{
			var statusList = additionalInfo.Lookups.StatusList;
			AssertEquals(AdditionalInfoIssuerList.Codes.Customs, statusList[0].Code);
			AssertEquals(AdditionalInfoIssuerList.Descriptions.Customs, statusList[0].Description);
		}

		public void TestSubTypeListType()
		{
			AssertEquals("Pre-requisite: UCCAdditionalInfosSupport default false", false, declaration.Configuration.UCCAdditionalInfosSupport(declaration));
			AssertContainsExactElementsInAnyOrder("SubTypeList contains 3 elements for EU", new[] { "INF", "REF", "TRA" }, additionalInfo.Lookups.SubTypeList.GetAllCodes());
		}

		public void TestCodeList_UCCAdditionalInfosSupport_False()
		{
			var config = new RefDataConfig(
				dataGroupings:
				[
					new(code: "EUN", description: "European Union", parent: ""),
					new(code: "LV", description: "European Union",  parent: "EUN")
				],
				cusCodeTypes:
				[
					new(typeCode: "ADDIN", description: "Additional Information", attributeTypes: [new(name: "Level", dataGrouping: "EUN"), new(name: "Direction", dataGrouping: "EUN")] ,
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item"), new(name: "Direction", value: "Import")]),
							new(code: "CODE2", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item"), new(name: "Direction", value: "Export")])
						]
					)
				]
			);

			EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);
			CombineAssertions("AdditionalInfo in InvoiceLine", () =>
			{
				declaration.JE_MessageType = "IMP";
				AssertEquals("Pre-requisite: additionalInfo.ImportExportParent.Level", "Item", additionalInfo.ImportExportParent.Level);
				var codeList = additionalInfo.Lookups.CodeList;
				AssertType<CodeDescriptionPairList>("CodeList Type", codeList);
				AssertContainsExactElementsInAnyOrder("Only codes that match Level can be loaded ", new[] { "CODE1" }, codeList.ToList<CodeDescriptionPair>().Select(x => x.Code));
			});
			CombineAssertions("AdditionalInfo in Declaration", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "EXP";
				var additionalInfo2 = declaration.AdditionalInfos.AddNew();
				var codeList = additionalInfo2.Lookups.CodeList;
				AssertType<CodeDescriptionPairList>("CodeList Type", codeList);
				AssertContainsExactElementsInAnyOrder("Only codes that match Level can be loaded ", new[] { "CODE2" }, codeList.ToList<CodeDescriptionPair>().Select(x => x.Code));
			});
			CombineAssertions("AdditionalInfo in PartPivot", () =>
			{
				partPivot.CI_ChildType = ClassificationType.IMP;
				var additionalInfo2 = partPivot.AdditionalInfos.AddNew();
				var codeList = additionalInfo2.Lookups.CodeList;
				AssertType<CodeDescriptionPairList>("CodeList Type", codeList);
				AssertContainsExactElementsInAnyOrder("Only codes that match Level can be loaded ", new[] { "CODE1" }, codeList.ToList<CodeDescriptionPair>().Select(x => x.Code));
			});
		}

		public void TestCodeList_UCCAdditionalInfosSupport_True()
		{
			var config = new RefDataConfig(
				dataGroupings:
				[
					new(code: "EUN", description: "European Union", parent: ""),
					new(code: "LV", description: "Latvia", parent: "EUN")
				],
				cusCodeTypes:
				[
					new(typeCode: "AI44I", description: "ImportAddDocAdditionalInformation", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item")]),
							new(code: "CODE2", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
							new(code: "CODE3", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					),
					new(typeCode: "AR44I", description: "ImportAddDocAdditionalReference", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new(code: "CODE4", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item")]),
							new(code: "CODE5", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
							new(code: "CODE6", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					),
					new(typeCode: "TD44I", description: "ImportAddDocTransportContract", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new(code: "CODE7", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item")]),
							new(code: "CODE8", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
							new(code: "CODE9", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					),
					new(typeCode: "AI44E", description: "ExportAddDocAdditionalInformation", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new(code: "CODE10", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item")]),
							new(code: "CODE11", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
							new(code: "CODE12", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					),
					new(typeCode: "AR44E", description: "ExportAddDocAdditionalReference", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new(code: "CODE13", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item")]),
							new(code: "CODE14", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
							new(code: "CODE15", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					),
					new(typeCode: "TD44E", description: "ExportAddDocTransportContract", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new(code: "CODE16", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item")]),
							new(code: "CODE17", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
							new(code: "CODE18", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					)
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "UCCAdditionalInfosSupportCore", true))
			{
				CombineAssertions("Declaration", () =>
				{
					AssertAdditionalCodesLoadedCorrectly(declaration, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "IMP", new[] { "CODE2", "CODE3" });
					AssertAdditionalCodesLoadedCorrectly(declaration, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "EXP", new[] { "CODE11", "CODE12" });
					AssertAdditionalCodesLoadedCorrectly(declaration, AdditionalInfoSubTypeList.Codes.AdditionalReference, "IMP", new[] { "CODE5", "CODE6" });
					AssertAdditionalCodesLoadedCorrectly(declaration, AdditionalInfoSubTypeList.Codes.AdditionalReference, "EXP", new[] { "CODE14", "CODE15" });
					AssertAdditionalCodesLoadedCorrectly(declaration, AdditionalInfoSubTypeList.Codes.TransportDocument, "IMP", new[] { "CODE8", "CODE9" });
					AssertAdditionalCodesLoadedCorrectly(declaration, AdditionalInfoSubTypeList.Codes.TransportDocument, "EXP", new[] { "CODE17", "CODE18" });
				});

				CombineAssertions("Invoice Header", () =>
				{
					AssertAdditionalCodesLoadedCorrectly(invoiceHeader, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "IMP", new[] { "CODE2", "CODE3" });
					AssertAdditionalCodesLoadedCorrectly(invoiceHeader, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "EXP", new[] { "CODE11", "CODE12" });
					AssertAdditionalCodesLoadedCorrectly(invoiceHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference, "IMP", new[] { "CODE5", "CODE6" });
					AssertAdditionalCodesLoadedCorrectly(invoiceHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference, "EXP", new[] { "CODE14", "CODE15" });
					AssertAdditionalCodesLoadedCorrectly(invoiceHeader, AdditionalInfoSubTypeList.Codes.TransportDocument, "IMP", new[] { "CODE8", "CODE9" });
					AssertAdditionalCodesLoadedCorrectly(invoiceHeader, AdditionalInfoSubTypeList.Codes.TransportDocument, "EXP", new[] { "CODE17", "CODE18" });
				});

				CombineAssertions("Invoice Line", () =>
				{
					AssertAdditionalCodesLoadedCorrectly(invoiceLine, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "IMP", new[] { "CODE1", "CODE2" });
					AssertAdditionalCodesLoadedCorrectly(invoiceLine, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "EXP", new[] { "CODE10", "CODE11" });
					AssertAdditionalCodesLoadedCorrectly(invoiceLine, AdditionalInfoSubTypeList.Codes.AdditionalReference, "IMP", new[] { "CODE4", "CODE5" });
					AssertAdditionalCodesLoadedCorrectly(invoiceLine, AdditionalInfoSubTypeList.Codes.AdditionalReference, "EXP", new[] { "CODE13", "CODE14" });
					AssertAdditionalCodesLoadedCorrectly(invoiceLine, AdditionalInfoSubTypeList.Codes.TransportDocument, "IMP", new[] { "CODE7", "CODE8" });
					AssertAdditionalCodesLoadedCorrectly(invoiceLine, AdditionalInfoSubTypeList.Codes.TransportDocument, "EXP", new[] { "CODE16", "CODE17" });
				});

				CombineAssertions("Entry Instruction", () =>
				{
					AssertAdditionalCodesLoadedCorrectly(entryInstruction, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "IMP", new[] { "CODE2", "CODE3" });
					AssertAdditionalCodesLoadedCorrectly(entryInstruction, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "EXP", new[] { "CODE11", "CODE12" });
					AssertAdditionalCodesLoadedCorrectly(entryInstruction, AdditionalInfoSubTypeList.Codes.AdditionalReference, "IMP", new[] { "CODE5", "CODE6" });
					AssertAdditionalCodesLoadedCorrectly(entryInstruction, AdditionalInfoSubTypeList.Codes.AdditionalReference, "EXP", new[] { "CODE14", "CODE15" });
					AssertAdditionalCodesLoadedCorrectly(entryInstruction, AdditionalInfoSubTypeList.Codes.TransportDocument, "IMP", new[] { "CODE8", "CODE9" });
					AssertAdditionalCodesLoadedCorrectly(entryInstruction, AdditionalInfoSubTypeList.Codes.TransportDocument, "EXP", new[] { "CODE17", "CODE18" });
				});
			}
			using (ConfigurationTestHelper.TemporarilyClearPartPivotConfigurationAndThenSetPartPivotConfiguration(partPivot, "UCCAdditionalInfosSupportCore", true))
			{
				AssertAdditionalCodesLoadedCorrectly(partPivot, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "IMP", new[] { "CODE1", "CODE2" });
				AssertAdditionalCodesLoadedCorrectly(partPivot, AdditionalInfoSubTypeList.Codes.AdditionalInformation, "EXP", new[] { "CODE10", "CODE11" });
				AssertAdditionalCodesLoadedCorrectly(partPivot, AdditionalInfoSubTypeList.Codes.AdditionalReference, "IMP", new[] { "CODE4", "CODE5" });
				AssertAdditionalCodesLoadedCorrectly(partPivot, AdditionalInfoSubTypeList.Codes.AdditionalReference, "EXP", new[] { "CODE13", "CODE14" });
				AssertAdditionalCodesLoadedCorrectly(partPivot, AdditionalInfoSubTypeList.Codes.TransportDocument, "IMP", new[] { "CODE7", "CODE8" });
				AssertAdditionalCodesLoadedCorrectly(partPivot, AdditionalInfoSubTypeList.Codes.TransportDocument, "EXP", new[] { "CODE16", "CODE17" });
			}

			void AssertAdditionalCodesLoadedCorrectly(object addInfoParent, string subType, string messageType, string[] expectedCodes)
			{
				AdditionalInfo additionalInfo = null;
				if (addInfoParent is JobDeclaration declaration)
				{
					declaration.JE_MessageType = messageType;
					additionalInfo = declaration.AdditionalInfos.AddNew();
				}
				else if (addInfoParent is JobComInvoiceHeader invoice)
				{
					invoice.JobDeclaration.JE_MessageType = messageType;
					additionalInfo = invoice.AdditionalInfos.AddNew();
				}
				else if (addInfoParent is JobComInvoiceLine invoiceLine)
				{
					invoiceLine.Declaration.JE_MessageType = messageType;
					additionalInfo = invoiceLine.AdditionalInfos.AddNew();
				}
				else if (addInfoParent is CusEntryInstruction entryInstruction)
				{
					entryInstruction.JobDeclaration.JE_MessageType = messageType;
					additionalInfo = entryInstruction.AdditionalInfos.AddNew();
				}
				else if (addInfoParent is CusClassPartPivot partPivot)
				{
					partPivot.CI_ChildType = messageType;
					additionalInfo = partPivot.AdditionalInfos.AddNew();
				}
				else
				{
					Fail("Not proper Additional Info Parent is provided.");
				}
				additionalInfo.CSI_SubType = subType;
				var codeList = additionalInfo.Lookups.CodeList;
				AssertType<ZZRefCusCodeListCombinedCollection>("CodeList Type", codeList);

				var actualCodes = (codeList as ZZRefCusCodeListCombinedCollection)?.Select(x => x.ZZD_Code) ?? Array.Empty<ZString>();
				AssertContainsExactElementsInAnyOrder($"Code list should be loaded correctly for {addInfoParent.GetType()} being {messageType}", expectedCodes, actualCodes);
			}
		}

		public void TestCodeList_UCCAdditionalInfosSupport_OmitLevelAttribute_False()
		{
			AssertCodeList_UCCAdditionalInfosSupport_OmitLevelAttribute(false, new[] { "CODE1", "CODE2" });
		}

		public void TestCodeList_UCCAdditionalInfosSupport_OmitLevelAttribute_True()
		{
			AssertCodeList_UCCAdditionalInfosSupport_OmitLevelAttribute(true, new[] { "CODE1", "CODE2", "CODE3" });
		}

		void AssertCodeList_UCCAdditionalInfosSupport_OmitLevelAttribute(bool omitLevelAttribute, string[] expectedCodes)
		{
			var config = new RefDataConfig(
				dataGroupings:
				[
					new(code: "EUN", description: "European Union", parent: ""),
					new(code: "LV", description: "Latvia", parent: "EUN")
				],
				cusCodeTypes:
				[
					new(typeCode: "AR44E", description: "ExportAddDocAdditionalReference", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item")]),
							new(code: "CODE2", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Level", value: "Item")]),
							new(code: "CODE3", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
						]
					)
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "UCCAdditionalInfosSupportCore", true))
			{
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				var lookups = new AdditionalInfoLookupsForTest(additionalInfo, omitLevelAttribute);
				var actualCodes = (lookups.CodeList as ZZRefCusCodeListCombinedCollection).Select(x => x.ZZD_Code);
				AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			additionalInfo = invoiceLine.AdditionalInfos.AddNew();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "P1";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = ClassificationType.Both;
			relationship.OU_OH = org.PK;
			partPivot = product.PivotsForBinding.AddNew();
		}

		AdditionalInfo additionalInfo;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusClassPartPivot partPivot;

		#endregion
	}

	sealed class AdditionalInfoLookupsForTest : AdditionalInfoLookups
	{
		public AdditionalInfoLookupsForTest(AdditionalInfo parent) : base(parent)
		{
		}

		public AdditionalInfoLookupsForTest(AdditionalInfo parent, ZBool omitLevelAttribute) : this(parent)
		{
			this.omitLevelAttribute = omitLevelAttribute;
		}
		readonly ZBool omitLevelAttribute;

		protected override ZBool OmitLevelAttribute => omitLevelAttribute;
	}
}
