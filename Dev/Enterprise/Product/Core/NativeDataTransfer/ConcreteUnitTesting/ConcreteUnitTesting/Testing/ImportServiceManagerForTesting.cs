using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class ImportServiceManagerForTesting
	{
		internal ImportServiceManagerForTesting()
		{
			session = new AncillaryImportServices();
			Logs = session.Logger as  MemoryLogger;

			ImportService = new ImportHandler(session)
			{
				BeforeProcess = BeforeProcess,
				BeforeUnitProcess = BeforeUnitProcess,
				AfterProcess = AfterProcess,
				AfterUnitProcess = AfterUnitProcess,
				UnitProcessSuccess = UnitProcessSuccess,
				ErrorOccur = ErrorOccur
			};
		}

		internal readonly ImportHandler ImportService;
		internal readonly MemoryLogger Logs;
		readonly AncillaryImportServices session;

		public string GetLogs()
		{
			return string.Join("\r\n", Logs.Buffer.Logs().Select(log => log.Message).ToArray());
		}

		void BeforeProcess()
		{
			Logs.Information("--- Start Import Process --------------------------------------------------------------");
		}

		void BeforeUnitProcess(XElement source)
		{
		}

		void AfterUnitProcess(XElement source)
		{
		}

		void UnitProcessSuccess(XElement source)
		{
			if (session.EntitiesReferencingPK.Count > 0)
			{
				throw new InvalidOperationException("EntitiesReferencingPK should be reset after each unit is processed.");
			}
			Logs.Information("Processed: " + source.Name);
		}

		void AfterProcess()
		{
			Logs.Information("--- Import Process Finished -----------------------------------------------------------");
		}

		void ErrorOccur(XElement source, Exception ex)
		{
			var exception = ex as ZDataException;
			if (exception != null)
			{
				Logs.Error("Record: " + source.Name + " failed to Import:\r\n" + exception.FriendlyMessage);
			}
			else if (ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex) != null)
			{
				Logs.Error("Record: " + source.Name + " failed to Import:\r\n" + ex.Message);
			}
			else
			{
				Logs.Error("Failed to Import:\r\n" + ex.Message);
			}
		}
	}
}
