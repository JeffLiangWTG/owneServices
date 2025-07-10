using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSendingObjectParent))]
	sealed class ManifestMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExportPath()
		{
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(Factory.New<AsycudaManifestHeaderForTest>());
			var data = DataBoundResourceStrings.GetDataForProperty(messageSendingObjectParent.ExportPathInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Export To", data.Caption);
				AssertEquals("FullDescription", $"The folder the message will be exported to. This can be set in Registry -> {JPRegistry.Instance.DefaultFolderForExportingMessages.GetLocationInEnglish()}", data.FullDescription);
			});
		}

		public void TestGetNotificationsMessage()
		{
			var header = Factory.New<AsycudaManifestHeaderForTest>();
			ManifestMessageSendingObjectParent messageSendingObjectParent;
			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 }))
			{
				header.AMA_CustomsOffice = "";
				header.Validation.ValidateAll();
				var bill = header.Bills.AddNew();
				messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				(messageSendingObjectParent.SendingObjectsCollection.First() as ManifestMessageSendingObject).ShouldSend = false;
				Assert(string.IsNullOrEmpty(messageSendingObjectParent.AdditionalWarnings));
			}

			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_TransportMode = "SEA";

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01, Action = JPMessageActionList.Codes.Five }))
			{
				header.AMA_MasterBill = "123";
				header.MasterBill.ABL_GoodsLocation = "321";
				header.Validation.ValidateAll();
				messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection.First() as ManifestMessageSendingObject;
				sendingObject.ShouldSend = false;
				sendingObject.Action = NVC01MessageActionList.Codes.Five;
				Assert(messageSendingObjectParent.AdditionalWarnings.Contains(header.AMA_CustomsOfficeInfo.HumanReadableName));
				Assert(!messageSendingObjectParent.AdditionalWarnings.Contains(header.AMA_ManifestDescriptionInfo.HumanReadableName));
			}
		}

		public void TestGetContentProviders_HCH01_End()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 }))
			{
				header.Bills.AddNew();
				var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				(messageSendingObjectParent.SendingObjectsCollection.First() as ManifestMessageSendingObject).ShouldSend = true;

				var provider = messageSendingObjectParent.GetContentProviders().FirstOrDefault() as ManifestMessageContentProvider;
				AssertEquals("Count when it is not sending HCH01 END message.", 1, provider.SendingObjects.Count());

				messageSendingObjectParent.EndSendMessage = true;
				provider = messageSendingObjectParent.GetContentProviders().FirstOrDefault() as ManifestMessageContentProvider;
				AssertEquals("Count when it is sending HCH01 END message.", 2, provider.SendingObjects.Count());
			}
		}

		public void TestGetContentProviders_SendNVC01BondedLocationAmendment()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01, Action = JPMessageActionList.Codes.Five }))
			{
				var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var providers = messageSendingObjectParent.GetContentProviders().Cast<ManifestMessageContentProvider>().ToList();
				AssertEquals("Should get 1 content providers.", 1, providers.Count);

				var provider = providers[0];
				AssertEquals("First provider should contain 1 bills for Max Count.", 1, provider.SendingObjects.Count());

				AssertEquals("Action of dummy bill should be 5.", NVC01MessageActionList.Codes.Five, provider.SendingObjects.First().Action);
			}
		}

		public void TestGetContentProviders_NVC01()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 }))
			{
				for (var i = 0; i < 23; i++)
				{
					header.Bills.AddNew();
				}
				var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjects = messageSendingObjectParent.SendingObjectsCollection.ToList();

				for (var i = 0; i < 21; i++)
				{
					(sendingObjects[i] as ManifestMessageSendingObject).Action = NVC01MessageActionList.Codes.Nine;
				}

				(sendingObjects[21] as ManifestMessageSendingObject).Action = NVC01MessageActionList.Codes.Five;
				(sendingObjects[22] as ManifestMessageSendingObject).Action = NVC01MessageActionList.Codes.Five;

				var providers = messageSendingObjectParent.GetContentProviders().Cast<ManifestMessageContentProvider>().ToList();
				AssertEquals("Should get 3 content providers.", 3, providers.Count);

				var provider0 = providers[0];
				AssertEquals("First provider should contain 20 bills for Max Count.", 20, provider0.SendingObjects.Count());
				for (var i = 0; i < 20; i++)
				{
					Assert($"First provider should contain bill of number {i}.", provider0.SendingObjects.Contains(sendingObjects[i]));
				}

				var provider1 = providers[1];
				AssertEquals("Second provider should contain 1 bills for Max Count and Action Group.", 1, provider1.SendingObjects.Count());
				Assert("Second provider should contain bill of number 20.", provider1.SendingObjects.Contains(sendingObjects[20]));

				var provider2 = providers[2];
				AssertEquals("Third provider should contain 2 bills for Action Group.", 2, provider2.SendingObjects.Count());
				Assert("Third provider should contain bill of number 21.", provider2.SendingObjects.Contains(sendingObjects[21]));
				Assert("Third provider should contain bill of number 22.", provider2.SendingObjects.Contains(sendingObjects[22]));
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SetCurrentMessageSendingContext(new MessageSendingContext());
			return new ManifestMessageSendingObjectParent(header);
		}

		public void TestSetDefaultValues()
		{
			using (var dir = new TempDirectory())
			using (JPRegistry.Instance.DefaultFolderForExportingMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dir.DirectoryName))
			{
				var defaultFolderForExportingMessages = JPRegistry.Instance.DefaultFolderForExportingMessages.Value;
				AssertEquals(dir.DirectoryName, defaultFolderForExportingMessages);

				var header = Factory.New<AsycudaManifestHeader>();
				var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				AssertEquals(defaultFolderForExportingMessages, messageSendingObjectParent.ExportPath);
			}
		}

		public void TestGetBizObjValidationMessageErrors()
		{
			var header = Factory.New<AsycudaManifestHeaderForTest>();
			header.Bills.AddNew();
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			(messageSendingObjectParent.SendingObjectsCollection.First() as ManifestMessageSendingObject).ShouldSend = false;
			Assert(string.IsNullOrWhiteSpace(messageSendingObjectParent.BizObjValidationMessageErrors));

			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_TransportMode = "SEA";

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01, Action = JPMessageActionList.Codes.Five }))
			{
				header.AMA_MasterBill = "111";
				header.MasterBill.ABL_GoodsLocation = "333";
				messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObject = (messageSendingObjectParent.SendingObjectsCollection.First() as ManifestMessageSendingObject);
				sendingObject.ShouldSend = false;
				sendingObject.Action = NVC01MessageActionList.Codes.Five;
				Assert(!string.IsNullOrWhiteSpace(messageSendingObjectParent.BizObjValidationMessageErrors));
				Assert(messageSendingObjectParent.AdditionalWarnings.Contains(header.AMA_CustomsOfficeInfo.HumanReadableName));
				Assert(!messageSendingObjectParent.AdditionalWarnings.Contains(header.AMA_ManifestDescriptionInfo.HumanReadableName));
			}
		}

		public void TestUseVisualDataAndSupportVisual()
		{
			var parent = GetNewBusinessObject() as ManifestMessageSendingObjectParent;
			Assert("Should default to false.", !parent.UseVisualData);
			Assert("Should default to false.", !parent.Context.EnableMessageVisual);
		}

		public void TestInitializeVisualObjects()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "JPTYK";

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 }))
			{
				var maxCount = Constants.Message.MaxNumberOfBillsForHCH01;
				var totalCount = maxCount + 5;

				for (var i = 0; i < totalCount; i++)
				{
					header.Bills.AddNew();
				}

				var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
				AssertEquals("PreCondition", totalCount, messageSendingObjectParent.SendingObjectsCollection.Count);

				foreach (var sendingObject in messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>())
				{
					sendingObject.ShouldSend = true;
				}

				var providers = messageSendingObjectParent.GetContentProviders();
				messageSendingObjectParent.VisualObjectParent.InitializeVisualObjects(providers);

				var batchSendingObjectCounts = messageSendingObjectParent.VisualObjectParent
					.VisualObjects
					.Select(c => (c.ContentProvider as ManifestMessageContentProvider).SendingObjects.Count())
					.ToArray();

				AssertArrayEqualsByElements("Should batch to 2 visual objects.", new[] { maxCount, 5 }, batchSendingObjectCounts);
			}
		}

		public void TestSendingObjectCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
			AssertEquals(0, sendingObjectParent.SendingObjectsCollection.Count);

			header.Bills.AddNew();
			header.Bills.AddNew();
			sendingObjectParent = new ManifestMessageSendingObjectParent(header);
			AssertEquals(2, sendingObjectParent.SendingObjectsCollection.Count);
		}

		public void TestBizObjValidationMessageErrors()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.AMA_RL_NKPortOfLoading = "JPTKX";
			header.AMA_RL_NKPortOfDischarge = "JPTYK";
			header.CreateMessageErrorForTest = true;
			header.Validation.ValidateAll();
			header.Bills.AddNew();
			header.Bills.AddNew().CreateMessageErrorForTest = true;
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var messageSendingObject1 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(0);
			messageSendingObject1.ShouldSend = false;
			var messageSendingObject2 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(1);
			messageSendingObject2.ShouldSend = false;

			AssertNullOrEmpty(messageSendingObjectParent.BizObjValidationMessageErrors);

			messageSendingObject1.ShouldSend = true;
			var bizObjValidationMessageErrors = messageSendingObjectParent.BizObjValidationMessageErrors;
			CombineAssertions(() =>
			{
				AssertContains("Test message error on AMA_ManifestType.", bizObjValidationMessageErrors);
				AssertNotContains("Test message error on ABL_BillNumber.", bizObjValidationMessageErrors);
			});

			messageSendingObject2.ShouldSend = true;
			bizObjValidationMessageErrors = messageSendingObjectParent.BizObjValidationMessageErrors;
			CombineAssertions(() =>
			{
				AssertContains("Test message error on AMA_ManifestType.", bizObjValidationMessageErrors);
				AssertContains("Test message error on ABL_BillNumber.", bizObjValidationMessageErrors);
			});
		}

		public void TestBizObjValidationMessageErrorsWhenSendingSpecificMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.CreateMessageErrorForTest = true;
			header.Validation.ValidateAll();
			header.Bills.AddNew();
			header.Bills.AddNew().CreateMessageErrorForTest = true;

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 };
			header.SetCurrentMessageSendingContext(messageSendingContext);

			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var messageSendingObject1 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(0);
			var messageSendingObject2 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(1);

			messageSendingObject1.ShouldSend = false;
			messageSendingObject2.ShouldSend = false;
			AssertNullOrEmpty(messageSendingObjectParent.BizObjValidationMessageErrors);

			messageSendingObject1.ShouldSend = true;
			messageSendingObject2.ShouldSend = true;
			var bizObjValidationMessageErrors = messageSendingObjectParent.BizObjValidationMessageErrors;

			CombineAssertions(() =>
			{
				AssertContains("Test message error on AMA_ManifestType when sending NVC01 message.", bizObjValidationMessageErrors);
				AssertContains("Test message error on ABL_BillNumber when sending NVC01 message.", bizObjValidationMessageErrors);
			});

			messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.EDA;

			messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			messageSendingObject1 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(0);
			messageSendingObject2 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(1);

			messageSendingObject1.ShouldSend = true;
			messageSendingObject2.ShouldSend = true;
			bizObjValidationMessageErrors = messageSendingObjectParent.BizObjValidationMessageErrors;

			CombineAssertions(() =>
			{
				AssertNotContains("Test message error on AMA_ManifestType when sending NVC01 message.", bizObjValidationMessageErrors);
				AssertNotContains("Test message error on ABL_BillNumber when sending NVC01 message.", bizObjValidationMessageErrors);
			});
		}

		public void TestAdditionalWarnings()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.CreateWarningForTest = true;
			header.Validation.ValidateAll();
			header.Bills.AddNew();
			header.Bills.AddNew().CreateWarningForTest = true;

			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var messageSendingObject1 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(0);
			messageSendingObject1.ShouldSend = false;

			var messageSendingObject2 = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ElementAt(1);
			messageSendingObject2.ShouldSend = false;
			AssertNullOrEmpty(messageSendingObjectParent.AdditionalWarnings);

			messageSendingObject1.ShouldSend = true;
			var additionalWarnings = messageSendingObjectParent.AdditionalWarnings;
			CombineAssertions(() =>
			{
				AssertContains("Test warning on AMA_ManifestType.", additionalWarnings);
				AssertNotContains("Test warning on ABL_BillNumber.", additionalWarnings);
			});

			messageSendingObject2.ShouldSend = true;
			additionalWarnings = messageSendingObjectParent.AdditionalWarnings;
			CombineAssertions(() =>
			{
				AssertContains("Test warning on AMA_ManifestType.", additionalWarnings);
				AssertContains("Test warning on ABL_BillNumber.", additionalWarnings);
			});
		}

		public void TestAllowSendWithError()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.CreateMessageErrorForTest = true;
			header.Validation.ValidateAll();
			header.Bills.AddNew();
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;

			messageSendingObject.ShouldSend = true;
			Assert(!messageSendingObjectParent.AllowSendWithErrorInfo.ReadOnly);
			Assert(!messageSendingObjectParent.AllowSendWithError);

			messageSendingObjectParent.AllowSendWithError = true;
			messageSendingObject.ShouldSend = false;
			Assert(!messageSendingObjectParent.AllowSendWithError);
			Assert(messageSendingObjectParent.AllowSendWithErrorInfo.ReadOnly);
		}

		public void TestRunPreSaveValidationInConstructor()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.CreateMessageErrorForTest = true;
			header.CreateWarningForTest = true;
			var targetInfo = header.AMA_ManifestTypeInfo;
			CombineAssertions(() =>
			{
				AssertNoMessageErrors(targetInfo);
				AssertNoWarnings(targetInfo);
			});

			new ManifestMessageSendingObjectParent(header);
			CombineAssertions(() =>
			{
				AssertHasMessageError(targetInfo, "Test message error on AMA_ManifestType.");
				AssertHasWarning(targetInfo, "Test warning on AMA_ManifestType.");
			});
		}

		public void TestRefreshValidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.Bills.AddNew();
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			header.CreateMessageErrorForTest = true;
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
			header.MarkAsNeedingValidation();
			messageSendingObject.ShouldSend = true;
			AssertNotContains("Test message error on AMA_ManifestType.", messageSendingObjectParent.BizObjValidationMessageErrors);
		}
	}
}
