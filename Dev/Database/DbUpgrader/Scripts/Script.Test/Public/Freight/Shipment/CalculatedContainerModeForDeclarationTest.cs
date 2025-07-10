using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(CalculatedContainerModeForDeclaration))]
	class CalculatedContainerModeForDeclarationTest : DbCreateScriptTest
	{
		public void TestCalculatedContainerMode()
		{
			declarationPK = Guid.NewGuid();
			var clusterKey = 1;
			TestConnection.ExecuteNonQuery(GetInsertDeclarationCommand(declarationPK, clusterKey));

			var calculatedContainerMode = (string)TestConnection.ExecuteScalar(string.Format("SELECT Value FROM dbo.CalculatedContainerModeForDeclaration('{0}', null)", declarationPK));
			AssertEquals("Container Mode should be 'LSE'", "LSE", calculatedContainerMode);

			TestConnection.ExecuteNonQuery(GetInsertContainerCommand("FCL", clusterKey));

			calculatedContainerMode = (string)TestConnection.ExecuteScalar(string.Format("SELECT Value FROM dbo.CalculatedContainerModeForDeclaration('{0}', null)", declarationPK));
			AssertEquals("Container Mode should be 'FCL'", "FCL", calculatedContainerMode);

			TestConnection.ExecuteNonQuery(GetInsertContainerCommand("FCL", clusterKey));

			calculatedContainerMode = (string)TestConnection.ExecuteScalar(string.Format("SELECT Value FROM dbo.CalculatedContainerModeForDeclaration('{0}', null)", declarationPK));
			AssertEquals("Container Mode should be 'FCL'", "FCL", calculatedContainerMode);

			TestConnection.ExecuteNonQuery(GetInsertContainerCommand("BLC", clusterKey));

			calculatedContainerMode = (string)TestConnection.ExecuteScalar(string.Format("SELECT Value FROM dbo.CalculatedContainerModeForDeclaration('{0}', null)", declarationPK));
			AssertEquals("Container Mode should be 'LCL'", "LCL", calculatedContainerMode);

			declaration2PK = Guid.NewGuid();
			shipmentPK = Guid.NewGuid();
			clusterKey = 2;

			TestConnection.ExecuteNonQuery(GetInsertDeclarationCommand(declaration2PK, clusterKey));
			TestConnection.ExecuteNonQuery(GetInsertShipmentCommand());
			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobDeclaration
SET
	JE_JS = '{0}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{1}'", shipmentPK, declaration2PK));

			calculatedContainerMode = (string)TestConnection.ExecuteScalar(string.Format("SELECT Value from dbo.CalculatedContainerModeForDeclaration('{0}', '{1}')", declaration2PK, shipmentPK));
			AssertEquals("Container Mode should be from shipment - BLK", "BLK", calculatedContainerMode);
		}
		Guid declarationPK;
		Guid declaration2PK;
		Guid shipmentPK;

		string GetInsertDeclarationCommand(Guid declarationPK, int clusterKey)
		{
			Guid importerPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");
			Guid branchPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			Guid companyPK = (Guid)TestConnection.ExecuteScalar($"SELECT GB_GC FROM dbo.GlbBranch WHERE GB_PK = '{branchPK}'");
			string declarationRef = Guid.NewGuid().ToString("n");

			return string.Format(@"INSERT INTO dbo.JobDeclaration
           ([JE_PK]
           ,[JE_DataModel]
           ,[JE_OH_Importer]
           ,[JE_IsCancelled]
           ,[JE_OverrideFreightDefaults]
           ,[JE_MessageType]
           ,[JE_MessageSubType]
           ,[JE_RS_NKServiceLevel]
           ,[JE_IsPersonalEffects]
           ,[JE_DeclarationReference]
           ,[JE_RL_NKOrigin]
           ,[JE_RL_NKPortOfLoading]
           ,[JE_ExportDate]
           ,[JE_RL_NKPortOfFirstArrival]
           ,[JE_DateOfFirstArrival]
           ,[JE_RL_NKPortOfArrival]
           ,[JE_DateOfArrival]
           ,[JE_RL_NKFinalDestination]
           ,[JE_DateAtFinalDestination]
           ,[JE_VesselName]
           ,[JE_LloydsIMO]
           ,[JE_VoyageFlightNo]
           ,[JE_TransportMode]
           ,[JE_AddInfo]
           ,[JE_ContainerMode]
           ,[JE_GB]
           ,[JE_GC]
           ,[JE_ClusterKey])
     VALUES
           ('{0}'
           ,'!!'
           ,'{1}'
           ,0
           ,0
           ,'IMP'
           ,'FRM'
           ,'STD'
           ,0
           ,'{5}'
           ,'CNSHA'
           ,'CNSHA'
           ,'2010-10-15 00:00:00'
           ,'AUSYD'
           ,'2010-10-15 00:00:00'
           ,'AUSYD'
           ,'2010-10-15 00:00:00'
           ,'AUCNS'
           ,'2010-10-15 00:00:00'
           ,'APL IVORY'
           ,''
           ,'5841'
           ,'SEA'
           ,''
           ,'FCL'
           ,'{2}'
           ,'{3}'
           ,{4})", declarationPK, importerPK, branchPK, companyPK, clusterKey, declarationRef);
		}

		string GetInsertContainerCommand(string containerMode, int clusterKey)
		{
			return string.Format(@"INSERT INTO dbo.CusContainer
           ([CO_PK]
           ,[CO_ContainerNumber]
           ,[CO_Seal]
           ,[CO_FCL_LCL_AIR]
           ,[CO_ContainerSize]
           ,[CO_ContainerUQ]
           ,[CO_Weight]
           ,[CO_WeightUQ]
           ,[CO_AddInfo]
           ,[CO_CustomFlag1]
           ,[CO_CustomAttrib1]
           ,[CO_CustomDecimal1]
           ,[CO_JE]
           ,[CO_SecondSeal]
           ,[CO_MessageStatus]
           ,[CO_ClusterKey]
           ,[CO_DataModel])
     VALUES
           (NEWID()
           ,'CNT12456'
           ,''
           ,'{0}'
           ,''
           ,''
           ,0
           ,''
           ,''
           ,''
           ,''
           ,0
           ,'{1}'
           ,''
           ,''
           ,{2}
           ,'ZZ')", containerMode, declarationPK, clusterKey);
		}

		string GetInsertShipmentCommand()
		{
			Guid importerPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");

			return string.Format(@"INSERT INTO dbo.JobShipment
           ([JS_PK]
           ,[JS_IsCancelled]
           ,[JS_PackingMode])
     VALUES
           ('{0}'
           ,0
           ,'BLK')
", shipmentPK);
		}
	}
}

