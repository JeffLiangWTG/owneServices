using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

public class EComRcvRequestPrettyFormatterTest : EComBaseRequestPrettyFormatterTest
{
	public static string MessageText = $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<edecComplaintRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns=""http://www.e-dec.ch/xml/schema/edecComplaintRequest/v1""
xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecComplaintRequest/v1
http://www.ezv.admin.ch/pdf_linker.php?doc=edecComplaintRequest_v_1_0"" schemaVersion=""1.0"">
<requestorTraderIdentificationNumber>CHE123456789</requestorTraderIdentificationNumber>
   <requestDate>2015-06-08</requestDate>
   <requestTime>15:01:05</requestTime>
   <customsOfficer>Zollfachperson Name</customsOfficer>
   <customsDeclarationNumber>14CHEI000000358095</customsDeclarationNumber>
   <correctionReason>2</correctionReason>
   <attachedDeclaration>0</attachedDeclaration>
   <appealText>Diese beanstandete Zollanmeldung wurde bis zum heutigen Tag noch nicht korrigiert...</appealText>
  <paperCorrespondence>Antrag/Beschwerde muss in schriftlicher Form eingereicht werden.</paperCorrespondence>
   <complaints>
      <complaint>
         <location>Header</location>
         <elementName>correctionTerm</elementName>
         <remark>15.06.2015</remark>
      </complaint>
      <complaint>
         <location>Header</location>
         <elementName>VATNumber</elementName>
         <remark>falsch</remark>
      </complaint>
   </complaints>
</edecComplaintRequest>";

	protected override ZString GetEM_MessageText() => MessageText;

	protected override ZString FormattedText() => $@"<h2>eCom Request by Customs</h2>
<p>Correction Reason: 2 - Formelle Überprüfung</p>
<p>Attached Declaration: No</p>
<h3>Customs</h3>
<table border=""0"">
<tr><td>Customs Officer:</td><td>Zollfachperson Name</td></tr>
<tr><td>Reminder:</td><td>Diese beanstandete Zollanmeldung wurde bis zum heutigen Tag noch nicht korrigiert...</td></tr>
<tr><td>Paper Correspondence:</td><td>Antrag/Beschwerde muss in schriftlicher Form eingereicht werden.</td></tr>
</table>
<h3>eCom Lines</h3>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<thead><tr class=""tableheadings""><th>Location</th><th>Entry Line #</th><th>Field Name</th><th>Remark</th></tr></thead>
<tr><td>Header</td><td>&nbsp;</td><td>Berichtigungsfrist</td><td>15.06.2015</td></tr>
<tr><td>Header</td><td>&nbsp;</td><td>Mehrwertsteuernummer</td><td>falsch</td></tr>
</table>";

	protected override bool IsTransmitMessage() => false;
}
