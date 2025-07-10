using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public abstract class ActivityDataContextManager<T> : EventDataContextManager<T>, IActivityDataContextManager
		where T : BusinessObject, IJobNumber
	{
		public bool ManagesActivities => true;

		public bool DoesManageDataContextType(DataContextType type)
		{
			return DataContextType == type;
		}

		public override ZString DataContextKey => ParentBO.JobNumber;

		public abstract ITopLevelDataObjectWriter GetActivityDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedActivities);
		public abstract ITopLevelDataObjectReader GetActivityDataObjectReader(ITopLevelDataObject activity, IXmlImportLogger logger, IUniversalObjectFactory factory);

		#region Universal Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ActivityEventParentFinder(factory, this, logger);
		}

		class ActivityEventParentFinder : EventParentFinder
		{
			public ActivityEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
			{
				return null;
			}
		}

		#endregion
	}
}
