using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSDeclarationSearchResponseEDIMessagePrettierTests : TestCaseWithFactory
	{
		public void TestMakeHumanReadable()
		{
			var message = Factory.New<CDSDeclarationInfoResponseEDIMessage>();
			message.EM_MessageText = System.FormattableString.Invariant($@"<p:DeclarationSearchResponse xsi:schemaLocation=""http://gov.uk/customs/declarationInformationRetrieval/declarationSummary/v1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/declarationInformationRetrieval/declarationSummary/v1"">
    <p:DeclarationSearchDetails>
        <p:Declaration>
            <p:ID>23GBB7KF5523KMKAR1</p:ID>
            <p:ReceivedDateTime>
                <p:DateTimeString formatCode=""304"">20231010102247Z</p:DateTimeString>
            </p:ReceivedDateTime>
            <p:ROE>H</p:ROE>
            <p:ICS>14</p:ICS>
            <p:LRN>HYEDUKCM20000000003421</p:LRN>
        </p:Declaration>
        <p2:Declaration>
            <p2:FunctionCode>9</p2:FunctionCode>
            <p2:TypeCode>IMD</p2:TypeCode>
            <p2:Submitter>
                <p2:ID>GB048834222514</p2:ID>
            </p2:Submitter>
            <p2:Declarant>
                <p2:ID>GB896458895015</p2:ID>
            </p2:Declarant>
            <p2:GoodsShipment>
                <p2:Consignment>
                    <p2:GoodsLocation>
                        <p2:Name>ABDABDABM</p2:Name>
                        <p2:TypeCode>A</p2:TypeCode>
                        <p2:Address>
                            <p2:TypeCode>U</p2:TypeCode>
                            <p2:CountryCode>GB</p2:CountryCode>
                        </p2:Address>
                    </p2:GoodsLocation>
                </p2:Consignment>
                <p2:Importer>
                    <p2:ID>GB896458895015</p2:ID>
                </p2:Importer>
            </p2:GoodsShipment>
        </p2:Declaration>
    </p:DeclarationSearchDetails>
    <p:DeclarationSearchDetails>
        <p:Declaration>
            <p:ID>23GBB7K6WRBIPQDAR8</p:ID>
            <p:ReceivedDateTime>
                <p:DateTimeString formatCode=""304"">20231010101623Z</p:DateTimeString>
            </p:ReceivedDateTime>
            <p:ROE>H</p:ROE>
            <p:ICS>14</p:ICS>
            <p:LRN>HYEDUKCM20000000003420</p:LRN>
        </p:Declaration>
        <p2:Declaration>
            <p2:FunctionCode>9</p2:FunctionCode>
            <p2:TypeCode>IMD</p2:TypeCode>
            <p2:Submitter>
                <p2:ID>GB048834222514</p2:ID>
            </p2:Submitter>
            <p2:Declarant>
                <p2:ID>GB896458895015</p2:ID>
            </p2:Declarant>
            <p2:GoodsShipment>
                <p2:Consignment>
                    <p2:GoodsLocation>
                        <p2:Name>ABDABDABM</p2:Name>
                        <p2:TypeCode>A</p2:TypeCode>
                        <p2:Address>
                            <p2:TypeCode>U</p2:TypeCode>
                            <p2:CountryCode>GB</p2:CountryCode>
                        </p2:Address>
                    </p2:GoodsLocation>
                </p2:Consignment>
                <p2:Importer>
                    <p2:ID>GB896458895015</p2:ID>
                </p2:Importer>
            </p2:GoodsShipment>
        </p2:Declaration>
    </p:DeclarationSearchDetails>
    <p:CurrentPageNumber>1</p:CurrentPageNumber>
    <p:TotalResultsAvailable>2</p:TotalResultsAvailable>
    <p:TotalPagesAvailable>1</p:TotalPagesAvailable>
    <p:NoResultsReturned>false</p:NoResultsReturned>
</p:DeclarationSearchResponse>");

			//Test HTML has elements in particular the "Goods released date time", ROE, ICS and ICR
			AssertEquals(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}table, tr, td {border: none;}hr.rounded {border-top: 4px solid #bbb;border-radius: 5px;</style><H4>Page 1 of 1: 2 results</H4><p><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td width=""33%"">MRN: 23GBB7KF5523KMKAR1</td><td width=""33%"">LRN: HYEDUKCM20000000003421</td><td width=""33%"">Date: 10-Oct-23 10:22:47</td></tr><tr><td>Status: route H, ICS 14</td><td>Type: IMD</td><td>Location: GBAUABDABDABM</td></tr><tr><td>Importer: GB896458895015</td><td>Declarant: GB896458895015</td><td>Submitter: GB048834222514</td></tr></table></p><hr class=""rounded""><p><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td width=""33%"">MRN: 23GBB7K6WRBIPQDAR8</td><td width=""33%"">LRN: HYEDUKCM20000000003420</td><td width=""33%"">Date: 10-Oct-23 10:16:23</td></tr><tr><td>Status: route H, ICS 14</td><td>Type: IMD</td><td>Location: GBAUABDABDABM</td></tr><tr><td>Importer: GB896458895015</td><td>Declarant: GB896458895015</td><td>Submitter: GB048834222514</td></tr></table></p><hr class=""rounded"">", message.EM_MessageInterpretation);
		}
	}
}
