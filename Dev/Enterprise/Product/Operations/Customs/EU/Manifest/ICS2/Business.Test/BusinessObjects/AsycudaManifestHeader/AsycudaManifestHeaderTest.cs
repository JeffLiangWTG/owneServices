using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestCountryName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = RefDataGroupingCodes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICS2EUMemberState;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, CountryCodes.Germany, "Germany", startDate, endDate);
			Factory.Save();
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			AssertEquals("EU", manifestHeader.CountryName);

			manifestHeader.AddressedMemberState = CountryCodes.Germany;

			AssertEquals("Germany", manifestHeader.CountryName);
		}

		public void TestZZValidationHelperType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ZZDatabaseValidationHelper>(header.ZZValidationHelper);
		}

		public void TestIsPackedItemTypeOfGoodsAndGoodsValueEnabled()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals("PreReq", expected: false, header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F43);
			CombineAssertions(() =>
			{
				AssertEquals("SpecificCircumstanceIndicator != F43", expected: false, header.IsPackedItemTypeOfGoodsAndGoodsValueEnabled);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				AssertEquals("SpecificCircumstanceIndicator == F43", expected: true, header.IsPackedItemTypeOfGoodsAndGoodsValueEnabled);
			});
		}

		public void TestSynchroniser()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<AsycudaManifestHeader>();
			header.SetParent(consol);

			AssertType<AsycudaManifestHeaderSynchroniser>(header.Synchroniser);
		}

		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestManifestNature()
		{
			var argmanifestTypes = new EUICS2ManifestTypes().All;
			var man = argmanifestTypes.FirstOrDefault(x => x.Code == EUICS2ManifestTypes.Codes.ENS);
			AssertEquals(ShipmentTypeList.Codes.Import23, man.ManifestNatures.CodesAsString);
		}

		public void TestVoyageFlightNoLabel()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Flight/Voyage", header.VoyageFlightNoLabel.Caption);
			header.AMA_TransportMode = "IWT";
			AssertEquals("Voyage", header.VoyageFlightNoLabel.Caption);
			header.AMA_TransportMode = "AIR";
			AssertEquals("Flight", header.VoyageFlightNoLabel.Caption);
			header.AMA_TransportMode = "SEA";
			AssertEquals("Voyage", header.VoyageFlightNoLabel.Caption);
		}
		public void TestCustomsProfileIsReadOnlyWhenMessagesSent()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Not ReadOnly when no messages have been sent", false, manifestHeader.AMA_CustomsProfileInfo.ReadOnly);

			manifestHeader.AMA_CustomsProfile = GlbCompany.CurrentCompany.GC_Code;
			manifestHeader.AddressedMemberState = CountryCodes.Germany;
			manifestHeader.Messages.AddNew();
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			AssertEquals("ReadOnly once the first message has been sent", true, manifestHeader.AMA_CustomsProfileInfo.ReadOnly);
		}

		public void TestMemberStateReadOnly()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Not ReadOnly when no messages have been sent", false, manifestHeader.AddressedMemberStateInfo.ReadOnly);

			manifestHeader.AddressedMemberState = CountryCodes.Germany;
			Factory.Save();
			AssertEquals("Still not ReadOnly even when in the database", false, manifestHeader.AddressedMemberStateInfo.ReadOnly);

			manifestHeader.AddressedMemberState = CountryCodes.Germany;
			manifestHeader.Messages.AddNew();
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			AssertEquals("ReadOnly once the first message has been sent", true, manifestHeader.AddressedMemberStateInfo.ReadOnly);
		}

		public void TestAddressedMemberState()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MS;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "DE", "Germany", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "FR", "France", startDate, endDate);
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Not ReadOnly when not in database", false, manifestHeader.AddressedMemberStateInfo.ReadOnly);
			manifestHeader.AddressedMemberState = CountryCodes.Germany;
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			Factory.Save();

			AssertEquals("Not ReadOnly just when in database", false, manifestHeader.AddressedMemberStateInfo.ReadOnly);
			AssertEquals("Manifest Type", typeof(AsycudaManifestHeader), manifestHeader.GetType());
			AssertEquals("AddressedMemberState DE", CountryCodes.Germany, manifestHeader.AddressedMemberState);
			AssertEquals("AMA_RN_NKCountry DE", CountryCodes.Germany, manifestHeader.AMA_RN_NKCountry);

			manifestHeader.AMA_RN_NKCountry = CountryCodes.France;
			AssertEquals("AddressedMeberState changed to FR after country change", CountryCodes.France, manifestHeader.AddressedMemberState);

			manifestHeader.AddressedMemberState = "XX";
			AssertEquals(CountryCodes.France, manifestHeader.AMA_RN_NKCountry);
			AssertEquals("XX", manifestHeader.AddressedMemberState);

			manifestHeader.Messages.AddNew();
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			AssertEquals("ReadOnly now based on the first message having been sent - the same as for Customs Profile", true, manifestHeader.AddressedMemberStateInfo.ReadOnly);
		}

		public void TestAddressMemberStateResetOnCountryChange()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MS;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "DE", "Germany", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "FR", "France", startDate, endDate);
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AddressedMemberState = CountryCodes.Germany;
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			AssertEquals("AMA_RN_NKCountry should be DE", CountryCodes.Germany, manifestHeader.AMA_RN_NKCountry);
			manifestHeader.AMA_RN_NKCountry = CountryCodes.France;
			AssertEquals("AddressMemberState should be set to FR", CountryCodes.France, manifestHeader.AddressedMemberState);

			manifestHeader.AMA_RN_NKCountry = "EU";
			AssertEquals(CountryCodes.EuropeanUnion, manifestHeader.AddressedMemberState);
		}

		public void TestAddressedMemberState_Caption()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(manifestHeader.AddressedMemberStateInfo);
			AssertEquals("Country", propertyData.Caption);
		}

		public void TestAMA_PaymentMethod_Caption()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(manifestHeader.AMA_PaymentMethodInfo);
			AssertEquals("Method of Payment", propertyData.Caption);
		}

		public void TestSupportedCusCodeDataTypes()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var supportedCusCodeDataTypes = header.GetCusCodeDataTypes();
			AssertCollectionContains(CusCodeDataTypeList.Codes.EUICS2RouteEntry, supportedCusCodeDataTypes.Keys);
		}

		public void TestPreviousMRN_ReadOnlyWhenSpecificCircumstanceIndicatorUnapplicable()
		{
			var applicableSpecificCircumstanceIndicatorList = new string[] {
				EUICS2SpecificCircumstanceList.Codes.F10,
				EUICS2SpecificCircumstanceList.Codes.F11,
				EUICS2SpecificCircumstanceList.Codes.F12,
				EUICS2SpecificCircumstanceList.Codes.F13,
				EUICS2SpecificCircumstanceList.Codes.F25,
				EUICS2SpecificCircumstanceList.Codes.F30,
			};

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			foreach (var item in new EUICS2SpecificCircumstanceList().GetAllCodes())
			{
				manifestHeader.SpecificCircumstanceIndicator = item;

				if (applicableSpecificCircumstanceIndicatorList.Contains(item))
				{
					Assert(!manifestHeader.PreviousMRNInfo.ReadOnly);
				}
				else
				{
					Assert(manifestHeader.PreviousMRNInfo.ReadOnly);
				}
			}
		}

		public void TestPreviousMRN_ClearWhenChangeSpecificCircumstanceIndicatorToUnapplicable()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
			manifestHeader.PreviousMRN = "PreviousMRN";

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F20;
			Assert(manifestHeader.PreviousMRN.IsEmpty);
		}

		public void TestPreviousMRN()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.PreviousMRN = "Test1";
			AssertNotNull(manifestHeader.PreviousMRN);
			AssertEquals("Test1", manifestHeader.PREEntryNumber.CE_EntryNum);

			manifestHeader.PreviousMRN = "";
			Factory.Save();
			AssertNull(manifestHeader.PREEntryNumber);

			var manifestHeader2 = Factory.New<AsycudaManifestHeader>();
			var entryNum = CusEntryNumber.LoadOrCreate(manifestHeader2, CusEntryNumberTypes.EU.PRE, manifestHeader2.AMA_RN_NKCountry);
			entryNum.CE_EntryNum = "Test2";
			AssertEquals("Test2", manifestHeader2.PreviousMRN);

			entryNum.CE_EntryType = "";
			AssertEquals("", manifestHeader2.PreviousMRN);
		}

		public void TestMOTIdentifier()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.MOTIdentifier = "Test1";
			AssertEquals("Test1", manifestHeader.MOTIdentifier);

			AssertEquals(35, manifestHeader.MOTIdentifierInfo.MaxLength);
			AssertExceptionThrown<MaxLengthExceededException>(() => manifestHeader.MOTIdentifier = new ZString('X', 37));
			ErrorReporter.Clear();
		}

		public void TestMOTIdentifierType()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.MOTIdentifierType = "12";
			AssertEquals("12", manifestHeader.MOTIdentifierType);

			AssertEquals(2, manifestHeader.MOTIdentifierTypeInfo.MaxLength);
			AssertExceptionThrown<MaxLengthExceededException>(() => manifestHeader.MOTIdentifierType = "123");
			ErrorReporter.Clear();
		}

		public void TestPackedItemRelationship()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Should be RelationshipType.One", ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One, header.PackedItemRelationship);
		}

		public void TestDefaultSpecificCircumstanceIndicator_WhenSetTransportModes()
		{
			CombineAssertions(() =>
			{
				var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				manifestHeader.AMA_TransportMode = TransportModes.Air;
				AssertEquals("Default Specific Circumstance F24", EUICS2SpecificCircumstanceList.Codes.F24, manifestHeader.SpecificCircumstanceIndicator);
				manifestHeader.AMA_TransportMode = TransportModes.Sea;
				AssertEquals("Empty", ZString.Empty, manifestHeader.SpecificCircumstanceIndicator);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				manifestHeader.AMA_TransportMode = TransportModes.InlandWaterwayTransport;
				AssertEquals("Empty", ZString.Empty, manifestHeader.SpecificCircumstanceIndicator);

				var manifestHeader2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestHeader2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				manifestHeader2.AMA_TransportMode = TransportModes.Road;
				AssertEquals("Default Specific Circumstance F50", EUICS2SpecificCircumstanceList.Codes.F50, manifestHeader2.SpecificCircumstanceIndicator);
			});
		}

		public void TestProperties_ShouldBeReadOnly_WhenManifestIsRegisteredAndF14F15F16()
		{
			var testCases = new[]
			{
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isRegistered: true, expectedReadonly: (true, true, true, true, true, true, true)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isRegistered: false, expectedReadonly: (false, false, false, false, false, false, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isRegistered: true, expectedReadonly: (true, true, true, true, true, true, true)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isRegistered: false, expectedReadonly: (false, false, false, false, false, false, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isRegistered: true, expectedReadonly: (true, true, true, true, true, true, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isRegistered: false, expectedReadonly: (false, false, false, false, false, false, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F17, isRegistered: true, expectedReadonly: (false, false, false, false, false, false, false)),
			};

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			foreach (var (specificCircumstanceIndicator, isRegistered, (specificCircumstanceIndicatorReadonly, addressedMemberStateReadonly, masterBillReadonly, shippingAgentReadonly, declarantReadonly, carrierReadonly, transportModeReadonly)) in testCases)
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				manifestHeader.RegistrationNumber = isRegistered ? "123" : string.Empty;

				CombineAssertions($"specificCircumstanceIndicator: {specificCircumstanceIndicator}, isRegistered: {isRegistered}", () =>
				{
					AssertEquals("SpecificCircumstanceIndicator", specificCircumstanceIndicatorReadonly, manifestHeader.SpecificCircumstanceIndicatorInfo.ReadOnly);
					AssertEquals("AddressedMemberState", addressedMemberStateReadonly, manifestHeader.AddressedMemberStateInfo.ReadOnly);
					AssertEquals("MasterBill", masterBillReadonly, manifestHeader.AMA_MasterBillInfo.ReadOnly);
					AssertEquals("ShippingAgen", shippingAgentReadonly, manifestHeader.AMA_OA_ShippingAgentInfo.ReadOnly);
					AssertEquals("Declarant", declarantReadonly, manifestHeader.AMA_OA_DeclarantInfo.ReadOnly);
					AssertEquals("Carrier", carrierReadonly, manifestHeader.AMA_OA_CarrierInfo.ReadOnly);
					AssertEquals("TransportMode", transportModeReadonly, manifestHeader.AMA_TransportModeInfo.ReadOnly);
				});
			}
		}

		public void TestProperties_ShouldNotBeReadOnly_WhenCustomsStatusIsCancelledAndF14F15F16()
		{
			var testCases = new[]
			{
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isCancelled: true, expectedReadonly: (false, false, false, false, false, false, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isCancelled: false, expectedReadonly: (true, true, true, true, true, true, true)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isCancelled: true, expectedReadonly: (false, false, false, false, false, false, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isCancelled: false, expectedReadonly: (true, true, true, true, true, true, true)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isCancelled: true, expectedReadonly: (false, false, false, false, false, false, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isCancelled: false, expectedReadonly: (true, true, true, true, true, true, false)),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F17, isCancelled: true, expectedReadonly: (false, false, false, false, false, false, false)),
			};

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.RegistrationNumber = "123";

			foreach (var (specificCircumstanceIndicator, isCancelled, (specificCircumstanceIndicatorReadonly, addressedMemberStateReadonly, masterBillReadonly, shippingAgentReadonly, declarantReadonly, carrierReadonly, transportModeReadonly)) in testCases)
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				manifestHeader.RegistrationStatus = isCancelled ? EUICS2CustomsStatusList.Codes.CAN : EUICS2CustomsStatusList.Codes.REG;

				CombineAssertions($"specificCircumstanceIndicator: {specificCircumstanceIndicator}, isCancelled: {isCancelled}", () =>
				{
					AssertEquals("SpecificCircumstanceIndicator", specificCircumstanceIndicatorReadonly, manifestHeader.SpecificCircumstanceIndicatorInfo.ReadOnly);
					AssertEquals("AddressedMemberState", addressedMemberStateReadonly, manifestHeader.AddressedMemberStateInfo.ReadOnly);
					AssertEquals("MasterBill", masterBillReadonly, manifestHeader.AMA_MasterBillInfo.ReadOnly);
					AssertEquals("ShippingAgen", shippingAgentReadonly, manifestHeader.AMA_OA_ShippingAgentInfo.ReadOnly);
					AssertEquals("Declarant", declarantReadonly, manifestHeader.AMA_OA_DeclarantInfo.ReadOnly);
					AssertEquals("Carrier", carrierReadonly, manifestHeader.AMA_OA_CarrierInfo.ReadOnly);
					AssertEquals("TransportMode", transportModeReadonly, manifestHeader.AMA_TransportModeInfo.ReadOnly);
				});
			}
		}

		public void TestSpecificCircumstanceCompatibilityAtManifestHeaderLevel()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var supportingDocument = manifestHeader.SupportingDocuments.AddNew();

			var applicableSpecificCircumstanceIndicatorList = new string[] {
				EUICS2SpecificCircumstanceList.Codes.F10,
				EUICS2SpecificCircumstanceList.Codes.F11,
				EUICS2SpecificCircumstanceList.Codes.F12,
				EUICS2SpecificCircumstanceList.Codes.F13,
				EUICS2SpecificCircumstanceList.Codes.F14,
				EUICS2SpecificCircumstanceList.Codes.F20,
				EUICS2SpecificCircumstanceList.Codes.F21,
				EUICS2SpecificCircumstanceList.Codes.F27,
				EUICS2SpecificCircumstanceList.Codes.F28,
				EUICS2SpecificCircumstanceList.Codes.F29,
				EUICS2SpecificCircumstanceList.Codes.F42,
				EUICS2SpecificCircumstanceList.Codes.F50,
				EUICS2SpecificCircumstanceList.Codes.F51
			};

			foreach (var item in new EUICS2SpecificCircumstanceList().GetAllCodes())
			{
				manifestHeader.SpecificCircumstanceIndicator = item;
				supportingDocument.RunPreSaveValidation();

				if (applicableSpecificCircumstanceIndicatorList.Contains(item))
				{
					AssertNoRowWarnings(supportingDocument);
				}
				else
				{
					AssertHasRowWarning("Has row warning as specific circumstance is not applicable.", supportingDocument, "The Specific Circumstance does not support Supporting Document details at this level. These will not be sent in the ICS2 message.");
				}
			}
		}

		public void TestBillScreenings()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(0, header.BillScreenings.Count);
			header.MasterBill.BillScreenings.AddNew();
			AssertEquals(1, header.BillScreenings.Count);
			Factory.Save();
			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(1, headerReloaded.BillScreenings.Count);
		}

		public void TestIsForwarderManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			AssertEquals("Should be non forwarder manifest when application code is VOC", false, header.IsForwarderManifest);

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			AssertEquals("Should be forwarder manifest when application code is NVC", true, header.IsForwarderManifest);
		}

		public void TestIsCarrierManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			AssertEquals("Should be non carrier manifest when application code is NVC", false, header.IsCarrierManifest);

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			AssertEquals("Should be carrier manifest when application code is VOC", true, header.IsCarrierManifest);
		}

		[TestDate(2022, 9, 23)]
		public void TestGenerateUniqueLocalReferenceNumber()
		{
			var newFactory = new BusinessObjectFactory();

			var org = newFactory.NewWithValidTestData<OrgHeader>();

			var address = newFactory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "Address1";
			address.OA_Address2 = "Address2";
			address.OA_Code = "Code";
			address.OA_OH = org.PK;

			var company = newFactory.New<GlbCompany>();
			company.GC_Code = "TSC";
			company.GC_RN_NKCountryCode = CountryCodes.France;
			company.GC_Name = "Test Company";
			company.GC_OH_OrgProxy = org.PK;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "TSB";
			branch.GB_RL_NKHomePort = newFactory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, company.GC_RN_NKCountryCode)).Code;
			branch.GB_BranchName = "Test Branch";

			newFactory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F22;
				new EUManifestMessageSender(header).SendFilingMessage();
				AssertEquals("TSCTSBF222200000000001", header.LocalReferenceNumber);
				header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
				new EUManifestMessageSender(header).SendFilingMessage();
				AssertEquals("TSCTSBF222200000000002", header.LocalReferenceNumber);
				header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
				new EUManifestMessageSender(header).SendFilingMessage();
				AssertEquals("TSCTSBF222200000000003", header.LocalReferenceNumber);

				header.Reload();
				AssertEquals("Should always load the latest LRN", "TSCTSBF222200000000003", header.LocalReferenceNumber);
			}
		}

		public void TestRequestHeaders()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(0, header.RequestHeaders.Count);
			var requestHeader = header.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "XXX";
			requestHeader.EUS_Type = "YY";
			AssertEquals(1, header.RequestHeaders.Count);
			Factory.Save();
			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(1, headerReloaded.RequestHeaders.Count);
		}

		public void TestDefaultBillTransportDocument_AfterChangeTransportMode()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
				AssertEquals("pre-condition", string.Empty, header.MasterBill.TransportDocumentType);
				AssertEquals("pre-condition", string.Empty, bill.TransportDocumentType);

				header.AMA_TransportMode = TransportModes.Air;
				AssertEquals(TransportDocumentTypes.Codes.CL754_N741, header.MasterBill.TransportDocumentType);
				AssertEquals(TransportDocumentTypes.Codes.CL754_N703, bill.TransportDocumentType);
			});
		}

		public void TestDefaultBillTransportDocument_AfterChangeSpecificCircumstanceIndicator()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_TransportMode = TransportModes.Sea;
			AssertEquals("pre-condition", string.Empty, header.MasterBill.TransportDocumentType);
			AssertEquals("pre-condition", string.Empty, bill.TransportDocumentType);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
			AssertEquals(TransportDocumentTypes.Codes.CL754_N705, header.MasterBill.TransportDocumentType);
			AssertEquals(TransportDocumentTypes.Codes.CL754_N714, bill.TransportDocumentType);
		}

		public void TestDefaultTransportMeans_AfterTransportModeChanged()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Sea;
			header.AMA_TransportMeans = "A";

			header.AMA_TransportMode = TransportModes.Air;

			AssertEquals(ZString.Empty, header.AMA_TransportMeans);
		}

		public void TestDefaultTransportMode_WhenSpecificCircumstanceChanged()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertNotEquals("Precondition", TransportModes.Road, manifestHeader.AMA_TransportMode);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;

			AssertEquals("Default Transport Mode ROA", TransportModes.Road, manifestHeader.AMA_TransportMode);
		}

		public void TestDefaultBillTransportDocument_AfterChangeAgentType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Air;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F20;
			AssertEquals("pre-condition", TransportDocumentTypes.Codes.CL754_N741, header.MasterBill.TransportDocumentType);

			header.AMA_AgentType = AgentType.Direct;
			AssertEquals(TransportDocumentTypes.Codes.CL754_N740, header.MasterBill.TransportDocumentType);
		}

		public void TestRegistrationNumberForBinding()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertNullOrEmpty(header.RegistrationNumberForBinding);

			header.RegistrationNumber = "ASY";
			AssertEquals("ASY", header.RegistrationNumberForBinding);

			header.ArrivalReferenceNumber = "ARN";
			AssertEquals("ASY,ARN", header.RegistrationNumberForBinding);
		}

		public void TestReadOnly_WhenMessageStatusIsAwaiting()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();

			AssertEquals("Precondition", false, header.ReadOnly);

			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(true, header.ReadOnly);

			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var freshHeader = anotherFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(true, freshHeader.ReadOnly);
		}

		public void TestReadOnly_WhenRegistrationStatusIsASC()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();

			AssertEquals("Precondition", false, header.ReadOnly);

			header.RegistrationStatus = EUICS2CustomsStatusList.Codes.ASC;
			AssertEquals(false, header.ReadOnly);

			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var freshHeader = anotherFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(false, freshHeader.ReadOnly);
		}

		public void TestNeedPersonsTab()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(false, header.NeedPersonsTab);
		}

		public void TestAMA_ManifestTypeReadOnly()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();

			AssertEquals(true, header.AMA_ManifestTypeInfo.ReadOnly);
		}

		public void TestContainerType()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<AsycudaContainer>(header.Containers.AddNew());
		}

		public void TestAMA_OA_Declarant_Defaulted()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();

			var orgProxyMainAddressPk = header.Branch.OrgProxy.MainAddress.PK;
			AssertNotEquals("Org Proxy should have a Main Address set.", ZGuid.Empty, orgProxyMainAddressPk);
			AssertEquals("Declarant should be defaulted to the Org Proxy Address Value.", orgProxyMainAddressPk, header.AMA_OA_Declarant);
		}

		public void TestAMA_OA_Declarant_Caption()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(header.AMA_OA_DeclarantInfo);
			AssertEquals("Declarant", propertyData.Caption);
		}

		public void TestDeclarantDefaultAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "FULLNAME 1";

			org.MainAddress.OA_Address1 = "MAIN ADDRESS";
			org.MainAddress.OA_Address2 = "ADDRESS 1";
			org.MainAddress.OA_City = "CITY 1";
			org.MainAddress.OA_State = "STATE 1";
			org.MainAddress.OA_PostCode = "102034";

			var officeAddress = org.Addresses.AddNew(OrgAddressType.Office, isDefault: true);
			officeAddress.OA_Address1 = "OFFICE ADDRESS";
			officeAddress.OA_Address2 = "ADDRESS 2";
			officeAddress.OA_City = "CITY 2";
			officeAddress.OA_State = "STATE 2";
			officeAddress.OA_PostCode = "102035";

			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, isDefault: true);
			deliveryAddress.OA_Address1 = "DELIVERY ADDRESS";
			deliveryAddress.OA_Address2 = "ADDRESS 3";
			deliveryAddress.OA_City = "CITY 3";
			deliveryAddress.OA_State = "STATE 3";
			deliveryAddress.OA_PostCode = "102036";

			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_OA_Declarant_ZAddress.OrgPK = org.PK;

			AssertEquals("Default address set", officeAddress, header.AMA_OA_Declarant_ZAddress.OrgAddress);
		}

		public void TestAddressedMemberState_Defaulted()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MS;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "DE", "Germany", startDate, endDate);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var header = (AsycudaManifestHeader)GetNewBusinessObject();
				AssertEquals(CountryCodes.Germany, header.AddressedMemberState);
				AssertEquals(CountryCodes.Germany, header.AMA_RN_NKCountry);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var header = (AsycudaManifestHeader)GetNewBusinessObject();
				AssertEquals(CountryCodes.EuropeanUnion, header.AddressedMemberState);
				AssertEquals(CountryCodes.EuropeanUnion, header.AMA_RN_NKCountry);
			}
		}

		public void TestDeclarantEori()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_OA_Declarant = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertNull(header.Declarant);
				AssertEquals("No Declarant, empty ZString returned.", ZString.Empty, header.DeclarantEori);

				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_FullName = "Name123";
				var mainAddress = orgHeader.MainAddress;

				header.AMA_OA_Declarant = orgHeader.MainAddress.PK;
				AssertNotNull(header.Declarant);
				AssertEquals("Declarant set, no EORI, empty ZString returned.", ZString.Empty, header.DeclarantEori);

				mainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.Germany);
				mainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", CountryCodes.UnitedKingdom);
				AssertEquals("EORI of Declarant is returned.", "DE654321", header.DeclarantEori);
			});
		}

		public void TestDefaultTransportModeForManifestType_ICS2TransportModeRAI_Enabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ICS2TransportModeRAI, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
				AssertEquals("Should be 'RAI' for Carrier Manifest and enable ICS2RAI functionality", TransportModes.Rail, header.AMA_TransportMode);

				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header.AMA_ManifestType = "XYZ";
				AssertEquals("Should be 'AIR' for Forwarder Manifest", TransportModes.Air, header.AMA_TransportMode);
			}
		}

		public void TestDefaultTransportModeForManifestType_ICS2TransportModeRAI_Disabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ICS2TransportModeRAI, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_ManifestType = "ENS";
				AssertEquals("Should be empty for Carrier Manifest and not enabled ICS2RAI functionality", ZString.Empty, header.AMA_TransportMode);

				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header.AMA_ManifestType = "ABC";
				AssertEquals("Should be 'AIR' for Forwarder Manifest", TransportModes.Air, header.AMA_TransportMode);
			}
		}

		public void TestIsSupplementaryDeclarantsEnabled()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(true, header.IsSupplementaryDeclarantsEnabled);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;
			AssertEquals(false, header.IsSupplementaryDeclarantsEnabled);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			AssertEquals(false, header.IsSupplementaryDeclarantsEnabled);
		}

		public void TestFactorySave_ShouldCleanSupplementaryDeclarants_WhenIsSupplementaryDeclarantsEnabledIsFalse()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill = header.Bills.AddNew();
			bill.SupplementaryDeclarants.AddNew();

			AssertCollectionCleanedAfterSave(bill.SupplementaryDeclarants,
				() => header.IsSupplementaryDeclarantsEnabled,
				() => header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24);
		}

		public void TestIsBillScreeningsEnabled()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(true, header.IsBillScreeningsEnabled);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			AssertEquals(false, header.IsBillScreeningsEnabled);
		}

		public void TestFactorySave_ShouldCleanBillScreenings_WhenIsBillScreeningsEnabledIsFalse()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill = header.Bills.AddNew();
			bill.BillScreenings.AddNew();

			AssertCollectionCleanedAfterSave(bill.BillScreenings,
				() => header.IsBillScreeningsEnabled,
				() => header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50);
		}

		public void TestIsCusSupplyChainActorReferencesEnabled()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(true, header.IsCusSupplyChainActorReferencesEnabled);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;
			AssertEquals(false, header.IsCusSupplyChainActorReferencesEnabled);
		}

		public void TestFactorySave_ShouldCleanCusSupplyChainActorReferences_WhenIsCusSupplyChainActorReferencesEnabledIsFalse()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill = header.Bills.AddNew();
			var cusSupplyChainActorReference = bill.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReference.CFR_Reference = "123";
			cusSupplyChainActorReference.CFR_Code = "CE";

			AssertCollectionCleanedAfterSave(bill.CusSupplyChainActorReferences,
				() => header.IsCusSupplyChainActorReferencesEnabled,
				() => header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24);
		}

		public void TestIsAsycudaTransportMeansEnabled()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_TransportMode = TransportModes.Road;
			AssertEquals(true, header.IsAsycudaTransportMeansEnabled);

			header.AMA_TransportMode = TransportModes.Rail;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;
			AssertEquals(true, header.IsAsycudaTransportMeansEnabled);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
			AssertEquals(false, header.IsAsycudaTransportMeansEnabled);

			header.AMA_TransportMode = TransportModes.Air;
			AssertEquals(false, header.IsAsycudaTransportMeansEnabled);
		}

		public void TestFactorySave_ShouldCleanAsycudaTransportMeans_WhenIsAsycudaTransportMeansEnabledIsFalse()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_TransportMode = TransportModes.Road;
			var bill = header.Bills.AddNew();
			bill.AsycudaTransportMeans.AddNew();

			AssertCollectionCleanedAfterSave(bill.AsycudaTransportMeans,
				() => header.IsAsycudaTransportMeansEnabled,
				() => header.AMA_TransportMode = TransportModes.Air);
		}

		public void TestIsAdditionalInfosEnabled()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(true, header.IsAdditionalInfosEnabled);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			AssertEquals(false, header.IsAdditionalInfosEnabled);
		}

		public void TestFactorySave_ShouldCleanAdditionalInfos_WhenIsAdditionalInfosEnabledIsFalse()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill = header.Bills.AddNew();
			bill.AdditionalInfos.AddNew();

			AssertCollectionCleanedAfterSave(bill.AdditionalInfos,
				() => header.IsAdditionalInfosEnabled,
				() => header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50);
		}

		public void TestIsAdditionalFiscalReferenceEnabled()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(false, header.IsAdditionalFiscalReferenceEnabled);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
			AssertEquals(true, header.IsAdditionalFiscalReferenceEnabled);
		}

		public void TestFactorySave_ShouldCleanAdditionalFiscalReference_WhenIsAdditionalFiscalReferenceEnabledIsFalse()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
			var bill = header.Bills.AddNew();
			var additionalFiscalReference = bill.AdditionalFiscalReferences.AddNew();
			additionalFiscalReference.CFR_Code = EUICS2AdditionalFiscalReferenceTypes.Codes.FR5;
			additionalFiscalReference.CFR_Reference = "FR5";

			AssertCollectionCleanedAfterSave(bill.AdditionalFiscalReferences,
				() => header.IsAdditionalFiscalReferenceEnabled,
				() => header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44);
		}

		public void TestFactorySave_CleanPackedItemsValues()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var bill1 = header.Bills.AddNew();
			var packedItem1 = bill1.Packs.AddNew().PackedItem;
			var bill2 = header.Bills.AddNew();
			var packedItem2 = bill2.Packs.AddNew().PackedItem;
			var packedItem3 = bill2.Packs.AddNew().PackedItem;
			packedItem1.API_GoodsValue = 100;
			packedItem2.API_GoodsValue = 100;
			packedItem3.API_GoodsValue = 100;
			packedItem1.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			packedItem2.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			packedItem3.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			packedItem1.API_TypeOfGoods = "Type1";
			packedItem2.API_TypeOfGoods = "Type1";
			packedItem3.API_TypeOfGoods = "Type1";

			CombineAssertions("IsPackedItemGoodsValueAndTypeOfGoodsEnabled == true", () =>
			{
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				Factory.Save();
				AssertEquals("packedItem1.API_GoodsValue", 100m, packedItem1.API_GoodsValue);
				AssertEquals("packedItem2.API_GoodsValue", 100m, packedItem2.API_GoodsValue);
				AssertEquals("packedItem3.API_GoodsValue", 100m, packedItem3.API_GoodsValue);
				AssertEquals("packedItem1.API_RX_NKGoodsValueCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, packedItem1.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItem2.API_RX_NKGoodsValueCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, packedItem2.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItem3.API_RX_NKGoodsValueCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, packedItem3.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItem1.API_TypeOfGoods", "Type1", packedItem1.API_TypeOfGoods);
				AssertEquals("packedItem2.API_TypeOfGoods", "Type1", packedItem2.API_TypeOfGoods);
				AssertEquals("packedItem3.API_TypeOfGoods", "Type1", packedItem2.API_TypeOfGoods);
			});
			CombineAssertions("IsPackedItemGoodsValueAndTypeOfGoodsEnabled == false", () =>
			{
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				Factory.Save();
				AssertEquals("packedItem1.API_GoodsValue", ZDecimal.Zero, packedItem1.API_GoodsValue);
				AssertEquals("packedItem2.API_GoodsValue", ZDecimal.Zero, packedItem2.API_GoodsValue);
				AssertEquals("packedItem3.API_GoodsValue", ZDecimal.Zero, packedItem3.API_GoodsValue);
				AssertEquals("packedItem1.API_RX_NKGoodsValueCurrency", ZString.Empty, packedItem1.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItem2.API_RX_NKGoodsValueCurrency", ZString.Empty, packedItem2.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItem3.API_RX_NKGoodsValueCurrency", ZString.Empty, packedItem3.API_RX_NKGoodsValueCurrency);
				AssertEquals("packedItem1.API_TypeOfGoods", ZString.Empty, packedItem1.API_TypeOfGoods);
				AssertEquals("packedItem2.API_TypeOfGoods", ZString.Empty, packedItem2.API_TypeOfGoods);
				AssertEquals("packedItem3.API_TypeOfGoods", ZString.Empty, packedItem2.API_TypeOfGoods);
			});
		}

		public void TestReceptacles()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var collection = header.Receptacles;
			AssertEquals("", header.ReceptacleId);

			collection.AddNew("", "357159");
			collection.AddNew("", "159357");
			AssertEquals("357159,159357", header.ReceptacleId);
		}

		public void TestReceptacleId_ReadOnly()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(true, header.ReceptacleIdInfo.ReadOnly);
		}

		public void TestIsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator()
		{
			CombineAssertions("IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator", () =>
			{
				var header = (AsycudaManifestHeader)GetNewBusinessObject();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator needs SEA/IWT and F11/F12.", false, header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
				AssertEquals("IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator needs SEA/IWT and F11/F12.", false, header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator);

				header.AMA_TransportMode = TransportTypeList.Codes.Sea;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
				AssertEquals($"{header.AMA_TransportMode}, {header.SpecificCircumstanceIndicator}, IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator should be true.", true, header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator);

				header.AMA_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F12;
				AssertEquals("IWT, F12, IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator should be true.", true, header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
				AssertEquals("IWT, F11, IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator should be true.", true, header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator);
			});
		}

		public void TestGetMessageSendingNotificationHelper()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany, eunDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Norway, Core.Constants.CountryCodes.Norway, wcoDataGrouping);
			Factory.Save();

			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Norway;
			AssertType<MessageSendingNotificationHelper>(header.GetMessageSendingNotificationHelper());
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		protected override Type ExpectedEUMemberStateCommunicationType => typeof(RequestHeader);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues() => new() { AutoAsycudaManifestHeader.Schema.AMA_OA_Declarant };

		void AssertCollectionCleanedAfterSave(IBusinessObjectCollection collection, Func<bool> controlPropertyGetter, Action controlPropertyToNotEnabledSetter)
		{
			AssertEquals("Precondition", 1, collection.Count);
			AssertEquals("Precondition", true, controlPropertyGetter());
			Factory.Save();
			AssertEquals("Nothing cleaned up after save", 1, collection.Count);

			controlPropertyToNotEnabledSetter();
			CombineAssertions("controlProperty = false", () =>
			{
				AssertEquals("Precondition", false, controlPropertyGetter());
				AssertEquals("Nothing cleaned up before save", 1, collection.Count);
			});

			Factory.Save();
			AssertEquals("Collection is cleaned up", 0, collection.Count);
		}
	}
}
