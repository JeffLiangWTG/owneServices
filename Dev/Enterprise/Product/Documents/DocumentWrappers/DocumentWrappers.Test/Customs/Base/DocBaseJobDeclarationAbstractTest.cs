using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using JobInvoicingExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBaseJobDeclaration))]
	public abstract class DocBaseJobDeclarationAbstractTest<T, TWrapper> : DocumentWrapperTestCase
			where T : BaseJobDeclaration
			where TWrapper : DocBaseJobDeclaration
	{
		public void TestDocBaseJobDeclarationNewForCountrySpecialDocDeclaration()
		{
			var declaration = Factory.New<T>();
			var docDeclaration = DocBaseJobDeclaration.New(declaration, Factory);
			AssertType<TWrapper>(docDeclaration);
		}

		public void TestLogoIsBranchBasedFromDeclaration()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;

			GlbBranch anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, currentBranch.PK.ToGuid(), Guid.Empty, new Bitmap(1, 1));
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, anotherBranch.PK.ToGuid(), Guid.Empty, new Bitmap(2, 2));

			Declaration.JE_GB = currentBranch.PK;
			AssertEquals("Logo should be Current Default Branches Logo", new Size(1, 1), DeclarationWrapper.CompanyLogo.Size);

			Declaration.JE_GB = anotherBranch.PK;
			AssertEquals("Logo should be Another Branches Logo", new Size(2, 2), DeclarationWrapper.CompanyLogo.Size);

			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GB = currentBranch.PK;
			jobHeader.JH_ParentID = Declaration.PK;
			AssertEquals("Logo should NOW be Current Default Branches Logo again.", new Size(1, 1), DeclarationWrapper.CompanyLogo.Size);
		}

		#region General Field Tests
		public void TestNetMass()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.NetMassInKG, "10.5");
			AssertEquals("Net Mass (kg):", "10.5", DeclarationWrapper.NetMass);
		}

		public void TestNetMassInWords()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.NetMassInKG, "200");
			AssertEquals("Net Mass in words:", "two hundred", DeclarationWrapper.NetMassInWords);
		}

		public void TestPreCarriageFrom()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.PreCarriageFrom, "PreCarriage");
			AssertEquals("DeclarationWrapper.PreCarriageFrom", "PreCarriage", DeclarationWrapper.PreCarriageFrom);
		}

		public void TestPreCarriageBy()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.PreCarriageBy, "PreCarriageBy");
			AssertEquals("DeclarationWrapper.PreCarriageBy", "PreCarriageBy", DeclarationWrapper.PreCarriageBy);
		}

		public void TestDocumentaryCreditNumber()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.DocumentaryCreditNumber, "012457");
			AssertEquals(DeclarationWrapper.DocumentaryCreditNumber, "012457", DeclarationWrapper.DocumentaryCreditNumber);
		}

		public void TestPercentageContentFromOrigin()
		{
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals("DeclarationWrapper.PercentageContentFromOrigin", "0", DeclarationWrapper.PercentageContentFromOrigin);
			documentNote.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.PercentageContentFromOrigin, "65");
			AssertEquals("DeclarationWrapper.PercentageContentFromOrigin", "65", DeclarationWrapper.PercentageContentFromOrigin);
		}

		public void TestBrokerStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "BA4";
			staff.GS_FullName = "Blackadder";
			staff.GetNZWrapper().NZBPassword.GP_UserID = "98654321Z";

			Declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BrokerStaff has been assigned.", "Blackadder", DeclarationWrapper.BrokerStaff.FullName);
		}

		public void TestAdditionalPaymentTerms()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "GBMAN - Manchester";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.AdditionalPaymentTerms);
			documentNote.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.AdditionalPaymentTerms);
		}

		public void TestLetterOfCreditNumber()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.LetterOfCreditNumber, "TestLetterOfCreditNumber");
			AssertEquals("LetterOfCreditNumber", "TestLetterOfCreditNumber", DeclarationWrapper.LetterOfCreditNumber);
		}

		public void TestLetterOfCreditDate()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.LetterOfCreditDate, "TestLetterOfCreditDate");
			AssertEquals("LetterOfCreditDate", "TestLetterOfCreditDate", DeclarationWrapper.LetterOfCreditDate);
		}

		public void TestInsurancePolicyNumber()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.InsurancePolicyNumber, "TestInsurancePolicyNumber");
			AssertEquals("InsurancePolicyNumber", "TestInsurancePolicyNumber", DeclarationWrapper.InsurancePolicyNumber);
		}

		public void TestInsuredValue()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.InsuredValue, "300000.0");
			AssertEquals("Insured Value:", "300000.0", DeclarationWrapper.InsuredValue);
		}

		public void TestPackingListNo()
		{
			Declaration.JE_DeclarationReference = "PSP123456";
			AssertEquals("Packing List No.", "PSP123456", DeclarationWrapper.PackingListNo);
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.PackingListNo, "XBOX12345");
			AssertEquals("Packing List No.", "XBOX12345", DeclarationWrapper.PackingListNo);
		}

		[TestDate(2006, 12, 25)]
		public void TestPackDate()
		{
			AssertEquals("Pack Date", "25-Dec-06", DeclarationWrapper.PackDate);
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.PackDate, "29-DEC-06");
			AssertEquals("Pack Date", "29-DEC-06", DeclarationWrapper.PackDate);
		}

		public void TestContainerListNo()
		{
			Declaration.JE_DeclarationReference = "PSP123456";
			AssertEquals("Container List No.", "PSP123456", DeclarationWrapper.ContainerListNo);
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.ContainerListNo, "XBOX12345");
			AssertEquals("Container List No.", "XBOX12345", DeclarationWrapper.ContainerListNo);
		}

		public void TestAdditionalInformation()
		{
			AssertNullOrEmpty("AdditionalInformation", DeclarationWrapper.AdditionalInformation);
			Declaration.Invoices.AddNew().JZ_Remarks = "TestAdditionalInformation";
			AssertEquals("AdditionalInformation", "TestAdditionalInformation", DeclarationWrapper.AdditionalInformation);
		}

		public void TestExportersBankName()
		{
			AssertNullOrEmpty("ExportersBankName", DeclarationWrapper.ExportersBankName);
			Declaration.Invoices.AddNew().JZ_ExporterBankName = "TestExportersBankName";
			AssertEquals("ExportersBankName", "TestExportersBankName", DeclarationWrapper.ExportersBankName);
		}
		public void TestExportersBankAccountNo()
		{
			AssertNullOrEmpty("ExportersBankAccountNo", DeclarationWrapper.ExportersBankAccountNo);
			Declaration.Invoices.AddNew().JZ_ExporterBankAccountNumber = "TestExportersBankAccountNo";
			AssertEquals("ExportersBankAccountNo", "TestExportersBankAccountNo", DeclarationWrapper.ExportersBankAccountNo);
		}
		public void TestExportersBankSWIFTCode()
		{
			AssertNullOrEmpty("ExportersBankSWIFTCode", DeclarationWrapper.ExportersBankSWIFTCode);
			Declaration.Invoices.AddNew().JZ_ExporterBankSWIFTCode = "TestSWIFT";
			AssertEquals("ExportersBankSWIFTCode", "TestSWIFT", DeclarationWrapper.ExportersBankSWIFTCode);
		}
		#endregion

		public void TestPreAlertReference()
		{
			AssertEquals("ORDER NUMBERS / REFERENCE", DeclarationWrapper.PreAlertReferenceHeading);

			AssertEquals("", DeclarationWrapper.PreAlertReference);

			Declaration.AttachedOrders.AddNew().JD_OrderNumber = "order1";
			Declaration.AttachedOrders.AddNew().JD_OrderNumber = "order2";
			AssertEquals("order1,order2", DeclarationWrapper.PreAlertReference);

			Declaration.JE_OwnerRef = "OWNERS REFERENCE";

			AssertEquals("order1,order2 OWNERS REFERENCE", DeclarationWrapper.PreAlertReference);
		}

		public void TestCompleteRouting()
		{
			AssertEquals("should have a single leg", 1, DeclarationWrapper.CompleteRouting.Count);
			AssertEquals("the single leg should wrap the declaration", typeof(Freight.DeclarationSource), DeclarationWrapper.CompleteRouting[0].WrappedObject.GetType());

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.Transports.AddNew();
			shipment.Transports.AddNew();
			Declaration.JE_JS = shipment.PK;

			AssertEquals("legs from the shipment take precidence", 2, DeclarationWrapper.CompleteRouting.Count);
			AssertEquals("the first leg should wrap a transport", typeof(Freight.TransportSource), DeclarationWrapper.CompleteRouting[0].WrappedObject.GetType());
			AssertEquals("the second leg should wrap a transport", typeof(Freight.TransportSource), DeclarationWrapper.CompleteRouting[1].WrappedObject.GetType());
		}

		public virtual void TestIsExWarehouse()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, DeclarationWrapper.IsExWarehouse);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, DeclarationWrapper.IsExWarehouse);
		}

		public void TestToString()
		{
			AssertEquals("ToString()", Declaration.DeclarationNumber, DeclarationWrapper.ToString());
		}

		public void TestBills()
		{
			T declaration = Declaration;
			declaration.JE_HouseBill = "1";
			Bill hbl2 = declaration.Bills.AddNew();
			hbl2.CU_HouseBill = "2";
			Bill hbl3 = declaration.Bills.AddNew();
			hbl3.CU_HouseBill = "3";
			DocBaseJobDeclaration wrapper = CreateDeclarationWrapper(declaration);
			AssertEquals("1,2,3", wrapper.Bills);
		}

		public void TestMessageSubTypeDescription()
		{
			var mockDeclaration = Factory.NewMoq<T>();
			mockDeclaration.Setup(m => m.MessageSubTypeDescription).Returns(new ZString("new subtype description"));
			var decWrapper = DocBaseJobDeclaration.New(mockDeclaration.Object, Factory);
			AssertEquals("new subtype description", decWrapper.MessageSubTypeDescription);
		}

		#region Abstract

		protected virtual TWrapper CreateDeclarationWrapper(T declaration)
		{
			TWrapper result = (TWrapper)DocBaseJobDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>()); // Will call through to OnDocWrappersContextSet().
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		#endregion

		#region ZString Array Fields

		public void TestDetailedGoodsDescriptionArray()
		{
			AssertEquals(0, DeclarationWrapper.DetailedGoodsDescriptionArray.Length);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "WEWEWEW\r\nWOWOWOW\r\nWAWAWAW", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.LoadListInstructions.Description, "Other notes that should not be included.", Factory);
			AssertEquals("WEWEWEW", DeclarationWrapper.DetailedGoodsDescriptionArray[0]);
			AssertEquals("WOWOWOW", DeclarationWrapper.DetailedGoodsDescriptionArray[1]);
			AssertEquals("WAWAWAW", DeclarationWrapper.DetailedGoodsDescriptionArray[2]);
		}

		public void TestMarksAndNumbersArray()
		{
			string longMarksAndNumbers =
					"Long marks and numbers.\n" +
					"Second line of marks and numbers. This sould still be the second line.\n" +
					"Third line is a very long line. This should be more than 120 characters, which is really long to test if the split is working. Some more characters to go. Third line should start here. More characters here. Ok this is enough.";
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, longMarksAndNumbers, Factory);

			AssertEquals(4, DeclarationWrapper.MarksAndNumberArray.Length);
			AssertEquals("Long marks and numbers.", DeclarationWrapper.MarksAndNumberArray[0].Substring(0, 23));
			AssertEquals("Second line", DeclarationWrapper.MarksAndNumberArray[1].Substring(0, 11));
			AssertEquals("Third line is a", DeclarationWrapper.MarksAndNumberArray[2].Substring(0, 15));
			AssertEquals("working. Some more", DeclarationWrapper.MarksAndNumberArray[3].Substring(0, 18));
		}

		public void TestAllNotes()
		{
			AssertEquals(0, DeclarationWrapper.DetailedGoodsDescriptionArray.Length);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "WEWEWEW\r\nWOWOWOW\r\nWAWAWAW", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.LoadListInstructions.Description, "Other notes that should not be included.", Factory);

			string longMarksAndNumbers =
					@"Long marks and numbers.
				Second line of marks and numbers. This sould still be the second line.
				Third line is a very long line. This should be more than 120 characters, which is really long to test if the split is working. Some more characters to go. Third line should start here. More characters here. Ok this is enough.";
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, longMarksAndNumbers, Factory);

			AssertEquals("All notes length", 14, DeclarationWrapper.AllNotes.Length);
		}

		public void TestCertificateOfOriginNoteArray()
		{
			string text = "Test certificate of origin notes.\nTest 123";
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, text, Factory);

			AssertEquals(2, DeclarationWrapper.CertificateOfOriginNoteArray.Length);
			Assert(DeclarationWrapper.CertificateOfOriginNoteArray[0].StartsWith("Test certificate"));
			Assert(DeclarationWrapper.CertificateOfOriginNoteArray[1].StartsWith("Test 123"));
		}
		#endregion

		#region ZString Fields

		[TestDate(2005, 5, 20)]
		public void TestTodaysDateFormat()
		{
			AssertEquals("2005 05 20", DeclarationWrapper.TodaysDateYYYYMMDD);
		}

		public void TestEmailSubjectNumber()
		{
			Declaration.JE_DeclarationReference = "B0000001";
			AssertEquals("Email Subject Number", "B0000001", DeclarationWrapper.EmailSubjectNumber);
		}
		public void TestMessageTypeDescription()
		{
			Declaration.JE_MessageType = Declaration.Lookups.MessageTypeList[0].Code;
			AssertNotNullOrEmpty(Declaration.MessageTypeDescription);
			AssertEquals(Declaration.MessageTypeDescription, DeclarationWrapper.MessageTypeDescription);
		}

		public void TestExporterContact()
		{
			OrgHeader supplierBizO = CreateTestOrgHeader(Factory, "SUXXX", "Supplier Org", "000 Street", "ZYZ Lane", "Sydney");
			Declaration.JE_OH_Supplier = supplierBizO.PK;
			OrgContact cNRContact = supplierBizO.Contacts.AddNew();
			cNRContact.OC_ContactName = "Consignor Contact";
			cNRContact.Documents.AddNew();
			cNRContact.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact otherContact = supplierBizO.Contacts.AddNew();
			otherContact.OC_ContactName = "This is not the right contact";
			otherContact.Documents.AddNew();
			otherContact.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			AssertEquals("Exporter contact should be 'Consignor Contact'", "Consignor Contact", DeclarationWrapper.ExporterContact);
		}

		public void TestImporterContact()
		{
			OrgHeader importerBizO = CreateTestOrgHeader(Factory, "IMPXXX", "Importer Org", "123 Street", "ABC Lane", "Sydney");
			Declaration.JE_OH_Importer = importerBizO.PK;
			OrgContact cNEContact = importerBizO.Contacts.AddNew();
			cNEContact.OC_ContactName = "Consignee Contact";
			cNEContact.Documents.AddNew();
			cNEContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			OrgContact otherContact = importerBizO.Contacts.AddNew();
			otherContact.OC_ContactName = "This is not the right contact";
			otherContact.Documents.AddNew();
			otherContact.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			AssertEquals("Importer contact should be 'Consignee Contact'", "Consignee Contact", DeclarationWrapper.ImporterContact);
		}

		public void TestExporterDefaultContact()
		{
			OrgHeader supplierBizO = CreateTestOrgHeader(Factory, "SUXXX", "Supplier Org", "000 Street", "ZYZ Lane", "Sydney");
			Declaration.JE_OH_Supplier = supplierBizO.PK;
			OrgContact cNRContact = supplierBizO.Contacts.AddNew();
			cNRContact.OC_ContactName = "Consignor Contact";
			cNRContact.Documents.AddNew();
			cNRContact.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact otherContact = supplierBizO.Contacts.AddNew();
			otherContact.OC_ContactName = "This is not the right contact";
			otherContact.Documents.AddNew();
			otherContact.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			AssertEquals("Exporter contact should be 'Consignor Contact'", "Consignor Contact", DeclarationWrapper.ExporterDefaultContact.Code);
		}

		public void TestImporterDefaultContact()
		{
			OrgHeader importerBizO = CreateTestOrgHeader(Factory, "IMPXXX", "Importer Org", "123 Street", "ABC Lane", "Sydney");
			Declaration.JE_OH_Importer = importerBizO.PK;
			OrgContact cNEContact = importerBizO.Contacts.AddNew();
			cNEContact.OC_ContactName = "Consignee Contact";
			cNEContact.Documents.AddNew();
			cNEContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			OrgContact otherContact = importerBizO.Contacts.AddNew();
			otherContact.OC_ContactName = "This is not the right contact";
			otherContact.Documents.AddNew();
			otherContact.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			AssertEquals("Importer contact should be 'Consignee Contact'", "Consignee Contact", DeclarationWrapper.ImporterDefaultContact.Code);
		}

		public void TestPackTypeDescription()
		{
			Declaration.JE_TotalNoOfPacksPackType = "";
			AssertEquals("", DeclarationWrapper.PackTypeDescription);
		}

		public void TestLineOfContainerNumbers()
		{
			AssertEquals("Container numbers", ZString.Empty, DeclarationWrapper.LineOfContainerNumbers);

			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "Container1";
			BaseCusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "Container2";
			BaseCusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "Container3";

			AssertEquals("Container numbers", "CONTAINER1, CONTAINER2, CONTAINER3", DeclarationWrapper.LineOfContainerNumbers);
		}

		public void TestLineOfContainerNumbersWithContainerMode()
		{
			AssertEquals("Container numbers with Container Mode", ZString.Empty, DeclarationWrapper.LineOfContainerNumbersWithContainerMode);
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "Container1";
			container1.CO_FCL_LCL_AIR = "FCL";

			BaseCusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "Container2";
			container2.CO_FCL_LCL_AIR = "LCL";

			BaseCusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "Container3";
			container3.CO_FCL_LCL_AIR = ZString.Empty;

			AssertEquals("Container numbers with Container Mode", "F/CONTAINER1, L/CONTAINER2, CONTAINER3", DeclarationWrapper.LineOfContainerNumbersWithContainerMode);
		}

		public void TestContainerNumberAndTypeLine()
		{
			AssertEquals("Container Number And Type Line", ZString.Empty, DeclarationWrapper.ContainerNumberAndTypeLine);
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "Container1";
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			container1.CO_RC = ref1.PK;
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "Container2";
			BaseCusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "Container3";
			RefContainer ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = "40FR";
			container3.CO_RC = ref2.PK;
			BaseCusContainer container4 = Declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "Container4";
			RefContainer ref4 = Factory.New<RefContainer>();
			ref4.RC_Code = "30FR";
			container4.CO_RC = ref4.PK;
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "Container5";
			AssertEquals("Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5", DeclarationWrapper.ContainerNumberAndTypeLine);
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "Container6";
			AssertEquals("Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5 ...", DeclarationWrapper.ContainerNumberAndTypeLine);

			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Container Number And Type Line", "See attached Container Details list.", DeclarationWrapper.ContainerNumberAndTypeLine);
		}

		public void TestContainerNumberOnNewLine()
		{
			AssertEquals("Container Number On New Line", ZString.Empty, DeclarationWrapper.ContainerNumberOnNewLine);
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR1";
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR2";
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR3";
			ZString expectedLine = "CONTNMBR1" + System.Environment.NewLine + "CONTNMBR2" + System.Environment.NewLine + "CONTNMBR3" + System.Environment.NewLine;
			AssertEquals("Container Number On New Line and etalon must be equal", expectedLine, DeclarationWrapper.ContainerNumberOnNewLine);
		}

		public void TestContainerTypeOnNewLine()
		{
			AssertEquals("Container Number And Type Line", ZString.Empty, DeclarationWrapper.ContainerTypeOnNewLine);
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "Container1";
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			container1.CO_RC = ref1.PK;
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "Container2";
			BaseCusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "Container3";
			RefContainer ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = "40FR";
			container3.CO_RC = ref2.PK;
			ZString expectedLine = "20FR" + System.Environment.NewLine + System.Environment.NewLine + "40FR" + System.Environment.NewLine;
			AssertEquals("ContainerTypeOnNewLine and etalon must be equal", expectedLine, DeclarationWrapper.ContainerTypeOnNewLine);
		}

		public void TestPrintPageWithContainerNumber()
		{
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR1";
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR2";
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR3";
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR4";
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR5";
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)DeclarationWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)DeclarationWrapper.PrintPageWithContainerNumber);
			Declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTNMBR6";
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)DeclarationWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be true", true, (bool)DeclarationWrapper.PrintPageWithContainerNumber);
		}

		public void TestDisbursementNoteTitle()
		{
			AssertEquals(Env.Registry.DisbursementNoteTitle.Trim().ToUpper(), DeclarationWrapper.DisbursementNoteTitle);
		}

		public void TestVoyageFlightDetails()
		{
			ZString nKVessel = ZArchitecture.Core.Utilities.GetFieldFromRandomRowInTable(RefVesselSchema.Constants.RV_Code, RefVesselSchema.Constants.TableName).ToString();
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VesselName = nKVessel;
			Declaration.JE_VoyageFlightNo = "VOY 222";

			AssertEquals("Voyage details should be vessel and voyage number", nKVessel + " / VOY 222", DeclarationWrapper.VoyageFlightDetails);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.JE_VoyageFlightNo = "FLY 888";
			AssertEquals("Voyage details should be the flight number", "FLY 888", DeclarationWrapper.VoyageFlightDetails);
		}

		public void TestPackages()
		{
			Declaration.JE_TotalNoOfPacks = 34;
			Declaration.JE_TotalNoOfPacksPackType = "PLT";

			string innerPack = "12" + " (INNER)";
			string outerPack = "34 " + Declaration.JE_TotalNoOfPacksPackType + " (OUTER)";

			Declaration.JE_TotalNoOfPieces = 0;

			AssertEquals("Package details shows outer packs only", outerPack, DeclarationWrapper.Packages);

			Declaration.JE_TotalNoOfPacks = 0;
			Declaration.JE_TotalNoOfPieces = 12;

			AssertEquals("Package details shows inner packs only", innerPack, DeclarationWrapper.Packages);

			Declaration.JE_TotalNoOfPacks = 34;
			Declaration.JE_TotalNoOfPacksPackType = "PLT";
			Declaration.JE_TotalNoOfPieces = 12;

			AssertEquals("Package details shows outer and inner packs", outerPack + ", " + innerPack, DeclarationWrapper.Packages);
		}

		public void TestOwnerRefAndOrderRef()
		{
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			Declaration.JE_OwnerRef = "Owner's Ref 123";
			AssertEquals("Test with order items as string", "Owner's Ref 123 Orders", DeclarationWrapper.OwnerRefAndOrderRef);

			Enterprise.Freight.Forwarding.Orders.Business.Order order = Declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "456";
			AssertEquals("Test with order items as objects", "Owner's Ref 123 456", DeclarationWrapper.OwnerRefAndOrderRef);

			Declaration.JE_OwnerRef = "123";
			order.JD_OrderNumber = "456 123";
			AssertEquals("Test with order items as objects", "456 123", DeclarationWrapper.OwnerRefAndOrderRef);
		}

		public void TestOrderReference()
		{
			Declaration.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			AssertEquals("Order Reference", "Orders", DeclarationWrapper.OrderReference);
		}

		public void TestFirstOrderNumber()
		{
			Enterprise.Freight.Forwarding.Orders.Business.Order order1 = Declaration.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "456";

			Enterprise.Freight.Forwarding.Orders.Business.Order order2 = Declaration.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "789";

			AssertEquals("First Order Number", order1.JD_OrderNumber, DeclarationWrapper.FirstOrderNumber);
		}

		public void TestOrders()
		{
			AssertEquals("Initial declaration should have zero orders", 0, DeclarationWrapper.Orders.Count);

			Declaration.AttachedOrders.AddNew();
			Declaration.AttachedOrders.AddNew();
			Declaration.AttachedOrders.AddNew();
			AssertEquals("Initial declaration should have 3 orders", 3, DeclarationWrapper.Orders.Count);
		}

		public void TestCargoStatus()
		{
			Declaration.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			Declaration.JE_OwnerRef = "Owner's Ref 123";
			AssertEquals("Test with order items as string", "Orders", DeclarationWrapper.CargoStatus);

			Enterprise.Freight.Forwarding.Orders.Business.Order order1 = Declaration.AttachedOrders.AddNew();
			Enterprise.Freight.Forwarding.Orders.Business.Order order2 = Declaration.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "456";
			order2.JD_OrderNumber = "789";
			AssertEquals("Test with order items as objects", "456,789", DeclarationWrapper.CargoStatus);
		}

		public void TestDeclarationNumber()
		{
			AssertEquals("DeclarationNumber", Declaration.DeclarationNumber, DeclarationWrapper.DeclarationNumber);
		}

		public void TestCustomsEntryNumber()
		{
			AssertEquals("CustomsEntryNumber", Declaration.DeclarationNumber, DeclarationWrapper.CustomsEntryNumber);
		}

		public void TestEntryStatusDescription()
		{
			AssertEquals("EntryStatusDescription", Declaration.JE_EntryStatusDescription, DeclarationWrapper.EntryStatusDescription);
		}

		public void TestFCLDeliveryOrPickupEquipmentNeeded()
		{
			Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "FCL";
			AssertEquals("FCLDeliveryOrPickupEquipmentNeeded", Declaration.JE_FCLDeliveryOrPickupEquipmentNeeded, DeclarationWrapper.FCLDeliveryOrPickupEquipmentNeeded);
		}

		public void TestAddInfo()
		{
			Declaration.JE_AddInfo = "AddInfo";
			AssertEquals("AddInfo", Declaration.JE_AddInfo, DeclarationWrapper.AddInfo);
		}

		public void TestAgentsReference()
		{
			Declaration.JE_AgentsReference = "AgentsReference";
			AssertEquals("AgentsReference", Declaration.JE_AgentsReference, DeclarationWrapper.AgentsReference);
		}

		public void TestContainerMode()
		{
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("ContainerMode", Core.Constants.ContainerModes.FCL, DeclarationWrapper.ContainerMode);
		}

		public void TestDeclarationReference()
		{
			Declaration.JE_DeclarationReference = "DeclarationReference";
			AssertEquals("DeclarationReference", Declaration.JE_DeclarationReference, DeclarationWrapper.DeclarationReference);
		}

		public void TestCartageAdviceDeclarationReference()
		{
			Declaration.JE_DeclarationReference = "DeclarationReference";
			AssertEquals("DeclarationReference", "DeclarationReference", DeclarationWrapper.CartageAdviceDeclarationReference);
		}

		public void TestEFTMode()
		{
			Declaration.JE_EFTMode = "EFT";
			AssertEquals("EFTMode", Declaration.JE_EFTMode, DeclarationWrapper.EFTMode);
		}

		public void TestEntryStatus()
		{
			Declaration.JE_EntryStatus = "EEE";
			AssertEquals("EntryStatus", Declaration.JE_EntryStatus, DeclarationWrapper.EntryStatus);
		}

		public void TestExportGoodsType()
		{
			Declaration.JE_ExportGoodsType = "EEE";
			AssertEquals("ExportGoodsType", Declaration.JE_ExportGoodsType, DeclarationWrapper.ExportGoodsType);
		}

		public void TestFolio()
		{
			Declaration.JE_Folio = "Folio";
			AssertEquals("Folio", Declaration.JE_Folio, DeclarationWrapper.Folio);
		}

		public void TestGoodsDescription()
		{
			Declaration.JE_GoodsDescription = "GoodsDescription";
			AssertEquals("GoodsDescription", Declaration.JE_GoodsDescription, DeclarationWrapper.GoodsDescription);
		}

		public void TestLloydsIMO()
		{
			Declaration.JE_LloydsIMO = "Lloyds";
			AssertEquals("LloydsIMO", Declaration.JE_LloydsIMO, DeclarationWrapper.LloydsIMO);
		}

		public void TestMasterBill()
		{
			Declaration.JE_MasterBill = "MasterBill";
			AssertEquals("MasterBill", Declaration.JE_MasterBill, DeclarationWrapper.MasterBill);
		}

		public void TestMergeBy()
		{
			Declaration.JE_MergeBy = "MMM";
			AssertEquals("MergeBy", Declaration.JE_MergeBy, DeclarationWrapper.MergeBy);
		}

		public void TestMessageSubType()
		{
			Declaration.JE_MessageSubType = "Sub";
			AssertEquals("MessageSubType", Declaration.JE_MessageSubType, DeclarationWrapper.MessageSubType);
		}

		public void TestMessageType()
		{
			Declaration.JE_MessageType = "Mes";
			AssertEquals("MessageType", Declaration.JE_MessageType, DeclarationWrapper.MessageType);
		}

		public void TestHouseBill()
		{
			Declaration.JE_HouseBill = "HouseBill";
			AssertEquals("Bills", Declaration.JE_HouseBill, DeclarationWrapper.HouseBill);
		}

		public void TestOperationalStatus()
		{
			Declaration.JE_OperationalStatus = "OOO";
			AssertEquals("OperationalStatus", Declaration.JE_OperationalStatus, DeclarationWrapper.OperationalStatus);
		}

		public void TestOwnerRef()
		{
			Declaration.JE_OwnerRef = "OwnerRef";
			AssertEquals("OwnerRef", Declaration.JE_OwnerRef, DeclarationWrapper.OwnerRef);
		}

		public virtual void TestPaymentMethod()
		{
			Declaration.JE_PaymentMethod = "CSH";
			AssertEquals("PaymentMethod", Declaration.JE_PaymentMethod, DeclarationWrapper.PaymentMethod);
		}

		public void TestShipmentIncoTerm()
		{
			Declaration.JE_ShipmentIncoTerm = "III";
			AssertEquals("ShipmentIncoTerm", Declaration.JE_ShipmentIncoTerm, DeclarationWrapper.ShipmentIncoTerm);
		}

		public void TestPackType()
		{
			Declaration.JE_TotalNoOfPacksPackType = "TTT";
			AssertEquals("TotalNoOfPacksPackType", Declaration.JE_TotalNoOfPacksPackType, DeclarationWrapper.PackType);
		}

		public void TestWeightUQ()
		{
			Declaration.JE_TotalWeightUnit = "WG";
			AssertEquals("TotalWeightUnit", Declaration.JE_TotalWeightUnit, DeclarationWrapper.WeightUQ);
		}

		public void TestTransportMode()
		{
			Declaration.JE_TransportMode = "TTT";
			AssertEquals("TransportMode", "TTT", DeclarationWrapper.TransportMode);
		}

		public void TestVoyageFlightNo()
		{
			Declaration.JE_VoyageFlightNo = "Voyage";
			AssertEquals("VoyageFlightNo", Declaration.JE_VoyageFlightNo, DeclarationWrapper.VoyageFlightNo);
		}

		public void TestVolumeUQ()
		{
			Declaration.JE_TotalVolumeUnit = "Vl";
			AssertEquals("TotalVolumeUnit", Declaration.JE_TotalVolumeUnit, DeclarationWrapper.VolumeUQ);
		}

		public virtual void TestSupplierInvoiceNumbers()
		{
			AssertEquals("SupplierInvoiceNumbers", ZString.Empty, DeclarationWrapper.SupplierInvoiceNumbers);

			BaseJobComInvoiceGroupHeader groupHeader1 = Declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "Inv One";

			AssertEquals("SupplierInvoiceNumbers", "INV ONE", DeclarationWrapper.SupplierInvoiceNumbers);

			BaseJobComInvoiceHeader invoiceHeader2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "Inv Two";

			AssertEquals("SupplierInvoiceNumbers", "INV ONE, INV TWO", DeclarationWrapper.SupplierInvoiceNumbers);

			BaseJobComInvoiceGroupHeader groupHeader2 = Declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoiceHeader3.JZ_InvoiceNumber = "Inv Three";

			AssertEquals("SupplierInvoiceNumbers", "INV ONE, INV TWO, INV THREE", DeclarationWrapper.SupplierInvoiceNumbers);
		}

		public void TestForwardingAgentHeader()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertEquals("ForwardingAgentHeader", "FORWARDER", DeclarationWrapper.ForwardingAgentHeader);

			Declaration.JE_OH_ShippingLine = Factory.New(typeof(OrgHeader)).PK;
			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("ForwardingAgentHeader", "AIRLINE", DeclarationWrapper.ForwardingAgentHeader);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("ForwardingAgentHeader", "SHIPPING LINE", DeclarationWrapper.ForwardingAgentHeader);

			Declaration.JE_OH_Forwarder = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("ForwardingAgentHeader", "FORWARDER", DeclarationWrapper.ForwardingAgentHeader);
		}

		public void TestStatementFooterAddress()
		{
			Assert("StatementFooterAddress is not empty", DeclarationWrapper.StatementFooterAddress != ZString.Empty);
		}

		public void TestTransport()
		{
			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("Transport", ZString.Empty, DeclarationWrapper.Transport);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VoyageFlightNo = "Voyage";
			AssertEquals("Transport", "Voyage", DeclarationWrapper.Transport);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.JE_VesselName = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / Voyage / " + vessel.RV_LloydsNumber, DeclarationWrapper.Transport);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.JE_VoyageFlightNo = "Voyage";
			AssertEquals("Transport", "Voyage", DeclarationWrapper.Transport);

			Declaration.JE_ExportDate = new ZDateTime(2004, 04, 03);
			AssertEquals("Transport", "Voyage 03-Apr-04", DeclarationWrapper.Transport);
		}

		public void TestVoyageFlightHeading()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("VESSEL / VOYAGE NO.", DeclarationWrapper.VoyageFlightHeading);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("FLIGHT NO.", DeclarationWrapper.VoyageFlightHeading);
		}

		public void TestExportImportText()
		{
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Message type 'Export'", "Export", DeclarationWrapper.ImportExportText);

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Message type 'Import'", "Import", DeclarationWrapper.ImportExportText);

			Declaration.JE_MessageType = "DRW";
			AssertEquals("Message type ''", "", DeclarationWrapper.ImportExportText);
		}

		public void TestTransportFields()
		{
			ZString nKVessel = ZArchitecture.Core.Utilities.GetFieldFromRandomRowInTable(RefVesselSchema.Constants.RV_Code, RefVesselSchema.Constants.TableName).ToString();
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VesselName = nKVessel;
			Declaration.JE_VoyageFlightNo = "VOY 222";

			AssertEquals(string.Format("Transport mode should be '{0}'", Declaration.TransportModeSeaCodeForTesting), Declaration.TransportModeSeaCodeForTesting, DeclarationWrapper.TransportMode);
			AssertEquals("Voyage Flight No should match", "VOY 222", DeclarationWrapper.VoyageFlightNo);
			AssertEquals("Transport field name is 'VESSEL / VOYAGE NO. / IMO(Lloyds)'", "VESSEL / VOYAGE NO. / IMO(Lloyds)", DeclarationWrapper.TransportHeading);

			RefVessel vessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, Declaration.JE_VesselName);
			ZString lloyds = vessel.RV_LloydsNumber;

			AssertEquals("Transport is vessel name and voyage no and lloyds", vessel.RV_Code + " / " + "VOY 222" + " / " + lloyds, DeclarationWrapper.Transport);
		}

		public void TestMasterBillHeading()
		{
			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("MasterBillHeading", "MASTER BILL", DeclarationWrapper.MasterBillHeading);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("MasterBillHeading", "MAWB", DeclarationWrapper.MasterBillHeading);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("MasterBillHeading", "OCEAN BILL OF LADING", DeclarationWrapper.MasterBillHeading);
		}

		public void TestMasterBillAndIssueHeading()
		{
			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("MasterBillHeading", "MASTER BILL", DeclarationWrapper.MasterBillAndIssueHeading);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			ZDateTime today = ZDateTime.Today;
			consol.JK_MasterBillIssueDate = today;
			Declaration.JE_JS = shipment.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("MasterBillHeading - Only show issue date for air", "MASTER BILL", DeclarationWrapper.MasterBillAndIssueHeading);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("MasterBillHeading", "MASTER BILL / ISSUE", DeclarationWrapper.MasterBillAndIssueHeading);
		}

		public void TestEquipmentType()
		{
			AssertEquals("EquipmentType", ZString.Empty, DeclarationWrapper.EquipmentType);

			Declaration.JE_MessageType = MessageTypeCodeForExports;
			Declaration.JE_TransportMode = "AIR";
			Declaration.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Declaration.DocsAndCartage.Lookups.PickupEquipmentNeededList[0].Code;
			ZString expected = Declaration.DocsAndCartage.Lookups.PickupEquipmentNeededList[0].Code + " - " + Declaration.DocsAndCartage.Lookups.PickupEquipmentNeededList[0].Description;
			AssertEquals("EquipmentType", expected, DeclarationWrapper.EquipmentType);

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.JE_TransportMode = "AIR";
			Declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Declaration.DocsAndCartage.Lookups.DeliveryEquipmentNeededList[0].Code;
			expected = Declaration.DocsAndCartage.Lookups.DeliveryEquipmentNeededList[0].Code + " - " + Declaration.DocsAndCartage.Lookups.DeliveryEquipmentNeededList[0].Description;
			AssertEquals("EquipmentType", expected, DeclarationWrapper.EquipmentType);
		}

		public void TestHouseBillHeading()
		{
			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("HouseBillHeading", "HOUSE BILL", DeclarationWrapper.HouseBillHeading);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("HouseBillHeading", "HAWB", DeclarationWrapper.HouseBillHeading);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("HouseBillHeading", "HOUSE BILL OF LADING", DeclarationWrapper.HouseBillHeading);

			Declaration.JE_TransportMode = Declaration.TransportModeMailCodeForTesting;
			AssertEquals("HouseBillHeading", "PARCEL POST NUMBERS", DeclarationWrapper.HouseBillHeading);
		}

		public void TestHouseBillAndIssueHeading()
		{
			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("HouseBillHeading", "HOUSE BILL", DeclarationWrapper.HouseBillAndIssueHeading);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ZDateTime today = ZDateTime.Today;
			shipment.JS_HouseBillIssueDate = today;
			Declaration.JE_JS = shipment.PK;

			AssertEquals("HouseBillHeading", "HOUSE BILL / ISSUE", DeclarationWrapper.HouseBillAndIssueHeading);
		}

		public virtual void TestTotalInvoiceLineCustomAttrib2()
		{
			var header = Declaration.Invoices.AddNew();
			var line1 = header.InvoiceLines.AddNew();
			var line2 = header.InvoiceLines.AddNew();
			var line3 = header.InvoiceLines.AddNew();

			line1.JI_CustomAttrib2 = "0";
			line2.JI_CustomAttrib2 = "3.5000";
			line3.JI_CustomAttrib2 = "100";
			AssertEquals("Total Custom Attrib2", "103.5", DeclarationWrapper.TotalInvoiceLineCustomAttrib2);

			line1.JI_CustomAttrib2 = "0";
			line2.JI_CustomAttrib2 = "3.5";
			line3.JI_CustomAttrib2 = "123 blah";
			AssertEquals("Total Custom Attrib2", "3.5", DeclarationWrapper.TotalInvoiceLineCustomAttrib2);
		}

		public void TestContainerWeightHeading()
		{
			AssertEquals("Empty Heading", "", DeclarationWrapper.ContainerWeightHeading);

			BaseCusContainer container1 = CreateContainer(Declaration, "FCL");
			BaseCusContainer container2 = CreateContainer(Declaration, "FCL");
			AssertEquals("Empty Heading", "", DeclarationWrapper.ContainerWeightHeading);

			container1.CO_Weight = 1M;
			AssertEquals("Weight Heading", "Weight", DeclarationWrapper.ContainerWeightHeading);
		}
		public void TestCertificateOfOriginClause()
		{
			DocumentsDataRegistry.Instance.CertificateOfOriginStandardClause.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123 NewValue");
			AssertEquals("Certificate of origin clause from registry", "123 NewValue", DeclarationWrapper.CertificateOfOriginClause);
		}

		public void TestRequestForMissingDocumentsInstruction()
		{
			AssertEquals(Env.Registry.ShipmentRequestForMissingDocumentsClause, DeclarationWrapper.RequestForMissingDocumentsInstruction);
		}

		public void TestShipmentOrBrokerageNumber()
		{
			Declaration.JE_DeclarationReference = "POND";
			AssertEquals(Declaration.JE_DeclarationReference, DeclarationWrapper.ShipmentOrBrokerageNumber);
		}

		public void TestDeclarationOrConsolNumber()
		{
			SetupDeclarationForDeclarationNumberTests("LOCH");
			AssertEquals("LOCH", DeclarationWrapper.DeclarationOrConsolNumber);
		}

		protected virtual void SetupDeclarationForDeclarationNumberTests(ZString declarationNumber)
		{
			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = declarationNumber;
		}

		public void TestImporterABNOrCID()
		{
			AssertEquals("", DeclarationWrapper.ImporterABNOrCID);

			OrgHeader importer = OrgHeader.New(Factory);
			CreateCustomCode(importer, OrgCusCode.CodeTypes.CustomsClientID, "ABC", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Get Importer CID", "ABC", DeclarationWrapper.ImporterABNOrCID);

			CreateCustomCode(importer, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", "AU");
			AssertEquals("Get Importer ABN", "ABN", DeclarationWrapper.ImporterABNOrCID);
		}

		public void TestImportersName()
		{
			AssertEquals("", DeclarationWrapper.ImportersName);
			OrgHeader importer = CreateTestOrgHeader("IMXXX", "Importer Full Name", "000 Street", "ZYZ Lane", "Sydney");
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("ImportersName", "Importer Full Name", DeclarationWrapper.ImportersName);
		}

		public void TestBrokerName()
		{
			ZString oldFullName = GlbBranch.CurrentBranch.OrgProxy.OH_FullName;
			try
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "Brokers Inc";
				AssertEquals("Brokers Inc", DeclarationWrapper.BrokerName);
				Declaration.JE_GB = Guid.Empty;
				AssertEquals("Brokers Inc", DeclarationWrapper.BrokerName);
			}
			finally
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = oldFullName;
			}
		}

		public void TestBroker()
		{
			ZString oldFullName = GlbBranch.CurrentBranch.OrgProxy.OH_FullName;
			try
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "Brokers Inc";
				OrgAddress brokerAddress = GlbBranch.CurrentBranch.OrgProxy.Addresses.MainAddress;
				brokerAddress.OA_Phone_Formatted = "02 8001 8001";
				brokerAddress.OA_Fax_Formatted = "02 8001 8002";
				AssertEquals("Brokers Inc", DeclarationWrapper.Broker.Name);
				AssertEquals("+61 2 8001 8001", DeclarationWrapper.Broker.Phone);
				AssertEquals("+61 2 8001 8002", DeclarationWrapper.Broker.Fax);
				Declaration.JE_GB = Guid.Empty;
				AssertEquals("Brokers Inc", DeclarationWrapper.Broker.Name);
				AssertEquals("+61 2 8001 8001", DeclarationWrapper.Broker.Phone);
				AssertEquals("+61 2 8001 8002", DeclarationWrapper.Broker.Fax);
			}
			finally
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = oldFullName;
			}
		}

		public void TestBrokerABNOrCID()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			var companyProxy = Factory.NewWithValidTestData<OrgHeader>();

			companyProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "ABC");
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyProxy.PK;
			Factory.Save();
			AssertEquals("Get Company Proxy CID", "ABC", DeclarationWrapper.BrokerABNOrCID);

			CreateCustomCode(companyProxy, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", Core.Constants.CountryCodes.Australia);
			Factory.Save();
			AssertEquals("Get Company Proxy ABN", "ABN", DeclarationWrapper.BrokerABNOrCID);

			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "OOO");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;
			Factory.Save();
			AssertEquals("Get Branch Proxy CID", "OOO", DeclarationWrapper.BrokerABNOrCID);

			CreateCustomCode(branchProxy, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "BNA", Core.Constants.CountryCodes.Australia);
			Factory.Save();
			AssertEquals("Get Company Proxy ABN", "BNA", DeclarationWrapper.BrokerABNOrCID);
		}

		public void TestAlertText()
		{
			try
			{
				DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "A test message for alert");
				AssertEquals("Alert text", "A test message for alert", DeclarationWrapper.AlertText);
			}
			finally
			{
				((IRegistryItemInternals)DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText).ClearCache();
			}
		}

		public void TestOwnerRefAndOrderRefHeading()
		{
			AssertEquals("ORDER NUMBERS / REFERENCE", DeclarationWrapper.OwnerRefAndOrderRefHeading);
		}

		public void TestConsigneeOrgHeading()
		{
			AssertEquals("IMPORTER", DeclarationWrapper.ConsigneeOrgHeading);
		}

		public void ConsignorOrgHeading()
		{
			AssertEquals("SUPPLER", DeclarationWrapper.ConsignorOrgHeading);
		}

		#endregion

		#region ZDecimal Fields

		public void TestTotalFOBInLocalCurrency()
		{
			BaseJobComInvoiceHeader header1 = Declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader header2 = Declaration.Invoices.AddNew();

			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			header1.JZ_IncoTerm = "FOB";

			header2.JZ_InvoiceAmount = 2500m;
			header2.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			header2.JZ_IncoTerm = "FOB";

			AssertEquals(DeclarationWrapper.TotalFOBInLocalCurrency, Declaration.TotalFOBInLocalCurrency.Amount);
		}

		public void TestTotalExportFOBAmount()
		{
			BaseJobComInvoiceHeader header1 = Declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader header2 = Declaration.Invoices.AddNew();

			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			header1.JZ_IncoTerm = "FOB";

			header2.JZ_InvoiceAmount = 2500m;
			header2.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			header2.JZ_IncoTerm = "FOB";

			AssertEquals(3500m, DeclarationWrapper.TotalExportFOBAmount);
		}

		public void TestDeliveryOrPickupLabourTime()
		{
			Declaration.JE_DeliveryOrPickupLabourTime = new ZDateTime(2006, 1, 1, 5, 45, 0);
			AssertEquals("DeliveryOrPickupLabourHours", Declaration.JE_DeliveryOrPickupLabourTime, DeclarationWrapper.DeliveryOrPickupLabourTime);
		}

		public void TestDeliveryOrPickupLabourCharge()
		{
			Declaration.JE_DeliveryOrPickupLabourCharge = 123.34M;
			AssertEquals("DeliveryOrPickupLabourCharge", Declaration.JE_DeliveryOrPickupLabourCharge, DeclarationWrapper.DeliveryOrPickupLabourCharge);
		}

		public void TestDemurrageOnDeliveryOrPickupCharge()
		{
			Declaration.JE_PickupOrDeliveryTruckWaitCharge = 123.34M;
			AssertEquals("DetentionOnDeliveryOrPickupCharge", Declaration.JE_PickupOrDeliveryTruckWaitCharge, DeclarationWrapper.DemurrageOnDeliveryOrPickupCharge);
		}

		public void TestTotalVolume()
		{
			Declaration.JE_TotalVolume = 123.34M;
			AssertEquals("TotalVolume", Declaration.JE_TotalVolume, DeclarationWrapper.TotalVolume);
		}

		public void TestTotalWeight()
		{
			Declaration.JE_TotalWeight = 123.34M;
			AssertEquals("TotalWeight", Declaration.JE_TotalWeight, DeclarationWrapper.TotalWeight);
		}

		#endregion

		#region ZDateTime Fields

		public void TestETD()
		{
			Declaration.JE_DateAtOrigin = ZDateTime.Today;
			AssertEquals("ETD is Date at Origin", ZDateTime.Today, DeclarationWrapper.ETD);
		}

		public void TestETA()
		{
			Declaration.JE_DateAtFinalDestination = ZDateTime.Today;
			AssertEquals("ETA is Date at Final Destination", ZDateTime.Today, DeclarationWrapper.ETA);
		}

		public void TestConversionDate()
		{
			Declaration.JE_ExportDate = new ZDateTime(2005, 1, 12);
			AssertEquals(DeclarationWrapper.ConversionDate, ((ICurrencyConverterProvider)Declaration).CurrencyConverter.DateForRate);
		}

		public void TestEstimatedDeliveryOrPickup()
		{
			ZDateTime estimatedDeliveryOrPickup = new ZDateTime(2004, 04, 04);
			Declaration.JE_EstimatedDeliveryOrPickup = estimatedDeliveryOrPickup;
			AssertEquals("EstimatedDeliveryOrPickup", estimatedDeliveryOrPickup, DeclarationWrapper.EstimatedDeliveryOrPickup);
		}

		public void TestDeliveryOrPickupRequiredBy()
		{
			ZDateTime deliveryOrPickupRequiredBy = new ZDateTime(2004, 04, 04);
			Declaration.JE_DeliveryOrPickupRequiredBy = deliveryOrPickupRequiredBy;
			AssertEquals("DeliveryOrPickupRequiredBy", deliveryOrPickupRequiredBy, DeclarationWrapper.DeliveryOrPickupRequiredBy);
		}

		public void TestDateAtFinalDestination()
		{
			ZDateTime dateAtFinalDestination = new ZDateTime(2004, 04, 04);
			Declaration.JE_DateAtFinalDestination = dateAtFinalDestination;
			AssertEquals("DateAtFinalDestination", dateAtFinalDestination, DeclarationWrapper.DateAtFinalDestination);
		}

		public void TestDateAtOrigin()
		{
			ZDateTime dateAtOrigin = new ZDateTime(2004, 04, 04);
			Declaration.JE_DateAtOrigin = dateAtOrigin;
			AssertEquals("DateAtOrigin", dateAtOrigin, DeclarationWrapper.DateAtOrigin);
		}

		public void TestDateOfArrival()
		{
			ZDateTime dateOfArrival = new ZDateTime(2004, 04, 04);
			Declaration.JE_DateOfArrival = dateOfArrival;
			AssertEquals("DateOfArrival", dateOfArrival, DeclarationWrapper.DateOfArrival);
		}

		public virtual void TestDateOfFirstArrival()
		{
			ZDateTime dateOfFirstArrival = new ZDateTime(2004, 04, 04);
			Declaration.JE_DateOfFirstArrival = dateOfFirstArrival;
			AssertEquals("DateOfFirstArrival", dateOfFirstArrival, DeclarationWrapper.DateOfFirstArrival);
		}

		public void TestEntryAuthorisationDate()
		{
			ZDateTime entryAuthorisationDate = new ZDateTime(2004, 04, 04);
			Declaration.JE_EntryAuthorisationDate = entryAuthorisationDate;
			AssertEquals("EntryAuthorisationDate", entryAuthorisationDate, DeclarationWrapper.EntryAuthorisationDate);
		}

		public void TestEntrySubmittedDate()
		{
			ZDateTime entrySubmittedDate = new ZDateTime(2004, 04, 04);
			Declaration.JE_EntrySubmittedDate = entrySubmittedDate;
			AssertEquals("EntrySubmittedDate", entrySubmittedDate, DeclarationWrapper.EntrySubmittedDate);
		}

		public void TestExportDate()
		{
			ZDateTime exportDate = new ZDateTime(2004, 04, 04);
			Declaration.JE_ExportDate = exportDate;
			AssertEquals("ExportDate", exportDate, DeclarationWrapper.ExportDate);
		}

		public void TestSystemCreateTime()
		{
			ZDateTime systemCreateTime = new ZDateTime(2004, 04, 04);
			Declaration.JE_SystemCreateTimeUtc = systemCreateTime;
			AssertEquals("SystemCreateTime", systemCreateTime, DeclarationWrapper.SystemCreateTime);
		}

		public void TestSystemLastEditTime()
		{
			ZDateTime systemLastEditTime = new ZDateTime(2004, 04, 04);
			Declaration.JE_SystemLastEditTimeUtc = systemLastEditTime;
			AssertEquals("SystemLastEditTime", systemLastEditTime, DeclarationWrapper.SystemLastEditTime);
		}

		public void TestCartageAdvised()
		{
			ZDateTime cartageAdvised = new ZDateTime(2004, 04, 04);
			Declaration.JP_Calc_CartageAdvised = cartageAdvised;
			AssertEquals("CartageAdvised", cartageAdvised, DeclarationWrapper.CartageAdvised);
		}

		public void TestCartageCompleted()
		{
			ZDateTime cartageCompleted = new ZDateTime(2004, 04, 04);
			Declaration.JE_CartageCompleted = cartageCompleted;
			AssertEquals("CartageCompleted", cartageCompleted, DeclarationWrapper.CartageCompleted);
		}

		public void TestStorageCommenceDate()
		{
			AssertEquals("StorageCommenceDate", ZDateTime.Empty, DeclarationWrapper.StorageCommenceDate);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.LCLStorageCommences = new ZDateTime(2004, 02, 02);
			AssertEquals("StorageCommenceDate Sea & LCL & Container", container1.LCLStorageCommences, DeclarationWrapper.StorageCommenceDate);

			Declaration.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2004, 04, 04);
			AssertEquals("StorageCommenceDate Docs and Cartage", Declaration.DocsAndCartage.JP_LCLStorageCommences, DeclarationWrapper.StorageCommenceDate);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2004, 05, 05);
			AssertEquals("StorageCommenceDate Sea & FCL", Declaration.DocsAndCartage.JP_FCLStorageCommences, DeclarationWrapper.StorageCommenceDate);
		}

		public void TestLCLStorageCommenceDate()
		{
			AssertEquals("LCLStorageCommenceDate", ZDateTime.Empty, DeclarationWrapper.LCLStorageCommenceDate);

			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.LCLStorageCommences = new ZDateTime(2004, 02, 02);
			AssertEquals("LCLStorageCommenceDate", container1.LCLStorageCommences, DeclarationWrapper.LCLStorageCommenceDate);

			Declaration.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2004, 04, 04);
			AssertEquals("LCLStorageCommenceDate", Declaration.DocsAndCartage.JP_LCLStorageCommences, DeclarationWrapper.LCLStorageCommenceDate);
		}

		public void TestFCLStorageCommenceDate()
		{
			AssertEquals("FCLStorageCommenceDate", ZDateTime.Empty, DeclarationWrapper.FCLStorageCommenceDate);

			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.ArrivalCTOStorageStartDate = new ZDateTime(2004, 02, 02);
			AssertEquals("FCLStorageCommenceDate", container1.ArrivalCTOStorageStartDate, DeclarationWrapper.FCLStorageCommenceDate);

			Declaration.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2004, 04, 04);
			AssertEquals("FCLStorageCommenceDate", Declaration.DocsAndCartage.JP_FCLStorageCommences, DeclarationWrapper.FCLStorageCommenceDate);
		}

		public void TestAvailableDate()
		{
			AssertEquals("AvailableDate", ZDateTime.Empty, DeclarationWrapper.AvailableDate);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.LCLAvailable = new ZDateTime(2004, 02, 02);

			Declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2004, 04, 04);
			AssertEquals("AvailableDate Docs and Cartage & LCL", Declaration.DocsAndCartage.JP_LCLAvailable, DeclarationWrapper.AvailableDate);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2004, 05, 05);
			AssertEquals("AvailableDate Sea & FCL", Declaration.DocsAndCartage.JP_FCLAvailable, DeclarationWrapper.AvailableDate);
		}

		public void TestLCLAvailableDate()
		{
			AssertEquals("LCLAvailableDate", ZDateTime.Empty, DeclarationWrapper.LCLAvailableDate);

			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.LCLAvailable = new ZDateTime(2004, 02, 02);
			AssertEquals("LCLAvailableDate", container1.LCLAvailable, DeclarationWrapper.LCLAvailableDate);

			Declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2004, 04, 04);
			AssertEquals("LCLAvailableDate", Declaration.DocsAndCartage.JP_LCLAvailable, DeclarationWrapper.LCLAvailableDate);
		}

		public void TestFCLAvailableDate()
		{
			AssertEquals("FCLAvailableDate", ZDateTime.Empty, DeclarationWrapper.FCLAvailableDate);

			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.FCLAvailable = new ZDateTime(2004, 02, 02);
			AssertEquals("FCLAvailableDate", container1.FCLAvailable, DeclarationWrapper.FCLAvailableDate);

			Declaration.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2004, 04, 04);
			AssertEquals("FCLAvailableDate", Declaration.DocsAndCartage.JP_FCLAvailable, DeclarationWrapper.FCLAvailableDate);
		}

		public void HouseBillIssueDate()
		{
			AssertEquals("HouseBillIssueDate should be emtpy", ZDateTime.Empty, DeclarationWrapper.HouseBillIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ZDateTime today = ZDateTime.Today;
			shipment.JS_HouseBillIssueDate = today;
			Declaration.JE_JS = shipment.PK;
			AssertEquals("HouseBillIssueDate should be emtpy", today, DeclarationWrapper.HouseBillIssueDate);
		}

		public void HouseBillAndIssueDate()
		{
			AssertEquals("HouseBillIssueDate should be emtpy", ZDateTime.Empty, DeclarationWrapper.HouseBillAndIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ZDateTime today = ZDateTime.Today;
			shipment.JS_HouseBillIssueDate = today;
			Declaration.JE_JS = shipment.PK;
			Declaration.JE_HouseBill = "12345";
			AssertEquals("HouseBillIssueDate should be emtpy", "12345 // " + today.ToShortDateString(), DeclarationWrapper.HouseBillAndIssueDate);
		}

		public void MasterBillIssueDate()
		{
			AssertEquals("MasterBillIssueDate should be emtpy", ZDateTime.Empty, DeclarationWrapper.MasterBillIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			ZDateTime today = ZDateTime.Today;
			consol.JK_MasterBillIssueDate = today;
			Declaration.JE_JS = shipment.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("MasterBillIssueDate - Only show issue date for air", ZDateTime.Empty, DeclarationWrapper.MasterBillIssueDate);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("MasterBillIssueDate should be emtpy", today, DeclarationWrapper.MasterBillIssueDate);
		}

		public void MasterBillAndIssueDate()
		{
			AssertEquals("MasterBillIssueDate should be emtpy", ZDateTime.Empty, DeclarationWrapper.MasterBillAndIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			ZDateTime today = ZDateTime.Today;
			consol.JK_MasterBillIssueDate = today;
			Declaration.JE_MasterBill = "1234";
			Declaration.JE_JS = shipment.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("MasterBillIssueDate - Only show issue date for air", "1234", DeclarationWrapper.MasterBillAndIssueDate);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("MasterBillIssueDate should be emtpy", "1234 // " + today.ToShortDateString(), DeclarationWrapper.MasterBillAndIssueDate);
		}
		#endregion

		#region ZByte Fields

		public void TestDemurrageOnDeliveryOrPickupTime()
		{
			Declaration.JE_PickupOrDeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 5, 45, 0);
			AssertEquals("DemurrageOnDeliveryOrPickupTime", Declaration.JE_PickupOrDeliveryTruckWaitTime, DeclarationWrapper.DemurrageOnDeliveryOrPickupTime);
		}

		public void TestLandedCostByCost()
		{
			Declaration.JE_LandedCostByCost = 12;
			AssertEquals("LandedCostByCost", (decimal)Declaration.JE_LandedCostByCost / 100, DeclarationWrapper.LandedCostByCost);
		}

		public void TestLandedCostByUnits()
		{
			Declaration.JE_LandedCostByUnits = 12;
			AssertEquals("LandedCostByUnits", (decimal)Declaration.JE_LandedCostByUnits / 100, DeclarationWrapper.LandedCostByUnits);
		}

		public void TestLandedCostByVolume()
		{
			Declaration.JE_LandedCostByVolume = 12;
			AssertEquals("LandedCostByVolume", (decimal)Declaration.JE_LandedCostByVolume / 100, DeclarationWrapper.LandedCostByVolume);
		}

		public void TestLandedCostByWeight()
		{
			Declaration.JE_LandedCostByWeight = 12;
			AssertEquals("LandedCostByWeight", (decimal)Declaration.JE_LandedCostByWeight / 100, DeclarationWrapper.LandedCostByWeight);
		}

		#endregion

		#region Wrapper Fields

		public void TestLocalParty()
		{
			OrgHeader importerObj = CreateTestOrgHeader("IMXXX", "Supplier Org", "000 Street", "ZYZ Lane", "Sydney");
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.JE_OH_Importer = importerObj.PK;
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)DeclarationWrapper.LocalParty.WrappedObject;
			AssertEquals(importerObj.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);

			OrgHeader supplierObj = CreateTestOrgHeader("SUXXX", "Supplier Org", "000 Street", "ZYZ Lane", "Sydney");
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			Declaration.JE_OH_Supplier = supplierObj.PK;
			intermediateWrapper = (DocBaseWrapper)DeclarationWrapper.LocalParty.WrappedObject;
			AssertEquals(supplierObj.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);
		}

		public void TestExportPickupAddress()
		{
			OrgHeader supplierObject = CreateTestOrgHeader(Factory, "SUXXX", "Supplier Org", "000 Street", "ZYZ Lane", "Sydney");
			OrgAddress pICAddress = supplierObject.Addresses.AddNew();
			pICAddress.OA_Address1 = "34 Hobbiton Street";
			pICAddress.OA_Address2 = "Hobbitown";
			pICAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			pICAddress.SetDefaultUsageComment(null);

			OrgAddress oFCAddress = supplierObject.MainAddress;
			oFCAddress.OA_Address1 = "44 Gollumlane Street";
			oFCAddress.OA_Address2 = "Gollumville";
			oFCAddress.SetDefaultUsageComment(null);
			Declaration.JE_OH_Supplier = supplierObject.PK;

			Factory.Save();
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Export pickup address is to Consignor's address", pICAddress.OA_Address1, DeclarationWrapper.PickupAddress.Address1);
		}

		public virtual void TestImportSeaCNTPickupAddress()
		{
			OrgAddress depotAddress = CreateOrgAddress(Factory, "DepotAddress1", "DepotAddress2");
			OrgAddress cTOAddress = CreateOrgAddress(Factory, "CTOAddress1", "CTOAddress2");
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;

			Declaration.JE_MessageType = "IMP";
			Declaration.JE_ContainerMode = "CNT";
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			CreateContainer(Declaration, "FCL");

			AssertEquals("Pickup address from CTO", cTOAddress.OA_Address1, DeclarationWrapper.PickupAddress.Address1);
			AssertEquals("Pickup from CTO company", cTOAddress.Header.OH_FullName, DeclarationWrapper.PickupAddress.CompanyName);

			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Delivery address null", DeclarationWrapper.PickupAddress);
		}

		public void TestImportSeaBBKPickupAddress()
		{
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			OrgAddress depotAddress = CreateOrgAddress(Factory, "DepotAddress1", "DepotAddress2");
			OrgAddress cTOAddress = CreateOrgAddress(Factory, "CTOAddress1", "CTOAddress2");
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;

			AssertEquals("Pickup address from Depot", cTOAddress.OA_Address1, DeclarationWrapper.PickupAddress.Address1);
			AssertEquals("Pickup from Depot company", cTOAddress.Header.OH_FullName, DeclarationWrapper.PickupAddress.CompanyName);

			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Delivery address null", DeclarationWrapper.PickupAddress);
		}

		public void TestImportAirPickupAddress()
		{
			OrgAddress depotAddress = CreateOrgAddress(Factory, "DepotAddress1", "DepotAddress2");
			OrgAddress cTOAddress = CreateOrgAddress(Factory, "CTOAddress1", "CTOAddress2");
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;

			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;

			AssertEquals("Pickup address from Depot", depotAddress.OA_Address1, DeclarationWrapper.PickupAddress.Address1);
			AssertEquals("Pickup from Depot company", depotAddress.Header.OH_FullName, DeclarationWrapper.PickupAddress.CompanyName);
		}

		public void TestImportDeliveryAddress()
		{
			var importerObject = CreateTestOrgHeader(Factory, "IMPXXX", "Importer Org", "123 Street", "ABC Lane", "Sydney");
			var address1 = importerObject.Addresses.AddNew();
			address1.OA_Address1 = "15 Elvish Street";
			address1.OA_Address2 = "Rivendell";
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			address1.SetDefaultUsageComment(null);

			var oFCAddress = importerObject.MainAddress;
			oFCAddress.OA_Address1 = "99 Devilish Street";
			oFCAddress.OA_Address2 = "Mordor";
			oFCAddress.SetDefaultUsageComment(null);

			Declaration.JE_OH_Importer = importerObject.PK;

			Declaration.JE_MessageType = "IMP";
			Factory.Save();
			AssertEquals(address1.OA_Address1, DeclarationWrapper.DeliverToAddress.Address1);

			Declaration.JE_OverrideFreightDefaults = true;
			Declaration.ClientPickupDeliveryAddressPK = oFCAddress.PK;
			Declaration.JE_MessageType = "IMP";
			AssertEquals(oFCAddress.OA_Address1, DeclarationWrapper.DeliverToAddress.Address1);
		}

		public void TestExportSeaFCLDeliveryAddress()
		{
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;

			OrgAddress depotAddress = CreateOrgAddress(Factory, "DepotAddress1", "DepotAddress2");
			OrgAddress cTOAddress = CreateOrgAddress(Factory, "CTOAddress1", "CTOAddress2");
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = "FCL";
			//To do : changed back when changes for NZ will be checked in; expected on 15th of September
			Declaration.CusContainers[0].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			//
			AssertEquals("Delivery address to CTO", cTOAddress.OA_Address1, DeclarationWrapper.DeliverToAddress.Address1);
			AssertEquals("Delivery to CTO company", cTOAddress.Header.OH_FullName, DeclarationWrapper.DeliverToAddress.CompanyName);

			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Delivery address null", DeclarationWrapper.DeliverToAddress);
		}

		public void TestExportSeaLCLDeliveryAddress()
		{
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;

			OrgAddress depotAddress = CreateOrgAddress(Factory, "DepotAddress1", "DepotAddress2");
			OrgAddress cTOAddress = CreateOrgAddress(Factory, "CTOAddress1", "CTOAddress2");
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;

			AssertEquals("Delivery address to Depot", depotAddress.OA_Address1, DeclarationWrapper.DeliverToAddress.Address1);
			AssertEquals("Delivery to Depot company", depotAddress.Header.OH_FullName, DeclarationWrapper.DeliverToAddress.CompanyName);

			Declaration.DepotDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Delivery address null", DeclarationWrapper.DeliverToAddress);
		}

		public void TestExportAirDeliveryAddress()
		{
			OrgAddress depotAddress = CreateOrgAddress(Factory, "DepotAddress1", "DepotAddress2");
			OrgAddress cTOAddress = CreateOrgAddress(Factory, "CTOAddress1", "CTOAddress2");
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.JE_MessageType = MessageTypeCodeForExports;

			AssertEquals("Delivery address to Depot", depotAddress.OA_Address1, DeclarationWrapper.DeliverToAddress.Address1);
			AssertEquals("Delivery to Depot company", depotAddress.Header.OH_FullName, DeclarationWrapper.DeliverToAddress.CompanyName);
		}

		public void TestForwardingAgentMerchant()
		{
			Declaration.JE_OH_ShippingLine = ZGuid.Empty;
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VoyageFlightNo = "VOY 222";

			AssertEquals("Return nothing", null, DeclarationWrapper.ForwardingAgentMerchant);
			AssertEquals("Heading is 'FORWARDER'", "FORWARDER", DeclarationWrapper.ForwardingAgentHeader);

			var headerBisObj = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			Declaration.JE_OH_ShippingLine = headerBisObj.PK;
			DocOrganisation organisation = DocOrganisation.New(headerBisObj, Factory);
			AssertEquals("Return shipping line address", organisation.PostalAddress, DeclarationWrapper.ForwardingAgentMerchant.PostalAddress);
			AssertEquals("Heading is 'SHIPPING LINE'", "SHIPPING LINE", DeclarationWrapper.ForwardingAgentHeader);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("Heading is 'AIRLINE'", "AIRLINE", DeclarationWrapper.ForwardingAgentHeader);

			headerBisObj = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			Declaration.JE_OH_Forwarder = headerBisObj.PK;
			organisation = DocOrganisation.New(headerBisObj, Factory);
			AssertEquals("Forwarders address", organisation.PostalAddress, DeclarationWrapper.ForwardingAgentMerchant.PostalAddress);
			AssertEquals("Heading is 'FORWARDER'", "FORWARDER", DeclarationWrapper.ForwardingAgentHeader);
		}

		public void TestGoodsAvailableAt()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			OrgAddress depotAddress = CreateOrgAddress(Factory, "DepotAddress1", "DepotAddress2");
			OrgAddress cTOAddress = CreateOrgAddress(Factory, "CTOAddress1", "CTOAddress2");
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;
			AssertEquals("FCL Shipment, Goods available at CTO", cTOAddress.OA_Address1, DeclarationWrapper.GoodsAvailableAt.Address1);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("LCL Container, Goods available at Depot", depotAddress.OA_Address1, DeclarationWrapper.GoodsAvailableAt.Address1);
		}

		public void TestNotifyParty()
		{
			OrgHeader importerBizo = CreateTestOrgHeader(Factory, "IMPXXX", "Importer Org", "123 Street", "ABC Lane", "Sydney");
			importerBizo.Contacts.RemoveAndDeleteAll();

			Declaration.JE_OH_Importer = importerBizo.PK;
			OrgContact cNRContact = importerBizo.Contacts.AddNew();
			cNRContact.OC_ContactName = "Suzie";
			cNRContact.Documents.AddNew();
			cNRContact.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact nOTContact = importerBizo.Contacts.AddNew();
			nOTContact.OC_ContactName = "Lizzie";
			nOTContact.Documents.AddNew();
			nOTContact.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			Factory.Save();
			ZString importerAddress = DocOrganisation.New(importerBizo, Factory).PostalAddress;

			AssertEquals("Notify Party contact name", "Lizzie", DeclarationWrapper.NotifyParty.ContactName);
			AssertEquals("Notify Party contact with no address override", "LIZZIE\n" + importerAddress, DeclarationWrapper.NotifyParty.PostalAddress);
		}

		public void TestRegistryCartageCompany()
		{
			SetFCL_LCL_AIRCartageBizo(Factory);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			Guid fCLCartageGuid = FreightDataRegistry.Instance.FCLCartageCompany.Value;
			Guid lCLCartageGuid = FreightDataRegistry.Instance.LCLCartageCompany.Value;
			Guid aIRCartageGuid = FreightDataRegistry.Instance.AIRCartageCompany.Value;

			try
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, FCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, LCLCartage.PK.ToGuid());
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, AIRCartage.PK.ToGuid());

				AssertEquals("Cartage company should be Registry's FCL Cartage", "ABC Pty Ltd.", DeclarationWrapper.CartageOrganisation.Name);

				Declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
				AssertEquals("Cartage company should be Registry's LCL Cartage", "LCL Pty Ltd.", DeclarationWrapper.CartageOrganisation.Name);

				Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
				AssertEquals("Cartage company should be Registry's AIR Cartage", "Airway Pty Ltd.", DeclarationWrapper.CartageOrganisation.Name);
			}
			finally
			{
				FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, fCLCartageGuid);
				FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, lCLCartageGuid);
				FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, aIRCartageGuid);
			}
		}

		public void TestBankAccount()
		{
			AccBankAccount[] defaultBankAccounts = (AccBankAccount[])Factory.Load(typeof(AccBankAccount), new ZQuery(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, ZBool.True));
			foreach (AccBankAccount account in defaultBankAccounts)
			{
				account.AB_IsDefaultReceiptBankAccount = ZBool.False;
			}

			var headerBisObj = Factory.NewWithValidTestData<AccGLHeader>();

			var accountBisObj = Factory.New<AccBankAccount>();
			accountBisObj.AB_BankName = "Test Bank Account";
			accountBisObj.AB_BankAddress = "123 Address Test";
			accountBisObj.AB_GC = GlbCompany.CurrentCompany.PK;
			accountBisObj.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			accountBisObj.AB_GB = GlbBranch.CurrentBranch.PK;
			accountBisObj.AB_IsDefaultReceiptBankAccount = ZBool.True;
			accountBisObj.AB_AG = headerBisObj.PK;
			accountBisObj.AB_Code = "ABCBANK";
			Factory.Save();

			AssertEquals("Test Bank Account", DeclarationWrapper.BankAccount.BankName);
			AssertEquals("123 Address Test", DeclarationWrapper.BankAccount.BankAddress);
		}

		public void TestDeliveryOrPickupCartageCo()
		{
			AssertNull("DeliveryOrPickupCartageCo", DeclarationWrapper.DeliveryOrPickupCartageCo);

			Declaration.DeliveryOrPickupCartageCoPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("DeliveryOrPickupCartageCo", DeclarationWrapper.DeliveryOrPickupCartageCo);
			AssertEquals("DeliveryOrPickupCartageCo is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.DeliveryOrPickupCartageCo.GetType());
		}

		public void TestBranch()
		{
			Declaration.JE_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			AssertNotNull("Branch", DeclarationWrapper.Branch);
			AssertEquals("Branch is of type DocBranch", typeof(DocBranch), DeclarationWrapper.Branch.GetType());
		}

		public void TestJobHeader()
		{
			AssertNull("JobHeader", DeclarationWrapper.JobHeader);
		}

		public void TestShipment()
		{
			AssertNull("Shipment", DeclarationWrapper.Shipment);

			Declaration.JE_JS = Factory.New(typeof(ForwardingShipment)).PK;
			AssertNotNull("Shipment", DeclarationWrapper.Shipment);
			AssertEquals("Shipment is of type DocForwardingShipment", typeof(DocForwardingShipment), DeclarationWrapper.Shipment.GetType());
		}

		public void TestContainerParkAddress()
		{
			AssertNull("ContainerParkAddress", DeclarationWrapper.ContainerParkAddress);

			Declaration.ContainerYardDocAddress.E2_OA_Address = Factory.New(typeof(OrgAddress)).PK;
			AssertNotNull("ContainerParkAddress", DeclarationWrapper.ContainerParkAddress);
			AssertEquals("ContainerParkAddress is of type DocDocAddress", typeof(DocDocAddress), DeclarationWrapper.ContainerParkAddress.GetType());
		}

		public void TestCTOAddress()
		{
			AssertNull("CTOAddress", DeclarationWrapper.CTOAddress);

			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = Factory.New(typeof(OrgAddress)).PK;
			AssertNotNull("CTOAddress", DeclarationWrapper.CTOAddress);
			AssertEquals("CTOAddress is of type DocDocAddress", typeof(DocDocAddress), DeclarationWrapper.CTOAddress.GetType());
		}

		public void TestDepotAddress()
		{
			AssertNull("DepotAddress", DeclarationWrapper.DepotAddress);

			OrgHeader header = OrgHeader.New(Factory);
			Declaration.DepotDocAddress.E2_OA_Address = header.Addresses.AddNew().PK;
			AssertNotNull("DepotAddress", DeclarationWrapper.DepotAddress);
			AssertEquals("DepotAddress is of type DocDocAddress", typeof(DocDocAddress), DeclarationWrapper.DepotAddress.GetType());
		}

		public void TestWarehouseAddress()
		{
			AssertNull("WarehouseAddress", DeclarationWrapper.WarehouseAddress);

			var header = Factory.New<OrgHeader>();
			OrgAddress address = header.Addresses.AddNew();
			Declaration.WarehouseDocAddress.E2_OA_Address = address.PK;
			AssertNotNull("WarehouseAddress", DeclarationWrapper.WarehouseAddress);
			AssertEquals("WarehouseAddress is of type DocDocAddress", typeof(DocDocAddress), DeclarationWrapper.WarehouseAddress.GetType());
		}

		public void TestForwarder()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNull("Forwarder", DeclarationWrapper.Forwarder);

			Declaration.JE_OH_Forwarder = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Forwarder", DeclarationWrapper.Forwarder);
			AssertEquals("Forwarder is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.Forwarder.GetType());
		}

		public void TestImporter()
		{
			AssertNull("Importer", DeclarationWrapper.Importer);

			Declaration.JE_OH_Importer = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Importer", DeclarationWrapper.Importer);
			AssertEquals("Importer is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.Importer.GetType());
		}

		public void TestShippingLine()
		{
			AssertNull("ShippingLine", DeclarationWrapper.ShippingLine);

			Declaration.JE_OH_ShippingLine = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("ShippingLine", DeclarationWrapper.ShippingLine);
			AssertEquals("ShippingLine is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.ShippingLine.GetType());
		}

		public void TestSupplier()
		{
			AssertNull("Supplier", DeclarationWrapper.Supplier);

			Declaration.JE_OH_Supplier = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Supplier", DeclarationWrapper.Supplier);
			AssertEquals("Supplier is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.Supplier.GetType());
		}

		public void TestExportForwarder()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertEquals("No Forwarder", null, DeclarationWrapper.ExportForwarder);

			var shipment = Factory.New<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			AssertEquals("No Forwarder", null, DeclarationWrapper.ExportForwarder);

			ForwardingConsol consol = shipment.Consols.AddNew();
			OrgHeader fwd = OrgHeader.New(Factory);
			fwd.OH_FullName = "TEST FWD XYZ";
			consol.SetDefaultSendingForwarderAddress(fwd);
			AssertEquals("Forwarder from Consol's Agent", "TEST FWD XYZ", DeclarationWrapper.ExportForwarder.Name);

			OrgHeader decFwd = OrgHeader.New(Factory);
			decFwd.OH_FullName = "DEC FWD ABC";
			Declaration.JE_OH_Forwarder = decFwd.PK;
			AssertEquals("Forwarder from Declaration's Forwarder", "DEC FWD ABC", DeclarationWrapper.ExportForwarder.Name);
		}
		public void TestFinalDestination()
		{
			AssertNull("FinalDestination", DeclarationWrapper.FinalDestination);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Declaration.JE_RL_NKFinalDestination = uNLOCO.RL_Code;
			AssertNotNull("FinalDestination", DeclarationWrapper.FinalDestination);
			AssertEquals("FinalDestination is of type DocUNLOCO", typeof(DocUNLOCO), DeclarationWrapper.FinalDestination.GetType());
		}

		public void TestOrigin()
		{
			AssertNull("Origin", DeclarationWrapper.Origin);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Declaration.JE_RL_NKOrigin = uNLOCO.RL_Code;
			AssertNotNull("Origin", DeclarationWrapper.Origin);
			AssertEquals("Origin is of type DocUNLOCO", typeof(DocUNLOCO), DeclarationWrapper.Origin.GetType());
		}

		public void TestPortOfArrival()
		{
			AssertNull("PortOfArrival", DeclarationWrapper.PortOfArrival);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Declaration.JE_RL_NKPortOfArrival = uNLOCO.RL_Code;
			AssertNotNull("PortOfArrival", DeclarationWrapper.PortOfArrival);
			AssertEquals("PortOfArrival is of type DocUNLOCO", typeof(DocUNLOCO), DeclarationWrapper.PortOfArrival.GetType());
		}

		public virtual void TestPortOfFirstArrival()
		{
			AssertNull("PortOfFirstArrival", DeclarationWrapper.PortOfFirstArrival);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Declaration.JE_RL_NKPortOfFirstArrival = uNLOCO.RL_Code;
			AssertNotNull("PortOfFirstArrival", DeclarationWrapper.PortOfFirstArrival);
			AssertEquals("PortOfFirstArrival is of type DocUNLOCO", typeof(DocUNLOCO), DeclarationWrapper.PortOfFirstArrival.GetType());
		}

		public void TestPortOfLoading()
		{
			AssertNull("PortOfLoading", DeclarationWrapper.PortOfLoading);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Declaration.JE_RL_NKPortOfLoading = uNLOCO.RL_Code;
			AssertNotNull("PortOfLoading", DeclarationWrapper.PortOfLoading);
			AssertEquals("PortOfLoading is of type DocUNLOCO", typeof(DocUNLOCO), DeclarationWrapper.PortOfLoading.GetType());
		}

		public void TestServiceLevel()
		{
			var serviceLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery());
			Declaration.JE_RS_NKServiceLevel = serviceLevel.RS_Code;
			AssertNotNull("ServiceLevel", DeclarationWrapper.ServiceLevel);
			AssertEquals("ServiceLevel is of type DocServiceLevel", typeof(DocServiceLevel), DeclarationWrapper.ServiceLevel.GetType());
		}

		public void TestVessel()
		{
			AssertNull("Vessel", DeclarationWrapper.Vessel);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.JE_VesselName = vessel.RV_Code;
			AssertNotNull("Vessel", DeclarationWrapper.Vessel);
			AssertEquals("Vessel is of type DocVessel", typeof(DocVessel), DeclarationWrapper.Vessel.GetType());
		}

		public void TestCartage()
		{
			Declaration.JE_MessageType = "IMP";
			JobDocsAndCartage docsAndCartage = Declaration.DocsAndCartage;

			var deliveryOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			docsAndCartage.DeliveryCartageCoPK = deliveryOrg.PK;

			var pickupOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			docsAndCartage.PickupCartageCoPK = pickupOrg.PK;

			AssertNotNull("Cartage", DeclarationWrapper.Cartage);
			AssertEquals("Cartage is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.Cartage.GetType());
			AssertEquals("Cartage is DeliveryOrg", deliveryOrg.OH_FullName, DeclarationWrapper.Cartage.Name);

			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertNotNull("Cartage", DeclarationWrapper.Cartage);
			AssertEquals("Cartage is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.Cartage.GetType());
			AssertEquals("Cartage is PickupOrg", pickupOrg.OH_FullName, DeclarationWrapper.Cartage.Name);
		}

		public void TestDocsAndCartage()
		{
			JobDocsAndCartage docsAndCartage = Declaration.DocsAndCartage;
			AssertNotNull("DocsAndCartage", DeclarationWrapper.DocsAndCartage);
			AssertEquals("DocsAndCartage is of type DocJobDocsAndCartage", typeof(DocJobDocsAndCartage), DeclarationWrapper.DocsAndCartage.GetType());
		}

		public void TestConsigneeOrg()
		{
			AssertNull("Consignee", DeclarationWrapper.ConsigneeOrg);

			Declaration.JE_OH_Importer = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Consignee", DeclarationWrapper.ConsigneeOrg);
			AssertEquals("Consignee is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.ConsigneeOrg.GetType());
		}

		public void TestConsignorOrg()
		{
			AssertNull("Consignor", DeclarationWrapper.ConsignorOrg);

			Declaration.JE_OH_Supplier = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Consignor", DeclarationWrapper.ConsignorOrg);
			AssertEquals("Consignor is of type DocOrganisation", typeof(DocOrganisation), DeclarationWrapper.ConsignorOrg.GetType());
		}

		#endregion

		#region ZBool Fields

		public void TestIsPersonalEffects()
		{
			Declaration.JE_IsPersonalEffects = ZBool.False;
			Assert("!IsPersonalEffects", !DeclarationWrapper.IsPersonalEffects);

			Declaration.JE_IsPersonalEffects = ZBool.True;
			Assert("IsPersonalEffects", DeclarationWrapper.IsPersonalEffects);
		}

		public void TestPrintAsContainers()
		{
			AssertEquals("Print As container", ZBool.False, DeclarationWrapper.PrintAsContainers);
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			BaseCusContainer container2 = Declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("Print As container", ZBool.False, DeclarationWrapper.PrintAsContainers);

			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Print As container", ZBool.True, DeclarationWrapper.PrintAsContainers);
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Print As container", ZBool.False, DeclarationWrapper.PrintAsContainers);

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("Print As container", ZBool.True, DeclarationWrapper.PrintAsContainers);
		}

		public void TestTransportModesIsXXX()
		{
			//do not change - tostring results to be used in templates
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("Transport Mode Air, TransportModeIsAir is true", "Y", DeclarationWrapper.TransportModeIsAir.ToString());

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("Transport Mode Sea, TransportModeIsSea is true", "Y", DeclarationWrapper.TransportModeIsSea.ToString());

			Declaration.JE_TransportMode = Declaration.TransportModeMailCodeForTesting;
			AssertEquals("Transport Mode Mail, TransportModeIsPost is true", "Y", DeclarationWrapper.TransportModeIsPost.ToString());

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Transport Mode Other, TransportModeIsOther is true", "Y", DeclarationWrapper.TransportModeIsOther.ToString());
		}

		public void TestDisplayLogo()
		{
			DocumentsDataRegistry.Instance.DisplayLogo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Should return false", ZBool.False, DeclarationWrapper.DisplayLogo);

			DocumentsDataRegistry.Instance.DisplayLogo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Should return true", ZBool.True, DeclarationWrapper.DisplayLogo);
		}

		#endregion

		#region ZShort Fields

		public void TestContainerCount()
		{
			Declaration.JE_ContainerCount = Convert.ToByte(1);
			AssertEquals("ContainerCount", Declaration.JE_ContainerCount, DeclarationWrapper.ContainerCount);
		}

		#endregion

		#region ZInt Fields

		public void TestTotalNoOfPacks()
		{
			Declaration.JE_TotalNoOfPacks = 12;
			AssertEquals("TotalNoOfPacks", Declaration.JE_TotalNoOfPacks, DeclarationWrapper.TotalNoOfPacks);
		}

		public void TestTotalNoOfPieces()
		{
			Declaration.JE_TotalNoOfPieces = 12;
			AssertEquals("TotalNoOfPieces", Declaration.JE_TotalNoOfPieces, DeclarationWrapper.TotalNoOfPieces);
		}

		#endregion

		#region Addresses

		public void TestJourneyOnePickUpAddress()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//FCL - 2 Journey's
			AssertNull("JourneyOnePickUpAddress", DeclarationWrapper.JourneyOnePickUpAddress);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.ContainerParkAddress.Address1, DeclarationWrapper.JourneyOnePickUpAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.CTOAddress.Address1, DeclarationWrapper.JourneyOnePickUpAddress.Address1);

			//LCL - 1 Journey
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.PickupAddress.Address1, DeclarationWrapper.JourneyOnePickUpAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.PickupAddress.Address1, DeclarationWrapper.JourneyOnePickUpAddress.Address1);

			//BBK - 1 Journey
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.BreakBulk);
			AssertEquals("JourneyOnePickUpAddress", Declaration.SupplierPickupAddress.Address.OA_Address1, DeclarationWrapper.JourneyOnePickUpAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.BreakBulk);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.CTOAddress.Address1, DeclarationWrapper.JourneyOnePickUpAddress.Address1);
		}

		public void TestJourneyOneDeliverToAddress()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//FCL - 2 Journey's
			AssertNull("JourneyOneDeliverToAddress", DeclarationWrapper.JourneyOneDeliverToAddress);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOneDeliverToAddress", DeclarationWrapper.DocsAndCartage.PickupAddress.Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOneDeliverToAddress", DeclarationWrapper.DocsAndCartage.DeliveryAddress.Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			Declaration.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToAddress fallback", DeclarationWrapper.Consignor.PickUpAddress.Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToAddress fallback", DeclarationWrapper.Consignee.DeliverAddress.Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);

			//LCL - 1 Journey
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.DeliverToAddress.Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.DeliverToAddress.Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);

			//BBK - 1 Journey
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.BreakBulk);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.CTOAddress.Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.BreakBulk);
			AssertEquals("JourneyOnePickUpAddress", Declaration.ImporterDeliveryAddress.Address.OA_Address1, DeclarationWrapper.JourneyOneDeliverToAddress.Address1);
		}

		public void TestJourneyTwoPickUpAddress()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//FCL - 2 Journey's
			AssertNull("TestJourneyTwoPickUpAddress", DeclarationWrapper.JourneyTwoPickUpAddress);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("TestJourneyTwoPickUpAddress", DeclarationWrapper.DocsAndCartage.PickupAddress.Address1, DeclarationWrapper.JourneyTwoPickUpAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("TestJourneyTwoPickUpAddress", DeclarationWrapper.DocsAndCartage.DeliveryAddress.Address1, DeclarationWrapper.JourneyTwoPickUpAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			Declaration.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("TestJourneyTwoPickUpAddress fallback", DeclarationWrapper.Consignor.PickUpAddress.Address1, DeclarationWrapper.JourneyTwoPickUpAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("TestJourneyTwoPickUpAddress fallback", DeclarationWrapper.Consignee.DeliverAddress.Address1, DeclarationWrapper.JourneyTwoPickUpAddress.Address1);
		}

		public void TestJourneyTwoDeliverToAddress()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//FCL - 2 Journey's
			AssertNull("JourneyTwoDeliverToAddress", DeclarationWrapper.JourneyTwoDeliverToAddress);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyTwoDeliverToAddress", DeclarationWrapper.CTOAddress.Address1, DeclarationWrapper.JourneyTwoDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyTwoDeliverToAddress", DeclarationWrapper.ContainerParkAddress.Address1, DeclarationWrapper.JourneyTwoDeliverToAddress.Address1);
		}

		#endregion

		#region Contacts

		public void TestJourneyOnePickUpContactDetails()
		{
			AssertEquals("JourneyOnePickUpContactName", ZString.Empty, DeclarationWrapper.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", ZString.Empty, DeclarationWrapper.JourneyOnePickUpContactPhone);

			//FCL - 2 journey's
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOnePickUpContactName", "ContainerParkName", DeclarationWrapper.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "ContainerParkPhone", DeclarationWrapper.JourneyOnePickUpContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOnePickUpContactName", "CTOName", DeclarationWrapper.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "CTOPhone", DeclarationWrapper.JourneyOnePickUpContactPhone);

			//LCL - 1 journey
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpContactName", "PickupName", DeclarationWrapper.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "PickupPhone", DeclarationWrapper.JourneyOnePickUpContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpContactName", "DepotName", DeclarationWrapper.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "DepotPhone", DeclarationWrapper.JourneyOnePickUpContactPhone);

			//LCL - Fallback
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			Declaration.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("JourneyOnePickUpContactName", "ConsignorName", DeclarationWrapper.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "ConsignorPhone", DeclarationWrapper.JourneyOnePickUpContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.LCL);
			Declaration.DepotDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("JourneyOnePickUpContactName", "", DeclarationWrapper.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "", DeclarationWrapper.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactDetails()
		{
			AssertEquals("JourneyOneDeliverToContactName", ZString.Empty, DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone", ZString.Empty, DeclarationWrapper.JourneyOneDeliverToContactPhone);

			//FCL - 2 journey's
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOneDeliverToContactName", "PickupName", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone", "PickupPhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyOneDeliverToContactName", "DeliveryName", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone", "DeliveryPhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);

			//FCL - Fallback
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			Declaration.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToContactName fallback", "ConsignorName", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone fallback", "ConsignorPhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToContactName fallback", "ConsigneeName", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone fallback", "ConsigneePhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);

			//LCL - 1 journey
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpContactName", "DepotName", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "DepotPhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.LCL);
			AssertEquals("JourneyOnePickUpContactName", "DeliveryName", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "DeliveryPhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);

			//LCL - FallBack
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			Declaration.DepotDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("JourneyOnePickUpContactName", "", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "", DeclarationWrapper.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.LCL);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("JourneyOnePickUpContactName", "ConsigneeName", DeclarationWrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "ConsigneePhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);
		}

		public void TestJourneyTwoPickUpContactDetails()
		{
			AssertEquals("JourneyTwoPickUpContactName", ZString.Empty, DeclarationWrapper.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone", ZString.Empty, DeclarationWrapper.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyTwoPickUpContactName", "PickupName", DeclarationWrapper.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone", "PickupPhone", DeclarationWrapper.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyTwoPickUpContactName", "DeliveryName", DeclarationWrapper.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone", "DeliveryPhone", DeclarationWrapper.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			Declaration.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyTwoPickUpContactName fallback", "ConsignorName", DeclarationWrapper.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone fallback", "ConsignorPhone", DeclarationWrapper.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyTwoPickUpContactName fallback", "ConsigneeName", DeclarationWrapper.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone fallback", "ConsigneePhone", DeclarationWrapper.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactDetails()
		{
			AssertEquals("JourneyTwoDeliverToContactName", ZString.Empty, DeclarationWrapper.JourneyTwoDeliverToContactName);
			AssertEquals("JourneyTwoDeliverToContactPhone", ZString.Empty, DeclarationWrapper.JourneyTwoDeliverToContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyTwoDeliverToContactName", "CTOName", DeclarationWrapper.JourneyTwoDeliverToContactName);
			AssertEquals("JourneyTwoDeliverToContactPhone", "CTOPhone", DeclarationWrapper.JourneyTwoDeliverToContactPhone);

			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			AssertEquals("JourneyTwoDeliverToContactName", "ContainerParkName", DeclarationWrapper.JourneyTwoDeliverToContactName);
			AssertEquals("JourneyTwoDeliverToContactPhone", "ContainerParkPhone", DeclarationWrapper.JourneyTwoDeliverToContactPhone);
		}

		#region JourneyOne

		public void TestJourneyOnePickUpContactName()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)DeclarationWrapper.JourneyOnePickUpAddress.WrappedObject;
			AssertEquals("Precondition: JourneyOnePickUpContactName", "ContainerParkName", DeclarationWrapper.JourneyOnePickUpContactName);
			journeyOnePickupDocAddress.E2_Contact = "ContainerParkName2";
			AssertEquals("JourneyOnePickUpContactName - No change expected as ContainerPark does not have JobDocAddress", "ContainerParkName", DeclarationWrapper.JourneyOnePickUpContactName);
		}

		public void TestJourneyOnePickUpContactPhone()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)DeclarationWrapper.JourneyOnePickUpAddress.WrappedObject;
			AssertEquals("Precondition: JourneyOnePickUpContactPhone", "ContainerParkPhone", DeclarationWrapper.JourneyOnePickUpContactPhone);
			journeyOnePickupDocAddress.E2_Contact = "ContainerParkName2";
			AssertEquals("JourneyOnePickUpContactPhone - No change expected as ContainerPark is not JobDocAddress", "ContainerParkPhone", DeclarationWrapper.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactName()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			DeclarationWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)DeclarationWrapper.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "DepotName";
			AssertEquals("JourneyOneDeliverToContactName", "DepotName", DeclarationWrapper.JourneyOneDeliverToContactName);
			journeyOneDeliverToDocAddress.E2_Contact = "CTOName";
			AssertEquals("JourneyOneDeliverToContactName", "CTOName", DeclarationWrapper.JourneyOneDeliverToContactName);
		}

		public void TestJourneyOneDeliverToContactPhone()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			DeclarationWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)DeclarationWrapper.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "DeliveryName2";
			AssertEquals("JourneyOneDeliverToContactPhone", "DeliveryPhone2", DeclarationWrapper.JourneyOneDeliverToContactPhone);
			journeyOneDeliverToDocAddress.E2_Contact = "DeliveryName";
			AssertEquals("JourneyOneDeliverToContactPhone", "DeliveryPhone", DeclarationWrapper.JourneyOneDeliverToContactPhone);
		}
		#endregion

		#region Journey Two

		public void TestJourneyTwoPickUpContactName()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			DeclarationWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)DeclarationWrapper.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "DepotName";
			AssertEquals("JourneyTwoPickUpContactName", "DepotName", DeclarationWrapper.JourneyTwoPickUpContactName);
			journeyTwoPickupDocAddress.E2_Contact = "CTOName";
			AssertEquals("JourneyTwoPickUpContactName", "CTOName", DeclarationWrapper.JourneyTwoPickUpContactName);
		}

		public void TestJourneyTwoPickUpContactPhone()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			DeclarationWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)DeclarationWrapper.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "PickupName2";
			AssertEquals("JourneyTwoPickUpContactPhone", "PickupPhone2", DeclarationWrapper.JourneyTwoPickUpContactPhone);
			journeyTwoPickupDocAddress.E2_Contact = "PickupName";
			AssertEquals("JourneyTwoPickUpContactPhone", "PickupPhone", DeclarationWrapper.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactName()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)DeclarationWrapper.JourneyTwoDeliverToAddress.WrappedObject;
			AssertEquals("JourneyTwoDeliverToContactName - No change expected as ContainerPark is not JobDocAddress", "ContainerParkName", DeclarationWrapper.JourneyTwoDeliverToContactName);
			journeyTwoDeliverToDocAddress.E2_Contact = "ContainerParkName2";
			AssertEquals("JourneyTwoDeliverToContactName - No change expected as ContainerPark is not JobDocAddress", "ContainerParkName", DeclarationWrapper.JourneyTwoDeliverToContactName);
		}

		public void TestJourneyTwoDeliverToContactPhone()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.FCL);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)DeclarationWrapper.JourneyTwoDeliverToAddress.WrappedObject;
			AssertEquals("Precondition: JourneyTwoDeliverToContactPhone", "ContainerParkPhone", DeclarationWrapper.JourneyTwoDeliverToContactPhone);
			journeyTwoDeliverToDocAddress.E2_Contact = "ContainerParkName2";
			AssertEquals("JourneyTwoDeliverToContactPhone - No change expected as ContainerPark is not JobDocAddress", "ContainerParkPhone", DeclarationWrapper.JourneyTwoDeliverToContactPhone);
		}

		#endregion

		#endregion

		#region Notes

		public void TestCartageInstructions()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Export note", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Cartage instruction notes\nMore Notes here.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "This is other notes and should not be included.", Factory);

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Cartage instructions", "Cartage instruction notes\nMore Notes here.", DeclarationWrapper.CartageInstructions);

			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Export note", DeclarationWrapper.CartageInstructions);
		}

		public void TestDeclarationAndOrgCartageInstructionIMP()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgPickup = Factory.NewWithValidTestData<OrgHeader>();
			var orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
			Declaration.JE_OH_Importer = orgConsignee.PK;
			Declaration.JE_OH_Supplier = orgConsignor.PK;
			Declaration.SupplierPickupAddress.OrganisationPK = orgPickup.PK;
			Declaration.ImporterDeliveryAddress.OrganisationPK = orgDelivery.PK;

			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions");
			FreightHelperClass.AddNote(orgPickup, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgPickup, deliveryDesc, "Delivery Instructions");
			FreightHelperClass.AddNote(orgDelivery, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgDelivery, deliveryDesc, "Delivery Instructions");

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Cartage Instructions from organizations", "Consignor Delivery Instructions\nConsignee Delivery Instructions", DeclarationWrapper.DeclarationOrOrgCartageInstructions);

			CreateNote_ConsideringShipment(Declaration, pickupDesc, "Declaration Pickup Instructions", Factory);
			CreateNote_ConsideringShipment(Declaration, deliveryDesc, "Declaration Delivery Instructions", Factory);

			AssertEquals("Cartage Instructions now from details on Declaration", "Declaration Delivery Instructions", DeclarationWrapper.DeclarationOrOrgCartageInstructions);
		}

		public void TestDeclarationOrOrgCartageInstructionEXP()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgPickup = Factory.NewWithValidTestData<OrgHeader>();
			var orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
			Declaration.JE_OH_Importer = orgConsignee.PK;
			Declaration.JE_OH_Supplier = orgConsignor.PK;
			Declaration.SupplierPickupAddress.OrganisationPK = orgPickup.PK;
			Declaration.ImporterDeliveryAddress.OrganisationPK = orgDelivery.PK;

			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions");
			FreightHelperClass.AddNote(orgPickup, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgPickup, deliveryDesc, "Delivery Instructions");
			FreightHelperClass.AddNote(orgDelivery, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgDelivery, deliveryDesc, "Delivery Instructions");

			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("All Cartage Instructions from Organizations", "Consignor Pickup Instructions\nConsignee Pickup Instructions", DeclarationWrapper.DeclarationOrOrgCartageInstructions);

			CreateNote_ConsideringShipment(Declaration, pickupDesc, "Declaration Pickup Instructions - Overriding Instruction Note entered on Job Declaration", Factory);
			CreateNote_ConsideringShipment(Declaration, deliveryDesc, "Declaration Delivery Instructions - Overriding Instruction Note entered on Job Declaration", Factory);
			AssertEquals("Cartage Instructions - should now only return the specific instructions entered on the Job.", "Declaration Pickup Instructions - Overriding Instruction Note entered on Job Declaration", DeclarationWrapper.DeclarationOrOrgCartageInstructions);
		}

		public void TestDetailedGoodsDescription()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Detailed goods description\r\nDetailed description of goods\r\n12345.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.LoadListInstructions.Description, "Other notes that should not be included.", Factory);
			AssertEquals("Detailed goods description\r\nDetailed description of goods\r\n12345.", DeclarationWrapper.DetailedGoodsDescription);
		}

		public void TestMarksAndNumbers()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is other notes and should not be included.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Marks and numbers Line One\nLine Two", Factory);

			AssertEquals("Marks and numbers", "Marks and numbers Line One\nLine Two", DeclarationWrapper.MarksAndNumbers);
		}

		public void TestCertificateOfOriginNotes()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "Certificate of origin notes\nMore Notes here.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "This is other notes and should not be included.", Factory);

			AssertEquals("Handling instructions", "Certificate of origin notes\nMore Notes here.", DeclarationWrapper.CertificateOfOriginNote);
		}

		public void TestHandlingInstructions()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Please take extra care\nWhen handling the goods.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "This is other notes and should not be included.", Factory);

			AssertEquals("Handling instructions", "Please take extra care\nWhen handling the goods.", DeclarationWrapper.HandlingInstruction);
		}

		public void TestDeclarationOrOrgHandlingInstruction()
		{
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeNote = orgConsignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = orgConsignee.PK;
			consigneeNote.ST_Table = orgConsignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgImporter Note";

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorNote = orgConsignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNote.ST_ParentID = orgConsignor.PK;
			consignorNote.ST_Table = orgConsignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgSuplier Note";

			var orgConsigneeDelivery = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeDeliveryNote = orgConsigneeDelivery.Notes.AddNew();
			consigneeDeliveryNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeDeliveryNote.ST_ParentID = orgConsigneeDelivery.PK;
			consigneeDeliveryNote.ST_Table = orgConsigneeDelivery.TableName;
			consigneeDeliveryNote.ST_NoteDataAsText = "OrgImporterDelivery Note";

			var orgConsignorPickup = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorPickupNote = orgConsignorPickup.Notes.AddNew();
			consignorPickupNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorPickupNote.ST_ParentID = orgConsignorPickup.PK;
			consignorPickupNote.ST_Table = orgConsignorPickup.TableName;
			consignorPickupNote.ST_NoteDataAsText = "OrgSuplierPickup Note";

			Declaration.JE_OH_Importer = orgConsignee.PK;
			Declaration.JE_OH_Supplier = orgConsignor.PK;
			Declaration.SupplierPickupAddress.OrganisationPK = orgConsignorPickup.PK;
			Declaration.ImporterDeliveryAddress.OrganisationPK = orgConsigneeDelivery.PK;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Handling Instructions - from organizations", "OrgImporter Note\nOrgSuplier Note", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Handling Instructions - from organizations", "OrgImporter Note\nOrgSuplier Note", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instruction notes\nMore Notes here.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "This is not a Handling instruction note and should not be included.", Factory);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Handling Instructions - declaration instructions only.", "Handling instruction notes\nMore Notes here.", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Handling Instructions - declaration instructions only.", "Handling instruction notes\nMore Notes here.", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);
		}

		public void TestDeclarationOrOrgHandlingInstructionWithContext()
		{
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			orgConsignee.OH_FullName = "ABConsignee";
			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			orgConsignor.OH_FullName = "DEConsignor";

			StmNote consigneeNoteFCL = orgConsignee.Notes.AddNew();
			consigneeNoteFCL.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNoteFCL.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consigneeNoteFCL.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.F);
			consigneeNoteFCL.ST_NoteDataAsText = "OrgImporter Note - Brokerage/Import/FCL";

			Declaration.JE_OH_Importer = orgConsignee.PK;
			Declaration.JE_OH_Supplier = orgConsignor.PK;
			Declaration.JE_ContainerMode = ZString.Empty;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Export Handling Instructions - from organization, should be blank", "", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals("Import Handling Instructions - from organization, should be ConsigneeNoteFCL note by fallback to Message Type", "OrgImporter Note - Brokerage/Import/FCL", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, should be ConsigneeNoteFCL note by fallback to Message Type", "OrgImporter Note - Brokerage/Import/FCL", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			AssertEquals("Import Handling Instructions - from organization, Sea Transport, LCL, should be blank as specific container mode entered", "", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, FCL, should find note for this context", "OrgImporter Note - Brokerage/Import/FCL", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("Import Handling Instructions - from organization, Air Transport, should be blank - no specific container type", "", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			StmNote consigneeNoteLCL = orgConsignee.Notes.AddNew();
			consigneeNoteLCL.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNoteLCL.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consigneeNoteLCL.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.L);
			consigneeNoteLCL.ST_NoteDataAsText = "OrgImporter Note - Brokerage/Import/LCL";

			StmNote consigneeNoteSEA = orgConsignee.Notes.AddNew();
			consigneeNoteSEA.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNoteSEA.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consigneeNoteSEA.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.S);
			consigneeNoteSEA.ST_NoteDataAsText = "OrgImporter Note - Brokerage/Import/SEA";

			StmNote consigneeNoteAIR = orgConsignee.Notes.AddNew();
			consigneeNoteAIR.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNoteAIR.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consigneeNoteAIR.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);
			consigneeNoteAIR.ST_NoteDataAsText = "OrgImporter Note - Brokerage/Import/AIR";

			StmNote consignorNoteEXP = orgConsignor.Notes.AddNew();
			consignorNoteEXP.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNoteEXP.ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignorNoteEXP.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			consignorNoteEXP.ST_NoteDataAsText = "OrgExporter Note - Brokerage/Export/All";

			Declaration.CusContainers.RemoveAndDeleteAll();
			Declaration.JE_TransportMode = ZString.Empty;
			Declaration.JE_ContainerMode = ZString.Empty;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Export Handling Instructions - from organization, should find ConsignorNoteEXP", "OrgExporter Note - Brokerage/Export/All", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals("Import Handling Instructions - from organization, should find all organization Import notes", "OrgImporter Note - Brokerage/Import/FCL\nOrgImporter Note - Brokerage/Import/LCL\nOrgImporter Note - Brokerage/Import/SEA\nOrgImporter Note - Brokerage/Import/AIR", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, should find ConsigneeNoteSEA", "OrgImporter Note - Brokerage/Import/SEA", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, LCL, should find notes for ConsigneeNoteSEA & ConsigneeNoteLCL", "OrgImporter Note - Brokerage/Import/SEA\nOrgImporter Note - Brokerage/Import/LCL", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, FCL, should find note for ConsigneeNoteSEA & ConsigneeNoteFCL", "OrgImporter Note - Brokerage/Import/SEA\nOrgImporter Note - Brokerage/Import/FCL", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, should find ConsigneeNoteAIR", "OrgImporter Note - Brokerage/Import/AIR", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			StmNote consigneeNoteALL = orgConsignee.Notes.AddNew();
			consigneeNoteALL.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNoteALL.ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
			consigneeNoteALL.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			consigneeNoteALL.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			consigneeNoteALL.ST_NoteDataAsText = "Consignee Note - All modules and Freight Modes";

			StmNote consignorNoteALL = orgConsignor.Notes.AddNew();
			consignorNoteALL.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNoteALL.ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
			consignorNoteALL.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			consignorNoteALL.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			consignorNoteALL.ST_NoteDataAsText = "Consignor Note - All modules and Freight Modes";

			Declaration.CusContainers.RemoveAndDeleteAll();
			Declaration.JE_TransportMode = ZString.Empty;
			Declaration.JE_ContainerMode = ZString.Empty;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Export Handling Instructions - from organization, should find ConsignorNoteEXP & ConsignorNoteALL", "OrgExporter Note - Brokerage/Export/All\nConsignor Note - All modules and Freight Modes", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals("Import Handling Instructions - from organization, should find all organization Import notes & now ConsigneeNoteALL", "OrgImporter Note - Brokerage/Import/FCL\nOrgImporter Note - Brokerage/Import/LCL\nOrgImporter Note - Brokerage/Import/SEA\nOrgImporter Note - Brokerage/Import/AIR\nConsignee Note - All modules and Freight Modes", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, should find ConsigneeNoteSEA & ConsigneeNoteALL", "OrgImporter Note - Brokerage/Import/SEA\nConsignee Note - All modules and Freight Modes", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, LCL, should find notes for ConsigneeNoteSEA & ConsigneeNoteLCL & ConsigneeNoteALL", "OrgImporter Note - Brokerage/Import/SEA\nConsignee Note - All modules and Freight Modes\nOrgImporter Note - Brokerage/Import/LCL", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, FCL, should find note for ConsigneeNoteSEA & ConsigneeNoteFCL & ConsigneeNoteALL", "OrgImporter Note - Brokerage/Import/SEA\nConsignee Note - All modules and Freight Modes\nOrgImporter Note - Brokerage/Import/FCL", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("Import Handling Instructions - from organization, Sea Transport, should find ConsigneeNoteAIR & ConsigneeNoteALL", "OrgImporter Note - Brokerage/Import/AIR\nConsignee Note - All modules and Freight Modes", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			// If notes are entered on the declaration job itself, then ONLY those notes will be returned.
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instruction notes entered on Job Declaration\nMore Notes here.", Factory);

			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Export Handling Instructions - declaration instructions only.", "Handling instruction notes entered on Job Declaration\nMore Notes here.", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Import Handling Instructions - declaration instructions only.", "Handling instruction notes entered on Job Declaration\nMore Notes here.", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);
		}

		public void TestSpecialInstructions()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Please take extra care\nWhen handling the goods.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Special instructions are so special.", Factory);

			AssertEquals("Special instructions", "Special instructions are so special.", DeclarationWrapper.SpecialInstructions);
		}

		public void TestDeclarationOrOrgSpecialInstruction()
		{
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeNote = orgConsignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			consigneeNote.ST_ParentID = orgConsignee.PK;
			consigneeNote.ST_Table = orgConsignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgImporter Special Instructions Note";

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorNote = orgConsignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			consignorNote.ST_ParentID = orgConsignor.PK;
			consignorNote.ST_Table = orgConsignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgSuplier Special Instructions Note";

			var orgConsigneeDelivery = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeDeliveryNote = orgConsigneeDelivery.Notes.AddNew();
			consigneeDeliveryNote.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			consigneeDeliveryNote.ST_ParentID = orgConsigneeDelivery.PK;
			consigneeDeliveryNote.ST_Table = orgConsigneeDelivery.TableName;
			consigneeDeliveryNote.ST_NoteDataAsText = "OrgImporterDelivery Special Instructions Note";

			var orgConsignorPickup = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorPickupNote = orgConsignorPickup.Notes.AddNew();
			consignorPickupNote.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			consignorPickupNote.ST_ParentID = orgConsignorPickup.PK;
			consignorPickupNote.ST_Table = orgConsignorPickup.TableName;
			consignorPickupNote.ST_NoteDataAsText = "OrgSuplierPickup Special Instructions Note";

			Declaration.JE_OH_Importer = orgConsignee.PK;
			Declaration.JE_OH_Supplier = orgConsignor.PK;
			Declaration.SupplierPickupAddress.OrganisationPK = orgConsignorPickup.PK;
			Declaration.ImporterDeliveryAddress.OrganisationPK = orgConsigneeDelivery.PK;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Special Instructions - from organizations", "OrgImporter Special Instructions Note\nOrgSuplier Special Instructions Note", DeclarationWrapper.DeclarationOrOrgSpecialInstruction);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Special Instructions - from organizations", "OrgImporter Special Instructions Note\nOrgSuplier Special Instructions Note", DeclarationWrapper.DeclarationOrOrgSpecialInstruction);

			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Additional Special instruction notes\nMore Notes here.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "This is another note and should not be included.", Factory);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Special Instructions - declaration instructions only.", "Additional Special instruction notes\nMore Notes here.", DeclarationWrapper.DeclarationOrOrgSpecialInstruction);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Special Instructions - declaration instructions only.", "Additional Special instruction notes\nMore Notes here.", DeclarationWrapper.DeclarationOrOrgSpecialInstruction);
		}

		#endregion

		#region Collections

		public void TestJobCharges()
		{
			JobHeader jobHeaderBisObj = CreateJobHeader(Declaration);

			AccChargeCode dSBChargeCode = CreateChargeCode(Factory, "DSBChg");
			dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeCode mRGChargeCode = CreateChargeCode(Factory, "MRGChg");
			mRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AccChargeCode rEVChargeCode = CreateChargeCode(Factory, "REVChg");
			rEVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			JobCharge lineCharge1 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 10.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge2 = CreateLineCharge(Factory, jobHeaderBisObj, mRGChargeCode.PK, 30.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge3 = CreateLineCharge(Factory, jobHeaderBisObj, rEVChargeCode.PK, 20.000M, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName));
			JobCharge lineCharge4 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 20.000M, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName));
			JobCharge lineCharge5 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 20.000M, jobHeaderBisObj.LocalChargesPK);

			Factory.Save();
			AssertEquals(3, DeclarationWrapper.JobHeader.JobChargesForLocalClient.Count);
			AssertEquals(10.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient[0].LocalSellAmount);
			AssertEquals(30.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient[1].LocalSellAmount);
			AssertEquals(20.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient[2].LocalSellAmount);
		}

		public void TestNonZeroJobCharges()
		{
			JobHeader jobHeaderBisObj = CreateJobHeader(Declaration);

			AccChargeCode dSBChargeCode = CreateChargeCode(Factory, "DSBChg");
			dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeCode mRGChargeCode = CreateChargeCode(Factory, "MRGChg");
			mRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AccChargeCode rEVChargeCode = CreateChargeCode(Factory, "REVChg");
			rEVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			JobCharge lineCharge1 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 10.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge2 = CreateLineCharge(Factory, jobHeaderBisObj, mRGChargeCode.PK, 50.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge3 = CreateLineCharge(Factory, jobHeaderBisObj, rEVChargeCode.PK, 0.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge4 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 20.000M, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName));
			JobCharge lineCharge5 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 20.000M, jobHeaderBisObj.LocalChargesPK);

			Factory.Save();
			AssertEquals(3, DeclarationWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges.Count);
			AssertEquals(10.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges[0].LocalSellAmount);
			AssertEquals(50.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges[1].LocalSellAmount);
			AssertEquals(20.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges[2].LocalSellAmount);
		}

		public void TestNullLocalChargeDoesntThrowException()
		{
			JobHeader jobHeaderBisObj = CreateJobHeader(Declaration);
			jobHeaderBisObj.LocalChargesPK = ZGuid.Empty;
			Factory.Save();
			AssertEquals(0, DeclarationWrapper.JobHeader.JobChargesForLocalClient.DisbursementTypeJobCharges.Count);
		}

		public void TestDisbursmentTypeJobCharges()
		{
			JobHeader jobHeaderBisObj = CreateJobHeader(Declaration);

			AccChargeCode dSBChargeCode = CreateChargeCode(Factory, "DSBChg");
			dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeCode mRGChargeCode = CreateChargeCode(Factory, "MRGChg");
			mRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AccChargeCode rEVChargeCode = CreateChargeCode(Factory, "REVChg");
			rEVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			JobCharge lineCharge1 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 10.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge2 = CreateLineCharge(Factory, jobHeaderBisObj, mRGChargeCode.PK, 30.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge3 = CreateLineCharge(Factory, jobHeaderBisObj, rEVChargeCode.PK, 20.000M, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName));
			JobCharge lineCharge4 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 20.000M, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName));
			JobCharge lineCharge5 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 20.000M, jobHeaderBisObj.LocalChargesPK);

			Factory.Save();
			AssertEquals(2, DeclarationWrapper.JobHeader.JobChargesForLocalClient.DisbursementTypeJobCharges.Count);
			AssertEquals(10.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient.DisbursementTypeJobCharges[0].LocalSellAmount);
			AssertEquals(20.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient.DisbursementTypeJobCharges[1].LocalSellAmount);
		}

		public void TestNonZeroDSBJobCharges()
		{
			JobHeader jobHeaderBisObj = CreateJobHeader(Declaration);

			AccChargeCode dSBChargeCode = CreateChargeCode(Factory, "DSBChg");
			dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeCode mRGChargeCode = CreateChargeCode(Factory, "MRGChg");
			mRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AccChargeCode rEVChargeCode = CreateChargeCode(Factory, "REVChg");
			rEVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			JobCharge lineCharge1 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 10.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge2 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 0.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge3 = CreateLineCharge(Factory, jobHeaderBisObj, rEVChargeCode.PK, 60.000M, jobHeaderBisObj.LocalChargesPK);
			JobCharge lineCharge4 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 20.000M, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName));
			JobCharge lineCharge5 = CreateLineCharge(Factory, jobHeaderBisObj, dSBChargeCode.PK, 40.000M, jobHeaderBisObj.LocalChargesPK);

			Factory.Save();
			AssertEquals(2, DeclarationWrapper.JobHeader.JobChargesForLocalClient.NonZeroDisbursementJobCharges.Count);
			AssertEquals(10.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient.NonZeroDisbursementJobCharges[0].LocalSellAmount);
			AssertEquals(40.000M, DeclarationWrapper.JobHeader.JobChargesForLocalClient.NonZeroDisbursementJobCharges[1].LocalSellAmount);
		}

		public void TestLCLAddressesWithWareHousing()
		{
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("AddressesWithWareHousing should be", 0, DeclarationWrapper.AddressesWithWareHousing.Count);

			var pickUpAddress = Factory.NewWithValidTestData<OrgAddress>();
			var deliverToAddress = Factory.NewWithValidTestData<OrgAddress>();

			Declaration.SupplierPickupAddress.E2_OA_Address = pickUpAddress.PK;
			Declaration.DepotDocAddress.E2_OA_Address = deliverToAddress.PK;

			AssertEquals("AddressesWithWareHousing should be", 0, DeclarationWrapper.AddressesWithWareHousing.Count);

			pickUpAddress.OA_ForkLift = ZBool.True;
			deliverToAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("AddressesWithWareHousing should have 2", 2, DeclarationWrapper.AddressesWithWareHousing.Count);

			Hashtable hashtable = new Hashtable();

			hashtable.Add(pickUpAddress.PK, pickUpAddress);
			hashtable.Add(deliverToAddress.PK, deliverToAddress);
			foreach (DocDocAddress dAddress in DeclarationWrapper.AddressesWithWareHousing)
			{
				Assert("is in col", hashtable.ContainsKey(((JobDocAddress)dAddress.WrappedObject).Address.PK));
			}

			Declaration.SupplierPickupAddress.E2_OA_Address = deliverToAddress.PK;
			AssertEquals("AddressesWithWareHousing both the same, should be 1", 1, DeclarationWrapper.AddressesWithWareHousing.Count);
		}

		public void TestFCLAddressesWithWareHousing()
		{
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			var cusContainer = Declaration.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("AddressesWithWareHousing should be", 0, DeclarationWrapper.AddressesWithWareHousing.Count);

			var j1PickUpAddress = Factory.NewWithValidTestData<OrgAddress>();
			var j1J2ExporterAddress = Factory.NewWithValidTestData<OrgAddress>();

			var j2DeliverToAddress = Factory.NewWithValidTestData<OrgAddress>();

			Declaration.ContainerYardDocAddress.E2_OA_Address = j1PickUpAddress.PK;
			Declaration.SupplierPickupAddress.E2_OA_Address = j1J2ExporterAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = j2DeliverToAddress.PK;

			AssertEquals("AddressesWithWareHousing should be", 0, DeclarationWrapper.AddressesWithWareHousing.Count);

			j1PickUpAddress.OA_ForkLift = ZBool.True;
			j1J2ExporterAddress.OA_DockLeveler = ZBool.True;
			j2DeliverToAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("AddressesWithWareHousing should have 3, 1 is repeated", 3, DeclarationWrapper.AddressesWithWareHousing.Count);

			Hashtable hashtable = new Hashtable();

			hashtable.Add(j1PickUpAddress.PK, j1PickUpAddress);
			hashtable.Add(j1J2ExporterAddress.PK, j1J2ExporterAddress);
			hashtable.Add(j2DeliverToAddress.PK, j2DeliverToAddress);
			foreach (DocDocAddress dAddress in DeclarationWrapper.AddressesWithWareHousing)
			{
				Assert("is in col", hashtable.ContainsKey(((JobDocAddress)dAddress.WrappedObject).Address.PK));
			}
		}
		#endregion

		#region IDocJobDetail Tests

		protected IDocJobDetail DeclarationAsJobDocDetail
		{
			get { return DeclarationWrapper; }
		}

		public void TestOrderNumbersForInvoice()
		{
			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			Declaration.JE_OwnerRef = "Owner's Ref 123";

			AssertEquals("Order Numbers for Invoice", DeclarationWrapper.OrderNumbers, DeclarationAsJobDocDetail.OrderNumbersForInvoice);
		}

		public void TestOurReference()
		{
			Declaration.JE_DeclarationReference = "B12345678";
			AssertEquals(DeclarationWrapper.DeclarationReference, DeclarationAsJobDocDetail.OurReference);
		}

		public void TestSupplierAsString()
		{
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Declaration.JE_OH_Supplier = organisation.PK;
			AssertEquals("Supplier For Invoice", organisation.OH_FullName, DeclarationAsJobDocDetail.SupplierAsString);
		}

		public void TestVesselAndVoyage()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VoyageFlightNo = "Voyage";

			AssertEquals("Vessel & Voyage for Invoice", DeclarationWrapper.Transport, DeclarationAsJobDocDetail.VesselAndVoyage);
		}

		public void TestMasterBillNumber()
		{
			Declaration.JE_MasterBill = "B12345678";
			AssertEquals("MasterBill Number for Invoice", DeclarationWrapper.MasterBillNum, DeclarationAsJobDocDetail.MasterBillNumber);
		}

		public void TestETAPortName()
		{
			AssertEquals("ETA Port Name for Invoice", "", DeclarationAsJobDocDetail.ETAPortName);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Declaration.JE_RL_NKFinalDestination = uNLOCO.RL_Code;
			AssertEquals("ETA Port Name for Invoice", DeclarationWrapper.FinalDestination.PortName, DeclarationAsJobDocDetail.ETAPortName);
		}

		public void TestETDPortName()
		{
			AssertEquals("ETD Port Name for Invoice", "", DeclarationAsJobDocDetail.ETDPortName);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Declaration.JE_RL_NKOrigin = uNLOCO.RL_Code;
			AssertEquals("ETD Port Name for Invoice", DeclarationWrapper.Origin.PortName, DeclarationAsJobDocDetail.ETDPortName);
		}

		public void TestService()
		{
			Declaration.JE_ContainerMode = "CCC";
			AssertEquals("ETD Port Name for Invoice", DeclarationWrapper.ContainerMode, DeclarationAsJobDocDetail.Service);
		}

		public void TestPackageQuantity()
		{
			Declaration.JE_TotalNoOfPacks = 12;
			AssertEquals("Package Quantity for Invoice", DeclarationWrapper.TotalNoOfPacks.ToString(), DeclarationAsJobDocDetail.PackageQuantity);
		}

		public void TestPackageType()
		{
			Declaration.JE_TotalNoOfPacksPackType = "PLT";
			AssertEquals("Package Type for Invoice", DeclarationWrapper.PackType, DeclarationAsJobDocDetail.PackageType);
		}

		public void TestConsolDepot()
		{
			AssertEquals(ZString.Empty, DeclarationAsJobDocDetail.ConsolDepot);
		}

		public void TestWeightAsString()
		{
			Declaration.JE_TotalWeight = 1234;
			Declaration.JE_TotalWeightUnit = "WG";
			ZString resultShouldBe = DeclarationWrapper.Weight + " " + DeclarationWrapper.WeightUnit;
			AssertEquals("Weight as String for Invoice", resultShouldBe, DeclarationAsJobDocDetail.WeightAsString);
		}

		public void TestVolumeAsString()
		{
			Declaration.JE_TotalVolume = 1234;
			Declaration.JE_TotalVolumeUnit = "WG";
			ZString resultShouldBe = DeclarationWrapper.Volume + " " + DeclarationWrapper.VolumeUnit;
			AssertEquals("Volume as String for Invoice", resultShouldBe, DeclarationAsJobDocDetail.VolumeAsString);
		}

		public void TestUnitOfWeight()
		{
			Declaration.JE_TotalWeightUnit = "KG";
			AssertEquals("Weight should be in KGs", "KG", DeclarationWrapper.UnitOfWeight);
		}

		public void TestUnitOfVolume()
		{
			Declaration.JE_TotalVolumeUnit = "M3";
			AssertEquals("Volume should be in CBMs", "M3", DeclarationWrapper.UnitOfVolume);
		}

		public void TestNote()
		{
			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			Declaration.JE_OwnerRef = "Owner's Ref 123";
			AssertEquals("Volume as String for Invoice", "Order Numbers: " + DeclarationWrapper.OrderNumbers, DeclarationAsJobDocDetail.Note);
		}

		public void TestConsignorAsString()
		{
			Declaration.JE_OH_Supplier = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertEquals("Consignor as String for Invoice", DeclarationWrapper.Consignor.Name, DeclarationAsJobDocDetail.ConsignorAsString);
		}

		public void TestConsigneeAsString()
		{
			Declaration.JE_OH_Importer = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertEquals("Consignee as String for Invoice", DeclarationWrapper.Consignee.Name, DeclarationAsJobDocDetail.ConsigneeAsString);
		}

		public void TestShortContainerAndSealNumbers()
		{
			AssertEquals("Short Container & Seal numbers for Invoice", ZString.Empty, DeclarationAsJobDocDetail.ShortContainerAndSealNumbersForInvoice);

			BaseCusContainer container = Declaration.CusContainers.AddNew();
			container.CO_Seal = "1233";
			container.CO_ContainerNumber = "CONTAINER1";
			container.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertEquals("Short Container & Seal numbers for Invoice", "CONTAINER1 / 1233 / 20GP", DeclarationAsJobDocDetail.ShortContainerAndSealNumbersForInvoice);
		}

		public void TestLongContainerAndSealNumbers()
		{
			AssertEquals("Short Container & Seal numbers for Invoice", ZString.Empty, DeclarationAsJobDocDetail.ShortContainerAndSealNumbersForInvoice);

			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_Seal = "1233";
			container1.CO_ContainerNumber = "CONTAINER1";
			container1.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			BaseCusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_Seal = "1234";
			container2.CO_ContainerNumber = "CONTAINER2";
			container2.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			BaseCusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_Seal = "1235";
			container3.CO_ContainerNumber = "CONTAINER3";
			container3.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			ZString resultShouldBe = "- CONTAINER1      - 1233                 - 20GP           \n";
			resultShouldBe += "- CONTAINER2      - 1234                 - 40GP           \n";
			resultShouldBe += "- CONTAINER3      - 1235                 - 20GP           \n";
			AssertEquals("Long Container & Seal Numbers for Invoice", resultShouldBe, DeclarationAsJobDocDetail.LongContainerAndSealNumbersForInvoice);
		}

		public void TestNumberOfContainers()
		{
			AssertEquals("Number of Containers", 0, DeclarationAsJobDocDetail.NumberOfContainers);

			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_Seal = "1233";
			container1.CO_ContainerNumber = "CONTAINER1";
			container1.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			BaseCusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_Seal = "1234";
			container2.CO_ContainerNumber = "CONTAINER2";
			container2.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			AssertEquals("Number of Containers", 2, DeclarationAsJobDocDetail.NumberOfContainers);
		}

		public void TestMarksAndNumbersForInvoice()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is other notes and should not be included.", Factory);
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Marks and numbers Line One\nLine Two", Factory);

			AssertEquals("Marks & Numbers for Invoice", DeclarationWrapper.MarksAndNumbers, DeclarationAsJobDocDetail.MarksAndNumbersForInvoice);
		}

		public void TestFirstLineOfMarksAndNumbers()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Marks and numbers Line One\nLine Two\nLine Three", Factory);

			AssertEquals("Marks and numbers Line One", DeclarationWrapper.FirstLineOfMarksAndNumbers);
		}

		public void TestShortGoodsDescription()
		{
			Declaration.JE_GoodsDescription = "GoodsDescription";
			AssertEquals("Short Goods DEscription for Invoice", DeclarationWrapper.GoodsDescription, DeclarationAsJobDocDetail.ShortGoodsDescriptionForInvoice);
		}

		public void TestLongGoodsDescription()
		{
			Declaration.JE_GoodsDescription = "GoodsDescription";
			AssertEquals("Long Goods DEscription for Invoice", DeclarationWrapper.GoodsDescription, DeclarationAsJobDocDetail.LongGoodsDescriptionForInvoice);
		}

		public void TestETADateOnDocJobDetail()
		{
			ZDateTime dateOfArrival = new ZDateTime(2004, 04, 04);
			Declaration.JE_DateOfArrival = dateOfArrival;
			AssertEquals("ETA Date for Invoice", DeclarationWrapper.DateOfArrival, DeclarationAsJobDocDetail.ETADate);
		}

		public void TestETDDateOnDocJobDetail()
		{
			ZDateTime exportDate = new ZDateTime(2004, 04, 04);
			Declaration.JE_ExportDate = exportDate;
			AssertEquals("ETD Date for Invoice", DeclarationWrapper.ExportDate, DeclarationAsJobDocDetail.ETDDate);
		}

		public void TestTransportModeIsAir()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("Transport Mode is Air", ZBool.True, DeclarationAsJobDocDetail.TransportModeIsAir);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Transport Mode is Air", ZBool.False, DeclarationAsJobDocDetail.TransportModeIsAir);
		}

		public void TestTransportModeIsSea()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("Transport Mode is Sea", ZBool.True, DeclarationAsJobDocDetail.TransportModeIsSea);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Transport Mode is Sea", ZBool.False, DeclarationAsJobDocDetail.TransportModeIsSea);
		}

		#endregion

		#region IShipperDepartureNotice Members

		public virtual void TestShipperDepartureNoticeDocumentHeader()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "Air Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "Sea Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "FCL Sea Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "LCL Sea Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);
		}

		public virtual void TransportModeAndPackingMode()
		{
			ZString result = "Transport Mode " + DeclarationWrapper.TransportMode + " " + "Container Mode " + DeclarationWrapper.PackingMode;
			Assert(" Declaration TransportMode is empty ", DeclarationWrapper.TransportModeAndPackingMode.IsEmpty);
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("TransportModeAndPackingMode", result, DeclarationWrapper.TransportModeAndPackingMode);
		}

		public virtual void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("TransportModeDescription", "Air Freight", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("TransportModeDescription", "Sea Freight", DeclarationWrapper.TransportModeDescription);
		}

		public virtual void TestHeadingTransportModeWithPackingMode()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_ContainerMode = Declaration.TransportModeAirCodeForTesting;

			ZString expected = "Sea";
			AssertEquals("No packing mode in heading", expected, DeclarationWrapper.HeadingTransportMode);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			expected = Declaration.JE_ContainerMode + " Sea";
			AssertEquals("Packing mode in heading", expected.Trim(), DeclarationWrapper.HeadingTransportMode);
		}

		public virtual void TestHeadingTransportMode()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("Air", DeclarationWrapper.HeadingTransportMode);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("Sea", DeclarationWrapper.HeadingTransportMode);
		}

		public void TestPackingMode()
		{
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals(Core.Constants.ContainerModes.FCL, DeclarationWrapper.PackingMode);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.LCL, DeclarationWrapper.PackingMode);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals(Declaration.JE_ContainerMode, DeclarationWrapper.PackingMode);

			container.Delete();
			Declaration.JE_ContainerMode = "";
			AssertEquals(Declaration.JE_ContainerMode, DeclarationWrapper.PackingMode);
		}

		public void TestReceivingForwarder()
		{
			OrgHeader importerBizo = CreateTestOrgHeader(Factory, "IMPXXX", "Importer Org", "123 Street", "ABC Lane", "Sydney");
			importerBizo.Contacts.RemoveAndDeleteAll();

			Declaration.JE_OH_Importer = importerBizo.PK;
			OrgContact cNRContact = importerBizo.Contacts.AddNew();
			cNRContact.OC_ContactName = "Suzie";
			cNRContact.Documents.AddNew();
			cNRContact.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact nOTContact = importerBizo.Contacts.AddNew();
			nOTContact.OC_ContactName = "Lizzie";
			nOTContact.Documents.AddNew();
			nOTContact.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			Factory.Save();
			ZString importerAddress = DocOrganisation.New(importerBizo, Factory).PostalAddress;

			AssertNotNull("Receiving Forwarder", DeclarationWrapper.ReceivingForwarder);
		}

		public void TestShippersDeliveryAgent()
		{
			AssertNull("ShippersDeliveryAgent", DeclarationWrapper.ShippersDeliveryAgent);

			var header = Factory.New<OrgHeader>();
			OrgAddress address = header.Addresses.AddNew();
			Declaration.DepotDocAddress.E2_OA_Address = address.PK;
			AssertNotNull("ShippersDeliveryAgent", DeclarationWrapper.ShippersDeliveryAgent);
		}

		public void TestRedundantShipperDeptartureNoticeFields()
		{
			AssertEquals("AgentsBookingReference is empty", "", DeclarationWrapper.AgentsBookingReference);
			AssertEquals("DepartureReference is empty", "", DeclarationWrapper.DepartureReference);
			AssertEquals("NoOfOriginalBills", 0, DeclarationWrapper.NoOfOriginalBills);
			AssertEquals("NoOfCopyBills", 0, DeclarationWrapper.NoOfCopyBills);
		}

		#endregion

		#region ITimeSlotRequest Members

		public void TestConsolNumber()
		{
			Assert(DeclarationWrapper.ConsolNumber.IsEmpty);
		}

		public void TestBookingReference()
		{
			Assert(DeclarationWrapper.BookingReference.IsEmpty);
		}

		public void TestSimpleContainers()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ctr1";

			AssertEquals(1, DeclarationWrapper.SimpleContainers.Count);
			AssertEquals(container1.CO_ContainerNumber, DeclarationWrapper.SimpleContainers[0].ContainerNumber);
		}

		public void TestCutOffOrAvailableDate()
		{
			AssertNotNull("Poke JobDocsAndCartage", Declaration.DocsAndCartage);
			Declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2004, 1, 1);
			AssertEquals(DeclarationWrapper.AvailableDate, DeclarationWrapper.CutOffOrAvailableDate);
		}

		public void TestPickupOrStorageCommenceDate()
		{
			AssertNotNull("Poke JobDocsAndCartage", Declaration.DocsAndCartage);
			Declaration.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2004, 1, 1);
			AssertEquals(DeclarationWrapper.StorageCommenceDate, DeclarationWrapper.PickupOrStorageCommenceDate);
		}

		public void TestCartageStorageandAvailableDates()
		{
			Declaration.JE_DeliveryOrPickupRequiredBy = new ZDateTime(2011, 1, 1);
			Declaration.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2011, 1, 5);
			Declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2011, 1, 6);
			Declaration.JE_EstimatedDeliveryOrPickup = new ZDateTime(2011, 2, 1);
			Declaration.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2011, 2, 5);
			Declaration.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2011, 2, 6);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 1, 5), DeclarationWrapper.CartageAvailableDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 2, 5), DeclarationWrapper.CartageStorageCommenceDate);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 1, 6), DeclarationWrapper.CartageAvailableDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 2, 6), DeclarationWrapper.CartageStorageCommenceDate);
		}

		public void TestCartageCutOffandReceivalDates()
		{
			Transport transport1 = Declaration.Transports.AddNew();
			transport1.JW_ETD = new ZDateTime(2011, 2, 1);
			transport1.JW_ETA = new ZDateTime(2011, 2, 15);
			transport1.JW_TerminalCutOff = new ZDateTime(2011, 1, 18);
			transport1.JW_DepotCutOff = new ZDateTime(2011, 1, 20);
			transport1.JW_TerminalReceivalCommences = new ZDateTime(2011, 1, 22);
			transport1.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 25);

			Transport transport2 = Declaration.Transports.AddNew();
			transport2.JW_ETD = new ZDateTime(2012, 1, 1);
			transport2.JW_ETA = new ZDateTime(2012, 1, 15);
			transport2.JW_TerminalCutOff = new ZDateTime(2012, 1, 30);
			transport2.JW_DepotCutOff = new ZDateTime(2012, 1, 30);
			transport2.JW_TerminalReceivalCommences = new ZDateTime(2012, 1, 22);
			transport2.JW_DepotReceivalCommences = new ZDateTime(2012, 1, 25);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), DeclarationWrapper.CartageReceivalDate);
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), DeclarationWrapper.CartageCutOffDate);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 25), DeclarationWrapper.CartageReceivalDate);
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 20), DeclarationWrapper.CartageCutOffDate);
		}

		public void TestETAString()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_DateAtFinalDestination = new ZDateTime(2005, 2, 5);
			AssertEquals(DeclarationWrapper.ETA.ToShortDateString(), DeclarationWrapper.ETAString);
		}

		public void TestETDString()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_DateAtOrigin = new ZDateTime(2005, 2, 5);
			AssertEquals(DeclarationWrapper.ETD.ToShortDateString(), DeclarationWrapper.ETDString);
		}

		public void TestBookingETA()
		{
			Assert(DeclarationWrapper.BookingETA.IsEmpty);
		}

		public void TestBookingETD()
		{
			Assert(DeclarationWrapper.BookingETD.IsEmpty);
		}

		public void TestFullCartageInstructions()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Cartage instruction notes\nMore Notes here.", Factory);
			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorNote = orgConsignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			consignorNote.ST_ParentID = orgConsignor.PK;
			consignorNote.ST_Table = orgConsignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgSuplier Note";
			Declaration.JE_OH_Supplier = orgConsignor.PK;
			Declaration.JE_MessageType = "EXP";

			AssertEquals(DeclarationWrapper.DeclarationOrOrgCartageInstructions, DeclarationWrapper.FullCartageInstructions);
		}

		public void TestFullHandlingInstructionsExport()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instruction notes on dec\nMore Notes here.", Factory);
			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorNote = orgConsignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNote.ST_ParentID = orgConsignor.PK;
			consignorNote.ST_Table = orgConsignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgSuplier Note";
			Declaration.JE_OH_Supplier = orgConsignor.PK;
			Declaration.JE_MessageType = "EXP";

			AssertEquals(DeclarationWrapper.DeclarationOrOrgHandlingInstruction, DeclarationWrapper.FullHandlingInstructions);
		}

		public void TestFullHandlingInstructionsImport()
		{
			CreateNote_ConsideringShipment(Declaration, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instruction notes on dec\nMore Notes here.", Factory);

			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeNote = orgConsignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = orgConsignee.PK;
			consigneeNote.ST_Table = orgConsignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgImporter Note";
			Declaration.JE_OH_Importer = orgConsignee.PK;
			Declaration.JE_MessageType = "IMP";

			AssertEquals(DeclarationWrapper.DeclarationOrOrgHandlingInstruction, DeclarationWrapper.FullHandlingInstructions);

			consigneeNote.ST_NoteDataAsText = "Handling instruction notes on dec";
			AssertEquals("Should not see the dec's note twice, even though it's defined on both dec and importer", "Handling instruction notes on dec\nMore Notes here.", DeclarationWrapper.DeclarationOrOrgHandlingInstruction);
		}

		#endregion

		#region Summary Commercial Invoice Test
		public virtual void TestInvoiceNumber()
		{
			AssertEquals("", DeclarationWrapper.InvoiceNumber);
			GroupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			GroupHeader.JobComInvoiceHeaders.DeleteAll();
			InvoiceHeader = GroupHeader.JobComInvoiceHeaders.AddNew();

			InvoiceHeader.JZ_InvoiceNumber = "TEST123";
			AssertEquals("TEST123", DeclarationWrapper.InvoiceNumber);

			InvoiceHeader2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			InvoiceHeader2.JZ_InvoiceNumber = "TEST321";
			AssertEquals("AS BELOW", DeclarationWrapper.InvoiceNumber);
		}
		public virtual void TestEarliestInvoiceDate()
		{
			CreateGroupHeaderAndTwoInvoiceHeader();
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
			InvoiceHeader2.JZ_InvoiceDate = ZDateTime.Today.AddDays(-2);

			AssertEquals("Get the earliest invoice date", ZDateTime.Today.AddDays(-2), DeclarationWrapper.EarliestInvoiceDate);
		}

		public virtual void TestCountryOfOrigin()
		{
			CreateGroupHeaderAndTwoInvoiceHeader();
			BaseJobComInvoiceLine line1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_CountryOfOrigin = "AU";
			BaseJobComInvoiceLine line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_CountryOfOrigin = "AU";

			BaseJobComInvoiceLine line3 = InvoiceHeader2.JobComInvoiceLines.AddNew();
			line3.JI_CountryOfOrigin = "AU";
			AssertEquals("Country AU", "AUSTRALIA", DeclarationWrapper.CountryOfOrigin.ToUpper());

			line2.JI_CountryOfOrigin = "SG";
			AssertEquals("Country VARIOUS", "VARIOUS", DeclarationWrapper.CountryOfOrigin.ToUpper());

			line1.JI_CountryOfOrigin = "SG";
			line3.JI_CountryOfOrigin = "SG";
			AssertEquals("Country Singapore", "SINGAPORE", DeclarationWrapper.CountryOfOrigin.ToUpper());
		}

		public virtual void TestTotalInvoiceAmount()
		{
			Declaration.JE_ExportDate = ZDateTime.Today;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, ForeignCurr, Declaration.IsReciprocalRates ? 0.192308m : 5.2M);

			CreateGroupHeaderAndTwoInvoiceHeader();
			InvoiceHeader.JZ_InvoiceAmount = 2000M;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			InvoiceHeader2.JZ_InvoiceAmount = 300M;
			InvoiceHeader2.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			AssertEquals("Total of Invoice amount", 2300M, DeclarationWrapper.TotalInvoiceAmount);
			AssertEquals("Total Invoice currency", GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, DeclarationWrapper.TotalInvoiceCurrency.Code);

			InvoiceHeader2.JZ_RX_NKInvoice_Currency = ForeignCurr.RX_Code;
			InvoiceHeader2.JZ_InvoiceCurrExRate = 5.2M;
			AssertEquals("Total of Invoice amount", new ZDecimal(2000M + (300M / 5.2M)).ToString(2), DeclarationWrapper.TotalInvoiceAmount.ToString(2));
			AssertEquals("Total Invoice currency", GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, DeclarationWrapper.TotalInvoiceCurrency.Code);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = ForeignCurr.RX_Code;
			AssertEquals("Total of Invoice amount", 2300M, DeclarationWrapper.TotalInvoiceAmount);
			AssertEquals("Total Invoice currency", ForeignCurr.RX_Code, DeclarationWrapper.TotalInvoiceCurrency.Code);
		}

		public void TestTotalOverseasFreightIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.OverseasFreight, "TotalIncludedOverseasFreight", "TotalExcludedOverseasFreight");
		}

		public void TestTotalConvertedOverseasFreight()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.OverseasFreight, "TotalIncludedOverseasFreight", "TotalExcludedOverseasFreight");
		}

		public void TestTotalOverseasInsuranceIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.OverseasInsurance, "TotalIncludedOverseasInsurance", "TotalExcludedOverseasInsurance");
		}

		public void TestTotalConvertedOverseasInsurance()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.OverseasInsurance, "TotalIncludedOverseasInsurance", "TotalExcludedOverseasInsurance");
		}

		public void TestTotalExWorksIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.ExWorks, "TotalIncludedExWorks", "TotalExcludedExWorks");
		}

		public void TestTotalConvertedExWorks()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.ExWorks, "TotalIncludedExWorks", "TotalExcludedExWorks");
		}
		public void TestTotalForeignInlandFreightIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, "TotalIncludedForeignInlandFreight", "TotalExcludedForeignInlandFreight");
		}

		public void TestTotalConvertedInlandFreight()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, "TotalIncludedForeignInlandFreight", "TotalExcludedForeignInlandFreight");
		}

		public void TestTotalPackingCostsIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.PackingCost, "TotalIncludedPackingCosts", "TotalExcludedPackingCosts");
		}

		public void TestTotalConvertedPackingCosts()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.PackingCost, "TotalIncludedPackingCosts", "TotalExcludedPackingCosts");
		}

		public void TestTotalLandingChargesIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.LandingCharges, "TotalIncludedLandingCharges", "TotalExcludedLandingCharges");
		}

		public void TestTotalConvertedLandingCharges()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.LandingCharges, "TotalIncludedLandingCharges", "TotalExcludedLandingCharges");
		}

		public void TestTotalOtherCharges1IncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.OtherCharges, "TotalIncludedOtherCharges1", "TotalExcludedOtherCharges1");
		}

		public void TestTotalConvertedOtherCharges1()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.OtherCharges, "TotalIncludedOtherCharges1", "TotalExcludedOtherCharges1");
		}

		public void TestTotalDiscountIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.Discount, "TotalIncludedDiscount", "TotalExcludedDiscount");
		}

		public void TestTotalConvertedDiscount()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.Discount, "TotalIncludedDiscount", "TotalExcludedDiscount");
		}

		public void TestTotalCommissionIncludedExcluded()
		{
			AssertTotalIncludedExcludedCharge(CustomsChargeTypeList.Codes.Commission, "TotalIncludedCommission", "TotalExcludedCommission");
		}

		public void TestTotalConvertedCommission()
		{
			AssertTotalConvertedForeignCurrCharge(CustomsChargeTypeList.Codes.Commission, "TotalIncludedCommission", "TotalExcludedCommission");
		}

		protected virtual void AssertTotalIncludedExcludedCharge(string chargeName, string wrapperPropertyNameForIncluded, string wrapperPropertyNameForExcluded)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var expected = 300m;
				if (Declaration.IncoTermAndChargeFactory.GetCharge(chargeName) == null)
				{
					expected = 0M;
				}

				CreateGroupHeaderAndTwoInvoiceHeaderForIncludedCase(chargeName, true);
				var actualAmountIncluded = new ZDecimal(DeclarationWrapper[wrapperPropertyNameForIncluded]);
				AssertEquals("Total included amount", expected, actualAmountIncluded);

				CreateGroupHeaderAndTwoInvoiceHeaderForExcludedCase(chargeName, true);
				var actualAmountExcluded = new ZDecimal(DeclarationWrapper[wrapperPropertyNameForExcluded]);
				AssertEquals("Total excluded amount", expected, actualAmountExcluded);
			}
		}

		protected virtual void AssertTotalConvertedForeignCurrCharge(string chargeName, string wrapperPropertyNameForIncluded, string wrapperPropertyNameForExcluded)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var expectedIncluded = new ZDecimal(200M + (100M / 0.8M));
				var expectedExcluded = 300M / 0.75M;
				if (Declaration.IncoTermAndChargeFactory.GetCharge(chargeName) == null)
				{
					expectedIncluded = expectedExcluded = 0M;
				}

				CreateGroupHeaderAndTwoInvoiceHeaderForIncludedCase(chargeName, false);
				var actualAmountIncluded = new ZDecimal(DeclarationWrapper[wrapperPropertyNameForIncluded]);
				AssertEquals(string.Format("Total included amount with converted {0} amount", ForeignCurr.RX_Code), expectedIncluded.ToString(2), actualAmountIncluded.ToString(2));

				CreateGroupHeaderAndTwoInvoiceHeaderForExcludedCase(chargeName, false);
				var actualAmountExcluded = new ZDecimal(DeclarationWrapper[wrapperPropertyNameForExcluded]);
				AssertEquals(string.Format("Total excluded amount with converted {0} amount", ForeignCurr.RX_Code), expectedExcluded, actualAmountExcluded);
			}
		}

		#endregion

		#region Landed Costing Exchange Rates Test

		public void TestLandedCostingExchangeRatesInCurrencyCodeOrder()
		{
			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = true;

			var declaration = Factory.New<T>();
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceHeader1.JZ_InvoiceCurrExRate = 0.9m;
			BaseJobComInvoiceHeader invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader2.JZ_InvoiceCurrExRate = 0.8m;

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			JobInvoicingExchangeRate jobInvoicingRate = job.ExchangeRates.AddNew();
			jobInvoicingRate.JF_RX_NKRateCurrency = "HKD";
			jobInvoicingRate.JF_BaseRate = 0.7m;

			DocBaseJobDeclaration doc = DocBaseJobDeclaration.New(declaration, Factory);
			if (doc != null)
			{
				AssertEquals("AUD", doc.LandedCostingExchangeRatesInCurrencyCodeOrder[0].CurrencyCode);
				AssertEquals(0.9m, doc.LandedCostingExchangeRatesInCurrencyCodeOrder[0].SellRate);
				AssertEquals("HKD", doc.LandedCostingExchangeRatesInCurrencyCodeOrder[1].CurrencyCode);
				AssertEquals(0.7m, doc.LandedCostingExchangeRatesInCurrencyCodeOrder[1].SellRate);
				AssertEquals("USD", doc.LandedCostingExchangeRatesInCurrencyCodeOrder[2].CurrencyCode);
				AssertEquals(0.8m, doc.LandedCostingExchangeRatesInCurrencyCodeOrder[2].SellRate);
			}
			Assert(true);
		}

		public void TestLandedCostingExchangeRatesInCurrencyCodeOrder_FollowsFallbackRegistryItem()
		{
			var declaration = Factory.New<T>();
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "HKD";
			invoiceHeader1.JZ_InvoiceCurrExRate = 0m;

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			JobInvoicingExchangeRate jobInvoicingRate = job.ExchangeRates.AddNew();
			jobInvoicingRate.JF_RX_NKRateCurrency = "HKD";
			jobInvoicingRate.JF_BaseRate = 0.4m;

			DocBaseJobDeclaration doc = DocBaseJobDeclaration.New(declaration, Factory);
			if (doc != null)
			{
				Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = true;
				AssertEquals(0.4m, doc.LandedCostingExchangeRatesInCurrencyCodeOrder[0].SellRate);

				Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = false;
				AssertEquals(1m, doc.LandedCostingExchangeRatesInCurrencyCodeOrder[0].SellRate);
			}
			Assert(true);
		}

		#endregion

		#region Implementation Methods on DocBaseDeclaration

		public void TestContainersInternal()
		{
			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);

			AssertEquals("ContainersInternal is empty", 0, decTestClassWrapper.ContainersInternalTestMethod.Count);

			Declaration.CusContainers.AddNew();
			AssertEquals("ContainersInternal is not empty", 1, decTestClassWrapper.ContainersInternalTestMethod.Count);

			Declaration.CusContainers.AddNew();
			AssertEquals("ContainersInternal is not empty", 2, decTestClassWrapper.ContainersInternalTestMethod.Count);
		}

		public void TestInvoiceGroupHeadersInternal()
		{
			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			AssertEquals("InvoiceGroupHeadersInternal is not empty", 1, decTestClassWrapper.InvoiceGroupHeadersInternalTestMethod.Count);
		}

		public virtual void TestInvoiceLinesInternal()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			AssertEquals("InvoiceLinesInternal is not empty", 1, decTestClassWrapper.InvoiceLinesInternalTestMethod.Count);

			invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("InvoiceLinesInternal is not empty", 2, decTestClassWrapper.InvoiceLinesInternalTestMethod.Count);
		}

		public virtual void TestInvoiceLinesSortedByLineNoInternal()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			AssertEquals("InvoiceLinesSortedByLineNoInternalTestMethod is not empty", 1, decTestClassWrapper.InvoiceLinesSortedByLineNoInternalTestMethod.Count);

			invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("InvoiceLinesSortedByLineNoInternalTestMethod is not empty", 2, decTestClassWrapper.InvoiceLinesSortedByLineNoInternalTestMethod.Count);
		}

		public virtual void TestInvoiceLinesSortedByMergedLineNoInternal()
		{
			var groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			var decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			AssertEquals("InvoiceLinesSortedByMergedLineNoInternalTestMethod is not empty", 1, decTestClassWrapper.InvoiceLinesSortedByMergedLineNoInternalTestMethod.Count);

			invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("InvoiceLinesSortedByMergedLineNoInternalTestMethod is not empty", 2, decTestClassWrapper.InvoiceLinesSortedByMergedLineNoInternalTestMethod.Count);
		}

		public virtual void TestInvoiceLinesSortedByMergedNumericLineNoInternal()
		{
			var groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "Inv1";
			var invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "Inv2";

			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entry2 = Declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_Description = "A";

			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Description = "B";

			var entryLine3 = entry2.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 11;
			var invoiceLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Description = "C";

			var entryLine4 = entry1.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 1;
			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LineNo = 4;
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Description = "D";

			var entryLine5 = entry1.MergedLines.AddNew();
			entryLine5.CL_LineNumber = 1;
			var invoiceLine5 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_LineNo = 5;
			invoiceLine5.JI_CL = entryLine5.PK;
			invoiceLine5.JI_Description = "E";

			var decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			var lines = decTestClassWrapper.InvoiceLinesSortedByMergedNumericLineNoInternalTestMethod;
			AssertEquals("InvoiceLinesSortedByMergedNumericLineNoInternalTestMethod.Count", 5, lines.Count);
			AssertLine(lines[0], 1, invoiceLine.JI_Description);
			AssertLine(lines[1], 2, invoiceLine4.JI_Description);
			AssertLine(lines[2], 3, invoiceLine5.JI_Description);
			AssertLine(lines[3], 1, invoiceLine2.JI_Description);
			AssertLine(lines[4], 2, invoiceLine3.JI_Description);
		}

		protected static void AssertLine(DocBaseJobComInvoiceLine line, ZShort lineNum, ZString description)
		{
			AssertEquals("LineNo", lineNum, line.LineNo);
			AssertEquals("Description", description, line.Description);
		}

		public void TestInvoiceHeaderSorted()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			AssertNull("No invoice header defined yet", decTestClassWrapper.InvoiceHeaderInternalTestMethod);

			BaseJobComInvoiceHeader invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "HEADER2";
			invoiceHeader1.JZ_GroupInvoice = ZBool.False;

			BaseJobComInvoiceHeader invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "HEADER1";
			invoiceHeader2.JZ_GroupInvoice = ZBool.False;

			AssertNotNull("invoice header", decTestClassWrapper.InvoiceHeaderInternalTestMethod);
			AssertEquals("Invoice Header No. should be 'HEADER1' (sorted by invoice number)", "HEADER1", decTestClassWrapper.InvoiceHeaderInternalTestMethod.InvoiceNumber);
		}

		public void TestInvoiceHeaderWithGroupInvoiceFalse()
		{
			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			AssertNull("No invoice header defined yet", decTestClassWrapper.InvoiceHeaderInternalTestMethod);

			BaseJobComInvoiceHeader invoiceHeader1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "HEADER2";
			invoiceHeader1.JZ_GroupInvoice = ZBool.False;

			BaseJobComInvoiceGroupHeader groupHeader2 = Declaration.JobComInvoiceGroupHeaders[0];

			AssertNotNull("invoice header", decTestClassWrapper.InvoiceHeaderInternalTestMethod);
			AssertEquals("Invoice Header No. should be 'HEADER2' (JZ_GroupInvoice=='N')", "HEADER2", decTestClassWrapper.InvoiceHeaderInternalTestMethod.InvoiceNumber);
		}

		public void TestActiveInvoiceHeaderGroupInternal()
		{
			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			AssertNotNull("ActiveInvoiceHeaderGroupInternal", decTestClassWrapper.ActiveInvoiceHeaderGroupInternalTestMethod);
		}

		public void TestInvoiceHeadersInternal()
		{
			DocBaseJobDeclarationTestClass decTestClassWrapper = DocBaseJobDeclarationTestClass.New(Declaration, Factory);
			BaseJobComInvoiceGroupHeader invoiceGroupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			AssertNotNull("InvoiceHeadersInternal", decTestClassWrapper.InvoiceHeadersInternalTestMethod);
		}

		#endregion

		#region Sub Class Tests

		public virtual void TestInvoiceHeader()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.JobComInvoiceHeaders.AddNew();

			PropertyInfo property = DeclarationWrapper.GetType().GetProperty("InvoiceHeader");
			AssertNotNull("You must implement a property call InvoiceHeader", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceHeader invoiceHeader = (DocBaseJobComInvoiceHeader)method.Invoke(DeclarationWrapper, Array.Empty<object>());
			AssertNotNull("InvoiceHeader is not null", invoiceHeader);
			Assert("InvoiceHeader is of type DocJobComInvoiceHeader", invoiceHeader.GetType().ToString().EndsWith("DocJobComInvoiceHeader"));
		}

		public virtual void TestActiveInvoiceHeader()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];

			PropertyInfo property = DeclarationWrapper.GetType().GetProperty("ActiveInvoiceHeader");
			AssertNotNull("You must implement a property call ActiveInvoiceHeader", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceGroupHeader activeInvoiceHeader = (DocBaseJobComInvoiceGroupHeader)method.Invoke(DeclarationWrapper, Array.Empty<object>());
			AssertNotNull("ActiveInvoiceHeader is not null", activeInvoiceHeader);
			Assert("ActiveInvoiceHeader is of type DocBaseJobComInvoiceGroupHeader", activeInvoiceHeader.GetType().ToString().EndsWith("DocJobComInvoiceGroupHeader"));
		}

		public void TestContainers()
		{
			Declaration.CusContainers.AddNew();
			Declaration.CusContainers.AddNew();

			PropertyInfo property = DeclarationWrapper.GetType().GetProperty("Containers");
			AssertNotNull("You must implement a property call Containers", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseCusContainerCollection containers = (DocBaseCusContainerCollection)method.Invoke(DeclarationWrapper, Array.Empty<object>());
			AssertNotNull("Containers is not null", containers);
			AssertEquals("Containers has 2 elements", 2, containers.Count);
			Assert("Containers is of type DocCusContainerCollection", containers.GetType().ToString().EndsWith("DocCusContainerCollection"));
		}

		public virtual void TestInvoiceLines()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();

			PropertyInfo property = DeclarationWrapper.GetType().GetProperty("InvoiceLines");
			AssertNotNull("You must implement a property call InvoiceLines", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceLineCollection invoiceLines = (DocBaseJobComInvoiceLineCollection)method.Invoke(DeclarationWrapper, Array.Empty<object>());
			AssertNotNull("InvoiceLines is not null", invoiceLines);
			AssertEquals("InvoiceLines has 2 elements", 2, invoiceLines.Count);
			Assert("InvoiceLines is of type DocJobComInvoiceLineCollection", invoiceLines.GetType().ToString().EndsWith("DocJobComInvoiceLineCollection"));

			line1.JI_PartNo = "123";
			line2.JI_PartNo = "111";

			BaseJobComInvoiceHeader invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line3 = invoiceHeader2.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line4 = invoiceHeader2.JobComInvoiceLines.AddNew();

			line3.JI_PartNo = "111";
			line4.JI_PartNo = "000";

			invoiceHeader.JZ_InvoiceNumber = "INV 111";
			invoiceHeader2.JZ_InvoiceNumber = "INV 000";

			DocBaseJobComInvoiceLineCollection coll = (DocBaseJobComInvoiceLineCollection)method.Invoke(DeclarationWrapper, Array.Empty<object>());
			AssertEquals("InvoiceLines has 4 elements", 4, coll.Count);
			AssertEquals("Sort order test - first line is from INV 000", "000", coll[0].PartNo);
			AssertEquals("Sort order test - second line is from INV 000", "111", coll[1].PartNo);
			AssertEquals("Sort order test - third line is from INV 111", "111", coll[2].PartNo);
			AssertEquals("Sort order test - fourth line is from INV 111", "123", coll[3].PartNo);
		}

		public virtual void TestInvoiceLinesSortedByLineNo()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoiceHeader1.JobComInvoiceLines.AddNew();

			PropertyInfo property = DeclarationWrapper.GetType().GetProperty("InvoiceLinesSortedByLineNo");
			AssertNotNull("You must implement a property call InvoiceLinesSortedByLineNo", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceLineCollection invoiceLinesSortedByLineNo = (DocBaseJobComInvoiceLineCollection)method.Invoke(DeclarationWrapper, Array.Empty<object>());
			AssertNotNull("InvoiceLinesSortedByLineNo is not null", invoiceLinesSortedByLineNo);
			AssertEquals("InvoiceLinesSortedByLineNo has 2 elements", 2, invoiceLinesSortedByLineNo.Count);
			Assert("InvoiceLinesSortedByLineNo is of type DocJobComInvoiceLineCollection", invoiceLinesSortedByLineNo.GetType().ToString().EndsWith("DocJobComInvoiceLineCollection"));

			line1.JI_LineNo = 2;
			line2.JI_LineNo = 1;
			invoiceLinesSortedByLineNo = (DocBaseJobComInvoiceLineCollection)method.Invoke(DeclarationWrapper, Array.Empty<object>());
			AssertEquals("Hastwo elements", 2, invoiceLinesSortedByLineNo.Count);
			AssertEquals("Sort order - first line", "1", invoiceLinesSortedByLineNo[0].LineNo.ToString());
			AssertEquals("Sort order - second line", "2", invoiceLinesSortedByLineNo[1].LineNo.ToString());
		}

		#endregion

		#region IPreAlert

		public void TestPortDisplayMode()
		{
			AssertEquals("LoadDischargeCollectDeliver", DeclarationWrapper.PortDisplayMode);
		}

		public void TestShowChargesOnArrivalNotice()
		{
			AssertEquals(false, DeclarationWrapper.ShowChargesOnArrivalNotice);
		}

		public void TestShowExchangeRatesOnArrivalNotice()
		{
			AssertEquals(false, DeclarationWrapper.ShowExchangeRatesOnArrivalNotice);
		}

		#endregion

		#region IDoc Cartage Advice & Test Cartage Advice Addresses - Work Item W00023252

		#region Headings

		public void TestJourneyOnePickUpHeading()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyOnePickUpHeading: FCL Export", "PICKUP EMPTY", DeclarationWrapper.JourneyOnePickUpHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyOnePickUpHeading: FCL Import", "PICKUP FULL", DeclarationWrapper.JourneyOnePickUpHeading);

			//! PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyOnePickUpHeading: LCL export, No date", "PICKUP", DeclarationWrapper.JourneyOnePickUpHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyOnePickUpHeading: LCL import, No date", "PICKUP", DeclarationWrapper.JourneyOnePickUpHeading);

			//! PrintTwoJourneys + Dates
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			Declaration.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now;
			Declaration.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now.AddDays(1);
			AssertEquals("JourneyOnePickUpHeading: LCL Export + Date", "PICKUP DATE " + Declaration.DocsAndCartage.JP_PickupRequiredBy.ToLongTimeString(), DeclarationWrapper.JourneyOnePickUpHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now;
			Declaration.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now.AddDays(1);
			AssertEquals("JourneyOnePickUpHeading: LCL Import + Date", "PICKUP DATE " + Declaration.DocsAndCartage.JP_EstimatedDelivery.ToLongTimeString(), DeclarationWrapper.JourneyOnePickUpHeading);
		}

		public void TestJourneyOneDeliverToHeading()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyOneDeliverToHeading: FCL Export", "DELIVER TO EMPTY", DeclarationWrapper.JourneyOneDeliverToHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyOneDeliverToHeading: FCL import", "DELIVER TO FULL", DeclarationWrapper.JourneyOneDeliverToHeading);

			//! PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyOneDeliverToHeading: LCL Export", "DELIVER TO", DeclarationWrapper.JourneyOneDeliverToHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyOneDeliverToHeading: LCL Import", "DELIVER TO", DeclarationWrapper.JourneyOneDeliverToHeading);

			//! PrintTwoJourneys + Dates
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			Declaration.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Now;
			AssertEquals("JourneyOneDeliverToHeading: LCL Export + Date", "DELIVER TO", DeclarationWrapper.JourneyOneDeliverToHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			Declaration.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Now;
			AssertEquals("JourneyOneDeliverToHeading: LCL Import + Date", "DELIVER TO DATE " + Declaration.DocsAndCartage.JP_DeliveryRequiredBy.ToLongTimeString(), DeclarationWrapper.JourneyOneDeliverToHeading);
		}

		public void TestJourneyTwoPickUpHeading()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyTwoPickUpHeading: FCL Export", "PICKUP FULL", DeclarationWrapper.JourneyTwoPickUpHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyTwoPickUpHeading: FCL Import", "PICKUP EMPTY", DeclarationWrapper.JourneyTwoPickUpHeading);

			//! PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyTwoPickUpHeading: LCL Export", "PICKUP", DeclarationWrapper.JourneyTwoPickUpHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyTwoPickUpHeading: LCL Import", "PICKUP", DeclarationWrapper.JourneyTwoPickUpHeading);
		}

		public void TestJourneyTwoDeliverToHeading()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;

			//PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyTwoDeliverToHeading: FCL Export", "DELIVER TO FULL", DeclarationWrapper.JourneyTwoDeliverToHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyTwoDeliverToHeading: FCL Import", "DELIVER TO EMPTY", DeclarationWrapper.JourneyTwoDeliverToHeading);

			//! PrintTwoJourneys
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("JourneyTwoDeliverToHeading: LCL Export", "DELIVER TO", DeclarationWrapper.JourneyTwoDeliverToHeading);
			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("JourneyTwoDeliverToHeading: LCL Import", "DELIVER TO", DeclarationWrapper.JourneyTwoDeliverToHeading);
		}

		#endregion

		#region Contacts
		public void TestImportPickUpAndDeliverToContactDetails()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForImports, Core.Constants.ContainerModes.LCL);
			CheckPickupAndDeliveryContactDetails(Core.Constants.ContainerModes.LCL,
					"Depot", ZString.Empty, "Delivery", "Consignee");
		}

		public void TestExportPickUpAndDeliverToContactDetails()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			CheckPickupAndDeliveryContactDetails(Core.Constants.ContainerModes.LCL,
					"Pickup", "Consignor", "Depot", ZString.Empty);
		}

		#endregion

		public void TestIsEmptyLeg()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Journey One, Two Journeys, Export: Should be True", true, DeclarationWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be False", false, DeclarationWrapper.IsEmptyLeg(false));

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Journey One, Two Journeys, Import: Should be False", false, DeclarationWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be True", true, DeclarationWrapper.IsEmptyLeg(false));

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Journey One, One Journey, Import: Should be False", false, DeclarationWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be True", false, DeclarationWrapper.IsEmptyLeg(false));

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Journey One, One Journey, Import: Should be False", false, DeclarationWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be True", false, DeclarationWrapper.IsEmptyLeg(false));
		}

		public void TestIsFullLeg()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Journey One, Two Journeys, Export: Should be True", false, DeclarationWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be False", true, DeclarationWrapper.IsFullLeg(false));

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Journey One, Two Journeys, Import: Should be False", true, DeclarationWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be True", false, DeclarationWrapper.IsFullLeg(false));

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			Declaration.JE_MessageType = MessageTypeCodeForExports;
			AssertEquals("Journey One, One Journey, Import: Should be False", false, DeclarationWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be True", false, DeclarationWrapper.IsFullLeg(false));

			Declaration.JE_MessageType = MessageTypeCodeForImports;
			AssertEquals("Journey One, One Journey, Import: Should be False", false, DeclarationWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be True", false, DeclarationWrapper.IsFullLeg(false));
		}

		public void TestCartageAdviceAddressesImportNoContainers()
		{
			SetupDeclarationForAddressTests(JobMessageTypeList.Codes.Import, ZString.Empty);
			CheckPickupAndDeliveryAddresses(ZString.Empty, Declaration.DepotDocAddress.Address.OA_Address1, ZString.Empty, Declaration.ImporterDeliveryAddress.E2_Address1, DeclarationWrapper.Consignee.DeliverAddress.Address1);
		}

		public void TestCartageAdviceAddressesImportFCLContainers()
		{
			SetupDeclarationForAddressTests(JobMessageTypeList.Codes.Import, Core.Constants.ContainerModes.FCL);
			CheckPickupAndDeliveryAddresses(Core.Constants.ContainerModes.FCL, Declaration.ContainerTerminalOperatorDocAddress.Address.OA_Address1, ZString.Empty, Declaration.ImporterDeliveryAddress.E2_Address1, DeclarationWrapper.Consignee.DeliverAddress.Address1);
		}

		public void TestCartageAdviceAddressesImportLCLContainers()
		{
			SetupDeclarationForAddressTests(JobMessageTypeList.Codes.Import, Core.Constants.ContainerModes.LCL);
			CheckPickupAndDeliveryAddresses(Core.Constants.ContainerModes.LCL, Declaration.DepotDocAddress.Address.OA_Address1, ZString.Empty, Declaration.ImporterDeliveryAddress.E2_Address1, DeclarationWrapper.Consignee.DeliverAddress.Address1);
		}

		public void TestCartageAdviceAddressesImportFCXContainers()
		{
			SetupDeclarationForAddressTests(JobMessageTypeList.Codes.Import, Core.Constants.ContainerModes.FCLMixedShipper);
			CheckPickupAndDeliveryAddresses(Core.Constants.ContainerModes.FCLMixedShipper, Declaration.ContainerTerminalOperatorDocAddress.Address.OA_Address1, ZString.Empty, Declaration.ImporterDeliveryAddress.E2_Address1, DeclarationWrapper.Consignee.DeliverAddress.Address1);
		}

		public void TestCartageAdviceAddressesExportNoContainers()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, ZString.Empty);
			CheckPickupAndDeliveryAddresses(ZString.Empty, Declaration.SupplierPickupAddress.E2_Address1, DeclarationWrapper.Consignor.PickUpAddress.Address1, Declaration.DepotDocAddress.Address.OA_Address1, ZString.Empty);
		}

		public void TestCartageAdviceAddressesExportFCLContainers()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCL);
			CheckPickupAndDeliveryAddresses(Core.Constants.ContainerModes.FCL, Declaration.SupplierPickupAddress.E2_Address1, DeclarationWrapper.Consignor.PickUpAddress.Address1, Declaration.ContainerTerminalOperatorDocAddress.Address.OA_Address1, ZString.Empty);
		}

		public void TestCartageAdviceAddressesExportLCLContainers()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.LCL);
			CheckPickupAndDeliveryAddresses(Core.Constants.ContainerModes.LCL, Declaration.SupplierPickupAddress.E2_Address1, DeclarationWrapper.Consignor.PickUpAddress.Address1, Declaration.DepotDocAddress.Address.OA_Address1, ZString.Empty);
		}

		public void TestCartageAdviceAddressesExportFCXContainers()
		{
			SetupDeclarationForAddressTests(MessageTypeCodeForExports, Core.Constants.ContainerModes.FCLMixedShipper);
			CheckPickupAndDeliveryAddresses(Core.Constants.ContainerModes.FCLMixedShipper, Declaration.SupplierPickupAddress.E2_Address1, DeclarationWrapper.Consignor.PickUpAddress.Address1, Declaration.DepotDocAddress.Address.OA_Address1, ZString.Empty);
		}

		protected virtual string MessageTypeCodeForImports => JobMessageTypeList.Codes.Import;

		protected virtual string MessageTypeCodeForExports => JobMessageTypeList.Codes.Export;

		void CheckPickupAndDeliveryAddresses(ZString containerMode, ZString expectedPickup, ZString expectedPickupFallback, ZString expectedDelivery, ZString expectedDeliveryFallback)
		{
			Declaration.CusContainers.RemoveAndDeleteAll();

			if (!containerMode.IsEmpty)
			{
				BaseCusContainer container1 = Declaration.CusContainers.AddNew();
				container1.CO_FCL_LCL_AIR = containerMode;
			}

			AssertEquals("Pickup address", expectedPickup, DeclarationWrapper.PickupAddress.Address1);
			if (!expectedPickupFallback.IsEmpty)
			{
				Declaration.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
				AssertEquals("Pickup address fallback", expectedPickupFallback, DeclarationWrapper.PickupAddress.Address1);
			}

			AssertEquals("Delivery address", expectedDelivery, DeclarationWrapper.DeliverToAddress.Address1);
			if (!expectedDeliveryFallback.IsEmpty)
			{
				Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
				AssertEquals("Pickup address fallback", expectedDeliveryFallback, DeclarationWrapper.DeliverToAddress.Address1);
			}
		}

		void CheckPickupAndDeliveryContactDetails(ZString containerMode, ZString expectedPickup, ZString expectedPickupFallBack, ZString expectedDelivery, ZString expectedDeliveryFallback)
		{
			Declaration.CusContainers.RemoveAndDeleteAll();

			if (!containerMode.IsEmpty)
			{
				BaseCusContainer container1 = Declaration.CusContainers.AddNew();
				container1.CO_FCL_LCL_AIR = containerMode;
			}

			AssertEquals("Name", expectedPickup + "Name", DeclarationWrapper.JourneyOnePickUpContactName);
			if (!expectedPickupFallBack.IsEmpty)
			{
				Declaration.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
				AssertEquals("Name fallback", expectedPickupFallBack + "Name", DeclarationWrapper.JourneyOnePickUpContactName);
			}

			AssertEquals("Phone fallback", expectedDelivery + "Phone", DeclarationWrapper.JourneyOneDeliverToContactPhone);
			AssertEquals("Name fallback", expectedDelivery + "Name", DeclarationWrapper.JourneyOneDeliverToContactName);
			if (!expectedDeliveryFallback.IsEmpty)
			{
				Declaration.ImporterDeliveryAddress.E2_OA_Address = ZGuid.Empty;
				AssertEquals("Phone fallback", expectedDeliveryFallback + "Phone", DeclarationWrapper.JourneyOneDeliverToContactPhone);
				AssertEquals("Name fallback", expectedDeliveryFallback + "Name", DeclarationWrapper.JourneyOneDeliverToContactName);
			}
		}

		void SetupDeclarationForAddressTests(ZString messageType, ZString containerMode)
		{
			Declaration.JE_MessageType = messageType;
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			if (!containerMode.IsEmpty)
			{
				Declaration.CusContainers.AddNew();
				foreach (BaseCusContainer container in Declaration.CusContainers)
				{
					container.CO_FCL_LCL_AIR = containerMode;
				}
			}
			var depotOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var depotContact = depotOrg.Contacts.AddNew();
			var depotAddress = depotOrg.Addresses.AddNew();
			depotContact.OC_Phone = "DepotPhone";
			depotContact.OC_ContactName = "DepotName";
			depotAddress.OA_Address1 = "abcd";
			depotAddress.OA_Phone = "123";
			depotContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var ctoContact = ctoOrg.Contacts.AddNew();
			var ctoAddress = ctoOrg.Addresses.AddNew();
			ctoContact.OC_Phone = "CTOPhone";
			ctoContact.OC_ContactName = "CTOName";
			ctoAddress.OA_Address1 = "efgh";
			ctoAddress.OA_Phone = "234";
			ctoContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var deliveryContact = deliveryOrg.Contacts.AddNew();
			var deliveryAddress = deliveryOrg.Addresses.AddNew();
			deliveryContact.OC_Phone = "DeliveryPhone";
			deliveryContact.OC_ContactName = "DeliveryName";
			deliveryAddress.OA_Address1 = "ijkl";
			deliveryAddress.OA_Phone = "345";
			deliveryContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var deliveryContact2 = deliveryOrg.Contacts.AddNew();
			deliveryContact2.OC_Phone = "DeliveryPhone2";
			deliveryContact2.OC_ContactName = "DeliveryName2";

			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var pickupContact = pickupOrg.Contacts.AddNew();
			var pickupAddress = pickupOrg.Addresses.AddNew();
			pickupContact.OC_Phone = "PickupPhone";
			pickupContact.OC_ContactName = "PickupName";
			pickupAddress.OA_Address1 = "mnop";
			pickupAddress.OA_Phone = "456";
			pickupContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var pickupContact2 = pickupOrg.Contacts.AddNew();
			pickupContact2.OC_Phone = "PickupPhone2";
			pickupContact2.OC_ContactName = "PickupName2";

			var containerParkOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var containerParkContact = containerParkOrg.Contacts.AddNew();
			var containerParkAddress = containerParkOrg.Addresses.AddNew();
			containerParkContact.OC_Phone = "ContainerParkPhone";
			containerParkContact.OC_ContactName = "ContainerParkName";
			containerParkAddress.OA_Address1 = "ContPark";
			containerParkAddress.OA_Phone = "0123";
			containerParkContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var containerParkContact2 = containerParkOrg.Contacts.AddNew();
			containerParkContact2.OC_Phone = "ContainerParkPhone2";
			containerParkContact2.OC_ContactName = "ContainerParkName2";

			var consignee = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var consigneeContact = consignee.Contacts.AddNew();
			var consigneeAddress = consignee.Addresses.AddNew();
			consigneeContact.OC_Phone = "ConsigneePhone";
			consigneeContact.OC_ContactName = "ConsigneeName";
			consigneeAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			consigneeAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);
			consigneeAddress.OA_Address1 = "qrst";
			consigneeAddress.OA_Phone = "567";
			consigneeContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			var consignor = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var consignorContact = consignor.Contacts.AddNew();
			var consignorAddress = consignor.Addresses.AddNew();
			consignorContact.OC_Phone = "ConsignorPhone";
			consignorContact.OC_ContactName = "ConsignorName";
			consignorAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			consignorAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Pickup);
			consignorAddress.OA_Address1 = "uvwx";
			consignorAddress.OA_Phone = "678";
			consignorContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctoAddress.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			Declaration.SupplierPickupAddress.E2_OA_Address = pickupAddress.PK;
			Declaration.JE_OH_Importer = consignee.PK;
			Declaration.JE_OH_Supplier = consignor.PK;
			Declaration.ContainerYardDocAddress.E2_OA_Address = containerParkAddress.PK;
		}

		#endregion

		#region IContainsSuppressedFields

		public void TestMasterBill_OrSuppressed()
		{
			AssertEquals("MasterBill_OrSuppressed", "", DeclarationWrapper.MasterBill_OrSuppressed);

			Declaration.JE_MasterBill = "12345";
			AssertEquals("MasterBill_OrSuppressed", "12345", DeclarationWrapper.MasterBill_OrSuppressed);
		}

		public void TestTransportInfo_OrSuppressed()
		{
			AssertEquals("TransportInfo_OrSuppressed", "", DeclarationWrapper.TransportInfo_OrSuppressed);

			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("Transport", ZString.Empty, DeclarationWrapper.TransportInfo_OrSuppressed);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VoyageFlightNo = "Voyage";
			AssertEquals("Transport", "Voyage", DeclarationWrapper.TransportInfo_OrSuppressed);

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			Declaration.JE_VesselName = vessel.RV_Code;
			Declaration.JE_OH_ShippingLine = Factory.New<OrgHeader>().PK;
			AssertEquals("Transport", vessel.RV_Code + " / Voyage / " + vessel.RV_LloydsNumber, DeclarationWrapper.TransportInfo_OrSuppressed);
		}

		public void TestETD_OrSuppressed()
		{
			AssertEquals("ETD_OrSuppressed", "", DeclarationWrapper.ETD_OrSuppressed);

			Declaration.JE_DateAtOrigin = ZDateTime.Today;
			AssertEquals("ETD_OrSuppressed only shows date", DeclarationWrapper.CollectedFromETDString, DeclarationWrapper.ETD_OrSuppressed);
		}

		public void TestLoadingETD_OrSuppressed()
		{
			AssertEquals("LoadingETD_OrSuppressed", "", DeclarationWrapper.LoadingETD_OrSuppressed);

			Declaration.JE_ExportDate = ZDateTime.Today;
			AssertEquals("ETD_OrSuppressed only shows date", DeclarationWrapper.LoadingETDString, DeclarationWrapper.LoadingETD_OrSuppressed);
		}

		public void TestCarrierName_OrSuppressed()
		{
			AssertEquals("CarrierName_OrSuppressed", "", DeclarationWrapper.CarrierName_OrSuppressed);
		}

		public void TestCarrierCCC_OrSuppressed()
		{
			AssertEquals("CarrierCCC_OrSuppressed", "", DeclarationWrapper.CarrierCCC_OrSuppressed);
		}

		public void TestSuppressFlightDetailsFooter()
		{
			AssertEquals("SuppressFlightDetailsFooter", "", DeclarationWrapper.SuppressFlightDetailsFooter);
		}
		#endregion

		#region ITrackingBusinessObject

		public void TestTrackingBusinessContext()
		{
			AssertEquals(TrackingConstants.BusinessContext.Declaration, DeclarationWrapper.TrackingBusinessContext);
		}

		public void TestTrackingBusinessObjectPK()
		{
			AssertEquals(Declaration.PK, DeclarationWrapper.TrackingBusinessObjectPK);
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocBaseJobDeclaration.New(Declaration, Factory)
			};
		}

		#region Implementation

		protected OrgHeader FCLCartage;
		protected OrgHeader LCLCartage;
		protected OrgHeader AIRCartage;
		protected BaseJobComInvoiceGroupHeader GroupHeader;
		protected BaseJobComInvoiceHeader InvoiceHeader;
		protected BaseJobComInvoiceHeader InvoiceHeader2;
		protected RefCurrency fSGCurr;
		protected RefCurrency SGCurr
		{
			get
			{
				if (fSGCurr == null)
				{
					fSGCurr = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "SGD"));
				}

				return fSGCurr;
			}
		}

		protected RefCurrency fUSCurr;
		protected RefCurrency USCurr
		{
			get
			{
				if (fUSCurr == null)
				{
					fUSCurr = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
				}

				return fUSCurr;
			}
		}

		protected RefCurrency ForeignCurr
		{
			get { return (USCurr.PK == GlbCompany.CurrentCompany.Country.LocalCurrency.PK) ? SGCurr : USCurr; }
		}

		protected T Declaration;
		protected TWrapper DeclarationWrapper
		{
			get { return CreateDeclarationWrapper(Declaration); }
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreateDeclarationWrapper(Declaration);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			Declaration = GetNewJobDeclaration();
			base.SetUp();
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}

		protected virtual T GetNewJobDeclaration()
		{
			return Factory.New<T>();
		}

		protected OrgAddress CreateOrgAddress(BusinessObjectFactory factory, string address1, string address2)
		{
			var address = factory.New<OrgAddress>();
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address.OA_City = "City";
			address.OA_OH = ZArchitecture.Core.Utilities.GetGuidFromRandomRowInTable(OrgHeaderSchema.Constants.PK, OrgHeaderSchema.Constants.TableName);
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			return address;
		}

		protected BaseCusContainer CreateContainer(T declaration, string mode)
		{
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = mode;
			return container;
		}

		protected OrgHeader CreateTestOrgHeader(BusinessObjectFactory factory, string code, string fullName, string address1, string address2, string city)
		{
			var orgHeaderBizO = factory.New<OrgHeader>();
			orgHeaderBizO.OH_Code = code;
			orgHeaderBizO.OH_FullName = fullName;
			orgHeaderBizO.MainAddress.OA_Address1 = address1;
			orgHeaderBizO.MainAddress.OA_Address2 = address2;
			orgHeaderBizO.MainAddress.OA_City = city;
			factory.Save();
			return orgHeaderBizO;
		}

		protected void CreateNote_ConsideringShipment(T declaration, ZString noteTypeDesc, ZString data, BusinessObjectFactory factory)
		{
			StmNote note = declaration.NotesOfDeclarationOrShipment.AddNew();
			note.ST_Description = noteTypeDesc;
			note.ST_Table = declaration.TableName;
			note.ST_ParentID = declaration.PK;
			note.ST_NoteDataAsText = data;
			factory.Save();
		}

		protected JobHeader CreateJobHeader(T declaration)
		{
			JobHeader jobHeaderBisObj = new JobHeader.Loader(declaration).TryCreateWithMutex();
			jobHeaderBisObj.Parent = declaration;
			jobHeaderBisObj.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = declaration.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			var departmentBisObj = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			jobHeaderBisObj.JH_GE = departmentBisObj.PK;
			jobHeaderBisObj.JH_JobNum = declaration.JE_DeclarationReference;
			jobHeaderBisObj.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "AVTEST";
			orgHeaderBisObj.OH_FullName = "Angie's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";
			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;

			return jobHeaderBisObj;
		}

		protected AccChargeCode CreateChargeCode(BusinessObjectFactory factory, string chargeCode)
		{
			var code = factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "Test Charge Code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			code.FillWithValidTestData();
			return code;
		}

		protected JobCharge CreateLineCharge(BusinessObjectFactory factory, JobHeader jobHeaderBisObj, ZGuid chargeCodePK, ZDecimal amount, ZGuid localChargesPK)
		{
			var lineCharge = factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_LocalSellAmt = amount;
			lineCharge.JR_OSSellAmt = amount;
			lineCharge.JR_OH_SellAccount = localChargesPK;
			return lineCharge;
		}

		protected void SetFCL_LCL_AIRCartageBizo(BusinessObjectFactory factory)
		{
			FCLCartage = factory.New<OrgHeader>();
			FCLCartage.OH_FullName = "ABC Pty Ltd.";
			FCLCartage.MainAddress.OA_Address1 = "ABC Street";
			FCLCartage.OH_Code = "ABCPTY";

			LCLCartage = factory.New<OrgHeader>();
			LCLCartage.OH_FullName = "LCL Pty Ltd.";
			LCLCartage.MainAddress.OA_Address1 = "LCL Street";
			LCLCartage.OH_Code = "LCLPTY";

			AIRCartage = factory.New<OrgHeader>();
			AIRCartage.OH_FullName = "Airway Pty Ltd.";
			AIRCartage.MainAddress.OA_Address1 = "Air Street";
			AIRCartage.OH_Code = "AIRPTY";
		}

		protected void CreateGroupHeaderAndTwoInvoiceHeader()
		{
			GroupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			GroupHeader.JobComInvoiceHeaders.DeleteAll();
			InvoiceHeader = GroupHeader.JobComInvoiceHeaders.AddNew();
			InvoiceHeader2 = GroupHeader.JobComInvoiceHeaders.AddNew();
		}

		protected void CreateGroupHeaderAndTwoInvoiceHeaderForIncludedCase(string chargeName, ZBool invoiceHeadersInLocalCurr)
		{
			CreateGroupHeaderAndTwoInvoiceHeader();

			InvoiceHeader.JZ_InvoiceAmount = 1000;
			if (invoiceHeadersInLocalCurr)
			{
				InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			}
			else
			{
				CurrencyConverterTestHelper.SetExchangeRate(Factory, ForeignCurr, InvoiceHeader.IsReciprocalRates ? 1.25m : 0.8M);
				InvoiceHeader.JZ_RX_NKInvoice_Currency = ForeignCurr.RX_Code;
			}
			InvoiceHeader.Charges.AddNew(chargeName, 100);
			InvoiceHeader2.JZ_InvoiceAmount = 2000;
			InvoiceHeader2.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			InvoiceHeader2.Charges.AddNew(chargeName, 200);
		}

		protected void CreateGroupHeaderAndTwoInvoiceHeaderForExcludedCase(string chargeName, ZBool invoiceHeadersInLocalCurr)
		{
			CreateGroupHeaderAndTwoInvoiceHeader();

			InvoiceHeader.JZ_InvoiceAmount = 1000;
			if (invoiceHeadersInLocalCurr)
			{
				GroupHeader.Charges.AddNew(chargeName, 300m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
				InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			}
			else
			{
				CurrencyConverterTestHelper.SetExchangeRate(Factory, ForeignCurr, InvoiceHeader.IsReciprocalRates ? 1.333333m : 0.75M);
				GroupHeader.Charges.AddNew(chargeName, 300m, ForeignCurr.RX_Code);
				InvoiceHeader.JZ_RX_NKInvoice_Currency = ForeignCurr.RX_Code;
			}
			InvoiceHeader.JZ_InvoiceAmount = 1000;
			InvoiceHeader.JZ_IncoTerm = "DDP";

			InvoiceHeader2.JZ_InvoiceAmount = 2000;
			InvoiceHeader2.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			InvoiceHeader2.JZ_IncoTerm = "DDP";
			Declaration.ResumeApportionment();
			AssertEquals("PreCondition:This charge has been apportioned", 1, InvoiceHeader2.GroupCharges.Count);
			AssertEquals("PreCondition:This charge has been apportioned", 1, InvoiceHeader.GroupCharges.Count);
			InvoiceHeader.GroupCharges[0].J7_Calc_IsIncludedInInvoiceAmount = false;
			InvoiceHeader2.GroupCharges[0].J7_Calc_IsIncludedInInvoiceAmount = false;
		}

		protected OrgCusCode CreateCustomCode(OrgHeader org, ZString type, ZString value, ZString countryCode)
		{
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = type;
			cusCode.OK_CustomsRegNo = value;
			cusCode.OK_RN_NKCodeCountry = countryCode;
			return cusCode;
		}

		protected OrgHeader CreateTestOrgHeader(string code, string fullName, string address1, string address2, string city)
		{
			var orgHeaderBizO = Factory.New<OrgHeader>();
			orgHeaderBizO.OH_Code = code;
			orgHeaderBizO.OH_FullName = fullName;
			orgHeaderBizO.MainAddress.OA_Address1 = address1;
			orgHeaderBizO.MainAddress.OA_Address2 = address2;
			orgHeaderBizO.MainAddress.OA_City = city;
			Factory.Save();
			return orgHeaderBizO;
		}
		#endregion

	}
}
