using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusMiscRequestHeader))]
	sealed class CusMiscRequestHeaderTest : Customs.Business.Testing.CusMiscRequestHeaderTest
	{
		public void TestIControllerIDProvider()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			var iCusMiscRequestHeader = (IControllerIDProvider)cusMiscRequestHeader;
			AssertEquals(cusMiscRequestHeader.PK, iCusMiscRequestHeader.BusinessObjectPK);
			AssertEquals(ControllerIDs.Customs.KR.MiscRequestMessages, iCusMiscRequestHeader.ControllerID);
		}

		public void TestMessagesParent()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			var iCusMiscRequestHeader = (IEDIMessageCollectionProvider)cusMiscRequestHeader;
			AssertEquals(Factory, iCusMiscRequestHeader.Factory);
			AssertEquals(cusMiscRequestHeader.Messages, iCusMiscRequestHeader.Messages);
		}
		public void TestCusEntryNumber()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			AssertNull(cusMiscRequestHeader.CusEntryNumber);

			var entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.CE_EntryIsSystemGenerated = true;
			entryNum1.CE_ParentID = cusMiscRequestHeader.PK;
			entryNum1.CE_ParentTable = cusMiscRequestHeader.TableName;
			entryNum1.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5AC;
			entryNum1.CE_EntryNum = "6N00221000001X";
			entryNum1.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			AssertEquals("6N00221000001X", cusMiscRequestHeader.CusEntryNumber.CE_EntryNum);

			var entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.CE_EntryIsSystemGenerated = true;
			entryNum2.CE_ParentID = cusMiscRequestHeader.PK;
			entryNum2.CE_ParentTable = cusMiscRequestHeader.TableName;
			entryNum2.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5AC;
			entryNum2.CE_EntryNum = "6N00221000002X";
			entryNum2.CE_SystemCreateTimeUtc = ZDateTime.Today;

			AssertLessThan("entryNum1 was created before entryNum2", entryNum1.CE_SystemCreateTimeUtc, entryNum2.CE_SystemCreateTimeUtc);
			AssertEquals("6N00221000001X", cusMiscRequestHeader.CusEntryNumber.CE_EntryNum);
		}
		[TestDate(2021, 10, 10)]
		public void TestGenerateJobNumberAndEntryNumberWhenFirstSave()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			cusMiscRequestHeader.CMR_RequestDate = ZDateTime.Today;
			cusMiscRequestHeader.CMR_CustomsOffice = "010";
			cusMiscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(cusMiscRequestHeader.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			Factory.Save();

			var entryNumber5AC = cusMiscRequestHeader.CusEntryNumber.CE_EntryNum;
			AssertEquals("cusMiscRequestHeader is in db", true, cusMiscRequestHeader.IsInDatabase);
			AssertEquals("entry number length is 15", 15, entryNumber5AC.Length);
			AssertEquals("entry number is assigned", "6N002210000001U", entryNumber5AC);
			AssertEquals("JobNumber is assigned", "MSC00000001", cusMiscRequestHeader.CMR_JobNumber);

			var cusMiscRequestHeader2 = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader2.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			cusMiscRequestHeader2.CMR_RequestDate = ZDateTime.Today;
			cusMiscRequestHeader2.CMR_CustomsOffice = "010";
			cusMiscRequestHeader2.CMR_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var entryNumber5GW = cusMiscRequestHeader2.CusEntryNumber.CE_EntryNum;
			AssertEquals("cusMiscRequestHeader is in db", true, cusMiscRequestHeader2.IsInDatabase);
			AssertEquals("entry number length is 15", 15, entryNumber5GW.Length);
			AssertEquals("entry number is assigned", "6N002210000001U", entryNumber5GW);
			AssertEquals("JobNumber is increased", "MSC00000002", cusMiscRequestHeader2.CMR_JobNumber);

			var cusMiscRequestHeader3 = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader3.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			cusMiscRequestHeader3.CMR_RequestDate = ZDateTime.Today;
			cusMiscRequestHeader3.CMR_CustomsOffice = "010";
			cusMiscRequestHeader3.CMR_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var entryNumber5SG = cusMiscRequestHeader3.CusEntryNumber.CE_EntryNum;
			AssertEquals("cusMiscRequestHeader is in db", true, cusMiscRequestHeader3.IsInDatabase);
			AssertEquals("entry number length is 19", 19, entryNumber5SG.Length);
			AssertEquals("entry number is assigned", "5SG6N0022021X000001", entryNumber5SG);
			AssertEquals("JobNumber is increased", "MSC00000003", cusMiscRequestHeader3.CMR_JobNumber);
		}
		public void TestClearJobNumberAndEntryNumberWhenSaveFails()
		{
			MiscRequestHeaderThrowingExceptionAfterOnSaving cusMiscRequestHeader = Factory.New<MiscRequestHeaderThrowingExceptionAfterOnSaving>();
			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			cusMiscRequestHeader.CMR_RequestDate = ZDateTime.Today;
			cusMiscRequestHeader.CMR_CustomsOffice = "010";
			cusMiscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
			cusMiscRequestHeader.ShouldThrowException = true;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");

			AssertNull("EntryNumber is empty", cusMiscRequestHeader.CusEntryNumber);
			AssertEquals("JobNumber is empty", true, cusMiscRequestHeader.CMR_JobNumber.IsEmpty);
			try
			{
				Factory.Save();
			}
			catch (Exception) { }

			AssertEquals("cusMiscRequestHeader is not in db", false, cusMiscRequestHeader.IsInDatabase);
			AssertEquals("EntryNumber is empty", true, cusMiscRequestHeader.CusEntryNumber.CE_EntryNum.IsEmpty);
			AssertEquals("JobNumber is empty", true, cusMiscRequestHeader.CMR_JobNumber.IsEmpty);

			cusMiscRequestHeader.ShouldThrowException = false;
			Factory.Save();
			AssertEquals("cusMiscRequestHeader is in db", true, cusMiscRequestHeader.IsInDatabase);
			AssertNotNull("EntryNumber is assigned", cusMiscRequestHeader.CusEntryNumber.CE_EntryNum);
			AssertEquals("JobNumber is assigned", false, cusMiscRequestHeader.CMR_JobNumber.IsEmpty);

			var entryNumber = cusMiscRequestHeader.CusEntryNumber.CE_EntryNum;
			var jobNumber = cusMiscRequestHeader.CMR_JobNumber;
			cusMiscRequestHeader.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("EntryNumber is not removed or changed as it was assigned before this transaction", entryNumber, cusMiscRequestHeader.CusEntryNumber.CE_EntryNum);
			AssertEquals("JobNumber is not removed or changed as it was assigned before this transaction", jobNumber, cusMiscRequestHeader.CMR_JobNumber);
		}

		public void TestMessageTypeName()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_MessageType = "XXX";
			AssertEquals("", cusMiscRequestHeader.MessageTypeName);

			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			AssertEquals("수출 임시개청 신청서", cusMiscRequestHeader.MessageTypeName);

			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			AssertEquals("수입 임시개청 신청서", cusMiscRequestHeader.MessageTypeName);
		}
		public void TestStatusName()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_Status = "XXX";
			AssertEquals("", cusMiscRequestHeader.StatusName);

			cusMiscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			AssertEquals("오류통보", cusMiscRequestHeader.StatusName);

			cusMiscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal;
			AssertEquals("전송오류", cusMiscRequestHeader.StatusName);

			cusMiscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			AssertEquals("전송진행/완료", cusMiscRequestHeader.StatusName);

			cusMiscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals("접수통보", cusMiscRequestHeader.StatusName);
		}
		public void TestCustomsOfficeName()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_CustomsOffice = "00000";
			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			AssertEquals("", cusMiscRequestHeader.CustomsOfficeName);

			cusMiscRequestHeader.CMR_CustomsOffice = "01000";
			AssertEquals("서울세관", cusMiscRequestHeader.CustomsOfficeName);

			cusMiscRequestHeader.CMR_CustomsOffice = "00010";
			AssertEquals("통관지원(1)과", cusMiscRequestHeader.CustomsOfficeName);

			cusMiscRequestHeader.CMR_CustomsOffice = "01010";
			AssertEquals("서울세관 통관지원(1)과", cusMiscRequestHeader.CustomsOfficeName);

			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;

			cusMiscRequestHeader.CMR_CustomsOffice = "010";
			AssertEquals("서울세관", cusMiscRequestHeader.CustomsOfficeName);
		}

		public void TestDeclarantCompanyName()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = ZGuid.Empty;

			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_GB = branch.PK;
			AssertEquals("", cusMiscRequestHeader.DeclarantCompanyName);

			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Name = "Test Company Name";
			branch.GB_GC = company.PK;
			company.GC_Name = "Test Company Name";
			AssertEquals("Test Company Name", cusMiscRequestHeader.DeclarantCompanyName);
		}
		public void TestBrokerName()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TSD";
			staff.GS_FullName = "Test Staff Data";
			Factory.Save();

			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_GS_NKBroker = "XXX";
			AssertEquals("", cusMiscRequestHeader.BrokerName);

			cusMiscRequestHeader.CMR_GS_NKBroker = "TSD";
			AssertEquals("Test Staff Data", cusMiscRequestHeader.BrokerName);
		}
		public void TestFormattedCustomsOffice()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_CustomsOffice = "010";
			AssertEquals("010", cusMiscRequestHeader.FormattedCustomsOffice);

			cusMiscRequestHeader.CMR_CustomsOffice = "01010";
			AssertEquals("010-10", cusMiscRequestHeader.FormattedCustomsOffice);
		}

		public void TestGetCustomsOffice()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			cusMiscRequestHeader.CMR_CustomsOffice = "01010";
			AssertEquals("010", cusMiscRequestHeader.CustomsOffice);

			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			AssertEquals("010", cusMiscRequestHeader.CustomsOffice);

			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			AssertEquals("01010", cusMiscRequestHeader.CustomsOffice);
		}

		public void TestFormattedApplicationNumber()
		{
			var cusMiscRequestHeader5AC = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader5AC.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			var entryNum5AC = Factory.New<CusEntryNumber>();
			entryNum5AC.CE_EntryIsSystemGenerated = true;
			entryNum5AC.CE_ParentID = cusMiscRequestHeader5AC.PK;
			entryNum5AC.CE_ParentTable = cusMiscRequestHeader5AC.TableName;
			entryNum5AC.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			entryNum5AC.CE_EntryType = ElectronicDocumentTypeList.Codes._5AC;
			entryNum5AC.CE_EntryNum = "6N00221000001X";
			entryNum5AC.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			AssertEquals("6N002-21-000001X", cusMiscRequestHeader5AC.FormattedApplicationNumber);

			var cusMiscRequestHeader5GW = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader5GW.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			var entryNum5GW = Factory.New<CusEntryNumber>();
			entryNum5GW.CE_EntryIsSystemGenerated = true;
			entryNum5GW.CE_ParentID = cusMiscRequestHeader5GW.PK;
			entryNum5GW.CE_ParentTable = cusMiscRequestHeader5GW.TableName;
			entryNum5GW.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			entryNum5GW.CE_EntryType = ElectronicDocumentTypeList.Codes._5GW;
			entryNum5GW.CE_EntryNum = "6N00221000001X";
			entryNum5GW.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			AssertEquals("6N002-21-000001X", cusMiscRequestHeader5GW.FormattedApplicationNumber);

			var cusMiscRequestHeader5SG = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader5SG.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			var entryNum5SG = Factory.New<CusEntryNumber>();
			entryNum5SG.CE_EntryIsSystemGenerated = true;
			entryNum5SG.CE_ParentID = cusMiscRequestHeader5SG.PK;
			entryNum5SG.CE_ParentTable = cusMiscRequestHeader5SG.TableName;
			entryNum5SG.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			entryNum5SG.CE_EntryType = ElectronicDocumentTypeList.Codes._5SG;
			entryNum5SG.CE_EntryNum = "5SG6N0022021X000001";
			entryNum5SG.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			AssertEquals("5SG-6N002-2021-X-000001", cusMiscRequestHeader5SG.FormattedApplicationNumber);
		}

		public void Test5SGReviewData()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			cusMiscRequestHeader.CMR_RequestDate = ZDateTime.Today;
			cusMiscRequestHeader.CMR_CustomsOffice = "010";
			cusMiscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_ParentID = cusMiscRequestHeader.PK;
			entryNum.CE_ParentTable = cusMiscRequestHeader.TableName;
			entryNum.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			entryNum.CE_EntryType = cusMiscRequestHeader.CMR_MessageType;
			entryNum.CE_EntryNum = "6N00221000001X";
			entryNum.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			entryNum.CE_IssueDate = new ZDateTime(2022, 1, 1);

			var outgoingMessage = cusMiscRequestHeader.Messages.AddNew();
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_SystemCreateUser = "ORG";

			Factory.Save();

			var incomingMessage = cusMiscRequestHeader.Messages.AddNew();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5SH;
			incomingMessage.EM_SystemCreateUser = "ORG";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			incomingMessage.EM_ApplicationReference = outgoingMessage.EM_MessageNum;

			Factory.Save();
			cusMiscRequestHeader.Reload();

			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, cusMiscRequestHeader.CustomsReviewStatus);
			AssertEquals(new ZDateTime(2022, 1, 1), cusMiscRequestHeader.ReviewDate5SG);
		}

		class MiscRequestHeaderThrowingExceptionAfterOnSaving : CusMiscRequestHeader
		{
			public MiscRequestHeaderThrowingExceptionAfterOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ShouldThrowException;
			public override void OnSaving()
			{
				base.OnSaving();
				if (ShouldThrowException)
				{
					throw new ApplicationException("intended");
				}
			}
		}
	}
}
