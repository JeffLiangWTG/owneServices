using System;
using System.Linq;
using System.Net.Sockets;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using Enterprise.Dat.Implementation.Preconditions;

namespace CargoWise.Bi.BusinessIntelligence.Testing
{
	public static class DATHelper
	{
		public static string GetSomeOtherRandomDATVM()
		{
			var random = new Random();
			var searcher = new DirectorySearcherWrapper(domainName: "sand.wtg.zone");
			var computers = searcher.FindAllComputers().Select(m => m.Name).Where(n => n.EndsWith("-DAT1") && !n.Equals(Environment.MachineName)).ToArray();
			string computer;
			do
			{
				computer = computers[random.Next(computers.Length)];
			}
			while (!DnsResolves(computer));
			return computer;
		}

		static bool DnsResolves(string computer)
		{
			try
			{
				return System.Net.Dns.GetHostAddresses(computer + ".sand.wtg.zone").Any();
			}
			catch (SocketException)
			{
				return false;
			}
		}

		static public string GetVMWithPowerBi()
		{
			var vmName = string.Empty;
			while (string.IsNullOrEmpty(vmName))
			{
				vmName = DATHelper.GetSomeOtherRandomDATVM() + ".sand.wtg.zone";
				if (!VmHasPowerBiService(vmName))
				{
					vmName = string.Empty;
				}
			}
			return vmName;
		}

		static bool VmHasPowerBiService(string vmName)
		{
			try
			{
				if (!PowerBiCheck.IsPowerBiWorking(vmName))
				{
					return false;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return false; //Just to avoid any crash
			}
			return true;
		}
	}
}
