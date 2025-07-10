using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DEPDATHouseConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IDEPDATHouseConsignment>
	{
		public void TestNullParameter()
		{
			CombineAssertions(() =>
			{
				AssertNull("HeaderProvider is null", DEPDATHouseConsignmentProvider.NewOrNull(null, headerProviderMock.Object));
				AssertNull("NctsBill is null", DEPDATHouseConsignmentProvider.NewOrNull(nctsBill, null));
			});
		}

		public void TestSequenceNumber()
		{
			nctsBill.SequenceNumber = 9;
			AssertEquals(9, Provider.SequenceNumber);
		}

		public void TestCountryOfDispatch_CountryOfDispatchNullInHeaderProvider_EffectiveCountryOfDispatchAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfExport = "FR";
				movementHeader.BM_RN_NKCountryOfDispatch = "DE";
				headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
				var result = Provider.CountryOfDispatch;
				AssertEquals("FR", result);
				AssertSame("Cached", result, Provider.CountryOfDispatch);
			});
		}

		public void TestCountryOfDispatch_CountryOfDispatchNullInHeaderProvider_EffectiveCountryOfDispatchAllSame_IsPhase5TP()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfExport = "FR";
				headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
				AssertNull(Provider.CountryOfDispatch);
			});
		}

		public void TestCountryOfDispatch_HeaderProviderProvidesCountryOfDispatch_EffectiveCountryOfDispatchAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfExport = "FR";
				headerProviderMock.Setup(x => x.CountryOfDispatch).Returns("IT");
				AssertNull(Provider.CountryOfDispatch);
			});
		}

		public void TestCountryOfDispatch_FallBack_CountryOfDispatchNullInHeaderProvider_EffectiveCountryOfDispatchAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
				movementHeader.BM_RN_NKCountryOfDispatch = "DE";
				headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
				AssertEquals("DE", Provider.CountryOfDispatch);
			});
		}

		public void TestCountryOfDispatch_CountryOfDispatchNullInHeaderProvider_EffectiveCountryOfDispatchDifferent_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				cargoDesc.BY_RN_NKCountryOfDispatch = "BE";
				var cargoDesc2 = nctsBill.GoodsItems.AddNew();
				cargoDesc2.BY_RN_NKCountryOfDispatch = "PL";
				headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
				AssertNull(Provider.CountryOfDispatch);
			});
		}

		public void TestCountryOfDestination_CountryOfDestinationNullInHeaderProvider_EffectiveCountryOfDestinationAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfDestination = "FR";
				movementHeader.BM_RL_NKDestinationPort = "DE";
				headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
				var result = Provider.CountryOfDestination;
				AssertEquals("result", "FR", result);
				AssertSame("Cached", result, Provider.CountryOfDestination);
			});
		}

		public void TestCountryOfDestination_CountryOfDestinationNullInHeaderProvider_EffectiveCountryOfDestinationAllSame_IsPhase5TP()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfDestination = "FR";
				headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
				AssertNull(Provider.CountryOfDestination);
			});
		}

		public void TestCountryOfDestination_HeaderProviderProvidesCountryOfDispatch_EffectiveCountryOfDestinationAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfDestination = "FR";
				headerProviderMock.Setup(x => x.CountryOfDestination).Returns("IT");
				AssertNull(Provider.CountryOfDestination);
			});
		}

		public void TestCountryOfDestination_FallBack_CountryOfDestinationNullInHeaderProvider_EffectiveCountryOfDestinationAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
				movementHeader.BM_RL_NKDestinationPort = "DE";
				headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
				AssertEquals("DE", Provider.CountryOfDestination);
			});
		}

		public void TestCountryOfDestination_CountryOfDestinationNullInHeaderProvider_EffectiveCountryOfDestinationDifferent_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				cargoDesc.BY_RN_NKCountryOfDestination = "BE";
				var cargoDesc2 = nctsBill.GoodsItems.AddNew();
				cargoDesc2.BY_RN_NKCountryOfDestination = "PL";
				headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
				AssertNull(Provider.CountryOfDestination);
			});
		}

		public void TestGrossMass()
		{
			nctsBill.B0_Weight = 1.1235;
			AssertEquals(1.124m, Provider.GrossMass);
		}

		public void TestGrossMass_Normalize()
		{
			nctsBill.B0_Weight = 1.200m;
			AssertEquals("1.2", Provider.GrossMass.ToString());
		}

		public void TestReferenceNumberUCR_ReferenceNumberNullInHeaderProvider_EffectiveReferenceNumberUCRAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_ReferenceID = "REF123";
				movementHeader.BM_UniqueConsignmentReference = "REF456";
				headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
				var result = Provider.ReferenceNumberUCR;
				AssertEquals("result", "REF123", result);
				AssertSame("Cached", result, Provider.ReferenceNumberUCR);
			});
		}

		public void TestReferenceNumberUCR_ReferenceNumberNullInHeaderProvider_EffectiveReferenceNumberUCRAllSame_IsPhase5TP()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				nctsBill.B0_ReferenceID = "REF123";
				headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
				AssertNull(Provider.ReferenceNumberUCR);
			});
		}

		public void TestReferenceNumberUCR_HeaderProviderProvidesReferenceNumber_EffectiveReferenceNumberUCRAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_ReferenceID = "REF123";
				headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns("REF789");
				AssertNull(Provider.ReferenceNumberUCR);
			});
		}

		public void TestReferenceNumberUCR_FallBack_ReferenceNumberNullInHeaderProvider_EffectiveReferenceNumberUCRAllSame_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_ReferenceID = ZString.Empty;
				movementHeader.BM_UniqueConsignmentReference = "REF456";
				headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
				AssertEquals("REF456", Provider.ReferenceNumberUCR);
			});
		}

		public void TestReferenceNumberUCR_FallBack_ReferenceNumberNullInHeaderProvider_EffectiveReferenceNumberUCRDifferent_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				cargoDesc.BY_CommercialReferenceNumber = "REF123";
				var cargoDesc2 = nctsBill.GoodsItems.AddNew();
				cargoDesc2.BY_CommercialReferenceNumber = "REF456";
				headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
				AssertNull(Provider.ReferenceNumberUCR);
			});
		}

		public void TestConsignor_ConsignorIsNullInHeaderProvider_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consignorBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				consignorBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
				var consignorHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
				consignorHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorHeader").PK;
				headerProviderMock.Setup(x => x.Consignor).Returns((INCTSPartyIDAddressContact)null);
				var result = Provider.Consignor;
				AssertEquals("PartyName", "ConsignorBill", result.PartyName);
				AssertSame("Cached", result, Provider.Consignor);
			});
		}

		public void TestConsignor_ConsignorIsNullInHeaderProvider_IsPhase5TP()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var consignorBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				consignorBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;
				headerProviderMock.Setup(x => x.Consignor).Returns((INCTSPartyIDAddressContact)null);
				AssertNull(Provider.Consignor);
			});
		}

		public void TestConsignor_HeaderProviderProvidesConsignor_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consignorBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				consignorBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;
				headerProviderMock.Setup(x => x.Consignor).Returns(NCTSPartyIDAddressContactProvider.NewOrNull(Factory.New<JobDocAddress>()));
				AssertNull(Provider.Consignor);
			});
		}

		public void TestConsignor_FallBack_ConsignorIsNullInHeaderProvider_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consignorHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
				consignorHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorHeader").PK;
				headerProviderMock.Setup(x => x.Consignor).Returns((INCTSPartyIDAddressContact)null);
				AssertEquals("ConsignorHeader", Provider.Consignor.PartyName);
			});
		}

		public void TestConsignor_CUSAllocationContact()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consignorBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				consignorBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;

				var contact = consignorBill.Organisation.Contacts.AddNew();
				contact.OC_ContactName = "TEST";
				contact.OC_Phone = "+123 234 345";
				contact.OC_Email = "test@test.com";

				contact.Allocations.AddNew().PC_Type = "CUS";

				var result = Provider.Consignor;

				AssertEquals("TEST", result.Name);
				AssertEquals("+123 234 345", result.PhoneNumber);
				AssertEquals("test@test.com", result.MailAddress);
			});
		}

		public void TestConsignor_SelectedContact()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consignorDocAddress = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				consignorDocAddress.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;

				var contact = consignorDocAddress.Organisation.Contacts.AddNew();
				contact.OC_ContactName = "TEST";
				contact.OC_Phone = "+123 234 345";
				contact.OC_Email = "test@test.com";

				consignorDocAddress.E2_Contact = "TEST";

				var result = Provider.Consignor;

				AssertEquals("TEST", result.Name);
				AssertEquals("+123 234 345", result.PhoneNumber);
				AssertEquals("test@test.com", result.MailAddress);
			});
		}

		public void TestConsignor_NoSelectedAddressContact()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consignorDocAddress = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				consignorDocAddress.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;

				var contact = consignorDocAddress.Organisation.Contacts.AddNew();
				contact.OC_ContactName = "TEST";
				contact.OC_Phone = "+123 234 345";
				contact.OC_Email = "test@test.com";

				var result = Provider.Consignor;

				AssertEquals(null, result.Name);
				AssertEquals(null, result.PhoneNumber);
				AssertEquals(null, result.MailAddress);
			});
		}

		public void TestConsignee_ConsigneeIsNullInHeaderProvider_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
				var consignorBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				consignorBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;
				var consigneeHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
				consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
				headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
				headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);
				var result = Provider.Consignee;
				AssertEquals("PartyName", "ConsigneeBill", result.PartyName);
				AssertSame("Cached", result, Provider.Consignee);
			});
		}

		public void TestConsignee_CUSContactAllocation()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;

				var contact = nctsBill.Consignee.Organisation.Contacts.AddNew();
				contact.OC_ContactName = "TEST";
				contact.OC_Phone = "+123 234 345";
				contact.OC_Email = "test@test.com";

				contact.Allocations.AddNew().PC_Type = "CUS";

				headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);

				var result = Provider.Consignee;

				AssertEquals("TEST", result.Name);
				AssertEquals("+123 234 345", result.PhoneNumber);
				AssertEquals("test@test.com", result.MailAddress);
			});
		}

		public void TestConsignee_NoSelectedContact()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;

				var contact = nctsBill.Consignee.Organisation.Contacts.AddNew();
				contact.OC_ContactName = "TEST";
				contact.OC_Phone = "+123 234 345";
				contact.OC_Email = "test@test.com";

				headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);

				var result = Provider.Consignee;

				AssertEquals(null, result.Name);
				AssertEquals(null, result.PhoneNumber);
				AssertEquals(null, result.MailAddress);
			});
		}

		public void TestConsignee_SelectedContact()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;

				var contact = nctsBill.Consignee.Organisation.Contacts.AddNew();
				contact.OC_ContactName = "TEST";
				contact.OC_Phone = "+123 234 345";
				contact.OC_Email = "test@test.com";

				consigneeBill.E2_Contact = "TEST";

				headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);

				var result = Provider.Consignee;

				AssertEquals("TEST", result.Name);
				AssertEquals("+123 234 345", result.PhoneNumber);
				AssertEquals("test@test.com", result.MailAddress);
			});
		}

		public void TestConsignee_ConsigneeIsNullInHeaderProvider_IsPhase5TP()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
				headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
				AssertNull(Provider.Consignee);
			});
		}

		public void TestConsignee_HeaderProviderProvidesConsignee_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
				headerProviderMock.Setup(x => x.Consignee).Returns(NCTSPartyIDAddressContactProvider.NewOrNull(Factory.New<JobDocAddress>()));
				AssertNull(Provider.Consignee);
			});
		}

		public void TestConsignee_FallBack_ConsigneeIsNullInHeaderProvider_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consigneeHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
				consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
				nctsHeader.Consignee.E2_OA_Address = consigneeHeader.E2_OA_Address;
				headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
				headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);
				AssertEquals("ConsigneeHeader", Provider.Consignee.PartyName);
			});
		}

		public void TestConsignee_BM_TypeOfSecurityAndAdditionalInfo30600()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
				headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
				headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(false);
				var result = Provider.Consignee;
				AssertNull(Provider.Consignee);
			});
		}

		public void TestAdditionalSupplyChainActors()
		{
			CombineAssertions(() =>
			{
				nctsBill.CusSupplyChainActorReferences.AddNew();
				nctsBill.CusSupplyChainActorReferences.AddNew();
				var result = Provider.AdditionalSupplyChainActors;
				AssertEquals("Count", 2, Provider.AdditionalSupplyChainActors.Count);
				AssertSame("Cached", result, Provider.AdditionalSupplyChainActors);
			});
		}

		public void TestPreviousDocuments()
		{
			CombineAssertions(() =>
			{
				nctsBill.PreviousDocuments.AddNew();
				nctsBill.PreviousDocuments.AddNew();
				var result = Provider.PreviousDocuments;
				AssertEquals("Count", 2, result.Count);
				AssertSame("Cached", result, Provider.PreviousDocuments);
			});
		}

		public void TestSupportingDocuments()
		{
			CombineAssertions(() =>
			{
				nctsBill.SupportingDocuments.AddNew();
				nctsBill.SupportingDocuments.AddNew();
				var result = Provider.SupportingDocuments;
				AssertEquals("Count", 2, result.Count);
				AssertSame("Cached", result, Provider.SupportingDocuments);
			});
		}

		public void TestTransportDocuments()
		{
			CombineAssertions(() =>
			{
				var transportDocument1 = nctsBill.AdditionalDocuments.AddNew();
				transportDocument1.CSI_SubType = "TRA";
				transportDocument1.CSI_Code = "1";
				var transportDocument2 = nctsBill.AdditionalDocuments.AddNew();
				transportDocument2.CSI_SubType = "TRA";
				transportDocument2.CSI_Code = "2";
				var additionalReference = nctsBill.AdditionalDocuments.AddNew();
				additionalReference.CSI_SubType = "REF";
				additionalReference.CSI_Code = "3";
				var additionalInformation = nctsBill.AdditionalDocuments.AddNew();
				additionalInformation.CSI_SubType = "INF";
				additionalInformation.CSI_Code = "4";
				var result = Provider.TransportDocuments;
				AssertContainsExactElementsInAnyOrder("SubType TRA", new string[] { "1", "2" }, result.Select(x => x.Type));
				AssertSame("Cached", result, Provider.TransportDocuments);
			});
		}

		public void TestAdditionalReferences()
		{
			CombineAssertions(() =>
			{
				var transportDocument = nctsBill.AdditionalDocuments.AddNew();
				transportDocument.CSI_SubType = "TRA";
				transportDocument.CSI_Code = "1";
				var additionalReference1 = nctsBill.AdditionalDocuments.AddNew();
				additionalReference1.CSI_SubType = "REF";
				additionalReference1.CSI_Code = "2";
				var additionalReference2 = nctsBill.AdditionalDocuments.AddNew();
				additionalReference2.CSI_SubType = "REF";
				additionalReference2.CSI_Code = "3";
				var additionalInformation = nctsBill.AdditionalDocuments.AddNew();
				additionalInformation.CSI_SubType = "INF";
				additionalInformation.CSI_Code = "4";
				var result = Provider.AdditionalReferences;
				AssertContainsExactElementsInAnyOrder("SubType REF", new string[] { "2", "3" }, result.Select(x => x.Type));
				AssertSame("Cached", result, Provider.AdditionalReferences);
			});
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				var transportDocument = nctsBill.AdditionalDocuments.AddNew();
				transportDocument.CSI_SubType = "TRA";
				transportDocument.CSI_Code = "1";
				var additionalReference = nctsBill.AdditionalDocuments.AddNew();
				additionalReference.CSI_SubType = "REF";
				additionalReference.CSI_Code = "2";
				var additionalInformation1 = nctsBill.AdditionalDocuments.AddNew();
				additionalInformation1.CSI_SubType = "INF";
				additionalInformation1.CSI_Code = "3";
				var additionalInformation2 = nctsBill.AdditionalDocuments.AddNew();
				additionalInformation2.CSI_SubType = "INF";
				additionalInformation2.CSI_Code = "4";
				var result = Provider.AdditionalInformation;
				AssertContainsExactElementsInAnyOrder("SubType INF", new string[] { "3", "4" }, result.Select(x => x.Code));
				AssertSame("Cached", result, Provider.AdditionalInformation);
			});
		}

		public void TestMethodOfPayment_MethodOfPaymentNullInHeaderProvider_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_TransportPaymentMethod = "A";
				movementHeader.BM_MethodOfPayment = "B";
				headerProviderMock.Setup(x => x.MethodOfPayment).Returns((string)null);
				var result = Provider.MethodOfPayment;
				AssertEquals("Result", "A", result);
				AssertSame("Cached", result, Provider.MethodOfPayment);
			});
		}

		public void TestMethodOfPayment_MethodOfPaymentNullInHeaderProvider_IsPhase5TP()
		{
			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				nctsBill.B0_TransportPaymentMethod = "A";
				headerProviderMock.Setup(x => x.MethodOfPayment).Returns((string)null);
				AssertNull(Provider.MethodOfPayment);
			});
		}

		public void TestMethodOfPayment_HeaderProviderProvidesMethodOfPayment_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_TransportPaymentMethod = "A";
				headerProviderMock.Setup(x => x.MethodOfPayment).Returns("C");
				AssertNull(Provider.MethodOfPayment);
			});
		}

		public void TestMethodOfPayment_FallBack_MethodOfPaymentNullInHeaderProvider_OutsidePhase5TP()
		{
			RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				nctsBill.B0_TransportPaymentMethod = ZString.Empty;
				movementHeader.BM_MethodOfPayment = "B";
				headerProviderMock.Setup(x => x.MethodOfPayment).Returns((string)null);
				AssertEquals("B", Provider.MethodOfPayment);
			});
		}

		public void TestConsignmentItems()
		{
			CombineAssertions(() =>
			{
				nctsBill.GoodsItems.AddNew();
				var result = Provider.ConsignmentItems;
				AssertEquals("Count", 2, result.Count);
				AssertSame("Cached", result, Provider.ConsignmentItems);
				AssertContainsExactElementsInExactOrder(new [] { 1, 2 }, result.Select(e => e.GoodsItemNumber));
			});
		}

		protected override IDEPDATHouseConsignment GetProvider() => DEPDATHouseConsignmentProvider.NewOrNull(nctsBill, headerProviderMock.Object);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader = nctsHeader.MovementHeader;
			nctsBill = nctsHeader.Bills.AddNew();
			cargoDesc = nctsBill.GoodsItems.AddNew();
			headerProviderMock = new Mock<IDEPDATHeader>();
		}
		Mock<IDEPDATHeader> headerProviderMock;
		NctsHeader nctsHeader;
		NctsBill nctsBill;
		NctsDepartureMovementHeader movementHeader;
		EU.NCTS.Business.NctsDepartureCargoDesc cargoDesc;
	}
}
