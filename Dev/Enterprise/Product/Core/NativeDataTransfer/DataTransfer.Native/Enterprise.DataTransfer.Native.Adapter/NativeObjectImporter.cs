using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeObjectImporter : INativeObjectImporter
	{
		readonly ImportHandler handler;
		readonly StringBuilder logger;

		public NativeObjectImporter()
		{
			logger = new StringBuilder();
			handler = new ImportHandler(new AncillaryImportServices())
			{
				BeforeProcess = BeforeProcess,
				BeforeUnitProcess = BeforeUnitProcess,
				AfterProcess = AfterProcess,
				AfterUnitProcess = AfterUnitProcess,
				UnitProcessSuccess = UnitProcessSuccess,
				ErrorOccur = ErrorOccur
			};
		}

		public bool AlwaysUseProvidedPKs
		{
			get { return handler.AlwaysUseProvidedPKs; }
			set { handler.AlwaysUseProvidedPKs = value; }
		}

		public void Import(Stream stream, out string errorLog)
		{
			logger.Clear();
			handler.Import(stream);
			errorLog = logger.ToString();
		}

		public void Import(Stream stream, out string errorLog, string[] assemblyNames)
		{
			handler.SetDefinitionFinder(assemblyNames);
			Import(stream, out errorLog);
		}

		void ErrorOccur(XElement source, Exception ex)
		{
			ZDataException zdex = ex as ZDataException;
			if (zdex != null)
			{
				var message = String.IsNullOrEmpty(zdex.FriendlyMessage) || zdex.FriendlyMessage.Length < 2 ? zdex.Message : zdex.FriendlyMessage;
				logger.AppendLine(Res.GetString("7ab41165-3f41-42d2-9747-46ffa11a7387", "Record: {0} failed to Import:\r\n {1}", source.Name, message));
			}
			else if (ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex) != null)
			{
				logger.AppendLine(Res.GetString("b1da5587-5d67-4756-8729-4c81116585f4", "Record: {0} failed to Import:\r\n{1}", source.Name, ex.Message));
			}
			else
			{
				logger.AppendLine(Res.GetString("9dd1ff92-7e19-4b7d-a351-76f01ba97ab8", "Failed to Import:\r\n{0}", ex.Message));
			}
		}

		void BeforeProcess()
		{
		}

		void BeforeUnitProcess(XElement source)
		{
		}

		void AfterUnitProcess(XElement source)
		{
		}

		void UnitProcessSuccess(XElement source)
		{
		}

		void AfterProcess()
		{
		}
	}
}
