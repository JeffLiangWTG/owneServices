using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class EComTrxRequestPrettyFormatterTest : EComBaseRequestPrettyFormatterTest
{
	public static string MessageText = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<edecComplaintRequest xsi:schemaLocation=""http://www.edec.ch/xml/schema/edecComplaintRequest/v1
http://www.ezv.admin.ch/pdf_linker.php?doc=edecComplaintRequest_v_1_0"" schemaVersion=""1.0""
xmlns=""http://www.e-dec.ch/xml/schema/edecComplaintRequest/v1""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <requestorTraderIdentificationNumber>CHE123456789</requestorTraderIdentificationNumber>
   <requestorCorrelationID>MyId</requestorCorrelationID>
   <requestDate>2015-03-17</requestDate>
   <requestTime>15:00:00</requestTime>
   <customsOfficer>Zollfachperson Name</customsOfficer>
   <customsDeclarationNumber>14CHEI000000123456</customsDeclarationNumber>
   <correctionReason>{RefCusCodeTestHelper.ValidCorrectionReasonTypeCode}</correctionReason>
   <attachedDeclaration>0</attachedDeclaration>
   <complaints>
      <complaint>
         <location>Header</location>
         <elementName>VATNumber</elementName>
         <remark>Angaben sind falsch</remark>
      </complaint>
      <complaint>
         <location>Position</location>
         <traderItemID>1</traderItemID>
         <elementName>statisticalValue</elementName>
         <remark>Wert zu gross</remark>
      </complaint>
   </complaints>
</edecComplaintRequest>";

	protected override ZString GetEM_MessageText() => MessageText;

	protected override ZString FormattedText() => $@"<h2>eCom Request</h2>
<p>Correction Reason: {RefCusCodeTestHelper.ValidCorrectionReasonTypeCode} - {nameof(RefCusCodeTestHelper.ValidCorrectionReasonTypeCode)}</p>
<p>Attached Declaration: No</p>
<h3>eCom Lines</h3>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead>
<tr class=""tableheadings"">
<th>Location</th><th>Entry Line #</th><th>Field Name</th><th>Remark</th>
</tr></thead>
<tr><td>Header</td><td>&nbsp;</td><td>Mehrwertsteuernummer</td><td>Angaben sind falsch</td></tr>
<tr><td>Line</td><td>1</td><td>Statistischer Wert</td><td>Wert zu gross</td></tr>
</table>";

	protected override bool IsTransmitMessage() => true;
}
