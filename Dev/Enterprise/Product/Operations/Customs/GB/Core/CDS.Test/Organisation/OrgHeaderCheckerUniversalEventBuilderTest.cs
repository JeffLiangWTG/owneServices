using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Organisation;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(OrgHeaderCheckerUniversalEventBuilder))]
	sealed class OrgHeaderCheckerUniversalEventBuilderTest : TestCaseWithFactory
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new OrgHeaderCheckerUniversalEventBuilder(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrgHeaderCheckerUniversalEventBuilder(Factory.New<OrgHeader>(), null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrgHeaderCheckerUniversalEventBuilder(null, Factory.New<OrgCusCode>()));
		}

		public void TestBuildUniversalEvent_WhenOrgCusCodeIsNotValid()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgCusCode = Factory.New<OrgCusCode>();
			var builder = new OrgHeaderCheckerUniversalEventBuilder(orgHeader, orgCusCode);
			AssertNull(builder.BuildUniversalEvent());
		}

		public void TestBuildUniversalEvent_VAT()
		{
			SetupCredentialsKey();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC123";
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345", Core.Constants.CountryCodes.UnitedKingdom);
			var builder = new OrgHeaderCheckerUniversalEventBuilder(orgHeader, orgCusCode);

			var universalEvent = builder.BuildUniversalEvent();
			AssertVATUniversalEvent(universalEvent, orgHeader, "12345");
		}

		public void TestBuildUniversalEvent_EORI()
		{
			SetupCredentialsKey();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC123";
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.UnitedKingdom);
			var builder = new OrgHeaderCheckerUniversalEventBuilder(orgHeader, orgCusCode);

			var universalEvent = builder.BuildUniversalEvent();
			AssertEORIUniversalEvent(universalEvent, orgHeader, "GB12345");
		}

		public void TestBuildUniversalEvent_EORI_NOP()
		{
			SetupCredentialsKey();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC123";
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI12345", Core.Constants.CountryCodes.UnitedKingdom);
			var builder = new OrgHeaderCheckerUniversalEventBuilder(orgHeader, orgCusCode);

			var universalEvent = builder.BuildUniversalEvent();
			AssertNOPUniversalEvent(universalEvent, orgHeader, "XI12345");
		}

		void AssertVATUniversalEvent(UniversalEvent universalEvent, OrgHeader orgHeader, string varNumber)
		{
			var xml = OrgHeaderCheckerUniversalEventBuilder.ConvertToXml(universalEvent);
			var contextCollectionXml = @$"      <Context>
        <Type>NotificationType</Type>
        <Value>VAT</Value>
      </Context>
      <Context>
        <Type>EntryNumber</Type>
        <Value>{varNumber}</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>HYECMT.GB999999999888.CDS</Value>
      </Context>";
			AssertMultilineASCIIEquals("VAT Universal Event", GetExpected(universalEvent, orgHeader, contextCollectionXml), ReplaceEventTime(xml, "2016-08-16T00:28:45.837"));
		}

		void AssertEORIUniversalEvent(UniversalEvent universalEvent, OrgHeader orgHeader, string eori)
		{
			var xml = OrgHeaderCheckerUniversalEventBuilder.ConvertToXml(universalEvent);
			var contextCollectionXml = @$"      <Context>
        <Type>NotificationType</Type>
        <Value>EORI</Value>
      </Context>
      <Context>
        <Type>Payload</Type>
        <Value>{{""eoris"":[""{eori}""]}}</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>HYECMT.GB999999999888.CDS</Value>
      </Context>";
			AssertMultilineASCIIEquals("EORI Universal Event", GetExpected(universalEvent, orgHeader, contextCollectionXml), ReplaceEventTime(xml, "2016-08-16T00:28:45.837"));
		}

		void AssertNOPUniversalEvent(UniversalEvent universalEvent, OrgHeader orgHeader, string eori)
		{
			var xml = OrgHeaderCheckerUniversalEventBuilder.ConvertToXml(universalEvent);
			var contextCollectionXml = @$"      <Context>
        <Type>NotificationType</Type>
        <Value>NOP</Value>
      </Context>
      <Context>
        <Type>Payload</Type>
        <Value>{{""eoris"":[""{eori}""]}}</Value>
      </Context>
      <Context>
        <Type>Key</Type>
        <Value>HYECMT.GB999999999888.CDS</Value>
      </Context>";
			AssertMultilineASCIIEquals("NOP Universal Event", GetExpected(universalEvent, orgHeader, contextCollectionXml), ReplaceEventTime(xml, "2016-08-16T00:28:45.837"));
		}

		string GetExpected(UniversalEvent universalEvent, OrgHeader orgHeader, string contextCollectionXml)
		{
			var company = GlbBranch.CurrentBranch.Company;
			var server = universalEvent.DataContext.GetEnterpriseServerAndCompanyIDs();
			return @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>Organization</Type>
          <Key>" + orgHeader.OH_Code + @"</Key>
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
    <EventType>SVR</EventType>
    <EventReference>|MST=QUERY|SER=GBCustomsCDS</EventReference>

    <ContextCollection>
" + contextCollectionXml + @"
    </ContextCollection>
  </Event>
</UniversalEvent>
";
		}

		string ReplaceEventTime(string text, string dateString)
		{
			var startTag = "<EventTime>";
			var endTag = "</EventTime>";
			var startPos = text.IndexOf(startTag);
			var endPos = text.IndexOf(endTag);

			return text.Substring(0, startPos + startTag.Length) + dateString + text.Substring(endPos, text.Length - endPos);
		}

		void SetupCredentialsKey()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.Badge = "CDS";
			password.EORI = "GB999999999888";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_PasswordType = PasswordTypesList.Codes.CDS;

			Factory.Save();
		}
	}
}
