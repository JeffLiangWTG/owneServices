using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataWritingManager
	{
		IUniversalActionInfo Action { get; }
		IUniversalXmlSchema Schema { get; }
		IDataObjectWriterStrategy WriterStrategy { get; }
		bool PKAlreadyExported(ZGuid pk);
		void AddPK(ZGuid pk);
		IDisposable UseNewListForDuplicatePKCheck();
		void NotifyExported(IDataObject dataObject, BusinessObject businessObject);
		bool OverrideSendCostingData { get; set; }
		bool ShouldPopulateInternalMilestones { get; set; }
		DataContextType? FilteredDataContextType { get; set; }
		bool IsPublishingInternally { get; }
		IDisposable SetIsPublishingInternally();
		IEDIMessageContentFilterManager ContentFilterManager { get; }
	}
}
