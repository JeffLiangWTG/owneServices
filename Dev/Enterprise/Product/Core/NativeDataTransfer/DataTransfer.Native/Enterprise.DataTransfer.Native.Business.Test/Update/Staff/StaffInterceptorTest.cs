using System;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.Staff
{
	class StaffInterceptorTest : TransactionedTestCase
	{
		public void TestLoginName_InvalidCharacters()
		{
			var staff = new Entity(TestUtil.FindEntityDefinition("Staff", "GlbStaff"), sessionServices) { Action = EntityAction.MERGE };
			staff["Code"] = "TS1";
			staff["LoginName"] = "TEST";
			AssertNoExceptionThrown(() => context.Update(new EntitySet("Staff") { Root = staff }));

			staff["Code"] = "TS2";
			staff["LoginName"] = "in\\va?lid*nam[e]";
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), "Login Name can not contain any of these symbols: \" / \\ [ ] : ; | = , + * ? < >",
				() => context.Update(new EntitySet("Staff") { Root = staff }));
		}

		public void TestLoginName_ControlCharacters()
		{
			var staff = new Entity(TestUtil.FindEntityDefinition("Staff", "GlbStaff"), sessionServices) { Action = EntityAction.MERGE };
			staff["Code"] = "TS1";
			staff["LoginName"] = "TEST";
			AssertNoExceptionThrown(() => context.Update(new EntitySet("Staff") { Root = staff }));

			staff["Code"] = "TS3";
			staff["LoginName"] = "user\r\n\tname";
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), "Login Name can not contain control characters, such as line break or tab.",
				() => context.Update(new EntitySet("Staff") { Root = staff }));
		}

		public void TestLoginName_InvalidForADPrefix()
		{
			var staff = new Entity(TestUtil.FindEntityDefinition("Staff", "GlbStaff"), sessionServices) { Action = EntityAction.MERGE };
			staff["Code"] = "TS4";
			staff["LoginName"] = "user1";
			AssertNoExceptionThrown(() => context.Update(new EntitySet("Staff") { Root = staff }));

			staff["Code"] = "TS6";
			staff["LoginName"] = "user3";
			ObjectFactory.Get<IADRegistry>().UserLoginPrefix = "EDI-PRD-";
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), "Login Name must begin with 'EDI-PRD-'.",
				() => context.Update(new EntitySet("Staff") { Root = staff }));
		}

		public void TestActiveDirectoryObjectGuidIgnored()
		{
			var staff = new Entity(TestUtil.FindEntityDefinition("Staff", "GlbStaff"), sessionServices) { Action = EntityAction.MERGE };
			staff["Code"] = "TS1";
			staff["LoginName"] = "TEST";
			staff["ActiveDirectoryObjectGuid"] = "14188456-F95B-493A-94FD-D93852437872";
			var entitySet = new EntitySet("Staff") { Root = staff };
			context.Update(entitySet);
			AssertEquals("ActiveDirectoryObjectGuid", "", entitySet.Root["ActiveDirectoryObjectGuid"]);
		}

		[UseSnapshotProtection]
		public void TestNativeXMLUserVisibleException_WhenSCIMEnabled()
		{
			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var factory = new BusinessObjectFactory();
				var boStaff = factory.NewWithValidTestData<GlbStaff>();
				boStaff.GS_ExternalId = "123456";
				factory.Save();

				var staff = new Entity(TestUtil.FindEntityDefinition("Staff", "GlbStaff"), sessionServices) { Action = EntityAction.MERGE };
				staff.InternalPK = boStaff.PK.ToGuid();
				staff["PK"] = boStaff.PK;
				staff["Code"] = "TS1";
				staff["LoginName"] = "TEST";

				AssertExceptionThrown(typeof(NativeXMLUserVisibleException), "Import Native XML is disabled when the Enable Scim Service Registry setting has been enabled.",
					() => context.Update(new EntitySet("Staff") { Root = staff }));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var setting = new StaffSetting();
			sessionServices = new AncillaryImportServices();
			var interceptor = new StaffInterceptor(setting, sessionServices);

			context = new UpdateContext(sessionServices, new FactoryProvider());
			context.InterceptorSettings.Add(setting);

			setting.Enable = true;
			setting.Context = context;
			setting.Interceptor = interceptor;
		}

		AncillaryImportServices sessionServices;
		IUpdateContext context;
	}
}
