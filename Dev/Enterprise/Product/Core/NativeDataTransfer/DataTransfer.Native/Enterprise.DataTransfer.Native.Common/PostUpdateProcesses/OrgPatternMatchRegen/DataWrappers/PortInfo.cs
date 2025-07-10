using CargoWise.Types;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	class PortInfo
	{
		internal PortInfo(ZString countryName, ZString portName)
		{
			this.CountryName = countryName;
			this.PortName = portName;
		}

		internal readonly ZString CountryName;
		internal readonly ZString PortName;
	}
}
