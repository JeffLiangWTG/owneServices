using Enterprise.ArchiveManager.Module.Records;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Module.Test.Records
{
	[TestedType(typeof(ArchivedRecordsFilterBusinessObject))]
	class ArchivedRecordsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			=> new ArchivedRecordsFilterBusinessObject();

		protected override bool ShouldBeLocalizable
			=> false;
	}
}
