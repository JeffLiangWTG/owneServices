using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(DeclarationCustomResponseSenderSubscriber))]
	class DeclarationCustomResponseSenderSubscriberTest : LogSubscriberTest<DeclarationCustomResponseSenderSubscriber>
	{
		public void TestEventTypes()
		{
			EventRegistryBusinessObjectCollection collection = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.Code = Events.CustomsEntryStatus.Code;
			bizObj.Reference = "ABC";
			EventRegistryBusinessObject bizObj2 = collection.AddNew();
			bizObj2.Code = Events.CustomsEntryStatus.Code;
			bizObj2.Reference = "DEF";
			TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var eventTypes = LogSubscriber.EventTypes;
			AssertEquals(1, eventTypes.Length);
			AssertEquals(Events.CustomsEntryStatus.Code, eventTypes[0]);
			AssertSame(eventTypes, LogSubscriber.EventTypes);
		}

		[TestDate(2007, 5, 31, 10, 8, 1)]
		public void TestProcess()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			dec.JE_HouseBill = "HB";
			dec.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			string[] expectedOutput = { "HB             SYD  AKL  ABC123456CES  Y" + System.Environment.NewLine };
			AssertProcessFile(expectedOutput);
			var reloadFactory = new BusinessObjectFactory();
			var updatedDec = reloadFactory.Load<BaseJobDeclaration>(dec.PK);
			AssertNotNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
			Assert(NotifiedEventList.Contains("[TNT Declaration Customs Response] Customs Response File with following 1 house bills has been successfully sent (Declaration Reference: S00001000) :\r\nHB"));
		}

		[TestDate(2007, 5, 31, 10, 8, 1)]
		public void TestProcess_ReferenceMustMatch()
		{
			EventRegistryBusinessObjectCollection collection = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.Code = Events.CustomsEntryStatus.Code;
			bizObj.Reference = "ABC123";
			TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			dec.JE_HouseBill = "HB";
			dec.Logs.AddNew(Events.CustomsEntryStatus, "");
			Factory.Save();
			AssertProcessFile(null);
			var updatedDec = new BusinessObjectFactory().Load<BaseJobDeclaration>(dec.PK);
			AssertNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
			dec.Logs.AddNew(Events.CustomsEntryStatus, "ABC123");
			Factory.Save();
			string[] expectedOutput = { "HB             SYD  AKL  ABC123456CES  Y" + System.Environment.NewLine };
			AssertProcessFile(expectedOutput);
			updatedDec = new BusinessObjectFactory().Load<BaseJobDeclaration>(dec.PK);
			AssertNotNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
			Assert(NotifiedEventList.Contains("[TNT Declaration Customs Response] Customs Response File with following 1 house bills has been successfully sent (Declaration Reference: S00001000) :\r\nHB"));
		}

		[TestDate(2007, 5, 31, 10, 8, 1)]
		public void TestProcess_EventOnLogParent()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			dec.JE_HouseBill = "HB";
			dec.LogsOfDeclarationOrShipment.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			string[] expectedOutput = { "HB             SYD  AKL  ABC123456CES  Y" + System.Environment.NewLine };
			AssertProcessFile(expectedOutput);
			var reloadFactory = new BusinessObjectFactory();
			var updatedDec = reloadFactory.Load<BaseJobDeclaration>(dec.PK);
			AssertNotNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
			AssertCollectionContains("[TNT Declaration Customs Response] Customs Response File with following 1 house bills has been successfully sent (Declaration Reference: S00001000) :\r\nHB", NotifiedEventList);
		}

		[TestDate(2014, 1, 15, 10, 8, 1)]
		public void TestProcess_WithoutShipment()
		{
			BaseJobDeclaration dec = SetupDeclaration();
			dec.JE_HouseBill = "HB";
			dec.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			string[] expectedOutput = { "HB                       ABC123456CES  Y" + System.Environment.NewLine };
			AssertProcessFile(expectedOutput);
			var reloadFactory = new BusinessObjectFactory();
			var updatedDec = reloadFactory.Load<BaseJobDeclaration>(dec.PK);
			AssertNotNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
			var expectedFileName = TNTDataRegistry.Instance.DeclarationCustomsResponseExportDirectoryRaw.Value + "TIES" + "20140115100801000E." + dec.Branch.GB_Code + ".ok";
			Assert(NotifiedEventList.Contains(ZString.Format(RecordWriteMessage, "HB", expectedFileName)));
			Assert(NotifiedEventList.Contains("[TNT Declaration Customs Response] Customs Response File with following 1 house bills has been successfully sent (Declaration Reference: B00001000) :\r\nHB"));
		}

		[TestDate(2013, 4, 24, 10, 8, 2)]
		public void TestProcessWithMultipleHouseBills()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			dec.JE_HouseBill = "HB";
			dec.JE_MasterBill = "MB";
			Bill bill2 = dec.Bills.AddNew();
			bill2.CU_HouseBill = "HB2";
			Bill bill3 = dec.Bills.AddNew();
			bill3.CU_HouseBill = "HB3";
			AssertEquals("PRE: Declaration has four bills, including one masterbill and three housebill", 4, dec.Bills.Count);
			dec.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			string[] expectedOutputLines = { "HB             SYD  AKL  ABC123456CES  Y" + System.Environment.NewLine, "HB2            SYD  AKL  ABC123456CES  Y" + System.Environment.NewLine, "HB3            SYD  AKL  ABC123456CES  Y" + System.Environment.NewLine };
			AssertProcessFile(expectedOutputLines);
			var reloadFactory = new BusinessObjectFactory();
			var updatedDec = reloadFactory.Load<BaseJobDeclaration>(dec.PK);
			AssertNotNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
			var expectedFileName = TNTDataRegistry.Instance.DeclarationCustomsResponseExportDirectoryRaw.Value + "TIES20130424100802000E." + dec.Branch.GB_Code + ".ok";
			var log = NotifiedEventList.Find(l => l.StartsWith("[TNT Declaration Customs Response] Customs Response File with following 3 house bills has been successfully sent (Declaration Reference: S00001000) :\r\nHB"));
			AssertNotNull(log);
			Assert(log.Contains("\r\nHB"));
			Assert(log.Contains("\r\nHB2"));
			Assert(log.Contains("\r\nHB3"));
			Assert(NotifiedEventList.Contains(ZString.Format(RecordWriteMessage, "HB", expectedFileName)));
			Assert(NotifiedEventList.Contains(ZString.Format(RecordWriteMessage, "HB2", expectedFileName)));
			Assert(NotifiedEventList.Contains(ZString.Format(RecordWriteMessage, "HB3", expectedFileName)));
		}

		[TestDate(2014, 1, 15, 10, 8, 1)]
		public void TestProcess_WithError()
		{
			TNTDataRegistry.Instance.DeclarationCustomsResponseExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZString.Empty);
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			dec.JE_HouseBill = "HB";
			Bill bill2 = dec.Bills.AddNew();
			bill2.CU_HouseBill = "HB2";
			dec.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			AssertNotSendFile(GlbBranch.CurrentBranch.GB_Code);
			Assert("There should be an error message", NotifiedEventList.Contains("[TNT Declaration Customs Response] House Bill (HB) cannot be sent: Directory for Declaration Customs Response Files has not been set. (Declaration Reference: S00001000)"));
			Assert("There should be an error message", NotifiedEventList.Contains("[TNT Declaration Customs Response] House Bill (HB2) cannot be sent: Directory for Declaration Customs Response Files has not been set. (Declaration Reference: S00001000)"));
		}

		[TestDate(2014, 1, 15, 10, 8, 1)]
		public void TestProcess_BranchNotBelongsToCurrentCompany()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "TTT";
			dec.JE_GB = branch.PK;
			AssertNull("PRE: Branch in declaration not belongs to current company", GlbCompany.CurrentCompany.Branches.FindByPK(dec.JE_GB));
			dec.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			AssertNotSendFile(branch.GB_Code);
			Assert("There should be an error message", NotifiedEventList.Contains("[TNT Declaration Customs Response] Customs Response File will not be sent: Declaration's branch does not belong to current company. (Declaration Reference: S00001000)"));
		}

		[TestDate(2014, 1, 15, 10, 8, 1)]
		public void TestProcess_IsNotImport()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			dec.JE_MessageType = "EXP";
			Assert("PRE: Declaration Message Type is not import", !dec.IsImport);
			StmALog log = dec.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			AssertNotSendFile(GlbBranch.CurrentBranch.GB_Code);
			Assert("There should be an error message", NotifiedEventList.Contains("[TNT Declaration Customs Response] Customs Response File will not be sent: Declaration Entry Type is not Import. (Declaration Reference: S00001000)"));
		}

		[TestDate(2014, 1, 15, 10, 8, 1)]
		public void TestProcess_NoHouseBill()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			dec.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			AssertEquals("PRE: Declaration has no house bill", 0, dec.Bills.FindByBillType("HB").Length);
			AssertNotSendFile(GlbBranch.CurrentBranch.GB_Code);
			Assert("There should be an error message", NotifiedEventList.Contains("[TNT Declaration Customs Response] Customs Response File will not be sent: No house bill in the declaration. (Declaration Reference: S00001000)"));
		}

		[TestDate(2014, 1, 15, 10, 8, 1)]
		public void TestProcess_MultipleDeclarations()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TC1";
			var company1Branch = company1.Branches.AddNew();
			company1Branch.GB_Code = "TB1";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "TC2";
			var company2Branch = company2.Branches.AddNew();
			company2Branch.GB_Code = "TB2";
			BaseJobDeclaration dec1 = SetupDeclarationWithShipment();
			dec1.JE_GB = company1Branch.PK;
			dec1.JE_HouseBill = "HB1";
			BaseJobDeclaration dec2 = SetupDeclaration();
			dec2.JE_GB = company2Branch.PK;
			dec2.JE_JS = dec1.JE_JS;
			dec2.JE_HouseBill = "HB2";
			Factory.Save();
			dec2.LogsOfDeclarationOrShipment.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			AssertNotSendFile(GlbBranch.CurrentBranch.GB_Code);
			var expectedMessage = "[TNT Declaration Customs Response] Customs Response File will not be sent: Shipment does not contain a Declaration where the branch belongs to the Current Company. (Parent Table Code: JS, Parent ID: " + dec2.JE_JS + ")";
			AssertCollectionContains("There should be an error message", expectedMessage, NotifiedEventList);
			AssertCollectionNotContains("[TNT Declaration Customs Response] Customs Response File with following 1 house bills has been successfully sent (Declaration Reference: S00001000) :\r\nHB", NotifiedEventList);
			NotifiedEventList.Clear();
			dec2.JE_GB = GlbBranch.CurrentBranch.PK;
			dec2.LogsOfDeclarationOrShipment.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			string[] expectedOutput = { "HB2            SYD  AKL  ABC123456CES  Y" + System.Environment.NewLine };
			AssertProcessFile(expectedOutput);
			AssertCollectionContains("[TNT Declaration Customs Response] Customs Response File with following 1 house bills has been successfully sent (Declaration Reference: S00001000) :\r\nHB2", NotifiedEventList);
		}

		public void TestLogResult_AddDEX()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			Factory.Save();
			var subscriber = new DeclarationCustomResponseSenderSubscriber();
			subscriber.LogResult(dec, 2, new List<ZString>()
			{ "HB", "HB2" });
			Factory.Save();
			var reloadFactory = new BusinessObjectFactory();
			var updatedDec = reloadFactory.Load<BaseJobDeclaration>(dec.PK);
			AssertNotNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		public void TestLogResult_NotAddDEX()
		{
			BaseJobDeclaration dec = SetupDeclarationWithShipment();
			Factory.Save();
			var subscriber = new DeclarationCustomResponseSenderSubscriber();
			subscriber.LogResult(dec, 3, new List<ZString>()
			{ "HB", "HB2" });
			Factory.Save();
			var reloadFactory = new BusinessObjectFactory();
			var updatedDec = reloadFactory.Load<BaseJobDeclaration>(dec.PK);
			AssertNull(updatedDec.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		void AssertProcessFile(string[] expectedLines)
		{
			string fileDateTimeStamp = ZDateTime.Now.ToString("yyyyMMddHHmmssfff");
			string expectedFileName = Path.Combine(Env.TempPath, "TIES" + fileDateTimeStamp + "E." + GlbBranch.CurrentBranch.GB_Code + ".ok");
			try
			{
				RunLogWalkerCycleForTest();
				if (expectedLines != null)
				{
					Assert("file should have been created", File.Exists(expectedFileName));
					string actualOutput = GetStringFromFile(expectedFileName);
					foreach (string expectedLine in expectedLines)
					{
						AssertContains(expectedLine, actualOutput);
					}
				}
				else
				{
					Assert("file should not have been created", !File.Exists(expectedFileName));
				}
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		void AssertNotSendFile(ZString branchCode)
		{
			string fileDateTimeStamp = ZDateTime.Now.ToString("yyyyMMddHHmmssfff");
			string expectedFileName = Path.Combine(Env.TempPath, "TIES" + fileDateTimeStamp + "E." + branchCode + ".ok");
			try
			{
				RunLogWalkerCycleForTest();
				Assert("file should not have been created", !File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		const string RecordWriteMessage = "[TNT Declaration Customs Response] Attempt to write record with bill no {0} in file {1} ......\r\nRecord with bill no {0} is written into file {1}\r\n";
		BaseJobDeclaration SetupDeclaration()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_EntryStatus = "CES";
			dec.JE_RL_NKFinalDestination = "AUSYD";
			dec.JE_RL_NKOrigin = "KHHKG";
			dec.JE_MessageType = "IMP";
			CusEntryHeader entry = dec.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "ABC123456";
			return dec;
		}

		BaseJobDeclaration SetupDeclarationWithShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_CartageWaybill = "SYD-AKL-BNE";
			BaseJobDeclaration dec = SetupDeclaration();
			dec.JE_JS = shipment.PK;
			return dec;
		}

		string GetStringFromFile(string fileName)
		{
			string result;
			using (StreamReader reader = File.OpenText(fileName))
			{
				result = reader.ReadToEnd();
			}

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupBranch();
			SetupRegistry();
		}

		void SetupBranch()
		{
			var branch = (GlbBranch)GlbCompany.CurrentCompany.Branches.FindByPK(GlbBranch.CurrentBranch.PK);
			if (branch == null)
			{
				branch = GlbCompany.CurrentCompany.Branches.AddNew();
				branch.GB_Code = "ABC";
				branch.GB_BranchName = "ABC Branch";
				Factory.Save();
			}
		}

		void SetupRegistry()
		{
			EventRegistryBusinessObjectCollection collection = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.Code = Events.CustomsEntryStatus.Code;
			bizObj.Reference = ZString.Empty;
			TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			TNTDataRegistry.Instance.DeclarationCustomsResponseExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
		}
	}
}
