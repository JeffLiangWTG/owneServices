using Enterprise.ArchiveManager.Module.Schedule;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Module.Test.Schedule
{
	[TestedType(typeof(ArchiveScheduleFilterBusinessObject))]
	class ArchiveScheduleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			=> new ArchiveScheduleFilterBusinessObject();
	}
}
