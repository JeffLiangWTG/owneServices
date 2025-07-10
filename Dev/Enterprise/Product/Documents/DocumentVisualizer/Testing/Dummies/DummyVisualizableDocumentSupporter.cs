using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyVisualizableDocumentSupporter : IVisualizableDocumentSupporter
	{
		public DummyVisualizableDocumentSupporter(DummyWithUXmlSupport dummy)
		{
			this.dummy = dummy;
		}

		readonly DummyWithUXmlSupport dummy;

		public object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj)
		{
			if (bizObj is DummyWithUXmlSupport bizo)
			{
				var result = factory.ImportFromAnotherFactory(bizo) as DummyWithUXmlSupport;

				result.SubType1 = bizo.SubType1;

				return result;
			}

			return default;
		}
		public object GetEventParent(IXmlEventValueObject universalEvent) => default;

		public ISecurityCheckpoint CustomizeFormCheckpoint { get; }

		public IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;
		public IMessageLogCreator GetMessageLogCreator(IDocument document) => null;
		public IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			if (document?.Data?.Value is DocDataObjects.Address)
			{
				return new DummyMessagingExtensions(document);
			}

			return dummy.MessagingExtensions;
		}

		public IEnumerable<IMacroLibrary> GetLibraries(string dataContext)
		{
			yield break;
		}

		public static IDisposable TempSetGetCustomCommandsImpl(Func<string, IEnumerable<ICommand>> impl)
		{
			getCustomCommandsImpl = impl;
			return new DisposableAction(() => getCustomCommandsImpl = null);
		}

		[ThreadStatic]
		static Func<string, IEnumerable<ICommand>> getCustomCommandsImpl;

		public IEnumerable<ICommand> GetCustomCommands(string dataContext) => getCustomCommandsImpl?.Invoke(dataContext) ?? Enumerable.Empty<ICommand>();

		public static IDisposable TempSetGetDocDataObjectImpl(Func<object, string, IDocDataObjectParameters, Either<string, object>> impl)
		{
			getDocDataObjectImpl = impl;
			return new DisposableAction(() => getDocDataObjectImpl = null);
		}

		[ThreadStatic]
		static Func<object, string, IDocDataObjectParameters, Either<string, object>> getDocDataObjectImpl;

		public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			if (getDocDataObjectImpl != null)
			{
				return getDocDataObjectImpl.Invoke(parent, dataContext, parameters);
			}

			if (dataContext == "supported")
			{
				return new object();
			}

			return "data context is not supported";
		}

		public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			return new Shipment(DefaultDataObjectWriterStrategy.Instance)
			{
				DataContext = new DataContext
				{
					DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
					{
						Type = "DummyBusinessObject",
						Key = "K001"
					}
				},
				WayBillNumber = "12345"
			};
		}

		public Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => new Either<string, object>((object)null);

		public string GetMessageBroker() => dummy.MessageBroker;

		public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
		{
			var result = new List<IDocument>();

			if (dummy.Z0_AnotherNumber == 123)
			{
				var dynamicData = document?.Data?.Value is DocDataObjects.Address ? new DocDataObjects.Address(dummy.Factory)
				{
					CompanyName = "CargoWise"
				}.MakeDynamic() :
				new object().MakeDynamic();

				result.Add(new EmptyDocument(messageInstructions.DocumentName, messageInstructions.DataContext, dynamicData));
			}

			if (dummy.SubType1 == "123")
			{
				var dynamicData = document?.Data?.Value is DocDataObjects.Address ? new DocDataObjects.Address(dummy.Factory)
				{
					CompanyName = "WiseTechNext"
				}.MakeDynamic() :
				new object().MakeDynamic();

				result.Add(new EmptyDocument(messageInstructions.DocumentName, messageInstructions.DataContext, dynamicData));
			}

			if (dummy.LoadPort == "AUSYD")
			{
				result.Add(new EmptyDocument(messageInstructions.DocumentName, messageInstructions.DataContext, new object().MakeDynamic()));
			}

			return result;
		}

		public bool ShouldUseDraftWatermark(IDocument document) => this.dummy.ShouldUseDraftWatermark;
	}

	class DummyMessagingExtensions : IMessagingExtensions
	{
		public DummyMessagingExtensions(IDocument document)
		{
			this.document = document;
		}
		readonly IDocument document;

		public bool? ContinueWithSendingMessage(IUserNotifications notifications) => null;
		public bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => null;
		public bool? GetRequireMessageAmendmentReason() => null;
		public bool? IsSendingAmendment() => null;
		public ICodeDescriptionPairList GetAmendmentOptions() => null;
		public bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => null;
		public ICodeDescriptionPairList GetWithdrawalOptions() => null;
		public bool? ContinueWithResetToOriginal(IUserNotifications notifications) => null;
		public string GetMessageStatus() => null;
		public KeyValuePair<string, string>[] GetAdditionalParametersForEvent() => null;
		public bool? ShowEvents() => null;
		public bool? ShowLastEventDetails() => null;

		public string GetXmlNamespace()
		{
			if (document?.Data?.Value is DocDataObjects.Address address)
			{
				return address.CompanyName;
			}

			return null;
		}

		public string GetDocumentaryOverrideDocumentName()
		{
			if (document?.Data?.Value is DocDataObjects.Address address && address.CompanyName != "WiseTech")
			{
				return address.CompanyName;
			}

			return null;
		}
	}
}
