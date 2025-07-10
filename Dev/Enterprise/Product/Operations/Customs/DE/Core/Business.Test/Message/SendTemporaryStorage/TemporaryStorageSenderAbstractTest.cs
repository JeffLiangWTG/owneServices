using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	public abstract class TemporaryStorageSenderAbstractTest<T> : TestCaseWithFactory
		where T : TemporaryStorageSender
	{
		public void TestCanSend_MissingTitleFullNameAndWorkPhone()
		{
			using (Factory.SetTemporaryCurrentUser())
			{
				var (can, whyCannotSend) = TempStorageSender.CanSend;
				CombineAssertions(() =>
				{
					AssertEquals("Has Error", false, can);
					AssertContains("Error Message", "Staff profile is missing a Job Title, Full Name and Work Phone. Please add missing details in Module: Staff and Resources.", whyCannotSend);
				});
			}
		}

		public void TestCanSend_MissingFullNameAndWorkPhone()
		{
			using (Factory.SetTemporaryCurrentUser(title: "Senior Logistics Manager"))
			{
				var (can, whyCannotSend) = TempStorageSender.CanSend;
				CombineAssertions(() =>
				{
					AssertEquals("Has Error", false, can);
					AssertContains("Error Message", "Staff profile is missing a Full Name and Work Phone. Please add missing details in Module: Staff and Resources.", whyCannotSend);
				});
			}
		}

		public void TestCanSend_MissingTitle()
		{
			using (Factory.SetTemporaryCurrentUser(fullName: "Owen Daniels", workPhone: "+61 2 8001 2200"))
			{
				var (can, whyCannotSend) = TempStorageSender.CanSend;
				CombineAssertions(() =>
				{
					AssertEquals("Has Error", false, can);
					AssertContains("Error Message", "Staff profile is missing a Job Title. Please add missing details in Module: Staff and Resources.", whyCannotSend);
				});
			}
		}

		public void TestCanSend_AllPopulated()
		{
			using (Factory.SetTemporaryCurrentUser(title: "Senior Logistics Manager", fullName: "Owen Daniels", workPhone: "+61 2 8001 2200"))
			{
				var (canSend, _) = TempStorageSender.CanSend;
				AssertEquals(true, canSend);
			}
		}

		public void TestSendMessageATLASVersion10_2()
		{
			if (!ExpectedMessageTypeATLASVersion10_2.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._102 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEDIMessageAndLogbookDetails(ExpectedMessageTypeATLASVersion10_2);
				}
			}
			else
			{
				Assert($"ATLAS Version 10.2 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestLogbookRegNumIfIdentificationIndicatorIsREG10_2()
		{
			if (!ExpectedMessageTypeATLASVersion10_2.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._102 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertLogbookRegistrationNumber(TemporaryStorageIdentificationIndicatorList.Codes.REG, ExpectedLogbookRegNumForIndicatorREG);
				}
			}
			else
			{
				Assert($"ATLAS Version 10.2 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestLogbookRegNumIfIdentificationIndicatorIsNotREG10_2()
		{
			if (!ExpectedMessageTypeATLASVersion10_2.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._102 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertLogbookRegistrationNumber(TemporaryStorageIdentificationIndicatorList.Codes.AWB, ExpectedLogbookRegNumForIndicatorNotREG);
				}
			}
			else
			{
				Assert($"ATLAS Version 10.2 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestSendMessageATLASVersion10_1()
		{
			if (!ExpectedMessageTypeATLASVersion10_1.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEDIMessageAndLogbookDetails(ExpectedMessageTypeATLASVersion10_1);
				}
			}
			else
			{
				Assert($"ATLAS Version 10.1 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestLogbookRegNumIfIdentificationIndicatorIsREG10_1()
		{
			if (!ExpectedMessageTypeATLASVersion10_1.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertLogbookRegistrationNumber(TemporaryStorageIdentificationIndicatorList.Codes.REG, ExpectedLogbookRegNumForIndicatorREG);
				}
			}
			else
			{
				Assert($"ATLAS Version 10.1 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestLogbookRegNumIfIdentificationIndicatorIsNotREG10_1()
		{
			if (!ExpectedMessageTypeATLASVersion10_1.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertLogbookRegistrationNumber(TemporaryStorageIdentificationIndicatorList.Codes.AWB, ExpectedLogbookRegNumForIndicatorNotREG);
				}
			}
			else
			{
				Assert($"ATLAS Version 10.1 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestLogbookLocalReferenceNumberEmpty()
		{
			storageJobHeader.SJH_ReferenceNumber = ZString.Empty;
			TempStorageSender.Send();
			AssertEquals(ZString.Empty, TempStorageDec.Messages[0].GetLogbookLocalReferenceNumber());
		}

		protected CusTempStorageDec TempStorageDec => tempStorageDec ?? (tempStorageDec = GetTempStorageDecToTest());
		CusTempStorageDec tempStorageDec;

		protected TemporaryStorageSender TempStorageSender => tempStorageSender ?? (tempStorageSender = GetTempStorageSender());
		TemporaryStorageSender tempStorageSender;

		protected abstract CusTempStorageDec GetTempStorageDecToTest();

		protected abstract TemporaryStorageSender GetTempStorageSender();

		protected virtual ZString ExpectedMessageTypeATLASVersion10_2 => ZString.Empty;

		protected virtual ZString ExpectedMessageTypeATLASVersion10_1 => ZString.Empty;

		protected virtual ZString ExpectedLogbookRegNumForIndicatorREG => ZString.Empty;

		protected virtual ZString ExpectedLogbookRegNumForIndicatorNotREG => ZString.Empty;

		protected abstract ZString ExpectedMessageSubType { get; }

		protected override void SetUp()
		{
			base.SetUp();
			var representative = Factory.GetOrgHeaderWithEori("TEST", "1234567", Core.Constants.CountryCodes.Greece);
			var cusCode = representative.MainAddress.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			cusCode.OK_CustomsRegNo = "0000";
			storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OA_Representative = representative.MainAddress.PK;
			storageJobHeader.SJH_ReferenceNumber = LogbookLocalReferenceNumber;
		}
		protected CusTempStorageJobHeader storageJobHeader;

		void AssertEDIMessageAndLogbookDetails(ZString expectedMessageType)
		{
			TempStorageSender.Send();
			var message = TempStorageDec.Messages.Cast<EDIMessage>().Single();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.TemporaryStorage, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", expectedMessageType, message.EM_ApplicationReference);
				AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("EM_LinkedObject", TempStorageDec.PK, message.EM_LinkedObject.PK);
				AssertEquals("STH_MessageStatus", Common.Shared.MessageStatusList.Codes.Sent, TempStorageDec.STH_MessageStatus);
				AssertEquals("LogbookEORIBranchSuffix exists", "0000", message.GetLogbookEORIBranchSuffix());
				AssertEquals("LogbookLocalReferenceNumber exists", LogbookLocalReferenceNumber, message.GetLogbookLocalReferenceNumber());
			});
		}

		void AssertLogbookRegistrationNumber(ZString identificationIndicator, ZString expectedRegistrationNumber)
		{
			TempStorageDec.STH_IdentificationIndicator = identificationIndicator;
			TempStorageSender.Send();
			var message = TempStorageDec.Messages.Cast<EDIMessage>().Single();
			AssertEquals(expectedRegistrationNumber, message.GetLogbookRegistrationNumber());
		}

		const string LogbookLocalReferenceNumber = "ATB1234567";
	}
}

