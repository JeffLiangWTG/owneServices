using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Client.EDI.DeviceManagement.Business.Subscribers;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceHeaderGlowSubscriberForTest))]
	class ClientDeviceHeaderGlowSubscriberTestBase : ActualDataChangesAuditSubscriberTest
	{
		protected ClientDeviceHeader DeviceHeader { get; private set; }

		protected LicenceDatabase LicenceDatabase { get; private set; }

		protected OrgHeader OrgHeader { get; private set; }

		protected BusinessObjectFactory Factory { get; private set; }

		protected override DataTable GetTestDataTable() => null;

		public override void TestCustomFilter()
		{
			var subscriber = new ClientDeviceHeaderGlowSubscriberForTest();
			AssertNull(subscriber.CustomFilter);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Factory = new BusinessObjectFactory();

			OrgHeader = Factory.New<OrgHeader>();
			OrgHeader.OH_Code = "TEST000";
			OrgHeader.OH_FullName = "Test Organization";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "A12";
			licenceEnterprise.LE_OH = OrgHeader.PK;

			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyCode = "B34";
			licenceCompany.LC_LE = licenceEnterprise.PK;
			licenceCompany.LC_OH = OrgHeader.PK;

			LicenceDatabase = Factory.New<LicenceDatabase>();
			LicenceDatabase.LD_ServerCode = "C56";
			LicenceDatabase.LD_LE = licenceEnterprise.PK;

			DeviceHeader = Factory.New<ClientDeviceHeader>();
			DeviceHeader.CDH_Description = "Brain Scanner";
			DeviceHeader.CDH_Identifier = "TD01234567";
			DeviceHeader.CDH_IsTemplate = false;
			DeviceHeader.CDH_EnterpriseCode = LicenceDatabase.LicEnterprise.LE_EnterpriseCode;
			DeviceHeader.CDH_ServerCode = LicenceDatabase.LD_ServerCode;
			DeviceHeader.CDH_ModelID = "BSMK1";
			DeviceHeader.CDH_Status = ClientDeviceHeaderLookups.Statuses.Active;

			DeviceHeader.CDH_DeviceIdentifier = "QWEASDZXC";

			SetUpBeforeSave();

			Factory.Save();
		}

		protected virtual void SetUpBeforeSave()
		{
		}

		protected override void TearDown()
		{
			LicenceDatabase = null;
			DeviceHeader = null;

			base.TearDown();
		}

		protected virtual DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = NewDataTable();
			var dataRow = NewDataRow(dataTable, clientDeviceHeader ?? DeviceHeader);
			dataTable.Rows.Add(dataRow);
			dataRow.AcceptChanges();
			dataRow.SetModified();

			return dataTable;
		}

		DataTable NewDataTable()
		{
			var dataTable = new DataTable();
			AddColumns(
				dataTable,
				DmgDeviceHeaderSchema.CDH_Description,
				DmgDeviceHeaderSchema.CDH_DeviceIdentifier,
				DmgDeviceHeaderSchema.CDH_EnterpriseCode,
				DmgDeviceHeaderSchema.CDH_Identifier,
				DmgDeviceHeaderSchema.CDH_IsTemplate,
				DmgDeviceHeaderSchema.CDH_ModelID,
				DmgDeviceHeaderSchema.CDH_ServerCode,
				DmgDeviceHeaderSchema.CDH_Status,
				DmgDeviceHeaderSchema.CDH_DeviceKind,
				DmgDeviceHeaderSchema.CDH_IsBYOD);
			return dataTable;
		}

		DataRow NewDataRow(DataTable dataTable, ClientDeviceHeader clientDeviceHeader)
		{
			var dataRow = dataTable.NewRow();
			dataRow[DmgDeviceHeaderSchema.CDH_Description.Name] = clientDeviceHeader.CDH_Description;
			dataRow[DmgDeviceHeaderSchema.CDH_DeviceIdentifier.Name] = clientDeviceHeader.CDH_DeviceIdentifier;
			dataRow[DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = clientDeviceHeader.CDH_EnterpriseCode;
			dataRow[DmgDeviceHeaderSchema.CDH_Identifier.Name] = clientDeviceHeader.CDH_Identifier;
			dataRow[DmgDeviceHeaderSchema.CDH_IsTemplate.Name] = (bool)clientDeviceHeader.CDH_IsTemplate;
			dataRow[DmgDeviceHeaderSchema.CDH_ModelID.Name] = clientDeviceHeader.CDH_ModelID;
			dataRow[DmgDeviceHeaderSchema.CDH_ServerCode.Name] = clientDeviceHeader.CDH_ServerCode;
			dataRow[DmgDeviceHeaderSchema.CDH_Status.Name] = clientDeviceHeader.CDH_Status;
			dataRow[DmgDeviceHeaderSchema.CDH_DeviceKind.Name] = clientDeviceHeader.CDH_DeviceKind;
			dataRow[DmgDeviceHeaderSchema.CDH_IsBYOD.Name] = (bool)clientDeviceHeader.CDH_IsBYOD;
			return dataRow;
		}

		void AddColumns(DataTable dataTable, params SchemaColumn[] schemaColumns)
		{
			foreach (var schemaColumn in schemaColumns)
			{
				dataTable.Columns.Add(schemaColumn.Name, schemaColumn.DotNetType);
			}
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriberForTest))]
	class ClientDeviceHeaderGlowSubscriber_ActiveServices : ClientDeviceHeaderGlowSubscriberTestBase
	{
		public void TestActiveMiddlewareServicesAreGivenToTelematicsNotifier()
		{
			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				var notificationRecipient = EDIDataRegistry.Instance.MobileServicesEHubClientID.Value;

				var subscriber = new ClientDeviceHeaderGlowSubscriberForTest();
				var newFactory = subscriber.Factory;

				var changedDataTable = MakeChangedDataTable();
				changedDataTable.Rows[0][DmgDeviceHeaderSchema.CDH_Description.Name] = "NDES";
				telematicsNotifier.Setup(m => m.NotifyMobileServicesOfDeviceDetails(
					newFactory,
					notificationRecipient,
					DeviceHeader.CDH_Identifier,
					DeviceHeader.CDH_IsBYOD,
					"WiseTech Global",
					"NDES",
					DeviceHeader.GetDeviceKindForMobileServices(),
					DeviceHeader.CDH_DeviceIdentifier,
					LicenceDatabase.LicenceCodeForSystemMessage));
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(1, subscriber.Factory.SaveCount);
			}

			telematicsNotifier.VerifyAll();
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriberForTest))]
	class ClientDeviceHeaderGlowSubscriber_Notify_WhenCreatingNewRecord_WhenHasDeviceIdentifier : ClientDeviceHeaderGlowSubscriberTestBase
	{
		public void TestNotifiesOnCreation()
		{
			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				Factory.Save();

				var notificationRecipient = EDIDataRegistry.Instance.MobileServicesEHubClientID.Value;
				var cdh_description = "Device Model Here";
				var cdh_device_identifier = "WOWZA";
				var cdh_identifier = "CDHID";

				var header = Factory.New<ClientDeviceHeader>();
				header.CDH_IsTemplate = false;
				header.CDH_ModelID = "Boo";
				DeviceHeader.CDH_EnterpriseCode = LicenceDatabase.LicEnterprise.LE_EnterpriseCode;
				DeviceHeader.CDH_ServerCode = LicenceDatabase.LD_ServerCode;
				header.CDH_Description = cdh_description;
				header.CDH_Identifier = cdh_identifier;
				header.CDH_DeviceIdentifier = cdh_device_identifier;

				var subscriber = new ClientDeviceHeaderGlowSubscriberForTest();
				var changedDataTable = MakeChangedDataTable(header);
				changedDataTable.Rows[0].AcceptChanges();
				changedDataTable.Rows[0].SetAdded();
				telematicsNotifier.Setup(m => m.NotifyMobileServicesOfDeviceDetails(
					subscriber.Factory,
					notificationRecipient,
					cdh_identifier,
					false,
					"WiseTech Global",
					cdh_description,
					header.GetDeviceKindForMobileServices(),
					cdh_device_identifier,
					LicenceDatabase.LicenceCodeForSystemMessage));
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(1, subscriber.Factory.SaveCount);
			}

			telematicsNotifier.VerifyAll();
		}
	}

	abstract class ClientDeviceHeaderGlowSubscriber_DoesNotNotify : ClientDeviceHeaderGlowSubscriberTestBase
	{
		public void TestDoesNotNotifyOnChange()
		{
			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var changedDataTable = MakeChangedDataTable();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				var subscriber = new ClientDeviceHeaderGlowSubscriberForTest();
				telematicsNotifier.Verify(m => m.NotifyMobileServicesOfDeviceDetails(
					It.IsAny<BusinessObjectFactory>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<bool>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<CargoWise.MobileServices.Common.Messages.DeviceKind>(),
					It.IsAny<string>(),
					It.IsAny<string>()), Times.Never);
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(0, subscriber.Factory.SaveCount);
			}

			telematicsNotifier.VerifyAll();
		}
	}

	abstract class ClientDeviceHeaderGlowSubscriber_Notify : ClientDeviceHeaderGlowSubscriberTestBase
	{
		public void TestNotifiesOnChange()
		{
			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				var notificationRecipient = EDIDataRegistry.Instance.MobileServicesEHubClientID.Value;

				var subscriber = new ClientDeviceHeaderGlowSubscriberForTest();
				var changedDataTable = MakeChangedDataTable();
				changedDataTable.Rows[0][DmgDeviceHeaderSchema.CDH_Description.Name] = "NDES";
				telematicsNotifier.Setup(m => m.NotifyMobileServicesOfDeviceDetails(
					subscriber.Factory,
					notificationRecipient,
					DeviceHeader.CDH_Identifier,
					DeviceHeader.CDH_IsBYOD,
					"WiseTech Global",
					"NDES",
					DeviceHeader.GetDeviceKindForMobileServices(),
					DeviceHeader.CDH_DeviceIdentifier,
					LicenceDatabase.LicenceCodeForSystemMessage));
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(1, subscriber.Factory.SaveCount);
			}

			telematicsNotifier.VerifyAll();
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriber))]
	class ClientDeviceHeaderGlowSubscriber_Notify_WhenDeassigned : ClientDeviceHeaderGlowSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);

			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = string.Empty;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_ServerCode.Name] = string.Empty;
			return dataTable;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriber))]
	class ClientDeviceHeaderGlowSubscriber_Notify_WhenAssigned : ClientDeviceHeaderGlowSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);

			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = LicenceDatabase.LicEnterprise.LE_EnterpriseCode;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_ServerCode.Name] = LicenceDatabase.LD_ServerCode;
			return dataTable;
		}

		protected override void SetUpBeforeSave()
		{
			base.SetUpBeforeSave();

			DeviceHeader.CDH_EnterpriseCode = ZString.Empty;
			DeviceHeader.CDH_ServerCode = ZString.Empty;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriber))]
	class ClientDeviceHeaderGlowSubscriber_Notify_WhenAssignmentChanges : ClientDeviceHeaderGlowSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);

			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = LicenceDatabase.LicEnterprise.LE_EnterpriseCode;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_ServerCode.Name] = LicenceDatabase.LD_ServerCode;

			return dataTable;
		}

		protected override void SetUpBeforeSave()
		{
			base.SetUpBeforeSave();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TEST001";
			orgHeader.OH_FullName = "Shell company for Test Organization";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "QWE";
			licenceEnterprise.LE_OH = orgHeader.PK;

			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyCode = "ASD";
			licenceCompany.LC_LE = licenceEnterprise.PK;
			licenceCompany.LC_OH = orgHeader.PK;

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_ServerCode = "ZXC";
			licenceDatabase.LD_LE = licenceEnterprise.PK;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriber))]
	class ClientDeviceHeaderGlowSubscriber_Notify_WhenIdentifierChanges : ClientDeviceHeaderGlowSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);

			DeviceHeader.CDH_DeviceIdentifier = "QWERTYUIOP";
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_DeviceIdentifier.Name] = "QWERTYUIOP";

			return dataTable;
		}

		protected override void SetUpBeforeSave()
		{
			base.SetUpBeforeSave();
			DeviceHeader.CDH_DeviceIdentifier = string.Empty;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriber))]
	class ClientDeviceHeaderGlowSubscriber_Notify_WhenKindChanges : ClientDeviceHeaderGlowSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);

			DeviceHeader.CDH_DeviceKind = ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_DeviceKind.Name] = ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded;

			return dataTable;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriber))]
	class ClientDeviceHeaderGlowSubscriber_DoesNotNotify_WhenIsTemplate : ClientDeviceHeaderGlowSubscriber_DoesNotNotify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);

			DeviceHeader.CDH_Description = "Pretty Cool Device (TM)";
			DeviceHeader.CDH_EnterpriseCode = ZString.Empty;
			DeviceHeader.CDH_ServerCode = ZString.Empty;
			DeviceHeader.CDH_DeviceIdentifier = "QWERTYUIOP";

			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_Description.Name] = DeviceHeader.CDH_Description;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = DeviceHeader.CDH_EnterpriseCode;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_ServerCode.Name] = DeviceHeader.CDH_ServerCode;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_DeviceIdentifier.Name] = DeviceHeader.CDH_DeviceIdentifier;

			return dataTable;
		}

		protected override void SetUpBeforeSave()
		{
			base.SetUpBeforeSave();

			DeviceHeader.CDH_IsTemplate = true;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderGlowSubscriber))]
	class ClientDeviceHeaderGlowSubscriber_DoesNotNotify_WhenHasNoDeviceIdentifier : ClientDeviceHeaderGlowSubscriber_DoesNotNotify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);

			DeviceHeader.CDH_Description = "Pretty Cool Device (TM)";
			DeviceHeader.CDH_EnterpriseCode = ZString.Empty;
			DeviceHeader.CDH_ServerCode = ZString.Empty;

			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_Description.Name] = DeviceHeader.CDH_Description;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = DeviceHeader.CDH_EnterpriseCode;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_ServerCode.Name] = DeviceHeader.CDH_ServerCode;

			return dataTable;
		}

		protected override void SetUpBeforeSave()
		{
			base.SetUpBeforeSave();

			DeviceHeader.CDH_DeviceIdentifier = ZString.Empty;
		}
	}

	class ClientDeviceHeaderGlowSubscriberForTest : ClientDeviceHeaderGlowSubscriber
	{
		public ClientDeviceHeaderGlowSubscriberForTest()
		{
			PropertyInfo dataFactory = GetType().BaseType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Single(pi => pi.Name == "DataFactory");
			Factory = (BusinessObjectFactory)dataFactory.GetValue(this);
		}

		public BusinessObjectFactory Factory { get; }
	}
}
