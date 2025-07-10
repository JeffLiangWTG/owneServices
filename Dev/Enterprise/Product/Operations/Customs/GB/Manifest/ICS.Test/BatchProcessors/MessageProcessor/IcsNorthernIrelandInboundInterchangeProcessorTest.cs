using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public class IcsNorthernIrelandInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestInboundInterchangeProcessorNI()
		{
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN12345";
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.ICS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			manifestHeader.AMA_GB = aaaBranch.PK;

			var outgoingMessage = Factory.New<IcsNorthernIrelandEDIMessage>();
			outgoingMessage.EM_ApplicationReference = "16763";
			outgoingMessage.EM_MessageText = "";
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "999";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_GB = aaaBranch.PK;
			Factory.Save();
			manifestHeader.Messages.Add(outgoingMessage);

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "1";
			outgoingInterchange.EI_From = "WISETECHGLOBAL";
			outgoingInterchange.EI_To = "ICS";
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbMessageICSNorthernIreland;
			incomingInterchange.EI_InterchangeNum = "00001";
			incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_BodyText = ICSGBCustomsBusinessResponse_InterchangeBody;

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new ICSInboundInterchangeProcessor(logger);
			processor.ExecuteBatch();

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbMessageICSNorthernIreland);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { incomingInterchange.PK });
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;

			var message = new BusinessObjectFactory().LoadTop1<EDIMessage>(query);

			AssertStartsWith("Message should be a CC351A xml", "<CC351A>", message.EM_MessageText);
			AssertEquals("Message.EM_ApplicationReference should be CorrelationID", "87491122139921", message.EM_ApplicationReference);
			AssertEquals("00001/87491122139921", message.EM_MessageNum);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("351", message.EM_MessageSubType);
		}

		string ICSGBCustomsBusinessResponse_InterchangeBody => $@"
