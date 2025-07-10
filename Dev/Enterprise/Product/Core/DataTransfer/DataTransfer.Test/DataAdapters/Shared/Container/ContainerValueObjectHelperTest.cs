using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class ContainerValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestImportContainerType()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Xsd.ContainerType containerType = new Xsd.ContainerType();
			containerType.IsSpecified = true;

			CreateContainer("ABCD", "1234");
			CreateContainer("DEFG", "1234");
			CreateContainer("WXYZ", "9876");
			CreateContainer("LMNO", "3344");

			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);
			ContainerValueObjectHelper helper = new ContainerValueObjectHelper(context);

			containerType.ISOCode = "9876";
			helper.ImportContainerType(dummy.Z0_GuidInfo, containerType);
			AssertEquals("WXYZ", Factory.Load<RefContainer>(dummy.Z0_Guid).RC_Code);
			AssertEquals(false, notifications.HasErrors);

			dummy.Z0_Guid = ZGuid.Empty;
			containerType.ContainerCode = "LMNO";
			containerType.ISOCode = "";
			helper.ImportContainerType(dummy.Z0_GuidInfo, containerType);
			AssertEquals("LMNO", Factory.Load<RefContainer>(dummy.Z0_Guid).RC_Code);
			AssertEquals(false, notifications.HasErrors);

			dummy.Z0_Guid = ZGuid.Empty;
			containerType.ContainerCode = "";
			containerType.ISOCode = "1234";
			helper.ImportContainerType(dummy.Z0_GuidInfo, containerType);
			AssertNull(Factory.Load<RefContainer>(dummy.Z0_Guid));
			AssertEquals(true, notifications.HasErrors);
			notifications.Clear();

			dummy.Z0_Guid = ZGuid.Empty;
			containerType.ContainerCode = "";
			containerType.ISOCode = "4123";
			helper.ImportContainerType(dummy.Z0_GuidInfo, containerType);
			AssertNull(Factory.Load<RefContainer>(dummy.Z0_Guid));
			AssertEquals(true, notifications.HasErrors);
			notifications.Clear();

			dummy.Z0_Guid = ZGuid.Empty;
			containerType.ContainerCode = "DEFG";
			containerType.ISOCode = "";
			helper.ImportContainerType(dummy.Z0_GuidInfo, containerType);
			AssertEquals("DEFG", Factory.Load<RefContainer>(dummy.Z0_Guid).RC_Code);
			AssertEquals(false, notifications.HasErrors);
		}

		void CreateContainer(string containerCode, string iSOCode)
		{
			RefContainer container = Factory.New<RefContainer>();
			container.RC_Code = containerCode;
			container.RC_ISOType = iSOCode;
			Factory.Save();
		}
	}
}
