using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public class CDSH7SupportingDocUniversalEventBuilderTest : TestCaseWithFactory
	{
		public void TestBuildUniversalEvent()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			CombineAssertions("Catch Error in case pre-send check were missed", () =>
			{
				var sendingParent = new UploadDocumentsSendingActionParent(manifestHeader);
				var sendingObject1 = sendingParent.SendingObjectsCollection[0].EDocsCollection.AddNew();
				sendingObject1.EDoc = eDoc1.UniqueKey;
				sendingObject1.DocumentType = "CCIV";
				var sendingObject2 = sendingParent.SendingObjectsCollection[0].EDocsCollection.AddNew();
				sendingObject2.EDoc = eDoc2.UniqueKey;
				sendingObject2.DocumentType = "MMCD";

				var sendingObjects = new[] { sendingObject1, sendingObject2 };

				var eventBuilder = sendingObject1.GetSupportingDocUniversalEventBuilder();
				AssertType<CDSH7SupportingDocUniversalEventBuilder>(eventBuilder);
				var universalEvents = eventBuilder.BuildUniversalEvent(sendingObjects).ToArray();
				try
				{
					AssertEquals(2, universalEvents.Length);
					AssertUniversalEvent(0, universalEvents[0],
	@"      <AttachedDocument>
        <FileName>Invoice.pdf</FileName>
        <ImageData>AA==</ImageData>

        <ContextCollection>
          <Context>
            <Type>DOCTYPE</Type>
            <Value>CCIV</Value>
          </Context>
          <Context>
            <Type>MIME</Type>
            <Value>application/pdf</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>");

					AssertUniversalEvent(1, universalEvents[1],
	@"      <AttachedDocument>
        <FileName>Invoice2.pdf</FileName>
        <ImageData>AAAAAA==</ImageData>

        <ContextCollection>
          <Context>
            <Type>DOCTYPE</Type>
            <Value>MMCD</Value>
          </Context>
          <Context>
            <Type>MIME</Type>
            <Value>application/pdf</Value>
          </Context>
        </ContextCollection>
      </AttachedDocument>");
				}
				finally
				{
					foreach (var universalEvent in universalEvents)
					{
						universalEvent.Dispose();
					}
				}
			});
		}

		void AssertUniversalEvent(int index, UniversalEvent universalEvent, ZString singleAttachedDocumentXNodeString)
		{
			string xml;

			using (var stream = (SubStreamableStream)new MemoryStream())
			using (var reader = new StreamReader(stream))
			{
				new XmlWriter().WriteXML(universalEvent, stream, false);
				stream.Flush();
				stream.Position = 0;
				xml = reader.ReadToEnd();
			}

			AssertMultilineASCIIEquals("Universal Event - " + index, ExpectedResult(universalEvent, manifestHeader, singleAttachedDocumentXNodeString), ReplaceEventTime(xml, "2016-08-16T00:28:45.837"));
		}

		string ReplaceEventTime(string text, string dateString)
		{
			string startTag = "<EventTime>";
			string endTag = "</EventTime>";
			int startPos = text.IndexOf(startTag);
			int endPos = text.IndexOf(endTag);

			return text.Substring(0, startPos + startTag.Length) + dateString + text.Substring(endPos, text.Length - endPos);
		}

		string ExpectedResult(UniversalEvent universalEvent, AsycudaManifestHeader manifestHeader, ZString singleAttachedDocumentXNodeString)
		{
			var company = manifestHeader.Branch.Company;
			var server = universalEvent.DataContext.GetEnterpriseServerAndCompanyIDs();
			return @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AsycudaBill</Type>
          <Key>Bill1</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>" + company.GC_Code + @"</Code>
        <Country>
          <Code>" + company.Country.RN_Code + @"</Code>
          <Name>" + company.Country.RN_Desc + @"</Name>
        </Country>
        <Name>" + company.GC_Name + @"</Name>
      </Company>
      <DataProvider>" + universalEvent.DataContext.DataProviderForCodeMapping + @"</DataProvider>
      <EnterpriseID>" + server.EnterpriseID + @"</EnterpriseID>
      <ServerID>" + server.ServerID + @"</ServerID>
    </DataContext>

    <EventTime>2016-08-16T00:28:45.837</EventTime>
    <EventType>DSN</EventType>
    <EventReference>|MST=DOCUPLOAD|SER=GBCustomsCDS</EventReference>

    <AttachedDocumentCollection>
" + singleAttachedDocumentXNodeString + @"
    </AttachedDocumentCollection>

    <ContextCollection>
      <Context>
        <Type>MRN</Type>
        <Value>12341234</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>HYECMT.GB999999999888.ABC</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_CustomsProfile = "ABC";
			manifestHeader.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888");
			asycudaBill = manifestHeader.Bills.AddNew();
			asycudaBill.ABL_BillNumber = "Bill1";
			asycudaBill.MovementReferenceNumber = "12341234";
			byte[] imageBytes = new byte[1];
			byte[] imageBytes2 = new byte[4];
			eDoc1 = asycudaBill.DocManagerInfo().AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");
			eDoc2 = asycudaBill.DocManagerInfo().AddFileOrDocument(imageBytes2, "Invoice2.pdf", "MCD");

			Factory.Save();
		}

		AsycudaManifestHeader manifestHeader;
		AsycudaBill asycudaBill;
		IeDoc eDoc1;
		IeDoc eDoc2;

		#endregion
	}
}
