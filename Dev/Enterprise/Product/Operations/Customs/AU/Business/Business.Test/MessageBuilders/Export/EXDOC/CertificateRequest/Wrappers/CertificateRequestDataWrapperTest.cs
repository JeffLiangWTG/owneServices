using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CertificateRequestDataWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var wrapper = new CertificateRequestDataWrapper(quarantineExDocHeader);
			AssertEquals("", EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(quarantineExDocHeader.QH_ProduceType), wrapper.CommodityType);
			AssertEquals("DischargePort", jobDeclaration.PortOfArrival.Code, wrapper.DischargePort);
			AssertEquals("DestinationCity", jobDeclaration.FinalDestination.Description, wrapper.DestinationCity);

			AssertEquals("CertificateRequiredLocation", quarantineExDocHeader.QH_CertificateRequiredLocation, wrapper.CertificateRequiredLocation);

			AssertEquals("ExporterCertificateReference", jobDeclaration.JE_OwnerRef, wrapper.ExporterCertificateReference);
			jobDeclaration.JE_UseOwnerRefAsQuarantineRef = false;
			AssertEquals("ExporterCertificateReference", EDIMessage.SendersReferencePlaceHolder, wrapper.ExporterCertificateReference);
			jobDeclaration.JE_UseOwnerRefAsQuarantineRef = true;
			jobDeclaration.JE_OwnerRef = string.Empty;
			AssertEquals("ExporterCertificateReference", EDIMessage.SendersReferencePlaceHolder, wrapper.ExporterCertificateReference);

			AssertEquals("NotifyPartyText", jobDeclaration.EXDOCNotifyText, wrapper.NotifyPartyText);
			AssertEquals("LetterOfCreditText", jobDeclaration.EXDOCLetterOfCredit, wrapper.LetterOfCreditText);

			AssertEquals("SeparateCertificateContainerInd", quarantineExDocHeader.QH_SplitHealthCertByContainer, wrapper.SeparateCertificateContainerInd);
			AssertEquals("SeparateCertificateMarksInd", quarantineExDocHeader.QH_SplitHealthCertByMarks, wrapper.SeparateCertificateMarksInd);
			AssertEquals("SeparateCertificatePackerInd", quarantineExDocHeader.QH_SplitHealthCertByPacker, wrapper.SeparateCertificatePackerInd);

			AssertEquals("ImportPermits", 2, RFPNumberWrapperTest.GetCount(wrapper.ImportPermits));
			int i = 1;
			foreach (IImportPermit permit in wrapper.ImportPermits)
			{
				AssertEquals("PermitNumber", "PERMIT" + i, permit.PermitNumber);
				AssertEquals("PermitDate", new ZDateTime(2009, 10, i), permit.PermitDate);
				i++;
			}
			foreach (JobComInvoiceLine line in invoiceHeader.InvoiceLines)
			{
				line.JI_TempImportNum = ZString.Empty;
			}

			AssertEquals("ImportPermits", 0, RFPNumberWrapperTest.GetCount(wrapper.ImportPermits));

			AssertEquals("OwnerExporterNumber", invoiceHeader.EXDOCExporterNumber, wrapper.OwnerExporterNumber);
			AssertEquals("Consignee", jobDeclaration.Consignee, wrapper.Consignee);
			AssertEquals("Forwarder", jobDeclaration.Forwarder, wrapper.Forwarder);
			AssertEquals("TransportMode", EXDOCTransportModeCodes.GetEXDOCCodeFromTransportModeCode(jobDeclaration.TransportMode), wrapper.TransportMode);
			AssertEquals("VoyageFlightNumber", jobDeclaration.JE_VoyageFlightNo, wrapper.VoyageFlightNumber);
			AssertEquals("CarrierName", jobDeclaration.ShippingLine.OH_FullName, wrapper.CarrierName);
			AssertEquals("VesselName", jobDeclaration.JE_VesselName, wrapper.VesselName);
			AssertEquals("DepartureDate", jobDeclaration.JE_ExportDate, wrapper.DepartureDate);

			i = 1;
			foreach (ICertificateLine line in wrapper.CertificateLines)
			{
				AssertEquals("LineNumber", i, line.LineNumber);
				AssertEquals("CertificateLines sorted by JI_LineNo", i, invoiceHeader.JobComInvoiceLines[i - 1].JI_LineNo);
				i++;
			}
		}

		public void TestCertificateRequest()
		{
			var messageBuilder = new CertificateRequestMessageBuilder(new CertificateRequestDataWrapper(quarantineExDocHeader));
			var certificateRequest = messageBuilder.GenerateMessage();
			AssertEquals("Certificate Request Message", ExpectedMessage, certificateRequest.EM_FormattedMessageText.TrimEnd());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();

			jobDeclaration = helper.Declaration;
			jobDeclaration.CusContainers.AddNew().CO_ContainerNumber = "FESU1234567";
			var container = jobDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "FESU7654321";
			container.CO_Seal = "SEAL1";
			jobDeclaration.CusContainers.AddNew().CO_ContainerNumber = "FESU9876543";
			jobDeclaration.JE_UseOwnerRefAsQuarantineRef = true;
			jobDeclaration.JE_OwnerRef = "EXPORTER REFERENCE";
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			jobDeclaration.JE_VoyageFlightNo = "191";
			jobDeclaration.JE_VesselName = "VESSEL NAME";
			jobDeclaration.JE_ExportDate = new ZDate(2009, 10, 10);

			jobDeclaration.JE_OH_ShippingLine = Factory.New<OrgHeader>().PK;
			jobDeclaration.ShippingLine.OH_FullName = "CARRIER NAME";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE NAME";
			consignee.MainAddress.OA_Address1 = "ADDRESS LINE 1";
			consignee.MainAddress.OA_Address2 = "ADDRESS LINE 2";
			consignee.MainAddress.OA_City = "LOS ANGELES";
			consignee.MainAddress.OA_PostCode = "654321";
			consignee.OH_RL_NKClosestPort = "USLAX";
			consignee.MainAddress.OA_State = "CA";
			consignee.MainAddress.OA_Phone = "+02 (1234) 5678";
			jobDeclaration.JE_OH_Importer = consignee.PK;

			jobDeclaration.JE_OH_Forwarder = Factory.New<OrgHeader>().PK;
			jobDeclaration.Forwarder.OH_FullName = "FORWARDER NAME";

			var notifyTextNote = jobDeclaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "Notify Party Text";

			var letterOfCreditTextNote = jobDeclaration.Notes.AddNew();
			letterOfCreditTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			letterOfCreditTextNote.ST_NoteText = "Letter Of Credit Text";

			jobDeclaration.JE_RL_NKPortOfArrival = "USIFR";
			jobDeclaration.JE_RL_NKFinalDestination = "USIFR";

			invoiceHeader = helper.Header1;
			quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			quarantineExDocHeader.QH_CertificateRequiredLocation = "CBR";
			quarantineExDocHeader.QH_SplitHealthCertByContainer = true;
			quarantineExDocHeader.QH_SplitHealthCertByMarks = true;
			quarantineExDocHeader.QH_SplitHealthCertByPacker = true;

			invoiceHeader.JZ_OH_Supplier = Factory.New<OrgHeader>().PK;
			invoiceHeader.Supplier.OH_Code = "TSTSUP";
			var au = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			invoiceHeader.Supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, au, "EXPNUM");

			var invoiceLine = helper.Line1;
			invoiceLine.JI_LineNo = 2;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine = true;
			invoiceLine.JI_TempImportNum = "PERMIT1";
			invoiceLine.JI_TempImportDate = new ZDateTime(2009, 10, 1);
			CertificateLineWrapperTest.FillQuarantineExDocLine(invoiceLine.QuarantineExDocLine);
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine, "RFP1234", 1);
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine, "RFP4321", 1);
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine, "RFP1234", 2);

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 3;
			invoiceLine2.JI_TempImportNum = "PERMIT1";
			invoiceLine2.JI_TempImportDate = new ZDateTime(2009, 10, 10);
			CertificateLineWrapperTest.FillQuarantineExDocLine(invoiceLine2.QuarantineExDocLine);
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine2, "RFP2345", 1);

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 1;
			invoiceLine3.JI_TempImportNum = "PERMIT2";
			invoiceLine3.JI_TempImportDate = new ZDateTime(2009, 10, 2);
			CertificateLineWrapperTest.FillQuarantineExDocLine(invoiceLine3.QuarantineExDocLine);
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine3, "RFP3456", 1);
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceHeader invoiceHeader;
		QuarantineExDocHeader quarantineExDocHeader;

		string ExpectedMessage
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();

				/*Message Header*/
				builder.Append("UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:CR0801");
				/*Message Identifier*/
				builder.Append("BGM+G::AQ++9");
				/*Discharge Port*/
				builder.Append("LOC+12+USIFR");
				/*Destination City*/
				builder.Append("LOC+8+RANIER");
				/*Cert Req Location*/
				builder.Append("LOC+91+CBR");
				/*Exporter Reference*/
				builder.Append("RFF+ABE:EXPORTER REFERENCE");
				/*Notify Party Text*/
				builder.Append("FTX+AAG+++NOTIFY PARTY TEXT");
				/*Letter of Credit Text*/
				builder.Append("FTX+AAW+++LETTER OF CREDIT TEXT");
				/*Separate Cert Container Ind*/
				builder.Append("GIS+Y::AQ:SC");
				/*Separate Cert Marks Ind*/
				builder.Append("GIS+Y::AQ:SM");
				/*Separate Cert Packer Ind*/
				builder.Append("GIS+Y::AQ:SP");
				/*Import Permit Nbr 1*/
				builder.Append("DOC+911+PERMIT1");
				/*Import Permit Date 1*/
				builder.Append("DTM+137:20091001:102");
				/*Import Permit Nbr 2*/
				builder.Append("DOC+911+PERMIT2");
				/*Import Permit Date 2*/
				builder.Append("DTM+137:20091002:102");
				/*Owner Exporter Nbr*/
				builder.Append("PNA+EX+EXPNUM");
				/*Consignee Name*/
				builder.Append("PNA+CN+++++10:CONSIGNEE NAME");
				/*Consignee Address*/
				builder.Append("ADR++5:ADDRESS LINE 1 ADDRESS LINE 2+LOS ANGELES+654321+US+:::CA");
				/*Consignee Contact Details*/
				builder.Append("CTA+CN");
				/*Consignee Phone*/
				builder.Append("COM+0212345678:TE");
				/*Consignee Represent Name*/
				builder.Append("CTA+AG+:FORWARDER NAME");
				/*Transport Details*/
				builder.Append("TDT+12+191+1++:::CARRIER NAME+++:::VESSEL NAME");
				/*Departure Date*/
				builder.Append("DTM+136:20091010:102");
				/*Certificate Line 1*/
				AppendCertificateLine(builder, 1);
				/*RFP Number 1*/
				AppendRFPNumber(builder, "RFP3456", new[] { 1 }, System.Array.Empty<RFPContainerForTesting>());
				/*Certificate Line 2*/
				AppendCertificateLine(builder, 2);
				var containers = new[] { new RFPContainerForTesting { ContainerNumber = "FESU7654321", Seal = "SEAL1" }, new RFPContainerForTesting { ContainerNumber = "FESU9876543" } };
				/*RFP Number 1*/
				AppendRFPNumber(builder, "RFP1234", new[] { 1, 2 }, containers);
				/*RFP Number 2*/
				AppendRFPNumber(builder, "RFP4321", new[] { 1 }, containers);
				/*Certificate Line 3*/
				AppendCertificateLine(builder, 3);
				/*RFP Number 1*/
				AppendRFPNumber(builder, "RFP2345", new[] { 1 }, System.Array.Empty<RFPContainerForTesting>());
				/*Message Trailer*/
				builder.Append("UNT+73+<<MSGNO PLACEHOLDER>>");

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		void AppendCertificateLine(ZStringBuilder builder, int lineNum)
		{
			/*Line Identifier*/
			builder.Append("LIN+" + lineNum);
			/*Line Net Quantity*/
			builder.Append("MEA+AAA+SQ+KGM:2080");
			/*Product Code*/
			builder.Append("PIA+5+XWHTVRGC:CC");
			/*Exp Def Product Desc*/
			builder.Append("IMD+++UHC:::DESCRIPTION LINE 1---------------->:DESCRIPTION LINE 2");
			/*Additional Product Desc*/
			builder.Append("IMD+++AD:::DESCRIPTION LINE 1---------------->:DESCRIPTION LINE 2");
			/*Extra Certificate 1*/
			builder.Append("DOC+852:::E1234+:9");
			/*Packaging Details*/
			builder.Append("PAC+123+3+VR::AQ");
		}

		void AppendRFPNumber(ZStringBuilder builder, string rfpNum, int[] rfpLineNums, RFPContainerForTesting[] containers)
		{
			/*RFP Number*/
			builder.Append("RFF+DM:" + rfpNum);

			foreach (int rfpLineNum in rfpLineNums)
			{
				/*RFP Line Number*/
				builder.Append("LIN+" + rfpLineNum + "+++1");
				/*RFP Line Net Quantity*/
				builder.Append("MEA+AAA+SQ+KGM:10");
				/*RFP Line Packaging Details*/
				builder.Append("PAC+10+3+DR::AQ");

				foreach (IRFPContainer container in containers)
				{
					/*Container Num*/
					builder.Append("EQD+CN+" + container.ContainerNumber);
					/*Container Seal*/
					if (!container.Seal.IsEmpty)
					{
						builder.Append("SEL+" + container.Seal);
					}
				}
			}
		}
	}
}
