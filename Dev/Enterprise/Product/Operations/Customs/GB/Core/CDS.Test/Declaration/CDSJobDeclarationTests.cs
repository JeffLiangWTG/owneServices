using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration.Testing;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(JobDeclaration))]
	public class CDSJobDeclarationTests : JobDeclarationTests
	{
		public void TestCDSDeclarationTypeList_CodesInUppercase()
		{
			AssertEquals("Should be uppercase", "21I", Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I);
			AssertEquals("Should be uppercase", "21N", Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N);
			AssertEquals("Should be uppercase", "21E", Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E);
		}

		public new void TestUCCHelper()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertNotNull(dec.UCCHelper);
			AssertType<CDSUCCHelper>(dec.UCCHelper);
		}

		public void TestAddInfoValidation()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertType<CDSJobDeclarationValidation>(dec.AddInfoValidation);
		}

		public void TestIApportionInvoiceHolderCountryContext()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("GBCDS", ((IApportionInvoiceHolder)dec).CountryContext);
		}

		public new void TestAllEntriesCleared()
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)GetNewBusinessObject();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals("Empty collection", 0, declaration.CustomsEntryHeaders.Count);
			AssertEquals(false, declaration.HaveAllEntriesCleared);

			var mock1 = Factory.NewMoq<Business.Declaration.CusEntryHeader>();
			mock1.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2005, 2, 1));
			declaration.CustomsEntryHeaders.Add(mock1.Object);
			AssertEquals(true, declaration.HaveAllEntriesCleared);

			var mock2 = Factory.NewMoq<Business.Declaration.CusEntryHeader>();
			mock2.Setup(m => m.ClearanceDate).Returns(ZDateTime.Empty);
			declaration.CustomsEntryHeaders.Add(mock2.Object);
			AssertEquals(false, declaration.HaveAllEntriesCleared);
			mock1.VerifyAll();
			mock2.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}

		public void TestLogicAroundDeclarantType()
		{
			const string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;
			const string EORI = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				#region PrepareBusinessObjects

				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "Supplier";

				var importerA = Factory.New<OrgHeader>();
				importerA.OH_Code = "ImporterA";
				importerA.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, "X123", countryCodeUK);

				var importerB = Factory.New<OrgHeader>();
				importerB.OH_Code = "ImporterB";
				importerB.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, "X123", countryCodeUK);
				var addrImpB = importerB.Addresses.AddNew();
				addrImpB.Address1 = "Address for Importer B";

				var importerC = Factory.New<OrgHeader>();
				importerC.OH_Code = "ImporterC";
				importerC.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, "Y456", countryCodeUK);
				var addrImpC = importerC.Addresses.AddNew();
				addrImpC.Address1 = "Address for Importer C";

				Factory.Save();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "IMP";
				dec.JE_ApplicationCode = "CDS";

				dec.JE_OH_Supplier = supplier.PK;
				dec.JE_OH_Importer = importerA.PK;

				#endregion

				dec.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				CheckDeclarantRelatedPropertiesAndInfo(dec, RepresentationTypeList.Codes._2Direct, false, false, false);

				dec.JE_OA_DeclarantAddress = addrImpC.PK;
				CheckDeclarantRelatedPropertiesAndInfo(dec, RepresentationTypeList.Codes._2Direct, false, false, false);

				dec.JE_OA_DeclarantAddress = addrImpB.PK;
				CheckDeclarantRelatedPropertiesAndInfo(dec, "", false, false, true);

				dec.AdditionalInfos.RemoveAll();
				dec.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				CheckDeclarantRelatedPropertiesAndInfo(dec, RepresentationTypeList.Codes._1Self, true, false, true);

				dec.AdditionalInfos.RemoveAll();
				dec.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				CheckDeclarantRelatedPropertiesAndInfo(dec, RepresentationTypeList.Codes._3Indirect, false, false, true);

				dec.AdditionalInfos.RemoveAll();
				dec.JE_DeclarantType = "";
				CheckDeclarantRelatedPropertiesAndInfo(dec, "", false, false, true);

				dec.AdditionalInfos.RemoveAll();
				var docAddrRepresent = dec.DocAddresses.AddNew();
				docAddrRepresent.DocAddressType = DocAddressType.Representative;
				docAddrRepresent.OrganisationPK = importerC.PK;

				var validation = new CDSJobDeclarationValidation(dec);
				validation.ValidateJE_OA_DeclarantAddress();
				CheckDeclarantRelatedPropertiesAndInfo(dec, "", false, true, false);

				docAddrRepresent.OrganisationPK = ZGuid.Empty;
				validation.ValidateJE_OA_DeclarantAddress();
				CheckDeclarantRelatedPropertiesAndInfo(dec, "", false, false, false);

				dec.AdditionalInfos.RemoveAll();
				dec.DocAddresses.Remove(docAddrRepresent);
				validation.ValidateJE_OA_DeclarantAddress();
				CheckDeclarantRelatedPropertiesAndInfo(dec, "", false, false, false);

				dec.AdditionalInfos.RemoveAll();
				dec.JE_OA_Representative = importerC.PK;
				validation.ValidateJE_OA_DeclarantAddress();
				CheckDeclarantRelatedPropertiesAndInfo(dec, "", false, true, false);

				dec.AdditionalInfos.RemoveAll();
				dec.JE_OA_Representative = ZGuid.Empty;
				validation.ValidateJE_OA_DeclarantAddress();
				CheckDeclarantRelatedPropertiesAndInfo(dec, "", false, false, false);
			}
		}

		void CheckDeclarantRelatedPropertiesAndInfo(JobDeclaration dec,
													string expectedType,
													bool mustHaveMsg1,
													bool mustHaveMsg2,
													bool mustHaveImporterAdditionalInfo)
		{
			const string msg1 = "Under CDS, self representation is not declared using this field. For self representation, set the declarant and importer to be the same party (compared using EORI), and clear this field.";
			const string msg2 = "When the declarant is the importer, a representative is not allowed. Remove the representative, or select a different importer or declarant.";

			AssertEquals(expectedType, dec.JE_DeclarantType);
			AssertEquals(mustHaveMsg1, dec.JE_DeclarantTypeInfo.HasMessageError(msg1));
			AssertEquals(mustHaveMsg2, dec.JE_OA_DeclarantAddressInfo.HasMessageError(msg2));

			AssertEquals(mustHaveImporterAdditionalInfo, dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x =>
																				x.CSI_Code == "00500"
																				&& x.CSI_Description == "IMPORTER"));
		}

		#region TestMergeOperation_GB_CDS

		protected override void PrepareUniversalData_ToTestMergeOperation(string countryCode)
		{
			base.PrepareUniversalData_ToTestMergeOperation(countryCode);

			const string CDS = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(CDS, "To test CDS Merge operation");

			ZDateTime dtMIN = ZDateTime.MinSmallDateTimeValue;
			ZDateTime dtMAX = ZDateTime.MaxSmallDateTimeValue;

			helper.CreateCusCodeType("ENSUB", "Entry Sub Style");
			helper.CreateCusCodeList(CDS, "ENSUB", "A", "Standard customs declaration (Goods arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "B", "Simplified declaration on an occasional basis (Goods arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "C", "Simplified declaration with regular use (pre-authorised) (Goods arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "D", "Standard customs declaration (Goods not arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "E", "Simplified declaration on an occasional basis (Goods not arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "F", "Simplified declaration with regular use (pre-authorised) (Goods not arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "J", "C21 (Goods arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "K", "C21 (Goods not arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "X", "Supplementary declaration covered by types B and E (Goods arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "Y", "Supplementary declaration covered by types C and F (Goods arrived)", dtMIN, dtMAX);
			helper.CreateCusCodeList(CDS, "ENSUB", "Z", "Supplementary declarations for Entry in Declarants Records (Goods arrived)", dtMIN, dtMAX);

			helper.CreateRefCusProcedure(CDS, "F", "66", "66", "666", "Six", "IMP", group: "H1,I1,H5");
			helper.CreateRefCusProcedure(CDS, "G", "77", "77", "777", "Seven", "EXP", group: "AA,BB");

			Factory.Save();
		}

		protected override List<MergeScenario> PrepareMergeScenarios_ToTestMergeOperation(string countryCode)
		{
			const string CDS = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			const string EXP = MessageTypeList.Codes.Export;
			const string IMP = MessageTypeList.Codes.Import;

			var list = new List<MergeScenario>();

			void AddNewEntryInstructionToDeclarationAndLinkAllLinesOfInvoice(EU.Business.Declaration.JobDeclaration declaration, EU.Business.Declaration.JobComInvoiceHeader invoiceHeader)
			{
				var entryInstr = declaration.CustomsEntryInstructions.AddNew();
				entryInstr.CEI_Style = "H1";
				foreach (JobComInvoiceLine line in invoiceHeader.InvoiceLines)
				{
					line.JI_CEI = entryInstr.PK;
				}
			}

			#region Scenario_01_CDS

			var dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CDS, 2);

			var invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));

			var invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			var scenario = new MergeScenario(dec);
			scenario.AssertMessage = "GB CDS test scenario 01";

			var entry = scenario.AddEntry("-11-12");

			var entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("IMH01");
			entryLine.AddAdditionalInfo("IMI01");

			entryLine = entry.AddEntryLine("12");
			entryLine.AddAdditionalInfo("IMH01");

			list.Add(scenario);

			#endregion  //End: "Scenario_01_CDS"

			#region Scenario_02_CDS

			dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CDS, 2, 3);

			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));
			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH02"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI03"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			AddNewEntryInstructionToDeclarationAndLinkAllLinesOfInvoice(dec, dec.Invoices[1]);

			scenario = new MergeScenario(dec);
			scenario.AssertMessage = "GB CDS test scenario 02";

			entry = scenario.AddEntry("-11-12");

			entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("IMH01");
			entryLine.AddAdditionalInfo("IMI01");
			entryLine.AddAdditionalInfo("IMH02");
			entryLine.AddAdditionalInfo("IMI02");

			entryLine = entry.AddEntryLine("12");
			entryLine.AddAdditionalInfo("IMH01");
			entryLine.AddAdditionalInfo("IMI01");
			entryLine.AddAdditionalInfo("IMH02");
			entryLine.AddAdditionalInfo("IMI03");

			entry = scenario.AddEntry("-24-25-26");

			entryLine = entry.AddEntryLine("24");
			entryLine.AddAdditionalInfo("IMH01");
			entryLine.AddAdditionalInfo("IMI01");
			entryLine.AddAdditionalInfo("IMH03");

			entryLine = entry.AddEntryLine("25");
			entryLine.AddAdditionalInfo("IMH01");
			entryLine.AddAdditionalInfo("IMI01");
			entryLine.AddAdditionalInfo("IMH03");

			entryLine = entry.AddEntryLine("26");
			entryLine.AddAdditionalInfo("IMH01");
			entryLine.AddAdditionalInfo("IMI01");
			entryLine.AddAdditionalInfo("IMH03");
			entryLine.AddAdditionalInfo("IMI02");

			list.Add(scenario);

			#endregion  //End: "Scenario_02_CDS"

			#region Scenario_03_CDS

			dec = CreateJobDeclaration_ToTestMergeOperation(EXP, CDS, 1, 2, 3);
			dec.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH01"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH02"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[2];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));

			AddNewEntryInstructionToDeclarationAndLinkAllLinesOfInvoice(dec, dec.Invoices[1]);
			AddNewEntryInstructionToDeclarationAndLinkAllLinesOfInvoice(dec, dec.Invoices[2]);

			scenario = new MergeScenario(dec);
			scenario.AssertMessage = "GB CDS test scenario 03";

			entry = scenario.AddEntry("-11");

			entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("EXH03");
			entryLine.AddAdditionalInfo("EXH01");
			entryLine.AddAdditionalInfo("EXI02");

			entry = scenario.AddEntry("-24-25");

			entryLine = entry.AddEntryLine("24");
			entryLine.AddAdditionalInfo("EXH03");
			entryLine.AddAdditionalInfo("EXH02");
			entryLine.AddAdditionalInfo("EXI01");
			entryLine.AddAdditionalInfo("EXI03");

			entryLine = entry.AddEntryLine("25");
			entryLine.AddAdditionalInfo("EXH03");
			entryLine.AddAdditionalInfo("EXH02");
			entryLine.AddAdditionalInfo("EXI02");

			entry = scenario.AddEntry("-37-38-39");

			entryLine = entry.AddEntryLine("37");
			entryLine.AddAdditionalInfo("EXH03");
			entryLine.AddAdditionalInfo("EXI02");

			entryLine = entry.AddEntryLine("38");
			entryLine.AddAdditionalInfo("EXH03");
			entryLine.AddAdditionalInfo("EXI03");

			entryLine = entry.AddEntryLine("39");
			entryLine.AddAdditionalInfo("EXH03");
			entryLine.AddAdditionalInfo("EXI02");
			entryLine.AddAdditionalInfo("EXI01");

			list.Add(scenario);

			#endregion  //End: "Scenario_03_CDS"

			#region Scenario_04_CDS

			dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CDS, 2);

			invoice = dec.Invoices[0];
			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			scenario = new MergeScenario(dec);
			scenario.AssertMessage = "GB CDS test scenario 04";

			entry = scenario.AddEntry("-11-12");
			entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("IMI02");

			entryLine = entry.AddEntryLine("12");
			entryLine.AddAdditionalInfo("IMI01");

			list.Add(scenario);

			#endregion  //End: "Scenario_04_CDS"

			foreach (var theScenario in list)
			{
				if (theScenario.JobDeclaration.JE_MessageType == IMP)
				{
					foreach (var theEntry in theScenario.ExpectedEntryList)
					{
						foreach (var theLine in theEntry.EntryLines)
						{
							theLine.AddAdditionalInfo("00500");
						}
					}
				}
			}

			return list;
		}

		protected override EU.Business.Declaration.JobDeclaration CreateJobDeclaration_ToTestMergeOperation(string messageType, string applicationCode, int invLinesOnInvoice1, int invLinesOnInvoice2 = 0, int invLinesOnInvoice3 = 0)
		{
			var declaration = base.CreateJobDeclaration_ToTestMergeOperation(messageType, applicationCode, invLinesOnInvoice1, invLinesOnInvoice2, invLinesOnInvoice3);
			var dec = declaration as JobDeclaration;

			dec.JE_CustomsProfile = "BC2";
			dec.JE_ApplicationCode = applicationCode;
			dec.JE_DeclarationType = "H1";

			var instr = dec.CustomsEntryInstructions.AddNew();
			instr.CEI_Style = "H1";

			foreach (var line in dec.InvoiceLines)
			{
				var invLine = line as JobComInvoiceLine;
				invLine.JI_CEI = instr.PK;
				invLine.JI_Procedure = "66";
			}

			const string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;
			const string EORI = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			const string eoriCode = "AA";

			var importerA = Factory.New<OrgHeader>();
			importerA.OH_Code = ZGuid.NewZGuid().ToString().Substring(0, 8);
			importerA.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, eoriCode, countryCodeUK);

			var addrImpA = importerA.Addresses.AddNew();
			addrImpA.Address1 = "Address for Importer A";

			dec.JE_OH_Importer = importerA.PK;
			dec.JE_OA_DeclarantAddress = addrImpA.PK;

			Assert(!dec.DeclarantTraderId.IsEmpty);

			dec.JE_CHIEF_GoodsLocation = "ZZZ";
			dec.SubLocation = "XXX";

			dec.JE_GBRouteOfEntry = "H";
			dec.JE_EntrySubStyle = "C";

			Factory.Save();

			return dec;
		}

		protected override void Produce_EDI_Message_ToTestMergeOperation(MergeScenario scenario)
		{
			var msg = scenario.AssertMessage;
			var dec = scenario.JobDeclaration;
			dec.JE_DeclarationReference = "VICTEST";
			dec.Branch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			dec.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");

			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var abc = badgeCodeSettings.AddNew();
			abc.BadgeCode = "ABC";
			abc.RL_PortCode = "GBLBA";
			abc.Direction = dec.JE_MessageType;
			abc.CSPCode = "CCSUK";
			abc.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk;
			abc.ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings))
			{
				var credential = new CredentialsSetting
				{
					BadgeCode = "ABC",
					Printer = "Location",
					Company = "Role"
				};
				var credentials = GBCustomsDataRegistry.Instance.Credentials.GetValueWithoutFallback(dec.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
				credentials.Add(credential);
				using (GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(dec.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, credentials))
				{
					dec.JE_CustomsProfile = "ABC";
					var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
					var decWrapper = new JobDeclarationMessageSendingObjectParent(dec as JobDeclaration);
					var sender = new CDSMessageSender(decWrapper);
					sender.Send(shutUp);

					Assert(msg, shutUp.SuccessfulSendOccured);
				}
			}
		}

		protected override void Check_EDI_Message_ToTestMergeOperation(MergeScenario scenario, Entry_ToTestMergeOperation expectedEntry, EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			var msg = scenario.AssertMessage;
			var entryMessages = entryHeader.Messages;
			var msgText = entryMessages[0].EM_FormattedMessageText;

			Assert(!msgText.IsEmpty);

			var addInfoLookup = CreateAddInfoLookup_Header_ToTestMergeOperation(msgText);
			CompareAdditionalInfos_ToTestMergeOperation(msg, expectedEntry.AdditionalInfoList, addInfoLookup);

			var entryLineLookup = CreateEntryLineLookup(msgText);
			AssertEquals(msg + ": No of entry lines", expectedEntry.EntryLines.Count, entryLineLookup.Count);

			foreach (var expectedLine in expectedEntry.EntryLines)
			{
				var entryLine = FindCorrespondingEntryLine(expectedLine, entryLineLookup);
				Assert(msg + ": Entry line not found.", entryLine != null);

				addInfoLookup = CreateAddInfoLookup_ToTestMergeOperation(entryLine);
				var expectedList = GetExpectList(entryHeader, expectedEntry, expectedLine, entryLine);
				CompareAdditionalInfos_ToTestMergeOperation(msg, expectedList, addInfoLookup);
			}
		}

		protected List<AddInfo_ToTestMergeOperation> GetExpectList(EU.Business.Declaration.CusEntryHeader entryHeader, Entry_ToTestMergeOperation expectedEntry, EntryLine_ToTestMergeOperation expectedLine, XmlNode entryLine)
		{
			var result = new List<AddInfo_ToTestMergeOperation>();
			var actualEntryLines = entryHeader.AllEntryLines;
			var description = GetDescriptionFromMessage(entryLine);

			if (!string.IsNullOrEmpty(description))
			{
				var actualEntryLine = actualEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.Description == description);
				expectedLine.AdditionalInfoList.RemoveAll(x => actualEntryLine.AdditionalInfos.Any(y => y.CSI_Code == x.Code && !y.IsLineOnly));
				result.AddRange(expectedLine.AdditionalInfoList);
			}
			return result;
		}

		string GetDescriptionFromMessage(XmlNode msgText)
		{
			var result = string.Empty;
			var node = msgText.SelectSingleNode("./*[local-name()='Commodity']/*[local-name()='Description']");
			if (node != null)
			{
				result = node.InnerText;
			}

			return result;
		}

		Dictionary<string, XmlNode> CreateEntryLineLookup(string msgText)
		{
			var lookup = new Dictionary<string, XmlNode>();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(msgText);
			XmlNodeList nodeList = xmlDoc.SelectNodes("//*[local-name()='GovernmentAgencyGoodsItem']");

			foreach (XmlNode node in nodeList)
			{
				var nodeQuantity = node.SelectSingleNode("./*[local-name()='Commodity']/*[local-name()='GoodsMeasure']/*[local-name()='NetNetWeightMeasure'][1]");
				string key = nodeQuantity.InnerText;

				if (key != null && !lookup.ContainsKey(key))
				{
					lookup.Add(key, node);
				}
			}
			return lookup;
		}

		XmlNode FindCorrespondingEntryLine(EntryLine_ToTestMergeOperation expectedLine, Dictionary<string, XmlNode> entryLineLookup)
		{
			XmlNode entryLine = null;
			entryLineLookup.TryGetValue(expectedLine.lookupKey, out entryLine);
			return entryLine;
		}

		Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> CreateAddInfoLookup_ToTestMergeOperation(XmlNode entryLine)
		{
			var lookup = new Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			var nodeList = entryLine.SelectNodes("./*[local-name()='AdditionalInformation']/*[local-name()='StatementCode']");

			foreach (XmlNode node in nodeList)
			{
				string code = node.InnerText;
				if (code != null && !lookup.ContainsKey(code))
				{
					var addInfo = Factory.New<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
					Factory.EnqueueDelete(typeof(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo), addInfo.PK);
					addInfo.CSI_Code = code;
					lookup.Add(code, addInfo);
				}
			}
			return lookup;
		}

		Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> CreateAddInfoLookup_Header_ToTestMergeOperation(string msgText)
		{
			var lookup = new Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(msgText);
			XmlNodeList nodeList = xmlDoc.SelectNodes("//*[local-name()='GovernmentAgencyGoodsItem']");

			foreach (XmlNode node in nodeList)
			{
				XmlNode parentNode = node.ParentNode;
				parentNode.RemoveChild(node);
			}

			nodeList = xmlDoc.SelectNodes("//*[local-name()='AdditionalInformation']/*[local-name()='StatementCode']");

			foreach (XmlNode node in nodeList)
			{
				string code = node.InnerText;
				if (code != null && !lookup.ContainsKey(code))
				{
					var addInfo = Factory.New<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
					Factory.EnqueueDelete(typeof(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo), addInfo.PK);
					addInfo.CSI_Code = code;
					lookup.Add(code, addInfo);
				}
			}
			return lookup;
		}

		#endregion  //End: "TestMergeOperation_GB_CDS"

		#region TestClassTypes_GB_CDS

		public override void TestClassTypesBeingUsed()
		{
			TestClassTypesBeingUsed_Core(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
		}

		protected override void InspectVariousObjects_ToTestClassTypes(EU.Business.Declaration.JobDeclaration dec)
		{
			const string CDS = "GB CDS";

			var invoice = dec.Invoices[0];
			var invLine = invoice.InvoiceLines[0];

			CheckClassType(CDS, typeof(JobDeclaration), dec);
			CheckClassType(CDS, typeof(JobComInvoiceHeader), invoice);
			CheckClassType(CDS, typeof(JobComInvoiceLine), invLine);

			var entry = dec.CustomsEntryHeaders[0];
			var entryLine = entry.AllEntryLines[0];

			CheckClassType(CDS, typeof(Business.Declaration.CusEntryHeader), entry);
			CheckClassType(CDS, typeof(CusEntryLine), entryLine);

			var gbEntry = entry as Business.Declaration.CusEntryHeader;
			var gbEntryLine = entryLine as CusEntryLine;

			var list = new List<object>();

			list.AddRange(dec.AdditionalInfos);
			list.AddRange(invoice.AdditionalInfos);
			list.AddRange(invLine.AdditionalInfos);
			list.AddRange(gbEntry.AdditionalInfos);
			list.AddRange(gbEntryLine.AdditionalInfos);

			foreach (var obj in list)
			{
				CheckClassType(CDS, typeof(AdditionalInfo), obj);
			}

			list.Clear();
			list.AddRange(dec.SupportingDocuments);
			list.AddRange(invoice.SupportingDocuments);
			list.AddRange(invLine.SupportingDocuments);
			list.AddRange(gbEntry.SupportingDocuments);
			list.AddRange(gbEntryLine.SupportingDocuments);

			foreach (var obj in list)
			{
				CheckClassType(CDS, typeof(SupportingDocument), obj);
			}

			list.Clear();
			list.AddRange(dec.PreviousDocuments);
			list.AddRange(invoice.PreviousDocuments);
			list.AddRange(invLine.PreviousDocuments);
			list.AddRange(gbEntry.PreviousDocuments);
			list.AddRange(gbEntryLine.PreviousDocuments);

			foreach (var obj in list)
			{
				CheckClassType(CDS, typeof(PreviousDocument), obj);
			}
		}

		#endregion  //End: "TestClassTypes_GB_CDS"

		public override void TestCustomsOfficeRequirementHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertType<Business.JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
		}

		public void TestDoIfPaymentMethodAndDefermentAccountNumberBothAreSet_IsUCCCompliant()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", Core.Constants.CountryCodes.UnitedKingdom);
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "Declarant";
			declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "5678", Core.Constants.CountryCodes.UnitedKingdom);
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.Address1 = "Address";
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OA_DeclarantAddress = declarantAddress.PK;
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cei = dec.CustomsEntryInstructions.AddNew();

			var invoice = dec.Invoices.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = cei.PK;
			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CEI = cei.PK;
			var invLine3 = invoice.InvoiceLines.AddNew();
			invLine3.JI_CEI = cei.PK;

			invLine1.ZG_MethodOfPayment = "E";
			invLine2.ZG_MethodOfPayment = "D";
			invLine3.ZG_MethodOfPayment = "E";

			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
			dec.JE_DefermentAccountNumber = "1234567";

			CombineAssertions("Supporting docs should be created for InvLine1 & Line3", () =>
			{
				Assert("C506 1..7 should not be on DEC", !dec.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 1..7 should not be on DEC", !dec.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 guaranteenotrequired should not be on DEC", !dec.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && x.CSI_Actions == "C" && x.CSI_Availability == "C"));
				Assert("C506 1..7 should be on InvLine1", invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 1..7 should not be on InvLine1", !invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 guaranteenotrequired should be on InvLine1", invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && x.CSI_Actions == "C" && x.CSI_Availability == "C"));
				Assert("C506 1..7 should not be on InvLine2", !invLine2.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 guaranteenotrequired should not be on InvLine2", !invLine2.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && x.CSI_Actions == "C" && x.CSI_Availability == "C"));
				Assert("C506 1..7 should be on InvLine3", invLine3.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C505 guaranteenotrequired should be on InvLine3", invLine3.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C505" && x.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && x.CSI_Actions == "C" && x.CSI_Availability == "C"));
			});

			foreach (JobComInvoiceLine invoiceLine in dec.InvoiceLines)
			{
				CombineAssertions("Authorisations for GB1234 should be created", () =>
				{
					Assert(((Business.Declaration.CusEntryInstruction)invoiceLine.EntryInstruction).CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.AGC_Code == "DPO" && x.AGC_Number == "GB1234"));
					Assert(((Business.Declaration.CusEntryInstruction)invoiceLine.EntryInstruction).CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.AGC_Code == "CGU" && x.AGC_Number == "GB1234"));
				});
			}

			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			dec.JE_DefermentAccountNumber = "7654321";

			CombineAssertions("Supporting docs for C506 should be updated", () =>
			{
				Assert("C506 1..7 should not be on InvLine1", !invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C506 7..1 should be on InvLine1", invLine1.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO7654321"));
				Assert("C506 7..1 should not be on InvLine2", !invLine2.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO7654321"));
				Assert("C506 1..7 should not be on InvLine3", !invLine3.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO1234567"));
				Assert("C506 7..1 should be on InvLine3", invLine3.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == "C506" && x.CSI_ReferenceNumber == "GBDPO7654321"));
			});

			foreach (JobComInvoiceLine invoiceLine in dec.InvoiceLines)
			{
				CombineAssertions("Authorisations for GB5678 should be created", () =>
				{
					Assert(((Business.Declaration.CusEntryInstruction)invoiceLine.EntryInstruction).CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.AGC_Code == "DPO" && x.AGC_Number == "GB5678"));
					Assert(((Business.Declaration.CusEntryInstruction)invoiceLine.EntryInstruction).CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.AGC_Code == "CGU" && x.AGC_Number == "GB5678"));
				});
			}
		}

		public void TestDefermentNumberAndAuthorisationAreAddedWhenInvoiceLinesCreatedInDifferentOrders()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "MY CORP PTY";
			importer.OH_Code = "MYCORP";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI12", "GB");

			// Add invoice lines then deferment number
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_OH_Importer = importer.PK;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.ZG_MethodOfPayment = Constants.MethodOfPayment.DeferredPayment;
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
				declaration.JE_DefermentAccountNumber = "1234567";

				AssertCollectionContains(invoiceLine1.SupportingDocuments.Cast<SupportingDocument>(),
					d => d.CSI_Code == Constants.DocumentCodes.C505 && d.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && d.CSI_Actions == "C" && d.CSI_Availability == "C");
				AssertCollectionContains(invoiceLine1.SupportingDocuments.Cast<SupportingDocument>(),
					d => d.CSI_Code == Constants.DocumentCodes.C506 && d.CSI_ReferenceNumber == "GBDPO1234567");

				AssertCollectionNotContains(invoiceLine2.SupportingDocuments.Cast<SupportingDocument>(), d => d.CSI_Code == Constants.DocumentCodes.C505);
				AssertCollectionNotContains(invoiceLine2.SupportingDocuments.Cast<SupportingDocument>(), d => d.CSI_Code == Constants.DocumentCodes.C506);

				AssertCollectionContains(declaration.CusEntryInstruction.CusAuthorizationUsages,
					a => a.AGC_Code == CDSAuthorisationHeaderTypeList.Codes.ComprehensiveGuarantee && a.AGC_Number == "GBEORI12" && a.AGC_OH_Owner == declaration.Importer.PK);
				AssertCollectionContains(declaration.CusEntryInstruction.CusAuthorizationUsages,
					a => a.AGC_Code == CDSAuthorisationHeaderTypeList.Codes.DeferredPayment && a.AGC_Number == "GBEORI12" && a.AGC_OH_Owner == declaration.Importer.PK);
			});

			// Add deferment number then invoice lines
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
				declaration.JE_DefermentAccountNumber = "1234567";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.ZG_MethodOfPayment = Constants.MethodOfPayment.DeferredPayment;
				var invoiceLine2 = invoice.InvoiceLines.AddNew();

				AssertCollectionContains(invoiceLine1.SupportingDocuments.Cast<SupportingDocument>(),
					d => d.CSI_Code == Constants.DocumentCodes.C505 && d.CSI_ReferenceNumber == "GBCGUguaranteenotrequired" && d.CSI_Actions == "C" && d.CSI_Availability == "C");
				AssertCollectionContains(invoiceLine1.SupportingDocuments.Cast<SupportingDocument>(),
					d => d.CSI_Code == Constants.DocumentCodes.C506 && d.CSI_ReferenceNumber == "GBDPO1234567");

				AssertCollectionNotContains(invoiceLine2.SupportingDocuments.Cast<SupportingDocument>(), d => d.CSI_Code == Constants.DocumentCodes.C505);
				AssertCollectionNotContains(invoiceLine2.SupportingDocuments.Cast<SupportingDocument>(), d => d.CSI_Code == Constants.DocumentCodes.C506);

				AssertCollectionContains(declaration.CusEntryInstruction.CusAuthorizationUsages,
					a => a.AGC_Code == CDSAuthorisationHeaderTypeList.Codes.ComprehensiveGuarantee && a.AGC_Number == "GBEORI12" && a.AGC_OH_Owner == declaration.Importer.PK);
				AssertCollectionContains(declaration.CusEntryInstruction.CusAuthorizationUsages,
					a => a.AGC_Code == CDSAuthorisationHeaderTypeList.Codes.DeferredPayment && a.AGC_Number == "GBEORI12" && a.AGC_OH_Owner == declaration.Importer.PK);
			});
		}

		public void TestEntryStyleListImport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Env.CurrentCompany.Country.Code, parent: eunId);
				helper.CreateNewOrGetExistingDataGrouping(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, parent: eunId);
				var europeanUnionDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: europeanUnionDataGrouping);
				helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "Entry style EU customs");
				var entryType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType;
				helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListImport.Codes.ImportFromEFTAMember, EntryStyleListImport.Descriptions.ImportFromEFTAMember, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListImport.Codes.ImportFromSpecialTerritory, EntryStyleListImport.Descriptions.ImportFromSpecialTerritory, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListImport.Codes.ImportNormal, EntryStyleListImport.Descriptions.ImportNormal, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, entryType, MessageTypeList.Codes.Import);
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var list = declaration.Lookups.EntryStyleList;
				var codesAsString = list.CodesAsString;
				CombineAssertions("CDS expect CO, IM", () =>
				{
					AssertEquals(2, declaration.Lookups.EntryStyleList.Count);
					AssertContains(EntryStyleListImport.Codes.ImportFromSpecialTerritory, codesAsString);
					AssertContains(EntryStyleListImport.Codes.ImportNormal, codesAsString);
					AssertEquals("Import of Goods from a Special Territory of the Community", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportFromSpecialTerritory));
					AssertEquals("Import of Goods (All Other not covered by CO)", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportNormal));
				});
			}
		}

		public void TestEntryStyleListExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Env.CurrentCompany.Country.Code, parent: eunId);
			helper.CreateNewOrGetExistingDataGrouping(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, parent: eunId);
			var europeanUnionDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: europeanUnionDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "Entry style EU customs");
			var entryType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType;
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListExport.Codes.ExportToEFTAMember, EntryStyleListExport.Descriptions.ExportToEFTAMember, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, entryType, MessageTypeList.Codes.Export);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListExport.Codes.ExportToSpecialTerritory, EntryStyleListExport.Descriptions.ExportToSpecialTerritory, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, entryType, MessageTypeList.Codes.Export);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListExport.Codes.ExportNormal, EntryStyleListExport.Descriptions.ExportNormal, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, entryType, MessageTypeList.Codes.Export);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var list = declaration.Lookups.EntryStyleList;
			var codesAsString = list.CodesAsString;
			CombineAssertions("CDS expect CO, EX", () =>
			{
				AssertEquals(2, declaration.Lookups.EntryStyleList.Count);
				AssertContains(EntryStyleListExport.Codes.ExportNormal, codesAsString);
				AssertContains(EntryStyleListExport.Codes.ExportToSpecialTerritory, codesAsString);
				AssertEquals("Export or re-export of goods outside of the customs territory of the Union", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportNormal));
				AssertEquals("Trade of Union goods between EU customs territory not covered by the Council Directives 2006/112/EC or 2008/118/EC", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportToSpecialTerritory));
			});
		}

		public override void TestCustomsOfficeOfExit()
		{
			JobDeclaration decEcs = Factory.New<JobDeclaration>();
			decEcs.JE_MessageType = "EXP";
			AssertEquals("", decEcs.OfficeOfExit);
			decEcs.JE_CustomsOffice = "GB000001";
			AssertEquals("GB000001", decEcs.OfficeOfExit);

			decEcs.JE_MessageType = "1";
			decEcs.JE_ApplicationCode = "EMC";
			Assert(decEcs.IsEMCS);
		}

		public void TestSetIsGVMSPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var europeanUnionDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: europeanUnionDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port Code");

			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT001", "Test Port 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "GVMS", "GVMS Attribute");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT002", "Test Port 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Export, "GVMS Attribute");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = "CDS";
			dec.JE_GoodsLocation = "";
			dec.JE_SubLocationOfGoods = "";
			Assert("Port is not GVMS", !dec.JE_IsGvmsPort);
			dec.JE_GoodsLocation = "PORT001";
			Assert("Port is GVMS", dec.JE_IsGvmsPort);
			dec.JE_GoodsLocation = "";
			dec.JE_SubLocationOfGoods = "PORT002";
			Assert("Port is not GVMS", !dec.JE_IsGvmsPort);
		}

		public void TestSetAdditionalInfoIsGVMSAndShippingLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var europeanUnionDataGrouping =
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes
					.CustomsDeclarationService);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port,
				"Port Code");
			helper.CreateCusCodeListWithAttribute(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT001", "Test Port 1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "GVMS", "GVMS Attribute");
			Factory.Save();

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "Shipping Line Name";
			shippingLine.OH_Code = "1";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_OH_ShippingLine = shippingLine.PK;

			CombineAssertions("Declaration Location and Sub-Location not set to a GVMS Port", () =>
			{
				Assert("Additional Info RRS01 not present", !dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01"));
				Assert("Port is not GVMS", !dec.JE_IsGvmsPort);
			});

			dec.JE_GoodsLocation = "PORT001";

			CombineAssertions("Declaration Location is a GVMS Port", () =>
			{
				Assert("Additional Info RRS01 present", dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01"));
				Assert("Additional Info Description Name is correct", dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01" && x.CSI_Description == "Shipping Line Name"));
				Assert("Port is GVMS", dec.JE_IsGvmsPort);
				shippingLine.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
				dec.JE_IsGvmsPort = false;
				dec.JE_IsGvmsPort = true;
				Assert("Additional Info Description Name is correct", dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == "RRS01" && x.CSI_Description == "GB999999999888"));
			});
			var ai = dec.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.CSI_Code == "RRS01");
			dec.JE_IsGvmsPort = false;
			Factory.Save();
			Assert(ai.IsDeleted);
		}

		public void TestNIMode()
		{
			base.PrepNITestLocations();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";

			// Barcelona->Belfast
			dec.JE_RL_NKOrigin = "ESBCN";
			dec.JE_RL_NKFinalDestination = "GBBEL";
			Assert(dec.JE_NorthernIrelandMode == "NII");

			// Chester -> Belfast
			dec.JE_RL_NKOrigin = "GBCEG";
			dec.JE_RL_NKFinalDestination = "GBBEL";
			Assert(dec.JE_NorthernIrelandMode == "G2N");
			Assert(dec.JE_MessageType == MessageTypeList.Codes.Import);

			// Belfast -> Chester
			dec.JE_RL_NKOrigin = "GBBEL";
			dec.JE_RL_NKFinalDestination = "GBCEG";
			Assert(dec.JE_NorthernIrelandMode == "N2G");

			// Belfast -> Barcelona
			dec.JE_RL_NKFinalDestination = "ESBCN";
			Assert(dec.JE_NorthernIrelandMode == "NIE");
		}

		public void TestIsUCC5()
		{
			var declaration = GetNewBusinessObject() as JobDeclaration;
			CombineAssertions(() =>
			{
				AssertEquals("Disabled when business object is not JobDeclaration", false, declaration.Configuration.IsUCC5(Factory.New<DummyBusinessObject>()));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Enabled for export", true, declaration.Configuration.IsUCC5(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Enabled for import", true, declaration.Configuration.IsUCC5(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("Disabled for other", false, declaration.Configuration.IsUCC5(declaration));
			});
		}

		public override void TestAddFiscalReferenceWhenSettingZG_UsePostPonedVatAccounting()
		{
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTR");
			relatedParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789", Env.CurrentCompany.Country);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTI");
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "987654321", Env.CurrentCompany.Country);

			var euObj = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.UnitedKingdom);
			euObj.ZO_UseFr3FiscalRepresentation = true;
			var importerRelatedParty = importer.AllRelatedParties.AddNew();
			importerRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;
			importerRelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			importerRelatedParty.PR_OH_RelatedParty = relatedParty.PK;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.CustomsEntryInstructions.AddNew();

			// Verify scenario where the flag is the trigger
			AssertEquals(2, declaration.CustomsEntryInstructions.Count);
			foreach (var entryInstruction in declaration.CustomsEntryInstructions)
			{
				CombineAssertions(() =>
				{
					AssertEquals(1, entryInstruction.FiscalReferences.Count);
					var fiscalReference = entryInstruction.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>().FirstOrDefault();
					AssertEquals(FiscalReferenceCodeList.Codes.FR1_Importer, fiscalReference.CFR_Code);
					AssertEquals("GB123456789", fiscalReference.CFR_Reference);
				});
			}

			// Reset and invert triggers
			declaration.ZG_UsePostponedVatAccounting = false;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.CustomsEntryInstructions.AddNew();
			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.CustomsEntryInstructions.AddNew();

			// Verify scenario where the importer is the trigger
			AssertEquals(4, declaration.CustomsEntryInstructions.Count);
			foreach (var entryInstruction in declaration.CustomsEntryInstructions)
			{
				CombineAssertions(() =>
				{
					AssertEquals(1, entryInstruction.FiscalReferences.Count);
					var fiscalReference = entryInstruction.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>().FirstOrDefault();
					AssertEquals(FiscalReferenceCodeList.Codes.FR1_Importer, fiscalReference.CFR_Code);
					AssertEquals("GB123456789", fiscalReference.CFR_Reference);
				});
			}
		}

		public override void TestAddFiscalReferenceWhenSettingZG_UsePostPonedVatAccountingWhenNoRelatedParty()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTI");
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "987654321", Env.CurrentCompany.Country);

			var euObj = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.UnitedKingdom);
			euObj.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.CustomsEntryInstructions.AddNew();

			// Verify scenario where the flag is the trigger
			AssertEquals(2, declaration.CustomsEntryInstructions.Count);
			foreach (var entryInstruction in declaration.CustomsEntryInstructions)
			{
				CombineAssertions(() =>
				{
					AssertEquals(1, entryInstruction.FiscalReferences.Count);
					var fiscalReference = entryInstruction.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>().FirstOrDefault();
					AssertEquals(FiscalReferenceCodeList.Codes.FR1_Importer, fiscalReference?.CFR_Code);
					AssertEquals("GB987654321", fiscalReference?.CFR_Reference);
				});
			}

			// Reset and invert triggers
			declaration.ZG_UsePostponedVatAccounting = false;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.CustomsEntryInstructions.AddNew();
			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.CustomsEntryInstructions.AddNew();

			// Verify scenario where the importer is the trigger
			AssertEquals(4, declaration.CustomsEntryInstructions.Count);
			foreach (var entryInstruction in declaration.CustomsEntryInstructions)
			{
				CombineAssertions(() =>
				{
					AssertEquals(1, entryInstruction.FiscalReferences.Count);
					var fiscalReference = entryInstruction.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>().FirstOrDefault();
					AssertEquals(FiscalReferenceCodeList.Codes.FR1_Importer, fiscalReference?.CFR_Code);
					AssertEquals("GB987654321", fiscalReference?.CFR_Reference);
				});
			}
		}

		public void TestMasterUcr_CcsukImport()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRELX", "TORQUE LOGISTICS LIMITED at Heathrow", acpCode: "H");
			Factory.Save();

			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "WIS";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.ApplicationCode = "CDS";
			badgeCodeSetting.Direction = "IMP";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.GemsCcsuk;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var dec = (JobDeclaration)GetNewBusinessObject();
			dec.JE_MessageType = "IMP";
			dec.JE_CustomsProfile = "WIS";
			dec.JE_SubLocationOfGoods = "LHRELX";
			dec.JE_MasterBill = "11122222222";

			AssertEquals("HELX11122222222", dec.JE_MasterUCR);
		}

		public void TestGoodsLocationIsPortAttributeGvmsArrived()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port;
			var dataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService;

			helper.CreateNewOrGetExistingCusCodeType(codeType, "Port Code");
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "TESTPORT", "Test Port", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "GvmsArrived", "True");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_EntrySubStyle = "D";
			declaration.JE_GoodsLocation = "";
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_SubStyle = "D";
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_SubStyle = "D";
			AssertEquals("D", declaration.JE_EntrySubStyle);

			declaration.JE_GoodsLocation = "TESTPORT";
			AssertEquals("A", declaration.JE_EntrySubStyle);
			AssertEquals("A", cei1.CEI_SubStyle);
			AssertEquals("A", cei2.CEI_SubStyle);
		}
	}
}
