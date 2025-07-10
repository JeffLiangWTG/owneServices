using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Edifact.D11B.Segments;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class D11BMessageUtilitiesPopulateDetailsTest : TestCaseWithFactory
	{
		public void TestPopulateUNH()
		{
			var unh = new UNHSegment();
			D11BMessageUtilities.PopulateUNH(unh, "1234", "GOVCBR", "D", "11B", "UN", "ACIHG");
			AssertEquals("UNH", "UNH+1234+GOVCBR:D:11B:UN:ACIHG'", unh.ToString(new CACharSet()));
		}

		public void TestPopulateBGM()
		{
			var bgm = new BGMSegment();
			D11BMessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.HouseBillOfLading, "8XXX1234567", MessageFunctionCodeList.ProposedAmendment);
			AssertEquals("BGM", "BGM+714+8XXX1234567+52'", bgm.ToString(new CACharSet()));
		}

		public void TestPopulateRFF()
		{
			var rff = new RFFSegment();
			D11BMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference, "SECONDARY BUSINESS ID");
			AssertEquals("RFF", "RFF+ABO:SECONDARY BUSINESS ID'", rff.ToString(new CACharSet()));
		}

		public void TestPopulateNAD()
		{
			var nad = new NADSegment();
			D11BMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Carrier, "9XXX");
			AssertEquals("NAD", "NAD+CA+9XXX'", nad.ToString(new CACharSet()));
		}

		public void TestPopulateIFD()
		{
			var ifd = new IFDSegment();
			D11BMessageUtilities.PopulateIFD(ifd, "MF");
			AssertEquals("IFD", "IFD++++MF'", ifd.ToString(new CACharSet()));
		}

		public void TestPopulateDOC()
		{
			var doc = new DOCSegment();
			D11BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.StatusInformation, DocumentStatusCodeList.Status2);
			AssertEquals("DOC", "DOC+23+:24'", doc.ToString(new CACharSet()));

			doc = new DOCSegment();
			D11BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.CustomsManifest, "9XXX12345");
			AssertEquals("DOC", "DOC+85+9XXX12345'", doc.ToString(new CACharSet()));
		}

		public void TestPopulateRCS()
		{
			var rcs = new RCSSegment();
			D11BMessageUtilities.PopulateRCS(rcs, SectorAreaIdentificationCodeQualifierList.Government);
			AssertEquals("RCS", "RCS+15'", rcs.ToString(new CACharSet()));
		}

		public void TestPopulateFTX()
		{
			var ftx = new FTXSegment();
			D11BMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.AdditionalInformation, "FREE TEXT");
			AssertEquals("FTX", "FTX+ACB+++FREE TEXT'", ftx.ToString(new CACharSet()));

			D11BMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.AdditionalInformation, new string('X', 255) + "><ZZZZZ");
			AssertEquals("FTX", "FTX+ACB+++" + new string('X', 255) + ">:<ZZZZZ'", ftx.ToString(new CACharSet()));
		}

		public void TestPopulateAJT()
		{
			var ajt = new AJTSegment();
			D11BMessageUtilities.PopulateAJT(ajt, AdjustmentReasonDescriptionCodeList.MutuallyDefined, EManifestAmendmentReasonCodes.Codes.Typo);
			AssertEquals("AJT", "AJT+ZZZ+35'", ajt.ToString(new CACharSet()));
		}

		public void TestPopulateTDT()
		{
			var tdt = new TDTSegment();
			D11BMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.AtBorder, "1");
			AssertEquals("TDT", "TDT+11++1'", tdt.ToString(new CACharSet()));
		}

		public void TestPopulateUNS()
		{
			var uns = new UNSSegment();
			D11BMessageUtilities.PopulateUNS(uns, "D");
			AssertEquals("UNS", "UNS+D'", uns.ToString(new CACharSet()));
		}

		public void TestPopulateHYN()
		{
			var hyn = new HYNSegment();
			D11BMessageUtilities.PopulateHYN(hyn, HierarchyObjectCodeQualifierList.NoHierarchy);
			AssertEquals("HYN", "HYN+3'", hyn.ToString(new CACharSet()));
		}

		public void TestPopulateCNI()
		{
			var cni = new CNISegment();
			D11BMessageUtilities.PopulateCNI(cni, "1");
			AssertEquals("CNI", "CNI+1'", cni.ToString(new CACharSet()));
		}

		public void TestPopulateSTS()
		{
			var sts = new STSSegment();
			D11BMessageUtilities.PopulateSTS(sts, StatusDescriptionCodeList.GetFromString("0"));
			AssertEquals("STS", "STS++0'", sts.ToString(new CACharSet()));
		}

		public void TestPopulateMEAasInteger()
		{
			var mea = new MEASegment();
			D11BMessageUtilities.PopulateMEAasInteger(mea, MeasurementPurposeCodeQualifierList.ConsignmentMeasurement, 99.1m, "MTQ");
			AssertEquals("MEA", "MEA+AAX++MTQ:99'", mea.ToString(new CACharSet()));
			D11BMessageUtilities.PopulateMEAasInteger(mea, MeasurementPurposeCodeQualifierList.ConsignmentMeasurement, 0.5m, "MTQ");
			AssertEquals("MEA", "MEA+AAX++MTQ:1'", mea.ToString(new CACharSet()));
		}

		public void TestPopulateHAN()
		{
			var han = new HANSegment();
			D11BMessageUtilities.PopulateHAN(han, "SPECIAL INSTRUCTIONS");
			AssertEquals("HAN", "HAN+:::SPECIAL INSTRUCTIONS'", han.ToString(new CACharSet()));
		}

		public void TestPopulateCTA()
		{
			var cta = new CTASegment();
			D11BMessageUtilities.PopulateCTA(cta, "CONTACT NAME");
			AssertEquals("CTA", "CTA+IC+:CONTACT NAME'", cta.ToString(new CACharSet()));
		}

		public void TestPopulateCOM()
		{
			var cta = new CTASegment();
			var com = new COMSegment();
			D11BMessageUtilities.PopulateCOM(cta, com, "0016139544567");
			AssertEquals("CTA", "CTA+AH'", cta.ToString(new CACharSet()));
			AssertEquals("COM", "COM+0016139544567:TE'", com.ToString(new CACharSet()));
		}

		public void TestPopulateNADName()
		{
			var nad = new NADSegment();
			D11BMessageUtilities.PopulateNADName(nad, PartyFunctionCodeQualifierList.ContactParty, "UNDG CONTACT NAME");
			AssertEquals("NAD", "NAD+PK+++UNDG CONTACT NAME'", nad.ToString(new CACharSet()));
		}

		public void TestPopulateLOC()
		{
			var loc = new LOCSegment();
			D11BMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfDestination, "0453", "2955");
			AssertEquals("LOC", "LOC+8+0453+2955'", loc.ToString(new CACharSet()));
		}

		public void TestPopulateEQD()
		{
			var eqd = new EQDSegment();
			D11BMessageUtilities.PopulateEQD(eqd, "CONT1231230");
			AssertEquals("EQD", "EQD+CN+CONT1231230'", eqd.ToString(new CACharSet()));
		}

		public void TestPopulateSEQ()
		{
			var seq = new SEQSegment();
			D11BMessageUtilities.PopulateSEQ(seq, ActionCodeList.NoAction);
			AssertEquals("SEQ", "SEQ+4'", seq.ToString(new CACharSet()));
		}

		public void TestPopulateSEL()
		{
			var sel = new SELSegment();
			D11BMessageUtilities.PopulateSEL(sel, "SEAL NUMBER");
			AssertEquals("SEL", "SEL+SEAL NUMBER'", sel.ToString(new CACharSet()));
		}

		public void TestPopulatePAC()
		{
			var pac = new PACSegment();
			D11BMessageUtilities.PopulatePAC(pac, 9, "BOX");
			AssertEquals("PAC", "PAC+9++:::BOX'", pac.ToString(new CACharSet()));
		}

		public void TestPopulatePCI()
		{
			var pci = new PCISegment();
			D11BMessageUtilities.PopulatePCI(pci, "MARKS AND NUMBERS");
			AssertEquals("PCI", "PCI++MARKS AND NUMBERS'", pci.ToString(new CACharSet()));
		}

		public void TestPopulateGID()
		{
			var gid = new GIDSegment();
			D11BMessageUtilities.PopulateGID(gid, 1);
			AssertEquals("GID", "GID+1'", gid.ToString(new CACharSet()));
		}

		public void TestPopulateTCC()
		{
			var tcc = new TCCSegment();
			D11BMessageUtilities.PopulateTCC(tcc, "0000700000", "SRZ");
			AssertEquals("TCC", "TCC+++0000700000:SRZ'", tcc.ToString(new CACharSet()));
		}

		public void TestPopulateCNT()
		{
			var cnt = new CNTSegment();
			D11BMessageUtilities.PopulateCNT(cnt, ControlTotalTypeCodeQualifierList.TotalGrossWeight, 9, "LBR");
			AssertEquals("CNT", "CNT+7:9:LBR'", cnt.ToString(new CACharSet()));
		}

		public void TestPopulateUNT()
		{
			var uNT = new UNTSegment();
			D11BMessageUtilities.PopulateUNT(uNT, "10", "20");
			AssertEquals("UNT", "UNT+10+20'", uNT.ToString(new CACharSet()));
		}
	}
}
