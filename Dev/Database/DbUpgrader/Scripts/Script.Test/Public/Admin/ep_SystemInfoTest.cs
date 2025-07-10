using System;
using System.Management;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_SystemInfo))]
	class ep_SystemInfoTest : DbCreateScriptTest
	{
		public void TestSystemInfo()
		{
			string osName;
			string osVersion;
			string systemManufacturer;
			string biosReleaseDate;
			long physicalMemoryMb;
			int numberOfProcessors;
			int numberOfLogicalProcessors;
			string processorDescription;
			string processorVendor;
			int processorSpeedMhz;
			bool isOrHasVirtualMachine;
			string virtualMachineType;

			using (var connection = Db.NewAdminConnection())
			using (var cmd = connection.Command("EXEC " + ScriptToTest.Name))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals("Must return a row => Read()?", true, reader.Read());

				osName = reader["OsName"].ToString();
				osVersion = reader["OsVersion"].ToString();
				systemManufacturer = reader["SystemManufacturer"].ToString();
				biosReleaseDate = reader["BiosReleaseDate"].ToString();
				physicalMemoryMb = Convert.ToInt64(reader["PhysicalMemoryMb"]);
				numberOfProcessors = Convert.ToInt32(reader["NumberOfProcessors"]);
				numberOfLogicalProcessors = Convert.ToInt32(reader["NumberOfLogicalProcessors"]);
				processorDescription = reader["ProcessorDescription"].ToString();
				processorVendor = reader["ProcessorVendor"].ToString();
				processorSpeedMhz = Convert.ToInt32(reader["ProcessorSpeedMhz"]);
				isOrHasVirtualMachine = Convert.ToBoolean(reader["IsOrHasVirtualMachine"]);
				virtualMachineType = reader["VirtualMachineType"].ToString();

				AssertEquals("Return only one row => Read()?", false, reader.Read());
			}

			AssertOsInfo(osName, osVersion);
			AssertSystemInfo(systemManufacturer, physicalMemoryMb, numberOfProcessors, numberOfLogicalProcessors, isOrHasVirtualMachine, virtualMachineType);

			if (!isOrHasVirtualMachine || (isOrHasVirtualMachine && !string.IsNullOrWhiteSpace(biosReleaseDate)))
			{
				AssertBiosInfo(biosReleaseDate);
			}
			AssertProcessorInfo(processorDescription, processorVendor, processorSpeedMhz);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1031:DoNotUseGetVersionExOrEnvironmentDotOSVersionRule", Justification = "Testing")]
		void AssertOsInfo(string osName, string osVersion)
		{
			AssertEquals("OsName", System.Environment.OSVersion.VersionString, osName);
			AssertEquals("OsVersion", System.Environment.OSVersion.Version.ToString(), osVersion);
		}

		void AssertSystemInfo(string systemManufacturer, long physicalMemoryMb, int numberOfProcessors, int numberOfLogicalProcessors, bool isOrHasVirtualMachine, string virtualMachineType)
		{
			var sysMgmt = new ManagementClass("Win32_ComputerSystem");
			var mgmtObjects = sysMgmt.GetInstances();
			var mgmtObjArray = new ManagementBaseObject[mgmtObjects.Count];
			mgmtObjects.CopyTo(mgmtObjArray, 0);
			var mgmtObj = (ManagementObject)mgmtObjArray[0];

			if (!isOrHasVirtualMachine || (isOrHasVirtualMachine && !string.IsNullOrWhiteSpace(systemManufacturer)))
			{
				AssertEquals("SystemManufacturer", mgmtObj["Manufacturer"].ToString(), systemManufacturer);
			}
			AssertEquals("PhysicalMemoryMb", Convert.ToInt64(mgmtObj["TotalPhysicalMemory"]) / (1024 * 1024), physicalMemoryMb);
			AssertEquals("IsOrHasVirtualMachine", Convert.ToBoolean(mgmtObj["HypervisorPresent"]), isOrHasVirtualMachine && string.Equals(virtualMachineType, "HYPERVISOR", StringComparison.OrdinalIgnoreCase));
			AssertEquals("NumberOfProcessors", Convert.ToInt32(mgmtObj["NumberOfProcessors"]), numberOfProcessors);
			AssertEquals("NumberOfLogicalProcessors", Convert.ToInt32(mgmtObj["NumberOfLogicalProcessors"]), numberOfLogicalProcessors);
		}

		void AssertBiosInfo(string biosReleaseDate)
		{
			var biosMgmt = new ManagementClass("Win32_BIOS");
			var mgmtObjects = biosMgmt.GetInstances();
			var mgmtObjArray = new ManagementBaseObject[mgmtObjects.Count];
			mgmtObjects.CopyTo(mgmtObjArray, 0);
			var mgmtObj = (ManagementObject)mgmtObjArray[0];

			string releaseDateWmi = mgmtObj["ReleaseDate"].ToString();
			string releaseYear = releaseDateWmi.Substring(0, 4);
			string releaseMonth = releaseDateWmi.Substring(4, 2);
			string releaseDay = releaseDateWmi.Substring(6, 2);

			Assert(string.Format("BiosReleaseDate Year: expected = {0}, actual string = {1}", releaseYear, biosReleaseDate),
				Regex.IsMatch(biosReleaseDate, string.Format(@"\b{0}\b", releaseYear)));
			Assert(string.Format("BiosReleaseDate Month: expected = {0}, actual string = {1}", releaseMonth, biosReleaseDate),
				Regex.IsMatch(biosReleaseDate, string.Format(@"\b{0}\b", releaseMonth)));
			Assert(string.Format("BiosReleaseDate Day: expected = {0}, actual string = {1}", releaseDay, biosReleaseDate),
				Regex.IsMatch(biosReleaseDate, string.Format(@"\b{0}\b", releaseDay)));
		}

		void AssertProcessorInfo(string processorDescription, string processorVendor, int processorSpeedMhz)
		{
			var cpuMgmt = new ManagementClass("Win32_Processor");
			var mgmtObjects = cpuMgmt.GetInstances();
			var mgmtObjArray = new ManagementBaseObject[mgmtObjects.Count];
			mgmtObjects.CopyTo(mgmtObjArray, 0);
			var mgmtObj = (ManagementObject)mgmtObjArray[0];

			AssertEquals("ProcessorDescription", mgmtObj["Description"].ToString(), processorDescription);
			AssertNotEquals("ProcessorSpeedMhz", 0, processorSpeedMhz); // Intel SpeedStep makes comparing two such obtained values into a race condition, so just check it's non-zero.
			AssertEquals("ProcessorVendor empty?", false, string.IsNullOrWhiteSpace(processorVendor));
		}
	}
}

