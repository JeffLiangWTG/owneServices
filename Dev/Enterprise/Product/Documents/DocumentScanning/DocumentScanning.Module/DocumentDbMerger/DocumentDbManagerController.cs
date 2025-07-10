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
	class DocumentDbManagerController : ZSingletonController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentDbManager; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DocumentDbManager); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DocumentDbManagerForm((DocumentDbManager)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			if (documentDbMerger == null)
			{
				documentDbMerger = new DocumentDbManager(Factory as DocumentFactory);
			}
			return documentDbMerger;
		}

		IBusiness documentDbMerger;

		protected override BusinessObjectFactory GetNewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}
	}
}
