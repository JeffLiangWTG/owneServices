using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.GUI.Records;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ArchiveManager.Module.Records
{
	public class ArchivedRecordsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany
			=> true;

		protected override SecurityCheckpoint CheckPointForDelete
			=> Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit
			=> Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew
			=> Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView
			=> Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var entity = (ArchiveStorageMain)businessEntity;
			return new ArchivedRecordForm(entity);
		}

		public override ControllerID ID
			=> ControllerIDs.ArchivedRecords;

		public override Type TypeOfTopLevelBusinessObject
			=> typeof(ArchiveStorageMain);

		protected override BusinessObjectFactory GetNewFactory()
			=> new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
			=> factory is DocumentFactory
				? factory.Load(TypeOfTopLevelBusinessObject, sourceEntityPK)
				: (IBusiness)new DocumentFactoryProvider().GetFactory(factory).Load(TypeOfTopLevelBusinessObject, sourceEntityPK);

		public override ModuleIdentifier ModuleID
			=> ModuleIDs.ArchivedRecords;
	}
}
