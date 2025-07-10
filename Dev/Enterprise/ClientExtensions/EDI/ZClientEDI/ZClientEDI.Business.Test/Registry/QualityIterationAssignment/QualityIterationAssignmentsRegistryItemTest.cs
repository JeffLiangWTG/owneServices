using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(QualityIterationAssignmentRegistryItem))]
	class QualityIterationAssignmentsRegistryItemTest : StronglyTypedRegistryItemTestCase<QualityIterationAssignmentHeader>
	{
		public void TestDefaultValue()
		{
			var header = new QualityIterationAssignmentHeader { IsDefaultOptionSelected = ZBool.True };
			header.AddNewAssignment("AAA", ZBool.True);
			header.AddNewAssignment("BBB", ZBool.False);

			var item = new QualityIterationAssignmentRegistryItem("", null, null, null, RegistryStorageFlags.System, header);

			Assert(item.DefaultValue.IsDefaultOptionSelected);
			AssertEquals(2, item.DefaultValue.AssignmentCollection.Count);
		}

		protected override StronglyTypedRegistryItem<QualityIterationAssignmentHeader, QualityIterationAssignmentHeader> GetNewRegistryItem()
		{
			return new QualityIterationAssignmentRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
