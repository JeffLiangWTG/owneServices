using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	public class NctsEventParentFinder : EventParentFinder
	{
		public NctsEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
		{
			ZString GetDataTargetValue(DataContextType contextType)
			{
				var dataTargetKey = xmlEvent.DataContext.GetMatchingDataTarget(contextType)?.Key;
				return dataTargetKey ?? ZString.Empty;
			}

			BusinessObject[] logParents = null;

			var nctsJobNumber = GetDataTargetValue(DataContextType.NctsHeader);
			if (!nctsJobNumber.IsEmpty)
			{
				logParents = factory.Load<NctsHeader>(new ZQuery(CusInBondHeaderSchema.BH_JobReference, nctsJobNumber));
			}

			return logParents;
		}
	}
}
