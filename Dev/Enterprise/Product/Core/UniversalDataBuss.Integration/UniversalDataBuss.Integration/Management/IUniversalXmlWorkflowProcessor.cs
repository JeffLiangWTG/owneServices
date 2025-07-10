using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalXmlWorkflowProcessor : IProcessor
	{
		new IXmlEventValueObject[] Process(INotifications notifications, CancellationToken cancellationToken);

		void AddAdditionalTriggerParty(ZString partyCode, ZString partyService);
	}

	public interface IUniversalXmlWorkflowProvider
	{
		IUniversalXmlWorkflowProcessor GetUniversalXmlWorkflowProcessor(IUniversalActionInfo actionInfo, IMessageProcessorCommunicationModesResult communicationModesGetter, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null, IXmlWriter xmlWriter = null, IUniversalXmlSchema schema = null);
	}

	public interface IUniversalXmlCommunicationModeProvider
	{
		IList<IEDICommunicationsMode> CommunicationModes { get; }
		MultilingualString ReasonForNoCommunicationModes { get; }
	}

	public static class UniversalXmlWorkflowProcessorBuilder
	{
		public static IUniversalXmlWorkflowProcessor New(IUniversalActionInfo actionInfo, IMessageProcessorCommunicationModesResult communicationModesGetter, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null, IXmlWriter xmlWriter = null, IUniversalXmlSchema schema = null)
		{
			var provider = ObjectFactory.Get<IUniversalXmlWorkflowProvider>();
			return provider.GetUniversalXmlWorkflowProcessor(actionInfo, communicationModesGetter, dataWriterGetter, exportedBO, eventInfo, xmlWriter, schema);
		}
	}
}
