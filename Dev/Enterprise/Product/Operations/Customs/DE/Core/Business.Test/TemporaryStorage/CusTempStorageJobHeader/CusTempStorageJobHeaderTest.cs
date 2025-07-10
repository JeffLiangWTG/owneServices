using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeader))]
	sealed class CusTempStorageJobHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusTempStorageJobHeaderLookups>(header.Lookups);
		}

		public void TestValidation()
		{
			CombineAssertions(() =>
			{
				AssertType<SumACusTempStorageJobHeaderValidation>("SUM", header.Validation);
				header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.REX;
				AssertType<REXDISCusTempStorageJobHeaderValidation>("REX", header.Validation);
			});
		}

		public void TestTransportMeansOhne()
		{
			CombineAssertions(() =>
			{
				header.SJH_ContainerCount = 100;
				header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Without;
				AssertEquals("Container Count", 0, header.SJH_ContainerCount);
				AssertEquals("ReadOnly", true, header.SJH_ContainerCountInfo.ReadOnly);
				AssertEquals("Mode", ZString.Empty, header.SJH_TransportMode);
			});
		}

		public void TestTransportMeansLastkraftwagen()
		{
			AssertTransportMeansChanged(TemporaryStorageTransportMeansList.Codes.Truck);
		}

		public void TestTransportMeansSchiff()
		{
			AssertTransportMeansChanged(TemporaryStorageTransportMeansList.Codes.Vessel);
		}

		public void TestTransportMeansWaggon()
		{
			AssertTransportMeansChanged(TemporaryStorageTransportMeansList.Codes.Wagon);
		}

		public void TestTransportMeansFlugzeug()
		{
			CombineAssertions(() =>
			{
				header.SJH_ContainerCount = 100;
				header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
				AssertEquals("Container Count", 0, header.SJH_ContainerCount);
				AssertEquals("ReadOnly", true, header.SJH_ContainerCountInfo.ReadOnly);
				AssertEquals("Mode", TransportTypeList.Codes.Air, header.SJH_TransportMode);
			});
		}

		public void TestMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("SJH_TransportMeansDescriptionMaxLength", 17, header.SJH_TransportMeansDescriptionInfo.MaxLength);
				AssertEquals("SJH_ContainerCountMaxLength", 4, header.SJH_ContainerCountInfo.MaxLength);
				AssertEquals("SJH_TransportRegNoMaxLength", 30, header.SJH_TransportRegNoInfo.MaxLength);
				AssertEquals("SJH_PreviousReferenceNumberMaxLength", 30, header.SJH_PreviousReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCusTempStorageDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Should be SUM", "SUM", header.SJH_AppCode);
				AssertEquals("Branch should be", GlbBranch.CurrentBranch.PK, header.SJH_GB);
			});
		}

		public void TestSJH_NCTSFlag()
		{
			var presenter = Factory.NewWithValidTestData<OrgHeader>();
			presenter.OH_Code = "PRESENTER";
			var presenterAddress = presenter.Addresses.AddNew();

			var authorizedPresenter = Factory.NewWithValidTestData<OrgHeader>();
			authorizedPresenter.OH_Code = "AUTHZPRES";
			var authorizedPresenterAddress = authorizedPresenter.Addresses.AddNew();
			authorizedPresenter.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "DEATHZCONS");
			CombineAssertions(() =>
			{
				var header = Factory.New<CusTempStorageJobHeader>();
				header.SJH_OA_Presenter = presenterAddress.PK;
				AssertEquals("No permit ACE, Readonly", true, header.SJH_NCTSFlagInfo.ReadOnly);
				AssertEquals("No permit ACE, Not Supported", true, header.NCTSFlagNotSupported);
				header.SJH_OA_Presenter = authorizedPresenterAddress.PK;
				AssertEquals("Has permit ACE, editable", false, header.SJH_NCTSFlagInfo.ReadOnly);
				AssertEquals("Has permit ACE, Supported", false, header.NCTSFlagNotSupported);
				header.SJH_NCTSFlag = true;
				header.SJH_OA_Presenter = presenterAddress.PK;
				AssertEquals("Flag resets", false, header.SJH_NCTSFlag);
			});
		}

		public void TestCusTempStorageDecs()
		{
			CUSPRLCusTempStorageDec.New(header);
			AssertEquals(1, header.CusTempStorageDecs.Count);
		}

		public void TestMostRecentlyModifiedDeclarationType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No declarations", ZString.Empty, header.MostRecentlyModifiedDeclarationType);

				var chgtstDeclaration = header.CHGTSTCusTempStorageDecs.AddNew();
				var prlconDeclaration = header.PRLCONCusTempStorageDecs.AddNew();
				var chgspoDeclaration = header.CHGSPOCusTempStorageDecs.AddNew();
				Factory.Save();
				chgtstDeclaration.STH_SystemLastEditTimeUtc = chgtstDeclaration.STH_SystemLastEditTimeUtc.AddHours(-6);
				chgspoDeclaration.STH_SystemLastEditTimeUtc = chgspoDeclaration.STH_SystemLastEditTimeUtc.AddDays(-1);
				Factory.InvalidateCachedProperties();

				AssertContainsExactElementsInAnyOrder("Collection", new CusTempStorageDec[] { chgtstDeclaration, prlconDeclaration, chgspoDeclaration }, header.CusTempStorageDecs);
				AssertEquals("Last edited declaration", TemporaryStorageDeclarationTypeList.Codes.PresentationLedgerConsolidation, header.MostRecentlyModifiedDeclarationType);
			});
		}

		public void TestMostRecentlyModifiedDeclarationMessageStatus()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No declarations", ZString.Empty, header.MostRecentlyModifiedDeclarationMessageStatus);
				var chgtstDeclaration = header.CHGTSTCusTempStorageDecs.AddNew();
				chgtstDeclaration.STH_MessageStatus = EDIMessageStatusList.Codes.Queued;
				var prlconDeclaration = header.PRLCONCusTempStorageDecs.AddNew();
				prlconDeclaration.STH_MessageStatus = EDIMessageStatusList.Codes.Acknowledged;
				var chgspoDeclaration = header.CHGSPOCusTempStorageDecs.AddNew();
				chgspoDeclaration.STH_MessageStatus = EDIMessageStatusList.Codes.Cancelled;
				Factory.Save();
				chgtstDeclaration.STH_SystemLastEditTimeUtc = chgtstDeclaration.STH_SystemLastEditTimeUtc.AddHours(-6);
				chgspoDeclaration.STH_SystemLastEditTimeUtc = chgspoDeclaration.STH_SystemLastEditTimeUtc.AddDays(-1);
				Factory.InvalidateCachedProperties();

				AssertContainsExactElementsInAnyOrder("Collection", new CusTempStorageDec[] { chgtstDeclaration, prlconDeclaration, chgspoDeclaration }, header.CusTempStorageDecs);
				AssertEquals("Last edited declaration", EDIMessageStatusList.Codes.Acknowledged, header.MostRecentlyModifiedDeclarationMessageStatus);
			});
		}

		public void TestMostRecentlyModifiedDeclarationRegistrationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No declarations", ZString.Empty, header.MostRecentlyModifiedDeclarationRegistrationNumber);
				var chgtstDeclaration = header.CHGTSTCusTempStorageDecs.AddNew();
				chgtstDeclaration.ReferenceNumber = "ATB150002930220195877";
				var prlconDeclaration = header.PRLCONCusTempStorageDecs.AddNew();
				prlconDeclaration.ReferenceNumber = "ATB150002930220195875";
				var chgspoDeclaration = header.CHGSPOCusTempStorageDecs.AddNew();
				chgspoDeclaration.ReferenceNumber = "ATB150002930220195872";
				Factory.Save();
				chgtstDeclaration.STH_SystemLastEditTimeUtc = chgtstDeclaration.STH_SystemLastEditTimeUtc.AddHours(-6);
				chgspoDeclaration.STH_SystemLastEditTimeUtc = chgspoDeclaration.STH_SystemLastEditTimeUtc.AddDays(-1);
				Factory.InvalidateCachedProperties();

				AssertContainsExactElementsInAnyOrder("Collection", new CusTempStorageDec[] { chgtstDeclaration, prlconDeclaration, chgspoDeclaration }, header.CusTempStorageDecs);
				AssertEquals("Last edited declaration", "ATB150002930220195875", header.MostRecentlyModifiedDeclarationRegistrationNumber);
			});
		}

		public void TestCUSPRLCusTempStorageDecIsDeletedOnDelete()
		{
			var cusprlStorageDec = CUSPRLCusTempStorageDec.New(header);
			header.Delete();
			AssertEquals(true, cusprlStorageDec.IsDeleted);
		}

		public void TestCUSPRLCusTempStorageDecIsEditableChild()
		{
			CUSPRLCusTempStorageDec.LoadOrCreate(header);
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.CUSPRLCusTempStorageDec));
		}

		public void TestREXDISCusTempStorageDec()
		{
			var rexdis = REXDISCusTempStorageDec.New(header);
			AssertEquals(rexdis, header.REXDISCusTempStorageDec);
		}

		public void TestREXDISCusTempStorageDecIsDeletedOnDelete()
		{
			var rexdisStorageDec = REXDISCusTempStorageDec.New(header);
			header.Delete();
			AssertEquals(true, rexdisStorageDec.IsDeleted);
		}

		public void TestCHGTSTCusTempStorageDecs()
		{
			header.CHGTSTCusTempStorageDecs.AddNew();
			AssertEquals(1, header.CHGTSTCusTempStorageDecs.Count);
		}

		public void TestCHGTSTCusTempStorageDecIsDeletedOnDelete()
		{
			var chgtstStorageDec = header.CHGTSTCusTempStorageDecs.AddNew();
			header.Delete();
			AssertEquals(true, chgtstStorageDec.IsDeleted);
		}

		public void TestCHGTSTCusTempStorageDecsAreEditableChildren()
		{
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.CHGTSTCusTempStorageDecs));
		}

		public void TestCHGOFFCusTempStorageDecs()
		{
			header.CHGOFFCusTempStorageDecs.AddNew();
			AssertEquals(1, header.CHGOFFCusTempStorageDecs.Count);
		}

		public void TestCHGOFFCusTempStorageDecIsDeletedOnDelete()
		{
			var chgoffStorageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			header.Delete();
			AssertEquals(true, chgoffStorageDec.IsDeleted);
		}

		public void TestCHGOFFCusTempStorageDecIsEditableChild()
		{
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.CHGOFFCusTempStorageDecs));
		}

		public void TestDeleteOfUnkownDecType()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_OH_Customer = customer.PK;
			var dec = CUSPRLCusTempStorageDec.LoadOrCreate(header);
			dec.STH_DeclarationType = "UNK";
			Factory.Save();
			var headerReloaded = Factory.Load<CusTempStorageJobHeader>(header.PK);
			headerReloaded.Delete();

			AssertEquals("The CusTempStorageDecTypeDecider for DE could not load the object as the STH_DeclarationType 'UNK' is unknown. A base EU.CusTempStorageDec was returned instead.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPRLCONCusTempStorageDecs()
		{
			header.PRLCONCusTempStorageDecs.AddNew();
			AssertEquals(1, header.PRLCONCusTempStorageDecs.Count);
		}

		public void TestPRLCONCusTempStorageDecsIsEditableChild()
		{
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.PRLCONCusTempStorageDecs));
		}

		public void TestPRLCONCusTempStorageDecsIsDeletedOnDelete()
		{
			var prlconStorageDec = header.PRLCONCusTempStorageDecs.AddNew();
			header.Delete();
			AssertEquals(true, prlconStorageDec.IsDeleted);
		}

		public void TestCUSPCSCusTempStorageDecCollection()
		{
			header.CUSPCSCusTempStorageDecs.AddNew();
			AssertEquals(1, header.CUSPCSCusTempStorageDecs.Count);
		}

		public void TestCUSPCSCusTempStorageDecIsEditableChild()
		{
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.CUSPCSCusTempStorageDecs));
		}

		public void TestCUSPCSCusTempStorageDecIsDeletedOnDelete()
		{
			var cuspcsStorageDec = header.CUSPCSCusTempStorageDecs.AddNew();
			header.Delete();
			AssertEquals(true, cuspcsStorageDec.IsDeleted);
		}

		public void TestCHGSPOCusTempStorageDecs()
		{
			header.CHGSPOCusTempStorageDecs.AddNew();
			AssertEquals(1, header.CHGSPOCusTempStorageDecs.Count);
			Assert(header.IsRegisteredEditableChildObject(header.CHGSPOCusTempStorageDecs));
		}

		public void TestCHGSPOCusTempStorageDecsIsEditableChild()
		{
			AssertEquals(true, header.IsRegisteredEditableChildObject(header.CHGSPOCusTempStorageDecs));
		}

		public void TestCHGSPOCusTempStorageDecsIsDeletedOnDelete()
		{
			var chgspoStorageDec = header.CHGSPOCusTempStorageDecs.AddNew();
			header.Delete();
			AssertEquals(true, chgspoStorageDec.IsDeleted);
		}

		public void TestIsSea()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Blank", false, header.IsSea);
				header.SJH_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Sea", true, header.IsSea);
				header.SJH_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				AssertEquals("Own Propulsion", false, header.IsSea);
			});
		}

		public void TestIsAir()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Blank", false, header.IsAir);
				header.SJH_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Air", true, header.IsAir);
				header.SJH_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertEquals("Fixed Transport Installations", false, header.IsAir);
			});
		}

		public void TestPresenterEoriNumber()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var presenterAddress = SetupOrgAddressWithEORIDetails("1234567890", ZString.Empty);
				AssertEquals("No Address", ZString.Empty, header.PresenterEoriNumber);
				header.SJH_OA_Presenter = presenterAddress.PK;
				AssertEquals("Address", "GR1234567890", header.PresenterEoriNumber);
			});
		}

		public void TestPresenterEoriBranch()
		{
			CombineAssertions(() =>
			{
				var presenterAddress = SetupOrgAddressWithEORIDetails(ZString.Empty, "0001");
				AssertEquals("No Address", ZString.Empty, header.PresenterEoriBranch);
				header.SJH_OA_Presenter = presenterAddress.PK;
				AssertEquals("Address", "0001", header.PresenterEoriBranch);
			});
		}

		public void TestRepresentativeEoriNumber()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var representativeAddress = SetupOrgAddressWithEORIDetails("0987654321", ZString.Empty);
				AssertEquals("No Address", ZString.Empty, header.RepresentativeEoriNumber);
				header.SJH_OA_Representative = representativeAddress.PK;
				AssertEquals("Address", "GR0987654321", header.RepresentativeEoriNumber);
			});
		}

		public void TestRepresentativeEoriBranch()
		{
			CombineAssertions(() =>
			{
				var representativeAddress = SetupOrgAddressWithEORIDetails(ZString.Empty, "0002");
				AssertEquals("No Address", ZString.Empty, header.RepresentativeEoriBranch);
				header.SJH_OA_Representative = representativeAddress.PK;
				AssertEquals("Address", "0002", header.RepresentativeEoriBranch);
			});
		}

		public void TestGetHeaderAndTempStorageDecMessageErrors()
		{
			var cusprlStorageDec = CUSPRLCusTempStorageDec.New(header);
			var cusprlStorageLine = cusprlStorageDec.CusTempStorageLines.AddNew();
			var chgtstStorageDec = header.CHGTSTCusTempStorageDecs.AddNew();
			header.RunPreSaveValidation();
			header.CusTempStorageDecs.RunPreSaveValidation();

			AssertEquals("Different Dec has message errors", true, chgtstStorageDec.HasMessageErrors);
			var messageErrors = header.GetHeaderAndTempStorageDecMessageErrors(cusprlStorageDec).ToMessageListString();
			AssertContains("Header Error is added to the list", "Transport Means: You have not entered a Transport Means.", messageErrors);
			AssertContains("Dec being checked message error is added to the list", "Package Count: Package count must be between 1 and 99999.", messageErrors);
			AssertNotContains("Other Dec message errors do not exist on the list", "New Custodian Branch: You have not entered a New Custodian Branch.", messageErrors);

			header.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
			cusprlStorageLine.TSL_PackageQty = 1;
			header.RunPreSaveValidation();
			messageErrors = header.GetHeaderAndTempStorageDecMessageErrors(cusprlStorageDec).ToMessageListString();
			AssertNotContains("Fixed header message error removed", "Transport Means: You have not entered a Transport Means.", messageErrors);
			AssertNotContains("Fixed Dec message error is removed", "Package Count: Package count must be between 1 and 99999.", messageErrors);
			AssertNotContains("Other dec message error still not on the list", "New Custodian Branch: You have not entered a New Custodian Branch.", messageErrors);
		}

		public void TestPresenterReadOnly()
		{
			AssertPropertyReadOnlyFromCUSPRLMessaging(header.SJH_OA_PresenterInfo);
		}

		public void TestCustomsOfficeReadOnly()
		{
			AssertPropertyReadOnlyFromCUSPRLMessaging(header.SJH_CustomsOfficeInfo);
		}

		public void TestRepresentativeReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_OA_RepresentativeInfo);
		}

		public void TestTransportMeansReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_TransportMeansCodeInfo);
		}

		public void TestTransportModeReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_TransportModeInfo);
		}

		public void TestBorderTransportInfoReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_TransportMeansDescriptionInfo);
		}

		public void TestTransportRegNumReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_TransportRegNoInfo);
		}

		public void TestLoadingPlaceReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_RL_NKLoadingInfo);
		}

		public void TestArrivalDateReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_ArrivalDateInfo);
		}

		public void TestPresentationDateReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_PresentationDateInfo);
		}

		public void TestPreviousReferenceTypeReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_PreviousReferenceTypeInfo);
		}

		public void TestPreviousReferenceNumberReadOnly()
		{
			AssertPropertyReadOnlyIfRegHeaderForATNumberExists(header.SJH_PreviousReferenceNumberInfo);
		}

		[TestDate(2022, 11, 10, 12, 07, 00)]
		public void TestSetDefaultValuesForSumA()
		{
			CombineAssertions(() =>
			{
				header.SetDefaultValuesForSumA();
				AssertEquals("SJH_OH_Customer from CurrentBranch", GlbBranch.CurrentBranch.OrgProxy.PK, header.SJH_OH_Customer);
				AssertEquals("SJH_OA_Presenter from CurrentBranch", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, header.SJH_OA_Presenter);
				AssertEquals("SJH_PresentationDate", new ZDateTime(2022, 11, 10, 12, 07, 00), header.SJH_PresentationDate);
				AssertEquals("SJH_PreviousReferenceType", PreviousReferenceType.Codes._OHNE, header.SJH_PreviousReferenceType);

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				header.SetDefaultValuesForSumA();
				AssertEquals("SJH_OH_Customer from CurrentCompany", GlbCompany.CurrentCompany.OrgProxy.PK, header.SJH_OH_Customer);
				AssertEquals("SJH_OA_Presenter from CurrentCompany", GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, header.SJH_OA_Presenter);
			});
		}

		[UseSnapshotProtection]
		public void TestJobReferenceNumberOnSaving_NoDuplicateReferenceException()
		{
			var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
			header.OnSaving();
			var jobReference = header.SJH_JobReference;
			dbConnection.RollbackTransaction();

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.New<CusTempStorageJobHeader>();
			var orgHeader = newFactory.NewWithValidTestData<OrgHeader>();
			header2.SJH_OH_Customer = orgHeader.PK;
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Saving header2", () => newFactory.Save());
				AssertEquals("header and header2 have the same JobReference", jobReference, header2.SJH_JobReference);

				dbConnection.BeginTransaction();
				AssertEquals("Before saved, header has duplicate JobReference", jobReference, header.SJH_JobReference);
				AssertNoExceptionThrown("Saving header", () => Factory.Save());
				AssertNotEquals("After saved, header has unique JobReference", jobReference, header.SJH_JobReference);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_JobReference = ZString.Empty;
		}
		CusTempStorageJobHeader header;

		protected override BusinessObject GetNewBusinessObject() => header;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<CusTempStorageJobHeader>();
			result.SJH_JobReference = ZString.Empty;
			return result;
		}

		OrgAddress SetupOrgAddressWithEORIDetails(ZString eoriNumber, ZString eoriBranch)
		{
			var org = Factory.New<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			if (!eoriNumber.IsEmpty)
			{
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
				cusCode.OK_CustomsRegNo = eoriNumber;
			}
			if (!eoriBranch.IsEmpty)
			{
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				cusCode.OK_CustomsRegNo = eoriBranch;
				cusCode.OK_OA_PremisesAddress = orgAddress.PK;
			}
			return orgAddress;
		}

		void AssertPropertyReadOnlyFromCUSPRLMessaging(ZPropertyInfo propertyInfo)
		{
			CombineAssertions(() =>
			{
				AssertEquals("No CUSPRL Dec", false, propertyInfo.ReadOnly);
				var dec = CUSPRLCusTempStorageDec.LoadOrCreate(header);
				AssertEquals("No Issue Date", false, propertyInfo.ReadOnly);
				dec.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
				AssertEquals("Dec and Issue Date", true, propertyInfo.ReadOnly);
			});
		}

		void AssertPropertyReadOnlyIfRegHeaderForATNumberExists(ZPropertyInfo propertyInfo)
		{
			const string atbNumber = "ATB150002930220195877";
			CombineAssertions(() =>
			{
				AssertEquals("No CUSPRL Dec, no RegHeader", false, propertyInfo.ReadOnly);

				var cusprlStorageDec = CUSPRLCusTempStorageDec.LoadOrCreate(header);
				cusprlStorageDec.ReferenceNumber = atbNumber;
				AssertEquals("CUSPRL Dec with ATB Number, no RegHeader", false, propertyInfo.ReadOnly);

				var register = Factory.New<CusTempStorageRegHeader>();
				register.SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
				register.SRH_Reference = atbNumber;
				AssertEquals("Matching RegHeader Created", true, propertyInfo.ReadOnly);
			});
		}

		void AssertTransportMeansChanged(ZString transportmeansCode)
		{
			CombineAssertions(() =>
			{
				header.SJH_ContainerCount = 100;
				header.SJH_TransportMeansCode = transportmeansCode;
				AssertEquals("Container Count", 100, header.SJH_ContainerCount);
				AssertEquals("ReadOnly", false, header.SJH_ContainerCountInfo.ReadOnly);
				AssertEquals("Mode", ZString.Empty, header.SJH_TransportMode);
			});
		}
	}
}
