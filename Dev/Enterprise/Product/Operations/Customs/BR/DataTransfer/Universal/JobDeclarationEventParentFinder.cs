using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryHeader = Enterprise.Customs.BR.Business.CusEntryHeader;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class JobDeclarationEventParentFinder : Customs.DataTransfer.Universal.JobDeclarationEventParentFinder
	{
		public JobDeclarationEventParentFinder(BusinessObjectFactory factory, JobDeclarationDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContextCore(Event eventDataObject)
		{
			var entries = base.GetLogParentsForEventUsingContextCore(eventDataObject);
			entries?.OfType<CusEntryHeader>().ForEach(entry =>
			{
				if (ShouldUpdateEntryStatus(entry.CH_MessageType, entry.Declaration.JE_ApplicationCode))
				{
					UpdateEntryStatus(entry, eventDataObject);
				}
			});
			return entries;
		}

		protected override ZString GetDeclarationReference(ZString declarationReference)
		{
			return declarationReference.Split('-')[0];
		}

		bool ShouldUpdateEntryStatus(ZString messageType, ZString applicationCode) => applicationCode == DeclarationApplicationCodeList.Codes.Builtin && (ShouldUpdateISWEntryStatus(messageType) || ShouldUpdateLICEntryStatus(messageType));

		bool ShouldUpdateISWEntryStatus(ZString messageType) => messageType == MessageTypeList.Codes.ISW && BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_ISW.Value;

		bool ShouldUpdateLICEntryStatus(ZString messageType) => messageType == MessageTypeList.Codes.LIC && BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_LIC.Value;
	}
}
