using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	partial class NctsHeaderBaseOnlyTest
	{
		public void TestTotalInvoiceValue_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var goodsItem = departureHeader.MovementHeader.GoodsItems.AddNew();
			var goodsItem2 = departureHeader.MovementHeader.GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("not populated", 0m, departureHeader.TotalInvoiceValue);

				goodsItem.BY_MonetaryValue = 1.2;
				goodsItem2.BY_MonetaryValue = 2.3;
				AssertEquals("populated", 3.5m, departureHeader.TotalInvoiceValue);
			});
		}

		public void TestTotalInvoiceValue_Phase5()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			var goodsItem = departureHeader.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2 = departureHeader.Bills.AddNew().GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("not populated", 0m, departureHeader.TotalInvoiceValue);

				goodsItem.BY_MonetaryValue = 1.2;
				goodsItem2.BY_MonetaryValue = 2.3;
				AssertEquals("populated", 3.5m, departureHeader.TotalInvoiceValue);
			});
		}

		public void TestTotalNetMassInKilograms_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			CombineAssertions(() =>
			{
				AssertEquals("not populated", 0m, departureHeader.TotalNettMassInKilograms);

				var goodsItem = departureHeader.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_NetWeight = 1.2m;
				goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				var goodsItem2 = departureHeader.MovementHeader.GoodsItems.AddNew();
				goodsItem2.BY_NetWeight = 2.3m;
				goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				AssertEquals("populated with Kilograms", 3.5m, departureHeader.TotalNettMassInKilograms);

				goodsItem2.BY_NetWeight = 2923m;
				goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Grams;
				AssertEquals("populated with Grams and Kilograms", 4.123m, departureHeader.TotalNettMassInKilograms);
			});
		}

		public void TestTotalNetMassInKilograms_Phase5()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			var goodsItem = departureHeader.Bills.AddNew().GoodsItems.AddNew();
			var goodsItem2 = departureHeader.Bills.AddNew().GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("not populated", 0m, departureHeader.TotalNettMassInKilograms);

				goodsItem.BY_NetWeight = 1.2;
				goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItem2.BY_NetWeight = 2.3;
				goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				AssertEquals("populated with Kilograms", 3.5m, departureHeader.TotalNettMassInKilograms);

				goodsItem2.BY_NetWeight = 2923;
				goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Grams;
				AssertEquals("populated with Grams and Kilograms", 4.123m, departureHeader.TotalNettMassInKilograms);
			});
		}

		public void TestBH_RL_NKImportLoadPortReadOnly_Departure()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			AssertEquals(false, departureHeader.BH_RL_NKImportLoadPortInfo.ReadOnly);

			var goodItem = departureHeader.MovementHeader.GoodsItems.AddNew();
			AssertEquals(false, departureHeader.BH_RL_NKImportLoadPortInfo.ReadOnly);

			goodItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
			AssertEquals(true, departureHeader.BH_RL_NKImportLoadPortInfo.ReadOnly);

			departureHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Taiwan;
			AssertEquals(false, departureHeader.BH_RL_NKImportLoadPortInfo.ReadOnly);
		}

		public void TestGetEDocsProviderSupporter()
		{
			var lineEDocProvider = ((IEDocsProvider)GetNewBusinessObject()).GetEDocsProviderSupporter();
			AssertNotNull(lineEDocProvider);
			AssertType<JobInvoicingEDocsProviderSupporter>(lineEDocProvider);
		}

		public void TestDocumentSupporter()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			AssertNotNull(departureHeader.DocumentSupporter);
			AssertType<NctsHeaderDocumentSupporter>(departureHeader.DocumentSupporter);

			var documentSupporter = departureHeader.DocumentSupporter;
			AssertSame($"{nameof(NctsHeader.DocumentSupporter)} must be cached", documentSupporter, departureHeader.DocumentSupporter);
		}

		public void TestPrincipal_IsPersistentWhenIsPluggedIn()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_ParentID = shipment.PK;
			departureHeader.BH_ParentTableCode = shipment.TablePrefix;
			var principal = departureHeader.Principal;
			AssertEquals("Principal JobDocAddress IsPersistent", true, principal.IsPersistent);
		}

		public void TestConsignor_IsPersistentWhenIsPluggedIn()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_ParentID = shipment.PK;
			departureHeader.BH_ParentTableCode = shipment.TablePrefix;
			var consignor = departureHeader.Consignor;
			AssertEquals("Consignor JobDocAddress IsPersistent", true, consignor.IsPersistent);
		}

		public void TestConsignee_IsPersistentWhenIsPluggedIn()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_ParentID = shipment.PK;
			departureHeader.BH_ParentTableCode = shipment.TablePrefix;
			var consignee = departureHeader.Consignee;
			AssertEquals("Consignee JobDocAddress IsPersistent", true, consignee.IsPersistent);
		}

		public void TestDepartureMovementHeader_Delete()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				var departureMovementHeader = departureHeader.MovementHeader;
				AssertType<NctsDepartureMovementHeader>("Create", departureMovementHeader);
				departureMovementHeader.Delete();
				AssertEquals("Create new one", false, Equals(departureMovementHeader, departureHeader.MovementHeader));
			});
		}

		public void TestDepartureMovementHeader_NotDepartureHeaderType()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertNull(departureHeader.MovementHeader);
		}

		public void TestMakeDepartureAmendment()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var departureMovement = departureHeader.MovementHeader;
			departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			departureHeader.LocalReferenceNumber = "NCTIA00000001";
			departureHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.Ok;

			CombineAssertions(() =>
			{
				AssertEquals("Amendment not allowed", "Amendment is not allowed currently.", departureHeader.MakeDepartureAmendment());

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
				AssertEquals("Amendment allowed", "LRN updated and status reset. Please save, close and reopen this declaration to amend and retransmit.", departureHeader.MakeDepartureAmendment());
				AssertEquals("LocalReferenceNumber after amendment", "NCTIA00000001/1", departureHeader.LocalReferenceNumber);
				AssertEquals("BM_CustomsStatus after amendment", NctsTransitStatusList.Codes.Unknown, departureMovement.BM_CustomsStatus);
				AssertEquals("EffectiveMessageStatus after amendment", NctsMessageStatusList.Codes.DepartureDeclarationNotSent, departureHeader.EffectiveMessageStatus);
			});
		}

		public void TestLinkedShipmentCustomsEntryNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_ParentID = shipment.PK;
			departureHeader.BH_ParentTableCode = shipment.TablePrefix;
			NCTSTestHelper.SetMrnForTest(departureHeader, "MRN11111111");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CusEntryNumbers Count", 1, shipment.CusEntryNumbersForAllCountries.Count);
				AssertEquals("CusEntryNumber NCTS MRN", "MRN11111111", shipment.CusEntryNumbersForAllCountries[0].CE_EntryNum);
			});
		}

		public void TestIsDepartureAmendmentAllowed()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var departureMovement = departureHeader.MovementHeader;
			var amendmentAllowedStatuses = new HashSet<string>
			{
				NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid,
				NctsTransitStatusList.Codes.GoodsNotReleasedForTransit,
				NctsTransitStatusList.Codes.DeclarationCancelled
			};

			CombineAssertions(() =>
			{
				AssertEquals("BM_CustomsStatus empty", false, departureHeader.IsDepartureAmendmentAllowed);
				foreach (var status in departureMovement.Lookups.NctsTransitStatusList.GetAllCodes())
				{
					departureMovement.BM_CustomsStatus = status;
					AssertEquals($"BM_CustomsStatus '{status}'", amendmentAllowedStatuses.Contains(status), departureHeader.IsDepartureAmendmentAllowed);
				}
			});
		}

		public void TestClone_Departure()
		{
			var departure = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4);
			var clone = (NctsHeader)departure.TemplateCopy();
			AssertCloneResult_Common(clone, NctsMovementType.Codes.Departure);
			AssertCloneResult_Departure_Common_Phase4(clone, NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4);
			AssertEquals("MDN", clone.EffectiveMessageStatus);
		}

		public void TestCloneForDepartureAndArrivalHeader()
		{
			var departure = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.DepartureAndArrival);
			// Adding arrival movement to test it is not copied
			departure.ArrivalMovementHeader.BM_AdditionalText = "ABC";
			departure.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			departure.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;

			var dsaOffice = departure.CustomsOffices.AddNew();
			dsaOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			dsaOffice.CY_Data = "DSA Office";

			AssertEquals("Pre-assertion - Should be two movement headers (1 departure, 1 arrival", 2, departure.MovementHeaders.Count);

			var clone = (NctsHeader)departure.TemplateCopy();
			AssertEquals("Testing headerType", NctsMovementType.Codes.Departure, clone.BH_HeaderType);
			AssertEquals("Should only be one movement header (Departure)", 1, clone.MovementHeaders.Count);
			AssertEquals("EffectiveMessageStatus should be 'MDN'", NctsMessageStatusList.Codes.DepartureDeclarationNotSent, clone.EffectiveMessageStatus);
			AssertEquals("BM_CustomsStatus should be ''", ZString.Empty, clone.MovementHeader.BM_CustomsStatus);
			AssertEquals(3, clone.CustomsOffices.Count);
			Assert(!clone.CustomsOffices.Cast<NctsEuOfficeCode>().Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival));
		}

		public void TestClonedNctsHeaderLinkedToShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var departure = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4);
			departure.BH_ParentTableCode = "JS";
			departure.BH_ParentID = shipment.PK;
			var clone = (NctsHeader)departure.TemplateCopy();
			AssertEquals("Clone not linked to Shipment", ZGuid.Empty, clone.BH_ParentID);
			AssertEquals("Clone not linked to Shipment", ZString.Empty, clone.BH_ParentTableCode);
			AssertCloneResult_Common(clone, NctsMovementType.Codes.Departure);
			AssertCloneResult_Departure_Common_Phase4(clone, NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4);
		}

		public void TestIAccIntegrationDataProvider()
		{
			var departureHeader = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4);
			departureHeader.BH_JobReference = "NCT0001";
			var dataProvider = (IAccIntegrationDataProvider)departureHeader.GetJobDeclarationIAccIntegrationDataProvider(false);

			CombineAssertions(() =>
			{
				AssertEquals("Action = none because there are no charges to automatically post", ChargePosterBehaviours.None, dataProvider.Action);
				AssertEquals("AutoPostingEmailRecipient", ZGuid.Empty, dataProvider.AutoPostingEmailRecipient);
				AssertEquals("BusinessObjectPK", departureHeader.PK, dataProvider.BusinessObjectPK);
				AssertEquals("Company", departureHeader.Company, dataProvider.Company);
				AssertEquals("No DisbursementChargeCodes", 0, dataProvider.DisbursementChargeCodes.Length);
				AssertEquals("InvDataProviders", 0, dataProvider.InvDataProviders.Length);
				AssertContains("JobType", "NCTS Movement Header", dataProvider.JobType);
				AssertEquals("ReferenceID", "NCT0001", dataProvider.ReferenceID);
			});
		}

		public void TestNctsInvoicingSupporter()
		{
			var departureHeader = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4);
			departureHeader.BH_JobReference = "NCT0001";
			departureHeader.MovementHeader.BM_ExportTransportMode = "1"; // sea
			var invSupp = new NctsInvoicingSupporter(departureHeader);
			CombineAssertions(() =>
			{
				AssertEquals("Dept", ZGuid.Empty, invSupp.OverriddenDepartmentPK);
				AssertEquals("Serice", "STD", invSupp.ServiceLevel);
				AssertEquals("Cont mode", "CNT", invSupp.ContainerMode);
				AssertEquals("Cne", Factory.Load<OrgAddress>(cnePk).Header, invSupp.Consignee);
				AssertEquals("Cnr", Factory.Load<OrgAddress>(cnrPk).Header, invSupp.Consignor);
				AssertEquals("Consumer", JobInvoicingConsumerTypes.CustomsTransitNCTS, invSupp.ConsumerType);
				AssertEquals("ATA", ZDateTime.Empty, invSupp.ATA);
				AssertEquals("ATD", ZDateTime.Empty, invSupp.ATD);
				AssertEquals("ETA", ZDateTime.Empty, invSupp.ETA);
				AssertEquals("ETD", ZDateTime.Empty, invSupp.ETD);
				AssertEquals("Charge wt", 50.875733m, invSupp.ActualChargeable);
				AssertEquals("Actual wt", 50.875733m, invSupp.ActualWeight);
				AssertEquals("Charge wt units", "KG", invSupp.ActualChargeableUnit);
				AssertEquals("Actual wt units", "KG", invSupp.ActualWeightUnit);
				AssertEquals("Auto create when not saved", true, invSupp.CreateAccountingJobOnSavingOfOperationsJob);
				AssertSame("Destination", departureHeader.PortUnlading, invSupp.Destination);
				AssertEquals("Service flux", "EXP", invSupp.ServiceDirection);
				AssertEquals("Mode of transport", "SEA", invSupp.TransportMode);
				AssertEquals("Consol type", Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, invSupp.ConsolType);
				AssertEquals("Container count", 3, invSupp.ContainerCount);
				AssertEquals("Is import`", false, invSupp.IsImport);
				AssertEquals("Is export", false, invSupp.IsExport);
				AssertEquals("Is domestic", false, invSupp.IsDomestic);
				AssertSame("Origin", departureHeader.ImportLoadPort, invSupp.Origin);

				// House and master once attached ot shipment/conols:
				var shipment = Factory.New<ForwardingShipment>();
				var consol = Factory.New<ForwardingConsol>();
				shipment.JS_HouseBill = "HAWB1";
				consol.JK_MasterBillNum = "MASTER2";
				consol.Shipments.Add(shipment);
				departureHeader.BH_ParentID = shipment.PK;
				departureHeader.BH_ParentTableCode = shipment.TablePrefix;
				departureHeader.MovementHeader.BM_RL_NKDestinationPort = departureHeader.BH_RL_NKImportLoadPort;  // domestic
				invSupp = new NctsInvoicingSupporter(departureHeader);
				AssertEquals("Auto create when plugged in", false, invSupp.CreateAccountingJobOnSavingOfOperationsJob);
				AssertEquals("Consol type", Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, invSupp.ConsolType);
				AssertEquals("Is domestic", true, invSupp.IsDomestic);

				departureHeader.BH_ParentID = consol.PK;
				departureHeader.BH_ParentTableCode = consol.TablePrefix;
				invSupp = new NctsInvoicingSupporter(departureHeader);

				AssertEquals("Auto create when plugged in", false, invSupp.CreateAccountingJobOnSavingOfOperationsJob);
				AssertEquals("Consol type", Core.Constants.JobInvoicingDefaultDepartmentConsolType.All, invSupp.ConsolType);
			});
		}

		public void TestApportionedAmountToGuaranteesLiabilityAmount_RoundPhase5()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Latvia);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-2);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(2);

			Factory.Save();
			NCTSTestHelper.SetUpTariff(Factory);

			var header = Factory.New<NctsHeaderPhase5ForTest>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			var bill1 = header.Bills.AddNew();

			var goodsItem1 = Factory.New<NctsDepartureCargoDescForTest>();
			goodsItem1.BY_ParentTableCode = bill1.TablePrefix;
			goodsItem1.BY_ParentID = bill1.PK;
			goodsItem1.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem1.BY_ZZF_NKTaxType = ZString.Empty;
			goodsItem1.BY_MonetaryValue = 1.9;
			goodsItem1.ExciseAmountForTesting = 200.001m;

			var movementHeader = header.MovementHeader;
			movementHeader.Guarantees.RemoveAndDeleteAll();
			var guarantee1 = movementHeader.Guarantees.AddNew() as NctsGuaranteeForTest;
			guarantee1.PW_BondNumber = "19860101";
			guarantee1.PW_Override = false;

			header.ApportionedAmountToGuaranteesLiabilityAmount();

			CombineAssertions(() =>
			{
				AssertEquals("VatAmount", 40.43m, goodsItem1.VatAmount);
				AssertEquals("DutyAmount", 0.23m, goodsItem1.DutyAmount);
				AssertEquals("ExciseAmount", 200.00m, goodsItem1.ExciseAmount);

				AssertEquals("PW_BondAmount RoundToCurrency VatAmount (40.43) + DutyAmount(0.23) + ExciseAmount (200.00)", 240.66m, guarantee1.PW_BondAmount);

				guarantee1.PW_Override = false;
				guarantee1.SetupApportionmentType(GuaranteeApportionmentType.EqualShare);

				var guarantee2 = movementHeader.Guarantees.AddNew() as NctsGuaranteeForTest;
				guarantee2.PW_BondNumber = "19860102";
				guarantee2.PW_Override = false;
				guarantee2.SetupApportionmentType(GuaranteeApportionmentType.EqualShare);

				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Guarantee1 PW_BondAmount when total in items is equal to total guarantees", 120.33m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee2 PW_BondAmount when total in items is equal to total guarantees", 120.33m, guarantee2.PW_BondAmount);

				guarantee1.PW_Override = false;
				guarantee2.PW_Override = false;
				goodsItem1.BY_MonetaryValue = 1.888;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Total in items when total in items is less to total guarantees", 240.65m, Utilities.Round(goodsItem1.VatAmount, 2) + Utilities.Round(goodsItem1.DutyAmount, 2) + Utilities.Round(goodsItem1.ExciseAmount, 2));
				AssertEquals("Guarantee1 PW_BondAmount when total in items is less to total guarantees", 120.32m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee2 PW_BondAmount when total in items is less to total guarantees", 120.33m, guarantee2.PW_BondAmount);

				guarantee1.PW_Override = false;
				guarantee2.PW_Override = false;
				var guarantee3 = movementHeader.Guarantees.AddNew() as NctsGuaranteeForTest;
				guarantee3.PW_BondNumber = "19860103";
				guarantee3.PW_Override = false;
				guarantee3.SetupApportionmentType(GuaranteeApportionmentType.EqualShare);
				goodsItem1.BY_MonetaryValue = 293.40;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Total in items when total in items exceeds to total guarantees", 340.93m, Utilities.Round(goodsItem1.VatAmount, 2) + Utilities.Round(goodsItem1.DutyAmount, 2) + Utilities.Round(goodsItem1.ExciseAmount, 2));
				AssertEquals("Guarantee1 PW_BondAmount when total in items exceeds to total guarantees", 113.65m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee2 PW_BondAmount when total in items exceeds to total guarantees", 113.64m, guarantee2.PW_BondAmount);
				AssertEquals("Guarantee3 PW_BondAmount when total in items exceeds to total guarantees", 113.64m, guarantee3.PW_BondAmount);

				guarantee1.PW_Override = true;
				guarantee1.PW_BondAmount = 100.00m;
				guarantee2.PW_Override = false;
				guarantee3.PW_Override = false;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Guarantee1 PW_BondAmount when PW_Override true (one)", 100.00m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee2 PW_BondAmount when PW_Override false (one)", 120.46m, guarantee2.PW_BondAmount);
				AssertEquals("Guarantee3 PW_BondAmount when PW_Override false (one)", 120.47m, guarantee3.PW_BondAmount);

				guarantee1.PW_Override = true;
				guarantee1.PW_BondAmount = 100.00m;
				guarantee2.PW_Override = false;
				guarantee3.PW_Override = true;
				guarantee3.PW_BondAmount = 150.00m;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Guarantee1 PW_BondAmount when PW_Override true (two)", 100.00m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee2 PW_BondAmount when PW_Override false (two)", 90.93m, guarantee2.PW_BondAmount);
				AssertEquals("Guarantee3 PW_BondAmount when PW_Override true (two)", 150.00m, guarantee3.PW_BondAmount);

				guarantee1.PW_Override = false;
				guarantee2.PW_Override = false;
				guarantee3.PW_Override = false;
				guarantee1.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.HAL;
				guarantee2.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.HAL;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Guarantee1 PW_BondAmount when SuretyCode is HAL in Guarantee1 and Guarantee2", 56.83m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee2 PW_BondAmount when SuretyCode is HAL in Guarantee1 and Guarantee2", 56.82m, guarantee2.PW_BondAmount);
				AssertEquals("Guarantee3 PW_BondAmount when SuretyCode is HAL in Guarantee1 and Guarantee2", 113.64m, guarantee3.PW_BondAmount);

				guarantee1.PW_Override = true;
				guarantee1.PW_BondAmount = 100.00m;
				guarantee2.PW_Override = false;
				guarantee3.PW_Override = false;
				guarantee2.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.HAL;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Guarantee1 PW_BondAmount when PW_Override true (Guarantee2 HAL)", 100.00m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee2 PW_BondAmount when PW_Override false (Guarantee2 HAL)", 60.23m, guarantee2.PW_BondAmount);
				AssertEquals("Guarantee3 PW_BondAmount when PW_Override false (Guarantee2 HAL)", 120.47m, guarantee3.PW_BondAmount);

				guarantee3.Delete();
				guarantee1.PW_Override = true;
				guarantee1.PW_BondAmount = 500.00m;
				guarantee2.PW_Override = true;
				guarantee2.PW_BondAmount = 300.00m;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Guarantee1 two guarantee with PW_Override true", 500.00m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee1 two guarantee with PW_Override true", 300.00m, guarantee2.PW_BondAmount);

				guarantee2.Delete();
				guarantee1.PW_Override = true;
				guarantee1.PW_BondAmount = 500.00m;
				header.ApportionedAmountToGuaranteesLiabilityAmount();
				AssertEquals("Guarantee1 only one guarantee with PW_Override true", 500.00m, guarantee1.PW_BondAmount);
			});
		}

		public void TestApportionedAmountToGuaranteesLiabilityAmount()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Latvia);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;

			var guaranteeLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = "Ref";
			guaranteeLineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			guaranteeLineTransaction.CPL_TranValue = 150m;
			guaranteeLineTransaction.CPL_Reference = "XJ5 - 00003877";
			Factory.Save();
			NCTSTestHelper.SetUpTariff(Factory);

			var header = Factory.New<NctsHeaderForTest>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			var goodsItem1 = Factory.New<NctsDepartureCargoDescForTest>();
			goodsItem1.BY_ParentTableCode = header.MovementHeader.TablePrefix;
			goodsItem1.BY_ParentID = header.MovementHeader.PK;
			goodsItem1.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem1.BY_ZZF_NKTaxType = ZString.Empty;

			var goodsItem2 = header.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem2.BY_ZZF_NKTaxType = ZString.Empty;

			var guarantee1 = header.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 10m;
			guarantee1.PW_BondNumber = "19860101";
			guarantee1.SetupApportionmentType(GuaranteeApportionmentType.ConsumeAll);

			var guarantee2 = header.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 0m;
			guarantee2.SetupApportionmentType(GuaranteeApportionmentType.ConsumeAll);

			var guarantee3 = header.Guarantees.AddNew();
			guarantee3.PW_BondAmount = 0m;
			guarantee3.SetupApportionmentType(GuaranteeApportionmentType.EqualShare);

			header.ApportionedAmountToGuaranteesLiabilityAmount();
			AssertEquals(10m, guarantee1.PW_BondAmount);
			AssertEquals(0m, guarantee2.PW_BondAmount);
			AssertEquals(0m, guarantee3.PW_BondAmount);

			header.Guarantees.Cast<NctsGuaranteeForTest>().ForEach(c => c.SetupApportionmentType(GuaranteeApportionmentType.EqualShare));
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals(10m, guarantee1.PW_BondAmount);
			AssertEquals(0m, guarantee2.PW_BondAmount);
			AssertEquals(0m, guarantee3.PW_BondAmount);

			goodsItem1.BY_MonetaryValue = 1_000m;
			goodsItem2.BY_MonetaryValue = 501m;
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals(129.08m, guarantee1.PW_BondAmount);
			AssertEquals(129.09m, guarantee2.PW_BondAmount);
			AssertEquals(129.09m, guarantee3.PW_BondAmount);

			goodsItem1.ExciseAmountForTesting = 200.001m;
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals(189.08m, guarantee1.PW_BondAmount);
			AssertEquals(189.09m, guarantee2.PW_BondAmount);
			AssertEquals(189.09m, guarantee3.PW_BondAmount);

			goodsItem2.BY_MonetaryValue = 505m;
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals(189.43m, guarantee1.PW_BondAmount);
			AssertEquals(189.43m, guarantee2.PW_BondAmount);
			AssertEquals(189.43m, guarantee3.PW_BondAmount);

			header.Guarantees.Cast<NctsGuaranteeForTest>().ForEach(c => c.SetupApportionmentType(GuaranteeApportionmentType.None));
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals(757.720m, guarantee1.PW_BondAmount);
			AssertEquals(757.720m, guarantee2.PW_BondAmount);
			AssertEquals(757.720m, guarantee3.PW_BondAmount);

			header.Guarantees.Cast<NctsGuaranteeForTest>().ForEach(c => c.SetupApportionmentType(GuaranteeApportionmentType.ConsumeAll));
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals(150m, guarantee1.PW_BondAmount);
			AssertEquals(757.720m, guarantee2.PW_BondAmount);
			AssertEquals(757.720m, guarantee3.PW_BondAmount);

			header.Guarantees.Cast<NctsGuaranteeForTest>().ForEach(c => c.SetupApportionmentType(GuaranteeApportionmentType.Voucher));
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals(10000m, guarantee1.PW_BondAmount);
			AssertEquals(10000m, guarantee2.PW_BondAmount);
			AssertEquals(10000m, guarantee3.PW_BondAmount);
		}

		public void TestHasSecurityAtGoodsItemLevelSecurityConsignor_Phase4()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("No security at header or goods item level", false, departureHeader.IsSecurityDeclaration);
				departureHeader.BH_FTZMove = true;
				var line = departureHeader.MovementHeader.GoodsItems.AddNew();
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", line.SecurityConsignor);
				AssertEquals("Has security at line level", true, departureHeader.HasSecurityAtGoodsItemLevel);
			});
		}

		public void TestHasSecurityAtGoodsItemLevelSecurityConsignee_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_FTZMove = true;
			var line = departureHeader.MovementHeader.GoodsItems.AddNew();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CON", line.SecurityConsignee);
			AssertEquals("Has security at line level", true, departureHeader.HasSecurityAtGoodsItemLevel);
		}

		public void TestHasSecurityAtHeaderLevelSecurityConsignor_Phase4()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				departureHeader.BH_FTZMove = true;
				AssertEquals("No security at header or goods item level", false, departureHeader.IsSecurityDeclaration);
				var line = departureHeader.MovementHeader.GoodsItems.AddNew();
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", line.SecurityConsignor);
				AssertEquals("Added security at goods item level", false, departureHeader.HasSecurityAtHeaderLevel);
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", departureHeader.SecurityConsignor);
				AssertEquals("Added security at header level", true, departureHeader.HasSecurityAtHeaderLevel);
			});
		}

		public void TestHasSecurityAtHeaderLevelSecurityConsignee_Phase4()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				departureHeader.BH_FTZMove = true;
				var line = departureHeader.MovementHeader.GoodsItems.AddNew();
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CON", line.SecurityConsignee);
				AssertEquals("Added security at goods item level", false, departureHeader.HasSecurityAtHeaderLevel);
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CON", departureHeader.SecurityConsignee);
				AssertEquals("Added security at header level", true, departureHeader.HasSecurityAtHeaderLevel);
			});
		}

		public void TestIsSecurityDeclaration_Phase4()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				departureHeader.BH_FTZMove = true;
				var line = departureHeader.MovementHeader.GoodsItems.AddNew();
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CON", line.SecurityConsignee);
				AssertEquals("Goods item level security", true, departureHeader.IsSecurityDeclaration);
				line.Delete();
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", departureHeader.SecurityConsignor);
				AssertEquals("Header level security", true, departureHeader.IsSecurityDeclaration);
			});
		}

		public void TestIsSecurityDeclaration_Phase5()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase5DepartureHeader();
				var bill1 = departureHeader.Bills.AddNew();
				var bill2 = departureHeader.Bills.AddNew();

				var departureMovement = departureHeader.MovementHeader;
				departureMovement.BM_TypeOfSecurity = "ENT";
				AssertEquals("BM_TypeOfSecurity=ENT", true, departureHeader.IsSecurityDeclaration);
				departureMovement.BM_TypeOfSecurity = "BTH";
				AssertEquals("BM_TypeOfSecurity=BTH", true, departureHeader.IsSecurityDeclaration);
				departureMovement.BM_TypeOfSecurity = "EXI";
				AssertEquals("BM_TypeOfSecurity=EXI", true, departureHeader.IsSecurityDeclaration);
				departureMovement.BM_TypeOfSecurity = "NON";
				AssertEquals("BM_TypeOfSecurity=NON", false, departureHeader.IsSecurityDeclaration);
			});
		}

		public void TestIsDepartureTabReadOnly_MessageStatus()
		{
			var readOnlyMessageStatuses = new HashSet<string>
			{
				NctsMessageStatusList.Codes.DepartureDeclarationSent,
				NctsMessageStatusList.Codes.CancellationRequestSent,
				NctsMessageStatusList.Codes.MessageQueued
			};

			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				foreach (var status in departureHeader.Lookups.NctsMessageStatusList.GetAllCodes())
				{
					departureHeader.EffectiveMessageStatus = status;
					AssertEquals($"'{status}'", readOnlyMessageStatuses.Contains(status), departureHeader.IsDepartureTabReadOnly);
				}
			});
		}

		public void TestIsDepartureTabReadOnly_DepartureStatus()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var departureMovement = departureHeader.MovementHeader;
			var readOnlyDepartureStatuses = new HashSet<string>
			{
				NctsTransitStatusList.Codes.DeclarationAccepted,
				NctsTransitStatusList.Codes.DeclarationMrnAllocated,
				NctsTransitStatusList.Codes.GoodsUnderCustomsControl,
				NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid,
				NctsTransitStatusList.Codes.GoodsNotReleasedForTransit,
				NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture,
				NctsTransitStatusList.Codes.DeclarationCancelled
			};

			CombineAssertions(() =>
			{
				foreach (var status in departureMovement.Lookups.NctsTransitStatusList.GetAllCodes())
				{
					departureMovement.BM_CustomsStatus = status;
					AssertEquals($"'{status}'", readOnlyDepartureStatuses.Contains(status), departureHeader.IsDepartureTabReadOnly);
				}
			});
		}

		public void TestNctsGuaranteePINAndPWBondTypeareSetWhenGuaranteeIsSelected()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009 Desc");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, Core.Constants.CountryCodes.Latvia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "adresse ok";
			address.OA_OH = org.PK;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.MainAccessCode = "";
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "adresse ok";

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Number = "19860102";
			guaranteeHeader2.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader2.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader2.CPH_SubType = "3";
			guaranteeHeader2.MainAccessCode = "test";
			rule = guaranteeHeader2.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "adresse ko";

			var guaranteeHeader3 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader3.CPH_Number = "19860103";
			guaranteeHeader3.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader3.CPH_Type = "COM";
			guaranteeHeader3.MainAccessCode = "test";
			rule = guaranteeHeader3.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "adresse ko";
			Factory.Save();

			var header = Factory.New<NctsHeaderForTest>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals(0, header.Guarantees.Count);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			var guarantee = header.Guarantees.AddNew();

			guarantee.PW_BondNumber = "19860103";
			AssertEquals("", guarantee.PW_BondType);
			AssertEquals("test", guarantee.PW_Password);

			guarantee.PW_BondNumber = "19860102";
			AssertEquals("3", guarantee.PW_BondType);
			AssertEquals("test", guarantee.PW_Password);

			guarantee.PW_BondNumber = "19860101";
			AssertEquals("3", guarantee.PW_BondType);
			AssertEquals("", guarantee.PW_Password);
		}

		public void TestPackageCount_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var goodsItem1 = departureHeader.MovementHeader.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 30;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 20;
			var goodsItem2 = departureHeader.MovementHeader.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;
			AssertEquals(100, ((IAllowPermitProcessing)departureHeader).PackageCount);
		}

		public void TestPackageCount_Phase5()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			var goodsItem1 = departureHeader.Bills.AddNew().GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 30;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 20;
			var goodsItem2 = departureHeader.Bills.AddNew().GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;
			AssertEquals(100, ((IAllowPermitProcessing)departureHeader).PackageCount);
		}

		public void TestPlaceOfUnloadingCode_Departure()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.PlaceOfUnloadingCode = "ITBGO";
			CombineAssertions(() =>
			{
				AssertEquals("PlaceOfUnloadingCode", "ITBGO", departureHeader.PlaceOfUnloadingCode);
				AssertEquals("MovementHeader.BM_PlaceOfUnloading", "ITBGO", departureHeader.MovementHeader.BM_PlaceOfUnloading);
			});
		}

		public void TestPlaceOfUnloading()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.PlaceOfUnloadingCode = "ITBGO";
			AssertEquals("PlaceOfUnloading", "Bergamo", departureHeader.PlaceOfUnloading);
		}

		public void TestPresentationCustomsOffice()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("No DestinationCustomsOfficeCode", ZString.Empty, departureHeader.PresentationCustomsOffice);
				NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "ZZ123456", ZDateTime.Empty);
				AssertEquals("Has DestinationCustomsOfficeCode", "ZZ123456", departureHeader.PresentationCustomsOffice);
			});
		}

		public void TestPrincipal()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departureHeader.Principal, "1", traderTir: "GBR/022/1234567");
			NCTSTestHelper.AssertJobDocAddress(departureHeader.Principal, DocAddressType.Principal, "1", traderTir: "GBR/022/1234567");
		}

		public void TestConsignor()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CO1", departureHeader.Consignor, "2");
			NCTSTestHelper.AssertJobDocAddress(departureHeader.Consignor, DocAddressType.ConsignorDocumentaryAddress, "2");
		}

		public void TestConsignee()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", departureHeader.Consignee, "3");
			NCTSTestHelper.AssertJobDocAddress(departureHeader.Consignee, DocAddressType.ConsigneeAddress, "3");
		}

		public void TestSecurityConsignor()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", departureHeader.SecurityConsignor, "4");
			NCTSTestHelper.AssertJobDocAddress(departureHeader.SecurityConsignor, DocAddressType.NotifyParty2, "4");
		}

		public void TestSecurityConsignee()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "TSC", departureHeader.SecurityConsignee, "5");
			NCTSTestHelper.AssertJobDocAddress(departureHeader.SecurityConsignee, DocAddressType.NotifyParty3, "5");
		}

		public void TestGuarantees_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			NCTSTestHelper.SetupGuaranteesForTest(departureHeader);
			CombineAssertions(() =>
			{
				var result = departureHeader.Guarantees;
				AssertEquals("count", 2, result.Count);
				AssertEquals("Parent", departureHeader.PK, result[0].PW_ParentID);
			});
		}

		public void TestGuarantees_Phase5()
		{
			var departureHeader = Factory.New<NctsHeaderPhase5ForTest>();
			departureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = departureHeader.MovementHeader;

			movementHeader.Guarantees.AddNew();
			movementHeader.Guarantees.AddNew();
			CombineAssertions(() =>
			{
				var result = movementHeader.Guarantees;
				AssertEquals("count", 2, result.Count);
				Assert("Parent", result.All(x => x.PW_ParentID == movementHeader.PK));
			});
		}

		public void TestLocalReferenceNumberReadOnly_Departure()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("Departure Header", false, departureHeader.LocalReferenceNumberReadOnly);
				var mrn = CusEntryNumber.LoadOrCreate(departureHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
				mrn.CE_EntryNum = "MRN123";
				AssertEquals("Departure Header, has MRN", true, departureHeader.LocalReferenceNumberReadOnly);
			});
		}

		public void TestLocalReferenceNumber_Phase5Departure()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.BH_JobReference = "NCT000001";

			CombineAssertions(() =>
			{
				AssertEquals("No LRN filled, LocalReferenceNumber should be filled with JobReference", "NCT000001", departureHeader.LocalReferenceNumber);
				departureHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";
				AssertEquals("LRN filled, LocalReferenceNumber should be retrieved from BM_PaperlessInbondNum", "LRN123", departureHeader.LocalReferenceNumber);
				departureHeader.LocalReferenceNumber = "LRN321";
				AssertEquals("LocalReferenceNumber should be saved to BM_PaperlessInbondNum", "LRN321", departureHeader.MovementHeader.BM_PaperlessInbondNum);
			});
		}

		public void TestLocalReferenceNumber_Phase4Departure()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.BH_JobReference = "NCT000001";

			CombineAssertions(() =>
			{
				AssertEquals("No LRN filled, LocalReferenceNumber should be filled with JobReference", "NCT000001", departureHeader.LocalReferenceNumber);
				departureHeader.SetSystemDefinedValue(nameof(NctsHeader.Schema.LocalReferenceNumber), (ZString)"LRN123");
				AssertEquals("LRN filled, LocalReferenceNumber should be retrieved from BM_PaperlessInbondNum", "LRN123", departureHeader.LocalReferenceNumber);
				departureHeader.LocalReferenceNumber = "LRN321";
				AssertEquals("LocalReferenceNumber should be saved to BM_PaperlessInbondNum", "LRN321", departureHeader.GetSystemDefinedValue<ZString>(nameof(NctsHeader.Schema.LocalReferenceNumber)));
			});
		}

		public void TestDepartureCustomsOfficeCode()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("No DepartureCustomsOffice", ZString.Empty, departureHeader.DepartureCustomsOfficeCode);
				NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "AA123456", ZDateTime.Empty, true);
				AssertEquals("Has DepartureCustomsOffice", "AA123456", departureHeader.DepartureCustomsOfficeCode);
			});
		}

		public void TestTransitCustomsOfficeCodeList()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("No TransitCustomsOffices", 0, departureHeader.TransitCustomsOfficeCodeList.Count);
				NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
				NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));
				AssertEquals("Added 2 TransitCustomsOffices", 2, departureHeader.TransitCustomsOfficeCodeList.Count);
			});
		}

		public void TestDestinationCustomsOfficeCode_Departure()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("No DestinationCustomsOfficeCode", ZString.Empty, departureHeader.DestinationCustomsOfficeCode);
				departureHeader.CustomsOffices.Load();
				var desOffice = departureHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				desOffice.CY_Data = "ZZ123456";
				AssertEquals("Has DestinationCustomsOfficeCode", "ZZ123456", departureHeader.DestinationCustomsOfficeCode);
			});
		}

		public void TestEnquiryCustomsOfficeCode()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("No EnquiryCustomsOfficeCode", ZString.Empty, departureHeader.EnquiryCustomsOfficeCode);
				NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry, "ZZ123456", ZDateTime.Empty);
				AssertEquals("Has EnquiryCustomsOfficeCode", "ZZ123456", departureHeader.EnquiryCustomsOfficeCode);
			});
		}

		public void TestEnquiryCustomsOfficeCodeCountry()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("No EnquiryCustomsOfficeCode", ZString.Empty, departureHeader.EnquiryCustomsOfficeCodeCountry);
				NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry, "ZZ123456", ZDateTime.Empty);
				AssertEquals("Has EnquiryCustomsOfficeCode", "ZZ", departureHeader.EnquiryCustomsOfficeCodeCountry);
			});
		}

		public void TestTargetNctsSystemCountryCode_ForXIOfficeCode()
		{
			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "XI000001", ZDateTime.Empty);
				AssertEquals("XI TargetNctsSystemCountryCode", Core.Constants.CountryCodes.UnitedKingdom, departureHeader.TargetNctsSystemCountryCode);
			});
		}

		public void TestLocalReferenceNumberUsingCustomisation()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.TransportMode].Include = true;
			using (CustomsDataRegistry.Instance.NctsLocalReferenceNumberCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customisation))
			{
				var departureHeader = CreatePhase4DepartureHeader();
				departureHeader.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
				departureHeader.MovementHeader.BM_RL_NKForeignDestPort = "GBDVR";

				departureHeader.OnSaving();
				AssertEquals("LRN NCT-I-A-00000001 , I=Import A=Air", "NCTIA00000001", departureHeader.LocalReferenceNumber);
			}
		}

		public void TestDefaultMessageStatus_Departure()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			AssertEquals(NctsMessageStatusList.Codes.DepartureDeclarationNotSent, departureHeader.EffectiveMessageStatus);
		}

		public void TestDestinationCustomsOfficeCodeReadOnly_Departure()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			AssertEquals("Departure Header", true, departureHeader.DestinationCustomsOfficeCodeInfo.ReadOnly);
			AssertEquals("Departure Header", true, departureHeader.DestinationCustomsOfficeCodeForDepartureInfo.ReadOnly);
		}

		public void TestSetAndUnsetPrincipalSetsAndfDeletesGuarantees()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departureHeader.Principal);
			var orgAddress = departureHeader.Principal.E2_OA_Address;
			AssertEquals(0, departureHeader.Guarantees.Count);
			var wrapper = OrgHeaderWrapper.New(departureHeader.Principal.Organisation);
			var bond1 = wrapper.BondDetails.AddNew();
			bond1.PW_BondType = "1";
			bond1.PW_BondNumber = "ONE";
			departureHeader.Principal.E2_OA_Address = ZGuid.Empty;
			AssertEquals(0, departureHeader.Guarantees.Count);
			departureHeader.Principal.E2_OA_Address = orgAddress;
			AssertEquals(0, departureHeader.Guarantees.Count);

			var adHocGuaranteeTwo = departureHeader.Guarantees.AddNew();
			adHocGuaranteeTwo.PW_BondType = "2";
			adHocGuaranteeTwo.PW_BondNumber = "TWO";
			AssertEquals("TWO", departureHeader.Guarantees[0].PW_BondNumber);

			var bond2 = wrapper.BondDetails.AddNew();
			bond2.PW_BondType = "2";
			bond2.PW_BondNumber = "TWO";
			departureHeader.Principal.E2_OA_Address = ZGuid.Empty;
			AssertEquals(1, departureHeader.Guarantees.Count);
			departureHeader.Principal.E2_OA_Address = orgAddress;
			AssertEquals(1, departureHeader.Guarantees.Count);
			AssertEquals("TWO", departureHeader.Guarantees[0].PW_BondNumber);
			departureHeader.Principal.E2_OA_Address = ZGuid.Empty;
			AssertEquals(1, departureHeader.Guarantees.Count);
			departureHeader.Principal.E2_OA_Address = orgAddress;
			AssertEquals(1, departureHeader.Guarantees.Count);
		}

		public void TestSetConsignee()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "FRPAR";

			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("BM_RL_NKDestinationPort, no consignee address", ZString.Empty, departureHeader.MovementHeader.BM_RL_NKDestinationPort);

				departureHeader.Consignee.E2_OA_Address = address.PK;
				AssertEquals("BM_RL_NKDestinationPort, has consignee address", Core.Constants.CountryCodes.France, departureHeader.MovementHeader.BM_RL_NKDestinationPort);

				departureHeader.Consignee.E2_AddressOverride = true;
				departureHeader.Consignee.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
				AssertEquals("BM_RL_NKDestinationPort, consignee address override", Core.Constants.CountryCodes.Netherlands, departureHeader.MovementHeader.BM_RL_NKDestinationPort);
			});
		}

		public void TestSetConsignor()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "FRPAR";

			CombineAssertions(() =>
			{
				var departureHeader = CreatePhase4DepartureHeader();
				AssertEquals("BH_RL_NKImportLoadPort, no consignor address", ZString.Empty, departureHeader.BH_RL_NKImportLoadPort);

				departureHeader.Consignor.E2_OA_Address = address.PK;
				AssertEquals("BM_RL_NKDestinationPort, has consignor address", Core.Constants.CountryCodes.France, departureHeader.BH_RL_NKImportLoadPort);

				departureHeader.Consignor.E2_AddressOverride = true;
				departureHeader.Consignor.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
				AssertEquals("BM_RL_NKDestinationPort, consignor address override", Core.Constants.CountryCodes.Netherlands, departureHeader.BH_RL_NKImportLoadPort);
			});
		}

		public void TestBH_RL_NKPortOfDispatchWhenFullLoadPortSupportOn()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "FRPAR";

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsConfigurationMock = new Mock<NctsConfiguration>();
			nctsConfigurationMock
				.Protected()
				.Setup<ZBool>("FullLoadPortSupportCore")
				.Returns(true);
			header.Factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock
				.Setup(m => m.GetObject())
				.Returns(nctsConfigurationMock.Object);

			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object } };
			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				CombineAssertions(() =>
				{
					AssertEquals("BH_RL_NKImportLoadPort, no consignor address", ZString.Empty, header.BH_RL_NKImportLoadPort);
					AssertEquals("BH_RL_NKPortOfDispatch, no consignor address", ZString.Empty, header.PortOfDispatch);

					header.Consignor.E2_OA_Address = address.PK;
					AssertEquals("BH_RL_NKImportLoadPort, has consignor address", address.OA_RL_NKRelatedPortCode, header.BH_RL_NKImportLoadPort);
					AssertEquals("BH_RL_NKPortOfDispatch, has consignor address", address.OA_RL_NKRelatedPortCode, header.PortOfDispatch);
				});
			}
			nctsConfigurationMock.VerifyAll();
		}

		public void TestConsignorLinkToIsNctsSimplifiedNctsProcedure()
		{
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var cne = Factory.NewWithValidTestData<OrgHeader>();

			var cnrPk = cnr.MainAddress.PK;
			var cnePk = cne.MainAddress.PK;

			var org = Factory.New<OrgHeader>();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();

			var address2 = org.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "FRNIC";

			var departureHeader = CreatePhase4DepartureHeader();
			var consignor = departureHeader.Consignor;
			var consignee = departureHeader.Consignee;

			consignor.OrganisationPK = cnr.PK;
			consignee.OrganisationPK = cne.PK;

			authorizationHeader.CPH_OH_PermitHolder = cnr.PK;
			authorizationHeader.CPH_OA_AppliesTo = cnrPk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

			consignor.E2_OA_Address = cnePk;
			consignor.E2_OA_Address = cnrPk;
			Assert(!departureHeader.MovementHeader.IsSimplifiedNctsProcedure);

			authorizationHeader.CPH_OA_AppliesTo = cnePk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			consignor.E2_OA_Address = cnePk;
			consignor.E2_OA_Address = cnrPk;
			Assert(!departureHeader.MovementHeader.IsSimplifiedNctsProcedure);

			authorizationHeader.CPH_OH_PermitHolder = cne.PK;
			authorizationHeader.CPH_OA_AppliesTo = cnrPk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			consignor.E2_OA_Address = cnePk;
			consignor.E2_OA_Address = cnrPk;
			Assert(!departureHeader.MovementHeader.IsSimplifiedNctsProcedure);

			authorizationHeader.CPH_OH_PermitHolder = cnr.PK;
			authorizationHeader.CPH_OA_AppliesTo = cnrPk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			consignor.E2_OA_Address = cnePk;
			consignor.E2_OA_Address = cnrPk;
			Assert(departureHeader.MovementHeader.IsSimplifiedNctsProcedure);
		}

		public void TestApportionedAmountToGuaranteesWithEqualShare()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Latvia);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(2);

			var guaranteeLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = "Ref";
			guaranteeLineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			guaranteeLineTransaction.CPL_TranValue = 150m;
			guaranteeLineTransaction.CPL_Reference = "XJ5 - 00003877";
			Factory.Save();
			NCTSTestHelper.SetUpTariff(Factory);

			var header = Factory.New<NctsHeaderForTest>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			var goodsItem1 = header.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem1.BY_ZZF_NKTaxType = ZString.Empty;

			var goodsItem2 = header.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem2.BY_ZZF_NKTaxType = ZString.Empty;

			var guarantee1 = header.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 100m;
			guarantee1.PW_BondNumber = "19860101";

			var guarantee2 = header.Guarantees.AddNew();
			guarantee2.PW_BondAmount = 0m;

			var guarantee3 = header.Guarantees.AddNew();
			guarantee3.PW_BondAmount = 0m;

			header.Guarantees.Cast<NctsGuaranteeForTest>().ForEach(c => c.SetupApportionmentType(GuaranteeApportionmentType.EqualShare));
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			goodsItem1.BY_MonetaryValue = 100m;
			goodsItem2.BY_MonetaryValue = 50m;
			header.ApportionedAmountToGuaranteesLiabilityAmount();

			CombineAssertions(() =>
			{
				AssertEquals("Guarantee 1 rounding of apportioned amount to guarantees does not match", 12.90m, guarantee1.PW_BondAmount);
				AssertEquals("Guarantee 2 rounding of apportioned amount to guarantees does not match", 12.90m, guarantee2.PW_BondAmount);
				AssertEquals("Guarantee 3 rounding of apportioned amount to guarantees does not match", 12.90m, guarantee3.PW_BondAmount);
			});
		}

		public void TestMakeArrivalNotificationFromDeparture()
		{
			var departureHeader = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4);
			departureHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			departureHeader.MakeArrivalNotificationFromDeparture();
			CombineAssertions(() =>
			{
				AssertEquals("BH_HeaderType", NctsMovementType.Codes.DepartureAndArrival, departureHeader.BH_HeaderType);
				AssertEquals("MovementHeader.BM_CustomsStatus", NctsTransitStatusList.Codes.Unknown, departureHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("EffectiveMessageStatus", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, departureHeader.EffectiveMessageStatus);
				AssertEquals("Departure Consignee should match Arival Destination Trader", departureHeader.Consignee.OrganisationPK, departureHeader.DestinationTrader.OrganisationPK);
			});
		}

		public void TestHasTransitOffice()
		{
			var departure = CreateTestDeparture(CusInBondApplicationCodeList.Codes.NCTS4);
			departure.CustomsOffices.RemoveAndDeleteAll();

			AssertCollectionNotContains("[PRE-CONDITION] Any TRA Customs Office", "TRA", departure.CustomsOffices.Cast<EuOfficeCode>().Select(x => x.CY_Code));
			AssertEquals("When any TRA Customs Office has been added in CustomsOffices, HasTransitOffice", false, departure.HasTransitOffice());

			departure.CustomsOffices.AddNew("TRA", "EU123456");
			AssertEquals("HasTransitOffice", true, departure.HasTransitOffice());

			AssertEquals("HasTransitOffice(GB)", false, departure.HasTransitOffice("GB"));
			AssertEquals("HasTransitOffice(EU)", true, departure.HasTransitOffice("EU"));
		}

		public void TestPopulateGuaranteeFromPrincipal()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "IE address";
			address.OA_OH = org.PK;

			var cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var transaction = cusGuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			cusGuaranteeHeader.CPH_OH_PermitHolder = org.PK;
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(2);
			transaction.FillWithValidTestData();

			var rule = cusGuaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "LandRover";

			var cusGuaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var transaction2 = cusGuaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
			cusGuaranteeHeader2.CPH_Number = "96-G-7191";
			cusGuaranteeHeader2.CPH_OH_PermitHolder = org.PK;
			cusGuaranteeHeader2.CPH_EndDate = ZDate.Today.AddMonths(2);
			transaction2.FillWithValidTestData();

			var rule2 = cusGuaranteeHeader2.CusGuaranteeRules.AddNew();
			rule2.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule2.CPR_ValueFrom = "Toyota";

			Factory.Save();

			CombineAssertions(() =>
			{
				var header2 = Factory.New<NctsHeader>();
				org.MainAddress.AddressCode = "landrover";
				header2.SetMovementType(NctsMovementType.Codes.Departure);
				header2.Principal.E2_OA_Address = org.MainAddress.PK;
				AssertEquals("Guarantee found when lowercase AddressCode", 1, header2.MovementHeader.Guarantees.Count);

				var header3 = Factory.New<NctsHeader>();
				org.MainAddress.AddressCode = "LANDROVER";
				header3.SetMovementType(NctsMovementType.Codes.Departure);
				header3.Principal.E2_OA_Address = org.MainAddress.PK;
				AssertEquals("Guarantee found when uppercase AddressCode", 1, header3.MovementHeader.Guarantees.Count);
			});
		}

		public void TestTotalNumberOfPackages_Phase5()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			var bill1 = departureHeader.Bills.AddNew();
			var bill2 = departureHeader.Bills.AddNew();

			var goodsItem1 = bill1.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 20;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 30;
			var goodsItem2 = bill1.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;

			var bulkType = Factory.SetupBulkCusCode();
			var goodsItem3 = bill1.GoodsItems.AddNew();
			var package4 = goodsItem3.Packages.AddNew();
			package4.B5_UnitCount = 0;
			package4.B5_UnitType = bulkType;

			var goodsItem4 = bill1.GoodsItems.AddNew();
			var package5 = goodsItem4.Packages.AddNew();
			package5.B5_UnitCount = 10;
			package5.B5_UnitType = bulkType;

			var goodsItem5 = bill2.GoodsItems.AddNew();
			var package6 = goodsItem5.Packages.AddNew();
			package6.B5_UnitCount = 60;

			AssertEquals(162, departureHeader.TotalNumberOfPackages);
		}

		public void TestTotalNumberOfPackages_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var goodsItem1 = departureHeader.MovementHeader.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 20;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 30;

			AssertEquals(50, departureHeader.TotalNumberOfPackages);
		}

		public void TestTotalNumberOfItems_Phase5()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			var bill1 = departureHeader.Bills.AddNew();
			var bill2 = departureHeader.Bills.AddNew();
			bill1.GoodsItems.AddNew();
			bill1.GoodsItems.AddNew();
			bill2.GoodsItems.AddNew();

			AssertEquals(3, departureHeader.TotalNumberOfItems);
		}

		public void TestTotalNumberOfItems_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			departureHeader.MovementHeader.GoodsItems.AddNew();
			departureHeader.MovementHeader.GoodsItems.AddNew();

			AssertEquals(2, departureHeader.TotalNumberOfItems);
		}

		public void TestTotalGrossMassInKilograms_Phase5()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			departureHeader.MovementHeader.BM_GrossWeight = 999;

			AssertEquals(999m, departureHeader.TotalGrossMassInKilograms);
		}

		public void TestTotalGrossMassInKilograms_Phase4()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var goodsItem1 = departureHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 30;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			var goodsItem2 = departureHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 25;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

			AssertEquals(55m, departureHeader.TotalGrossMassInKilograms);
		}

		public void TestIsConditionR0520_UserShouldNotSaveAmendments()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departureHeader.Principal, suffix: "", traderTin: "123456789012");
			departureHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			Factory.Save();

			var movementHeader = departureHeader.MovementHeader;

			// one field of the OR group with changes
			var newOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			departureHeader.Principal.OrganisationPK = newOrgPK;
			Assert("Principal.HasChanges", departureHeader.Principal.HasChanges);

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));
				Assert("Rule disabled", !IsCondition());
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				movementHeader.BM_CustomsStatus = string.Empty;
				Assert("BM_CustomsStatus empty", !IsCondition());
				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;

				departureHeader.BH_ApplicationCode = "XYZ";
				Assert("Not NCTS5", !IsCondition());
				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				departureHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				Assert("Not Departure", !IsCondition());
				departureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

				Assert(departureHeader.Principal.HasChanges);
				Assert("Principal.HasChanges!", IsCondition());
				Factory.Save();

				var dep = departureHeader.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
				dep.CY_Data = "ABC";
				Assert(dep.CY_DataInfo.HasChanges);
				Assert("header.CustomsOfficesForDeparture.CY_DataInfo.HasChanges!", IsCondition());
				Factory.Save();
				dep.CY_Code = "DEF";
				Assert(dep.CY_CodeInfo.HasChanges);
				Assert("header.CustomsOfficesForDeparture.CY_CodeInfo.HasChanges!", IsCondition());
				Factory.Save();

				movementHeader.BM_InBondEntryType = "X";
				Assert(movementHeader.BM_InBondEntryTypeInfo.HasChanges);
				Assert("movementHeader.BM_InBondEntryTypeInfo.HasChanges!", IsCondition());
				Factory.Save();

				movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				Assert(movementHeader.BM_AdditionalDeclarationTypeInfo.HasChanges);
				Assert("MovementHeader.BM_AdditionalDeclarationTypeInfo.HasChanges!", IsCondition());
				Factory.Save();

				newOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				movementHeader.Representative.OrganisationPK = newOrgPK;
				Assert(movementHeader.Representative.HasChanges);
				Assert("movementHeader.Representative.HasChanges!", IsCondition());
				Factory.Save();
			}

			bool IsCondition()
			{
				return departureHeader.IsConditionR0520_UserShouldNotSaveAmendments;
			}
		}

		public void TestIsPhase5Departure()
		{
			var departureHeader = CreatePhase5DepartureHeader();
			AssertEquals("Phase 5 Departure => true", true, departureHeader.IsPhase5Departure);
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Phase 4 Departure => false", false, departureHeader.IsPhase5Departure);
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("Phase 5 Arrivals => false", false, departureHeader.IsPhase5Departure);
		}

		public void TestHeaderContainersSeals()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var container1 = departureHeader.DepartureHeaderContainers.AddNew();
			var container2 = departureHeader.DepartureHeaderContainers.AddNew();

			container1.Seal1 = "123";
			container1.Seal2 = "456";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "789";
			container2.Seal1 = "ABC";
			container2.Seal2 = "LMN";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "XYZ";

			AssertContainsExactElementsInAnyOrder(new string[] { "123", "456", "789", "ABC", "LMN", "XYZ" }, departureHeader.HeaderContainersSeals);
		}

		public void TestArrivalHeaderContainers()
		{
			var departureHeader = CreatePhase4DepartureHeader();
			var container = departureHeader.DepartureHeaderContainers.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("HeaderContainers should contain 1 record", 1, departureHeader.DepartureHeaderContainers.Count);
				AssertEquals("ArrivalHeaderContainers should not contain any records for departure", 0, departureHeader.ArrivalHeaderContainers.Count);
			});
		}

		public void TestBusinessObjectsWithRelatedEvents_Departure()
		{
			AssertCollectionContains(header.BusinessObjectsWithRelatedEvents, bo => bo == header.MovementHeader);
		}

		public void TestSynchronizeMonetaryValue_ShouldUpdateMonetaryValues_WhenExchangeRateChanged()
		{
			// Arrange
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();
			var goodsItem = bills.GoodsItems.AddNew();

			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate = NCTSTestHelper.CreateExchangeRate(Factory, 1.5, Core.Constants.CurrencyCodes.UnitedKingdom);
			Factory.Save();

			goodsItem.BY_LinePrice = 15;
			AssertEquals("Precondition", 10m, goodsItem.BY_MonetaryValue);

			exchangeRate.RE_SellRate = 1;

			// Act
			nctsHeader.SynchronizeMonetaryValue();

			//Assert
			AssertEquals("MonetaryValue is recalculated with new exchange rate", 15m, goodsItem.BY_MonetaryValue);
		}

		public void TestSynchronizeMonetaryValue_ShouldNotUpdateMonetaryValues_WhenBM_ValuationDateNotEmpty()
		{
			// Arrange
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();
			var goodsItem = bills.GoodsItems.AddNew();

			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate = NCTSTestHelper.CreateExchangeRate(Factory, 1.5, Core.Constants.CurrencyCodes.UnitedKingdom);
			Factory.Save();

			goodsItem.BY_LinePrice = 15;
			AssertEquals("Precondition", 10m, goodsItem.BY_MonetaryValue);
			nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.Now;

			exchangeRate.RE_SellRate = 1;

			// Act
			nctsHeader.SynchronizeMonetaryValue();

			//Assert
			AssertEquals("MonetaryValue is same", 10m, goodsItem.BY_MonetaryValue);
		}

		public void TestSynchronizeMonetaryValue_ShouldMarkGuaranteesAsDirty_WhenExchangeRateChanged()
		{
			// Arrange
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();
			var goodsItem = bills.GoodsItems.AddNew();

			var guaranteeOverride = nctsHeader.MovementHeader.Guarantees.AddNew();
			guaranteeOverride.PW_Override = true;
			var guaranteeNotOverride = nctsHeader.MovementHeader.Guarantees.AddNew();

			goodsItem.BY_RX_NKLinePriceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			goodsItem.Bill.Header.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var exchangeRate = NCTSTestHelper.CreateExchangeRate(Factory, 1.5, Core.Constants.CurrencyCodes.UnitedKingdom);
			Factory.Save();

			goodsItem.BY_LinePrice = 15;
			AssertEquals("Precondition", 10m, goodsItem.BY_MonetaryValue);

			exchangeRate.RE_SellRate = 1;

			// Act
			nctsHeader.SynchronizeMonetaryValue();
			AssertEquals("Precondition", 15m, goodsItem.BY_MonetaryValue);

			// Assert
			AssertEquals("Guarantee having Override is set dirty", NctsGuarantee.DirtyStatus, guaranteeOverride.PW_Status);
			AssertEquals("Guarantee not having Override isn't set dirty", string.Empty, guaranteeNotOverride.PW_Status);
		}

		protected override BusinessObject GetNewBusinessObject() => header;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			header.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
			return header;
		}
	}
}
