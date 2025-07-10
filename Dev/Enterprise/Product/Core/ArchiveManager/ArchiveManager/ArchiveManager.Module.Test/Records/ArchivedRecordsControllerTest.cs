using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Module.Records;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Module.Test.Records
{
	[TestedType(typeof(ArchivedRecordsController))]
	class ArchiveRecordsControllerBasherTest : ZControllerBasherTest
	{
		protected override BusinessObjectFactory NewFactory()
			=> new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

		protected override ControllerID GetControllerID()
			=> ControllerIDs.ArchivedRecords;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
			=> archiveRecord;

		protected override void SetUp()
		{
			base.SetUp();
			archiveRecord = Factory.New<ArchiveStorageMain>();
			archiveRecord.SM_Archived = ZDateTime.Now;
			archiveRecord.SM_ParentFK = Guid.NewGuid();
			archiveRecord.SM_DB = 1;
			Factory.Save();
		}

		ArchiveStorageMain archiveRecord;
	}
}
