using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSQueryUniversalEventBuilderTest : TestCaseWithFactory
	{
		[TestDate(2020, 02, 10, 13, 14, 15, 678)]
		public void TestBuildUniversalEvent()
		{
			var sendingObject = new CDSQueryDUCRSendingObject(declaration);

			var builder = new CDSQueryUniversalEventBuilder(sendingObject);
			using (var universalEvent = builder.BuildUniversalEvent(ZGuid.Empty))
			{
				AssertUniversalEvent(universalEvent, expectedDUCR.Replace("<<DeclarationReference>>", declaration.JE_DeclarationReference));
			}
		}

		[TestDate(2020, 02, 10, 13, 14, 15, 678)]
		public void TestBuildUniversalEventForListQuery()
		{
			var msg = Factory.New<CDSDISQueryMessage>();
			msg.EM_MessageOwner = "12345678901234.XYZ";

			var sendingObject = new CDSQueryListSendingObject(msg);

			var builder = new CDSQueryUniversalEventBuilder(sendingObject);
			using (var universalEvent = builder.BuildUniversalEvent(ZGuid.Empty))
			{
				AssertUniversalEvent(universalEvent, expectedList);
			}
		}

		void AssertUniversalEvent(UniversalEvent universalEvent, string expectedContent)
		{
			var xml = CDSQueryUniversalEventBuilder.ConvertToXml(universalEvent);

			AssertMultilineASCIIEquals("Universal Event Expected", expectedContent, xml);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "BOB";
			registrationKey.ServerCodeForTest = "CAT";

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_CustomsProfile = "FAN";
			_ = declaration.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB132435465768");

			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "H1";

			var inv1 = declaration.Invoices.AddNew();
			var line1 = inv1.InvoiceLines.AddNew();
			line1.JI_CEI = cei1.PK;

			var doc = declaration.PreviousDocuments.AddNew();
			doc.CSI_Code = "DCR";
			doc.CSI_ReferenceNumber = "UNITTEST/00001";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			_ = declaration.DoMerge();
			Factory.Save();
		}

		JobDeclaration declaration;

		const string expectedDUCR = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CustomsDeclaration</Type>
          <Key><<DeclarationReference>></Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>GB</Code>
          <Name>United Kingdom</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>BOBCATEDI</DataProvider>
      <EnterpriseID>BOB</EnterpriseID>
      <ServerID>CAT</ServerID>
    </DataContext>

    <EventTime>2020-02-10T13:14:15.678+00:00</EventTime>
    <EventType>SVR</EventType>
    <EventReference>|MST=QUERY|SER=GBCustomsCDS</EventReference>

    <ContextCollection>
      <Context>
        <Type>EntryNumberType</Type>
        <Value>DUCR</Value>
      </Context>
      <Context>
        <Type>EntryNumber</Type>
        <Value>UNITTEST/00001</Value>
      </Context>
      <Context>
        <Type>NotificationType</Type>
        <Value>status</Value>
      </Context>
      <Context>
        <Type>QueryString</Type>
        <Value>partyRole=submitter</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>BOBCAT.GB132435465768.FAN</Value>
      </Context>

    </ContextCollection>
  </Event>
</UniversalEvent>
";

		const string expectedList = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>GB</Code>
          <Name>United Kingdom</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>BOBCATEDI</DataProvider>
      <EnterpriseID>BOB</EnterpriseID>
      <ServerID>CAT</ServerID>
    </DataContext>

    <EventTime>2020-02-10T13:14:15.678+00:00</EventTime>
    <EventType>SVR</EventType>
    <EventReference>|MST=QUERY|SER=GBCustomsCDS</EventReference>

    <ContextCollection>
      <Context>
        <Type>NotificationType</Type>
        <Value>list</Value>
      </Context>
      <Context>
        <Type>QueryString</Type>
        <Value>partyRole=submitter&amp;declarationCategory=ALL&amp;declarationStatus=Uncleared&amp;dateFrom=2020-02-03&amp;dateTo=2020-02-10&amp;pageNumber=1</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>BOBCAT.12345678901234.XYZ</Value>
      </Context>

    </ContextCollection>
  </Event>
</UniversalEvent>
";
	}
}
