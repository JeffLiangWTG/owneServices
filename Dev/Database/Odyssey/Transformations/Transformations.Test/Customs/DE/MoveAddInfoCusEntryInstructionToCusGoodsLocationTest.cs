using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.DE
{
	[TestedType(typeof(MoveAddInfoCusEntryInstructionToCusGoodsLocation))]
	class MoveAddInfoCusEntryInstructionToCusGoodsLocationTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [IX_MoveCusEntryInstructionAddInfoZG_QualifierOfIdentificationAndZG_LoadingPlaceCodeToCusGoodsLocation_CEI_DataModel] ON [dbo].[CusEntryInstruction] ([CEI_DataModel]) INCLUDE ([CEI_AddInfo], [CEI_PK]) WHERE ([CEI_DataModel]='DE' AND [CEI_AddInfo]<>'') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Qualifier 'V' mapped", "V", (string)Db.Connection.Command($"SELECT CGL_Qualifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK7}'").ExecuteScalar());
				AssertEquals("Qualifier 'V', Type mapped", "A", (string)Db.Connection.Command($"SELECT CGL_Type FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK7}'").ExecuteScalar());
				AssertEquals("Qualifier 'V', CEI_AddInfo cleaned", "PartyConstellation=0001", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK7}'").ExecuteScalar());

				AssertEquals("Qualifier 'Y' mapped", "Y", (string)Db.Connection.Command($"SELECT CGL_Qualifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK2}'").ExecuteScalar());
				AssertEquals("Qualifier 'Y', Type mapped", "B", (string)Db.Connection.Command($"SELECT CGL_Type FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK2}'").ExecuteScalar());
				AssertEquals("Qualifier 'Y', LoadingPlaceCode mapped", "ABC", (string)Db.Connection.Command($"SELECT CGL_AdditionalIdentifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK2}'").ExecuteScalar());
				AssertEquals("Qualifier 'Y', CEI_AddInfo cleaned", "PartyConstellation=0000", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK2}'").ExecuteScalar());
				AssertEquals("Qualifier 'Y', AddInfo Not Moved for Not DE Company", "QualifierOfIdentification=Y*LoadingPlaceCode=ABC", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK4}'").ExecuteScalar());

				var companyZ = Db.Connection.Command($"SELECT E2_CompanyName FROM dbo.JobDocAddress " +
					$"INNER JOIN CusGoodsLocation ON CGL_PK = E2_ParentID " +
					$"WHERE CusGoodsLocation.CGL_ParentID = '{entryInstructionPK3}'");
				var addressZ = Db.Connection.Command($"SELECT E2_Address1 FROM dbo.JobDocAddress " +
					$"INNER JOIN CusGoodsLocation ON CGL_PK = E2_ParentID " +
					$"WHERE CusGoodsLocation.CGL_ParentID = '{entryInstructionPK3}'");
				AssertEquals("Qualifier 'Z' mapped", "Z", (string)Db.Connection.Command($"SELECT CGL_Qualifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK3}'").ExecuteScalar());
				AssertEquals("Qualifier 'Z', Type mapped", "D", (string)Db.Connection.Command($"SELECT CGL_Type FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK3}'").ExecuteScalar());
				AssertEquals("Qualifier 'Z', Company name mapped", "Test Org Company", (string)companyZ.ExecuteScalar());
				AssertEquals("Qualifier 'Z', Company address mapped", "Org Address Street 1", (string)addressZ.ExecuteScalar());
				AssertEquals("Qualifier 'Z', CEI_AddInfo cleaned", "", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK3}'").ExecuteScalar());

				var companyZOverridden = Db.Connection.Command($"SELECT E2_CompanyName FROM dbo.JobDocAddress " +
					$"INNER JOIN CusGoodsLocation ON CGL_PK = E2_ParentID " +
					$"WHERE CusGoodsLocation.CGL_ParentID = '{entryInstructionPK10}'");
				var addressZOverridden = Db.Connection.Command($"SELECT E2_Address1 FROM dbo.JobDocAddress " +
					$"INNER JOIN CusGoodsLocation ON CGL_PK = E2_ParentID " +
					$"WHERE CusGoodsLocation.CGL_ParentID = '{entryInstructionPK10}'");
				AssertEquals("Qualifier 'Z' mapped", "Z", (string)Db.Connection.Command($"SELECT CGL_Qualifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK10}'").ExecuteScalar());
				AssertEquals("Qualifier 'Z', Type mapped", "D", (string)Db.Connection.Command($"SELECT CGL_Type FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK10}'").ExecuteScalar());
				AssertEquals("Qualifier 'Z', Company name mapped", "Pickup Company", (string)companyZOverridden.ExecuteScalar());
				AssertEquals("Qualifier 'Z', Company address mapped", "Pickup Address 1", (string)addressZOverridden.ExecuteScalar());
				AssertEquals("Qualifier 'Z', CEI_AddInfo cleaned", "PartyConstellation=1100", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK10}'").ExecuteScalar());

				AssertEquals("Qualifier 'U' mapped", "U", (string)Db.Connection.Command($"SELECT CGL_Qualifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK8}'").ExecuteScalar());
				AssertEquals("Qualifier 'U', Type mapped", "D", (string)Db.Connection.Command($"SELECT CGL_Type FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK8}'").ExecuteScalar());
				AssertEquals("Qualifier 'U', UNLOCO mapped", "DEBER", (string)Db.Connection.Command($"SELECT CGL_AdditionalIdentifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK8}'").ExecuteScalar());
				AssertEquals("Qualifier 'U', CEI_AddInfo cleaned", "PartyConstellation=0100", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK8}'").ExecuteScalar());

				var addressW = Db.Connection.Command($"SELECT E2_GeoLocation FROM dbo.JobDocAddress " +
					$"INNER JOIN CusGoodsLocation ON CGL_PK = E2_ParentID " +
					$"WHERE CusGoodsLocation.CGL_ParentID = '{entryInstructionPK9}'");
				AssertEquals("Qualifier 'W' mapped", "W", (string)Db.Connection.Command($"SELECT CGL_Qualifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK9}'").ExecuteScalar());
				AssertEquals("Qualifier 'W', Type mapped", "D", (string)Db.Connection.Command($"SELECT CGL_Type FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK9}'").ExecuteScalar());
				AssertEquals("Qualifier 'W', GeoLocation mapped", "POINT (-122.36 47.616)", addressW.ExecuteScalar().ToString());
				AssertEquals("Qualifier 'W', CEI_AddInfo cleaned", "PartyConstellation=0100", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK9}'").ExecuteScalar());

				AssertEquals("Only 1 CusGoodsLocationExist", 1, (int)TestConnection.Command($"SELECT COUNT(*) FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK5}'").ExecuteScalar());
				AssertEquals("CusGoodsLocation is an existing one", cusGoodsLocationPK1, (Guid)TestConnection.Command($"SELECT CGL_PK FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK5}'").ExecuteScalar());
				AssertEquals("CGL_Gualifier updated", "Y", (string)TestConnection.Command($"SELECT CGL_Qualifier FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK5}'").ExecuteScalar());
				AssertEquals("CGL_Type updated", "B", (string)TestConnection.Command($"SELECT CGL_Type FROM dbo.CusGoodsLocation WHERE CGL_ParentID = '{entryInstructionPK5}'").ExecuteScalar());
				AssertEquals("AddInfo updated", "PartyConstellation=0000", (string)Db.Connection.Command($"SELECT CEI_AddInfo FROM CusEntryInstruction WHERE CEI_PK = '{entryInstructionPK5}'").ExecuteScalar());

				AssertEquals("Only 1 JobDocAddress mapped to CusGoodsLocation", 1, (int)TestConnection.Command($"SELECT COUNT(*) FROM dbo.JobDocAddress WHERE E2_ParentID = '{cusGoodsLocationPK2}'").ExecuteScalar());
				AssertEquals("JobDocAddress updated", "Berlin", (string)TestConnection.Command($"SELECT E2_City FROM dbo.JobDocAddress WHERE E2_ParentID = '{cusGoodsLocationPK2}'").ExecuteScalar());
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new MoveAddInfoCusEntryInstructionToCusGoodsLocation();

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE
				@deCompanyPK UNIQUEIDENTIFIER = NEWID(),
				@brCompanyPK UNIQUEIDENTIFIER = NEWID(),

				@deBranchPK UNIQUEIDENTIFIER = NEWID(),
				@brBranchPK UNIQUEIDENTIFIER = NEWID(),
				
				@orgHeaderPK UNIQUEIDENTIFIER = NEWID(),
				@orgAddressPK UNIQUEIDENTIFIER = NEWID(),

				@deDeclarationPK1 UNIQUEIDENTIFIER = NEWID(),
				@deDeclarationPK2 UNIQUEIDENTIFIER = NEWID(),
				@brDeclarationPK UNIQUEIDENTIFIER = NEWID(),

				@EntryInstructionPK1 UNIQUEIDENTIFIER = '{entryInstructionPK1}',
				@EntryInstructionPK2 UNIQUEIDENTIFIER = '{entryInstructionPK2}',
				@EntryInstructionPK3 UNIQUEIDENTIFIER = '{entryInstructionPK3}',
				@EntryInstructionPK4 UNIQUEIDENTIFIER = '{entryInstructionPK4}',
				@EntryInstructionPK5 UNIQUEIDENTIFIER = '{entryInstructionPK5}',
				@EntryInstructionPK6 UNIQUEIDENTIFIER = '{entryInstructionPK6}',
				@EntryInstructionPK7 UNIQUEIDENTIFIER = '{entryInstructionPK7}',
				@EntryInstructionPK8 UNIQUEIDENTIFIER = '{entryInstructionPK8}',
				@EntryInstructionPK9 UNIQUEIDENTIFIER = '{entryInstructionPK9}',
				@EntryInstructionPK10 UNIQUEIDENTIFIER = '{entryInstructionPK10}',

				@cusGoodsLocationPK1 UNIQUEIDENTIFIER = '{cusGoodsLocationPK1}',
				@cusGoodsLocationPK2 UNIQUEIDENTIFIER = '{cusGoodsLocationPK2}'

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser)	VALUES
					(@deCompanyPK, 'DE', 'DDE', 'DE company', GetUtcDate(), GetUtcDate(), 'E', 'E'),
					(@brCompanyPK, 'BR', 'DBR', 'BR company', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@deBranchPK, @deCompanyPK, 'BER', GetUtcDate(), GetUtcDate(), 'E', 'E'),
					(@brBranchPK, @brCompanyPK, 'BRN', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES
					(@orgHeaderPK, 'DDE', 'Test Org Company', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Address2, OA_RN_NKCountryCode, OA_City, OA_PostCode, OA_RL_NKRelatedPortCode, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) VALUES
					(@orgAddressPK, @orgHeaderPK, 'Org Address Street 1', 'Teststreet 2', 'DE', 'Berlin', '5555',  'DEBER', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES
					(@deDeclarationPK1, 'DE', @deBranchPK, @deCompanyPK, 10000, 'B00001', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@deDeclarationPK2, 'DE', @deBranchPK, @deCompanyPK, 12000, 'B00002', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@brDeclarationPK, 'BR', @brBranchPK, @brCompanyPK, 11000, 'C00003', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusEntryInstruction(CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_AddInfo, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES
					(@EntryInstructionPK1, 'DE', @deDeclarationPK1, 10000, 'PartyConstellation=0000', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK2, 'DE', @deDeclarationPK1, 1, 'QualifierOfIdentification=Y*PartyConstellation=0000*LoadingPlaceCode=ABC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK3, 'DE', @deDeclarationPK1, 5000, 'QualifierOfIdentification=Z', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK4, 'BR', @brDeclarationPK, 8000, 'QualifierOfIdentification=Y*LoadingPlaceCode=ABC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK5, 'DE', @deDeclarationPK1, 3000, 'PartyConstellation=0000*QualifierOfIdentification=Y*LoadingPlaceCode=ABC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK6, 'DE', @deDeclarationPK1, 1000, 'PartyConstellation=0000*QualifierOfIdentification=Z', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK7, 'DE', @deDeclarationPK1, 1000, 'PartyConstellation=0001*QualifierOfIdentification=V', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK8, 'DE', @deDeclarationPK1, 1000, 'QualifierOfIdentification=U*PartyConstellation=0100', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK9, 'DE', @deDeclarationPK1, 0100, 'PartyConstellation=0100*QualifierOfIdentification=W', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@EntryInstructionPK10, 'DE', @deDeclarationPK2, 2, 'PartyConstellation=1100*QualifierOfIdentification=Z', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_Address1, E2_Contact, E2_City, E2_State, E2_RN_NKCountryCode, E2_OA_Address, E2_AddressOverride ,E2_ValidationStatus, E2_CompanyName, E2_GeoLocation, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser) VALUES
					(NEWID(), @deDeclarationPK1, 'JE', 'SUG', 'Address 1', 'NLK1', 'Mainz', 'RLP', 'DE', @orgAddressPK, 0, 'NRQ', 'Supplier Company', geography::Point(47.616, -122.360, 4326), GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(NEWID(), @deDeclarationPK2, 'JE', 'SUG', 'Pickup Address 1', 'NLK1', 'Mainz', 'RLP', 'DE', @orgAddressPK, 1, 'INV', 'Pickup Company', geography::Point(47.616, -122.360, 4326), GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(NEWID(), @deDeclarationPK1, 'JE', 'EXP', 'Address 1', 'NLK2', 'Wiesbaden', 'HE', 'DE', NULL, 0, 'NRQ', 'Exporter Company', geography::Point(47.616, -122.360, 4326), GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(NEWID(), @cusGoodsLocationPK2, 'CGL', 'LOC', 'Address 1', 'NLK2', 'Test', 'RLP', 'DE', NULL, 0, 'NRQ', 'Test Company', geography::Point(47.616, -122.360, 4326), GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusGoodsLocation(CGL_PK, CGL_ParentID, CGL_ParentTableCode, CGL_Qualifier, CGL_Type, CGL_LocationUSe, CGL_SystemCreateTimeUtc, CGL_SystemCreateUser, CGL_SystemLastEditTimeUtc, CGL_SystemLastEditUser) VALUES
					(@cusGoodsLocationPK1, @EntryInstructionPK5, 'CEI', '', 'A', 'XYZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@cusGoodsLocationPK2, @EntryInstructionPK6, 'CEI', 'Z', 'D', 'CEI', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";

			TestConnection.ExecuteNonQuery(sql);
		}

		Guid entryInstructionPK1 = Guid.NewGuid();
		Guid entryInstructionPK2 = Guid.NewGuid();
		Guid entryInstructionPK3 = Guid.NewGuid();
		Guid entryInstructionPK4 = Guid.NewGuid();
		Guid entryInstructionPK5 = Guid.NewGuid();
		Guid entryInstructionPK6 = Guid.NewGuid();
		Guid entryInstructionPK7 = Guid.NewGuid();
		Guid entryInstructionPK8 = Guid.NewGuid();
		Guid entryInstructionPK9 = Guid.NewGuid();
		Guid entryInstructionPK10 = Guid.NewGuid();

		Guid cusGoodsLocationPK1 = Guid.NewGuid();
		Guid cusGoodsLocationPK2 = Guid.NewGuid();
	}
}
