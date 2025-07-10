using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class REXNotificationEventParentFinder : EventParentFinder
	{
		public REXNotificationEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
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

			var rexNumber = GetDataTargetValue(DataContextType.REXNotification);
			if (!rexNumber.IsEmpty && GetDataTargetValue(DataContextType.CustomsDeclaration).IsEmpty)
			{
				logParents = factory.Load<QuarantineNexDocNotification>(new ZQuery(QuarantineNexDocNotificationSchema.QN_RexNumber, rexNumber));
			}

			return logParents;
		}
	}
}
