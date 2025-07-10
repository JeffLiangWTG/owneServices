using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	[UseSnapshotProtection]
	public class RefVesselScreeningStatusPostUpdateTest : TestCase
	{
		public void TestVesselScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsNotScreened()
		{
			AssertVesselScreeningStatusShouldBeNotWhenFirstImport(ScreeningStatusesList.Codes.NotScreened);
		}

		public void TestVesselScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsUnknown()
		{
			AssertVesselScreeningStatusShouldBeNotWhenFirstImport(ScreeningStatusesList.Codes.Unknown);
		}

		public void TestVesselScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsMatched()
		{
			AssertVesselScreeningStatusShouldBeNotWhenFirstImport(ScreeningStatusesList.Codes.Matched);
		}

		public void TestVesselScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsClear()
		{
			AssertVesselScreeningStatusShouldBeNotWhenFirstImport(ScreeningStatusesList.Codes.Clear);
		}

		public void TestVesselScreeningStatusShouldBeNotScreened_WhenFirstImportAndStatusIsPermanentClear()
		{
			AssertVesselScreeningStatusShouldBeNotWhenFirstImport(ScreeningStatusesList.Codes.PermanentClear);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenImportScreeningStatusIsDifferent_AndStatusIsMatched()
		{
			AssertVesselScreeningStatusShouldNotBeChanged_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Matched);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenImportScreeningStatusIsDifferent_AndStatusIsNot()
		{
			AssertVesselScreeningStatusShouldNotBeChanged_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.NotScreened);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenImportScreeningStatusIsDifferent_AndStatusIsUnknown()
		{
			AssertVesselScreeningStatusShouldNotBeChanged_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.Unknown);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenImportScreeningStatusIsDifferent_AndStatusIsPermanentClear()
		{
			AssertVesselScreeningStatusShouldNotBeChanged_WhenImportAndStatusIsDifferent(ScreeningStatusesList.Codes.PermanentClear);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenImportScreeningStatusIsDifferent_AndStatusIsUnDefined()
		{
			AssertVesselScreeningStatusShouldNotBeChanged_WhenImportAndStatusIsDifferent("123");
		}

		public void TestVesselScreeningStatus_FromMatchedToUnknown_WhenCodeInfoHasChanges()
		{
			var newCode = "newCode";
			AssertNotEquals(RefVesselCode, newCode);

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Matched, true);
			nativeXml = nativeXml.Replace(RefVesselCode, newCode);
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, newCode);
			AssertEquals("Screening status should be UNK", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertHasLogs(vessel, ScreeningStatusesList.Codes.Matched);
		}

		public void TestVesselScreeningStatus_FromClearToUnknown_WhenRadioCallSignInfoHasChanges()
		{
			var newRefVesselRadioCallSign = "2AXH3";
			AssertNotEquals(RefVesselRadioCallSign, newRefVesselRadioCallSign);

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Clear, true);
			nativeXml = nativeXml.Replace(RefVesselRadioCallSign, newRefVesselRadioCallSign);
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			AssertEquals("Screening status should be UNK", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertHasLogs(vessel, ScreeningStatusesList.Codes.Clear);
		}

		public void TestVesselScreeningStatus_FromMatchedToUnknown_WhenLloydsNumberInfoHasChanges()
		{
			var newRefVesselLloydsNumber = "9204792";
			AssertNotEquals(RefVesselLloydsNumber, newRefVesselLloydsNumber);

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Matched, true);
			nativeXml = nativeXml.Replace(RefVesselLloydsNumber, newRefVesselLloydsNumber);
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			AssertEquals("Screening status should be UNK", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertHasLogs(vessel, ScreeningStatusesList.Codes.Matched);
		}

		public void TestVesselScreeningStatus_FromClearToUnknown_WhenActiveInfoHasChanges()
		{
			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Clear, true);
			nativeXml = nativeXml.Replace("<IsActive>true</IsActive>", "<IsActive>false</IsActive>");
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			AssertEquals("Screening status should be UNK", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertHasLogs(vessel, ScreeningStatusesList.Codes.Clear);
		}

		public void TestVesselScreeningStatus_FromMatchedToUnknown_WhenCountryOfRegInfoHasChanges()
		{
			var newCountry = factory.NewWithValidTestData<RefCountry>();
			newCountry.RN_Code = "8Z";
			factory.Save();

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Matched, true);

			AssertNotEquals(refVesselCountry.Code, newCountry.Code);

			nativeXml = nativeXml.Replace($"<Code>{refVesselCountry.Code}</Code>", $"<Code>{newCountry.Code}</Code>");
			nativeXml = nativeXml.Replace(refVesselCountry.PK.ToString(), newCountry.PK.ToString());
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			AssertEquals("Screening status should be UNK", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertHasLogs(vessel, ScreeningStatusesList.Codes.Matched);
		}

		public void TestVesselScreeningStatus_FromClearToUnknown_WhenShippingProviderChangedAndScreenStatusNotSame()
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Clear, true);

			AssertNotEquals(refVesselShippingProvider.OH_Code, header.OH_Code);
			AssertNotEquals(refVesselShippingProvider.OH_ScreeningStatus, header.OH_ScreeningStatus);

			nativeXml = nativeXml.Replace($"<Code>{refVesselShippingProvider.OH_Code}</Code>", $"<Code>{header.OH_Code}</Code>");
			nativeXml = nativeXml.Replace(refVesselShippingProvider.PK.ToString(), header.PK.ToString());
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			AssertEquals("Screening status should be UNK", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertHasLogs(vessel, ScreeningStatusesList.Codes.Clear);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenPropertyHasChange_ButOriginalStatusIsPermanentClear()
		{
			var newCode = "newCode";
			AssertNotEquals(RefVesselCode, newCode);

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.PermanentClear, true);
			nativeXml = nativeXml.Replace(RefVesselCode, newCode);
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, newCode);
			AssertEquals("Screening status should be PermanentClear", ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);
			AssertNoLogs(vessel);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenPropertyHasChange_ButOriginalStatusIsNot()
		{
			var newCode = "newCode";
			AssertNotEquals(RefVesselCode, newCode);

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.NotScreened, true);
			nativeXml = nativeXml.Replace(RefVesselCode, newCode);
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, newCode);
			AssertEquals("Screening status should be NotScreened", ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);
			AssertNoLogs(vessel);
		}

		public void TestVesselScreeningStatusShouldNotBeChanged_WhenPropertyHasChange_ButOriginalStatusIsUnKnown()
		{
			var newCode = "newCode";
			AssertNotEquals(RefVesselCode, newCode);

			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Unknown, true);
			nativeXml = nativeXml.Replace(RefVesselCode, newCode);
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, newCode);
			AssertEquals("Screening status should be Unknown", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			AssertNoLogs(vessel);
		}

		#region Implementation

		const string RefVesselCode = "DODGY VESSELS INC";
		const string RefVesselRadioCallSign = "2AXH2";
		const string RefVesselLloydsNumber = "9204791";
		RefCountry refVesselCountry;
		OrgHeader refVesselShippingProvider;
		readonly BusinessObjectFactory factory = new BusinessObjectFactory();

		void AssertVesselScreeningStatusShouldNotBeChanged_WhenImportAndStatusIsDifferent(string screeningStatus)
		{
			var nativeXml = CreateNativeXmlVesselStringFull(ScreeningStatusesList.Codes.Clear, true);
			nativeXml = nativeXml.Replace("<ScreeningStatus>CLR</ScreeningStatus>", $"<ScreeningStatus>{screeningStatus}</ScreeningStatus>");
			Import(nativeXml);

			var vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			AssertEquals("Screening status should be CLR", ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
		}

		void AssertVesselScreeningStatusShouldBeNotWhenFirstImport(string screeningStatus)
		{
			var vessel = factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			AssertNull("Vessel should be null", vessel);

			var nativeXml = CreateNativeXmlVesselStringFull(screeningStatus, false);
			Import(nativeXml);

			vessel = new BusinessObjectFactory().LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);
			Assert("Vessel should be in the database after importing", vessel.IsInDatabase);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, vessel.RV_ScreeningStatus);
			AssertNoLogs(vessel);
		}

		void AssertNoLogs(RefVessel vessel)
		{
			var stmALogRows = vessel.Logs.GetAllLogs();
			AssertEquals(0, stmALogRows.Count(x => x["SL_Reference"].ToString().StartsWith("Screening status from")));

			var logCollection = vessel.ScreeningLogCollection;
			AssertEquals(0, logCollection.Count);
		}

		void AssertHasLogs(RefVessel vessel, string previousScreeningStatus)
		{
			var stmALogRows = vessel.Logs.GetAllLogs();
			AssertEquals(1, stmALogRows.Count(x => x["SL_SE_NKEvent"].ToString() == AutoEvents.StatusChangeCode));

			var logCollection = vessel.ScreeningLogCollection;
			AssertEquals("RefVessel StmEntityScreeningLog count", 1, logCollection.Count);

			var screeningLog = logCollection[0] as StmEntityScreeningLog;
			AssertNotNull(screeningLog);

			CombineAssertions(() =>
			{
				AssertEquals("ScreeningStatus change", string.Empty, screeningLog.PJ_MatchingData.ToString());
				AssertEquals(vessel.PK, screeningLog.PJ_ParentID);
				AssertEquals(vessel.TablePrefix, screeningLog.PJ_ParentTableCode);
				AssertEquals(vessel.PK, screeningLog.PJ_SourceID);
				AssertEquals(vessel.TablePrefix, screeningLog.PJ_SourceTableCode);
				AssertEquals(false, screeningLog.PJ_IsForcedRescreen);
			});

			var currentTimeUTc = ZDateTime.UtcNow;

			CombineAssertions(() =>
			{
				AssertEquals(DateTimeKind.Utc, screeningLog.PJ_SystemCreateTimeUtc.Kind);
				AssertDateTime(currentTimeUTc, screeningLog.PJ_SystemCreateTimeUtc);
			});
		}

		void AssertDateTime(ZDateTime expectedDay, ZDateTime actualDay)
		{
			Assert((expectedDay - actualDay).TotalHours <= 1);
		}

		void Import(string nativeXml)
		{
			var xmlBytes = Encoding.Default.GetBytes(nativeXml);
			var manager = new ImportHandler(new AncillaryImportServices());
			manager.Import(new MemoryStream(xmlBytes));
		}

		string CreateNativeXmlVesselStringFull(string vesselScreeningStatus, bool saveVesselToDataBase)
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "ForVessel";
			refVesselShippingProvider = header;

			var country = factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "9K";
			factory.Save();
			refVesselCountry = country;

			header.OH_ScreeningStatus = vesselScreeningStatus;
			factory.Save();

			var vessel = factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = RefVesselCode;
			vessel.RV_LloydsNumber = RefVesselLloydsNumber;
			vessel.RV_RadioCallSign = RefVesselRadioCallSign;
			vessel.RV_IsActive = true;
			vessel.RV_OH = header.PK;
			vessel.RV_RN_NKCountryOfReg = country.Code;
			vessel.RV_ScreeningStatus = vesselScreeningStatus;

			if (saveVesselToDataBase)
			{
				factory.Save();
			}

			vessel = factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, RefVesselCode);

			CombineAssertions(() =>
			{
				AssertEquals(RefVesselCode, vessel.RV_Code);
				AssertEquals("9204791", vessel.RV_LloydsNumber);
				AssertEquals("2AXH2", vessel.RV_RadioCallSign);
				AssertEquals(true, vessel.RV_IsActive);
				AssertEquals(vesselScreeningStatus, vessel.RV_ScreeningStatus);
				AssertEquals(vesselScreeningStatus, header.OH_ScreeningStatus);
			});

			return string.Format(CultureInfo.InvariantCulture, fullVesselInformationXml
				, vessel.PK
				, vessel.RV_Code
				, vessel.RV_LloydsNumber
				, vessel.RV_RadioCallSign
				, vessel.RV_IsActive ? "true" : "false"
				, vessel.RV_ScreeningStatus
				, header.OH_Code
				, header.PK
				, country.Code
				, country.PK);
		}

		const string fullVesselInformationXml = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>CARGOWSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Vessel version=""2.0"">
      <RefVessel Action=""MERGE"">
        <PK>{0}</PK>
        <Code>{1}</Code>
        <LloydsNumber>{2}</LloydsNumber>
        <RadioCallSign>{3}</RadioCallSign>
        <NetRegisterTon>0</NetRegisterTon>
        <VesselType>CV</VesselType>
        <CarrierCode></CarrierCode>
        <YearOfConstruction>0</YearOfConstruction>
        <MalaysiaVesselId></MalaysiaVesselId>
        <IsActive>{4}</IsActive>
        <ScreeningStatus>{5}</ScreeningStatus>
        <OrgHeader>
          <Code>{6}</Code>
          <PK>{7}</PK>
        </OrgHeader>
        <CountryOfReg TableName=""RefCountry"">
          <Code>{8}</Code>
          <PK>{9}</PK>
        </CountryOfReg>
      </RefVessel>
    </Vessel>
  </Body>
</Native>
";

		#endregion
	}
}
