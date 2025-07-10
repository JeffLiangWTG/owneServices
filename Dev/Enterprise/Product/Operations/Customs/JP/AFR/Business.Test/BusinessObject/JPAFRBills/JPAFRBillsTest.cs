using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRBills))]
	sealed class JPAFRBillsTest : EnterpriseBusinessObjectTestCase, IInBondDetailInitiator
	{
		public void TestBLLFunctionInfo()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill.PK;
			bllFunctionInfo.B7_ParentTableCode = bill.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			Factory.Save();

			AssertNotNull(bill.BLLFunctionInfo);
		}

		public void TestJPB_SpecialCargoCode()
		{
			PrepareRefCusCodeList();

			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			bill.JPB_SpecialCargoCode = "PLQ";
			AssertEquals(bill.SpecialCargoCode.ZZD_Code, bill.JPB_SpecialCargoCode);
		}

		public void TestParentWorkflowProviders()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();
			var prov = ((IWorkflowTriggerEventSource)bill).ParentWorkflowProviders;
			AssertNotNull(bill.Header);
			AssertNotNull(prov);
			AssertEquals(1, prov.Count);
		}

		void PrepareRefCusCodeList()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			TestCaseHelper.ClearTable(RefCusCodeListAttribute.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeType.Schema.TableName);

			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "Japan Special Cargo Code");
			var jp1dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "PLQ", startDate, endDate);
			jp1dr.ZZD_Description = "Plant Protection Act ";
			var jp2dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "ARM", startDate, endDate);
			jp2dr.ZZD_Description = "firearms and swords Control Law";
			var jp3dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AVI", startDate, endDate);
			jp3dr.ZZD_Description = "a domestic animal infectious disease prophylaxis";
			var jp4dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "NRC", startDate, endDate);
			jp4dr.ZZD_Description = "drugs and psychotropic drugs Control Law";
			Factory.Save();
		}

		public void TestUniversalDataContext()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFR0015654";
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "MB3243";
			IDataContextManager manager = null;
			AssertNoExceptionThrown(() => { manager = bill.GetUniversalDataContextManager(); });
			AssertNotNull("JPAFRBills should have [UniversalDataContext(DataContextType.AFRBill)] attribute", manager);
			AssertEquals(DataContextType.AFRBill, manager.DataContextType);
			AssertEquals("", manager.DataContextKey);
		}

		public void TestInBondDetails()
		{
			var header = Factory.New<JPAFRHeader>();
			using (var initiator = new InBondDetailInitiatorTestHelper())
			{
				header.InBondDetailInitiator = initiator;
				var bill = header.Bills.AddNew();
				AssertNull(bill.InBondDetails);
				// InBondDetails should not be created when data is empty
				bill.JPB_Calc_ArrivalBondedAreaCode = ZString.Empty;
				AssertNull(bill.InBondDetails);
				bill.JPB_Calc_EFDT = ZDateTime.Empty;
				AssertNull(bill.InBondDetails);
				bill.JPB_Calc_ESDT = ZDateTime.Empty;
				AssertNull(bill.InBondDetails);
				bill.JPB_Calc_GoodsValue = ZDecimal.Zero;
				AssertNull(bill.InBondDetails);
				bill.JPB_Calc_RX_NKGoodsValueCurrency = ZString.Empty;
				AssertNull(bill.InBondDetails);
				bill.JPB_Calc_TemporaryLandingDuration = ZInt.Zero;
				AssertNull(bill.InBondDetails);
				bill.JPB_Calc_TemporaryLandingReason = ZString.Empty;
				AssertNull(bill.InBondDetails);
				bill.JPB_Calc_TransportMode = ZString.Empty;
				AssertNull(bill.InBondDetails);
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				var billInOtherFactory = factory2.Load<JPAFRBills>(bill.PK);
				billInOtherFactory.InBondDetailInitiator = new InBondDetailInitiatorTestHelper();
				billInOtherFactory.JPB_Calc_TransportMode = TransportModeList.Codes.Aircraft;
				var inBondInOtherFactory = billInOtherFactory.InBondDetails;
				AssertNotNull(inBondInOtherFactory);

				bill.JPB_Calc_TemporaryLandingReason = "1";
				AssertNull(bill.InBondDetails);
				AssertEquals(ZString.Empty, bill.JPB_Calc_TemporaryLandingReason);
				AssertEquals("Someone else is already in the process of creating InBond Details for this AFR.\r\nYou should be able to access the InBond Details when the person has saved the record. Please try later.", JPAFRBills.InBondMutexLockText);
				AssertEquals("Someone else is already in the process of creating InBond Details for this AFR.\r\nYou should be able to access the InBond Details when the person has saved the record. Please try later.", initiator.TextResult);
				initiator.TextResult = null;
				bill.JPB_Calc_ArrivalBondedAreaCode = "1";
				AssertNull(bill.InBondDetails);
				AssertEquals(ZString.Empty, bill.JPB_Calc_ArrivalBondedAreaCode);
				AssertEquals(JPAFRBills.InBondMutexLockText, initiator.TextResult);
				initiator.TextResult = null;
				bill.JPB_Calc_EFDT = ZDateTime.BrettsBirthday;
				AssertNull(bill.InBondDetails);
				AssertEquals(ZDateTime.Empty, bill.JPB_Calc_EFDT);
				AssertEquals(JPAFRBills.InBondMutexLockText, initiator.TextResult);
				initiator.TextResult = null;
				bill.JPB_Calc_ESDT = ZDateTime.BrettsBirthday.AddDays(1);
				AssertNull(bill.InBondDetails);
				AssertEquals(ZDateTime.Empty, bill.JPB_Calc_ESDT);
				AssertEquals(JPAFRBills.InBondMutexLockText, initiator.TextResult);
				initiator.TextResult = null;
				bill.JPB_Calc_GoodsValue = 10m;
				AssertNull(bill.InBondDetails);
				AssertEquals(ZDecimal.Zero, bill.JPB_Calc_GoodsValue);
				AssertEquals(JPAFRBills.InBondMutexLockText, initiator.TextResult);
				initiator.TextResult = null;
				bill.JPB_Calc_RX_NKGoodsValueCurrency = "3";
				AssertNull(bill.InBondDetails);
				AssertEquals(ZString.Empty, bill.JPB_Calc_RX_NKGoodsValueCurrency);
				AssertEquals(JPAFRBills.InBondMutexLockText, initiator.TextResult);
				initiator.TextResult = null;
				bill.JPB_Calc_TemporaryLandingDuration = 3;
				AssertNull(bill.InBondDetails);
				AssertEquals(ZInt.Zero, bill.JPB_Calc_TemporaryLandingDuration);
				AssertEquals(JPAFRBills.InBondMutexLockText, initiator.TextResult);
				initiator.TextResult = null;
				bill.JPB_Calc_TransportMode = "4";
				AssertNull(bill.InBondDetails);
				AssertEquals(ZString.Empty, bill.JPB_Calc_TransportMode);
				AssertEquals(JPAFRBills.InBondMutexLockText, initiator.TextResult);

				factory2.Save();
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals(TransportModeList.Codes.Aircraft, bill.JPB_Calc_TransportMode);

				initiator.TextResult = null;
				bill.JPB_Calc_ArrivalBondedAreaCode = "1";
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals("1", bill.JPB_Calc_ArrivalBondedAreaCode);
				AssertNull(initiator.TextResult);
				bill.JPB_Calc_EFDT = ZDateTime.BrettsBirthday;
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals(ZDateTime.BrettsBirthday, bill.JPB_Calc_EFDT);
				AssertNull(initiator.TextResult);
				bill.JPB_Calc_ESDT = ZDateTime.BrettsBirthday.AddDays(1);
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), bill.JPB_Calc_ESDT);
				AssertNull(initiator.TextResult);
				bill.JPB_Calc_GoodsValue = 10m;
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals(10m, bill.JPB_Calc_GoodsValue);
				AssertNull(initiator.TextResult);
				bill.JPB_Calc_RX_NKGoodsValueCurrency = "3";
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals("3", bill.JPB_Calc_RX_NKGoodsValueCurrency);
				AssertNull(initiator.TextResult);
				bill.JPB_Calc_TemporaryLandingDuration = 3;
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals(3, bill.JPB_Calc_TemporaryLandingDuration);
				AssertNull(initiator.TextResult);
				bill.JPB_Calc_TransportMode = "4";
				AssertEquals(inBondInOtherFactory.PK, bill.InBondDetails.PK);
				AssertEquals("4", bill.JPB_Calc_TransportMode);
				AssertNull(initiator.TextResult);

				Factory.Save();
				AssertEquals("1", inBondInOtherFactory.JPI_ArrivalBondedAreaCode);
				AssertEquals(ZDateTime.BrettsBirthday, inBondInOtherFactory.JPI_EFDT);
				AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), inBondInOtherFactory.JPI_ESDT);
				AssertEquals(10m, inBondInOtherFactory.JPI_GoodsValue);
				AssertEquals("3", inBondInOtherFactory.JPI_RX_NKGoodsValueCurrency);
				AssertEquals(3, inBondInOtherFactory.JPI_TemporaryLandingDuration);
				AssertEquals("4", inBondInOtherFactory.JPI_TransportMode);

				bill.InBondDetails.Delete();
				Factory.Save();
				Assert("InBond should be Deleted", inBondInOtherFactory.IsDeleted);
				AssertNull("InBondDetails in Bill should be null", billInOtherFactory.InBondDetails);
			}
		}

		public void TestReadOnlyFields()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			AssertEquals("bill.JPB_ReleaseStatusInfo.ReadOnly", true, bill.JPB_ReleaseStatusInfo.ReadOnly);
			AssertEquals("bill.JPB_MessageStatusInfo.ReadOnly", true, bill.JPB_MessageStatusInfo.ReadOnly);
		}

		public void TestHeader()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_ParentId = consol1.PK;
			header1.JPH_ParentTableCode = consol1.TablePrefix;
			var consol2 = Factory.New<ForwardingConsol>();
			var header2 = Factory.New<JPAFRHeader>();
			header2.JPH_ParentId = consol2.PK;
			header2.JPH_ParentTableCode = consol2.TablePrefix;
			var bill = header1.Bills.AddNew();
			AssertEquals(header1, bill.Header);

			bill.JPB_JPH_Header = header2.PK;
			AssertEquals(header2, bill.Header);
		}

		public void TestDefaultUQforWeightVolume()
		{
			var testBill = GetNewBusinessObject() as JPAFRBills;
			AssertEquals(WeightUnitCodeList.Codes.Kilogram, testBill.JPB_GrossWeightUQ);
			AssertEquals(VolumeUnitCodeList.Codes.CubicMeter, testBill.JPB_VolumeUQ);

			var testHeader = testBill.Header;
			var testBill2 = testHeader.Bills.AddNew();
			AssertEquals(WeightUnitCodeList.Codes.Kilogram, testBill2.JPB_GrossWeightUQ);
			AssertEquals(VolumeUnitCodeList.Codes.CubicMeter, testBill2.JPB_VolumeUQ);

			testBill.JPB_GrossWeightUQ = WeightUnitCodeList.Codes.MetricTon;
			testBill.JPB_VolumeUQ = VolumeUnitCodeList.Codes.BoardFootMeasureTimber;
			testBill.Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadBill = newFactory.Load<JPAFRBills>(testBill.PK);
			AssertEquals(WeightUnitCodeList.Codes.MetricTon, reloadBill.JPB_GrossWeightUQ);
			AssertEquals(VolumeUnitCodeList.Codes.BoardFootMeasureTimber, reloadBill.JPB_VolumeUQ);
		}

		#region TestDocAddresses

		public void TestDocAddresses()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var consignor = bill.Consignor;
			AssertEquals(DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			consignor.E2_AddressOverride = ZBool.True;
			AssertEquals(3, bill.DocAddresses.Count);
			var consignee = bill.Consignee;
			AssertEquals(DocAddressType.ConsigneeAddress, consignee.DocAddressType);
			consignee.E2_AddressOverride = ZBool.True;
			AssertEquals(3, bill.DocAddresses.Count);
			var notifyParty1 = bill.NotifyParty1;
			AssertEquals(DocAddressType.NotifyParty, notifyParty1.DocAddressType);
			notifyParty1.E2_AddressOverride = ZBool.True;
			AssertEquals(3, bill.DocAddresses.Count);
			var notifyParty2 = bill.NotifyParty2;
			AssertEquals(DocAddressType.NotifyParty2, notifyParty2.DocAddressType);
			notifyParty2.E2_AddressOverride = ZBool.True;
			AssertEquals(4, bill.DocAddresses.Count);
			AssertCollectionContains(consignor, bill.DocAddresses);
			AssertCollectionContains(consignee, bill.DocAddresses);
			AssertCollectionContains(notifyParty1, bill.DocAddresses);
			AssertCollectionContains(notifyParty2, bill.DocAddresses);
		}

		public void TestDocAddressesMandatory()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(3, bill.DocAddresses.Count);
				AssertNotNull(bill.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress));
				AssertNotNull(bill.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress));
				AssertNotNull(bill.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty));
				AssertNull(bill.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty2));

				bill.RunPreSaveValidation();
				AssertHasMessageErrorContaining(bill.Consignee.OrganisationPKInfo, ValidationConstants.Bill.AddressIsMandatory);
				AssertHasMessageErrorContaining(bill.Consignor.OrganisationPKInfo, ValidationConstants.Bill.AddressIsMandatory);
				AssertHasMessageErrorContaining(bill.NotifyParty1.OrganisationPKInfo, ValidationConstants.Bill.AddressIsMandatory);
				AssertNoMessageErrors(bill.NotifyParty2.OrganisationPKInfo);
				AssertNoErrors(bill.NotifyParty2.OrganisationPKInfo);
				AssertNoWarnings(bill.NotifyParty2.OrganisationPKInfo);
			});
		}

		public void TestDocAddressesWhenNotOverriden()
		{
			var testBill = Factory.New<JPAFRBills>();
			var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
			var testOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			testOrgAddress.OA_OH = testOrganization.PK;

			BillJobDocaddressValidationTestWhenNotOverriden(testBill.Consignee, testOrgAddress);
			BillJobDocaddressValidationTestWhenNotOverriden(testBill.Consignor, testOrgAddress);
			BillJobDocaddressValidationTestWhenNotOverriden(testBill.NotifyParty1, testOrgAddress);
			BillJobDocaddressValidationTestWhenNotOverriden(testBill.NotifyParty2, testOrgAddress);
		}

		public void TestDocAddressesWhenOverriden()
		{
			var testBill = Factory.New<JPAFRBills>();
			var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
			var testOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			testOrgAddress.OA_OH = testOrganization.PK;

			BillJobDocaddressValidationTestWhenOverriden(testBill.Consignee);
			BillJobDocaddressValidationTestWhenOverriden(testBill.Consignor);
			BillJobDocaddressValidationTestWhenOverriden(testBill.NotifyParty1);
			BillJobDocaddressValidationTestWhenOverriden(testBill.NotifyParty2);
		}

		#endregion

		public void TestShouldSynchronise()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.Prefix;
			var bill = header.Bills.AddNew();
			AssertEquals(true, bill.ShouldSynchronise);

			header.JPH_OverrideFreightDefaults = true;
			AssertEquals(false, bill.ShouldSynchronise);
		}

		public void TestIsBillAlreadyRegistered()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(false, bill.IsBillAlreadyRegistered);
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(true, bill.IsBillAlreadyRegistered);
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(false, bill.IsBillAlreadyRegistered);
		}

		public void TestRealeseAndMessageStatusDesc()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(ZString.Empty, bill.JPB_ReleaseStatusDescription);
			AssertEquals(ZString.Empty, bill.JPB_MessageStatusDescription);

			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillUpdate;
			AssertEquals(AFRBillCustomsStatusList.Descriptions.Registered, bill.JPB_ReleaseStatusDescription);
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillUpdate, bill.JPB_MessageStatusDescription);
		}

		public void TestIsShippingLineEntry()
		{
			var testHeader = Factory.New<JPAFRHeader>();
			var testBill = testHeader.Bills.AddNew();
			Assert(!testBill.IsShippingLineEntry);
			testHeader.JPH_IsShippingLineEntry = true;
			Assert(testBill.IsShippingLineEntry);

			var testBill2 = Factory.New<JPAFRBills>();
			Assert(!testBill2.IsShippingLineEntry);
		}

		public void TestDischagePortCode()
		{
			var testHeader = Factory.New<JPAFRHeader>();
			var testBill = testHeader.Bills.AddNew();
			Assert(testBill.DischargePortCode.IsEmpty);
			testHeader.JPH_RL_NKDischarge = "JPABA";
			AssertEquals("JPABA", testBill.DischargePortCode);

			var testBill2 = Factory.New<JPAFRBills>();
			Assert(testBill2.DischargePortCode.IsEmpty);
		}

		public void TestICanDeleteMembers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			header.JPH_ParentId = consol.PK;
			header.Synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			var bill = header.Bills[0];

			Assert("Bill should not be able to be deleted initially", !(bill as ICanDelete).CanDelete);
			AssertEquals("Bill Of Lading values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'.", (bill as ICanDelete).ReasonForNotAbleToDelete);

			header.JPH_OverrideFreightDefaults = true;
			Assert("Bill should not be able to be deleted initially", (bill as ICanDelete).CanDelete);

			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Assert("Bill shouldn't be able to be deleted when it's registered", !(bill as ICanDelete).CanDelete);
			AssertEquals("This Bill Of Lading is already registered with Customs.\r\nYou need to delete it from Customs file by sending Delete AFR amendment before deleting it here. (AFR > Amendment Manifest > Send with action ‘Delete’)", (bill as ICanDelete).ReasonForNotAbleToDelete);

			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Assert("Bill shouldn't be able to be deleted when awainting message response", !(bill as ICanDelete).CanDelete);
			AssertEquals("This Bill Of Lading is awaiting message response from Customs.\r\nPlease wait till the message response arrives and try again.", (bill as ICanDelete).ReasonForNotAbleToDelete);
		}

		public void TestPopulateTemporaryLandingDurationIfPossible()
		{
			var header = Factory.New<JPAFRHeader>();
			using (var initiator = new InBondDetailInitiatorTestHelper())
			{
				header.InBondDetailInitiator = initiator;
				var bill = header.Bills.AddNew();

				AssertEquals(0, bill.JPB_Calc_TemporaryLandingDuration);
				bill.JPB_Calc_ESDT = ZDateTime.Now;
				AssertEquals(0, bill.JPB_Calc_TemporaryLandingDuration);
				bill.JPB_Calc_EFDT = ZDateTime.Now;
				AssertEquals(1, bill.JPB_Calc_TemporaryLandingDuration);
				bill.JPB_Calc_ESDT = ZDateTime.Empty;
				AssertEquals(1, bill.JPB_Calc_TemporaryLandingDuration);

				bill = header.Bills.AddNew();
				bill.JPB_Calc_ESDT = ZDateTime.Now;
				AssertEquals(0, bill.JPB_Calc_TemporaryLandingDuration);
				bill.JPB_Calc_EFDT = ZDateTime.Now.AddDays(-2);
				AssertEquals(0, bill.JPB_Calc_TemporaryLandingDuration);
			}
		}

		public void TestJPB_Calc_GeneralCustomsTransitApprovalNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			using (var initiator = new InBondDetailInitiatorTestHelper())
			{
				header.InBondDetailInitiator = initiator;
				var bill = header.Bills.AddNew();

				AssertEquals("PRE: Empty GTN Number", string.Empty, bill.JPB_Calc_GeneralCustomsTransitApprovalNumber);
				AssertNull("PRE: No InBondDetails", Factory.LoadTop1<JPAFRInBondDetails>(new ZQuery(JPAFRInBondDetailsSchema.JPI_JPB_Bill, bill.PK)));

				AssertExceptionThrown<MaxLengthExceededException>("The maximum length of 'JPB_Calc_GeneralCustomsTransitApprovalNumber' has been exceeded. The maximum length of this property is 11 characters, but 20 were entered. New value: TEST1TEST2TEST3TEST4.", () => { bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TEST1TEST2TEST3TEST4 \t"; });
				AssertEquals("TST: Empty GTN Number", string.Empty, bill.JPB_Calc_GeneralCustomsTransitApprovalNumber);
				AssertNull("TST: No InBondDetails", Factory.LoadTop1<JPAFRInBondDetails>(new ZQuery(JPAFRInBondDetailsSchema.JPI_JPB_Bill, bill.PK)));

				bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TEST1TEST2 \t ";
				AssertEquals("TST: Empty GTN Number", "TEST1TEST2", bill.JPB_Calc_GeneralCustomsTransitApprovalNumber);
				AssertEquals("TST: new InBondDetails created", 1, Factory.Load<JPAFRInBondDetails>(new ZQuery(JPAFRInBondDetailsSchema.JPI_JPB_Bill, bill.PK)).Length);
			}
			CargoWise.Common.ErrorReporter.Clear();
		}

		public void TestOnFactorySavingBeforeTransactionCore()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();
			AssertEquals("MessageStatusChange Log Count", bill.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).Count(), 0);
			bill.JPB_MessageStatus = "AHR";
			Factory.Save();
			var log = bill.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).FirstOrDefault();
			AssertNotNull(log);
			AssertEquals("Reference", "AHR - Awaiting House Bill Registration", log.SL_Reference);

			bill = header.Bills.AddNew();
			Factory.Save();
			AssertEquals("StatusChange Log Count", bill.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count(), 0);
			bill.JPB_ReleaseStatus = "REG";
			Factory.Save();
			log = bill.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).FirstOrDefault();
			AssertNotNull(log);
			AssertEquals("Reference", "Status Change: -> REG", log.SL_Reference);
		}

		public void TestDelete()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();

			var pivot = bill.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_UNNO = "UN10";
			pivot.DP_Variant = "V";
			pivot.DP_Standard = "IMO";
			pivot.DP_IsDefault = true;

			var undg = DGSubstanceTestHelper.Create("1234", "a", "IMO");

			var extraUNDG = bill.UNDGs.AddNew();
			extraUNDG.DI_DG = undg.PK;

			Factory.Save();

			AssertEquals(false, pivot.IsDeleted);
			AssertEquals(1, bill.UNDGSubstancePivotCollection.Count);

			AssertEquals(false, extraUNDG.IsDeleted);
			AssertEquals(1, bill.UNDGs.Count);

			bill.Delete();

			AssertEquals(true, pivot.IsDeleted);
			AssertEquals(0, bill.UNDGSubstancePivotCollection.Count);

			AssertEquals(true, extraUNDG.IsDeleted);
			AssertEquals(0, bill.UNDGs.Count);
		}

		public void TestUNDGsMaxCount()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();

			var validator = ((ISupportMaxCountValidation)bill.UNDGs).MaxCountValidator;
			AssertEquals("Should not add more than 4 records.", 4, validator.MaxCount);
		}

		public void TestJPB_IMOClass()
		{
			var undg = DGSubstanceTestHelper.Create("1234", "a", "IMO", (d) =>
			{
				d.DG_Class = "1";
			});

			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("Default to empty", string.Empty, bill.JPB_IMOClass);

			bill.JPB_DG = undg.PK;
			AssertEquals("1", bill.JPB_IMOClass);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			header.InBondDetailInitiator = this;
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<JPAFRHeader>();
			header.InBondDetailInitiator = this;
			return header.Bills.AddNew();
		}

		protected override void TearDown()
		{
			if (onDisposing != null)
			{
				onDisposing(this, EventArgs.Empty);
			}
			base.TearDown();
		}

		void BillJobDocaddressValidationTestWhenNotOverriden(JobDocAddress jobDocAddress, OrgAddress testOrgAddress)
		{
			CombineAssertions(() =>
			{
				testOrgAddress.OA_City = ZString.Empty;
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertHasMessageErrorContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.AddressFieldIsRequired(jobDocAddress.E2_CityInfo.HumanReadableName));

				testOrgAddress.OA_City = "1234567890123456789012345";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertNoMessageErrorContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.AddressFieldIsRequired(jobDocAddress.E2_CityInfo.HumanReadableName));
			});

			CombineAssertions(() =>
			{
				testOrgAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
				testOrgAddress.OA_RN_NKCountryCode = ZString.Empty;
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertHasMessageErrorContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.AddressFieldIsRequired(jobDocAddress.E2_RN_NKCountryCodeInfo.HumanReadableName));

				testOrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertNoMessageErrorContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.AddressFieldIsRequired(jobDocAddress.E2_RN_NKCountryCodeInfo.HumanReadableName));
			});

			CombineAssertions(() =>
			{
				testOrgAddress.OA_Address2 = "1234567890123456789012345678901234567890";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertHasWarningContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(jobDocAddress.E2_Address2Info.HumanReadableName, 35));

				testOrgAddress.OA_Address2 = "12345678901234567890123456789012345";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertNoWarningContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(jobDocAddress.E2_Address2Info.HumanReadableName, 35));
			});

			CombineAssertions(() =>
			{
				testOrgAddress.OA_PostCode = "1234567890";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertHasWarningContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(jobDocAddress.E2_PostcodeInfo.HumanReadableName, 9));

				testOrgAddress.OA_PostCode = "123456789";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertNoWarningContaining(jobDocAddress.E2_OA_AddressInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(jobDocAddress.E2_PostcodeInfo.HumanReadableName, 9));
			});

			CombineAssertions(() =>
			{
				testOrgAddress.OA_Phone = "12345678901234567890";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertHasWarningContaining(jobDocAddress.E2_ContactInfo, ValidationConstants.JobDocAddress.PhoneNumberLengthReachedMaxAllowed(jobDocAddress.E2_PhoneInfo.HumanReadableName, 14));

				testOrgAddress.OA_Phone = "12345678901234";
				jobDocAddress.E2_OA_Address = ZGuid.Empty;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertNoWarningContaining(jobDocAddress.E2_ContactInfo, ValidationConstants.JobDocAddress.PhoneNumberLengthReachedMaxAllowed(jobDocAddress.E2_PhoneInfo.HumanReadableName, 14));
			});

			CombineAssertions(() =>
			{
				var testContact = Factory.NewWithValidTestData<OrgContact>();
				testContact.OC_OH = testOrgAddress.OA_OH;
				jobDocAddress.E2_OA_Address = testOrgAddress.PK;
				AssertNoWarningContaining(jobDocAddress.E2_ContactInfo, ValidationConstants.JobDocAddress.PhoneNumberLengthReachedMaxAllowed(jobDocAddress.E2_PhoneInfo.HumanReadableName, 14));

				testContact.OC_Phone = "12345678901234567890";
				jobDocAddress.E2_Contact = ZString.Empty;
				jobDocAddress.E2_Contact = testContact.OC_ContactName;
				AssertHasWarningContaining(jobDocAddress.E2_ContactInfo, ValidationConstants.JobDocAddress.PhoneNumberLengthReachedMaxAllowed(jobDocAddress.E2_PhoneInfo.HumanReadableName, 14));

				testContact.OC_Phone = "12345678901234";
				jobDocAddress.E2_Contact = ZString.Empty;
				jobDocAddress.E2_Contact = testContact.OC_ContactName;
				AssertNoWarningContaining(jobDocAddress.E2_ContactInfo, ValidationConstants.JobDocAddress.PhoneNumberLengthReachedMaxAllowed(jobDocAddress.E2_PhoneInfo.HumanReadableName, 14));
			});
		}

		void BillJobDocaddressValidationTestWhenOverriden(JobDocAddress jobDocAddress)
		{
			jobDocAddress.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				jobDocAddress.E2_City = "something";
				jobDocAddress.E2_City = ZString.Empty;
				AssertHasMessageErrorContaining(jobDocAddress.E2_CityInfo, MandatoryValidation.YouHaveNotEntered);

				jobDocAddress.E2_City = "1234567890123456789012345";
				AssertNoMessageErrors(jobDocAddress.E2_CityInfo);
				AssertNoWarnings(jobDocAddress.E2_CityInfo);
				AssertNoErrors(jobDocAddress.E2_CityInfo);
			});

			CombineAssertions(() =>
			{
				jobDocAddress.E2_RN_NKCountryCode = "XX";
				jobDocAddress.E2_RN_NKCountryCode = ZString.Empty;
				AssertHasMessageErrorContaining(jobDocAddress.E2_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

				jobDocAddress.E2_RN_NKCountryCode = "XX";
				AssertHasErrorContaining(jobDocAddress.E2_RN_NKCountryCodeInfo, ListValidation.InvalidCodeError);

				jobDocAddress.E2_RN_NKCountryCode = "AU";
				AssertNoMessageErrors(jobDocAddress.E2_RN_NKCountryCodeInfo);
				AssertNoWarnings(jobDocAddress.E2_RN_NKCountryCodeInfo);
				AssertNoErrors(jobDocAddress.E2_RN_NKCountryCodeInfo);
			});

			CombineAssertions(() =>
			{
				jobDocAddress.E2_Address2 = "1234567890123456789012345678901234567890";
				AssertHasWarningContaining(jobDocAddress.E2_Address2Info, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(jobDocAddress.E2_Address2Info.HumanReadableName, 35));

				jobDocAddress.E2_Address2 = "12345678901234567890123456789012345";
				AssertNoMessageErrors(jobDocAddress.E2_Address2Info);
				AssertNoWarnings(jobDocAddress.E2_Address2Info);
				AssertNoErrors(jobDocAddress.E2_Address2Info);
			});

			CombineAssertions(() =>
			{
				jobDocAddress.E2_Postcode = "1234567890";
				AssertHasWarningContaining(jobDocAddress.E2_PostcodeInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(jobDocAddress.E2_PostcodeInfo.HumanReadableName, 9));

				jobDocAddress.E2_RN_NKCountryCode = "AU";
				var australia = RefCountry.LoadFromCountryCode(Factory, "AU");
				australia.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

				jobDocAddress.E2_Postcode = "123456789";
				AssertNoMessageErrors(jobDocAddress.E2_PostcodeInfo);
				AssertHasWarningContaining(jobDocAddress.E2_PostcodeInfo, "The entered postcode does not comply with the postcode format rules of the country/region");
				AssertNoErrors(jobDocAddress.E2_PostcodeInfo);

				jobDocAddress.E2_Postcode = "1234";
				AssertNoMessageErrors(jobDocAddress.E2_PostcodeInfo);
				AssertNoWarnings(jobDocAddress.E2_PostcodeInfo);
				AssertNoErrors(jobDocAddress.E2_PostcodeInfo);
			});

			CombineAssertions(() =>
			{
				jobDocAddress.E2_Phone = "12345678901234567890";
				AssertHasWarningContaining(jobDocAddress.E2_PhoneInfo, ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(jobDocAddress.E2_PhoneInfo.HumanReadableName, 14));

				jobDocAddress.E2_Phone = "12345678901234";
				AssertNoMessageErrors(jobDocAddress.E2_PhoneInfo);
				AssertNoWarnings(jobDocAddress.E2_PhoneInfo);
				AssertNoErrors(jobDocAddress.E2_PhoneInfo);
			});
		}

		#endregion

		#region IInBondDetailInitiator Members

		void IInBondDetailInitiator.NotifyUserOfAnInvalidOperation(string text)
		{
		}

		event EventHandler IInBondDetailInitiator.OnDisposing
		{
			add { onDisposing += value; }
			remove { onDisposing -= value; }
		}
		event EventHandler onDisposing;

		#endregion

	}

	class JPAFRBillImportFromSailingTest : JPAFRImportFromSailingTestBase
	{
		public void TestImportBillsFromSailing()
		{
			SetUpSailingEnvironmentForImporting();
			SetUpHeaderEnvironmentForImporting();

			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var bill_AAA = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == "SCACAAA");
			var bill_BBB = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == "SCACBBB");
			var bill_CCC = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == "SCACCCC");
			CombineAssertions("Checking for Bill AAA After Initial Importing", () =>
			{
				AssertNotNull(bill_AAA);
				AssertEquals("AUSYD", bill_AAA.JPB_RL_NKOrigin);
				AssertEquals("NZAKL", bill_AAA.JPB_RL_NKDelivery);
				AssertEquals("NZAKL", bill_AAA.JPB_RL_NKFinalDestination);
				AssertEquals(1000m, bill_AAA.JPB_GrossWeight);
				AssertEquals("KG", bill_AAA.JPB_GrossWeightUQ);
				AssertEquals(new ZDecimal(1.966), bill_AAA.JPB_Volume);
				AssertEquals("M3", bill_AAA.JPB_VolumeUQ);
				AssertEquals(11, bill_AAA.JPB_ManifestQty);
				AssertEquals("PLT", bill_AAA.JPB_ManifestUQ);
				AssertEquals("010110", bill_AAA.JPB_Tariff);
				AssertEquals("!!5%", bill_AAA.UNDGSubstance.DG_Code);
				AssertEquals(consignor.PK, bill_AAA.Consignor.OrganisationPK);
				AssertEquals(consignee.PK, bill_AAA.Consignee.OrganisationPK);
				AssertEquals(consignee.PK, bill_AAA.NotifyParty1.OrganisationPK);
				AssertEquals(notifyParty.PK, bill_AAA.NotifyParty2.OrganisationPK);
				AssertEquals("TESTMARK01TESTMARK02TESTMARK03TESTMARK04TESTMARK05TESTMARK06TESTMARK07TESTMARK08TESTMARK09TESTMARK10TESTMARK11TESTMARK12TESTMARK13TESTMARK14", bill_AAA.JPB_MarksAndNumbers);
				AssertEquals("VICGOODSDESCRIPTION", bill_AAA.JPB_GoodsDescription);
				AssertEquals(false, bill_AAA.JPB_IsMaterBill);
				AssertEquals(string.Empty, bill_AAA.JPB_ReleaseStatus);
				AssertEquals(string.Empty, bill_AAA.JPB_MessageStatus);
			});
			AssertNull("BBB is not containerised", bill_BBB);
			AssertNull("CCC is not containerised", bill_CCC);

			bill_AAA.JPB_IsMaterBill = true;
			bill_AAA.JPB_Tariff = "TESTHS";
			bill_AAA.JPB_MessageStatus = "AMR";
			bill_AAA.JPB_ReleaseStatus = "REG";
			CombineAssertions("Preperation Check for replacement", () =>
			{
				AssertEquals(true, bill_AAA.JPB_IsMaterBill);
				AssertEquals("AMR", bill_AAA.JPB_MessageStatus);
				AssertEquals("REG", bill_AAA.JPB_ReleaseStatus);
				AssertEquals("TESTHS", bill_AAA.JPB_Tariff);
			});

			var updateActions = new BillImportActionCollection(header.Bills);
			updateActions[0].IsSelected = true;
			header.ImportBillsOfLadingLinkedToTheSameSailing(updateActions);
			CombineAssertions("Checking for Bills After Replacment with some value preserved", () =>
			{
				AssertEquals(true, bill_AAA.JPB_IsMaterBill);
				AssertEquals("AMR", bill_AAA.JPB_MessageStatus);
				AssertEquals("REG", bill_AAA.JPB_ReleaseStatus);
				AssertEquals("010110", bill_AAA.JPB_Tariff);
			});
		}

		public void TestDangerousGoodsLogs()
		{
			var header = Factory.New<JPAFRHeader>();
			var subs1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var bill = header.Bills.AddNew();
			bill.JPB_DG = subs1.PK;
			Factory.Save();
			var log1 = bill.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			AssertNotNull("DangerousGoodsChanged log created", log1);

			var subs2 = Factory.NewWithValidTestData<UNDGSubstance>();
			bill.JPB_DG = subs2.PK;
			Factory.Save();
			var log2 = bill.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			AssertNotNull("DangerousGoodsChanged log created", log2);
			AssertNotEquals(log1, log2);

			bill.JPB_DG = ZGuid.Empty;
			Factory.Save();
			var log3 = bill.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			AssertNotNull("DangerousGoodsChanged log created", log3);
			AssertNotEquals(log2, log3);
		}
	}
}
