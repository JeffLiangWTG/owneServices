using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;
using UniversalReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DEPDATConsignmentItemProviderTest : DataProviderTestCase<IDEPDATConsignmentItem>
	{
		public void TestGoodsItemNumber()
		{
			cargoDesc.BY_LineNo = 3;
			AssertEquals(3, Provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			cargoDesc.BY_DeclarationGoodsItemNumber = 2;
			AssertEquals(2, Provider.DeclarationGoodsItemNumber);
		}

		public void TestDeclarationType_HeaderProviderT()
		{
			cargoDesc.BY_Type = NctsDeclarationTypeList.Codes.T1;
			headerProviderMock.Setup(x => x.DeclarationType).Returns(NctsDeclarationTypeList.Codes.T);
			CombineAssertions(() =>
			{
				var result = Provider.DeclarationType;
				AssertEquals("Result", "T1", result);
				AssertSame("Cached", result, Provider.DeclarationType);
			});
		}

		public void TestDeclarationType_HeaderProviderT1()
		{
			cargoDesc.BY_Type = NctsDeclarationTypeList.Codes.T1;
			headerProviderMock.Setup(x => x.DeclarationType).Returns(NctsDeclarationTypeList.Codes.T1);
			AssertNull(Provider.DeclarationType);
		}

		public void TestDeclarationType_HeaderProviderT2()
		{
			cargoDesc.BY_Type = NctsDeclarationTypeList.Codes.T1;
			headerProviderMock.Setup(x => x.DeclarationType).Returns(NctsDeclarationTypeList.Codes.T2);
			AssertNull(Provider.DeclarationType);
		}

		public void TestCountryOfDispatch_CountryOfDispatchNullInHouseConsignmentProviderAndHeaderProvider()
		{
			CombineAssertions(() =>
			{
				cargoDesc.BY_RN_NKCountryOfDispatch = "BE";
				bill.B0_RN_NKCountryOfExport = "FR";
				movementHeader.BM_RN_NKCountryOfDispatch = "DE";
				houseConsignmentProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
				headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
				var result = Provider.CountryOfDispatch;
				AssertEquals("Result", "BE", result);
				AssertSame("Cached", result, Provider.CountryOfDispatch);
			});
		}

		public void TestCountryOfDispatch_CountryOfDispatchNullInHouseConsignmentProvider()
		{
			cargoDesc.BY_RN_NKCountryOfDispatch = "BE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
			headerProviderMock.Setup(x => x.CountryOfDispatch).Returns("PL");
			AssertNull(Provider.CountryOfDispatch);
		}

		public void TestCountryOfDispatch_CountryOfDispatchNullInHeaderProvider()
		{
			cargoDesc.BY_RN_NKCountryOfDispatch = "BE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDispatch).Returns("NL");
			headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
			AssertNull(Provider.CountryOfDispatch);
		}

		public void TestCountryOfDispatch_FallBackToHouseConsignment()
		{
			cargoDesc.BY_RN_NKCountryOfDispatch = ZString.Empty;
			bill.B0_RN_NKCountryOfExport = "FR";
			movementHeader.BM_RN_NKCountryOfDispatch = "DE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
			headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
			AssertEquals("FR", Provider.CountryOfDispatch);
		}

		public void TestCountryOfDispatch_FallBackToHeader()
		{
			cargoDesc.BY_RN_NKCountryOfDispatch = ZString.Empty;
			bill.B0_RN_NKCountryOfExport = ZString.Empty;
			movementHeader.BM_RN_NKCountryOfDispatch = "DE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
			headerProviderMock.Setup(x => x.CountryOfDispatch).Returns((string)null);
			AssertEquals("DE", Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination_CountryOfDestinationNullInHouseConsignmentProviderAndHeaderProvider()
		{
			CombineAssertions(() =>
			{
				cargoDesc.BY_RN_NKCountryOfDestination = "BE";
				bill.B0_RN_NKCountryOfDestination = "FR";
				movementHeader.BM_RL_NKDestinationPort = "DE";
				houseConsignmentProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
				headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
				var result = Provider.CountryOfDestination;
				AssertEquals("Result", "BE", result);
				AssertSame("Cached", result, Provider.CountryOfDestination);
			});
		}

		public void TestCountryOfDestination_CountryOfDestinationNullInHouseConsignmentProvider()
		{
			cargoDesc.BY_RN_NKCountryOfDestination = "BE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
			headerProviderMock.Setup(x => x.CountryOfDestination).Returns("PL");
			AssertNull(Provider.CountryOfDestination);
		}

		public void TestCountryOfDestination_CountryOfDestinationNullInHeaderProvider()
		{
			cargoDesc.BY_RN_NKCountryOfDestination = "BE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDestination).Returns("NL");
			headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
			AssertNull(Provider.CountryOfDestination);
		}

		public void TestCountryOfDestination_FallBackToHouseConsignment()
		{
			cargoDesc.BY_RN_NKCountryOfDestination = ZString.Empty;
			bill.B0_RN_NKCountryOfDestination = "FR";
			movementHeader.BM_RL_NKDestinationPort = "DE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
			headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
			AssertEquals("FR", Provider.CountryOfDestination);
		}

		public void TestCountryOfDestination_FallBackToHeader()
		{
			cargoDesc.BY_RN_NKCountryOfDestination = ZString.Empty;
			bill.B0_RN_NKCountryOfDestination = ZString.Empty;
			movementHeader.BM_RL_NKDestinationPort = "DE";
			houseConsignmentProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
			headerProviderMock.Setup(x => x.CountryOfDestination).Returns((string)null);
			AssertEquals("DE", Provider.CountryOfDestination);
		}

		public void TestReferenceNumberUCR_ReferenceNumberNullInHouseConsignmentProviderAndHeaderProvider()
		{
			CombineAssertions(() =>
			{
				cargoDesc.BY_CommercialReferenceNumber = "REF1";
				bill.B0_ReferenceID = "REF2";
				movementHeader.BM_UniqueConsignmentReference = "REF3";
				houseConsignmentProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
				headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
				var result = Provider.ReferenceNumberUCR;
				AssertEquals("Result", "REF1", result);
				AssertSame("Cached", result, Provider.ReferenceNumberUCR);
			});
		}

		public void TestReferenceNumberUCR_ReferenceNumberNullInHouseConsignmentProvider()
		{
			cargoDesc.BY_CommercialReferenceNumber = "REF1";
			bill.B0_ReferenceID = "REF2";
			movementHeader.BM_UniqueConsignmentReference = "REF3";
			houseConsignmentProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
			headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns("REF5");
			AssertNull(Provider.ReferenceNumberUCR);
		}

		public void TestReferenceNumberUCR_ReferenceNumberNullInHeaderProvider()
		{
			cargoDesc.BY_CommercialReferenceNumber = "REF1";
			bill.B0_ReferenceID = "REF2";
			movementHeader.BM_UniqueConsignmentReference = "REF3";
			houseConsignmentProviderMock.Setup(x => x.ReferenceNumberUCR).Returns("REF4");
			headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
			AssertNull(Provider.ReferenceNumberUCR);
		}

		public void TestReferenceNumberUCR_FallBackToHouseConsignment()
		{
			cargoDesc.BY_CommercialReferenceNumber = ZString.Empty;
			bill.B0_ReferenceID = "REF2";
			movementHeader.BM_UniqueConsignmentReference = "REF3";
			houseConsignmentProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
			headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
			AssertEquals("REF2", Provider.ReferenceNumberUCR);
		}

		public void TestReferenceNumberUCR_FallBackToHeader()
		{
			cargoDesc.BY_CommercialReferenceNumber = ZString.Empty;
			bill.B0_ReferenceID = ZString.Empty;
			movementHeader.BM_UniqueConsignmentReference = "REF3";
			houseConsignmentProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
			headerProviderMock.Setup(x => x.ReferenceNumberUCR).Returns((string)null);
			AssertEquals("REF3", Provider.ReferenceNumberUCR);
		}

		public void TestConsignee_ConsigneeNullInHouseConsignmentProviderAndHeaderProvider()
		{
			CombineAssertions(() =>
			{
				var consigneeCargoDesc = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsigneeJobDocAddressRequirement);
				consigneeCargoDesc.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;
				var consignorCargoDesc = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsignorJobDocAddressRequirement);
				consignorCargoDesc.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorCargoDesc").PK;
				var consigneeBill = bill.DocAddresses.CreateWithRequirement(bill.ConsigneeJobDocAddressRequirement);
				consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
				var consigneeHeader = header.DocAddresses.CreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
				consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
				houseConsignmentProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
				headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
				headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);
				var result = Provider.Consignee;
				AssertEquals("PartyName", "ConsigneeCargoDesc", result.PartyName);
				AssertSame("Cached", result, Provider.Consignee);
			});
		}

		public void TestConsignee_ConsigneeNullInHouseConsignmentProvider()
		{
			var consigneeCargoDesc = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsigneeJobDocAddressRequirement);
			consigneeCargoDesc.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;
			houseConsignmentProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			headerProviderMock.Setup(x => x.Consignee).Returns(NCTSPartyIDAddressContactProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNull(Provider.Consignee);
		}

		public void TestConsignee_ConsigneeNullInHeaderProvider()
		{
			var consigneeCargoDesc = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsigneeJobDocAddressRequirement);
			consigneeCargoDesc.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;
			houseConsignmentProviderMock.Setup(x => x.Consignee).Returns(NCTSPartyIDAddressContactProvider.NewOrNull(Factory.New<JobDocAddress>()));
			headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			AssertNull(Provider.Consignee);
		}

		public void TestConsignee_FallBackToHouseConsignment()
		{
			var consigneeBill = bill.DocAddresses.CreateWithRequirement(bill.ConsigneeJobDocAddressRequirement);
			consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
			var consigneeHeader = header.DocAddresses.CreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
			consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
			houseConsignmentProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);
			AssertEquals("ConsigneeBill", Provider.Consignee.PartyName);
		}

		public void TestConsignee_FallBackToHeader()
		{
			var consigneeHeader = header.DocAddresses.CreateWithRequirement(header.ConsigneeJobDocAddressRequirement);
			consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
			header.Consignee.E2_OA_Address = consigneeHeader.E2_OA_Address;
			houseConsignmentProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);
			AssertEquals("ConsigneeHeader", Provider.Consignee.PartyName);
		}

		public void TestConsignee_BM_TypeOfSecurityAndAdditionalInfo30600()
		{
			var consigneeCargoDesc = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsigneeJobDocAddressRequirement);
			consigneeCargoDesc.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;
			houseConsignmentProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			headerProviderMock.Setup(x => x.Consignee).Returns((INCTSPartyIDAddressContact)null);
			headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(false);
			AssertNull(Provider.Consignee);
		}

		public void TestConsignee_FallBackToCUSAllocationForOrg()
		{
			var consigneeDocAddress = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsigneeJobDocAddressRequirement);
			consigneeDocAddress.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;

			var contact = consigneeDocAddress.Organisation.Contacts.AddNew();
			contact.OC_ContactName = "TEST";
			contact.OC_Phone = "+123 234 345";
			contact.OC_Email = "test@test.com";

			contact.Allocations.AddNew().PC_Type = "CUS";

			headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);

			var result = Provider.Consignee;
			AssertEquals("TEST", result.Name);
			AssertEquals("+123 234 345", result.PhoneNumber);
			AssertEquals("test@test.com", result.MailAddress);
		}

		public void TestConsignee_UseSelectedContact()
		{
			var consigneeDocAddress = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsigneeJobDocAddressRequirement);
			consigneeDocAddress.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;

			var contact = consigneeDocAddress.Organisation.Contacts.AddNew();
			contact.OC_ContactName = "TEST";
			contact.OC_Phone = "+123 234 345";
			contact.OC_Email = "test@test.com";

			consigneeDocAddress.E2_Contact = "TEST";

			headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);

			var result = Provider.Consignee;
			AssertEquals("TEST", result.Name);
			AssertEquals("+123 234 345", result.PhoneNumber);
			AssertEquals("test@test.com", result.MailAddress);
		}

		public void TestConsignee_OverriddenContactDetails()
		{
			var consigneeDocAddress = cargoDesc.DocAddresses.CreateWithRequirement(cargoDesc.ConsigneeJobDocAddressRequirement);
			consigneeDocAddress.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeCargoDesc").PK;

			consigneeDocAddress.E2_AddressOverride = true;
			consigneeDocAddress.E2_Contact = "TEST";
			consigneeDocAddress.E2_Phone = "+123 234 345";
			consigneeDocAddress.E2_Email = "test@test.com";

			headerProviderMock.Setup(x => x.ShouldPopulateConsignee).Returns(true);

			var result = Provider.Consignee;
			AssertEquals("TEST", result.Name);
			AssertEquals("+123 234 345", result.PhoneNumber);
			AssertEquals("test@test.com", result.MailAddress);
		}

		public void TestAdditionalSupplyChainActors()
		{
			CombineAssertions(() =>
			{
				cargoDesc.CusSupplyChainActorReferences.AddNew();
				cargoDesc.CusSupplyChainActorReferences.AddNew();
				var result = Provider.AdditionalSupplyChainActors;
				AssertEquals("Count", 2, Provider.AdditionalSupplyChainActors.Count);
				AssertSame("Cached", result, Provider.AdditionalSupplyChainActors);
			});
		}

		public void TestDescriptionOfGoods()
		{
			cargoDesc.BY_Description = "Test Description";
			AssertEquals("Test Description", Provider.DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("empty", null, Provider.CusCode);

				cargoDesc.BY_CusC4Number = "12345";
				AssertEquals("not empty", "12345", Provider.CusCode);
			});
		}

		public void TestHarmonizedSystemSubheadingCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("empty", null, Provider.HarmonizedSystemSubheadingCode);

				cargoDesc.BY_HarmonisedTariff = "12345678";
				AssertEquals("Truncate 6 digits", "123456", Provider.HarmonizedSystemSubheadingCode);
			});
		}

		public void TestCombinedNomenclatureCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("empty", null, Provider.CombinedNomenclatureCode);

				cargoDesc.BY_HarmonisedTariff = "12345678";
				AssertEquals("Digit 7, 8", "78", Provider.CombinedNomenclatureCode);
			});
		}

		public void TestDangerousGoods()
		{
			CombineAssertions(() =>
			{
				var undgSubstance1 = Factory.New<UNDGSubstance>();
				undgSubstance1.DG_Code = "1100a";
				undgSubstance1.DG_UNNO = "1100";
				var undgSubstance2 = Factory.New<UNDGSubstance>();
				undgSubstance2.DG_Code = "2200b";
				undgSubstance2.DG_UNNO = "2200";
				var dangerousGood1 = cargoDesc.UNDGs.AddNew();
				dangerousGood1.DI_DG = undgSubstance1.PK;
				var dangerousGood2 = cargoDesc.UNDGs.AddNew();
				dangerousGood2.DI_DG = undgSubstance2.PK;
				var result = Provider.DangerousGoods;
				AssertContainsExactElementsInAnyOrder("Elements", new[] { "1100", "2200" }, result);
				AssertSame("Cached", result, Provider.DangerousGoods);
			});
		}

		public void TestGrossMass()
		{
			cargoDesc.BY_GrossWeight = 1.2;
			cargoDesc.BY_GrossWeightUnit = "KG";
			AssertEquals(1.2m, Provider.GrossMass);
		}

		public void TestGrossMassConversionAndRounding()
		{
			cargoDesc.BY_GrossWeight = 1123.567m;
			cargoDesc.BY_GrossWeightUnit = "G";
			AssertEquals(1.124m, Provider.GrossMass);
		}

		public void TestGrossMassNormalize()
		{
			cargoDesc.BY_GrossWeight = 1.200m;
			cargoDesc.BY_GrossWeightUnit = "KG";
			AssertEquals("1.2", Provider.GrossMass.ToString());
		}

		public void TestNetMass()
		{
			cargoDesc.BY_NetWeight = 1.2;
			cargoDesc.BY_NetWeightUnit = "KG";
			AssertEquals(1.2m, Provider.NetMass);
		}

		public void TestNetMassNormalize()
		{
			cargoDesc.BY_NetWeight = 1.000m;
			cargoDesc.BY_NetWeightUnit = "KG";
			AssertEquals("1", Provider.NetMass.ToString());
		}

		public void TestNetMassConversionAndRounding()
		{
			cargoDesc.BY_NetWeight = 1123.4567m;
			cargoDesc.BY_NetWeightUnit = "G";
			AssertEquals(1.123457m, Provider.NetMass);
		}

		public void TestNetMassRoundingTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				cargoDesc.BY_NetWeight = 1.123567;
				cargoDesc.BY_NetWeightUnit = "KG";
				AssertEquals(1.124m, Provider.NetMass);
			}
		}

		public void TestPackaging() => CombineAssertions(() =>
		{
			cargoDesc.Packages.AddNew();
			cargoDesc.Packages.AddNew();

			AssertType<DEPDATPackageProvider>("Type", Provider.Packaging.First());
			AssertEquals("Count", 2, Provider.Packaging.Count);
		});

		public void TestPreviousDocuments() => CombineAssertions(() =>
		{
			cargoDesc.PreviousDocuments.AddNew();
			cargoDesc.PreviousDocuments.AddNew();

			AssertType<NCTSDocumentProvider>("Type", Provider.PreviousDocuments.First());
			AssertEquals("Count", 2, Provider.PreviousDocuments.Count);
		});

		public void TestSupportingDocuments() => CombineAssertions(() =>
		{
			cargoDesc.SupportingDocuments.AddNew();
			cargoDesc.SupportingDocuments.AddNew();

			AssertType<NCTSDocumentProvider>("Type", Provider.SupportingDocuments.First());
			AssertEquals("Count", 2, Provider.SupportingDocuments.Count);
		});

		public void TestAdditionalReferences() => CombineAssertions(() =>
		{
			cargoDesc.AdditionalInfos.AddNew().CSI_SubType = "REF";
			cargoDesc.AdditionalInfos.AddNew().CSI_SubType = "REF";
			cargoDesc.AdditionalInfos.AddNew().CSI_SubType = "INF";

			AssertType<NCTSDocumentProvider>("Type", Provider.AdditionalReferences.First());
			AssertEquals("Count", 2, Provider.AdditionalReferences.Count);
		});

		public void TestAdditionalInformation() => CombineAssertions(() =>
		{
			cargoDesc.AdditionalInfos.AddNew().CSI_SubType = "REF";
			cargoDesc.AdditionalInfos.AddNew().CSI_SubType = "INF";
			cargoDesc.AdditionalInfos.AddNew().CSI_SubType = "INF";

			AssertType<NCTSAdditionalInformationProvider>("Type", Provider.AdditionalInformation.First());
			AssertEquals("Count", 2, Provider.AdditionalInformation.Count);
		});

		public void TestMethodOfPayment_NullInHeader() => CombineAssertions(() =>
		{
			var cargoDesc2 = cargoDesc.Bill.GoodsItems.AddNew();
			cargoDesc2.BY_TransportChargesMethodOfPayment = "A";
			headerProviderMock.SetupGet(c => c.MethodOfPayment).Returns((string)null);
			cargoDesc.Bill.Header.MovementHeader.BM_MethodOfPayment = "G";
			AssertEquals(nameof(CusInBondMoveHeader.BM_MethodOfPayment), "G", GetProvider().MethodOfPayment);
			cargoDesc.Bill.B0_TransportPaymentMethod = "B";
			AssertEquals(nameof(NctsBill.B0_TransportPaymentMethod), "B", GetProvider().MethodOfPayment);
			cargoDesc.BY_TransportChargesMethodOfPayment = "C";
			AssertEquals(nameof(NctsDepartureCargoDesc.BY_TransportChargesMethodOfPayment), "C", GetProvider().MethodOfPayment);
		});

		public void TestMethodOfPayment_NullInConsignment() => CombineAssertions(() =>
		{
			var cargoDesc2 = cargoDesc.Bill.GoodsItems.AddNew();
			cargoDesc2.BY_TransportChargesMethodOfPayment = "A";
			houseConsignmentProviderMock.SetupGet(c => c.MethodOfPayment).Returns((string)null);
			cargoDesc.Bill.Header.MovementHeader.BM_MethodOfPayment = "G";
			AssertEquals(nameof(CusInBondMoveHeader.BM_MethodOfPayment), "G", GetProvider().MethodOfPayment);
			cargoDesc.Bill.B0_TransportPaymentMethod = "B";
			AssertEquals(nameof(NctsBill.B0_TransportPaymentMethod), "B", GetProvider().MethodOfPayment);
			cargoDesc.BY_TransportChargesMethodOfPayment = "C";
			AssertEquals(nameof(NctsDepartureCargoDesc.BY_TransportChargesMethodOfPayment), "C", GetProvider().MethodOfPayment);
		});

		public void TestMethodOfPayment_ConsignmentMapped() => CombineAssertions(() =>
		{
			houseConsignmentProviderMock.SetupGet(c => c.MethodOfPayment).Returns("C");
			var cargoDesc2 = cargoDesc.Bill.GoodsItems.AddNew();
			cargoDesc2.BY_TransportChargesMethodOfPayment = "C";
			cargoDesc.Bill.Header.MovementHeader.BM_MethodOfPayment = "A";
			cargoDesc.Bill.B0_TransportPaymentMethod = "B";
			cargoDesc.BY_TransportChargesMethodOfPayment = "C";

			AssertNull("MethodOfPayment mapped on consignment level", GetProvider().MethodOfPayment);
		});

		public void TestMethodOfPayment_HeaderMapped() => CombineAssertions(() =>
		{
			headerProviderMock.SetupGet(c => c.MethodOfPayment).Returns("C");
			var cargoDesc2 = cargoDesc.Bill.GoodsItems.AddNew();
			cargoDesc2.BY_TransportChargesMethodOfPayment = "C";
			cargoDesc.Bill.Header.MovementHeader.BM_MethodOfPayment = "A";
			cargoDesc.Bill.B0_TransportPaymentMethod = "B";
			cargoDesc.BY_TransportChargesMethodOfPayment = "C";

			AssertNull("MethodOfPayment mapped on header level", GetProvider().MethodOfPayment);
		});

		public void TestSummaryDeclarationIdentificationType_ReturnsNullWhenNoPreviousDocumentsAndNoPreviousProcedure()
		{
			AssertNull("Should return null when there is no Previous Document with CSI_Code = N337 and no Previous Procedure with CSI_Procedure = N337", Provider.SummaryDeclarationIdentificationType);
		}

		public void TestSummaryDeclarationIdentificationType_ReturnsNullWhenNoPreviousProcedures()
		{
			var prevDocument = cargoDesc.PreviousDocuments.AddNew();
			prevDocument.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			AssertNull("Should return null when there are no Previous Procedures with CSI_Procedure = N337", Provider.SummaryDeclarationIdentificationType);
		}

		public void TestSummaryDeclarationIdentificationType_ReturnsNullWhenNoN337PreviousProcedures()
		{
			var prevDocument = cargoDesc.PreviousDocuments.AddNew();
			prevDocument.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			prevProcedure1.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			prevProcedure2.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;

			AssertNull("Should return null when there are no Previous Procedures with CSI_Procedure = N337", Provider.SummaryDeclarationIdentificationType);
		}

		public void TestSummaryDeclarationIdentificationType_AWBReturnsAWB()
		{
			var prevDocument = cargoDesc.PreviousDocuments.AddNew();
			prevDocument.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			prevProcedure1.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			prevProcedure2.CSI_SubType = PreviousDocSubTypeList.Codes.REG;

			AssertEquals(PreviousDocSubTypeList.Codes.AWB, Provider.SummaryDeclarationIdentificationType);
		}

		public void TestSummaryDeclarationIdentificationType_ULDReturnsAWB()
		{
			var prevDocument = cargoDesc.PreviousDocuments.AddNew();
			prevDocument.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			prevProcedure1.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;
			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			prevProcedure2.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;

			AssertEquals(PreviousDocSubTypeList.Codes.AWB, Provider.SummaryDeclarationIdentificationType);
		}

		public void TestSummaryDeclarationIdentificationType_REGReturnsREG()
		{
			var prevDocument = cargoDesc.PreviousDocuments.AddNew();
			prevDocument.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			prevProcedure1.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			prevProcedure2.CSI_SubType = PreviousDocSubTypeList.Codes.REG;

			AssertEquals(PreviousDocSubTypeList.Codes.REG, Provider.SummaryDeclarationIdentificationType);
		}

		public void TestSummaryDeclarationIdentificationType_N337ProceduresOnly()
		{
			var previousDoc = cargoDesc.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			prevProcedure1.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			prevProcedure2.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			var prevProcedure3 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure3.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			prevProcedure3.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;

			AssertEquals("Only Previous Procedures with Code N337 should be included in the SummaryDeclarationIdentificationType calculation",
				PreviousDocSubTypeList.Codes.REG, Provider.SummaryDeclarationIdentificationType);
		}

		public void TestSummaryDeclarationReferences_NoN337PreviousDocument()
		{
			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Code = NctsPreviousProcedureList.Codes._N337;
			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Code = NctsPreviousProcedureList.Codes._N337;
			var prevProcedure3 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure3.CSI_Code = NctsPreviousProcedureList.Codes._9DEY;
			var prevProcedure4 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure4.CSI_Code = NctsPreviousProcedureList.Codes._9DEZ;

			AssertEquals(0, Provider.SummaryDeclarationReferences.Count);
		}

		public void TestSummaryDeclarationReferences()
		{
			var prevDoc = cargoDesc.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			var prevProcedure3 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure3.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			var prevProcedure4 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure4.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

			AssertEquals(2, Provider.SummaryDeclarationReferences.Count);
		}

		public void TestLRN()
		{
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			cargoDesc.PreviousProcedureMaster.CSI_ReferenceNumber2 = "RefNum";
			AssertEquals("9DEZ PreviousDocument was added when setting CSI_Procedure", "RefNum", Provider.LRN);
		}

		public void TestLRN_No9DEZPreviousProcedure()
		{
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			cargoDesc.PreviousProcedureMaster.CSI_ReferenceNumber2 = "RefNum";

			cargoDesc.PreviousProcedures.RemoveAndDeleteAll();
			AssertNull(Provider.LRN);
		}

		public void TestCustomsWarehousingAuthorisationType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "C517", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			cargoDesc.PreviousProcedureMaster.AuthorizationNumber = "DECWP5864LA000068";
			AssertEquals("C517", Provider.CustomsWarehousingAuthorisationType);
		}

		public void TestCustomsWarehousingAuthorisationType_InvalidFormat()
		{
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			cargoDesc.PreviousProcedureMaster.AuthorizationNumber = "123";
			AssertEquals(null, Provider.CustomsWarehousingAuthorisationType);
		}

		public void TestCustomsWarehousingAuthorisationType_No9DEZPreviousDocumentOrPreviousProcedure()
		{
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			cargoDesc.PreviousProcedureMaster.AuthorizationNumber = "DECWP5864LA000068";
			cargoDesc.PreviousDocuments.RemoveAndDeleteAll();
			AssertEquals(null, Provider.CustomsWarehousingAuthorisationType);
		}

		public void TestCustomsWarehousingAuthorisationType_DoesNotThrow()
		{
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			AssertNoExceptionThrown(() => _ = Provider.CustomsWarehousingAuthorisationType);
		}

		public void TestCustomsWarehousingAuthorisationNumber()
		{
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			cargoDesc.PreviousProcedureMaster.AuthorizationNumber = "DECWP5864LA000068";
			AssertEquals("DECWP5864LA000068", Provider.CustomsWarehousingAuthorisationNumber);
		}

		public void TestCustomsWarehousingAuthorisationNumber_No9DEZPreviousDocumentOrPreviousProcedure()
		{
			cargoDesc.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			cargoDesc.PreviousProcedureMaster.AuthorizationNumber = "DECWP5864LA000068";
			cargoDesc.PreviousProcedures.RemoveAndDeleteAll();
			AssertNull(Provider.CustomsWarehousingAuthorisationNumber);
		}

		public void TestCustomsWarehousingReferences()
		{
			var procedure1 = cargoDesc.PreviousProcedures.AddNew();
			var procedure2 = cargoDesc.PreviousProcedures.AddNew();
			var procedure3 = cargoDesc.PreviousProcedures.AddNew();
			var document = cargoDesc.PreviousDocuments.AddNew();
			procedure1.CSI_Description = "First";
			procedure2.CSI_Description = "Second";
			procedure3.CSI_Description = "Third";
			procedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			procedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			procedure3.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			document.CSI_Code = NctsPreviousProcedureList.Codes._N337;

			CombineAssertions(() =>
			{
				document.CSI_Code = NctsPreviousProcedureList.Codes._9DEZ;

				AssertContainsExactElementsInAnyOrder("Only the 9DEZ procedures should be included.", new[] { "First", "Second" },
					GetProvider().CustomsWarehousingReferences.Select(x => x.Complement));
			});
		}

		public void TestInwardProcessingSimplyGrantedAuthorisation_Default()
		{
			AssertEquals(false, Provider.InwardProcessingSimplyGrantedAuthorisation);
		}

		public void TestInwardProcessingSimplyGrantedAuthorisation_ReturnsFalse()
		{
			var prevProc = cargoDesc.PreviousProcedures.AddNew();
			prevProc.SimplifiedGrantAuthorizationFlag = false;

			AssertEquals(false, Provider.InwardProcessingSimplyGrantedAuthorisation);
		}

		public void TestInwardProcessingSimplyGrantedAuthorisation_ReturnsTrue()
		{
			var prevProc = cargoDesc.PreviousProcedures.AddNew();
			prevProc.SimplifiedGrantAuthorizationFlag = true;

			AssertEquals(true, Provider.InwardProcessingSimplyGrantedAuthorisation);
		}

		public void TestInwardProcessingAuthorisationType()
		{
			AssertEquals(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601, Provider.InwardProcessingAuthorisationType);
		}

		public void TestInwardProcessingAuthorisationNumber()
		{
			var prevProc = cargoDesc.PreviousProcedures.AddNew();
			prevProc.AuthorizationNumber = "ABCD1234";

			AssertEquals("ABCD1234", Provider.InwardProcessingAuthorisationNumber);
		}

		public void TestInwardProcessingCustomsOffice()
		{
			var prevProc = cargoDesc.PreviousProcedures.AddNew();
			NCTSTestHelper.CreateCustomsOfficeForTest(cargoDesc.MoveHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "CUS1234", ZDateTime.Empty);

			prevProc.CSI_CustomsOffice = "CUS1234";

			AssertEquals("CUS1234", Provider.InwardProcessingCustomsOffice);
		}

		public void TestInwardProcessingReferences()
		{
			var prevProcedure1 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure1.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;

			var prevProcedure2 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure2.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;

			var prevProcedure3 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure3.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

			var prevProcedure4 = cargoDesc.PreviousProcedures.AddNew();
			prevProcedure4.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;

			AssertEquals("Only Previous Procedures with CSI_Procedure = 9DEY", 2, Provider.InwardProcessingReferences.Count);
		}

		protected override IDEPDATConsignmentItem GetProvider() => DEPDATConsignmentItemProvider.NewOrNull(cargoDesc, headerProviderMock.Object, houseConsignmentProviderMock.Object);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader = header.MovementHeader;
			bill = header.Bills.AddNew();
			cargoDesc = bill.GoodsItems.AddNew();
			houseConsignmentProviderMock = new Mock<IDEPDATHouseConsignment>();
			headerProviderMock = new Mock<IDEPDATHeader>();
		}
		NctsDepartureCargoDesc cargoDesc;
		NctsBill bill;
		NctsDepartureMovementHeader movementHeader;
		NctsHeader header;
		Mock<IDEPDATHouseConsignment> houseConsignmentProviderMock;
		Mock<IDEPDATHeader> headerProviderMock;
	}
}
