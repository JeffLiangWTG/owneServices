using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using CusGuaranteeHeader = Enterprise.Customs.EU.Business.CusGuaranteeHeader;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;
using TS = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeader))]
class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestMessages()
	{
		var messages = departureMovement.Messages;
		AssertEquals(departureMovement.Header, messages.Master);
	}

	public void TestSupportingDocuments()
	{
		AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(departureMovement.SupportingDocuments);
	}

	public void TestGetCusSupportingInfoTypes_SupportingDocument()
	{
		AssertEquals(typeof(NctsSupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)departureMovement).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.SupportingDocument]);
	}

	public void TestLookups_Phase4()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsDepartureMovementHeaderPhase4Lookups>(departureMovement.Lookups);
	}

	public void TestLookups_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsDepartureMovementHeaderPhase5Lookups>(departureMovement.Lookups);
	}

	public void TestValidation_Phase4()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsDepartureMovementHeaderPhase4Validation>(departureMovement.Validation);
	}

	public void TestValidation_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsDepartureMovementHeaderPhase5Validation>(departureMovement.Validation);
	}

	public void TestRepresentative_ReadOnly()
	{
		departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Annexes;
		AssertEquals("BM_Phase is not TNN, representative is not ReadOnly", false, departureMovement.Representative.ReadOnly);

		departureMovement.Delete();
		departureMovement = nctsHeader.MovementHeader;
		departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
		AssertEquals("BM_Phase is TNN, representative isReadOnly", true, departureMovement.Representative.ReadOnly);

		departureMovement.Delete();
		departureMovement = nctsHeader.MovementHeader;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
		AssertEquals("BM_Phase is TNN and not NCTS5, representative not isReadOnly", false, departureMovement.Representative.ReadOnly);

		departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;
		AssertEquals("BM_CustomStatus is PRE", true, departureMovement.Representative.ReadOnly);

		departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
		AssertEquals("BM_CustomStatus is not PRE", false, departureMovement.Representative.ReadOnly);
	}

	public void TestIsPhaseStatusTNNAndPhase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Annexes;
		AssertEquals("BM_Phase is not TNN And NCTS5", false, departureMovement.IsPhaseStatusTNNAndPhase5);

		departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
		AssertEquals("BM_Phase is TNN And NCTS5", true, departureMovement.IsPhaseStatusTNNAndPhase5);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertEquals("BM_Phase is TNN And NCTS4", false, departureMovement.IsPhaseStatusTNNAndPhase5);
	}

	public void TestBM_AdditionalDeclarationType_DefaultValue()
	{
		AssertEquals("BM_AdditionalDeclarationType should be 'A' by default", NctsTypeOfAdditionalDeclarationList.Codes.A, departureMovement.BM_AdditionalDeclarationType);
	}

	public void TestBM_InBondEntryType()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = "AH3";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_InBondEntryType = "AH";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestTirCarnetNumber()
	{
		CombineAssertions(() =>
		{
			departureMovement.TirCarnetNumber = "AH3";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.TirCarnetNumber = "AH";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_ReducedDatasetIndicator()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_ReducedDatasetIndicator = true;

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_ReducedDatasetIndicator = false;
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_RN_NKCountryOfDispatch()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_RN_NKCountryOfDispatch = "AH";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_RN_NKCountryOfDispatch = "A";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_RL_NKDestinationPort()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_RL_NKDestinationPort = "AH";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_RL_NKDestinationPort = "A";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_GrossWeight()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_GrossWeight = 96;

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_GrossWeight = 23;
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_PlaceOfLoading()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_PlaceOfLoading = "AH3";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_PlaceOfLoading = "AH";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_SpecificCircumstance()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Max Length", 3, departureMovement.BM_SpecificCircumstanceInfo.MaxLength);
			departureMovement.BM_SpecificCircumstance = "AH3";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_SpecificCircumstance = "AH";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_UniqueConsignmentReference()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_UniqueConsignmentReference = "AH3";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_UniqueConsignmentReference = "AH";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestBM_MethodOfPayment()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_MethodOfPayment = "A";

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.BM_MethodOfPayment = "H";
			AssertEquals("When CustomsStatus is PRE and field changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestOnChangedCarrierJobDocAddressRequirement()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			departureMovement.Carrier.E2_OA_Address = orgAddress.PK;

			AssertEquals("Prereq: ReleaseStatus empty + Dep", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var orgAddress2 = Factory.New<OrgAddress>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress2.OA_OH = orgHeader2.PK;
			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			departureMovement.Carrier.E2_OA_Address = orgAddress2.PK;
			AssertEquals("When CustomsStatus is PRE and Carrier changes", "1", nctsHeader.BH_ReleaseStatus);
		});
	}

	public void TestDoNotAddTirSupportingDoc()
	{
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		departureMovement.TirCarnetNumber = "GB12345678";
		AssertEquals(false, departureMovement.GoodsItems.Any());
	}

	public void TestBM_PlaceOfUnloadingMaxLength()
	{
		AssertEquals("BM_PlaceOfUnloading MaxLength", 35, departureMovement.BM_PlaceOfUnloadingInfo.MaxLength);
	}

	public void TestBH_CustomsProfileDefaulted()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert2";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var staff2 = Factory.New<GlbStaff>();
		staff2.GS_Code = "AA";
		staff2.GS_LoginName = "aatest";
		var wrapper2 = GlbStaffWrapper.Get(staff2);
		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert3";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper2.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert4";
		cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

		var staff3 = Factory.New<GlbStaff>();
		staff3.GS_Code = "AZ";
		staff3.GS_LoginName = "aztest";

		Factory.Save();

		CombineAssertions(() =>
		{
			departureMovement.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile not defaulted when broker has multiple certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			departureMovement.BM_GS_NKCusAgent = staff2.GS_Code;
			AssertEquals("BH_CustomsProfile defaulted when broker has only one certificate", "TESTCERT3", nctsHeader.BH_CustomsProfile);

			departureMovement.BM_GS_NKCusAgent = staff3.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has no certificates", ZString.Empty, nctsHeader.BH_CustomsProfile);

			departureMovement.BM_GS_NKCusAgent = staff.GS_Code;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			AssertEquals("BH_CustomsProfile has value when broker not empty", "TestCert2", nctsHeader.BH_CustomsProfile);

			departureMovement.BM_GS_NKCusAgent = ZString.Empty;
			AssertEquals("BH_CustomsProfile has been cleared when broker is empty", ZString.Empty, nctsHeader.BH_CustomsProfile);

			nctsHeader.BH_CustomsProfile = "TESTCERT1";
			departureMovement.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile left as is when broker changed and value is in certificates list for broker", "TESTCERT1", nctsHeader.BH_CustomsProfile);

			departureMovement.BM_GS_NKCusAgent = ZString.Empty;
			nctsHeader.BH_CustomsProfile = "TestCert2";
			departureMovement.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("BH_CustomsProfile cleared when broker changed and has multiple certificates but value is not in list", ZString.Empty, nctsHeader.BH_CustomsProfile);
		});
	}

	public void TestBM_PaperlessInbondNum()
	{
		CombineAssertions(() =>
		{
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_PaperlessInbondNum = ZString.Empty;
			departureMovement.Header.BH_JobReference = "AH3";
			AssertEquals("When empty but not saved yet", ZString.Empty, departureMovement.BM_PaperlessInbondNum);

			departureMovement.OnSaving();
			AssertEquals("When empty Header.BH_JobReference is populated onSaving", "AH3", departureMovement.BM_PaperlessInbondNum);

			departureMovement.BM_PaperlessInbondNum = "Test1";
			departureMovement.OnSaving();
			AssertEquals("When changed returns its value and it's not overrided when saving", "Test1", departureMovement.BM_PaperlessInbondNum);

			departureMovement.BM_PaperlessInbondNum = ZString.Empty;
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departureMovement.OnSaving();
			AssertEquals("Not populated onSaving because is not Phase5", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
		});
	}

	public void TestBM_PaperlessInbondNum_ReadOnly()
	{
		CombineAssertions("ManualCustomerReference not Enabled ", () =>
		{
			using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				departureMovement.Header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
				departureMovement.BM_CustomsStatus = NctsDepartureMovementHeaderPhaseStatusList.Codes.DeclarationRejected;
				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("Enabled for Phase4 - registry = false (default)", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);

				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Enabled for Phase5 - registry = false Message Status != SNT, Departure Status != Empty", true, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = ZString.Empty;
				AssertEquals("Enabled for Phase5 - registry = false (default) Message Status != SNT, Departure Status = Empty", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);

				departureMovement.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Disabled for Phase5 - registry = false (default) Message Status = SNT, Departure Status = Empty", true, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);
			}
		});
		CombineAssertions("ManualCustomerReference Enabled", () =>
		{
			using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				departureMovement.Header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
				departureMovement.BM_CustomsStatus = NctsDepartureMovementHeaderPhaseStatusList.Codes.DeclarationRejected;
				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("Enabled for Phase4 - registry = true (default)", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);

				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Enabled for Phase5 - registry = true (default) Message Status != SNT, Departure Status != Empty", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);

				departureMovement.BM_CustomsStatus = ZString.Empty;
				AssertEquals("Enabled for Phase5 - registry = true (default) Message Status != SNT, Departure Status = Empty", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);

				departureMovement.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Enabled for Phase5 - registry = true (default) Message Status = SNT, Departure Status = Empty", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);
			}
		});
	}

	public void TestValidateRepresentative()
	{
		const string messageWarning = "If Representative is needed, it must be added in Arrival Notification/Arrival Details/ Rep. Trader. Representative cannot be used in public Arrival Goods Locations.";
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
		movementHeader.Representative.OrganisationPK = ZGuid.Empty;

		CombineAssertions(() =>
		{
			AssertNoWarning("When Representative is not filled and Ncts is TNN and is not Phase5, no warning expected", movementHeader.Representative.OrganisationPKInfo, messageWarning);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader.BM_Phase = ZString.Empty;
			movementHeader.Representative.Validation.ValidateOrganisationPK();
			AssertNoWarning("When Representative is not filled and Ncts is not TNN and is Phase5, no warning expected", movementHeader.Representative.OrganisationPKInfo, messageWarning);

			movementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			movementHeader.Representative.Validation.ValidateOrganisationPK();
			AssertHasWarning("When Representative is not filled and Ncts is TNN and is Phase5, warning expected", movementHeader.Representative.OrganisationPKInfo, messageWarning);

			var representativeOrgHeader = Factory.New<OrgHeader>();
			movementHeader.Representative.OrganisationPK = representativeOrgHeader.PK;
			AssertNoWarning("When Representative is filled and Ncts is TNN and is Phase5, no warning expected", movementHeader.Representative.OrganisationPKInfo, messageWarning);
		});
	}

	public void TestValidateRepresentativeContact()
	{
		const string messageError = "Contact data are required if Representative is used.";
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var movementHeader = nctsHeader.MovementHeader;

		var representativeOrgHeader = Factory.New<OrgHeader>();
		movementHeader.Representative.OrganisationPK = representativeOrgHeader.PK;
		movementHeader.Representative.Validation.ValidateE2_Contact();
		AssertHasMessageError("Representative is filled, contact data is mandatory", movementHeader.Representative.E2_ContactInfo, messageError);

		var contact = representativeOrgHeader.Contacts.AddNew();
		contact.OC_IsActive = true;
		contact.OC_ContactName = "contact1";
		contact.OC_Title = "AAA";
		contact.OC_Phone = "123456";
		movementHeader.Representative.ContactPK = contact.PK;
		movementHeader.Representative.Validation.ValidateE2_Contact();
		AssertNoMessageError("Representative is filled, contact data is filled", movementHeader.Representative.E2_ContactInfo, messageError);
	}

	public void TestShouldNotUpdateBM_GrossWeightIfLessThanTotal()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill1 = departureMovement.Header.Bills.AddNew();
		bill1.B0_Weight = 100m;
		departureMovement.BM_GrossWeight = 50m;
		departureMovement.BM_GrossWeightUQ = "KG";
		Factory.Save();
		AssertEquals("GrossWeight is not update if it's less than Total", 50m, departureMovement.BM_GrossWeight);
	}

	public void TestCustomsStatusIsPRE()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.CancellationRequestedByCustoms;
			AssertEquals("BM_CustomStatus is not PRE", false, departureMovement.CustomsStatusIsPRE);

			departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;
			AssertEquals("BM_CustomStatus is PRE", true, departureMovement.CustomsStatusIsPRE);

			departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;
			AssertEquals("BM_CustomStatus is not PRE", false, departureMovement.CustomsStatusIsPRE);
		});
	}

	public void TestBM_TypeOfSecurity_ReadOnly()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
			AssertEquals("BM_CustomStatus is not PRE", false, departureMovement.BM_TypeOfSecurityInfo.ReadOnly);

			departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.PreLodged;
			AssertEquals("BM_CustomStatus is PRE", true, departureMovement.BM_TypeOfSecurityInfo.ReadOnly);

			departureMovement.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			AssertEquals("BM_CustomStatus is not PRE", false, departureMovement.BM_TypeOfSecurityInfo.ReadOnly);
		});
	}

	public void TestIsPhaseStatusTNN()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When BM_Phase is empty", false, departureMovement.IsPhaseStatusTNN);

			departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			AssertEquals("When BM_Phase is TNN", true, departureMovement.IsPhaseStatusTNN);

			departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals("When BM_Phase is not TNN", false, departureMovement.IsPhaseStatusTNN);
		});
	}

	public void TestIsCustomsStatusPRE()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When BM_CustomsStatus is empty", false, departureMovement.IsCustomsStatusPRE);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			AssertEquals("When BM_CustomsStatus is PRE", true, departureMovement.IsCustomsStatusPRE);

			departureMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
			AssertEquals("When BM_CustomsStatus is not PRE", false, departureMovement.IsCustomsStatusPRE);
		});
	}

	public void TestArrivalHeaderForTNN()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			var matchingTNN = departureMovement.ArrivalHeaderForTNN;
			AssertNull("There is no associated arrival", matchingTNN);

			var arrival = departureMovement.Factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);
			arrival.ArrivalMovementHeader.BM_BM_DepartureMovement = departureMovement.PK;

			matchingTNN = departureMovement.ArrivalHeaderForTNN;
			AssertNull("There is an associated arrival but the departure is not TNN", matchingTNN);

			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
			matchingTNN = departureMovement.ArrivalHeaderForTNN;
			AssertNotNull("There is an associated arrival and the departure is CEN_TNNArrival", matchingTNN);
		});
	}

	public void TestCustomsOffices()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsESOfficeCodeCollection>(header.MovementHeader.CustomsOffices);
	}

	public void TestGuarantees() => AssertType<NctsGuaranteeCollection<NctsGuarantee>>(GetPhase5DepartureMovementHeader().Guarantees);

	public void TestOnChangedGuarantees()
	{
		CombineAssertions(() =>
		{
			var movementHeader = GetPhase5DepartureMovementHeader();
			var header = movementHeader.Header;
			var guarantees = movementHeader.Guarantees;
			var guarantee = guarantees.AddNew();

			movementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
			guarantee.PW_BondNumber = "AH3";
			AssertEquals("When CustomsStatus is PRE and Guarantee changes", ZString.Empty, movementHeader.Header.BH_ReleaseStatus);

			movementHeader.BM_CustomsStatus = ZString.Empty;
			AssertEquals("Prereq: Departure", NctsMovementType.Codes.Departure, header.BH_HeaderType);
			AssertEquals("Prereq: ReleaseStatus empty", ZString.Empty, header.BH_ReleaseStatus);

			_ = guarantees.AddNew();
			AssertEquals($"When CustomsStatus is not PRE and Guarantees add new", ZString.Empty, header.BH_ReleaseStatus);

			header.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;

			var item = guarantees.AddNew();
			AssertEquals($"When CustomsStatus is PRE and Guarantees collection add new", "1", header.BH_ReleaseStatus);

			header.BH_ReleaseStatus = ZString.Empty;
			item.Delete();
			AssertEquals($"When CustomsStatus is not PRE and Guarantees add new", "1", header.BH_ReleaseStatus);
		});
	}

	public void TestCusReferenceTypeSupporter()
	{
		ICusReferenceTypeSupporter supporter = departureMovement;
		AssertEquals(typeof(CusSupplyChainActorReference), supporter.GetCusReferenceTypes()[CusReferenceTypeList.Codes.SupplyChainActor]);
	}

	#region ReserveTemporaryStorageGoods

	const string InternalReference = "ES00001";

	public void TestGetGoodsItemDataDeclaredForDepartureToReserveTSGoods()
	{
		Factory.SetBulkTypeHelper();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();
		var goodsItem2 = bill1.GoodsItems.AddNew();
		var bill2 = nctsHeader.Bills.AddNew();
		var goodsItem3 = bill2.GoodsItems.AddNew();
		var goodsItem4 = bill2.GoodsItems.AddNew();
		var movementHeader = nctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			var (declarationDataToReserveTSGoodsList, messageReturned) = movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods();
			AssertEquals("0 documents are returned since there are no documents in the declaration", 0, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned since there are no documents in the declaration", ZString.Empty, messageReturned);

			var previousDoc1 = goodsItem1.PreviousDocuments.AddNew();
			previousDoc1.CSI_ReferenceNumber = "Reference";
			previousDoc1.CSI_Code = "BBB";
			previousDoc1.CSI_ItemNumber = 1;
			goodsItem1.BY_GrossWeight = 200.4455m;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem1.IsVehicles = true;

			var vehicle1 = goodsItem1.Packages.AddNew();
			vehicle1.B5_PackageID = "VIN1";
			vehicle1.B5_Brand = "BRAND1";
			vehicle1.B5_Model = "MODEL1";

			var vehicle2 = goodsItem1.Packages.AddNew();
			vehicle2.B5_PackageID = "VIN2";
			vehicle2.B5_Brand = "BRAND1";
			vehicle2.B5_Model = "MODEL1";

			var previousDoc2 = goodsItem2.PreviousDocuments.AddNew();
			previousDoc2.CSI_ReferenceNumber = "Reference";
			previousDoc2.CSI_Code = "BBB";
			previousDoc2.CSI_ItemNumber = 2;
			goodsItem2.BY_GrossWeight = 2000.4455m;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			goodsItem2.IsVehicles = false;

			var package1 = goodsItem2.Packages.AddNew();
			package1.B5_MarksAndNumbers = "marks";
			package1.B5_UnitType = "CT";
			package1.B5_UnitCount = 9;

			var package2 = goodsItem2.Packages.AddNew();
			package2.B5_MarksAndNumbers = "marks2";
			package2.B5_UnitType = "NE";
			package2.B5_UnitCount = 4;

			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_MarksAndNumbers = "bulk gas marks";
			package3.B5_UnitType = "VG";
			package3.B5_UnitCount = 2;

			var previousDoc3 = goodsItem3.PreviousDocuments.AddNew();
			previousDoc3.CSI_ReferenceNumber = "Reference";
			previousDoc3.CSI_Code = "BBB";
			previousDoc3.CSI_ItemNumber = 2;
			previousDoc3.CSI_Quantity = 100.4455m;
			previousDoc3.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			goodsItem3.BY_GrossWeight = 20m;
			goodsItem3.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem3.IsVehicles = false;

			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_MarksAndNumbers = "marks4";
			package4.B5_UnitType = "CT";
			package4.B5_UnitCount = 8;

			var package5 = goodsItem3.Packages.AddNew();
			package5.B5_MarksAndNumbers = "marks";
			package5.B5_UnitType = "CT";
			package5.B5_UnitCount = 6;

			var package6 = goodsItem2.Packages.AddNew();
			package6.B5_MarksAndNumbers = "marks6";
			package6.B5_UnitType = "VG";
			package6.B5_UnitCount = 6;

			var previousDoc4 = goodsItem4.PreviousDocuments.AddNew();
			previousDoc4.CSI_ReferenceNumber = "Reference2";
			previousDoc4.CSI_Code = "AAA";
			previousDoc4.CSI_ItemNumber = 2;
			goodsItem4.BY_GrossWeight = 0.9886m;
			goodsItem4.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			var package7 = goodsItem4.Packages.AddNew();
			package7.B5_MarksAndNumbers = "marks5";
			package7.B5_UnitType = "BX";
			package7.B5_UnitCount = 7;

			var expectedPackagesForEntryLine1 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
			{
				("FR", 1, "VIN1", false),
				("FR", 1, "VIN2", false)
			};
			var expectedPackagesForEntryLine2 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
			{
					("CT", 15, "marks", false),
					("NE", 4, "marks2", false),
					("VG", 2, "bulk gas marks", true),
					("VG", 6, "marks6", true),
					("CT", 8, "marks4", false)
			};
			var expectedPackagesForEntryLine3 = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> { ("BX", 7, "marks5", false) };

			(declarationDataToReserveTSGoodsList, messageReturned) = movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods();
			AssertEquals("declarationDataToReserveTSGoodsList has 3 elements", 3, declarationDataToReserveTSGoodsList.Count());
			AssertEquals("empty string is returned even when there are documents in the declaration", ZString.Empty, messageReturned);

			var declarationDataToReserveTSGoods1 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 200.4455m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods1", declarationDataToReserveTSGoods1, "Reference", 1, 200.4455m, 200.4455m, expectedPackagesForEntryLine1);

			var declarationDataToReserveTSGoods2 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 102.445946m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods2", declarationDataToReserveTSGoods2, "Reference", 2, 102.445946m, 0, expectedPackagesForEntryLine2);

			var declarationDataToReserveTSGoods3 = declarationDataToReserveTSGoodsList.First(d => d.TotalGrossWeight == 0.9886m);
			AssertDeclarationDataToReserveTSGoods("declarationDataToReserveTSGoods3", declarationDataToReserveTSGoods3, "Reference2", 2, 0.9886m, 0, expectedPackagesForEntryLine3);
		});

		void AssertDeclarationDataToReserveTSGoods(ZString message, DeclarationDataToReserveTSGoods declarationData, ZString expectedDocRef, ZInt expectedLineNo, ZDecimal expectedGrossWeight, ZDecimal expectedGrossWeightForVINs, List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> expectedPackages)
		{
			AssertEquals(message + " docRef", expectedDocRef, declarationData.Document.CSI_ReferenceNumber);
			AssertEquals(message + " docLineNo", expectedLineNo, declarationData.Document.CSI_ItemNumber);
			AssertEquals(message + " totalGrossWeight", expectedGrossWeight, declarationData.TotalGrossWeight);
			AssertEquals(message + " totalGrossWeightForVINs", expectedGrossWeightForVINs, declarationData.TotalGrossWeightForVINs);
			AssertContainsExactElementsInAnyOrder(message + " packages", expectedPackages, declarationData.Packages);
		}
	}

	public void TestTemporaryStorageTransactionInternalReferenceNumberAndType()
	{
		var headerDep = Factory.New<NctsHeader>();
		headerDep.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		headerDep.SetMovementType(NctsMovementType.Codes.Departure);
		var depMovementheader = headerDep.MovementHeader;
		depMovementheader.BM_PaperlessInbondNum = InternalReference;

		CombineAssertions(() =>
		{
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to BM_PaperlessInbondNum when departure", InternalReference, depMovementheader.TemporaryStorageTransactionInternalReferenceNumber);

			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set to TRA when departure", TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration, depMovementheader.TemporaryStorageTransactionInternalReferenceType);
		});
	}

	#region GetDataToReserveTemporaryStorageGoods

	public void TestGetDataToReserveTemporaryStorageGoods_WithoutN337doc()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(false, ZString.Empty, ZString.Empty);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithoutRegHeader()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: "reference");

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithoutPremises_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithoutPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005", regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 24ES00999980001282 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithoutPremises_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(locationInPremises: "9999000005");

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithoutRegLineItem_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(prevDocLineNo: 2, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(prevDocLineNo: 2, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 24ES00999980001282. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithoutRegLineItem_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(prevDocLineNo: 2);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithVINError_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 24ES00999980001282, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithVINError_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithPackageError_NotBulk_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 24ES00999980001282, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithPackageError_Bulk_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, _, _, _, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again.", resultMessage);
			AssertEquals("dataToReserveGoodsList is empty", 0, dataToReserveGoodsList.Count());
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithGrossWeightError_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(1, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is not empty", "ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageForVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReference, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(1, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is not empty", "ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 24ES00999980001282, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageForVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithGrossWeightError_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty", "ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\nDo you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithGrossWeightForVINsError_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, createInTransaction1OBL: true);

		var expectedDataToReserveList = new List<(ZInt, CusTempStorageRegLine)>()
		{
			(1, regLine1),
			(9, regLine3),
			(0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessageVINs is not empty", "ES00001:\nGross weight 11 used for the declaration is different to the gross weight entered in the Temporary Storage 5 for TSD Number 99994000128, Item 1.\nThis can cause mismatches in the stock at ES Customs records.\n\nWould you like to cancel this action and check the gross weight declared for the vehicles?", resultMessageForVINs);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(prevDocReference: FormattedPrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(regHeaderReference: PrevDocReference);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, _, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods();

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is empty", ZString.Empty, resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithMultipleDocs_DocRefShorterThan18()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, prevDocReference: FormattedPrevDocReference, addExtraDoc: true, secondDocRef: FormattedSecondDocRef);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithMultipleDocs_DocRefLongerThan18_WithoutFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, regHeaderReference: PrevDocReference, addExtraDoc: true, secondRegHeaderReference: SecondDocRef);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) =	EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 24ES00999980001282, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 24ES00999898765432, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	public void TestGetDataToReserveTemporaryStorageGoods_WithN337docWithRegHeaderWithPremises_WithMultipleDocs_DocRefLongerThan18_WithFormatting()
	{
		var (movementHeader, regLineTransaction, regLine1, regLine2, regLine3, regLine4, _) = SetUpDataForGetDataToReserveTemporaryStorageGoods(transactionGrossWeight: 5m, addExtraDoc: true);

		var expectedDataToReserveList = new List<(ZDecimal, ZInt, CusTempStorageRegLine)>()
		{
			(22m, 1, regLine1),
			(22m, 9, regLine3),
			(22m, 0, regLine2),
			(20m, 0, regLine4),
		};

		CombineAssertions(() =>
		{
			var (resultMessage, resultMessageForVINs, dataToReserveGoodsList) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType, movementHeader.BM_PaperlessInbondNum, PrevDocCode, LocationInEntry, movementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

			AssertEquals("resultMessage is not empty",
				"ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 5\n\n" +
				"There might not be enough Gross Weight 20 for TSD Number 99984876543, Item 2.\nRemaining Gross Weight in the Temporary Storage: 6\n\n" +
				"Do you want to cancel this declaration to check?", resultMessage);
			AssertContainsExactElementsInAnyOrder("dataToReserveGoodsList is not empty", expectedDataToReserveList, dataToReserveGoodsList.Select(d => (d.TotalGrossWeight, d.TotalPackQty, (CusTempStorageRegLine)d.RegLine)));
			AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
		});
	}

	#endregion

	(NctsDepartureMovementHeader nctsMovementHeader, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine1, CusTempStorageRegLine regLine2, CusTempStorageRegLine regLine3, CusTempStorageRegLine regLine4, CusTempStorageRegLineItem regLineItem) SetUpDataForGetDataToReserveTemporaryStorageGoods
		(bool shouldSetPreviousDocuments = true, string prevDocCode = PrevDocCode, string prevDocReference = PrevDocReference, int prevDocLineNo = 1,
		string locationInPremises = LocationInEntry, string regHeaderReference = FormattedPrevDocReference, string packageVin = "VIN1", int packageQtyNotBulk = 9, decimal transactionGrossWeight = 40m,
		bool addExtraDoc = false, decimal transactionGrossWeightForExtraDoc = 6m, string bulkPackageTypeForRegLine = "VG", string secondDocRef = SecondDocRef, string secondRegHeaderReference = FormattedSecondDocRef, bool createInTransaction1OBL = false)
	{
		Factory.SetBulkTypeHelper();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var nctsMovementHeader = nctsHeader.MovementHeader;
		nctsMovementHeader.BM_PaperlessInbondNum = EntryReference;

		var bill1 = nctsHeader.Bills.AddNew();

		var goodsItem1 = bill1.GoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 11m;
		goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		goodsItem1.IsVehicles = true;

		var vehicle1 = goodsItem1.Packages.AddNew();
		vehicle1.B5_PackageID = "VIN1";

		var goodsItem2 = bill1.GoodsItems.AddNew();
		goodsItem2.BY_GrossWeight = 11m;
		goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		goodsItem2.IsVehicles = false;

		var package1 = goodsItem2.Packages.AddNew();
		package1.B5_MarksAndNumbers = "marks";
		package1.B5_UnitType = "BX";
		package1.B5_UnitCount = 9;

		var package2 = goodsItem2.Packages.AddNew();
		package2.B5_MarksAndNumbers = "bulk gas marks";
		package2.B5_UnitType = "VG";
		package2.B5_UnitCount = 0;

		if (shouldSetPreviousDocuments)
		{
			var previousDoc1 = goodsItem1.PreviousDocuments.AddNew();
			previousDoc1.CSI_Code = prevDocCode;
			previousDoc1.CSI_ReferenceNumber = prevDocReference;
			previousDoc1.CSI_ItemNumber = prevDocLineNo;

			var previousDoc2 = goodsItem2.PreviousDocuments.AddNew();
			previousDoc2.CSI_Code = prevDocCode;
			previousDoc2.CSI_ReferenceNumber = prevDocReference;
			previousDoc2.CSI_ItemNumber = prevDocLineNo;
		}

		if (addExtraDoc)
		{
			var goodsItem3 = bill1.GoodsItems.AddNew();
			goodsItem3.BY_GrossWeight = 20m;
			goodsItem3.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem3.IsVehicles = false;

			var package3 = goodsItem3.Packages.AddNew();
			package3.B5_MarksAndNumbers = "bulk gas marks2";
			package3.B5_UnitType = "VG";
			package3.B5_UnitCount = 0;

			if (shouldSetPreviousDocuments)
			{
				var previousDoc3 = goodsItem3.PreviousDocuments.AddNew();
				previousDoc3.CSI_Code = prevDocCode;
				previousDoc3.CSI_ReferenceNumber = secondDocRef;
				previousDoc3.CSI_ItemNumber = 2;
			}
		}

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = premises.PK;
		var regLine1 = Factory.New<CusTempStorageRegLine>();
		regLine1.SRL_LineNumber = 1;
		regLine1.SRL_CustomsStatus = "OPN";
		regLine1.SRL_PackageType = "FR";
		regLine1.SRL_PackageMarks = packageVin;
		regLine1.SRL_SRH = regHeader.PK;
		var regLineTransactionPND = (CusTempStorageRegLineTransaction)regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND.SRT_TransactionType = TS.CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND.SRT_InternalReferenceNumber = EntryReference;
		regLineTransactionPND.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = TS.CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		if (createInTransaction1OBL)
		{
			var regLineTransaction11 = (CusTempStorageRegLineTransaction)regLine1.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction11.SRT_TransactionType = TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regLineTransaction11.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction11.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction11.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
			regLineTransaction11.SRT_GrossWeight = transactionGrossWeight;
		}

		var regLine2 = Factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 3;
		regLine2.SRL_CustomsStatus = "OPN";
		regLine2.SRL_PackageType = bulkPackageTypeForRegLine;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = TS.CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction2.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction2.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

		var regLine3 = Factory.New<CusTempStorageRegLine>();
		regLine3.SRL_LineNumber = 4;
		regLine3.SRL_CustomsStatus = "OPN";
		regLine3.SRL_PackageType = "BX";
		regLine3.SRL_SRH = regHeader.PK;
		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = TS.CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
		regLineTransaction3.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

		var regLineItemPivot2 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

		var regLineItemPivot3 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

		var regLine4 = (CusTempStorageRegLine)null;
		if (addExtraDoc)
		{
			var regHeader2 = Factory.New<CusTempStorageRegHeader>();
			regHeader2.SRH_AppCode = "BBB";
			regHeader2.SRH_Reference = secondRegHeaderReference;
			regHeader2.SRH_SRP_Premises = premises.PK;

			regLine4 = Factory.New<CusTempStorageRegLine>();
			regLine4.SRL_LineNumber = 5;
			regLine4.SRL_CustomsStatus = "OPN";
			regLine4.SRL_PackageType = "VG";
			regLine4.SRL_SRH = regHeader2.PK;
			var regLineTransaction4 = regLine4.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction4.SRT_TransactionType = TS.CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction4.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			regLineTransaction4.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction4.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
			regLineTransaction4.SRT_GrossWeight = transactionGrossWeightForExtraDoc;

			var regLineItem2 = Factory.New<CusTempStorageRegLineItem>();
			regLineItem2.SRI_GoodsItemNumber = 2;

			var regLineItemPivot4 = Factory.New<CusTempStorageRegLineItemPivot>();
			regLineItemPivot4.SRV_SRI_Item = regLineItem2.PK;
			regLineItemPivot4.SRV_SRL_Line = regLine4.PK;
		}

		Factory.Save();

		return (nctsMovementHeader, regLineTransactionPND, regLine1, regLine2, regLine3, regLine4, regLineItem);
	}

	#endregion

	#region ConfirmTemporaryStorageGoodsConsumption

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = EntryStatusCodes.PreDeclarationAccepted;

				AssertEquals("BM_CustomsStatus is changed to PDA", "PDA", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_EmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInEntry: ZString.Empty);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Cancelled;

				AssertEquals("BM_CustomsStatus is changed to CAN", "CAN", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_LocationNotManagedInPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Cancelled;

				AssertEquals("BM_CustomsStatus is changed to CAN", "CAN", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_CustomsStatusCO1()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

				AssertEquals("BM_CustomsStatus is changed to CO1", "CO1", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_CustomsStatusDGP()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance;

				AssertEquals("BM_CustomsStatus is changed to DGP", "DGP", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusNRL_OnlyCancel()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

				AssertEquals("BM_CustomsStatus is changed to NRL", "NRL", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusGIV_OnlyCancel()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

				AssertEquals("BM_CustomsStatus is changed to GIV", "GIV", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusINV_OnlyCancel()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Invalidated;

				AssertEquals("BM_CustomsStatus is changed to INV", "INV", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusCAN_OnlyCancel()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Cancelled;

				AssertEquals("BM_CustomsStatus is changed to CAN", "CAN", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusREL_Confirm()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			SetUpRefData();
			Factory.Save();

			var oldComment = "Extra Old Comment";

			var orgHeader = SetUpOrgHeader();
			var nctsMovementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = (CusTempStorageRegLine)regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "NE";
			var regLine2 = (CusTempStorageRegLine)regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			var regLineTransaction1 = SetUpTransaction(regLine1, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, 10, 6);
			var regLineTransaction2 = SetUpTransaction(regLine1, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2);

			var regLineTransaction3 = SetUpTransaction(regLine2, TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5);
			var regLineTransaction4 = SetUpTransaction(regLine2, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);
			var regLineTransaction5 = SetUpTransaction(regLine2, TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 6);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = (CusTempStorageRegLine)regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_PackageType = "AA";
			var regLine4 = (CusTempStorageRegLine)regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";

			var regLineTransaction6 = SetUpTransaction(regLine3, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, 5, 6);
			regLineTransaction6.SRT_Comments = oldComment;
			var regLineTransaction7 = SetUpTransaction(regLine4, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("BM_CustomsStatus is changed to REL", "REL", nctsMovementHeader.BM_CustomsStatus);

				AssertTransactionAfterConfirmAction("regLineTransaction1", regLineTransaction1);
				AssertTransactionAfterConfirmAction("regLineTransaction2", regLineTransaction2);
				AssertEquals("regLineTransaction3 was not changed since it wasn't PND", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction4", regLineTransaction4);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction6", regLineTransaction6, comment: ExpectedComment + " - " + oldComment);
				AssertTransactionAfterConfirmAction("regLineTransaction7", regLineTransaction7);

				AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine3 CustomsStatus is changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
				AssertEquals("regLine4 CustomsStatus is changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoActionWithCustomsStatusREL_PhaseTNN()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("BM_CustomsStatus is changed to REL", "REL", nctsMovementHeader.BM_CustomsStatus);
				AssertTransactionAfterConfirmAction("regLineTransaction", regLineTransaction, transactionStatus: TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, referenceType: ZString.Empty, mrnCode: ZString.Empty, comment: ZString.Empty, expectedIssueDate: ZDateTimeOffset.Empty, expectedReleaseDate: ZDateTimeOffset.Empty);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_WriteOff()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var nctsMovementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, -3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 2, grossWeight: -2);
			var regLineTransaction2 = SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 1, grossWeight: -1);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("2 Write off transactions in Guarantee are created", 2, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);

				var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<CusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", FormattedPrevDocReference, 2.0m);
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", FormattedPrevDocReference, 1.0m);
			});
		}

		void AssertWriteOffTransaction(CusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZDecimal expectedTranValue)
		{
			AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
			AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
			AssertEquals(transactionName + "'s CPL_TransactionDate is Entry Release Date", releaseDate, transaction.CPL_TransactionDate);
			AssertEquals(transactionName + "'s CPL_Comment is Write-off + reference / TRA + MRN", string.Format("Write-off TS {0} / TRA {1}", expectedReference, MRNCode), transaction.CPL_Comment);
			AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
			AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_NoPendingAmount()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var nctsMovementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 0.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_PendingAmountPositive()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var nctsMovementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

				var expectedError = "|RES=Reference 99994000128 has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
				AssertEquals("New event in logs", expectedError, nctsMovementHeader.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			});
		}
	}

	void AssertTransactionAfterConfirmAction(ZString transactionName, CusTempStorageRegLineTransaction transaction, string transactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, string referenceType = "MRN", string mrnCode = MRNCode, string comment = ExpectedComment, ZDateTimeOffset? expectedIssueDate = null, ZDateTimeOffset? expectedReleaseDate = null)
	{
		AssertEquals(transactionName + "'s SRT_TransactionStatus is correct", transactionStatus, transaction.SRT_TransactionStatus);
		AssertEquals(transactionName + "'s SRT_ReferenceType is correct", referenceType, transaction.SRT_ReferenceType);
		AssertEquals(transactionName + "'s SRT_Reference is correct", mrnCode, transaction.SRT_Reference);
		AssertEquals(transactionName + "'s SRT_Comments is correct", comment, transaction.SRT_Comments);
		AssertEquals(transactionName + "'s SRT_TransactionDate is correct", expectedIssueDate != null ? expectedIssueDate : issueDate.ToOffset(), transaction.SRT_TransactionDate);
		AssertEquals(transactionName + "'s SRT_PhysicalInOutDate is correct", expectedReleaseDate != null ? expectedReleaseDate : releaseDate.ToOffset(), transaction.SRT_PhysicalInOutDate);
	}

	#region ConfirmTemporaryStorageGoodsConsumptionAfterAddingPNDTransactions

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactionsWithCustomsStatusNotInLists_WithPNDTransaction()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

				AssertEquals("BM_CustomsStatus is changed to CO1", "CO1", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactionsWithCustomsStatusNotInLists_WithCONTransaction()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldSetPreviousDocuments: true);
			regLineTransaction.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

				AssertEquals("BM_CustomsStatus is changed to CO1", "CO1", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusNotInLists_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

				AssertEquals("BM_CustomsStatus is changed to CO1", "CO1", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusNotInLists_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: PrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

				AssertEquals("BM_CustomsStatus is changed to CO1", "CO1", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusNotInLists_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: FormattedPrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;

				AssertEquals("BM_CustomsStatus is changed to CO1", "CO1", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusToConfirm_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("BM_CustomsStatus is changed to REL", "REL", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusToConfirm_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: PrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("BM_CustomsStatus is changed to REL", "REL", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusToConfirm_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true, regHeaderReference: FormattedPrevDocReference, docReference: PrevDocReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

				AssertEquals("BM_CustomsStatus is changed to REL", "REL", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactionsWithCustomsStatusInList()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsMovementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldSetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				nctsMovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;

				AssertEquals("BM_CustomsStatus is changed to PRE", "PRE", nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not created (PRE)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				nctsMovementHeader.BM_CustomsStatus = ZString.Empty;
				AssertEquals("BM_CustomsStatus is changed to empty", ZString.Empty, nctsMovementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not created (empty)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	#endregion

	NctsDepartureMovementHeader SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(OrgAddress orgAddress, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry
		, string phaseStatus = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, bool shouldSetPreviousDocuments = false, string docReference = FormattedPrevDocReference)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_JobReference = JobReference;

		var mrnEntryNum = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		mrnEntryNum.CE_EntryNum = MRNCode;
		mrnEntryNum.CE_IssueDate = issueDate;

		var clrEntryNum = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
		clrEntryNum.CE_EntryNum = "fvgbhjnlkml";
		clrEntryNum.CE_IssueDate = releaseDate;

		var nctsMovementHeader = nctsHeader.MovementHeader;
		nctsMovementHeader.BM_PaperlessInbondNum = EntryReference;
		nctsMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = locationInEntry;
		nctsMovementHeader.BM_Phase = phaseStatus;

		var bill = nctsHeader.Bills.AddNew();

		var goodsItem = bill.GoodsItems.AddNew();
		goodsItem.BY_GrossWeight = 100.4455m;
		goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

		var package = goodsItem.Packages.AddNew();
		package.B5_MarksAndNumbers = "marks";
		package.B5_UnitType = "CT";
		package.B5_UnitCount = 9;

		if (shouldSetPreviousDocuments)
		{
			var previousDoc = goodsItem.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = PrevDocCode;
			previousDoc.CSI_ReferenceNumber = docReference;
			previousDoc.CSI_LineNo = 1;
		}

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		return nctsMovementHeader;
	}

	(NctsDepartureMovementHeader nctsMovementHeader, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine) SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction
		(string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, string phaseStatus = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, bool createTransaction = true, bool shouldSetPreviousDocuments = false
		, string regHeaderReference = FormattedPrevDocReference, string docReference = FormattedPrevDocReference)
	{
		var orgHeader = SetUpOrgHeader();
		var nctsMovementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, locationInEntry: locationInEntry, locationInPremises: locationInPremises, phaseStatus: phaseStatus, shouldSetPreviousDocuments: shouldSetPreviousDocuments, docReference: docReference);

		var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_CustomsStatus = "OPN";
		regLine.SRL_PackageType = "CT";
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction = (CusTempStorageRegLineTransaction)null;
		if (createTransaction)
		{
			regLineTransaction = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = TS.CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction.SRT_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		}

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot.SRV_SRL_Line = regLine.PK;

		SetUpTransaction(regLine, TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: TS.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);
		Factory.Save();

		return (nctsMovementHeader, regLineTransaction, regLine);
	}

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString transactionStatus, int packageQty = 0, decimal grossWeight = 0, string transactionType = "TRN", decimal bondAmount = 0.0m)
	{
		var regLineTransaction = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = transactionType;
		regLineTransaction.SRT_TransactionStatus = transactionStatus;
		regLineTransaction.SRT_InternalReferenceType = TS.CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		regLineTransaction.SRT_PackageQty = packageQty;
		regLineTransaction.SRT_GrossWeight = grossWeight;

		if (transactionType == "OBL")
		{
			regLineTransaction.SRT_BondAmount = bondAmount;
		}
		else
		{
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
		}

		return regLineTransaction;
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
	}

	CusTempStorageRegHeader SetUpTmpRegHeader(string appCode = "AAA", string reference = FormattedPrevDocReference)
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = appCode;
		regHeader.SRH_Reference = reference;

		return regHeader;
	}

	CusGuaranteeHeader SetUpGauranteeForTempStorage(CusTempStorageRegHeader regHeader, ZDecimal value, OrgHeader orgHeader)
	{
		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
		cusGuarantee.CPH_Balance = 1000.0m;

		var commonGuarantee = Factory.New<EU.Business.Declaration.CommonGuarantee>();
		commonGuarantee.PW_BondNumber = "Test1";
		commonGuarantee.PW_ParentID = regHeader.PK;
		commonGuarantee.PW_ParentTableCode = CusBondDetailSchema.Constants.Prefix;
		commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		var guarantee = regHeader.Guarantee.CusGuarantee;
		var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
		guaranteeLineTransaction.CPL_Reference = FormattedPrevDocReference;
		guaranteeLineTransaction.CPL_TransactionStatus = TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		guaranteeLineTransaction.CPL_TranValue = value;

		guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		return guarantee;
	}

	CusTempStorageRegLine SetUpRegLine(CusTempStorageRegHeader regHeader)
	{
		var regLine = (CusTempStorageRegLine)regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_PackageType = "BX";

		return regLine;
	}

	OrgHeader SetUpOrgHeader()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		return orgHeader;
	}

	#endregion

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		nctsHeader = factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}

	NctsDepartureMovementHeader GetPhase5DepartureMovementHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}

	protected override BusinessObject GetNewBusinessObject() => departureMovement;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;

	const string EntryReference = "ES00001";
	const string LocationInEntry = "9999000002";
	const string PrevDocCode = "N337";
	const string PrevDocReference = "24ES00999980001282";
	const string FormattedPrevDocReference = "99994000128";
	const string SecondDocRef = "24ES00999898765432";
	const string FormattedSecondDocRef = "99984876543";
	const string MRNCode = "22ES00999950008716";
	const string JobReference = "Reference";
	const string CommentPrefix = "TRANSIT JOB:";
	const string ExpectedComment = CommentPrefix + " " + JobReference;
	readonly ZDateTime issueDate = new ZDateTime(2024, 06, 10, 11, 11, 11);
	readonly ZDateTime releaseDate = new ZDateTime(2024, 06, 14, 11, 11, 11);
}
