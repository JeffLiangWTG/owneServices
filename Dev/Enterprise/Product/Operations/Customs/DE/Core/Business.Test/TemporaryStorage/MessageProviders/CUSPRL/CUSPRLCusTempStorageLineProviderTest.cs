using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPRLCusTempStorageLineProvider))]
	class CUSPRLCusTempStorageLineProviderTest : CusTempStorageLineProviderAbstractTest<CUSPRLCusTempStorageLineProvider>
	{
		public void TestHasPreliminaryChanges()
		{
			TempStorageLine.TSL_IsModified = true;
			AssertEquals(true, TempStorageLineWrapped.HasPreliminaryChanges);
			TempStorageLine.TSL_IsModified = false;
			AssertEquals(false, TempStorageLineWrapped.HasPreliminaryChanges);
		}

		public void TestCustomsAuthorisationNumber_NoCustodian()
		{
			AssertEquals(PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
		}

		public void TestCustomsAuthorisationNumber()
		{
			CombineAssertions(() =>
			{
				var custodian = Factory.New<OrgHeader>();
				TempStorageLine.StorageHeader.SJH_PresentationDate = ZDate.Today;
				TempStorageLine.TSL_OA_Custodian = custodian.MainAddress.PK;
				AssertEquals("Has no AuthorisationNumber", PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
				custodian.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "AUT123456");
				AssertEquals("Has AuthorisationNumber", "AUT123456", TempStorageLineWrapped.CustomsAuthorisationNumber);
				TempStorageLine.StorageHeader.SJH_PresentationDate = ZDate.Today.AddDays(-2);
				AssertEquals("Has AuthorisationNumber but PresentationDate out of range.", PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
			});
		}

		public void TestTransportNumberType()
		{
			TempStorageLine.TSL_TransportNumberType = "0754";
			AssertEquals("0754", TempStorageLineWrapped.TransportNumberType);
		}

		public void TestTransportReferenceNumber()
		{
			TempStorageLine.TSL_TransportNumber = "TransportNumber3124";
			AssertEquals("TransportNumber3124", TempStorageLineWrapped.TransportReferenceNumber);
		}

		public void TestCarrierEoriNumber()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			var carrierOrg = Factory.GetOrgHeaderWithEori("TEST", "12345", Core.Constants.CountryCodes.Germany);
			var carrierAddress = carrierOrg.MainAddress;
			carrierAddress.Address1 = "Address1";
			carrierAddress.City = "City";
			carrierAddress.Postcode = "2730018";
			carrierAddress.OA_RN_NKCountryCode = "DE";
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = carrierAddress.PK;

			CombineAssertions(() =>
			{
				((CUSPRLCusTempStorageLine)TempStorageLine).CarrierDocAddress.OrganisationPK = carrierOrg.PK;
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				AssertEquals("Mapped", "DE12345", GetTempStorageLineWrapped().CarrierEoriNumber);

				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
				AssertNull("NOT mapped, as PreviousReferenceType != N355", TempStorageLineWrapped.CarrierEoriNumber);

				carrierOrg.GetOrgCusCode("EOR", "12345").Delete();
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				AssertNull("NOT mapped, as EORI is empty", TempStorageLineWrapped.CarrierEoriNumber);
			});
		}

		public void TestTransportDocumentMasterLevelType()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			CombineAssertions(() =>
			{
				((CUSPRLCusTempStorageLine)TempStorageLine).TransportDocumentMaster.CSI_Code = "123";
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				AssertEquals("Mapped", "123", TempStorageLineWrapped.TransportDocumentMasterLevelType);

				((CUSPRLCusTempStorageLine)TempStorageLine).TransportDocumentMaster.CSI_Code = ZString.Empty;
				AssertNull("NOT mapped, as Code is empty", TempStorageLineWrapped.TransportDocumentMasterLevelType);
			});
		}

		public void TestTransportDocumentMasterLevelType_PreviousReferenceTypeNotN355()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			((CUSPRLCusTempStorageLine)TempStorageLine).TransportDocumentMaster.CSI_Code = "123";
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
			AssertNull("NOT mapped, as PreviousReferenceType != N355", TempStorageLineWrapped.TransportDocumentMasterLevelType);
		}

		public void TestTransportDocumentMasterLevelNumber()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			CombineAssertions(() =>
			{
				((CUSPRLCusTempStorageLine)TempStorageLine).TransportDocumentMaster.CSI_ReferenceNumber = "123";
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				AssertEquals("Mapped", "123", TempStorageLineWrapped.TransportDocumentMasterLevelNumber);

				((CUSPRLCusTempStorageLine)TempStorageLine).TransportDocumentMaster.CSI_ReferenceNumber = ZString.Empty;
				AssertNull("NOT mapped, as ReferenceNumber is empty", TempStorageLineWrapped.TransportDocumentMasterLevelNumber);
			});
		}

		public void TestTransportDocumentMasterLevelNumber_PreviousReferenceTypeNotN355()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			((CUSPRLCusTempStorageLine)TempStorageLine).TransportDocumentMaster.CSI_ReferenceNumber = "123";
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
			AssertNull("NOT mapped, as PreviousReferenceType != N355", TempStorageLineWrapped.TransportDocumentMasterLevelNumber);
		}

		public void TestReceptacleIdentificationNumber()
		{
			CombineAssertions(() =>
			{
				var storageHeader = TempStorageLine.StorageHeader;
				TempStorageLine.TSL_TransportNumberType = ZString.Empty;
				((CUSPRLCusTempStorageLine)TempStorageLine).Receptacle = "TEST";
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				AssertEquals("Mapped", "TEST", TempStorageLineWrapped.ReceptacleIdentificationNumber);

				((CUSPRLCusTempStorageLine)TempStorageLine).Receptacle = ZString.Empty;
				AssertNull("NOT mapped, as Receptacle is empty", TempStorageLineWrapped.ReceptacleIdentificationNumber);

				TempStorageLine.TSL_TransportNumberType = "1";
				AssertNull("NOT mapped, as TSL_TransportNumberType NOT blank", TempStorageLineWrapped.ReceptacleIdentificationNumber);
			});
		}

		public void TestReceptacleIdentificationNumber_PreviousReferenceTypeNotN355()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			TempStorageLine.TSL_TransportNumberType = ZString.Empty;
			((CUSPRLCusTempStorageLine)TempStorageLine).Receptacle = "TEST";
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
			AssertNull("NOT mapped, as PreviousReferenceType != N355", TempStorageLineWrapped.ReceptacleIdentificationNumber);
		}

		public void TestTransportEquipmentIdentificationNumber()
		{
			CombineAssertions(() =>
			{
				var storageHeader = TempStorageLine.StorageHeader;
				((CUSPRLCusTempStorageLine)TempStorageLine).ContainerNumber = "11111";
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				TempStorageLine.TSL_ReferenceNumber = "123";
				AssertEquals("Mapped", "11111", TempStorageLineWrapped.TransportEquipmentIdentificationNumber);

				TempStorageLine.TSL_ReferenceNumber = ZString.Empty;
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				AssertNull("NOT mapped, as TSL_ReferenceNumber is empty", TempStorageLineWrapped.TransportEquipmentIdentificationNumber);

				((CUSPRLCusTempStorageLine)TempStorageLine).ContainerNumber = ZString.Empty;
				TempStorageLine.TSL_ReferenceNumber = "123";
				AssertNull("NOT mapped, as ContainerNumber is empty", TempStorageLineWrapped.TransportEquipmentIdentificationNumber);
			});
		}

		public void TestTransportEquipmentIdentificationNumber_PreviousReferenceTypeNotN355()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			((CUSPRLCusTempStorageLine)TempStorageLine).ContainerNumber = "11111";
			TempStorageLine.TSL_ReferenceNumber = "123";
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
			AssertNull("NOT mapped, as PreviousReferenceType != N355", TempStorageLineWrapped.TransportEquipmentIdentificationNumber);
		}

		public void TestPreviousReferenceTypeIsN355()
		{
			var storageHeader = TempStorageLine.StorageHeader;
			CombineAssertions(() =>
			{
				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
				var provider = new CUSPRLCusTempStorageLineProvider(TempStorageLine);
				Assert(provider.PreviousReferenceTypeIsN355);

				storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
				provider = new CUSPRLCusTempStorageLineProvider(TempStorageLine);
				Assert(!provider.PreviousReferenceTypeIsN355);
			});
		}

		protected override CusTempStorageLine GetTempStorageLineToTest()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			return storageDec.CusTempStorageLines.AddNew();
		}

		protected override CUSPRLCusTempStorageLineProvider GetTempStorageLineWrapped() => new CUSPRLCusTempStorageLineProvider(TempStorageLine);

		protected new ICUSPRLTempStorageLine TempStorageLineWrapped => (ICUSPRLTempStorageLine)base.TempStorageLineWrapped;
	}
}
