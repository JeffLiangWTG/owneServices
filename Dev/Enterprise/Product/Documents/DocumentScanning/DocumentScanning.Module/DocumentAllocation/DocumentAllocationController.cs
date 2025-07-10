using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	public class DocumentAllocationController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ZAllocateDocumentsForm((AllocateDocumentsManager)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AllocateDocuments; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentAllocation; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AllocateDocumentsManager); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			if (AllocateDocumentsManager == null)
			{
				AllocateDocumentsManager = new AllocateDocumentsManager(Factory as DocumentFactory);
			}
			return AllocateDocumentsManager;
		}

		IBusiness AllocateDocumentsManager;

		protected override BusinessObjectFactory GetNewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return Env.Security.AllocateDocumentsModify.IsAllowed ? ODisplayMode.Browse : ODisplayMode.ReadOnly;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DocumentAllocation; }
		}
	}
}
