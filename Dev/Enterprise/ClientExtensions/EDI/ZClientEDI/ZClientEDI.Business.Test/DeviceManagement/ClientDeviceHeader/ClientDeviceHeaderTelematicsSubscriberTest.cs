using System;
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
using Enterprise.Registry.Business;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriberTestBase : ActualDataChangesAuditSubscriberTest
	{
		protected ClientDeviceHeader DeviceHeader { get; private set; }

		protected LicenceDatabase LicenceDatabase { get; private set; }

		protected LicenceDatabase LicenceDatabaseNew { get; private set; }

		protected OrgHeader OrgHeader { get; private set; }

		protected BusinessObjectFactory Factory { get; private set; }

		protected override DataTable GetTestDataTable() => null;

		public override void TestCustomFilter()
		{
			var subscriber = new ClientDeviceHeaderTelematicsSubscriberForTest();
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

			LicenceDatabaseNew = Factory.New<LicenceDatabase>();
			LicenceDatabaseNew.LD_ServerCode = "C78";
			LicenceDatabaseNew.LD_LE = licenceEnterprise.PK;

			DeviceHeader = Factory.New<ClientDeviceHeader>();
			DeviceHeader.CDH_Description = "Brain Scanner";
			DeviceHeader.CDH_Identifier = "TD01234567";
			DeviceHeader.CDH_IsTemplate = false;
			DeviceHeader.CDH_EnterpriseCode = LicenceDatabase.LicEnterprise.LE_EnterpriseCode;
			DeviceHeader.CDH_ServerCode = LicenceDatabase.LD_ServerCode;
			DeviceHeader.CDH_ModelID = "BSMK1";
			DeviceHeader.CDH_Status = ClientDeviceHeaderLookups.Statuses.Active;
			DeviceHeader.CDH_DeviceKind = ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded;

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
				DmgDeviceHeaderSchema.CDH_DeviceKind);
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

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_ActiveServices : ClientDeviceHeaderTelematicsSubscriberTestBase
	{
		public void TestActiveMiddlewareServicesAreGivenToTelematicsNotifier()
		{
			var telematicsServiceCodeMaxLength = 16;
			var registeredTmsServices = new CodeDescriptionBoolCollection();
			registeredTmsServices.Add(telematicsServiceCodeMaxLength, "TELMIDSERV1", (NoResString)"Middle-ware Service 1", true);
			registeredTmsServices.Add(telematicsServiceCodeMaxLength, "TELMIDSERV2", (NoResString)"Middle-ware Service 2", false);
			registeredTmsServices.Add(telematicsServiceCodeMaxLength, "TELMIDSERV3", (NoResString)"Middle-ware Service 3", true);
			EDIDataRegistry.Instance.ActiveMiddlewareService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registeredTmsServices);

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				var notificationRecipient = EDIDataRegistry.Instance.MobileServicesEHubClientID.Value;
				var tmsRecipients = new[] { "TELMIDSERV1", "TELMIDSERV3" };

				var subscriber = new ClientDeviceHeaderTelematicsSubscriberForTest();
				var newFactory = subscriber.Factory;

				var changedDataTable = MakeChangedDataTable();
				changedDataTable.Rows[0].AcceptChanges();
				changedDataTable.Rows[0].SetAdded();
				telematicsNotifier.Setup(m => m.NotifyTelematicsServicesOfDeviceDetails(newFactory,
					tmsRecipients,
					DeviceHeader.CDH_Identifier,
					DeviceHeader.CDH_Description,
					DeviceHeader.CDH_DeviceIdentifier,
					string.Empty,
					LicenceDatabase.LicenceCodeForSystemMessage));
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(1, subscriber.Factory.SaveCount);
			}

			telematicsNotifier.VerifyAll();
		}
	}

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_Notify_WhenCreatingNewRecord_WhenHasDeviceIdentifier : ClientDeviceHeaderTelematicsSubscriberTestBase
	{
		public void TestNotifiesOnCreation()
		{
			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				Factory.Save();

				var notificationRecipient = EDIDataRegistry.Instance.MobileServicesEHubClientID.Value;
				var tmsRecipients = EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().GetAllCodes();
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
				header.CDH_DeviceKind = ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded;

				var subscriber = new ClientDeviceHeaderTelematicsSubscriberForTest();
				var changedDataTable = MakeChangedDataTable(header);
				changedDataTable.Rows[0].AcceptChanges();
				changedDataTable.Rows[0].SetAdded();
				telematicsNotifier.Setup(m => m.NotifyTelematicsServicesOfDeviceDetails(subscriber.Factory,
					tmsRecipients,
					cdh_identifier,
					cdh_description,
					cdh_device_identifier,
					string.Empty,
					LicenceDatabase.LicenceCodeForSystemMessage));
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(1, subscriber.Factory.SaveCount);
			}

			telematicsNotifier.VerifyAll();
		}
	}

	abstract class ClientDeviceHeaderTelematicsSubscriber_DoesNotNotify : ClientDeviceHeaderTelematicsSubscriberTestBase
	{
		public void TestDoesNotNotifyOnChange()
		{
			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var changedDataTable = MakeChangedDataTable();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				var subscriber = new ClientDeviceHeaderTelematicsSubscriberForTest();
				telematicsNotifier.Verify(m => m.NotifyTelematicsServicesOfDeviceDetails(
					It.IsAny<BusinessObjectFactory>(),
					It.IsAny<string[]>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>()), Times.Never);
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(0, subscriber.Factory.SaveCount);
			}

			telematicsNotifier.VerifyAll();
		}
	}

	abstract class ClientDeviceHeaderTelematicsSubscriber_Notify : ClientDeviceHeaderTelematicsSubscriberTestBase
	{
		public void TestNotifiesOnChange()
		{
			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				var notificationRecipient = EDIDataRegistry.Instance.MobileServicesEHubClientID.Value;
				var tmsRecipients = EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().GetAllCodes();

				var subscriber = new ClientDeviceHeaderTelematicsSubscriberForTest();
				var changedDataTable = MakeChangedDataTable();
				telematicsNotifier.Setup(m => m.NotifyTelematicsServicesOfDeviceDetails(subscriber.Factory,
					tmsRecipients,
					DeviceHeader.CDH_Identifier,
					DeviceHeader.CDH_Description,
					DeviceHeader.CDH_DeviceIdentifier,
					string.Empty,
					LicenceDatabase.LicenceCodeForSystemMessage));
				subscriber.ProcessChanges(new DummyLogger(), changedDataTable);
				AssertEquals(1, subscriber.Factory.SaveCount);
			}
			telematicsNotifier.VerifyAll();
		}
	}

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_Notify_WhenDeassigned : ClientDeviceHeaderTelematicsSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);
			dataTable.Rows[0].AcceptChanges();
			dataTable.Rows[0].SetModified();
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = string.Empty;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_ServerCode.Name] = string.Empty;
			return dataTable;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_Notify_WhenAssigned : ClientDeviceHeaderTelematicsSubscriber_Notify
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

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_Notify_WhenAssignmentChanges : ClientDeviceHeaderTelematicsSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_EnterpriseCode.Name] = string.Empty;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_ServerCode.Name] = string.Empty;
			dataTable.Rows[0].AcceptChanges();
			dataTable.Rows[0].SetModified();
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

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_Notify_WhenIdentifierChanges : ClientDeviceHeaderTelematicsSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);
			dataTable.Rows[0].AcceptChanges();
			dataTable.Rows[0].SetModified();
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

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_Notify_WhenKindChanges : ClientDeviceHeaderTelematicsSubscriber_Notify
	{
		protected override DataTable MakeChangedDataTable(ClientDeviceHeader clientDeviceHeader = null)
		{
			var dataTable = base.MakeChangedDataTable(clientDeviceHeader);
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_DeviceKind.Name] = ClientDeviceHeaderLookups.Kinds.Android;
			dataTable.Rows[0].AcceptChanges();
			dataTable.Rows[0].SetModified();
			DeviceHeader.CDH_DeviceKind = ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded;
			dataTable.Rows[0][DmgDeviceHeaderSchema.CDH_DeviceKind.Name] = ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded;

			return dataTable;
		}
	}

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_DoesNotNotify_WhenIsTemplate : ClientDeviceHeaderTelematicsSubscriber_DoesNotNotify
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

	[TestedType(typeof(ClientDeviceHeaderTelematicsSubscriberForTest))]
	class ClientDeviceHeaderTelematicsSubscriber_DoesNotNotify_WhenHasNoDeviceIdentifier : ClientDeviceHeaderTelematicsSubscriber_DoesNotNotify
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

	class ClientDeviceHeaderTelematicsSubscriberForTest : ClientDeviceHeaderTelematicsSubscriber
	{
		public ClientDeviceHeaderTelematicsSubscriberForTest()
		{
			PropertyInfo dataFactory = GetType().BaseType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Single(pi => pi.Name == "DataFactory");
			Factory = (BusinessObjectFactory)dataFactory.GetValue(this);
		}

		public BusinessObjectFactory Factory { get; }
	}
}
