using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using CusEntryInstruction = Enterprise.Customs.CN.Business.CusEntryInstruction;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	sealed class UniversalShipmentTesting : TestCaseWithFactoryAndMessagingHelpers
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporting()
		{
			using (Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = new TestErrorLogger();

				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "SUP1";
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "IMP1";
				var buyerDocOrgHeader = Factory.New<OrgHeader>();
				buyerDocOrgHeader.OH_FullName = "Buyer 1";
				buyerDocOrgHeader.OH_Code = "BUY1";
				buyerDocOrgHeader.MainAddress.Address1 = "Buyer 1 Address 1";
				var buyerOrgHeader = Factory.New<OrgHeader>();
				buyerOrgHeader.OH_FullName = "Buyer 2";
				buyerOrgHeader.OH_Code = "BUY2";
				buyerOrgHeader.MainAddress.Address1 = "Buyer 2 Address 1";
				var manufacturer = Factory.New<OrgHeader>();
				manufacturer.OH_FullName = "Manufacturer 1";
				manufacturer.OH_Code = "MFC1";
				manufacturer.MainAddress.Address1 = "Manufacturer 2 Address 1";
				var declarant = Factory.New<OrgHeader>();
				declarant.OH_FullName = "Declarant Company";
				declarant.OH_Code = "DEC1";
				declarant.MainAddress.Address1 = "Declarant address 1";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Supplier = supplier.PK;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_DeclarationReference = "B00172039";

				using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.CN.DataTransfer.Testing.TestFiles.UniversalShipmentInput.xml"))
				using (var inputReader = new StringReader(stream.ConvertToUTF8StringAndCloseStream()))
				{
					var inputShipment = inputReader.Parse<UniversalShipment>();
					var reader = new CNJobDeclarationDataObjectReader(inputShipment, logger, Factory);
					var declarationResult = reader.ReadIntoBusinessObject();

					AssertEquals(buyerOrgHeader.PK, declarationResult.JE_OH_Buyer);
					AssertEquals(buyerDocOrgHeader.PK, declarationResult.BuyerDocAddress.OrganisationPK);
					AssertEquals(manufacturer.PK, declarationResult.JE_OH_Manufacturer);
					AssertNotEquals(declarant.MainAddress.PK, declarationResult.JE_OA_DeclarantAddress);

					AssertEquals(1, declarationResult.AdditionalReferenceNumbers.Count);
					Assert(declarationResult.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Any(num => num.CE_EntryType == "SLD" && num.CE_EntryNum == "SLDSLDSLD"));

					var instruction = (CusEntryInstruction)declarationResult.CustomsEntryInstructions[0];
					AssertEquals("Instruction Operation Matters", "担保验放", instruction.OperationMattersAsString);
					AssertEquals("Instruction Other Packages", "散装", instruction.OtherPackagesAsString);
					AssertEquals("Instruction Special Business", "国际赛事", instruction.SpecialBusinessIdentifiersAsString);
					Assert("Instruction Enterprise Qualifications", instruction.EnterpriseQualifications.Cast<EnterpriseQualification>().Any(eq => eq.CY_Code == "000" && eq.CY_Data == "ENTERPRISE QUALIFICATION 1"));

					AssertEquals(0, instruction.Notes.VisibleNotes.Count);
					AssertEquals("REMARKS 0110", instruction.CustomsMessageRemarks);

					AssertEquals("BLBLBL", instruction.BillOfLading);
					AssertEquals(new ZDateTime(2019, 1, 10), instruction.BillOfLadingDate);

					AssertEquals(1, instruction.CIQRequiredDocuments.Count);
					AssertEquals("13", instruction.CIQRequiredDocuments[0].XC_DocumentType);
					AssertEquals(5, instruction.CIQRequiredDocuments[0].XC_NumberOfCopies);
					AssertEquals(6, instruction.CIQRequiredDocuments[0].XC_NumberOfOriginals);

					var invoiceHeader = (JobComInvoiceHeader)declarationResult.Invoices[0];
					AssertEquals(3, invoiceHeader.ContractNumbers.Count);
					Assert(invoiceHeader.ContractNumbers.Any(num => num.J2_ReferenceNumber == "ZZZ"));
					Assert(invoiceHeader.ContractNumbers.Any(num => num.J2_ReferenceNumber == "AAA"));
					Assert(invoiceHeader.ContractNumbers.Any(num => num.J2_ReferenceNumber == "CCC"));

					var invoiceLine = (JobComInvoiceLine)declarationResult.InvoiceLines[0];
					AssertEquals("INGINGING", invoiceLine.CIQIngredient);
					Assert(invoiceLine.CusSupportingDocuments.Cast<CusSupportingDocument>().Any(sup => sup.CSI_Code == "01" && sup.CSI_ReferenceNumber == "SUPSUPSUP"));

					var pqs = invoiceLine.CIQProductQualifications.Cast<CIQProductQualification>();
					AssertEquals(1, pqs.Count());
					Assert(pqs.Any(pq => pq.CSI_Type == "PQD" && pq.CSI_Code == "408" && pq.CSI_ReferenceNumber == "PQDPQDPQD"));
				}
			}
		}

		public void TestExporting()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Business.Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff).PK;
			Factory.SaveForTesting();
			helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "2009891200101", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "未混合芒果汁");
			Factory.SaveForTesting();

			var proxy = Factory.New<OrgHeader>();
			proxy.OH_Code = "Agent";
			proxy.OH_FullName = "Agent Company";
			proxy.CustomsCodes.AddNew("CCD", "111111", "CN");
			proxy.CustomsCodes.AddNew("USC", "222222", "CN");
			proxy.CustomsCodes.AddNew("CIQ", "333333", "CN");

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP1";
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP1";
			var buyerDocOrgHeader = Factory.New<OrgHeader>();
			buyerDocOrgHeader.OH_FullName = "Buyer 1";
			buyerDocOrgHeader.OH_Code = "BUY1";
			buyerDocOrgHeader.MainAddress.Address1 = "Buyer 1 Address 1";
			var buyerOrgHeader = Factory.New<OrgHeader>();
			buyerOrgHeader.OH_FullName = "Buyer 2";
			buyerOrgHeader.OH_Code = "BUY2";
			buyerOrgHeader.MainAddress.Address1 = "Buyer 2 Address 1";
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer 1";
			manufacturer.OH_Code = "MFC1";
			manufacturer.MainAddress.Address1 = "Manufacturer 2 Address 1";

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TBR";
			var brkCrt = broker.Certificates.AddNew();
			brkCrt.XZ_Type = "BRK";
			brkCrt.XZ_RN_NKCountryOfIssuance = "CN";
			brkCrt.XZ_RefNumber = "BRK001";
			brkCrt.XZ_Comment = "Broker Name";
			var cnoCrt = broker.Certificates.AddNew();
			cnoCrt.XZ_Type = "CNO";
			cnoCrt.XZ_RN_NKCountryOfIssuance = "CN";
			cnoCrt.XZ_RefNumber = "CNO001";
			cnoCrt.XZ_Comment = "CNO Name";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "0000";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_PSN = "I am very dangerous";
			subs.DG_FlashPoint = "-4 cc";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_OH_Buyer = buyerOrgHeader.PK;
			declaration.Branch.GB_OH_OrgProxy = proxy.PK;
			declaration.BuyerDocAddress.OrganisationPK = buyerDocOrgHeader.PK;
			declaration.JE_OH_Manufacturer = manufacturer.PK;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_GS_NKCusAgent = "TBR";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "C-LD";
			var cnRefMap = refContainer.CodeMapCollection.AddNew();
			cnRefMap.RCM_RN_NKCountry = "CN";
			cnRefMap.RCM_Code = "12";

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_RC = refContainer.PK;
			cusContainer.CO_ContainerNumber = "CNT1";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CustomsMessageRemarks = "Remarks";
			instruction.BillOfLading = "BLBLBL";
			instruction.BillOfLadingDate = new ZDateTime(2018, 12, 12);
			var eq = instruction.EnterpriseQualifications.AddNew();
			eq.CY_Data = "ENTERPRISE QUALIFICATION 1";
			var special = instruction.SpecialBusinessIdentifiers.AddNew();
			special.CY_Code = "B01";
			var operationMatter = instruction.OperationMatters.AddNew();
			operationMatter.CY_Code = "00";
			var package = instruction.OtherPackages.AddNew();
			package.CY_Code = "01";

			var ciqReqDoc = instruction.CIQRequiredDocuments.AddNew();
			ciqReqDoc.XC_DocumentType = "11";
			ciqReqDoc.XC_NumberOfCopies = 2;
			ciqReqDoc.XC_NumberOfOriginals = 1;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_MarksAndNumbers = "Marks And Numbers";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var contract = invoiceHeader.ContractNumbers.AddNew();
			contract.J2_ReferenceNumber = "CONTRACT";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.CIQIngredient = "INGINGING";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_LinePrice = 30;
			invoiceLine.JI_CIQTariff = "2009891200101";
			var undg = invoiceLine.UNDGs.AddNew();
			undg.LinkDefault(subs);

			var doc = invoiceLine.CusSupportingDocuments.AddNew();
			doc.CSI_Code = "01";
			doc.CSI_ReferenceNumber = "SUPSUPSUP";
			var pq = invoiceLine.CIQProductQualifications.AddNew();
			pq.CSI_Code = "408";
			pq.CSI_ReferenceNumber = "PQDPQDPQD";

			#region Prepare for CusEntryHeader AddInfos

			invoiceHeader.Charges.RemoveAndDeleteAll();
			declaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();

			var charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "OFT";
			charge.J7_Amount = 4;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";

			charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = "ONS";
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_Amount = 5;
			charge.J7_DistributeBy = "VAL";
			charge.J7_FullOrPartialApportionment = "PAA";

			var notifier = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(notifier);

			#endregion

			Factory.SaveForTesting();

			var writer = new CNJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
			var output = writer.GetDataObject(declaration);

			Assert(output.OrganizationAddressCollection.Any(add => add.AddressType.StringEquals("BuyerDocumentaryAddress") && add.OrganizationCode.ToString() == "BUY1"));
			Assert(output.OrganizationAddressCollection.Any(add => add.AddressType.StringEquals("Manufacturer") && add.OrganizationCode.ToString() == "MFC1"));

			AssertEquals(1, output.EntryInstructionCollection.Count);
			var outInstruction = output.EntryInstructionCollection.First();

			var noteAddInfos = outInstruction.AddInfoCollection;
			Assert(noteAddInfos.Any(addinfo =>
				addinfo.Key.StringEquals(Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks)
				&& addinfo.Value.StringEquals("Remarks")
			));
			Assert(noteAddInfos.Any(addinfo =>
				addinfo.Key.StringEquals(Constants.AddInfoKeys.EntryInstruction.BillOfLading)
				&& addinfo.Value.StringEquals("BLBLBL")
			));
			Assert(noteAddInfos.Any(addinfo =>
				addinfo.Key.StringEquals(Constants.AddInfoKeys.EntryInstruction.BillOfLadingDate)
				&& addinfo.Value.StringEquals("2018-12-12T00:00:00")
			));

			var instructionRefs = outInstruction.CustomsReferenceCollection;
			Assert(instructionRefs.Any(@ref => @ref.Type.Code.StringEquals("PKG") && @ref.SubType.Code.StringEquals("01")));
			Assert(instructionRefs.Any(@ref => @ref.Type.Code.StringEquals("OPM") && @ref.SubType.Code.StringEquals("00")));
			Assert(instructionRefs.Any(@ref => @ref.Type.Code.StringEquals("EPQ") && @ref.Reference.StringEquals("ENTERPRISE QUALIFICATION 1")));
			Assert(instructionRefs.Any(@ref => @ref.Type.Code.StringEquals("SBI") && @ref.SubType.Code.StringEquals("B01")));

			var outInsAddGroup = outInstruction.AddInfoGroupCollection;
			AssertEquals(1, outInsAddGroup.Count);
			Assert(outInsAddGroup.Any(group => group.Type.Code.StringEquals("RQD")));
			var addinfos = outInsAddGroup[0].AddInfoCollection;
			AssertEquals(3, addinfos.Count);
			Assert(addinfos.Any(info => info.Key.StringEquals("DocumentType") && info.Value.StringEquals("11")));
			Assert(addinfos.Any(info => info.Key.StringEquals("NumberOfCopies") && info.Value.StringEquals("2")));
			Assert(addinfos.Any(info => info.Key.StringEquals("NumberOfOriginals") && info.Value.StringEquals("1")));

			var outInvoice = output.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals(1, outInvoice.CustomsReferenceCollection.Count);
			Assert(outInvoice.CustomsReferenceCollection.Any(@ref => @ref.Type.Code.StringEquals("CTR") && @ref.Reference.StringEquals("CONTRACT")));

			var outInvoiceLine = outInvoice.CommercialInvoiceLineCollection[0];
			var lineSups = outInvoiceLine.CustomsSupportingInformationCollection;
			AssertEquals(2, lineSups.Count);
			Assert(lineSups.Any(sup => sup.Category.Code.StringEquals("PQD") && sup.Type.Code.StringEquals("408") && sup.ReferenceNumber.StringEquals("PQDPQDPQD")));
			Assert(lineSups.Any(sup => sup.Category.Code.StringEquals("SUP") && sup.Type.Code.StringEquals("01") && sup.ReferenceNumber.StringEquals("SUPSUPSUP")));
			Assert(!outInvoiceLine.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals(nameof(ICIQDataLine.CIQTariffDescription)) && addinfo.Value.StringEquals("未混合芒果汁")));

			var outEntryHeader = output.EntryHeaderCollection.First();
			Assert(outEntryHeader.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("FreightFeeCurrencyCode") && addinfo.Value.StringEquals("CNY")));
			Assert(outEntryHeader.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("FreightFeeMarkCode") && addinfo.Value.StringEquals("3")));

			AssertEquals(1, output.OrganizationAddressCollection.Count(x => x.AddressType.Value == AddressTypes.Declarant));
			var declarantAddress = output.OrganizationAddressCollection.First(x => x.AddressType.Value == AddressTypes.Declarant);
			AssertEquals("Agent Company", declarantAddress.CompanyName);
			AssertEquals(3, declarantAddress.RegistrationNumberCollection.Count);
			Assert(declarantAddress.RegistrationNumberCollection.Any(x => x.Value.Value == "111111"));
			Assert(declarantAddress.RegistrationNumberCollection.Any(x => x.Value.Value == "222222"));
			Assert(declarantAddress.RegistrationNumberCollection.Any(x => x.Value.Value == "333333"));
			Assert(outEntryHeader.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("FreightFeeAmount") && addinfo.Value.StringEquals("4.0000")));
			Assert(outEntryHeader.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("InsuranceFeeCurrencyCode") && addinfo.Value.StringEquals("CNY")));
			Assert(outEntryHeader.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("InsuranceFeeMarkCode") && addinfo.Value.StringEquals("3")));
			Assert(outEntryHeader.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("InsuranceFeeAmount") && addinfo.Value.StringEquals("5.0000")));
			Assert(outEntryHeader.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("MarksAndNumbers") && addinfo.Value.StringEquals("Marks And Numbers")));

			var outEntryLine = outEntryHeader.EntryLineCollection.First();
			Assert(outEntryLine.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("NameOfGoods")));
			Assert(outEntryLine.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("GoodsSpecModel")));
			Assert(outEntryLine.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("UnitPrice")));
			Assert(outEntryLine.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("TotalPrice")));
			Assert(outEntryLine.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("CurrencyCode")));

			var outPackingLine = output.PackingLineCollection.First();
			Assert(outPackingLine.ContainerNumber.StringEquals("CNT1"));
			var outPackedItem = outPackingLine.PackedItemCollection.First();
			Assert(outPackedItem.CommercialInvoiceLineLink == 1);

			Assert(output.AddInfoCollection.Any(item => item.Key.StringEquals("BrokerNumber") && item.Value.StringEquals("BRK001")));
			Assert(output.AddInfoCollection.Any(item => item.Key.StringEquals("BrokerName") && item.Value.StringEquals("Broker Name")));
			Assert(output.AddInfoCollection.Any(item => item.Key.StringEquals("OperatorCardID") && item.Value.StringEquals("CNO001")));
			Assert(output.AddInfoCollection.Any(item => item.Key.StringEquals("OperatorName") && item.Value.StringEquals("CNO Name")));
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
