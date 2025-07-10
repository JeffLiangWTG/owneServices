using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using NUnit.Framework;
using NctsConfigurationTestHelper = Enterprise.Customs.EU.NCTS.Business.Testing.NctsConfigurationTestHelper;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeader))]
	class NctsHeaderTest : EU.NCTS.Business.Testing.NctsHeaderAbstractTest
	{
		public void TestGetCusSupportingInfoTypes_PreviousDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsHeader).GetCusSupportingInfoTypes();
			AssertEquals(typeof(CommonPreviousDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}
		public void TestMessageStatus()
		{
			var header = GetNewBusinessObject(Factory);
			AssertEquals("Message status should be blank", string.Empty, header.MovementHeader.BM_MessageStatus);
		}

		public void TestArrivalMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			AssertType<NctsArrivalMovementHeader>(nctsHeader.ArrivalMovementHeader);
		}

		public void TestDefaultApplicationCode()
		{
			AssertEquals("Default BH_ApplicationCode", "NC5", Factory.New<NctsHeader>().BH_ApplicationCode);
		}

		public void TestLookups()
		{
			var header = GetNewBusinessObject(Factory);
			AssertType<NctsHeaderLookups>(header.Lookups);
		}

		public void TestValidation()
		{
			var header = GetNewBusinessObject(Factory);
			AssertType<NctsHeaderValidation>(header.Validation);
		}

		public void TestCommonMovementHeader()
		{
			var arrHeader = Factory.New<NctsHeader>();
			arrHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertType<NctsArrivalMovementHeader>(arrHeader.CommonMovementHeader);
			var depHeader = Factory.New<NctsHeader>();
			depHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertType<NctsDepartureMovementHeader>(depHeader.CommonMovementHeader);
		}

		public void TestIMessageAttacheeMembers()
		{
			var header = GetNewBusinessObject(Factory);
			IMessageAttachee messageAttachee = header;
			AssertEquals("PK", header.PK, messageAttachee.PK);
			AssertEquals("TableName", header.TableName, messageAttachee.TableName);
			AssertEquals("TablePrefix", header.TablePrefix, messageAttachee.TablePrefix);
			var branch = header.Company.Branches.AddNew();
			header.BH_GB = branch.PK;
			AssertSame("Branch", branch, messageAttachee.Branch);
			AssertNull("CustomsAgent", messageAttachee.CustomsAgent);
			AssertSame("RelatedJob", header, messageAttachee.RelatedJob);
			AssertSame("Factory", Factory, messageAttachee.Factory);
			header.MovementHeader.BM_MessageStatus = "SNT";
			AssertEquals("LogicalStatus", "SNT", messageAttachee.LogicalStatus);
			header.MovementHeader.BM_CustomsStatus = "CLR";
			AssertEquals("EntryStatus", "CLR", messageAttachee.EntryStatus);
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
			AssertEquals("MovementReferenceNumber", "MRN123", messageAttachee.MovementReferenceNumber);
			header.Messages.AddNew();
			header.Messages.AddNew();
			AssertContainsExactElementsInAnyOrder("Messages", header.Messages, messageAttachee.Messages);
		}

		public void TestIMessageAttacheeMembers_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			IMessageAttachee messageAttachee = header;
			header.ArrivalMovementHeader.BM_CustomsStatus = "ERR";
			AssertEquals("EntryStatus", "ERR", messageAttachee.EntryStatus);
		}

		public void TestDocumentSupporter()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertNotNull(nameof(NctsHeader.DocumentSupporter), nctsHeader.DocumentSupporter);
			AssertType<NctsHeaderDocumentSupporter>($"{nameof(NctsHeader.DocumentSupporter)} type", nctsHeader.DocumentSupporter);
		}

		public void TestBills()
		{
			var header = GetNewBusinessObject(Factory);
			AssertType<NctsBillCollection<NctsBill>>(header.Bills);
		}

		public void TestIsConditionR0520_UserShouldNotSaveAmendments()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
			Factory.Save();
			var depOffice = nctsHeader.MovementHeader.CustomsOffices.Find(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).First();

			depOffice.CY_Data = "IE001";
			AssertEquals("Still can modify until we have a valid MRN.", false, nctsHeader.IsConditionR0520_UserShouldNotSaveAmendments);

			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
			Factory.Save();
			depOffice.CY_Data = "IE003";
			AssertEquals("Cannot modify when a MRN exists.", true, nctsHeader.IsConditionR0520_UserShouldNotSaveAmendments);
		}

		public void TestIsConditionR0520_UserShouldNotSaveAmendmentsToGuaranteesCore()
		{
			var nctsHeader = GetNewBusinessObject(Factory);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_InBondEntryType = "A";
			var guarantee = Factory.New<NctsGuarantee>();
			guarantee.PW_BondType = "3";
			nctsHeader.MovementHeader.Guarantees.Add(guarantee);

			var originalIE015sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var originalIE015sendingAction = new NctsMessageSendingAction(originalIE015sendingObject);
			originalIE015sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			var sender = new NctsMessageSender(originalIE015sendingAction);
			sender.Send();

			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Sent;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
			Factory.Save();

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0520Active), value: false))
			{
				Assert("UserShouldNotSaveAmendmentsToGuarantees should be false when IsRuleR0520Active disabled", !IsCondition());
			}

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleR0520Active), value: true))
			{
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				Assert("UserShouldNotSaveAmendmentsToGuarantees should be false when it's not a Departure declaration", !IsCondition());

				movementHeader.BM_CustomsStatus = string.Empty;
				Assert("UserShouldNotSaveAmendmentsToGuarantees  should be false when BM_CustomsStatus is empty", !IsCondition());

				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				movementHeader.BM_CustomsStatus = nctsHeader.MovementHeader.BM_CustomsStatus = EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				movementHeader.BM_InBondEntryType = "Z";
				nctsHeader.MovementHeader.Guarantees.AddNew();
				Assert("UserShouldNotSaveAmendmentsToGuarantees should be true when guarantees and other fields changed", IsCondition());
			}

			bool IsCondition()
			{
				return nctsHeader.IsConditionR0520_UserShouldNotSaveAmendmentsToGuarantees;
			}
		}

		public void TestGuarantees_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			CombineAssertions(() =>
			{
				nctsHeader.Guarantees.AddNew();
				var result = nctsHeader.Guarantees;
				AssertEquals("Parent", nctsHeader.PK, result[0].PW_ParentID);
				AssertType<NctsGuaranteeCollection<NctsGuarantee>>("Type", result);
			});
		}

		public void TestGetPermitReference()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertEquals(ZString.Empty, nctsHeader.GetPermitReference());
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals(ZString.Empty, nctsHeader.GetPermitReference());
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";
			AssertEquals("LRN123", nctsHeader.GetPermitReference());
		}

		public void TestGetPermitRecords()
		{
			var (cusGuaranteeHeader, nctsGuarantee) = NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory, NctsMovementType.Codes.Departure);
			var nctsHeader = nctsGuarantee.NctsHeader;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";
			AssertPermitRecords(nctsHeader, 1, "NCTS Guarantee added");

			AssertGreaterThan("[PRE-CONDITION] guarantee liability amount", nctsGuarantee.PW_BondAmount, 0);
			var unlinkedCusGuaranteeHeader = NctsTestDataProvider.CreateCusGuaranteeHeader(Factory, "1234", ZGuid.Empty);
			unlinkedCusGuaranteeHeader.AddTransaction("LRN123", "Test 1", "1234", "", -10000, 0, "PND");
			Factory.Save();
			AssertPermitRecords(nctsHeader, 2, "Transaction matches LRN");

			unlinkedCusGuaranteeHeader.AddTransaction("LRN456", "Test 1", "1234", "", -10000, 0, "PND");
			Factory.Save();
			AssertPermitRecords(nctsHeader, 2, "2nd transaction does not match LRN");
		}

		public void TestGetPermitRecordsForArrival()
		{
			var (cusGuaranteeHeader, nctsGuarantee) = NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory, NctsMovementType.Codes.Arrival);
			var nctsHeader = nctsGuarantee.NctsHeader;
			nctsHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
			AssertPermitRecords(nctsHeader, 1, "NCTS Guarantee added");

			AssertGreaterThan("[PRE-CONDITION] guarantee liability amount", nctsGuarantee.PW_BondAmount, 0);
			var unlinkedCusGuaranteeHeader = NctsTestDataProvider.CreateCusGuaranteeHeader(Factory, "1234", ZGuid.Empty);
			unlinkedCusGuaranteeHeader.AddTransaction("LRN123", "Test 1", "1234", "", -10000, 0, "PND");
			Factory.Save();
			AssertPermitRecords(nctsHeader, 1, "Transaction does not match as no LRN on arrival");
		}

		void AssertPermitRecords(NctsHeader nctsHeader, int expectedCount, string message = "")
		{
			var records = nctsHeader.GetPermitRecords();
			AssertEquals(message, expectedCount, records.Count);
		}

		public void TestBillsType()
		{
			var header = Factory.New<NctsHeader>();
			AssertType<NctsBillCollection<NctsBill>>(header.Bills);
		}

		public void TestAdditionalValidation()
		{
			var header = Factory.New<NctsHeader>();
			var errorMessages = header.GetAdditionalValidationErrorMessages();
			AssertEquals(1, errorMessages.Length);
			var msg = errorMessages.Single();
			AssertContains("doesn't have a Revenue ROS certificate uploaded or credential added", msg);

			InterchangeProcessorTestHelper.CreateValidCredential(header.Company);
			Factory.Save();
			errorMessages = header.GetAdditionalValidationErrorMessages();
			AssertEquals("Valid company credential provided, no additional errors.", 0, errorMessages.Length);
		}

		public static NctsHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			return header;
		}
	}
}
