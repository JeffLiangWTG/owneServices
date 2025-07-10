using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BranchImportContextService : IService
	{
		BranchImportContextService(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public static BranchImportContextService GetInstance(BusinessObjectFactory factory)
		{
			BranchImportContextService service = factory.ServiceContainer.GetService<BranchImportContextService>();

			if (service == null)
			{
				service = new BranchImportContextService(factory);
				factory.ServiceContainer.AddService(service);
			}
			return service;
		}

		public bool IsInBranchContext
		{
			get { return BranchContext != null; }
		}

		public GlbBranch BranchContext { get; private set; }

		IDisposable tempBranchContext;

		public void Set(GlbBranch branch)
		{
			if (branch != null && !IsInBranchContext)
			{
				if (branch.PK != GlbBranch.CurrentBranch.PK)
				{
					tempBranchContext = branch.SetAsTemporaryContext();
				}

				factory.Saved += OnFactorySaved;
				BranchContext = branch;
			}
		}

		void OnFactorySaved(BusinessObjectFactory savedFactory, bool savedSuccessfully)
		{
			if (tempBranchContext != null)
			{
				tempBranchContext.Dispose();
				tempBranchContext = null;
			}

			factory.ServiceContainer.RemoveService<BranchImportContextService>();
			factory.Saved -= OnFactorySaved;
			BranchContext = null;
		}
	}
}
