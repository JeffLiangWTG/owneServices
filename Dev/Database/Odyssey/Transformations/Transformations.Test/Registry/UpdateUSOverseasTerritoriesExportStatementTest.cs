using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(UpdateUSOverseasTerritoriesExportStatement))]

	class UpdateUSOverseasTerritoriesExportStatementTest : MasterFiles.UpdateUsExportStatementRegistryDataTransformationTest
	{
		protected override string XmlBeforeTransformationName
		{
			get => "UpdateUSOverseasTerritoriesExportStatementTest.BeforeTransformation.xml";
		}

		protected override string XmlAfterTransformationName
		{
			get => "UpdateUSOverseasTerritoriesExportStatementTest.AfterTransformation.xml";
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateUSOverseasTerritoriesExportStatement();
		}
	}
}
