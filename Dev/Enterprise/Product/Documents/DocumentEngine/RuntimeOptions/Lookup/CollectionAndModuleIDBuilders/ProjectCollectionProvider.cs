using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class ProjectCollectionProvider : CollectionProviderWithCodeSupport
	{
		public ProjectCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
			projectCollectionType = Assembly.Load("Enterprise.ProcessManagement.Business").GetType("Enterprise.ProcessManagement.Business.ProjectCollection");
			projectType = Assembly.Load("Enterprise.ProcessManagement.Business").GetType("Enterprise.ProcessManagement.Business.Project");
		}
		readonly Type projectCollectionType;
		readonly Type projectType;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return (IActiveBusinessObjectCollection)Activator.CreateInstance(projectCollectionType, new object[2] { BusinessObjectFactory, new AdhocCollectionRelationship(projectType) });
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return (IActiveBusinessObjectCollection)Activator.CreateInstance(projectCollectionType, new object[2] { BusinessObjectFactory, Filter });
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Project;

		public override int MaxLength => WorkProjectSchema.WKP_ProjectNumber.MaxLength;
	}
}
