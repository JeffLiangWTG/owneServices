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
	class DocumentDbMergerController : ZSingletonController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentDbMerger; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DocumentDbMerger); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DocumentDbMergerForm((DocumentDbMerger)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			if (documentDbMerger == null)
			{
				documentDbMerger = new DocumentDbMerger(Factory as DocumentFactory);
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
