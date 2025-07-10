using System.Linq;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoicingDefaultGatewayDepartmentsRegistryDataType))]
	class JobInvoicingDefaultGatewayDepartmentsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<JobInvoicingDefaultGatewayDepartmentsRegistryDataType>
	{
		protected override JobInvoicingDefaultGatewayDepartmentsRegistryDataType GetNewDataType()
		{
			return new JobInvoicingDefaultGatewayDepartmentsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "JobInvoicingDefaultGatewayDepartmentsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new JobInvoicingDefaultGatewayDepartmentsCollection();
			var entry1 = collection.AddNew();
			entry1.Direction = "IMP";
			entry1.TransportMode = "AIR";
			entry1.ConsolType = "AGT";
			entry1.Department = entry1.Departments.Single(x => x.GE_Code == "GIA").PK;

			var entry2 = collection.AddNew();
			entry2.Direction = "IMP";
			entry2.TransportMode = "SEA";
			entry2.ConsolType = "AGT";
			entry2.Department = entry2.Departments.Single(x => x.GE_Code == "GIS").PK;

			var collection2 = new JobInvoicingDefaultGatewayDepartmentsCollection();
			var entry3 = collection2.AddNew();
			entry3.Direction = "EXP";
			entry3.TransportMode = "SEA";
			entry3.ConsolType = "CLD";
			entry3.Department = entry3.Departments.Single(x => x.GE_Code == "GES").PK;

			var entry4 = collection2.AddNew();
			entry4.Direction = "EXP";
			entry4.TransportMode = "AIR";
			entry4.ConsolType = "CLD";
			entry4.Department = entry4.Departments.Single(x => x.GE_Code == "GEA").PK;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}
	}
}
