using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class WorkItemCollectionProvider : CollectionProviderWithCodeSupport
	{
		public WorkItemCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
			workItemCollectionType = Assembly.Load("Enterprise.ProcessManagement.Business").GetType("Enterprise.ProcessManagement.Business.WorkItemCollection");
			workItemType = Assembly.Load("Enterprise.ProcessManagement.Business").GetType("Enterprise.ProcessManagement.Business.WorkItem");
		}
		readonly Type workItemCollectionType;
		readonly Type workItemType;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IActiveBusinessObjectCollection)Activator.CreateInstance(workItemCollectionType, new object[2] { BusinessObjectFactory, new AdhocCollectionRelationship(workItemType) });
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return (IActiveBusinessObjectCollection)Activator.CreateInstance(workItemCollectionType, new object[2] { BusinessObjectFactory, Filter });
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WorkItem;

		public override int MaxLength => WorkItemSchema.WKI_WorkItemNumber.MaxLength;
	}
}
