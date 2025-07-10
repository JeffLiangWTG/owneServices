using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.UkCinv.Testing
{
	/// <summary>
	/// TO DO - have parsed a DUCR-level UKCINV/DEC response but also need to see MUCR-level one and one containing 'next' items. See DES 222 sect 3.1.1.
	/// </summary>
	public class UkCinvResponseTest : TestCaseWithFactory
	{
		readonly string eaaResponse = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'BGM+EAA:105:109'GEI+CRC+000'RFF+AES:movt-ref'LOC+14+gds-locn:156:109:shed-op-id+epu-no::109:eps-id'UNS+D'LOC+14'RFF+ABO:decln-ucr:decln-part-no'GEI+ICS+ics'GEI+SOE+soe'GEI+ROE+roe'CNT+11:tot-pkgs'MEA+AAR++KGM:tot-net-mass'CST++cmdty-code:122'AUT+submit-role'UNS+S'UNT+17+sys-mrn'";
		readonly string ealResponse = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'BGM+EAL:105:109'GEI+CRC+000'RFF+AES:movt-ref'LOC+14+gds-locn:156:109:shed-op-id+epu-no::109:eps-id'DTM+178:gds-arr-dtm:203'UNS+D'LOC+14'RFF+ABO:decln-ucr:decln-part-no'GEI+ICS+ics'GEI+SOE+soe'GEI+ROE+roe'CNT+11:tot-pkgs'MEA+AAR++KGM:tot-net-mass'CST++cmdty-code:122'AUT+submit-role'UNS+S'UNT+18+sys-mrn'";
		readonly string decResponse = @"UNH+09448335114876+UKCINV:D:00A:UN:109001+2TB01J81L'BGM+DEC:105:109'RFF+ABO:decln-ucr:decln-part-no'UNS+D'LOC+14+FXT:156:109'RFF+ABO:decln-ucr:decln-part-no'GEI+ICS+92'GEI+ROE+H'GEI+TYP+N'UNS+S'UNT+11+09448335114876'";
		readonly string decResponseWithMucr = @"UNH+10204360094950+UKCINV:D:00A:UN:109001+ED4EFDF0A86445F3869ECC5F2D755D50'BGM+DEC:105:109'GEI+OPN+Y'RFF+UCN:A?:41305041341'UNS+D'RFF+UCN:A?:11102051202'LOC+14'RFF+ABO:2GB945390992000-S00030113:R'GEI+ICS+A1'GEI+ROE+H'GEI+SOE+1'GEI+TYP+4'UNS+S'UNT+14+10204360094950'";

		static UNOACharacterSet CharSet
		{
			get
			{
				return new UkCharSet();
			}
		}

		public void TestDEC_ResponseWithMucr()
		{
			var ukcinv = new UkCinvMessage();
			ukcinv.Parse(CharSet, decResponseWithMucr);
			AssertEquals(Interrogate_DecMucr.FunctionCodeConst, ukcinv.BGM[0].DocumentMessageName.DocumentNameCode.ToString());
			UkcinvUnderstander understander = new UkcinvUnderstander(ukcinv);
			AssertEquals(UkcinvUnderstander.MessageClasses.DEC, understander.Class);
			AssertEquals("2GB945390992000-S00030113", understander.DUCRs[0]);
			AssertEquals("A:41305041341", understander.MUCRs[0]);
			AssertEquals("A:11102051202", understander.IntermediateMucrs[0]);
			AssertContains("Subordinate MUCR - A:11102051202 - 1 of 1", understander.GetInterpretation());
		}

		public void TestDEC_Response()
		{
			UkCinvMessage ukcinv = new UkCinvMessage();

			ukcinv.Parse(CharSet, decResponse);

			AssertEquals(Interrogate_DecMucr.FunctionCodeConst, ukcinv.BGM[0].DocumentMessageName.DocumentNameCode.ToString());
			AssertEquals("105", ukcinv.BGM[0].DocumentMessageName.CodeListIdentificationCode.ToString());
			AssertEquals("109", ukcinv.BGM[0].DocumentMessageName.CodeListResponsibleAgencyCode.ToString());

			AssertEquals("14", ukcinv.Group1[0].LOC[0].LocationFunctionCodeQualifier.ToString());
			AssertEquals("FXT", ukcinv.Group1[0].LOC[0].LocationIdentification.LocationNameCode);

			AssertEquals("ABO", ukcinv.Group1[0].RFF[0].Reference.ReferenceFunctionCodeQualifier.ToString());
			AssertEquals("decln-ucr", ukcinv.Group1[0].RFF[0].Reference.ReferenceIdentifier);
			AssertEquals("decln-part-no", ukcinv.Group1[0].RFF[0].Reference.DocumentLineIdentifier);

			AssertEquals("ICS", ukcinv.Group1[0].GEI[0].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("92", ukcinv.Group1[0].GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());
			AssertEquals("ROE", ukcinv.Group1[0].GEI[1].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("H", ukcinv.Group1[0].GEI[1].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());
			AssertEquals("TYP", ukcinv.Group1[0].GEI[2].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("N", ukcinv.Group1[0].GEI[2].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());

			AssertEquals("11", ukcinv.UNT[0].NumberOfSegmentsInTheMessage);

			UkcinvUnderstander understander = new UkcinvUnderstander(ukcinv);
			AssertEquals(UkcinvUnderstander.MessageClasses.DEC, understander.Class);
			AssertEquals("decln-ucr", understander.DUCRs[0]);
			AssertEquals("", understander.MUCRs[0]);  // Null, but no exception thrown.  We expect a mucr, but this test message happens to contain none. 
			AssertEquals(0, understander.IntermediateMucrs.Length);
		}

		public void TestEAA_Response()
		{
			UkCinvMessage ukcinv = new UkCinvMessage();
			ukcinv.Parse(CharSet, eaaResponse);

			AssertEquals("EAA", ukcinv.BGM[0].DocumentMessageName.DocumentNameCode.ToString());
			AssertEquals("105", ukcinv.BGM[0].DocumentMessageName.CodeListIdentificationCode.ToString());
			AssertEquals("109", ukcinv.BGM[0].DocumentMessageName.CodeListResponsibleAgencyCode.ToString());

			AssertEquals("CRC", ukcinv.GEI[0].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("000", ukcinv.GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());

			AssertEquals("14", ukcinv.LOC[0].LocationFunctionCodeQualifier.ToString());
			AssertEquals("gds-locn", ukcinv.LOC[0].LocationIdentification.LocationNameCode);
			AssertEquals("156", ukcinv.LOC[0].LocationIdentification.CodeListIdentificationCode.ToString());
			AssertEquals("109", ukcinv.LOC[0].LocationIdentification.CodeListResponsibleAgencyCode.ToString());
			AssertEquals("shed-op-id", ukcinv.LOC[0].LocationIdentification.LocationName);
			AssertEquals("109", ukcinv.LOC[0].LocationIdentification.CodeListResponsibleAgencyCode.ToString());
			AssertEquals("epu-no", ukcinv.LOC[0].RelatedLocationOneIdentification.FirstRelatedLocationNameCode);
			AssertEquals("eps-id", ukcinv.LOC[0].RelatedLocationOneIdentification.FirstRelatedLocationName);
			AssertEquals("14", ukcinv.Group1[0].LOC[0].LocationFunctionCodeQualifier.ToString());

			AssertEquals("ABO", ukcinv.Group1[0].RFF[0].Reference.ReferenceFunctionCodeQualifier.ToString());
			AssertEquals("decln-ucr", ukcinv.Group1[0].RFF[0].Reference.ReferenceIdentifier);
			AssertEquals("decln-part-no", ukcinv.Group1[0].RFF[0].Reference.DocumentLineIdentifier);

			AssertEquals("ICS", ukcinv.Group1[0].GEI[0].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("ics", ukcinv.Group1[0].GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());
			AssertEquals("SOE", ukcinv.Group1[0].GEI[1].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("soe", ukcinv.Group1[0].GEI[1].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());
			AssertEquals("ROE", ukcinv.Group1[0].GEI[2].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("roe", ukcinv.Group1[0].GEI[2].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());

			AssertEquals("11", ukcinv.Group1[0].CNT[0].Control.ControlTotalTypeCodeQualifier.ToString());
			AssertEquals("tot-pkgs", ukcinv.Group1[0].CNT[0].Control.ControlTotalValue);

			AssertEquals("AAR", ukcinv.Group1[0].MEA[0].MeasurementAttributeCode.ToString());
			AssertEquals("KGM", ukcinv.Group1[0].MEA[0].ValueRange.MeasurementUnitCode);
			AssertEquals("tot-net-mass", ukcinv.Group1[0].MEA[0].ValueRange.MeasurementValue);

			AssertEquals("122", ukcinv.Group1[0].CST[0].CustomsIdentityCodes1.CodeListIdentificationCode.ToString());
			AssertEquals("cmdty-code", ukcinv.Group1[0].CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier);

			AssertEquals("submit-role", ukcinv.Group1[0].AUT[0].ValidationResultValue);

			AssertEquals("17", ukcinv.UNT[0].NumberOfSegmentsInTheMessage);

			UkcinvUnderstander understander = new UkcinvUnderstander(ukcinv);
			AssertEquals(UkcinvUnderstander.MessageClasses.EAA, understander.Class);
			AssertEquals("decln-ucr", understander.DUCRs[0]);
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ string ignore = understander.MUCRs[0]; });
		}

		public void TestEAL_Response()
		{
			UkCinvMessage ukcinv = new UkCinvMessage();
			ukcinv.Parse(CharSet, ealResponse);

			AssertEquals("EAL", ukcinv.BGM[0].DocumentMessageName.DocumentNameCode.ToString());

			AssertEquals("CRC", ukcinv.GEI[0].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("000", ukcinv.GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());

			AssertEquals("14", ukcinv.LOC[0].LocationFunctionCodeQualifier.ToString());
			AssertEquals("gds-locn", ukcinv.LOC[0].LocationIdentification.LocationNameCode);
			AssertEquals("156", ukcinv.LOC[0].LocationIdentification.CodeListIdentificationCode.ToString());
			AssertEquals("109", ukcinv.LOC[0].LocationIdentification.CodeListResponsibleAgencyCode.ToString());
			AssertEquals("shed-op-id", ukcinv.LOC[0].LocationIdentification.LocationName);
			AssertEquals("109", ukcinv.LOC[0].LocationIdentification.CodeListResponsibleAgencyCode.ToString());
			AssertEquals("epu-no", ukcinv.LOC[0].RelatedLocationOneIdentification.FirstRelatedLocationNameCode);
			AssertEquals("eps-id", ukcinv.LOC[0].RelatedLocationOneIdentification.FirstRelatedLocationName);
			AssertEquals("14", ukcinv.Group1[0].LOC[0].LocationFunctionCodeQualifier.ToString());

			AssertEquals("ABO", ukcinv.Group1[0].RFF[0].Reference.ReferenceFunctionCodeQualifier.ToString());
			AssertEquals("decln-ucr", ukcinv.Group1[0].RFF[0].Reference.ReferenceIdentifier);
			AssertEquals("decln-part-no", ukcinv.Group1[0].RFF[0].Reference.DocumentLineIdentifier);

			AssertEquals("ICS", ukcinv.Group1[0].GEI[0].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("ics", ukcinv.Group1[0].GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());
			AssertEquals("SOE", ukcinv.Group1[0].GEI[1].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("soe", ukcinv.Group1[0].GEI[1].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());
			AssertEquals("ROE", ukcinv.Group1[0].GEI[2].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("roe", ukcinv.Group1[0].GEI[2].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());

			AssertEquals("11", ukcinv.Group1[0].CNT[0].Control.ControlTotalTypeCodeQualifier.ToString());
			AssertEquals("tot-pkgs", ukcinv.Group1[0].CNT[0].Control.ControlTotalValue);

			AssertEquals("AAR", ukcinv.Group1[0].MEA[0].MeasurementAttributeCode.ToString());
			AssertEquals("KGM", ukcinv.Group1[0].MEA[0].ValueRange.MeasurementUnitCode);
			AssertEquals("tot-net-mass", ukcinv.Group1[0].MEA[0].ValueRange.MeasurementValue);

			AssertEquals("122", ukcinv.Group1[0].CST[0].CustomsIdentityCodes1.CodeListIdentificationCode.ToString());
			AssertEquals("cmdty-code", ukcinv.Group1[0].CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier);

			AssertEquals("submit-role", ukcinv.Group1[0].AUT[0].ValidationResultValue);

			AssertEquals("18", ukcinv.UNT[0].NumberOfSegmentsInTheMessage);

			UkcinvUnderstander understander = new UkcinvUnderstander(ukcinv);
			AssertEquals(UkcinvUnderstander.MessageClasses.EAL, understander.Class);
			AssertEquals("decln-ucr", understander.DUCRs[0]);
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ string ignore = understander.MUCRs[0]; });
		}
	}

	public class UkCinvRequestTest : TestCase
	{
		readonly string eacRequest = @"UNH+292420+UKCINV:D:00A:UN:109001+2S30ZNGYW'BGM+EAC:105:109'RFF+ABO:9GB625512459000-RTES-001'UNS+D'UNS+S'UNT+6+292420'";
		readonly string eaaRequest = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'BGM+EAA:105:109'AUT+agent-role+agent-locn'GEI+OPT+master-opt'RFF+ABO:ucr:ucr-part-no'RFF+AES:movt-ref'LOC+14+gds-locn:156:109:shed-op-id'DTM+178:gds-arr-dtm:203'TDT+13++trpt-mode-code+++++:::trpt-id:trpt-cntry'UNS+D'UNS+S'UNT+12+sys-mrn'";
		//string cstRequest = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'BGM+CST:105:109'RFF+UCN:master-ucr'UNS+D'UNS+S'UNT+6+sys-mrn'";
		readonly string ealRequest = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'BGM+EAL:105:109'AUT+agent-role+agent-locn'GEI+OPT+master-opt'GEI+TRK+reports-rqd'RFF+ABO:ucr:ucr-part-no'RFF+AES:movt-ref'LOC+14+gds-locn:156:109:shed-op-id'DTM+178:gds-arr-dtm:203'TDT+13++trpt-mode-code+++++:::trpt-id:trpt-cntry'UNS+D'UNS+S'UNT+13+sys-mrn'";
		readonly string edlRequest = @"UNH+sys-mrn+UKCINV:D:00A:UN:109001'BGM+EDL:105:109'RFF+ABO:ucr:ucr-part-no'LOC+14+gds-locn:156:109:shed-op-id'DTM+189:gds-dep-dt:102'TDT+13++trpt-mode-code+++++:::trpt-id:trpt-cntry'UNS+D'UNS+S'UNT+9+sys-mrn'";

		// Requests: 
		public void TestEAC_request()
		{
			UkCinvMessage ukcinv = new UkCinvMessage();
			ukcinv.Parse(CharSet, eacRequest);

			AssertEquals("292420", ukcinv.UNH[0].MessageReferenceNumber);
			AssertEquals("UKCINV", ukcinv.UNH[0].MessageIdentifier.MessageType);
			AssertEquals("D", ukcinv.UNH[0].MessageIdentifier.MessageVersionNumber);
			AssertEquals("00A", ukcinv.UNH[0].MessageIdentifier.MessageReleaseNumber);
			AssertEquals("UN", ukcinv.UNH[0].MessageIdentifier.ControllingAgency);
			AssertEquals("109001", ukcinv.UNH[0].MessageIdentifier.AssociationAssignedCode);
			AssertEquals("2S30ZNGYW", ukcinv.UNH[0].CommonAccessReference);

			AssertEquals("EAC", ukcinv.BGM[0].DocumentMessageName.DocumentNameCode.ToString());
			AssertEquals("105", ukcinv.BGM[0].DocumentMessageName.CodeListIdentificationCode.ToString());
			AssertEquals("109", ukcinv.BGM[0].DocumentMessageName.CodeListResponsibleAgencyCode.ToString());

			AssertEquals("ABO", ukcinv.RFF1[0].Reference.ReferenceFunctionCodeQualifier.ToString());
			AssertEquals("9GB625512459000-RTES-001", ukcinv.RFF1[0].Reference.ReferenceIdentifier);

			AssertEquals("D", ukcinv.UNS1[0].SectionIdentification);
			AssertEquals("S", ukcinv.UNS2[0].SectionIdentification);
			AssertEquals("6", ukcinv.UNT[0].NumberOfSegmentsInTheMessage);
			AssertEquals("292420", ukcinv.UNT[0].MessageReferenceNumber);
		}

		public void TestEAA_request()
		{
			UkCinvMessage ukcinv = new UkCinvMessage();
			ukcinv.Parse(CharSet, eaaRequest);

			AssertEquals("sys-mrn", ukcinv.UNH[0].MessageReferenceNumber);
			AssertEquals("UKCINV", ukcinv.UNH[0].MessageIdentifier.MessageType);
			AssertEquals("D", ukcinv.UNH[0].MessageIdentifier.MessageVersionNumber);
			AssertEquals("00A", ukcinv.UNH[0].MessageIdentifier.MessageReleaseNumber);
			AssertEquals("UN", ukcinv.UNH[0].MessageIdentifier.ControllingAgency);
			AssertEquals("109001", ukcinv.UNH[0].MessageIdentifier.AssociationAssignedCode);

			AssertEquals("EAA", ukcinv.BGM[0].DocumentMessageName.DocumentNameCode.ToString());
			AssertEquals("105", ukcinv.BGM[0].DocumentMessageName.CodeListIdentificationCode.ToString());
			AssertEquals("109", ukcinv.BGM[0].DocumentMessageName.CodeListResponsibleAgencyCode.ToString());

			AssertEquals("agent-locn", ukcinv.AUT[0].ValidationKeyIdentifier);
			AssertEquals("agent-role", ukcinv.AUT[0].ValidationResultValue);

			AssertEquals("OPT", ukcinv.GEI[0].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("master-opt", ukcinv.GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());

			AssertEquals("ABO", ukcinv.RFF1[0].Reference.ReferenceFunctionCodeQualifier.ToString());
			AssertEquals("ucr", ukcinv.RFF1[0].Reference.ReferenceIdentifier);
			AssertEquals("ucr-part-no", ukcinv.RFF1[0].Reference.DocumentLineIdentifier);

			AssertEquals("AES", ukcinv.RFF1[1].Reference.ReferenceFunctionCodeQualifier.ToString());
			AssertEquals("movt-ref", ukcinv.RFF1[1].Reference.ReferenceIdentifier);

			AssertEquals("14", ukcinv.LOC[0].LocationFunctionCodeQualifier.ToString());
			AssertEquals("gds-locn", ukcinv.LOC[0].LocationIdentification.LocationNameCode);
			AssertEquals("156", ukcinv.LOC[0].LocationIdentification.CodeListIdentificationCode.ToString());
			AssertEquals("109", ukcinv.LOC[0].LocationIdentification.CodeListResponsibleAgencyCode.ToString());
			AssertEquals("shed-op-id", ukcinv.LOC[0].LocationIdentification.LocationName);

			AssertEquals("203", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodFormatCode.ToString());
			AssertEquals("178", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier.ToString());
			AssertEquals("gds-arr-dtm", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodValue);

			AssertEquals("203", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodFormatCode.ToString());
			AssertEquals("178", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier.ToString());
			AssertEquals("gds-arr-dtm", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodValue);

			AssertEquals("trpt-id", ukcinv.TDT[0].TransportIdentification.TransportMeansIdentificationName);
			AssertEquals("trpt-cntry", ukcinv.TDT[0].TransportIdentification.TransportMeansNationalityCode);
			AssertEquals("13", ukcinv.TDT[0].TransportStageCodeQualifier.ToString());
			AssertEquals("trpt-mode-code", ukcinv.TDT[0].ModeOfTransport.TransportModeNameCode);

			AssertEquals("D", ukcinv.UNS1[0].SectionIdentification);
			AssertEquals("S", ukcinv.UNS2[0].SectionIdentification);
			AssertEquals("12", ukcinv.UNT[0].NumberOfSegmentsInTheMessage);
			AssertEquals("sys-mrn", ukcinv.UNT[0].MessageReferenceNumber);

			AssertEquals(0, ukcinv.Group1.Count);
		}

		public void TestEAL_request()
		{
			UkCinvMessage ukcinv = new UkCinvMessage();
			ukcinv.Parse(CharSet, ealRequest);

			AssertEquals("TRK", ukcinv.GEI[1].ProcessingInformationCodeQualifier.ToString());
			AssertEquals("reports-rqd", ukcinv.GEI[1].ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString());

			AssertEquals("13", ukcinv.UNT[0].NumberOfSegmentsInTheMessage);
		}

		public void TestEDL_request()
		{
			UkCinvMessage ukcinv = new UkCinvMessage();
			ukcinv.Parse(CharSet, edlRequest);

			AssertEquals("102", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodFormatCode.ToString());
			AssertEquals("189", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier.ToString());
			AssertEquals("gds-dep-dt", ukcinv.DTM[0].DateTimePeriod.DateOrTimeOrPeriodValue);

			AssertEquals("9", ukcinv.UNT[0].NumberOfSegmentsInTheMessage);
		}

		static UNOACharacterSet CharSet
		{
			get
			{
				return new UkCharSet();
			}
		}
	}
}
