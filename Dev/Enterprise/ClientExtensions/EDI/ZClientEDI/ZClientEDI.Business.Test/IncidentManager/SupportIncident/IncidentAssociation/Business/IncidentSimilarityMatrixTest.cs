using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class IncidentSimilarityMatrixTest : TestCaseWithFactory
	{
		public void TestIncidentSimilarityMatrix_GetOtherIncident()
		{
			//Arrange 
			var incidentSimilarityMatrix = Factory.New<IncidentSimilarityMatrix>();
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			//Act
			incidentSimilarityMatrix.ISM_IM_Incident1 = guid1;
			incidentSimilarityMatrix.ISM_IM_Incident2 = guid2;
			Factory.Save();
			//Assert
			AssertEquals(null, incidentSimilarityMatrix.GetOtherIncident(guid2));
			AssertEquals(null, incidentSimilarityMatrix.GetOtherIncident(guid1));
		}

		public void TestIncidentSimilarityMatrix_ThrowExcepOnunknowenGuids()
		{
			//Arrange 
			var incidentSimilarityMatrix = Factory.New<IncidentSimilarityMatrix>();
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var guid3 = ZGuid.NewZGuid();
			//Act
			incidentSimilarityMatrix.ISM_IM_Incident1 = guid1;
			incidentSimilarityMatrix.ISM_IM_Incident2 = guid2;
			Factory.Save();
			//Assert
			AssertExceptionThrown<ArgumentException>(() => incidentSimilarityMatrix.GetOtherIncident(guid3));
		}
		public void TestIncidentSimilarityMatrix_ThrowExcepOnSameGuids()
		{
			//Arrange 
			var incidentSimilarityMatrix = Factory.New<IncidentSimilarityMatrix>();
			var guid1 = ZGuid.NewZGuid();
			//Act
			incidentSimilarityMatrix.ISM_IM_Incident1 = guid1;
			incidentSimilarityMatrix.ISM_IM_Incident2 = guid1;
			Factory.Save();
			//Assert
			AssertExceptionThrown<InvalidOperationException>(() => incidentSimilarityMatrix.GetOtherIncident(guid1));
		}
	}
}
