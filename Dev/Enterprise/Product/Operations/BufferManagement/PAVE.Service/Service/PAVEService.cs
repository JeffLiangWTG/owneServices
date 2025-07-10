using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Service
{
	public abstract class PAVEService : IPAVEService
	{
		void IPAVEService.Process(IEnumerable<Guid> processedPKs, ILogger logger, string callerMemberName)
		{
			Process(processedPKs, logger, callerMemberName);
		}

		protected void Process(IEnumerable<Guid> processedPKs, ILogger logger, [CallerMemberName] string callerMemberName = "")
		{
			var pks = processedPKs?.ToImmutableArray();
			if (!VerifyParameters(pks, logger, callerMemberName))
			{
				return;
			}

			try
			{
				ProcessCore(pks, logger);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var stringTransferablePKs = processedPKs != null ? string.Join(",", processedPKs.Select(g => g.ToString())) : (NoResString)"none"; // Log information
				logger?.Information(FormattableString.Invariant($"Error when call {GetClassAndMethodNames(callerMemberName)}, transferablePKs: {stringTransferablePKs} Exception: {ex.Message}")); // Log information
			}
		}

		bool VerifyParameters(IReadOnlyCollection<Guid> processedPKs, ILogger logger, string callerMemberName)
		{
			var bufferManagementEnabled = ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled;

			if (!bufferManagementEnabled)
			{
				logger?.Information($"Attempt to run {GetClassAndMethodNames(callerMemberName)} with Registry bufferManagement disabled"); // Log information

				return false;
			}

			if (processedPKs == null || !processedPKs.Any())
			{
				logger?.Information($"Attempt to run {GetClassAndMethodNames(callerMemberName)} without processed item PKs"); // Log information

				return false;
			}

			return true;
		}

		protected abstract void ProcessCore(IEnumerable<Guid> processedPKs, ILogger logger);

		string GetClassAndMethodNames(string callerMemberName) => FormattableString.Invariant($"{GetType().Name}.{callerMemberName}"); // Log information
	}
}