<GBCustomsBusinessResponse>
    <ResponseHeader Provider='ICSNI'>
        <CorrelationID>87491122139921</CorrelationID>
        <eHubTrackingId>E9AC3350-FABD-4C8E-AB64-0DFB5973C459</eHubTrackingId>
    </ResponseHeader>
	<ResponseBody ContentType = ""XML"" Encoding=""none"">
		&lt;CC351A&gt;
		    &lt;MesSenMES3&gt;GBCD1234/1234567890&lt;/MesSenMES3&gt;
		    &lt;MesRecMES6&gt;GBC123&lt;/MesRecMES6&gt;
		    &lt;DatOfPreMES9&gt;030211&lt;/DatOfPreMES9&gt;
		    &lt;TimOfPreMES10&gt;0123&lt;/TimOfPreMES10&gt;
		    &lt;MesIdeMES19&gt;ABC123&lt;/MesIdeMES19&gt;
		    &lt;MesTypMES20&gt;CC313A&lt;/MesTypMES20&gt;
		    &lt;CorIdeMES25&gt;ABC123&lt;/CorIdeMES25&gt;
		    &lt;HEAHEA&gt;
		        &lt;RefNumHEA4&gt;ABCD1234&lt;/RefNumHEA4&gt;
		        &lt;DocNumHEA5&gt;12AB3C4D5E6F7G8H90&lt;/DocNumHEA5&gt;
		        &lt;TraModAtBorHEA76&gt;4&lt;/TraModAtBorHEA76&gt;
		        &lt;NatHEA001&gt;GB&lt;/NatHEA001&gt;
		        &lt;IdeOfMeaOfTraCroHEA85&gt;ABC123&lt;/IdeOfMeaOfTraCroHEA85&gt;
		        &lt;TotNumOfIteHEA305&gt;42&lt;/TotNumOfIteHEA305&gt;
		        &lt;ComRefNumHEA&gt;ABC123&lt;/ComRefNumHEA&gt;
		        &lt;ConRefNumHEA&gt;ABC123&lt;/ConRefNumHEA&gt;
		        &lt;NotDatTimHEA104&gt;200302111234&lt;/NotDatTimHEA104&gt;
		        &lt;DecRegDatTimHEA115&gt;200302111234&lt;/DecRegDatTimHEA115&gt;
		        &lt;DecSubDatTimHEA118&gt;200302111234&lt;/DecSubDatTimHEA118&gt;
		    &lt;/HEAHEA&gt;
		    &lt;GOOITEGDS&gt;
		        &lt;IteNumGDS7&gt;1&lt;/IteNumGDS7&gt;
		        &lt;ComRefNumGIM1&gt;ABC123&lt;/ComRefNumGIM1&gt;
		        &lt;PRODOCDC2&gt;
		            &lt;DocTypDC21&gt;AB12&lt;/DocTypDC21&gt;
		            &lt;DocRefDC23&gt;ABC123&lt;/DocRefDC23&gt;
		            &lt;DocRefDCLNG&gt;en&lt;/DocRefDCLNG&gt;
		        &lt;/PRODOCDC2&gt;
		        &lt;CONNR2&gt;
		            &lt;ConNumNR21&gt;ABC123&lt;/ConNumNR21&gt;
		        &lt;/CONNR2&gt;
		        &lt;IDEMEATRAGI970&gt;
		            &lt;NatIDEMEATRAGI973&gt;EN&lt;/NatIDEMEATRAGI973&gt;
		            &lt;IdeMeaTraGIMEATRA971&gt;ABC123&lt;/IdeMeaTraGIMEATRA971&gt;
		            &lt;IdeMeaTraGIMEATRA972LNG&gt;en&lt;/IdeMeaTraGIMEATRA972LNG&gt;
		        &lt;/IDEMEATRAGI970&gt;
		    &lt;/GOOITEGDS&gt;
		    &lt;CUSOFFLON&gt;
		        &lt;RefNumCOL1&gt;ES000055&lt;/RefNumCOL1&gt;
		    &lt;/CUSOFFLON&gt;
		    &lt;TRAREP&gt;
		        &lt;NamTRE1&gt;ABC123&lt;/NamTRE1&gt;
		        &lt;StrAndNumTRE1&gt;ABC123&lt;/StrAndNumTRE1&gt;
		        &lt;PosCodTRE1&gt;ABC123&lt;/PosCodTRE1&gt;
		        &lt;CitTRE1&gt;ABC123&lt;/CitTRE1&gt;
		        &lt;CouCodTRE1&gt;EN&lt;/CouCodTRE1&gt;
		        &lt;TRAREPLNG&gt;en&lt;/TRAREPLNG&gt;
		        &lt;TINTRE1&gt;YZ^&lt;/TINTRE1&gt;
		    &lt;/TRAREP&gt;
		    &lt;PERLODSUMDEC&gt;
		        &lt;NamPLD1&gt;ABC123&lt;/NamPLD1&gt;
		        &lt;StrAndNumPLD1&gt;ABC123&lt;/StrAndNumPLD1&gt;
		        &lt;PosCodPLD1&gt;ABC123&lt;/PosCodPLD1&gt;
		        &lt;CitPLD1&gt;ABC123&lt;/CitPLD1&gt;
		        &lt;CouCodPLD1&gt;EN&lt;/CouCodPLD1&gt;
		        &lt;PERLODSUMDECLNG&gt;en&lt;/PERLODSUMDECLNG&gt;
		        &lt;TINPLD1&gt;YZ^&lt;/TINPLD1&gt;
		    &lt;/PERLODSUMDEC&gt;
		    &lt;CUSOFFFENT730&gt;
		        &lt;RefNumCUSOFFFENT731&gt;AB3C4D5E&lt;/RefNumCUSOFFFENT731&gt;
		        &lt;ExpDatOfArrFIRENT733&gt;200302111234&lt;/ExpDatOfArrFIRENT733&gt;
		    &lt;/CUSOFFFENT730&gt;
		    &lt;TRACARENT601&gt;
		        &lt;NamTRACARENT604&gt;ABC123&lt;/NamTRACARENT604&gt;
		        &lt;StrNumTRACARENT607&gt;ABC123&lt;/StrNumTRACARENT607&gt;
		        &lt;PstCodTRACARENT606&gt;ABC123&lt;/PstCodTRACARENT606&gt;
		        &lt;CtyTRACARENT603&gt;ABC123&lt;/CtyTRACARENT603&gt;
		        &lt;CouCodTRACARENT605&gt;EN&lt;/CouCodTRACARENT605&gt;
		        &lt;TRACARENT601LNG&gt;en&lt;/TRACARENT601LNG&gt;
		        &lt;TINTRACARENT602&gt;YZ^&lt;/TINTRACARENT602&gt;
		    &lt;/TRACARENT601&gt;
		    &lt;CUSINT632&gt;
		        &lt;IteNumConCUSINT668&gt;1&lt;/IteNumConCUSINT668&gt;
		        &lt;CusIntCodCUSINT665&gt;ABCD&lt;/CusIntCodCUSINT665&gt;
		        &lt;CusIntTexCUSINT666&gt;ABC12&lt;/CusIntTexCUSINT666&gt;
		        &lt;CusIntTexCUSINT667LNG&gt;en&lt;/CusIntTexCUSINT667LNG&gt;
		    &lt;/CUSINT632&gt;
		&lt;/CC351A&gt;
	</ResponseBody>
</GBCustomsBusinessResponse>";
	}
}
