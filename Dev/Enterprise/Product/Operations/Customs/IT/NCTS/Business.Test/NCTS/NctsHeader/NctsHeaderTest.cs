using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.Shared;
using NctsConstants = Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using NctsMessageStatusList = Enterprise.Customs.EU.NCTS.Business.NctsMessageStatusList;
using NCTSTestHelper = Enterprise.Customs.EU.NCTS.Business.Testing.NCTSTestHelper;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
sealed class NctsHeaderTest : EU.NCTS.Business.Testing.NctsHeaderAbstractTest
{
	public void TestIsInPhase5TransitionPeriod()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var effectiveDate = new ZDateTime(2025, 01, 20);
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, effectiveDate, true))
		{
			AssertEquals("When MRN Date is empty", false, nctsHeader.IsInPhase5TransitionPeriod);

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_EntryDate = effectiveDate;
			AssertEquals("When MRN Date is in phase 5 transition period", true, nctsHeader.IsInPhase5TransitionPeriod);

			movementHeader.BM_EntryDate = new ZDateTime(2025, 01, 31);
			AssertEquals("When MRN Date is outside phase 5 transition period", false, nctsHeader.IsInPhase5TransitionPeriod);
		}
	}

	public void TestValidationType_Phase4()
	{
		nctsHeader.BH_ApplicationCode = "NCT";
		AssertType<NctsHeaderValidation>("Validation type", nctsHeader.Validation);
	}

	public void TestValidationType_Phase5()
	{
		nctsHeader.BH_ApplicationCode = "NC5";
		AssertType<NctsHeaderPhase5Validation>("Validation type", nctsHeader.Validation);
	}

	public void TestSetAsFailedFromTrasmission()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.MovementHeader.BM_EntryDate = ZDate.Today;
		AssertEquals("Empty status expected", ZString.Empty, nctsHeader.BH_MessageStatus);

		nctsHeader.SetAsFailedFromTransmission();
		AssertEquals("MEE expected", "MEE", nctsHeader.BH_MessageStatus);
		AssertEquals("Empty BM_EntryDate expected", ZDate.Empty, nctsHeader.MovementHeader.BM_EntryDate);
	}

	public void TestEmptyAuthorizationIfNecessary()
	{
		nctsHeader = Factory.NewDepartureNctsHeader();

		var departureMovement = nctsHeader.MovementHeader;
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		nctsHeader.Authorization = "1234567";

		AssertEquals("Authorization field not empty for TIR declaration", "1234567", nctsHeader.Authorization);

		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;

		AssertEquals("Authorization field empty for TIR declaration", ZString.Empty, nctsHeader.Authorization);
	}

	public void TestNoAuthorizationDefault()
	{
		nctsHeader = Factory.NewDepartureNctsHeader();

		var departureMovement = nctsHeader.MovementHeader;
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;

		AssertEquals("Empty default for Authorization", ZString.Empty, nctsHeader.Authorization);
	}

	public void TestAuthorizationState()
	{
		nctsHeader = Factory.NewDepartureNctsHeader();

		var departureMovement = nctsHeader.MovementHeader;
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
		nctsHeader.Authorization = "1234567";

		AssertEquals("Authorization enabled when declaration is not TIR", false, nctsHeader.AuthorizationInfo.ReadOnly);
		AssertEquals("Authorization field not empty for TIR declaration", "1234567", nctsHeader.Authorization);

		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;

		AssertEquals("Authorization not enabled for TIR declaration", true, nctsHeader.AuthorizationInfo.ReadOnly);
		AssertEquals("Authorization field empty for TIR declaration", ZString.Empty, nctsHeader.Authorization);
	}

	public void TestAuthorization()
	{
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureMovement = nctsHeader.MovementHeader;
		JobDocAddress consignor = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);

		var orgHeader = Factory.New<OrgHeader>();

		consignor.E2_OA_Address = orgHeader.MainAddress.PK;
		var authorization = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		authorization.CPH_Number = "1234567";
		authorization.CPH_Type = "ACR";
		authorization.CPH_OH_PermitHolder = consignor.OrganisationPK;

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals("Authorization", ZString.Empty, nctsHeader.Authorization);
			AssertAuthorizationInfo(itemExpected: false);
		});

		nctsHeader.Authorization = authorization.CPH_Number;
		CombineAssertions("Item Added", () =>
		{
			AssertEquals("Authorization", "1234567", nctsHeader.Authorization);
			AssertAuthorizationInfo(itemExpected: true);
		});

		nctsHeader.Authorization = "7777777";
		CombineAssertions("Item Updated", () =>
		{
			AssertEquals("Authorization", "7777777", nctsHeader.Authorization);
			AssertAuthorizationInfo(itemExpected: false);
		});

		nctsHeader.Authorization = ZString.Empty;
		CombineAssertions("Item Deleted", () =>
		{
			AssertEquals("Authorization", ZString.Empty, nctsHeader.Authorization);
			AssertAuthorizationInfo(itemExpected: false);
		});
	}

	public void TestAuthorizationMaxLength()
	{
		AssertEquals("MaxLength", 7, nctsHeader.AuthorizationInfo.MaxLength);
	}

	void AssertAuthorizationInfo(bool itemExpected)
	{
		var authorizationList = GetAuthorizationList(Factory);
		if (!itemExpected)
		{
			AssertEquals("Expected no items", 0, authorizationList.Length);
		}
		else
		{
			AssertEquals("Expected single item", 1, authorizationList.Length);
			var singleAuthorization = authorizationList[0];
			AssertEquals("CPH_Number", nctsHeader.Authorization, singleAuthorization.CPH_Number);
			AssertEquals("CPH_Type", "ACR", singleAuthorization.CPH_Type);
			AssertEquals("CPH_OH_PermitHolder", nctsHeader.Consignor.OrganisationPK, singleAuthorization.CPH_OH_PermitHolder);
		}
	}

	CusAuthorisationHeader[] GetAuthorizationList(BusinessObjectFactory factory)
	{
		var query = new ZQuery(CusPermitHeaderSchema.CPH_Number, nctsHeader.Authorization);
		query.AddToFilter(CusPermitHeaderSchema.CPH_Type, "ACR");
		return factory.Load<CusAuthorisationHeader>(query);
	}

	public void TestDeclarantAddressDefault()
	{
		var org = Factory.New<OrgHeader>();
		var mainAddress = org.MainAddress;
		var address = org.Addresses.AddNew();
		nctsHeader.Principal.E2_OA_Address = address.PK;
		nctsHeader.RepresentationType = RepresentationTypeList.Codes._2Direct;
		AssertEquals("DeclarantAddressPKInfo.ReadOnly", false, nctsHeader.DeclarantAddressPKInfo.ReadOnly);
		AssertEquals("DeclarantAddressPK", address.PK, nctsHeader.DeclarantAddressPK);
		AssertEquals("DeclarantOrgPK", org.PK, nctsHeader.DeclarantOrgPK);
		nctsHeader.RepresentationType = RepresentationTypeList.Codes._1Self;
		AssertEquals("DeclarantAddressPKInfo.ReadOnly", true, nctsHeader.DeclarantAddressPKInfo.ReadOnly);
		AssertEquals("DeclarantAddressPK", ZGuid.Empty, nctsHeader.DeclarantAddressPK);
		AssertEquals("DeclarantOrgPK", ZGuid.Empty, nctsHeader.DeclarantOrgPK);
		nctsHeader.RepresentationType = RepresentationTypeList.Codes._3Indirect;
		AssertEquals("DeclarantAddressPKInfo.ReadOnly", false, nctsHeader.DeclarantAddressPKInfo.ReadOnly);
		AssertEquals("DeclarantAddressPK", address.PK, nctsHeader.DeclarantAddressPK);
		AssertEquals("DeclarantOrgPK", org.PK, nctsHeader.DeclarantOrgPK);
		nctsHeader.DeclarantOrgPK = ZGuid.Empty;
		AssertEquals("DeclarantAddressPK", ZGuid.Empty, nctsHeader.DeclarantAddressPK);
		nctsHeader.DeclarantOrgPK = org.PK;
		AssertEquals("DeclarantAddressPK", mainAddress.PK, nctsHeader.DeclarantAddressPK);
	}

	public void TestSubscriberMaxLength()
	{
		AssertEquals("MaxLength", 3, nctsHeader.SubscriberInfo.MaxLength);
	}

	public void TestSubscriber()
	{
		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals("Subscriber", ZString.Empty, nctsHeader.Subscriber);
			AssertSubscriberInfo(itemExpected: false);
		});

		nctsHeader.Subscriber = "XXX";
		CombineAssertions("Item Added", () =>
		{
			AssertEquals("Subscriber", "XXX", nctsHeader.Subscriber);
			AssertSubscriberInfo(itemExpected: true, staffCode: "XXX");
		});

		nctsHeader.Subscriber = "YYY";
		CombineAssertions("Item Updated", () =>
		{
			AssertEquals("Subscriber", "YYY", nctsHeader.Subscriber);
			AssertSubscriberInfo(itemExpected: true, staffCode: "YYY");
		});

		nctsHeader.Subscriber = ZString.Empty;
		CombineAssertions("Item Deleted", () =>
		{
			AssertEquals("Subscriber", ZString.Empty, nctsHeader.Subscriber);
			AssertSubscriberInfo(itemExpected: false);
		});
	}

	public void TestSubscriberPersistedInfoAfterSave()
	{
		nctsHeader.Subscriber = "YYY";
		Factory.Save();
		CombineAssertions("CP_GS_NKStaff = YYY", () => AssertSubscriberInfo(itemExpected: true, staffCode: "YYY"));

		nctsHeader.Subscriber = "XXX";
		Factory.Save();
		CombineAssertions("CP_GS_NKStaff = XXX", () => AssertSubscriberInfo(itemExpected: true, staffCode: "XXX"));

		nctsHeader.Subscriber = ZString.Empty;
		Factory.Save();
		CombineAssertions("Empty CP_GS_NKStaff", () => AssertSubscriberInfo(itemExpected: false));
	}

	public void TestDeleteNctsHeaderDeletesSubscriber()
	{
		var separateFactory = new BusinessObjectFactory();
		nctsHeader.Subscriber = "YYY";
		Factory.Save();
		AssertEquals("Saved", 1, GetSubscriberList(separateFactory).Length);

		nctsHeader.Delete();
		Factory.Save();
		AssertEquals("Deleted", 0, GetSubscriberList(separateFactory).Length);
	}

	public void TestSubscriberLoadsCusInBondPersonOfTypeSbr()
	{
		var separateFactory = new BusinessObjectFactory();
		var nctsHeaderSeparateFactory = separateFactory.New<NctsHeader>();
		var inBondPersonSeparateFactory = separateFactory.New<CusInBondPersonForTest>();
		inBondPersonSeparateFactory.CP_Type = "XXX";
		inBondPersonSeparateFactory.CP_GS_NKStaff = "EEE";
		inBondPersonSeparateFactory.CP_BH_Header = nctsHeaderSeparateFactory.PK;
		separateFactory.Save();

		var nctsHeaderMainFactory = Factory.Load<NctsHeader>(nctsHeaderSeparateFactory.PK);
		AssertEquals("Subscriber is missing", ZString.Empty, nctsHeaderMainFactory.Subscriber);

		var subscriberMainFactory = Factory.New<NctsSubscriber>();
		subscriberMainFactory.CP_GS_NKStaff = "NNN";
		subscriberMainFactory.CP_BH_Header = nctsHeaderMainFactory.PK;
		AssertEquals("Existing subscriber", "NNN", nctsHeaderMainFactory.Subscriber);
	}

	[ExpectNoExceptions]
	public void TestMultipleSubscribersNotAllowedAtDatabaseLevel()
	{
		var subscriber1 = Factory.New<NctsSubscriber>();
		subscriber1.CP_BH_Header = nctsHeader.PK;
		subscriber1.CP_GS_NKStaff = "AAA";
		var subscriber2 = Factory.New<NctsSubscriber>();
		subscriber2.CP_BH_Header = nctsHeader.PK;
		subscriber2.CP_GS_NKStaff = "BBB";

		NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(System.Data.SqlClient.SqlException), $"Cannot insert duplicate key row in object 'dbo.CusInBondPerson' with unique index 'NR_UX__CP_BH_Header_CP_Type'. The duplicate key value is ({nctsHeader.PK}, SBR)."), "Attempt to add multiple subscribers to the same NCTS Header");
	}

	public void TestIAeoCertificateSupporterMembers()
	{
		var declarant = Factory.New<OrgHeader>();

		CombineAssertions("Assert NctsHeader as IAeoCertificateSupporter", () =>
		{
			var nctsHeaderAsIAeoCertificateSupporter = (IAeoCertificateSupporter)nctsHeader;

			AssertNull("Supplier", nctsHeaderAsIAeoCertificateSupporter.Supplier);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CO1", nctsHeader.Consignor, "2");
			AssertNotNull("Supplier", nctsHeaderAsIAeoCertificateSupporter.Supplier);
			AssertSame("Supplier", nctsHeader.Consignor.Organisation, nctsHeaderAsIAeoCertificateSupporter.Supplier);

			AssertNull("Importer", nctsHeaderAsIAeoCertificateSupporter.Importer);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", nctsHeader.Consignee, "3");
			AssertNotNull("Importer", nctsHeaderAsIAeoCertificateSupporter.Importer);
			AssertSame("Importer", nctsHeader.Consignee.Organisation, nctsHeaderAsIAeoCertificateSupporter.Importer);

			AssertNull("Declarant", nctsHeaderAsIAeoCertificateSupporter.Declarant);
			nctsHeader.DeclarantAddressPK = declarant.MainAddress.PK;
			AssertNotNull("Declarant", nctsHeaderAsIAeoCertificateSupporter.Declarant);
			AssertSame("Declarant", nctsHeader.DeclarantAddress.Header, nctsHeaderAsIAeoCertificateSupporter.Declarant);

			AssertEquals("ShouldAddY022Certificate", true, nctsHeaderAsIAeoCertificateSupporter.ShouldAddY022Certificate);
			AssertEquals("ShouldAddY023Certificate", true, nctsHeaderAsIAeoCertificateSupporter.ShouldAddY023Certificate);
		});
	}

	public void TestAeoCertificateHasBeenUpdateChangingConsignor()
	{
		var changeConsignorAction = new Action<NctsHeader>(nctsHeader =>
		{
			NCTSTestHelper.CreateJobDocAddressForTest(
				Factory
				, traderId: "CO1"
				, nctsHeader.Consignor
				, "2"
				, configureOrgHeaderAction: x => x.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO022"));
		});

		AssertThatAeoCertificateHasBeenUpdatedChangingRequiredCondition(changeConsignorAction, "Y022", "AEO022");
	}

	public void TestAeoCertificateHasBeenUpdateChangingConsignee()
	{
		var changeConsigneeAction = new Action<NctsHeader>(nctsHeader =>
		{
			NCTSTestHelper.CreateJobDocAddressForTest(
				Factory
				, traderId: "CE1"
				, nctsHeader.Consignee
				, suffix: "3"
				, configureOrgHeaderAction: x => x.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO023"));
		});

		AssertThatAeoCertificateHasBeenUpdatedChangingRequiredCondition(changeConsigneeAction, "Y023", "AEO023");
	}

	public void TestAeoCertificateHasBeenUpdateChangingDeclarant()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO024");

		var changeDeclarantAction = new Action<NctsHeader>(nctsHeader => nctsHeader.DeclarantAddressPK = declarant.MainAddress.PK);
		AssertThatAeoCertificateHasBeenUpdatedChangingRequiredCondition(changeDeclarantAction, "Y024", "AEO024");
	}

	void AssertThatAeoCertificateHasBeenUpdatedChangingRequiredCondition(Action<NctsHeader> changeRequiredConditionAction, ZString aeoCode, ZString aeoReference)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		var goodItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		AssertEquals("No default supporting document expected", 0, goodItem.SupportingDocuments.Count);

		changeRequiredConditionAction(nctsHeader);

		var nctsSupportingDocuments = goodItem.SupportingDocuments.Cast<NctsSupportingDocument>();
		AssertEquals($"The {aeoCode} Supporting Document is expected, Count", 1, nctsSupportingDocuments.Count(x => x.CSI_Code == aeoCode));
		var aeoSupportingDocument = nctsSupportingDocuments.SingleOrDefault(x => x.CSI_Code == aeoCode);
		CombineAssertions($"Check {aeoCode} Supporting Document", () =>
		{
			AssertEquals("Code", aeoCode, aeoSupportingDocument.CSI_Code);
			AssertEquals("ReferenceNumber", aeoReference, aeoSupportingDocument.CSI_ReferenceNumber);
		});
	}

	void AssertSubscriberInfo(bool itemExpected, string staffCode = null)
	{
		var subscriberList = GetSubscriberList(Factory);
		if (!itemExpected)
		{
			AssertEquals("Expected no items", 0, subscriberList.Length);
		}
		else
		{
			AssertEquals("Expected single item", 1, subscriberList.Length);
			var singleSubscriber = subscriberList[0];
			AssertEquals("CP_GS_NKStaff", staffCode, singleSubscriber.CP_GS_NKStaff);
			AssertEquals("CP_OC_Contact", ZGuid.Empty, singleSubscriber.CP_OC_Contact);
			AssertEquals("CP_DateOfBirth", ZDateTime.Empty, singleSubscriber.CP_DateOfBirth);
			AssertEquals("CP_FullName", ZString.Empty, singleSubscriber.CP_FullName);
			AssertEquals("CP_Gender", ZString.Empty, singleSubscriber.CP_Gender);
			AssertEquals("CP_BH_Header", nctsHeader.PK, singleSubscriber.CP_BH_Header);
			Assert("CP_HasHazmatEndorsment", !singleSubscriber.CP_HasHazmatEndorsment);
			AssertEquals("CP_PER", ZGuid.Empty, singleSubscriber.CP_PER);
			AssertEquals("CP_Type", "SBR", singleSubscriber.CP_Type);
			AssertEquals("CP_RN_NKNationality", ZString.Empty, singleSubscriber.CP_RN_NKNationality);
		}
	}

	NctsSubscriber[] GetSubscriberList(BusinessObjectFactory factory)
	{
		var query = new ZQuery(CusInBondPersonSchema.CP_BH_Header, nctsHeader.PK);
		query.AddToFilter(CusInBondPersonSchema.CP_Type, NctsSubscriber.Constants.CodeType);
		return factory.Load<NctsSubscriber>(query);
	}

	public void TestICustomsProfileDataProviderMembers()
	{
		nctsHeader.BH_CustomsProfile = "1234";
		CombineAssertions(() =>
		{
			var nctsHeaderAsICustomsProfileDataProvider = (ICustomsProfileDataProvider)nctsHeader;
			AssertEquals("CustomsProfile", nctsHeader.BH_CustomsProfile, nctsHeaderAsICustomsProfileDataProvider.CustomsProfile);
		});
	}

	public void TestIDeclarantProviderMembers()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		nctsHeader.Principal.E2_OA_Address = orgAddress.PK;
		nctsHeader.RepresentationType = "DIR";

		CombineAssertions("Assert IDeclarantProviderMembers", () =>
		{
			var nctsHeaderAsIDeclarantProvider = (IDeclarantProvider)nctsHeader;
			AssertEquals("RepresentativeType", "DIR", nctsHeaderAsIDeclarantProvider.RepresentativeType);
			AssertNotNull("DeclarantAddress", nctsHeaderAsIDeclarantProvider.DeclarantAddress);
			AssertSame("DeclarantAddress", nctsHeader.DeclarantAddress, nctsHeaderAsIDeclarantProvider.DeclarantAddress);
		});
	}

	public void TestNodeMaxLength()
	{
		AssertEquals("MaxLength", 20, nctsHeader.BH_CustomsProfileInfo.MaxLength);
	}

	public void TestGetCustomsOfficeRequirementHelper()
	{
		AssertType<NctsHeaderCustomsOfficeRequirementHelper>(nctsHeader.CustomsOfficeRequirementHelper);
	}

	public void TestOriginStateReadOnly()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
		var goodsItem = header.MovementHeader.GoodsItems.AddNew();
		goodsItem.BY_RN_NKCountryOfOrigin = CountryCodes.Italy;
		goodsItem.BY_RW_NKOriginState = "AN";
		Assert(!goodsItem.OriginStateReadOnly);

		goodsItem.BY_RN_NKCountryOfOrigin = CountryCodes.Belgium;
		goodsItem.BY_RW_NKOriginState = "";
		Assert(goodsItem.OriginStateReadOnly);
	}

	public void TestOriginStateClearedOnCountryChange()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
		var goodsItem = header.MovementHeader.GoodsItems.AddNew();
		goodsItem.BY_RN_NKCountryOfOrigin = CountryCodes.Italy;
		goodsItem.BY_RW_NKOriginState = "AN";
		AssertNotNull(goodsItem.BY_RW_NKOriginState);

		goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Belgium;
		AssertNullOrEmpty(goodsItem.BY_RW_NKOriginState);

		goodsItem.BY_RW_NKOriginState = "AN";
		goodsItem.BY_RN_NKCountryOfOrigin = "";
		AssertNullOrEmpty(goodsItem.BY_RW_NKOriginState);
	}

	public void TestSetRepresentationTypeTriggerDefermentAccountNumberDefaulting()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureMovement = header.MovementHeader;
		departureMovement.PaymentParty = NctsPaymentPartyList.Codes.ConsigneesAccount;
		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		SimulateRepresentationTypeValueChange(RepresentationTypeList.Codes._1Self);
		AssertEquals("Source trader is not set, defaulting is not triggered", ZString.Empty, departureMovement.DefermentAccountNumber);

		var requirement = header.DocAddresses.FindOrCreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
		requirement.E2_OA_Address = consignee.MainAddress.PK;
		SimulateRepresentationTypeValueChange(RepresentationTypeList.Codes._1Self);
		AssertEquals("Source trader has no customs codes, defaulting is not triggered", ZString.Empty, departureMovement.DefermentAccountNumber);

		consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "1", Core.Constants.CountryCodes.Italy);
		var cusCodeDAT = consignee.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "2", Core.Constants.CountryCodes.Italy);
		requirement.E2_OA_Address = consignee.MainAddress.PK;
		SimulateRepresentationTypeValueChange(RepresentationTypeList.Codes._1Self);
		AssertEquals("Source trader has 2 customs codes, defaulting is not triggered", ZString.Empty, departureMovement.DefermentAccountNumber);

		consignee.CustomsCodes.RemoveAndDelete(cusCodeDAT);
		SimulateRepresentationTypeValueChange(RepresentationTypeList.Codes._1Self);
		AssertEquals("Source trader has 1 customs code, defaulting is triggered", "1", departureMovement.DefermentAccountNumber);

		void SimulateRepresentationTypeValueChange(ZString valueToSet)
		{
			header.RepresentationType = ZString.Empty;
			header.RepresentationType = valueToSet;
			Factory.ClearCachedValue<CodeDescriptionPairList>($"{consignee.PK}_Consignee");
		}
	}

	public void TestGuarantees_Phase4()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		CombineAssertions(() =>
		{
			nctsHeader.Guarantees.AddNew();
			var result = nctsHeader.Guarantees;
			AssertEquals("Parent", nctsHeader.PK, result[0].PW_ParentID);
			AssertType<NctsGuaranteeCollection<NctsGuarantee>>("Type", result);
		});
	}

	public void TestMessages()
	{
		AssertType<ITEDIMessageCollection>($"{nameof(nctsHeader.Messages)} type", nctsHeader.Messages);
	}

	public void TestGetApplicationReference()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "845A")
			.AppendAccountDetail("845A-DEC1", "DEC1")
			.Build();

		AssertEquals($"When fields are not filled, {nameof(nctsHeader.GetApplicationReference)}", "::", nctsHeader.GetApplicationReference());

		nctsHeader.BH_CustomsProfile = "845A-DEC1";
		nctsHeader.Subscriber = "CRR";
		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT279100");
		AssertEquals($"When fields are filled, {nameof(nctsHeader.GetApplicationReference)}", "845A:CRR:IT279100", nctsHeader.GetApplicationReference());
	}

	public void TestISingleWindowRequestDataProviderMembers()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "845A")
			.AppendAccountDetail("845A-DEC1", "DEC1")
			.Build();

		CombineAssertions("When CusEntryNum is provided", () =>
		{
			nctsHeader.BH_CustomsProfile = "845A-DEC1";
			nctsHeader.Subscriber = "CRR";
			nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT279100");
			Factory.NewCusEntryNumber(nctsHeader, entryType: "REG", entryNum: "4 T-123456G", issueDate: ZDate.Today);

			var singleWindowsRequestProvider = (ISingleWindowRequestDataProvider)nctsHeader;
			AssertEquals(nameof(singleWindowsRequestProvider.PK), nctsHeader.PK, singleWindowsRequestProvider.PK);
			AssertEquals(nameof(singleWindowsRequestProvider.ApplicationReference), "845A:CRR:IT279100", singleWindowsRequestProvider.ApplicationReference);
			AssertEquals(nameof(singleWindowsRequestProvider.IssueDate), ZDate.Today, singleWindowsRequestProvider.IssueDate);
			AssertEquals(nameof(singleWindowsRequestProvider.RegisterIncludingSeries), "4 T", singleWindowsRequestProvider.RegisterIncludingSeries);
			AssertEquals(nameof(singleWindowsRequestProvider.RegistrationNumberWithoutCin), "123456", singleWindowsRequestProvider.RegistrationNumberWithoutCin);
			AssertEquals(nameof(singleWindowsRequestProvider.CustomsOffice), "IT279100", singleWindowsRequestProvider.CustomsOffice);
			AssertEquals(nameof(singleWindowsRequestProvider.TableName), NctsHeader.Schema.TableName, singleWindowsRequestProvider.TableName);
		});

		CombineAssertions("When CusEntryNum is not provided", () =>
		{
			var singleWindowsRequestProvider = (ISingleWindowRequestDataProvider)Factory.New<NctsHeader>();
			AssertEquals(nameof(singleWindowsRequestProvider.IssueDate), ZDate.Empty, singleWindowsRequestProvider.IssueDate);
			AssertEquals(nameof(singleWindowsRequestProvider.RegisterIncludingSeries), ZString.Empty, singleWindowsRequestProvider.RegisterIncludingSeries);
			AssertEquals(nameof(singleWindowsRequestProvider.RegistrationNumberWithoutCin), ZString.Empty, singleWindowsRequestProvider.RegistrationNumberWithoutCin);
		});
	}

	public void TestAsIAuthorizationListDataProvider()
	{
		var asIAuthorizationListDataProvider = (IAuthorizationListDataProvider)nctsHeader;
		AssertArrayEqualsByElements(nameof(asIAuthorizationListDataProvider.AuthorizationTypes), new ZString[] { IT.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit }, asIAuthorizationListDataProvider.AuthorizationTypes.ToArray());

		AssertArrayEqualsByElements($"When Consignor is not set, {nameof(asIAuthorizationListDataProvider.GetEligibleHolders)}", Array.Empty<ZGuid>(), asIAuthorizationListDataProvider.GetEligibleHolders().ToArray());

		nctsHeader.Consignor.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
		AssertArrayEqualsByElements($"When Consignor is set, {nameof(asIAuthorizationListDataProvider.GetEligibleHolders)}", new ZGuid[1] { nctsHeader.Consignor.OrganisationPK }, asIAuthorizationListDataProvider.GetEligibleHolders().ToArray());
	}

	public void TestIAuthorizationHeaderDataProviderMembers()
	{
		var nctsHeaderAsIAuthorizationHeaderDataProvider = (IAuthorizationHeaderDataProvider)nctsHeader;

		AssertEquals("AuthorizationNumber", ZString.Empty, nctsHeaderAsIAuthorizationHeaderDataProvider.AuthorizationNumber);

		nctsHeader.Authorization = "123456";
		AssertEquals("AuthorizationNumber", nctsHeader.Authorization, nctsHeaderAsIAuthorizationHeaderDataProvider.AuthorizationNumber);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var requirement = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = orgHeader.MainAddress.PK;

		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: IT.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, permitHolder: nctsHeader.Consignor.OrganisationPK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		nctsHeader.Authorization = "999999";

		var customsOfficeOfTypeDEP = nctsHeader.CustomsOffices.AddNew();
		customsOfficeOfTypeDEP.CY_Code = "DEP";
		customsOfficeOfTypeDEP.CY_Data = "IT137100";

		AssertEquals("HolderPk", orgHeader.PK, nctsHeaderAsIAuthorizationHeaderDataProvider.HolderPk);
	}

	public void TestIAutHeaderWithCusOfficeProviderMembers()
	{
		var nctsHeaderAsIAutHeaderWithCusOfficeProvider = (IAutHeaderWithCusOfficeProvider)nctsHeader;

		AssertEquals("AuthorizationNumber", ZString.Empty, nctsHeaderAsIAutHeaderWithCusOfficeProvider.AuthorizationNumber);
		AssertNull("Authorization", nctsHeaderAsIAutHeaderWithCusOfficeProvider.Authorization);

		AssertEquals("IsExport", true, nctsHeaderAsIAutHeaderWithCusOfficeProvider.IsExport);

		nctsHeader.Authorization = "123456";
		AssertEquals("AuthorizationNumber", nctsHeader.Authorization, nctsHeaderAsIAutHeaderWithCusOfficeProvider.AuthorizationNumber);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var requirement = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = orgHeader.MainAddress.PK;

		var authorizationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: IT.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, permitHolder: nctsHeader.Consignor.OrganisationPK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		nctsHeader.Authorization = "999999";
		AssertEquals("Authorization", authorizationHeader, nctsHeaderAsIAutHeaderWithCusOfficeProvider.Authorization);

		var customsOfficeOfTypeDEP = nctsHeader.CustomsOffices.AddNew();
		customsOfficeOfTypeDEP.CY_Code = "DEP";
		customsOfficeOfTypeDEP.CY_Data = "IT137100";
		AssertEquals("CustomsOffice", customsOfficeOfTypeDEP.CY_Data, nctsHeaderAsIAutHeaderWithCusOfficeProvider.CustomsOffice);
	}

	public void TestEntryNumbersProvider()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		var entryNumbersProvider = nctsHeader.EntryNumbersProvider;
		AssertNotNull(nameof(nctsHeader.EntryNumbersProvider), entryNumbersProvider);
		AssertSame("Cached", entryNumbersProvider, nctsHeader.EntryNumbersProvider);
	}

	public void TestReleaseCodeAndDate()
	{
		var entryNumbersProvider = nctsHeader.EntryNumbersProvider;
		var entryNumber = entryNumbersProvider.InsertOrUpdateReleaseCode("ZZZ", new ZDateTime(2021, 01, 01));

		AssertEquals("Release Code", "ZZZ", nctsHeader.ReleaseCode);
		AssertEquals("MovementReferenceIssueDate", new ZDateTime(2021, 01, 01), nctsHeader.CustomsReleaseIssueDate);
	}

	public void TestCustomsWriteOffDate()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CustomsWriteOffDate", ZDateTime.Empty, nctsHeader.CustomsWriteOffDate);

			nctsHeader.EntryNumbersProvider.InsertOrUpdateEntryNum("IRI", "", new ZDateTime(2024, 05, 14));
			AssertEquals("CustomsWriteOffDate", new ZDateTime(2024, 05, 14), nctsHeader.CustomsWriteOffDate);
		});
	}

	public void TestCustomsWriteOffDateCaption()
	{
		CombineAssertions(() =>
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(nctsHeader.CustomsWriteOffDateInfo);
			AssertEquals("Caption", "Write-off Date", resourceStringData.Caption);
			AssertEquals("ShortCaption", "W.O. Date", resourceStringData.ShortCaption);
		});
	}

	public void TestICustomsEntryApplicationReferenceMembers()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT137100");
		nctsHeader.BH_CustomsProfile = "1234-DEC1";
		nctsHeader.Subscriber = "XXX";

		CombineAssertions(() =>
		{
			var customsEntryApplicationReference = (ICustomsEntryApplicationReference)nctsHeader;
			AssertEquals("CustomsOffice when 'DEP' office is provided", "IT137100", customsEntryApplicationReference.CustomsOffice);
			AssertEquals("Node", "1234", customsEntryApplicationReference.Node);
			AssertEquals("Subscriber", "XXX", customsEntryApplicationReference.Subscriber);
			AssertEquals("CustomsProfile", "1234-DEC1", customsEntryApplicationReference.CustomsProfile);

			nctsHeader.CustomsOffices.RemoveAndDeleteAll();
			AssertEquals("CustomsOffice when 'DEP' office is not provided", ZString.Empty, customsEntryApplicationReference.CustomsOffice);
		});
	}

	public void TestDocumentSupporter()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertNotNull(nameof(NctsHeader.DocumentSupporter), nctsHeader.DocumentSupporter);
		AssertType<NctsHeaderDocumentSupporter>($"{nameof(NctsHeader.DocumentSupporter)} type", nctsHeader.DocumentSupporter);
	}

	public void TestICustomsLinkedObjectAdapterProviderMembers()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var customsLinkedObjectAdapterProvider = nctsHeader as ICustomsLinkedObjectAdapterProvider;

		AssertNotNull("NctsHeader must be implement ICustomsLinkedObjectAdapterProvider", customsLinkedObjectAdapterProvider);
		CombineAssertions("Assert ICustomsLinkedObjectAdapterProviderMembers members", () =>
		{
			AssertType<NctsHeaderCustomsLinkedObjectAdapter>("GetSadCustomsLinkedObjectAdapter", customsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter());
			AssertType<NctsHeaderCustomsLinkedObjectAdapter>("GetNewSingleWindowCustomsLinkedObjectAdapter", customsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter());
			AssertType<NctsHeaderPhase5CustomsLinkedObjectAdapter>("GetNewXmlCustomsLinkedObjectAdapter", customsLinkedObjectAdapterProvider.GetNewXmlCustomsLinkedObjectAdapter());
		});
	}

	public void TestIsNbRejectedAndMok()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();

		AssertEquals("When MessageStatus != MOK and BM_CustomsStatus != NBR, IsNbRejectedAndMok should be false", false, nctsHeader.IsNbRejectedAndMok);

		nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;
		AssertEquals("When MessageStatus = MOK and BM_CustomsStatus != NBR, IsNbRejectedAndMok should be false", false, nctsHeader.IsNbRejectedAndMok);

		nctsHeader.MovementHeader.BM_CustomsStatus = ITEntryStatusList.Codes.NbRejected;
		AssertEquals("When MessageStatus = MOK and BM_CustomsStatus = NBR, IsNbRejectedAndMok should be false", true, nctsHeader.IsNbRejectedAndMok);

		nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationNotSent;
		AssertEquals("When MessageStatus != MOK and BM_CustomsStatus = NBR, IsNbRejectedAndMok should be false", false, nctsHeader.IsNbRejectedAndMok);
	}

	public void TestDepartureMovementStatusNonDepartureNctsHeader()
	{
		AssertEquals("PRE-CONDITION: IsDepartureMovement", false, nctsHeader.IsDepartureMovement);
		AssertEquals("DepartureMovementStatus", ZString.Empty, nctsHeader.DepartureMovementStatus);
	}

	public void TestDepartureMovementStatusDepartureNctsHeader()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		AssertEquals("PRE-CONDITION: IsDepartureMovement", true, departureNctsHeader.IsDepartureMovement);

		departureNctsHeader.MovementHeader.BM_CustomsStatus = "XXX";
		AssertEquals("DepartureMovementStatus", "XXX", departureNctsHeader.DepartureMovementStatus);
	}

	public void TestStatusAllowsSending()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();

		departureNctsHeader.MovementHeader.BM_CustomsStatus = "";
		departureNctsHeader.BH_MessageStatus = "";
		departureNctsHeader.BH_ApplicationCode = "NC5";
		AssertEquals("When Phase5, empty BH_MessageStatus allow sent", true, departureNctsHeader.StatusAllowsSending);

		departureNctsHeader.BH_ApplicationCode = "NCT";
		AssertEquals("When [BH_MessageStatus: Empty, BM_CustomsStatus: Empty], StatusAllowsSending", false, departureNctsHeader.StatusAllowsSending);

		departureNctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
		AssertEquals("When [BH_MessageStatus: MDN, BM_CustomsStatus: Empty], StatusAllowsSending", true, departureNctsHeader.StatusAllowsSending);

		departureNctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
		AssertEquals("When [BH_MessageStatus: MDS, BM_CustomsStatus: Empty], StatusAllowsSending", false, departureNctsHeader.StatusAllowsSending);

		departureNctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors;
		AssertEquals("When [BH_MessageStatus: MEE, BM_CustomsStatus: Empty], StatusAllowsSending", true, departureNctsHeader.StatusAllowsSending);

		departureNctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Ok;
		AssertEquals("When [BH_MessageStatus: MOK, BM_CustomsStatus: Empty], StatusAllowsSending", false, departureNctsHeader.StatusAllowsSending);

		departureNctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.NbRejected;
		AssertEquals("When [BH_MessageStatus: MOK, BM_CustomsStatus: NBR], StatusAllowsSending", true, departureNctsHeader.StatusAllowsSending);

		departureNctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.NbRejected;
		departureNctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
		AssertEquals("When [BH_MessageStatus: MDS, BM_CustomsStatus: NBR], StatusAllowsSending", false, departureNctsHeader.StatusAllowsSending);
	}

	public void TestIsDepartureTabReadOnly_CustomsStatus()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		var departureMovement = departureNctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			AssertEquals($"When BM_CustomsStatus == '{NctsTransitStatusList.Codes.Unknown}', IsDepartureTabReadOnly", false, departureNctsHeader.IsDepartureTabReadOnly);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.NbRejected;
			AssertEquals($"When BM_CustomsStatus == '{NctsTransitStatusList.Codes.NbRejected}', IsDepartureTabReadOnly", true, departureNctsHeader.IsDepartureTabReadOnly);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			AssertEquals($"When BM_CustomsStatus == '{NctsTransitStatusList.Codes.DeclarationRejected}', IsDepartureTabReadOnly", false, departureNctsHeader.IsDepartureTabReadOnly);

			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
			AssertEquals($"When BM_CustomsStatus == '{NctsTransitStatusList.Codes.GoodsWrittenOff}', IsDepartureTabReadOnly", true, departureNctsHeader.IsDepartureTabReadOnly);
		});
	}

	public void TestHasChangesOnSubscriberChanging()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		Factory.Save();
		AssertEquals("PRE-CONDITION: NctsHeader.HasChanges", false, departureNctsHeader.HasChanges);

		departureNctsHeader.Subscriber = "XYZ";
		AssertEquals("POST-CONDITION: NctsHeader.HasChanges", true, departureNctsHeader.HasChanges);
		Factory.Save();

		departureNctsHeader.Subscriber = "XYZ";
		AssertEquals("POST-CONDITION 2: NctsHeader.HasChanges", false, departureNctsHeader.HasChanges);
	}

	public void TestHasChangesOnAuthorizationChanging()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		Factory.Save();
		AssertEquals("PRE-CONDITION: NctsHeader.HasChanges", false, departureNctsHeader.HasChanges);

		departureNctsHeader.Authorization = "XYZ";
		AssertEquals("POST-CONDITION: NctsHeader.HasChanges", true, departureNctsHeader.HasChanges);

		Factory.Save();
		departureNctsHeader.Authorization = "XYZ";
		AssertEquals("POST-CONDITION 2: NctsHeader.HasChanges", false, departureNctsHeader.HasChanges);
	}

	public void TestHeaderContainersType()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		AssertType<NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>>("HeaderContainers Type", departureNctsHeader.DepartureHeaderContainers);
	}

	public void TestSupportingDocumentsType()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>("SupportingDocuments Type", departureNctsHeader.MovementHeader.SupportingDocuments);
	}

	public void TestBillsType()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		AssertType<NctsBillCollection<NctsBill>>("Bills Type", departureNctsHeader.Bills);
	}

	public void TestDefaultCustomsProfile()
	{
		var settings = new Mock<INctsSettings>();
		settings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
		using (ObjectFactory.Substitute(settings.Object))
		{
			var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			var declarantA = Factory.NewWithValidTestData<OrgHeader>();
			declarantA.OH_Code = "AA";
			var declarantB = Factory.NewWithValidTestData<OrgHeader>();
			declarantB.OH_Code = "BB";
			Factory.Save();

			var departureNctsHeader = Factory.NewDepartureNctsHeader();
			AssertEquals("When Company has no Customs Profile, BH_CustomsProfile", "", departureNctsHeader.BH_CustomsProfile);

			CustomsProfileListTestHelper.ClearCustomsProfilesLookupsCache(Factory, currentCompanyPk);
			new AccountCollectionTestBuilder(currentCompanyPk)
				.AppendAccount("11111111111-001", "XXXX")
				.AppendAccountDetail("XXXX-AA", "AA")
				.Build();

			departureNctsHeader = Factory.NewDepartureNctsHeader();
			AssertEquals("When Company has only one node, BH_CustomsProfile", "XXXX-AA", departureNctsHeader.BH_CustomsProfile);

			CustomsProfileListTestHelper.ClearCustomsProfilesLookupsCache(Factory, currentCompanyPk);
			new AccountCollectionTestBuilder(currentCompanyPk)
				.AppendAccount("11111111111-001", "XXXX").AppendAccountDetail("XXXX-AA", "AA")
				.AppendAccount("11111111111-002", "YYYY").AppendAccountDetail("YYYY-BB", "BB")
				.Build();

			departureNctsHeader = Factory.NewDepartureNctsHeader();
			AssertEquals("When Company has more than one node, BH_CustomsProfile", "", departureNctsHeader.BH_CustomsProfile);

			departureNctsHeader.BH_CustomsProfile = "1234";
			departureNctsHeader.DeclarantAddressPK = declarantA.MainAddress.PK;
			AssertEquals("When Declarant has only one node but CustomsProfile is not empty, BH_CustomsProfile", "1234", departureNctsHeader.BH_CustomsProfile);

			departureNctsHeader.BH_CustomsProfile = "";
			departureNctsHeader.DeclarantAddressPK = declarantB.MainAddress.PK;
			AssertEquals("When Declarant has only one node, BH_CustomsProfile", "YYYY-BB", departureNctsHeader.BH_CustomsProfile);
		}
	}

	public void TestDefaultSubscriber()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";

		var staff1 = Factory.NewWithValidTestData<GlbStaff>();
		staff1.GS_Code = "ST1";
		staff1.GS_FullName = "STAFF1 FULL NAME";
		var staff1Wrapper = IT.Business.GlbStaffWrapper.Get(staff1);
		staff1Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";

		var staff2 = Factory.NewWithValidTestData<GlbStaff>();
		staff2.GS_Code = "ST2";
		staff2.GS_FullName = "STAFF2 FULL NAME";
		var staff2Wrapper = IT.Business.GlbStaffWrapper.Get(staff2);
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "5678";

		var staff3 = Factory.NewWithValidTestData<GlbStaff>();
		staff3.GS_Code = "ST3";
		staff3.GS_FullName = "STAFF3 FULL NAME";
		var staff3Wrapper = IT.Business.GlbStaffWrapper.Get(staff3);
		staff3Wrapper.PasswordCollection.AddNew().GP_UserID = "9876";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-A", "DEC1")
			.AppendAccount("11111111111-002", "5678").AppendAccountDetail("5678-B", "DEC1")
			.AppendAccount("11111111111-003", "9876").AppendAccountDetail("9876-C", "DEC1")
			.Build();

		var nctsHeader = Factory.NewDepartureNctsHeader();

		CombineAssertions(() =>
		{
			AssertSubscriber(ZString.Empty, ZString.Empty);
			AssertSubscriber("9999", ZString.Empty);
			AssertSubscriber("1234-A", ZString.Empty);
			AssertSubscriber("5678-B", "ST2");
			AssertSubscriber("8888", "ST2");
			AssertSubscriber(ZString.Empty, "ST2");
			AssertSubscriber("1234-A", ZString.Empty);
			AssertSubscriber("9876-C", "ST3");
		});

		void AssertSubscriber(ZString customsProfile, ZString expectedSubscriber)
		{
			nctsHeader.BH_CustomsProfile = customsProfile;
			AssertEquals("Subscriber", expectedSubscriber, nctsHeader.Subscriber);
		}
	}

	public void TestNode()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		nctsHeader.BH_CustomsProfile = "";
		AssertEquals("Node", "", nctsHeader.Node);

		nctsHeader.BH_CustomsProfile = "1111-DEC1";
		AssertEquals("Node", "1111", nctsHeader.Node);

		nctsHeader.BH_CustomsProfile = "XXXX";
		AssertEquals("Node", "", nctsHeader.Node);
	}

	public void TestCustomsProfileResourceStringData()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertEquals("In Phase5, label must be 'Account'", "Account", nctsHeader.BH_CustomsProfileInfo.HumanReadableName);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertEquals("In Phase4, label must be 'Node'", "Node", nctsHeader.BH_CustomsProfileInfo.HumanReadableName);
	}

	public void TestPropertyCountriesOfRoutingElementType()
	{
		var countryOfRouting = nctsHeader.CountriesOfRouting.AddNew();
		AssertType<CountryOfRouting>(countryOfRouting);
	}

	public void TestAssignDeclarationGoodsItemNumbers()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();
		var goodsItem2 = bill1.GoodsItems.AddNew();

		var bill2 = nctsHeader.Bills.AddNew();
		var goodsItem3 = bill2.GoodsItems.AddNew();

		goodsItem3.BY_DeclarationGoodsItemNumber = 2;

		nctsHeader.AssignDeclarationGoodsItemNumbers();
		CombineAssertions("AssignDeclarationGoodsItemNumbers() reassigning numbers", () =>
		{
			AssertEquals("Bill 1 GoodsItem 1, BY_DeclarationGoodsItemNumber", 3, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 1 GoodsItem 2, BY_DeclarationGoodsItemNumber", 4, goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 2 GoodsItem 1, BY_DeclarationGoodsItemNumber", 2, goodsItem3.BY_DeclarationGoodsItemNumber);
		});

		goodsItem2.BY_DeclarationGoodsItemNumber = 0;
		nctsHeader.AssignDeclarationGoodsItemNumbers(reassignNumbers: true);
		CombineAssertions("AssignDeclarationGoodsItemNumbers() assigning only empty numbers", () =>
		{
			AssertEquals("Bill 1 GoodsItem 1, BY_DeclarationGoodsItemNumber", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 1 GoodsItem 2, BY_DeclarationGoodsItemNumber", 2, goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 2 GoodsItem 1, BY_DeclarationGoodsItemNumber", 3, goodsItem3.BY_DeclarationGoodsItemNumber);
		});
	}

	public void TestSetAsAmendment()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var mrnValue = "24ITQYG08AAB1956J4";
		var mrn = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
		mrn.CE_EntryNum = mrnValue;
		nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.Ok;
		nctsHeader.MovementHeader.BM_CustomsStatus = ITEntryStatusList.Codes.Arrival;
		nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.DeclarationAccepted;

		nctsHeader.SetAsAmendment();

		AssertEquals(nameof(nctsHeader.MovementReferenceNumber), mrnValue, nctsHeader.MovementReferenceNumber);
		AssertEquals(nameof(nctsHeader.EffectiveMessageStatus), string.Empty, nctsHeader.EffectiveMessageStatus);
		AssertEquals(nameof(nctsHeader.MovementHeader.BM_CustomsStatus), string.Empty, nctsHeader.MovementHeader.BM_CustomsStatus);
		AssertEquals(nameof(nctsHeader.MovementHeader.BM_Phase), NctsMovementHeaderTransactionStatusList.Codes.Amendment, nctsHeader.MovementHeader.BM_Phase);
	}

	public void TestSetAsAmendment_MovementReferenceNumber()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.Ok;
		nctsHeader.MovementHeader.BM_CustomsStatus = ITEntryStatusList.Codes.Arrival;
		nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.DeclarationAccepted;

		AssertEquals("PRE-CONDITION: empty MovementReferenceNumber", string.Empty, nctsHeader.MovementReferenceNumber);

		var mrnValue = "24ITQYG08AAB1956J4";
		nctsHeader.SetAsAmendment(mrnValue);

		AssertEquals(nameof(nctsHeader.MovementReferenceNumber), mrnValue, nctsHeader.MovementReferenceNumber);
		AssertEquals(nameof(nctsHeader.EffectiveMessageStatus), string.Empty, nctsHeader.EffectiveMessageStatus);
		AssertEquals(nameof(nctsHeader.MovementHeader.BM_CustomsStatus), string.Empty, nctsHeader.MovementHeader.BM_CustomsStatus);
		AssertEquals(nameof(nctsHeader.MovementHeader.BM_Phase), NctsMovementHeaderTransactionStatusList.Codes.Amendment, nctsHeader.MovementHeader.BM_Phase);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		header.CustomsOffices.AddNew();
		return header;
	}

	public override void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
	{
		// override to be removed in WI00674445
		Assert("Not tested for Arrivals: Arrivals do not support adding SupportingDocuments to NctsHeader", true);
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsHeader;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
	}

	NctsHeader nctsHeader;

	#region CusInBondPersonForTest

	class CusInBondPersonForTest : Customs.Business.CusInBondPerson
	{
		public CusInBondPersonForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}

	#endregion
}

sealed class NctsHeaderTraderValidationTest : NctsTraderValidationTest
{
	protected override JobDocAddress GetConsignorAddress() => Factory.New<NctsHeader>().Consignor;
}
