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
	public class ArchiveEDocsController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.ArchiveEDocs; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ArchiveEDocsManager); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ArchiveEDocsForm((ArchiveEDocsManager)businessEntity);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ArchiveEDocsManager(Factory as DocumentFactory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ArchiveEDocs; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}
	}
}
