using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconDeclaration))]
	sealed class CusReconDeclarationTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.NewWithValidTestData<CusReconDeclaration>();

		public void TestCusReconEntryLines()
		{
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "TestCompany");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			var reconDeclaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			reconDeclaration.CusReconEntryLines.AddNew();
			AssertEquals(1, reconDeclaration.CusReconEntryLines.Count);
			AssertEquals(1, reconDeclaration.CusReconEntries.Count);

			var reconEntry = reconDeclaration.CusReconEntries[0];
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var reconEntryLine = reconDeclaration.CusReconEntryLines[0];
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var savedReconDeclaration = factory.Load<CusReconDeclaration>(reconDeclaration.PK);
			AssertEquals(1, savedReconDeclaration.CusReconEntryLines.Count);
			AssertEquals(1, savedReconDeclaration.CusReconEntries.Count);
		}

		public void TestStandAloneRefundDeclarationCalculatedFields()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			reconDeclaration.CRD_CustomsStatus = CustomsEntryStatusTypeList.Codes.NDC;

			var message5UL = reconDeclaration.Messages.AddNew();
			message5UL.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			message5UL.EM_SystemCreateTimeUtc = new ZDateTime(2024, 10, 30, 23, 0, 0);
			message5UL.EM_LinkUniqueID = reconDeclaration.PK;
			message5UL.EM_LinkTable = "CusReconDeclaration";

			CreateCusEntryNumber(ElectronicDocumentTypeList.Codes._5UL, "6N00221000001X", new ZDate(2023, 01, 02));
			CreateCusEntryNumber(ElectronicDocumentTypeList.Codes._5UO, "6N00221000002X", new ZDate(2023, 03, 01));
			CreateCusEntryNumber(ElectronicDocumentTypeList.Codes._5UN, "6N00221000003X", new ZDate(2023, 03, 03));
			CreateEntryLineAndCharges(1, 2);
			CreateEntryLineAndCharges(3, 4);

			AssertEquals("6N00221000001X", reconDeclaration.RefundDeclarationNumber);
			AssertEquals("6N002-21-000001X", reconDeclaration.FormattedRefundDeclarationNumber);
			AssertEquals("6N00221000002X", reconDeclaration.RefundApprovalNumber);
			AssertEquals("6N00221000003X", reconDeclaration.ProvisionNumber);
			AssertEquals(new ZDate(2023, 01, 02), reconDeclaration.AcceptedDate);
			AssertEquals(new ZDate(2023, 03, 01), reconDeclaration.RefundApprovalDate);
			AssertEquals(new ZDate(2023, 03, 03), reconDeclaration.ProvisionDate);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalSent, reconDeclaration.MessageStatusDescription);
			AssertEquals(CustomsEntryStatusTypeList.Descriptions.NDC, reconDeclaration.EntryStatusDescription);
			AssertEquals(10m, reconDeclaration.TotalRefundAmount);
			AssertEquals(2, reconDeclaration.RefundBillCount);

			void CreateCusEntryNumber(ZString messageType, ZString entryNumber, ZDate issueDate)
			{
				var entryNum = Factory.New<CusEntryNumber>();
				entryNum.CE_EntryType = messageType;
				entryNum.CE_EntryNum = entryNumber;
				entryNum.CE_IssueDate = issueDate;
				entryNum.CE_ParentID = reconDeclaration.PK;
				entryNum.CE_ParentTable = CusReconDeclaration.Schema.TableName;
			}

			void CreateEntryLineAndCharges(decimal amount1, decimal amount2)
			{
				var reconEntryLine = reconDeclaration.CusReconEntryLines.AddNew();
				var reconCustomsCharge1 = reconEntryLine.CusReconCharges.AddNew();
				reconCustomsCharge1.CRC_Amount = amount1;
				var reconCustomsCharge2 = reconEntryLine.CusReconCharges.AddNew();
				reconCustomsCharge2.CRC_Amount = amount2;
			}
		}

		public void TestMaxLength()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			AssertEquals(3, reconDeclaration.CRD_CustomsOfficeInfo.MaxLength);
			AssertEquals(1, reconDeclaration.CRD_DeclarationTypeInfo.MaxLength);
		} 

		public void TestCaptions()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "RefundDeclarationNumber", true, attrib => attrib.Caption == "Entry Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "TotalRefundAmount", true, attrib => attrib.Caption == "Total Refund Amount");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "MessageStatusDescription", true, attrib => attrib.ShortCaption == "Msg. Status Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "MessageStatusDescription", true, attrib => attrib.Caption == "Message Status Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "EntryStatusDescription", true, attrib => attrib.ShortCaption == "Entry Status Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "EntryStatusDescription", true, attrib => attrib.Caption == "Entry Status Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "AcceptedDate", true, attrib => attrib.Caption == "Accepted Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "RefundApprovalDate", true, attrib => attrib.Caption == "Refund Approval Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "RefundApprovalNumber", true, attrib => attrib.MediumCaption == "Refund Approval No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "RefundApprovalNumber", true, attrib => attrib.Caption == "Refund Approval Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "RefundBillCount", true, attrib => attrib.Caption == "Refund Bill Count");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "PayerCompanyName", true, attrib => attrib.Caption == "Payer Company Name");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_GB_Branch", true, attrib => attrib.Caption == "Branch Code");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_CustomsStatus", true, attrib => attrib.Caption == "Entry Status");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_OA_DeclarantAddress", true, attrib => attrib.Caption == "Payer");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "OfficeDescription", true, attrib => attrib.Caption == "Customs Office Name");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "FormattedRefundDeclarationNumber", true, attrib => attrib.Caption == "Entry Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "ProvisionDate", true, attrib => attrib.Caption == "Provision Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "ProvisionNumber", true, attrib => attrib.MediumCaption == "Provision No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "ProvisionNumber", true, attrib => attrib.Caption == "Provision Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_DeclarationType", true, attrib => attrib.Caption == "Refund Type");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_RefundCauseCode", true, attrib => attrib.Caption == "Refund Cause");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_RefundReasonCode", true, attrib => attrib.Caption == "Refund Reason");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_CustomsDivision", true, attrib => attrib.Caption == "Department");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_TaxOffice", true, attrib => attrib.Caption == "Tax Office");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "CRD_GS_NKCustomsAgent", true, attrib => attrib.Caption == "Broker");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "PayerBank", true, attrib => attrib.Caption == "Payer Bank");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "BankAccountNumber", true, attrib => attrib.MediumCaption == "Bank Account No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "BankAccountNumber", true, attrib => attrib.Caption == "Bank Account Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "RegistrationNumberOne", true, attrib => attrib.MediumCaption == "Registration No.1");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "RegistrationNumberOne", true, attrib => attrib.Caption == "Registration Number 1");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "KoreanRegistrationNumberOfCEO", true, attrib => attrib.MediumCaption == "Registration No.2");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(reconDeclaration.GetType(), "KoreanRegistrationNumberOfCEO", true, attrib => attrib.Caption == "Registration Number 2");
		}

		public void TestFetchStrategy()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			Assertion.AssertEquals("CusReconDeclaration.FetchStrategy type", expected: true, reconDeclaration.FetchStrategy is CusReconDeclarationFetchStrategy);
		}

		public void TestLists()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			AssertEquals("Lookups.MessageStatusList", reconDeclaration.CRD_MessageStatusInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.EntryStatusList", reconDeclaration.CRD_CustomsStatusInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.RefundTypeList", reconDeclaration.CRD_DeclarationTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.CustomsOfficeList", reconDeclaration.CRD_CustomsOfficeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		[TestDate(2024, 10, 10)]
		public void TestGenerateJobNumberWhenFirstSave()
		{
			var reconDeclaration1 = Factory.New<CusReconDeclaration>();
			reconDeclaration1.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			reconDeclaration1.CRD_ApplicationCode = "KRC";
			Factory.Save();
			AssertEquals("reconDeclaration is in db", true, reconDeclaration1.IsInDatabase);
			AssertEquals("JobNumber is assigned", "CRD00000001", reconDeclaration1.CRD_JobReferenceNumber);

			var reconDeclaration2 = Factory.New<CusReconDeclaration>();
			reconDeclaration2.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			reconDeclaration2.CRD_ApplicationCode = "KRC";
			Factory.Save();
			AssertEquals("reconDeclaration is in db", true, reconDeclaration2.IsInDatabase);
			AssertEquals("JobNumber is assigned", "CRD00000002", reconDeclaration2.CRD_JobReferenceNumber);
		}

		public void TestClearJobNumberWhenSaveFails()
		{
			CusReconDeclarationThrowingExceptionAfterOnSaving reconDeclaration1 = Factory.New<CusReconDeclarationThrowingExceptionAfterOnSaving>();
			reconDeclaration1.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			reconDeclaration1.CRD_ApplicationCode = "KRC";
			reconDeclaration1.ShouldThrowException = true;

			AssertEquals("JobNumber is empty", true, reconDeclaration1.CRD_JobReferenceNumber.IsEmpty);
			try
			{
				Factory.Save();
			}
			catch (Exception) { }

			AssertEquals("reconDeclaration is not in db", false, reconDeclaration1.IsInDatabase);
			AssertEquals("JobNumber is empty", true, reconDeclaration1.CRD_JobReferenceNumber.IsEmpty);

			var newFactory = new BusinessObjectFactory();
			CusReconDeclarationThrowingExceptionAfterOnSaving reconDeclaration2 = newFactory.New<CusReconDeclarationThrowingExceptionAfterOnSaving>();
			reconDeclaration2.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			reconDeclaration2.CRD_ApplicationCode = "KRC";
			reconDeclaration2.ShouldThrowException = false;
			newFactory.Save();
			AssertEquals("reconDeclaration2 is in db", true, reconDeclaration2.IsInDatabase);
			AssertEquals("JobNumber is assigned", false, reconDeclaration2.CRD_JobReferenceNumber.IsEmpty);
			
			reconDeclaration1.ShouldThrowException = false;
			Factory.Save();
			AssertEquals("reconDeclaration is in db", true, reconDeclaration1.IsInDatabase);
			AssertEquals("JobNumber is assigned", false, reconDeclaration1.CRD_JobReferenceNumber.IsEmpty);
			AssertEquals("The JobNumber of reconDeclaration1 is the next number after the JobNumber of reconDeclaration2", 1, reconDeclaration1.CRD_JobReferenceNumber.CompareTo(reconDeclaration2.CRD_JobReferenceNumber));

			var jobNumber = reconDeclaration1.CRD_JobReferenceNumber;
			reconDeclaration1.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("JobNumber is not removed or changed as it was assigned before this transaction", jobNumber, reconDeclaration1.CRD_JobReferenceNumber);
		}

		[TestDate(2024, 12, 06)]
		public void TestGenerateEntryNumber()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			var reconDeclaration1 = Factory.New<CusReconDeclaration>();

			AssertEquals("Entry Number is empty", true, reconDeclaration1.RefundDeclarationNumber.IsEmpty);

			Factory.Save();

			AssertEquals("reconDeclaration is in db", true, reconDeclaration1.IsInDatabase);
			AssertEquals("Entry Number is assigned", "123452400001U", reconDeclaration1.RefundDeclarationNumber);

			var reconDeclaration2 = Factory.New<CusReconDeclaration>();

			AssertEquals("Entry Number is empty", true, reconDeclaration2.RefundDeclarationNumber.IsEmpty);

			Factory.Save();

			AssertEquals("reconDeclaration is in db", true, reconDeclaration2.IsInDatabase);
			AssertEquals("Entry Number is assigned", "123452400002U", reconDeclaration2.RefundDeclarationNumber);

			var reconDeclaration_Fails = Factory.New<CusReconDeclarationThrowingExceptionAfterOnSaving>();
			reconDeclaration_Fails.ShouldThrowException = true;

			AssertEquals("Entry Number is empty", true, reconDeclaration_Fails.RefundDeclarationNumber.IsEmpty);
			AssertExceptionThrown(typeof(Exception), Factory.Save);

			AssertEquals("reconDeclaration is not in db", false, reconDeclaration_Fails.IsInDatabase);
			AssertEquals("Entry Number is empty", true, reconDeclaration_Fails.RefundDeclarationNumber.IsEmpty);

			var newFactory = new BusinessObjectFactory();
			var reconDeclaration3 = newFactory.New<CusReconDeclarationThrowingExceptionAfterOnSaving>();
			reconDeclaration3.ShouldThrowException = false;

			AssertEquals("Entry Number is empty", true, reconDeclaration3.RefundDeclarationNumber.IsEmpty);

			newFactory.Save();

			AssertEquals("reconDeclaration is in db", true, reconDeclaration3.IsInDatabase);
			AssertEquals("Entry Number is assigned", false, reconDeclaration3.RefundDeclarationNumber.IsEmpty);

			reconDeclaration_Fails.ShouldThrowException = false;
			Factory.Save();
			AssertEquals("reconDeclaration is not in db", true, reconDeclaration_Fails.IsInDatabase);
			AssertEquals("Entry Number is empty", false, reconDeclaration_Fails.RefundDeclarationNumber.IsEmpty);
			AssertEquals("The Entry Number of reconDeclaration_Fails is the next number after the Entry Number of reconDeclaration3", 1, reconDeclaration_Fails.RefundDeclarationNumber.CompareTo(reconDeclaration3.RefundDeclarationNumber));
		}

		public void TestRegistrationNumbers()
		{
			var cusReconDeclaration = Factory.New<CusReconDeclaration>();
			Assert(cusReconDeclaration.RegistrationNumberOne.IsEmpty);
			Assert(cusReconDeclaration.KoreanRegistrationNumberOfCEO.IsEmpty);

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "READYKOREA", "�����ڸ���");
			OrgAddress payerAddress = payer.MainAddress;
			payerAddress.OA_OH = payer.PK;
			cusReconDeclaration.CRD_OA_DeclarantAddress = payerAddress.PK;
			AssertNotNull(cusReconDeclaration.Payer);
			Assert(cusReconDeclaration.RegistrationNumberOne.IsEmpty);
			Assert(cusReconDeclaration.KoreanRegistrationNumberOfCEO.IsEmpty);

			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "0001011399942", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);

			AssertEquals("0001011399942", cusReconDeclaration.RegistrationNumberOne);
			AssertEquals(ZString.Empty, cusReconDeclaration.KoreanRegistrationNumberOfCEO);

			payer.OH_Category = "BUS";
			AssertEquals("1168103897", cusReconDeclaration.RegistrationNumberOne);
			AssertEquals("0001011399942", cusReconDeclaration.KoreanRegistrationNumberOfCEO);
		}

		public void TestPayerBankInfo()
		{
			var orgHeader1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "TestCompany1");
			var orgHeader2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "KR2", "TestCompany2");
			var reconDeclaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			reconDeclaration.CRD_OA_DeclarantAddress = orgHeader1.MainAddress.PK;
			var wrapper = OrgHeaderWrapper.New(reconDeclaration.Payer);
			AssertNotNull(reconDeclaration.PayerWrapper);
			AssertNullOrEmpty(reconDeclaration.PayerBank);
			AssertNullOrEmpty(reconDeclaration.BankAccountNumber);
			wrapper.ZO_BankCode = "020";
			wrapper.ZO_BankAccNo = "1005002390381";
			AssertEquals("020", reconDeclaration.PayerBank);
			AssertEquals("1005002390381", reconDeclaration.BankAccountNumber);

			reconDeclaration.CRD_OA_DeclarantAddress = orgHeader2.MainAddress.PK;
			wrapper = OrgHeaderWrapper.New(reconDeclaration.Payer);
			AssertNotNull(reconDeclaration.PayerWrapper);
			AssertNullOrEmpty(reconDeclaration.PayerBank);
			AssertNullOrEmpty(reconDeclaration.BankAccountNumber);
			wrapper.ZO_BankCode = "030";
			wrapper.ZO_BankAccNo = "021323449323";
			AssertEquals("030", reconDeclaration.PayerBank);
			AssertEquals("021323449323", reconDeclaration.BankAccountNumber);
		}

		public void TestRefundCauseCodeAndRefundReasonCode()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._05;
			reconDeclaration.CRD_RefundReasonCode = RefundReasonCodeList.Codes._01;
			AssertEquals("05", reconDeclaration.CRD_RefundCauseCode);
			AssertEquals("01", reconDeclaration.CRD_RefundReasonCode);

			reconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._01;
			AssertEquals("01", reconDeclaration.CRD_RefundCauseCode);
			AssertEquals("Refund Reason Code is cleared when Refund Cause Code is not mandatory", ZString.Empty, reconDeclaration.CRD_RefundReasonCode);
		}

		public void TestIsRefundTypeContractRevocation()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			Assert(!reconDeclaration.IsRefundTypeContractRevocation);

			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.A;
			Assert(!reconDeclaration.IsRefundTypeContractRevocation);

			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.B;
			Assert(reconDeclaration.IsRefundTypeContractRevocation);

			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.C;
			Assert(!reconDeclaration.IsRefundTypeContractRevocation);

			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.D;
			Assert(!reconDeclaration.IsRefundTypeContractRevocation);

			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.E;
			Assert(!reconDeclaration.IsRefundTypeContractRevocation);
		}

		class CusReconDeclarationThrowingExceptionAfterOnSaving : CusReconDeclaration
		{
			public CusReconDeclarationThrowingExceptionAfterOnSaving(BusinessObjectFactory factory, DataRow row)
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
