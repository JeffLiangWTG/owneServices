using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DataTransferSwitchRegistryItem))]
	public class DataTransferSwitchRegistryItemTest : StronglyTypedRegistryItemTestCase<DataTransferSwitchRegistryBusinessObject>
	{
		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestCasting()
		{
			base.TestCasting();
		}

		public void TestUpdateLastRun()
		{
			ZDateTime testDate = new DateTime(2006, 1, 1, 1, 1, 50);
			DataTransferSwitchRegistryItem itemAtSystemLevel = new DataTransferSwitchRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
			DataTransferSwitchRegistryBusinessObject bizObj = ValidValue;
			itemAtSystemLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bizObj);
			AssertEquals("At system level", testDate, itemAtSystemLevel.Value.LastRunDateTime);
			itemAtSystemLevel.UpdateLastRun(testDate.AddDays(1));
			AssertEquals("At system level", testDate.AddDays(1), itemAtSystemLevel.Value.LastRunDateTime);
		}

		protected override DataTransferSwitchRegistryBusinessObject ValidValue
		{
			get
			{
				DataTransferSwitchRegistryBusinessObject bizObj = new DataTransferSwitchRegistryBusinessObject();
				bizObj.Directory = Env.TempPath;
				bizObj.Interval = 1;
				bizObj.LastRunDateTime = new DateTime(2006, 1, 1, 1, 1, 50);
				bizObj.NextRunDateTime = new DateTime(2006, 1, 2, 1, 1, 50);
				bizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
				bizObj.EnableInterface = true;

				return bizObj;
			}
		}

		protected override StronglyTypedRegistryItem<DataTransferSwitchRegistryBusinessObject, DataTransferSwitchRegistryBusinessObject> GetNewRegistryItem()
		{
			return new DataTransferSwitchRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		public static DataTransferSwitchRegistryBusinessObject GetNewValueWithValidDataForTesting(BusinessObjectFactory factory)
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(factory);
			newValue.Directory = TempForTest.TempPath;
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.EnableInterface = true;
			BusinessObject group = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbGroup)));
			BusinessObject staff = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbStaff)));
			staff[GlbStaffSchema.Constants.GS_EmailAddress] = "dummy@what.com";
			Assembly assembly = Assembly.Load("Enterprise.MasterFiles.Business");
			BusinessObject groupLink = factory.New(assembly.GetType("Enterprise.MasterFiles.Business.GlbGroupLink"));
			groupLink[GlbGroupLinkSchema.Constants.GK_GG] = group.PK;
			groupLink[GlbGroupLinkSchema.Constants.GK_GS] = staff.PK;
			newValue.GroupPK = group.PK;
			factory.Save();
			return newValue;
		}

		#region Tools
		public static void SetupRegistryWithValidDataForTesting(AutomaticProcessRegistryItemBase<DataTransferSwitchRegistryBusinessObject> item, BusinessObjectFactory factory)
		{
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetNewValueWithValidDataForTesting(factory));
		}

		public static void SetupRegistryWithValidDataForTesting(AutomaticProcessRegistryItemBase<DataTransferSwitchRegistryBusinessObject> item, Guid companyPK, Guid branchPK, Guid departmentPK, BusinessObjectFactory factory)
		{
			item.SetValue(companyPK, branchPK, departmentPK, GetNewValueWithValidDataForTesting(factory));
		}

		public static void UpdateValue(AutomaticProcessRegistryItemBase<DataTransferSwitchRegistryBusinessObject> item, DataTransferSwitchRegistryBusinessObject value)
		{
			UpdateValue(item, value, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public static void UpdateValue(AutomaticProcessRegistryItemBase<DataTransferSwitchRegistryBusinessObject> item, DataTransferSwitchRegistryBusinessObject value, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			item.SetValue(companyPK, branchPK, departmentPK, value);
		}
		#endregion
	}
}
