using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class IncidentAssociationSystemStatusTest : TestCaseWithFactory
	{
		public void DeleteAllStatusRowsTest()
		{
			// Arrange
			var stm1 = Factory.New<StmData>();
			stm1.SD_Name = IncidentAssociationSystemStatus.StmFieldName;

			var stm2 = Factory.New<StmData>();
			stm1.SD_Name = IncidentAssociationSystemStatus.StmFieldName;

			var stm3 = Factory.New<StmData>();
			stm1.SD_Name = IncidentAssociationSystemStatus.StmFieldName;

			Factory.Save();

			// Act
			IncidentAssociationSystemStatus.DeleteAllStatusRows(Factory);
			var result = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, IncidentAssociationSystemStatus.StmFieldName));

			// Assert
			Assert("There should be no results in StmData", result.Length == 0);
		}

		public void SetStatusFromEmptyTest()
		{
			// Arrange
			IncidentAssociationSystemStatus.DeleteAllStatusRows(Factory);

			var str = "oh hai";

			// Act
			IncidentAssociationSystemStatus.SetStatus(str, Factory);
			var result = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, IncidentAssociationSystemStatus.StmFieldName));

			// Assert
			Assert("Status should be set in StmData", Encoding.UTF8.GetString(result.SD_BinaryValue) == str);
		}

		public void SetStatusFromExistingTest()
		{
			// Arrange
			IncidentAssociationSystemStatus.DeleteAllStatusRows(Factory);

			var stm1 = Factory.New<StmData>();
			stm1.SD_Name = IncidentAssociationSystemStatus.StmFieldName;
			stm1.SD_BinaryValue = Encoding.UTF8.GetBytes("test");

			Factory.Save();

			var str = "oh hai";

			// Act
			IncidentAssociationSystemStatus.SetStatus(str, Factory);
			var result = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, IncidentAssociationSystemStatus.StmFieldName));

			// Assert
			Assert("Status should be set in StmData", Encoding.UTF8.GetString(result.SD_BinaryValue) == str);
		}

		public void GetStatusEmptyTest()
		{
			// Arrange
			IncidentAssociationSystemStatus.DeleteAllStatusRows(Factory);

			// Act
			// Assert
			AssertNull("No result so null should be returned", IncidentAssociationSystemStatus.GetStatus(Factory));
		}

		public void GetStatusFromExistingTest()
		{
			// Arrange
			IncidentAssociationSystemStatus.DeleteAllStatusRows(Factory);

			var str = "oh hai";

			var stm1 = Factory.New<StmData>();
			stm1.SD_Name = IncidentAssociationSystemStatus.StmFieldName;
			stm1.SD_BinaryValue = Encoding.UTF8.GetBytes(str);

			Factory.Save();

			// Act
			// Assert
			Assert("Status should be returned from dbo.StmData", IncidentAssociationSystemStatus.GetStatus(Factory) == str);
		}
	}
}
