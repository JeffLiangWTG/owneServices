using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Data;
using Enterprise.Faxing.Integration;
using Enterprise.Integration;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxConfigSettings : XmlSerializableSetting, IFaxConfig
	{
		public FaxConfigSettings()
		{
			LocalCountry = "";
			LocalAreaCode = "";
			LocalID = "";
			OutsideLinePrefix = "";
			InternationalCallPrefix = "";
			Ports = System.Array.Empty<FaxPortConfigSettings>();
		}

		public string LocalCountry { get; set; }
		public string LocalAreaCode { get; set; }
		public string LocalID { get; set; }
		public string OutsideLinePrefix { get; set; }
		public string InternationalCallPrefix { get; set; }
		[XmlElement("Port")]
		public IReadOnlyList<FaxPortConfigSettings> Ports { get; set; }
		public bool EnableLogging { get; set; }

		#region IFaxConfig Members

		IReadOnlyList<IFaxPortConfig> IFaxConfig.Ports
		{
			get { return Ports; }
		}

		IFaxLogger IFaxConfig.GetLogger()
		{
			return _faxLogger;
		}

		string IFaxConfig.LogDir
		{
			get { return ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName); }
		}

		#endregion

		#region Logger

		public void SetLogger(ILogger logger)
		{
			_faxLogger = new FaxLogger(logger);
		}

		IFaxLogger _faxLogger;

		class FaxLogger : IFaxLogger
		{
			public FaxLogger(ILogger logger)
			{
				_logger = logger;
			}

			public void Log(FaxLogType type, string message)
			{
				_logger.Log((LogType)(int)type, message);
			}

			readonly ILogger _logger;
		}

		#endregion
	}

	public class FaxPortConfigSettings : XmlSerializableSetting, IFaxPortConfig
	{
		public FaxPortConfigSettings()
		{
			PortName = "";
		}

		[XmlElement("Name")]
		public string PortName { get; set; }
	}
}
