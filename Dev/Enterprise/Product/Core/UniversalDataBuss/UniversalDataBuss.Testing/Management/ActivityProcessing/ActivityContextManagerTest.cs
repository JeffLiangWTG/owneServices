using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Testing.Management.ActivityProcessing
{
	class ActivityContextManagerTest : TransactionedTestCase
	{
		public void TestEventContextValues()
		{
			var manager = new DummyActivityContextManager();
			AssertContainsExactElementsInAnyOrder("Universal Events for Universal Activities don't contain any extra information, so this colleciton should be empty. SAD!", Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>(), manager.EventContextValues);
		}

		public void TestEventParentFinder()
		{
			var universalEvent = new Event();
			var manager = (IEventDataContextManager)(new DummyActivityContextManager());
			AssertNull("Universal Events for Universal Activities don't contain any extra information, so this colleciton should be null. SAD!", manager.GetLogParentsForEvent(universalEvent, new BusinessObjectFactory(), new DummyLogger()));
		}

		#region Implementation

		class DummyActivityContextManager : ActivityDataContextManager<DummyWithWorkflow>
		{
			public override DataContextType DataContextType { get; }

			public override string DefaultOutputDirectory { get; }

			protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
			{
				return null;
			}

			public override ITopLevelDataObjectWriter GetActivityDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedActivities)
			{
				return null;
			}

			public override ITopLevelDataObjectReader GetActivityDataObjectReader(ITopLevelDataObject activity, IXmlImportLogger logger, IUniversalObjectFactory factory)
			{
				return null;
			}
		}

		#endregion
	}
}
