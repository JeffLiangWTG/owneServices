using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupportEdwDataSourceReportRegistryDataType))]
	public class SupportEdwDataSourceReportRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SupportEdwDataSourceReportRegistryDataType>
	{
		protected override SupportEdwDataSourceReportRegistryDataType GetNewDataType()
		{
			return new SupportEdwDataSourceReportRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var report1 = factory.New<IStmMenuItem>();
			report1.SU_IsPublished = true;
			report1.SU_BusinessContext = "RepAReport";
			report1.SU_MenuName = "Test Report1";
			report1.SU_IsSystemDefined = true;

			var report2 = factory.New<IStmMenuItem>();
			report2.SU_IsPublished = true;
			report2.SU_BusinessContext = "RepAReport";
			report2.SU_MenuName = "Test Report2";
			report2.SU_IsSystemDefined = true;

			factory.Save();

			var collection1 = new SupportEdwDataSourceReportCollection();

			var collection2 = new SupportEdwDataSourceReportCollection();
			var supportEdwDataSourceReport1 = collection2.AddNew();
			supportEdwDataSourceReport1.ReportName = report1.SU_MenuName;
			supportEdwDataSourceReport1.BusinessContext = report1.SU_BusinessContext;
			var supportEdwDataSourceReport2 = collection2.AddNew();
			supportEdwDataSourceReport2.ReportName = report2.SU_MenuName;
			supportEdwDataSourceReport2.BusinessContext = report2.SU_BusinessContext;

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2)),
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "SupportEdwDataSourceReportRegistryItemEditor"; }
		}
	}
}
