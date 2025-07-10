using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.GUI.Records;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	[TestedType(typeof(ArchivedRecordForm))]
	class ArchivedRecordFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
			=> new ArchivedRecordForm(Factory.New<ArchiveStorageMain>());

		public override bool AllowUntranslatableFormTitle()
			=> true;

		protected override BusinessObjectFactory NewFactory()
			=> new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
	}
}
