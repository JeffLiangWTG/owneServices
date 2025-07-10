using System.Collections.Generic;

namespace Enterprise.Faxing.Integration
{
	public interface IFaxConfig
	{
		string LocalCountry { get; }
		string LocalAreaCode { get; }
		string LocalID { get; }
		string OutsideLinePrefix { get; }
		//string LongDistanceCallPrefix { get; }
		string InternationalCallPrefix { get; }
		IReadOnlyList<IFaxPortConfig> Ports { get; }

		bool EnableLogging { get; }
		string LogDir { get; }
		IFaxLogger GetLogger();
	}

	public interface IFaxPortConfig
	{
		string PortName { get; }
		//int OpenPortSleepTimeInMilliSeconds { get; }
	}
}
