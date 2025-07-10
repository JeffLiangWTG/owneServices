using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B3ImportDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestIsTotalAmountsEmpty

		public void TestIsTotalAmountsEmpty()
		{
			var footer = new TestB3Footer(wrapper.DocumentPages[3].B3Header);
			var totals = new TotalAmounts();

			AssertEquals(true, footer.IsTotalAmountsEmptyTest(totals));
			totals.Deposit = 0.1m;
			AssertEquals(false, footer.IsTotalAmountsEmptyTest(totals));
		}

		#endregion

		#region TestLVXPageInfos
		public void TestLVXPageInfos()
		{
			var carrier = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "CC01";
			carrier.ZZ4_Description = "CC01Desc";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			jobDeclaration.CustomsEntryHeaders.Add(cusEntryHeader);
			var wrapper = new B3ImportDocumentWrapper(jobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			Assert("LVX should be true.", wrapper.IsLVX);
			var invoice = jobDeclaration.Invoices.AddNew();
			Assert("HasConsolidatedToLVS should be false.", !wrapper.HasConsolidatedToLVS);
			var addDeclaration = Factory.New<JobDeclaration>();
			invoice.AttachToAdditionalDeclaration(addDeclaration);
			Assert("HasConsolidatedToLVS should be true.", wrapper.HasConsolidatedToLVS);
			Assert("OtherReferences should be empty.", wrapper.OtherReferences.IsEmpty);
			invoice.CA_OtherReference = "Test CA Other Reference";
			AssertEquals("Other Reference", "Test CA Other Reference", wrapper.OtherReferences);
			Assert("CarrierCode should be empty.", wrapper.CarrierDescription.IsEmpty);
			jobDeclaration.LVXInvoiceHeader.CA_LVSCarrier = "CC01";
			wrapper = new B3ImportDocumentWrapper(jobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			AssertEquals("CarrierCode", "CC01Desc", wrapper.CarrierDescription);
			jobDeclaration.LVXInvoiceHeader.JZ_InvoiceDate = new ZDateTime(2015, 9, 29);
			AssertEquals("CarrierDate", new ZDateTime(2015, 9, 29), wrapper.CarrierDate);
		}
		#endregion

		#region TestExtraInformation

		public void TestContainerNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			jobDeclaration.CustomsEntryHeaders.Add(cusEntryHeader);
			jobDeclaration.JE_JS = shipment.PK;

			var wrapper = new B3ImportDocumentWrapper(jobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			AssertEquals("0 containers", ZString.Empty, wrapper.ContainerNumber);

			var cusContainer = jobDeclaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "C1";
			var cusContainer2 = jobDeclaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "C2";

			wrapper = new B3ImportDocumentWrapper(jobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			AssertEquals("2 containers", "C1, C2", wrapper.ContainerNumber);

			var cusContainer3 = jobDeclaration.CusContainers.AddNew();
			cusContainer3.CO_ContainerNumber = "C3";
			var cusContainer4 = jobDeclaration.CusContainers.AddNew();
			cusContainer4.CO_ContainerNumber = "C4";
			wrapper = new B3ImportDocumentWrapper(jobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			AssertEquals("4 containers", "C1, C2, C3, C4", wrapper.ContainerNumber);

			var cusContainer5 = jobDeclaration.CusContainers.AddNew();
			cusContainer5.CO_ContainerNumber = "C5";
			wrapper = new B3ImportDocumentWrapper(jobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			AssertEquals("5 containers", "C1, C2, C3, AND 2 MORE CONTAINERS", wrapper.ContainerNumber);
		}

		[TestDate(2021, 5, 26)]
		public void TestExtraInfos()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "CW1";
			staff.GS_Code = "TES";
			staff.GS_GB_HomeBranch = branch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var carrier = Factory.New<Universal.ZZRefCarrierCombined>();
				carrier.ZZ4_Code = "123";
				carrier.ZZ4_Description = "123Desc";
				carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "Importer";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "HBL1";

				var jobDeclaration = Factory.New<JobDeclaration>();
				jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var cusEntryHeader = Factory.New<CusEntryHeader>();
				cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				jobDeclaration.JE_DeclarationReference = "B00100100";
				jobDeclaration.JE_OH_Importer = importer.PK;
				jobDeclaration.CustomsEntryHeaders.Add(cusEntryHeader);
				jobDeclaration.JE_JS = shipment.PK;
				jobDeclaration.JE_CarrierCode = "123";
				jobDeclaration.JE_OwnerRef = "BBB";
				jobDeclaration.JE_VesselName = "Vessel1";
				jobDeclaration.JE_VoyageFlightNo = "Voyage1";
				jobDeclaration.JE_MasterBill = "MB123";

				var cusContainer = jobDeclaration.CusContainers.AddNew();
				cusContainer.CO_ContainerNumber = "C1";

				var wrapper = new B3ImportDocumentWrapper(jobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));

				AssertEquals("Declaration.JE_DeclarationReference", "B00100100", wrapper.JobNumber);
				AssertEquals("Declaration.CusContainers", "C1", wrapper.ContainerNumber);
				AssertEquals("Declaration.CA_CarrierCode", "123Desc", wrapper.CarrierDescription);
				AssertEquals("Declaration.JE_OwnerRef", "BBB", wrapper.ImporterRefNo);
				AssertEquals("Declaration.JE_VesselName/JE_VoyageFlightNo", "Vessel1/Voyage1", wrapper.Vessel);
				AssertEquals("Declaration.JE_MasterBill", "MB123", wrapper.BillOfLading);
				AssertEquals("Declaration.JE_OH_Importer", "Importer", wrapper.ClientCode);
				AssertEquals("PrintedBy should be from CurrentUser", "CW1", wrapper.PrintedBy);
			}
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				var broker = Factory.New<GlbStaff>();
				broker.GS_FullName = "TestBroker";
				broker.GS_Code = "BRO";

				var buyer = Factory.New<OrgHeader>();
				buyer.OH_Code = "Buyer";

				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.JS_HouseBill = "HBL2";

				var jobDeclaration2 = Factory.New<JobDeclaration>();
				jobDeclaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				var cusEntryHeader2 = Factory.New<CusEntryHeader>();
				cusEntryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

				jobDeclaration2.JE_DeclarationReference = "B00200200";
				jobDeclaration2.CustomsEntryHeaders.Add(cusEntryHeader2);
				jobDeclaration2.JE_JS = shipment2.PK;
				jobDeclaration2.JE_VesselName = "Vessel1";
				jobDeclaration2.JE_VoyageFlightNo = "Voyage1";
				jobDeclaration2.JE_MasterBill = "MB123";
				jobDeclaration2.JE_GS_NKCusAgent = broker.GS_Code;

				var cusContainer2 = jobDeclaration2.CusContainers.AddNew();
				cusContainer2.CO_ContainerNumber = "C2";

				var invoice = jobDeclaration2.Invoices.AddNew();
				invoice.CA_LVSCarrier = "123";
				invoice.CA_OtherReference = "CCC";
				invoice.JZ_OH_Buyer = buyer.PK;

				var wrapper2 = new B3ImportDocumentWrapper(jobDeclaration2.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));

				Assert("LVX should be true.", wrapper2.IsLVX);
				AssertEquals("Declaration.JE_DeclarationReference", "B00200200", wrapper2.JobNumber);
				AssertEquals("Declaration.CusContainers", "C2", wrapper2.ContainerNumber);
				AssertEquals("Declaration.LVXInvoiceHeader.CA_LVSCarrier", "123Desc", wrapper2.CarrierDescription);
				AssertEquals("Declaration.LVXInvoiceHeader.CA_OtherReference", "CCC", wrapper2.ImporterRefNo);
				AssertEquals("Declaration.JE_VesselName/JE_VoyageFlightNo", "Vessel1/Voyage1", wrapper2.Vessel);
				AssertEquals("Declaration.JE_MasterBill", "MB123", wrapper2.BillOfLading);
				AssertEquals("Declaration.LVXInvoiceHeader.JZ_OH_Buyer", "Buyer", wrapper2.ClientCode);
				AssertEquals("PrintedBy should be from broker", "TestBroker", wrapper2.PrintedBy);
			}
		}
		#endregion

		#region TestDocumentPages

		public void TestPrintLineNumberInUnderB3SubHeaderWhenLVSForConsolidation()
		{
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";

			var helper = new DeclarationTestHelper(factory, true);
			declaration.WarehouseDocAddress.OrganisationPK = helper.CreateOrganisation("WAREHOUSE NAME", "CATOR").PK;
			declaration.WarehouseDocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "035", factory.Load<RefCountry>(Constants.CountryGuids.Canada));

			var brokerBranch = factory.New<GlbBranch>();
			brokerBranch.GB_Phone = "789";

			broker = factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			broker.GS_WorkPhone = "654";
			broker.GS_PublishWorkPhone = true;
			broker.GS_GB_HomeBranch = brokerBranch.PK;

			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			declaration.ReleaseStatuses.Load();
			var number1 = declaration.AdditionalReferenceNumbers.AddNew();
			number1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number1.CE_EntryNum = "123456";

			//Invoice 1
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.JZ_RN_NKDefaultOrigin = Constants.CountryCodes.Japan;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 1, 1, 1000, 1231231, 666, 150, Constants.Weight.Kilograms, 10);

			invoiceLine.CA_TRSNumber = "123456";
			invoiceLine.JI_Description = desc + "<EXTRA CHARACTERS>";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100001";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100002";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100003";

			//Invoice 2
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "2";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100000";

			//Invoice 3
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "3";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100000";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			wrapper = new B3ImportDocumentWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));

			AssertEquals("Page1.Line1.Line1.B3LineNumber", (short)1, wrapper.DocumentPages[0].Line1.Line1.B3LineNumber);
			AssertEquals("Page1.Line1.Line1.NumberInOrderForB3SubLines", 1, wrapper.DocumentPages[0].Line1.Line1.B3SubHeaderNumberForLVX);

			AssertNull("Page1.Line2.Line1.B3LineNumber", wrapper.DocumentPages[0].Line2.Line1);
			AssertNull("Page1.Line3.Line1.B3LineNumber", wrapper.DocumentPages[0].Line3.Line1);

			AssertEquals("Page1.Line4.Line1.B3LineNumber", (short)2, wrapper.DocumentPages[0].Line4.Line1.B3LineNumber);
			AssertEquals("Page1.Line4.Line1.B3LineNumber", (short)3, wrapper.DocumentPages[0].Line5.Line1.B3LineNumber);
			AssertEquals("Page2.Line1.Line1.B3LineNumber", (short)4, wrapper.DocumentPages[1].Line1.Line1.B3LineNumber);
			AssertEquals("Page3.Line1.Line1.B3LineNumber", (short)5, wrapper.DocumentPages[2].Line1.Line1.B3LineNumber);
			AssertEquals("Page4.Line1.Line1.B3LineNumber", (short)6, wrapper.DocumentPages[3].Line1.Line1.B3LineNumber);
		}

		[TestDate(2010, 8, 16)]
		public void TestDocumentPages()
		{
			AssertEquals("TransactionNumberFormated", "12345 - 000067897", wrapper.TransactionNumberFormated);
			AssertEquals("HideCargoControlContinuationSheet", true, wrapper.HideCargoControlContinuationSheet);

			AssertEquals("DocumentPages.Count", 4, wrapper.DocumentPages.Count);

			//Invoice 1
			//Page 1
			AssertNotNull("Page1.B3Header", wrapper.DocumentPages[0].B3Header);     //B3Header - first page only
			AssertEquals("B3Comments", ZString.Empty, wrapper.DocumentPages[0].B3Header.B3Comments);
			AssertNotNull("Page1.B3SubHeader", wrapper.DocumentPages[0].B3SubHeader); //B3SubHeader - new page for invoice 1
			AssertNotNull("Page1.Line1", wrapper.DocumentPages[0].Line1);
			AssertNotNull("Page1.Line1.Line1", wrapper.DocumentPages[0].Line1.Line1); //1st class line
			AssertNotNull("Page1.Line1.Line2", wrapper.DocumentPages[0].Line1.Line2); //1st duty line

			AssertNotNull("Page1.Line2", wrapper.DocumentPages[0].Line2);
			AssertNull("Page1.Line2.Line1", wrapper.DocumentPages[0].Line2.Line1);    //1st class line defined above
			AssertNotNull("Page1.Line2.Line2", wrapper.DocumentPages[0].Line2.Line2); //2nd duty line

			AssertNotNull("Page1.Line3", wrapper.DocumentPages[0].Line3);
			AssertNull("Page1.Line3.Line1", wrapper.DocumentPages[0].Line3.Line1);    //1st class line defined above
			AssertNotNull("Page1.Line3.Line2", wrapper.DocumentPages[0].Line3.Line2); //3rd duty line

			AssertNotNull("Page1.Line4", wrapper.DocumentPages[0].Line4);
			AssertNotNull("Page1.Line4.Line1", wrapper.DocumentPages[0].Line4.Line1); //2nd class line
			AssertNull("Page1.Line4.Line2", wrapper.DocumentPages[0].Line4.Line2);    //No duty lines

			AssertNull("B3Footer", wrapper.DocumentPages[0].B3Footer);          //B3Footer - last page only

			//Page 2
			AssertNull("Page2.B3Header", wrapper.DocumentPages[1].B3Header);      //B3Header - not required
			AssertNull("Page2.B3SubHeader", wrapper.DocumentPages[1].B3SubHeader);    //B3SubHeader - not required on 2nd page of invoice 1
			AssertNotNull("Page2.Line1", wrapper.DocumentPages[1].Line1);
			AssertNotNull("Page2.Line1.Line1", wrapper.DocumentPages[1].Line1.Line1); //4th class line
			AssertNull("Page2.Line1.Line2", wrapper.DocumentPages[1].Line1.Line2);    //No duty lines

			AssertNull("Page2.Line2", wrapper.DocumentPages[1].Line2);
			AssertNull("Page2.Line3", wrapper.DocumentPages[1].Line3);
			AssertNull("Page2.Line4", wrapper.DocumentPages[1].Line4);
			AssertNull("Page2.B3Footer", wrapper.DocumentPages[1].B3Footer);      //B3Footer - last page only

			//Invoice 2
			//Page 3
			AssertNull("Page3.B3Header", wrapper.DocumentPages[2].B3Header);      //B3Header - not required
			AssertNotNull("Page3.B3SubHeader", wrapper.DocumentPages[2].B3SubHeader);   //B3SubHeader - new page for invoice 2
			AssertNotNull("Page3.Line1", wrapper.DocumentPages[2].Line1);
			AssertNotNull("Page3.Line1.Line1", wrapper.DocumentPages[2].Line1.Line1); //1st class line
			AssertNull("Page3.Line1.Line2", wrapper.DocumentPages[2].Line1.Line2);    //No duty lines

			AssertNull("Page3.Line2", wrapper.DocumentPages[2].Line2);
			AssertNull("Page3.Line3", wrapper.DocumentPages[2].Line3);
			AssertNull("Page3.Line4", wrapper.DocumentPages[2].Line4);
			AssertNull("Page3.B3Footer", wrapper.DocumentPages[2].B3Footer);      //B3Footer - last page only

			//Invoice 3
			//Page 4
			AssertNull("Page4.B3Header", wrapper.DocumentPages[3].B3Header);      //B3Header - not required
			AssertNotNull("Page4.B3SubHeader", wrapper.DocumentPages[3].B3SubHeader);   //B3SubHeader - new page for invoice 3
			AssertNotNull("Page4.Line1", wrapper.DocumentPages[3].Line1);
			AssertNotNull("Page4.Line1.Line1", wrapper.DocumentPages[3].Line1.Line1); //1st class line
			AssertNull("Page4.Line1.Line2", wrapper.DocumentPages[3].Line1.Line2);    //No duty lines

			AssertNull("Page4.Line2", wrapper.DocumentPages[3].Line2);
			AssertNull("Page4.Line3", wrapper.DocumentPages[3].Line3);
			AssertNull("Page4.Line4", wrapper.DocumentPages[3].Line4);
			AssertNotNull("Page4.B3Footer", wrapper.DocumentPages[3].B3Footer);     //B3Footer - required on last page
		}

		[TestDate(2010, 8, 16)]
		public void TestDocumentPagesWithB3Comments()
		{
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description, "Note to be printed on B3");
			wrapper = new B3ImportDocumentWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));

			AssertEquals("TransactionNumberFormated", "12345 - 000067897", wrapper.TransactionNumberFormated);
			AssertEquals("HideCargoControlContinuationSheet", true, wrapper.HideCargoControlContinuationSheet);

			AssertEquals("DocumentPages.Count", 4, wrapper.DocumentPages.Count);

			//Invoice 1
			//Page 1
			AssertNotNull("Page1.B3Header", wrapper.DocumentPages[0].B3Header);     //B3Header - first page only
			AssertEquals("B3Comments", "Note to be printed on B3", wrapper.DocumentPages[0].B3Header.B3Comments);
			AssertNotNull("Page1.B3SubHeader", wrapper.DocumentPages[0].B3SubHeader); //B3SubHeader - new page for invoice 1
			AssertNotNull("Page1.Line1", wrapper.DocumentPages[0].Line1);
			AssertNotNull("Page1.Line1.Line1", wrapper.DocumentPages[0].Line1.Line1); //1st class line
			AssertNotNull("Page1.Line1.Line2", wrapper.DocumentPages[0].Line1.Line2); //1st duty line

			AssertNotNull("Page1.Line2", wrapper.DocumentPages[0].Line2);
			AssertNull("Page1.Line2.Line1", wrapper.DocumentPages[0].Line2.Line1);    //1st class line defined above
			AssertNotNull("Page1.Line2.Line2", wrapper.DocumentPages[0].Line2.Line2); //2nd duty line

			AssertNotNull("Page1.Line3", wrapper.DocumentPages[0].Line3);
			AssertNull("Page1.Line3.Line1", wrapper.DocumentPages[0].Line3.Line1);    //1st class line defined above
			AssertNotNull("Page1.Line3.Line2", wrapper.DocumentPages[0].Line3.Line2); //3rd duty line

			AssertNull("Page1.Line4", wrapper.DocumentPages[0].Line4);

			AssertNull("B3Footer", wrapper.DocumentPages[0].B3Footer);          //B3Footer - last page only

			//Page 2
			AssertNull("Page2.B3Header", wrapper.DocumentPages[1].B3Header);      //B3Header - not required
			AssertNull("Page2.B3SubHeader", wrapper.DocumentPages[1].B3SubHeader);    //B3SubHeader - not required on 2nd page of invoice 1

			AssertNotNull("Page1.Line1", wrapper.DocumentPages[1].Line1);
			AssertNotNull("Page1.Line1.Line1", wrapper.DocumentPages[1].Line1.Line1); //2nd class line
			AssertNull("Page1.Line1.Line2", wrapper.DocumentPages[1].Line1.Line2);    //No duty lines

			AssertNotNull("Page2.Line2", wrapper.DocumentPages[1].Line2);
			AssertNotNull("Page2.Line2.Line1", wrapper.DocumentPages[1].Line2.Line1); //3rd class line
			AssertNull("Page2.Line2.Line2", wrapper.DocumentPages[1].Line2.Line2);    //No duty lines

			AssertNotNull("Page2.Line3", wrapper.DocumentPages[1].Line3);
			AssertNotNull("Page2.Line3.Line1", wrapper.DocumentPages[1].Line3.Line1); //4th class line
			AssertNull("Page2.Line3.Line2", wrapper.DocumentPages[1].Line3.Line2);    //No duty lines

			AssertNull("Page2.Line4", wrapper.DocumentPages[1].Line4);
			AssertNull("Page2.B3Footer", wrapper.DocumentPages[1].B3Footer);      //B3Footer - last page only

			//Invoice 2
			//Page 3
			AssertNull("Page3.B3Header", wrapper.DocumentPages[2].B3Header);      //B3Header - not required
			AssertNotNull("Page3.B3SubHeader", wrapper.DocumentPages[2].B3SubHeader);   //B3SubHeader - new page for invoice 2
			AssertNotNull("Page3.Line1", wrapper.DocumentPages[2].Line1);
			AssertNotNull("Page3.Line1.Line1", wrapper.DocumentPages[2].Line1.Line1); //1st class line
			AssertNull("Page3.Line1.Line2", wrapper.DocumentPages[2].Line1.Line2);    //No duty lines

			AssertNull("Page3.Line2", wrapper.DocumentPages[2].Line2);
			AssertNull("Page3.Line3", wrapper.DocumentPages[2].Line3);
			AssertNull("Page3.Line4", wrapper.DocumentPages[2].Line4);
			AssertNull("Page3.B3Footer", wrapper.DocumentPages[2].B3Footer);      //B3Footer - last page only

			//Invoice 3
			//Page 4
			AssertNull("Page4.B3Header", wrapper.DocumentPages[3].B3Header);      //B3Header - not required
			AssertNotNull("Page4.B3SubHeader", wrapper.DocumentPages[3].B3SubHeader);   //B3SubHeader - new page for invoice 3
			AssertNotNull("Page4.Line1", wrapper.DocumentPages[3].Line1);
			AssertNotNull("Page4.Line1.Line1", wrapper.DocumentPages[3].Line1.Line1); //1st class line
			AssertNull("Page4.Line1.Line2", wrapper.DocumentPages[3].Line1.Line2);    //No duty lines

			AssertNull("Page4.Line2", wrapper.DocumentPages[3].Line2);
			AssertNull("Page4.Line3", wrapper.DocumentPages[3].Line3);
			AssertNull("Page4.Line4", wrapper.DocumentPages[3].Line4);
			AssertNotNull("Page4.B3Footer", wrapper.DocumentPages[3].B3Footer);     //B3Footer - required on last page
		}

		public void TestDocumentPagesGenerationDoesNotSetPageRelativeLineNumbers()
		{
			AssertEquals("Pre-Condition: DocumentPages generated", 4, wrapper.DocumentPages.Count);
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				AssertEquals("CA_PageRelativeLineNumber is not set", 0, line.CA_PageRelativeLineNumber);
			}
		}

		[ExpectNoExceptions]
		public void TestGetDocumentPages_EmptyDocumentPages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			declaration.CustomsEntryHeaders.Add(cusEntryHeader);
			var wrapper = new B3ImportDocumentWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			AssertEquals("DocumentPages generate 0", 0, wrapper.DocumentPages.Count);
		}

		#endregion

		#region TestB3BInputReleases

		public void TestB3BBlankWhenNoCCNs()
		{
			declaration.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			wrapper = new B3ImportDocumentWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
			AssertEquals("B3Footer.CargoControlNumber is blank when no CCNs", ZString.Empty, wrapper.DocumentPages[3].B3Footer.CargoControlNumber);
		}

		public void TestB3BInputReleases()
		{
			for (var i = 2; i < 114; i++)
			{
				declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = i.ToString();
			}

			AssertEquals("HideCargoControlContinuationSheet", false, wrapper.HideCargoControlContinuationSheet);
			AssertEquals("B3Footer.CargoControlNumber", "B3B", wrapper.DocumentPages[3].B3Footer.CargoControlNumber);

			AssertEquals("B3BInputReleases.Count", 75, wrapper.B3BInputReleases.Count);

			AssertB3BInputRelease(0, 1, "123456", "26");
			for (var i = 1; i < 50; i++)
			{
				var num = i + 1 + i / 25 * 25;
				AssertB3BInputRelease(i, num, i < 64 ? num.ToString() : string.Empty, i < 50 ? (num + 25).ToString() : string.Empty);
			}
		}

		void AssertB3BInputRelease(int index, int num, string ccn1, string ccn2)
		{
			AssertEquals("B3BInputReleases.Number", num, wrapper.B3BInputReleases[index].Number);
			AssertEquals("B3BInputReleases.CCN1", ccn1, wrapper.B3BInputReleases[index].CCN1);
			AssertEquals("B3BInputReleases.CCN2", ccn2, wrapper.B3BInputReleases[index].CCN2);
		}

		#endregion

		#region TestClassificationLineProperties

		public void TestClassificationLineProperties()
		{
			var tariff = JobComInvoiceLineTestHelper.AddTariffRecord(invoiceLine);

			AssertEquals("Page1.Line1.Description", "TRS #:123456;" + desc, wrapper.DocumentPages[0].Line1.Description);

			invoiceLine.JI_InvoiceQuantity = 123.45m;
			invoiceLine.JI_InvoiceUQ = "PCE";
			AssertEquals("Page1.Line1.Quantity", "1,231,231", wrapper.DocumentPages[0].Line1.Quantity);
			AssertEquals("Page1.Line1.UnitOfMeasere", CustomsUnitOfMeasureList.Codes.Litre, wrapper.DocumentPages[0].Line1.UnitOfMeasureCode);
			invoiceLine.JI_CustomsQuantity = 1231.12345;
			wrapper = new B3ImportDocumentWrapper(declaration.B3EntryHeader);
			AssertEquals("Page1.Line1.Quantity", "1,231.123", wrapper.DocumentPages[0].Line1.Quantity);
			AssertEquals("Page1.Line1.UnitOfMeasere", CustomsUnitOfMeasureList.Codes.Litre, wrapper.DocumentPages[0].Line1.UnitOfMeasureCode);
			invoiceLine.JI_CustomsQuantity = 1231.1;
			wrapper = new B3ImportDocumentWrapper(declaration.B3EntryHeader);
			AssertEquals("Page1.Line1.Quantity", "1,231.1", wrapper.DocumentPages[0].Line1.Quantity);
			AssertEquals("Page1.Line1.UnitOfMeasere", CustomsUnitOfMeasureList.Codes.Litre, wrapper.DocumentPages[0].Line1.UnitOfMeasureCode);
			invoiceLine.JI_CustomsQuantity = 0;
			wrapper = new B3ImportDocumentWrapper(declaration.B3EntryHeader);
			AssertEquals("Page1.Line1.Quantity", string.Empty, wrapper.DocumentPages[0].Line1.Quantity);
			AssertEquals("Page1.Line1.UnitOfMeasere", CustomsUnitOfMeasureList.Codes.Litre, wrapper.DocumentPages[0].Line1.UnitOfMeasureCode);
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			tariff.ZA_StatisticalUOMCode = ZString.Empty;
			var universalHelper = new UniversalReferenceTestDataHelper(invoiceLine.Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "8466939090", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			tariff1.UnitsOfMeasure.DeleteAll();
			wrapper = new B3ImportDocumentWrapper(declaration.B3EntryHeader);
			AssertEquals("Page1.Line1.Quantity", string.Empty, wrapper.DocumentPages[0].Line1.Quantity);
			AssertEquals("Page1.Line1.UnitOfMeasere", string.Empty, wrapper.DocumentPages[0].Line1.UnitOfMeasureCode);
		}

		#endregion

		#region TestB3FooterProperties

		[TestDate(2010, 8, 16)]
		public void TestB3FooterWarehouse10NumberProperty()
		{
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			var footer = wrapper.DocumentPages[3].B3Footer;
			AssertEquals("Page4.B3Footer.WarehouseNumber", "035", footer.WarehouseNumber);
		}

		[TestDate(2010, 8, 16)]
		public void TestB3FooterProperties()
		{
			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, Guid.Empty);
			CACustomsDataRegistry.Instance.ShouldPrintBrokerSignatureOnEntryDocs.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, false);
			var footer = wrapper.DocumentPages[3].B3Footer;
			AssertEquals("Page4.B3Footer.WarehouseNumber", ZString.Empty, footer.WarehouseNumber);
			AssertEquals("Page4.B3Footer.CarrierCodeAtImportation", "1234", footer.CarrierCodeAtImportation);
			AssertEquals("Page4.B3Footer.CargoControlNumber", "123456", footer.CargoControlNumber);
			AssertEquals("Page4.B3Footer.CurrentDate", new ZDateTime(2010, 8, 16), footer.CurrentDate);
			AssertNotNull("Page4.B3Footer.Totals", footer.Totals);

			AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Broker, 654", footer.BrokerNameAndPhone);
			broker.GS_PublishWorkPhone = false;
			AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Broker, 789", footer.BrokerNameAndPhone);
			broker.GS_GB_HomeBranch = ZGuid.Empty;
			AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Broker", footer.BrokerNameAndPhone);

			var currentUser = GlbStaff.CurrentUser;
			var originalProvider = Env.GetCurrentProvider();
			var originalUserFullName = currentUser.GS_FullName;
			var originalUserPublishPhoneNumber = currentUser.GS_PublishWorkPhone;
			var originalUserPhoneNumber = currentUser.GS_WorkPhone;
			var originalBranchPhoneNumber = GlbBranch.CurrentBranch.GB_Phone;
			var originalCompanyPhoneNumber = GlbCompany.CurrentCompany.GC_Phone;

			try
			{
				currentUser.GS_FullName = "Test User";
				currentUser.GS_WorkPhone = "123";
				currentUser.GS_PublishWorkPhone = true;
				GlbBranch.CurrentBranch.GB_Phone = "741";
				GlbCompany.CurrentCompany.GC_Phone = "258";

				declaration.JE_GS_NKCusAgent = ZString.Empty;
				AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Test User, 123", footer.BrokerNameAndPhone);
				currentUser.GS_WorkPhone = ZString.Empty;
				AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Test User, 741", footer.BrokerNameAndPhone);
				GlbBranch.CurrentBranch.GB_Phone = ZString.Empty;
				AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Test User, 258", footer.BrokerNameAndPhone);
				GlbCompany.CurrentCompany.GC_Phone = ZString.Empty;
				AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Test User", footer.BrokerNameAndPhone);
				using (var provider = new NullEnvProvider())
				{
					provider.Enable();
					AssertEquals("Page4.B3Footer.BrokerNameAndPhone", Constants.ProductName, footer.BrokerNameAndPhone);
					AssertNull("Broker Signature Image", footer.BrokerSignatureImage);
				}
			}
			finally
			{
				originalProvider.Enable();
				currentUser.GS_FullName = originalUserFullName;
				currentUser.GS_PublishWorkPhone = originalUserPublishPhoneNumber;
				currentUser.GS_WorkPhone = originalUserPhoneNumber;
				GlbBranch.CurrentBranch.GB_Phone = originalBranchPhoneNumber;
				GlbCompany.CurrentCompany.GC_Phone = originalCompanyPhoneNumber;
			}
			var declarant = Factory.New<GlbStaff>();
			declarant.GS_Code = "OOO";
			declarant.GS_WorkPhone = "Declarant PHONE";
			declarant.GS_PublishWorkPhone = true;
			declarant.GS_FullName = "Declarant NAME";
			declarant.SignatureImage = new Bitmap(1, 2);
			var declarantCompany = Factory.New<GlbCompany>();
			declarantCompany.GC_Name = "Declarant COMPANY";
			var declarantBranch = declarantCompany.Branches.AddNew();
			declarantBranch.GB_BranchName = "Declarant BRANCH";
			declarant.GS_GB_HomeBranch = declarantBranch.PK;
			declaration.JE_GS_NKCusAgent = declarant.GS_Code;
			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, declarant.PK.ToGuid());
			CACustomsDataRegistry.Instance.ShouldPrintBrokerSignatureOnEntryDocs.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, true);
			Factory.Save();
			AssertEquals("Page4.B3Footer.BrokerNameAndPhone", "Declarant NAME, Declarant PHONE", footer.BrokerNameAndPhone);
			AssertNotNull("Broker Signature Image", footer.BrokerSignatureImage);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "8466939090", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, "CU1", "LTR");
			factory.Save();
			declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";

			var helper = new DeclarationTestHelper(factory, true);
			declaration.WarehouseDocAddress.OrganisationPK = helper.CreateOrganisation("WAREHOUSE NAME", "CATOR").PK;
			declaration.WarehouseDocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "035", factory.Load<RefCountry>(Constants.CountryGuids.Canada));

			var brokerBranch = factory.New<GlbBranch>();
			brokerBranch.GB_Phone = "789";

			broker = factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			broker.GS_WorkPhone = "654";
			broker.GS_PublishWorkPhone = true;
			broker.GS_GB_HomeBranch = brokerBranch.PK;

			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			declaration.ReleaseStatuses.Load();
			var number1 = declaration.AdditionalReferenceNumbers.AddNew();
			number1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number1.CE_EntryNum = "123456";

			//Invoice 1
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.JZ_RN_NKDefaultOrigin = Constants.CountryCodes.Japan;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 1, 1, 1000, 1231231, 666, 150, Constants.Weight.Kilograms, 10);

			invoiceLine.CA_TRSNumber = "123456";
			invoiceLine.JI_Description = desc + "<EXTRA CHARACTERS>";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100001";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100002";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100003";

			//Invoice 2
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "2";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100000";

			//Invoice 3
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "3";
			invoice.JobComInvoiceLines.AddNew().JI_Tariff = "0201100000";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			wrapper = new B3ImportDocumentWrapper(declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC));
		}
		const string desc = "NUMBER DESCRIPTION234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789>";

		B3ImportDocumentWrapper wrapper;
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		GlbStaff broker;

		#endregion

		#region TestSourceIdentifierProvider

		public void TestSourceIdentifierProvider()
		{
			var provider = wrapper as ISourceIdentifierProvider;
			AssertNotNull(provider);
			AssertEquals(declaration.PK, provider.SourceIdentifier);
		}

		#endregion
	}
}
