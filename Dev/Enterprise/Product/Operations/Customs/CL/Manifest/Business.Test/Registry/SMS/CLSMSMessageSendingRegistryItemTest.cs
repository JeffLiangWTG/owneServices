using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(CLSMSMessageSendingRegistryItem))]
	class CLSMSMessageSendingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CLSMSMessageSending>
	{
		public void TestCredentialSending()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);

			var registryItem = CLCustomsDataRegistry.Instance.CLSMSMessageSending;

			var newValue1 = new CLSMSMessageSending(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			registryItem.OnUpdateAction(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newValue1);

			var newValue2 = new CLSMSMessageSending(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			newValue2.MachineName = "MACHINE2";
			newValue2.ApplicationNodeName = "Application Node 2";
			newValue2.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Awaiting;
			registryItem.OnUpdateAction(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newValue2);

			var newValue3 = new CLSMSMessageSending(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			newValue3.MachineName = "MACHINE3";
			newValue3.ApplicationNodeName = "Application Node 3";
			newValue3.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Registered;
			registryItem.OnUpdateAction(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newValue3);

			registryItem.OnAllValuesSaved();

			var interchange = Factory.Load<EDIInterchange>(new ZQuery()).Single();
			CombineAssertions("Interchange & Message", () =>
			{
				AssertContains(@"CLSMSMessageSending 1: no change, no Interchange should be created.
CLSMSMessageSending 2: status AWT, no Interchange should be created.
CLSMSMessageSending 3: should have created an Interchange.", "APPLICATION NODE 3", interchange.EI_BodyText);

				var message = Factory.Load<EDIMessage>(new ZQuery()).Single();
				AssertEquals("EM_LinkUniqueID", GlbCompany.CurrentCompany.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", GlbCompanySchema.Constants.TableName, message.EM_LinkTable);
			});
		}

		protected override StronglyTypedRegistryItem<CLSMSMessageSending, CLSMSMessageSending> GetNewRegistryItem()
		{
			return new CLSMSMessageSendingRegistryItem(
				"", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default,
				new CLSMSMessageSending
				{
					MachineName = "Machine Name",
					ApplicationNodePassword = "1234",
					RunningIntervalInSeconds = 60,
					SendFolder = @"D:\Folders\SendFolder",
					UnknownFolder = @"D:\Folders\UnknownFolder",
					InvalidFolder = @"D:\Folders\InvalidFolder",
					RejectedFolder = @"D:\Folders\RejectedFolder",
					ReceiveFolder = @"D:\Folders\ReceiveFolder",
					AcceptedFolder = @"D:\Folders\AcceptedFolder"
				});
		}
	}

	[TestedType(typeof(CLSMSMessageSendingRegistryDataType))]
	class CLSMSMessageSendingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CLSMSMessageSendingRegistryDataType>
	{
		protected override CLSMSMessageSendingRegistryDataType GetNewDataType() => new CLSMSMessageSendingRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new CLSMSMessageSending { MachineName = "Machine Name", ApplicationNodePassword = "1234", RunningIntervalInSeconds = 60, SendFolder = @"D:\Folders\SendFolder", UnknownFolder = @"D:\Folders\UnknownFolder", InvalidFolder = @"D:\Folders\InvalidFolder", RejectedFolder = @"D:\Folders\RejectedFolder", ReceiveFolder = @"D:\Folders\ReceiveFolder", AcceptedFolder = @"D:\Folders\AcceptedFolder" };
			var sample2 = new CLSMSMessageSending { MachineName = "Machine Name 2", ApplicationNodePassword = "1234", RunningIntervalInSeconds = 60, SendFolder = @"D:\Folders\SendFolder", UnknownFolder = @"D:\Folders\UnknownFolder", InvalidFolder = @"D:\Folders\InvalidFolder", RejectedFolder = @"D:\Folders\RejectedFolder", ReceiveFolder = @"D:\Folders\ReceiveFolder", AcceptedFolder = @"D:\Folders\AcceptedFolder" };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new CLSMSMessageSendingRegistryDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new CLSMSMessageSendingRegistryDataType().Serialise(sample2))
			};
		}

		protected override string ExpectedEditorName => "CLSMSMessageSendingRegistryItemEditor";
	}
}
