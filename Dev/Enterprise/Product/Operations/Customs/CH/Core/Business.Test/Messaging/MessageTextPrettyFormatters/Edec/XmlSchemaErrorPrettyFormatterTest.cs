using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class XmlSchemaErrorPrettyFormatterTest : TestCase
{
	ZString ResponseText => $@"<?xml version= ""1.0"" encoding= ""UTF-8""?>
       <goodsDeclarationsResponse schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"" >
        <goodsDeclarationRejection>
          <rejectionDate>2021-11-26</rejectionDate>
            <rejectionTime>08:31:45</rejectionTime>
            <errors>
            <XMLSchemaErrors>
              <schema>
                <location>http://www.ezv.admin.ch/pdf_linker.php?doc=edec_v_4_0</location>
                  <namespace>http://www.e-dec.ch/xml/schema/edec/v4</namespace>
                  <version>4.0</version>
                  </schema>
                <parser>
                <name>Xerces-J 2.12.0</name>
                  </parser>
                <error>
                <message>{MessageDescription}</message>
                  </error>
                </XMLSchemaErrors>
              </errors>
            </goodsDeclarationRejection>
          </goodsDeclarationsResponse>";

	static readonly ZString MessageDescription = "Message description";
	readonly ZString expectedMessageInterpretation = $"<b>Schema Errors</b><br><br>{MessageDescription}";

	public void TestGetFormattedText()
	{
		var responseDetail = (IXMLSchemaErrorsResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail;
		var interpretedMessage = new XMLSchemaErrorPrettyFormatter(responseDetail).GetFormattedText();
		HtmlAssertEquals("EM_MessageInterpretation", expectedMessageInterpretation, interpretedMessage, isHtmlMessage: true);
	}
}
