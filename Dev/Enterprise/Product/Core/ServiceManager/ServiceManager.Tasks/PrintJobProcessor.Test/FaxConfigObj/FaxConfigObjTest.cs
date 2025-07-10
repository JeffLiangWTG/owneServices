using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(FaxConfigObj))]
	sealed class FaxConfigObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestToFromXml()
		{
			IServiceTaskSchedule taskSchedule = Factory.New<IServiceTaskSchedule>();
			FaxConfigObj configObj1 = new FaxConfigObj(taskSchedule);
			configObj1.LocalCountry = "AU";
			configObj1.LocalAreaCode = "2";
			configObj1.LocalID = "1111-2222";
			configObj1.OutsideLinePrefix = "9";
			configObj1.InternationalCallPrefix = "0011";
			configObj1.Ports.AddNew().PortName = "COM1";
			configObj1.Ports.AddNew().PortName = "COM3";
			configObj1.EnableLogging = true;
			Factory.Save();

			FaxConfigObj configObj2 = new FaxConfigObj(taskSchedule);
			AssertEquals("AU", configObj2.LocalCountry);
			AssertEquals("2", configObj2.LocalAreaCode);
			AssertEquals("1111-2222", configObj2.LocalID);
			AssertEquals("9", configObj2.OutsideLinePrefix);
			AssertEquals("0011", configObj2.InternationalCallPrefix);
			AssertEquals(2, configObj2.Ports.Count);
			AssertEquals("COM1", configObj2.Ports[0].PortName);
			AssertEquals("COM3", configObj2.Ports[1].PortName);
			AssertEquals(true, configObj2.EnableLogging);
			Assert(!configObj2.HasChanges);
		}

		public void TestInternationalCallPrefix()
		{
			IServiceTaskSchedule taskSchedule = Factory.New<IServiceTaskSchedule>();
			FaxConfigObj configObj1 = new FaxConfigObj(taskSchedule);
			configObj1.LocalCountry = "AU";
			AssertEquals("0011", configObj1.InternationalCallPrefix);
			configObj1.LocalCountry = "US";
			AssertEquals("011", configObj1.InternationalCallPrefix);
			configObj1.InternationalCallPrefix = "0015";
			configObj1.LocalCountry = "AU";
			AssertEquals("0015", configObj1.InternationalCallPrefix);
			configObj1.LocalCountry = "HK";
			AssertEquals("001", configObj1.InternationalCallPrefix);
		}

		public void TestLocalCountry()
		{
			IServiceTaskSchedule taskSchedule = Factory.New<IServiceTaskSchedule>();
			FaxConfigObj configObj1 = new FaxConfigObj(taskSchedule);
			AssertEquals(taskSchedule.GetBranchCountryCode(), configObj1.LocalCountry);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FaxConfigObj(Factory.New<IServiceTaskSchedule>());
		}

		#endregion
	}
}
