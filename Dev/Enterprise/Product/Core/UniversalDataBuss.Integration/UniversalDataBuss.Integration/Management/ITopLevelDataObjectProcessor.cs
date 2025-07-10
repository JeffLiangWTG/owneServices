using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataMessageFactory
	{
		IDataMessageProcessor DataMessageProcessor { get; }
		ITopLevelDataObjectFactory TopLevelDataObjectFactory { get; }
		ITopLevelDataObjectProcessor TopLevelDataObjectProcessor { get; }
		IUserContextExtractor UserContextExtractor { get; }
		IUserContextScopeManager UserContextScopeManager { get; }
	}

	public interface IDataMessageProcessor
	{
		MessageStatus Process(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager codeMapper = null);
	}

	public enum MessageStatus
	{
		Rejected,
		Discarded,
		Processed,
		Linked
	}

	public interface IMessageKeyProvider
	{
		MessageKeyProviderResult GetKeysForBlockingParallelImport(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager codeMapper = null);
	}

	public struct MessageKeyProviderResult
	{
		public MessageKeyProviderResult(MessageStatus status)
		{
			ShouldShortCircuit = true;
			Status = status;
			KeysInfo = Enumerable.Empty<(string KeyValue, string KeySource)>();
		}

		public MessageKeyProviderResult(IEnumerable<(string KeyValue, string KeySource)> keysInfo)
		{
			KeysInfo = keysInfo.Where(RemovingFillerCharactersResultsInAValidKey).ToArray();
			ShouldShortCircuit = false;
			Status = MessageStatus.Processed;
		}

		static bool RemovingFillerCharactersResultsInAValidKey((string KeyValue, string KeySource) stringToVerify)
		{
			return !string.IsNullOrEmpty(NonKeyFillerCharactersRegex.Replace(stringToVerify.KeyValue, string.Empty));
		}
		static readonly Regex NonKeyFillerCharactersRegex = new Regex(@"[!@#$%^*()\-_=+[\]{}\\|;:'"" /?,.<>]", RegexOptions.Compiled);

		public bool ShouldShortCircuit { get; }
		public MessageStatus Status { get; }
		public IEnumerable<(string KeyValue, string KeySource)> KeysInfo { get; }
		public IEnumerable<string> Keys => KeysInfo.Select(k => k.KeyValue);
	}

	public interface ITopLevelDataObjectFactory
	{
		bool TryGetTopLevelDataObject(IEDIMessage message, ICodeMappingManager codeMapper, IXmlSessionTracker logger, out ITopLevelDataObject topLevelDataObject);
	}

	public interface IRecipientBranchLocator
	{
		ZGuid GetBranchPK(IEDIMessage message, ITopLevelDataObject topLevelDataObject, IGlbCompany glbCompany, IXmlSessionTracker logger);
		ZBool UseEventBranchToDecideImportCompany { get; }
	}

	public interface IDepartmentLocator
	{
		(bool active, IGlbDepartment department) TryGetActiveDepartment(IEDIMessage message, IDataContextDataObject contextDataObject);
	}

	public interface IUserContextExtractor
	{
		bool TryGetUserContext(IEDIMessage message, ITopLevelDataObject topLevelDataObject, IXmlSessionTracker logger, out IUserContext userContext);
	}

	public interface IUserContextScopeManager
	{
		IDisposable EnterUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
	}

	public interface ITopLevelDataObjectProcessor
	{
		MessageStatus ProcessDataObject(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger);
		MessageKeyProviderResult GetKeys(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger);
		void ValidateDataObject(ITopLevelDataObject dataObject, IXmlSessionTracker logger);
	}

	public interface IEDIMessageUniversalObjectFactoryLocator
	{
		IUniversalObjectFactory GetFactory(IEDIMessage message, IXmlSessionTracker logger);
	}
}
