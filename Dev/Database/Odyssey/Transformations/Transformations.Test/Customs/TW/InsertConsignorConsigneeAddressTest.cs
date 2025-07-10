using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Customs.TW.Testing
{
	[TestedType(typeof(InsertConsignorConsigneeAddress))]
	class InsertConsignorConsigneeAddressTest : DataTransformationTestCase
	{
		public override void TestNewIndex()
		{
			new TransformationTestDataCreator().CreateGlbCompany("~TW", "TW");
			base.TestNewIndex();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Insert Consignor And Consignee To JobDocAddress_1] ON [dbo].[JobDeclaration] ([JE_DataModel]) INCLUDE ([JE_OH_Consignee], [JE_OH_Exporter], [JE_PK]) WHERE ([JE_DataModel]='TW') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<string, string, string>>();

			var sql = string.Format(@"
			SELECT
				E2_AddressType, E2_OA_Address, E2_GovRegNumType, E2_GovRegNum
			FROM
				dbo.JobDocAddress
			WHERE
				E2_AddressType	IN ('CRA', 'CEA')");
			TestConnection.ExecuteReader(sql, reader => resultList.Add(Tuple.Create((string)reader["E2_AddressType"], (string)reader["E2_GovRegNumType"], (string)reader["E2_GovRegNum"])));

			AssertContainsExactElementsInAnyOrder(new[]
			{
				Tuple.Create("CRA", "VAT", "VAT11"),
				Tuple.Create("CRA", "PID", "PID12"),
				Tuple.Create("CRA", "PAS", "PAS13"),
				Tuple.Create("CRA", "FFF", "FFFCPW19"),
				Tuple.Create("CRA", "FFF", "FFFCCP110"),
				Tuple.Create("CRA", "FFF", "FFFCPW19"),
				Tuple.Create("CRA", "FFF", "FFFEPZ12345"),
				Tuple.Create("CRA", "FFF", "FFFEPZ14"),
				Tuple.Create("CRA", "FFF", "FFFCBF15"),
				Tuple.Create("CRA", "FFF", "FFFFTZ16"),
				Tuple.Create("CRA", "FFF", "FFFATP17"),
				Tuple.Create("CRA", "FFF", "FFFSPK18"),
				Tuple.Create("CEA", "VAT", "VAT21"),
				Tuple.Create("CEA", "PID", "PID22"),
				Tuple.Create("CEA", "PAS", "PAS23"),
				Tuple.Create("CEA", "FFF", "FFFEPZ12345"),
				Tuple.Create("CEA", "FFF", "FFFCPW210"),
				Tuple.Create("CEA", "FFF", "FFFCCC24"),
				Tuple.Create("CEA", "FFF", "FFFCBF12345"),
				Tuple.Create("CEA", "FFF", "FFFEPZ25"),
				Tuple.Create("CEA", "FFF", "FFFCCP211"),
				Tuple.Create("CEA", "FFF", "FFFCBF26"),
				Tuple.Create("CEA", "FFF", "FFFFTZ27"),
				Tuple.Create("CEA", "FFF", "FFFATP28"),
				Tuple.Create("CEA", "FFF", "FFFSPK29"),
			}, resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new InsertConsignorConsigneeAddress();

		protected override void PrepareTestData()
		{
			var creator = new TransformationTestDataCreator();
			var twCompanyPK = creator.CreateGlbCompany("~TW", "TW");
			var twBranchPK = creator.CreateGlbBranch("~TW", twCompanyPK);

			(var vatConsignor, var vatConsignoraddress) = CreateVATOrgForConsignorAddress(creator);
			(var pidConsignor, var pidConsignoraddress) = CreatePIDOrgForConsignorAddress(creator);
			(var pasConsignor, var pasConsignoraddress) = CreatePASOrgForConsignorAddress(creator);
			(var epzConsignor, var epzConsignoraddress) = CreateEPZOrgForConsignorAddress(creator);
			(var cbfConsignor, var cbfConsignoraddress) = CreateCBFOrgForConsignorAddress(creator);
			(var ftzConsignor, var ftzConsignoraddress) = CreateFTZOrgForConsignorAddress(creator);
			(var atpConsignor, var atpConsignoraddress) = CreateATPOrgForConsignorAddress(creator);
			(var spkConsignor, var spkConsignoraddress) = CreateSPKOrgForConsignorAddress(creator);
			(var vatConsignee, var vatConsigneeaddress) = CreateVATOrgForConsigneeAddress(creator);
			(var pidConsignee, var pidConsigneeaddress) = CreatePIDOrgForConsigneeAddress(creator);
			(var pasConsignee, var pasConsigneeaddress) = CreatePASOrgForConsigneeAddress(creator);
			(var cccConsignee, var cccConsigneeaddress) = CreateCCCOrgForConsigneeAddress(creator);
			(var epzConsignee, var epzConsigneeaddress) = CreateEPZOrgForConsigneeAddress(creator);
			(var cbfConsignee, var cbfConsigneeaddress) = CreateCBFOrgForConsigneeAddress(creator);
			(var ftzConsignee, var ftzConsigneeaddress) = CreateFTZOrgForConsigneeAddress(creator);
			(var atpConsignee, var atpConsigneeaddress) = CreateATPOrgForConsigneeAddress(creator);
			(var spkConsignee, var spkConsigneeaddress) = CreateSPKOrgForConsigneeAddress(creator);
			var cpwFromWarehouseAddress = CreateCPWOrgForFromWarehouse(creator);
			var ccpFromWarehouseAddress = CreateCCPOrgForFromWarehouse(creator);
			var cpwToWarehouseAddress = CreateCPWOrgForToWarehouse(creator);

			CreateJobDeclaration("TW", twBranchPK, twCompanyPK, vatConsignor, vatConsignee, "IMP", 1, "1");
			CreateJobDeclaration("TW", twBranchPK, twCompanyPK, pidConsignor, pidConsignee, "IMP", 2, "2");
			CreateJobDeclaration("TW", twBranchPK, twCompanyPK, pasConsignor, pasConsignee, "IMP", 3, "3");

			var decl4 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, epzConsignor, cccConsignee, "IMP", 4, "4");
			CreateCusEntryInstruction(decl4, 4, "D5", cpwFromWarehouseAddress, Guid.Empty);
			var importerDocumentaryAddressPK = Guid.NewGuid();
			creator.CreateDocAddress(importerDocumentaryAddressPK, decl4, "JE", "IMD", "", 0, null, "", "", true);
			CreateJobDocAddressNumber(importerDocumentaryAddressPK, "EPZ", "EPZ12345", "TW");

			var decl5 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, epzConsignor, Guid.Empty, "IMP", 5, "5");
			CreateCusEntryInstruction(decl5, 5, "D2", ccpFromWarehouseAddress, Guid.Empty);

			var decl6 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, epzConsignor, Guid.Empty, "IMP", 6, "6");
			CreateCusEntryInstruction(decl6, 6, "D7", cpwFromWarehouseAddress, Guid.Empty);

			var decl7 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, epzConsignor, Guid.Empty, "IMP", 7, "7");
			CreateCusEntryInstruction(decl7, 7, "F4", Guid.Empty, Guid.Empty);
			var supplierDocumentaryAddressPK = Guid.NewGuid();
			creator.CreateDocAddress(supplierDocumentaryAddressPK, decl7, "JE", "SUD", "", 0, null, "", "", true);
			CreateJobDocAddressNumber(supplierDocumentaryAddressPK, "EPZ", "EPZ12345", "TW");

			var decl8 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, epzConsignor, Guid.Empty, "IMP", 8, "8");
			CreateCusEntryInstruction(decl8, 8, "F5", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl8, "JE", "SUD", "", 0, epzConsignoraddress, "", "", false);

			var decl9 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, cbfConsignor, Guid.Empty, "IMP", 9, "9");
			CreateCusEntryInstruction(decl9, 9, "B6", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl9, "JE", "SUD", "", 0, cbfConsignoraddress, "", "", false);

			var decl10 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, ftzConsignor, cccConsignee, "IMP", 10, "10");
			CreateCusEntryInstruction(decl10, 10, "D8", Guid.Empty, cpwToWarehouseAddress);
			creator.CreateDocAddress(Guid.NewGuid(), decl10, "JE", "SUD", "", 0, ftzConsignoraddress, "", "", false);

			var decl11 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, atpConsignor, cccConsignee, "IMP", 11, "11");
			CreateCusEntryInstruction(decl11, 11, "F2", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl11, "JE", "SUD", "", 0, atpConsignoraddress, "", "", false);
			creator.CreateDocAddress(Guid.NewGuid(), decl11, "JE", "IMD", "", 0, cccConsigneeaddress, "", "", false);

			var decl12 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, spkConsignor, Guid.Empty, "IMP", 12, "12");
			CreateCusEntryInstruction(decl12, 12, "F3", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl12, "JE", "SUD", "", 0, spkConsignoraddress, "", "", false);

			var decl13 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, Guid.Empty, cccConsignee, "IMP", 13, "13");
			CreateCusEntryInstruction(decl13, 13, "B2", Guid.Empty, Guid.Empty);
			importerDocumentaryAddressPK = Guid.NewGuid();
			creator.CreateDocAddress(importerDocumentaryAddressPK, decl13, "JE", "IMD", "", 0, null, "", "", true);
			CreateJobDocAddressNumber(importerDocumentaryAddressPK, "EPZ", "CBF12345", "TW");

			var decl14 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, Guid.Empty, epzConsignee, "IMP", 14, "14");
			CreateCusEntryInstruction(decl14, 14, "D7", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl14, "JE", "IMD", "", 0, epzConsigneeaddress, "", "", false);

			var toWarehouseAddress2 = CreateOrgForToWarehouse2(creator);
			var decl15 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, Guid.Empty, epzConsignee, "IMP", 15, "15");
			CreateCusEntryInstruction(decl15, 15, "D1", Guid.Empty, toWarehouseAddress2);

			var decl16 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, Guid.Empty, cbfConsignee, "IMP", 16, "16");
			CreateCusEntryInstruction(decl16, 16, "B8", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl16, "JE", "IMD", "", 0, cbfConsigneeaddress, "", "", false);

			var decl17 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, Guid.Empty, ftzConsignee, "IMP", 17, "17");
			CreateCusEntryInstruction(decl17, 17, "B9", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl17, "JE", "IMD", "", 0, ftzConsigneeaddress, "", "", false);

			var decl18 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, Guid.Empty, atpConsignee, "IMP", 18, "18");
			CreateCusEntryInstruction(decl18, 18, "F4", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl18, "JE", "IMD", "", 0, atpConsigneeaddress, "", "", false);

			var decl19 = CreateJobDeclaration("TW", twBranchPK, twCompanyPK, Guid.Empty, spkConsignee, "IMP", 19, "19");
			CreateCusEntryInstruction(decl19, 19, "F1", Guid.Empty, Guid.Empty);
			creator.CreateDocAddress(Guid.NewGuid(), decl19, "JE", "IMD", "", 0, spkConsigneeaddress, "", "", false);
		}

		const string CreateJobDeclarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_GB, JE_GC, JE_OH_Exporter, JE_OH_Consignee, JE_ClusterKey, JE_DataModel, JE_DeclarationReference, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
			VALUES (@pk, @branch, @company, @exporter, @consignee, @clusterKey, @dataModel, @declarationReference, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobDeclaration(string dataModel, Guid branch, Guid company, Guid exporter, Guid consignee, string messageType, int clusterKey, string declarationReference)
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateJobDeclarationSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branch);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company);
				if (exporter == Guid.Empty)
				{
					command.AddParameter("@exporter", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@exporter", SqlDbType.UniqueIdentifier, exporter);
				}
				if (consignee == Guid.Empty)
				{
					command.AddParameter("@consignee", SqlDbType.UniqueIdentifier, DBNull.Value);
				} else
				{
					command.AddParameter("@consignee", SqlDbType.UniqueIdentifier, consignee);
				}
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationReference);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		const string CreateCusEntryInstructionSql = @"INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_Style, CEI_OA_Warehouse, CEI_OA_Warehouse2, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
			VALUES (@pk, 'TW', @declarationPK, @clusterKey, @style, @fromWarehouse, @toWarehouse, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateCusEntryInstruction(Guid declarationPK, int clusterKey, string style, Guid fromWarehouse, Guid toWarehouse)
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateCusEntryInstructionSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@style", SqlDbType.VarChar, style);
				if (fromWarehouse == Guid.Empty)
				{
					command.AddParameter("@fromWarehouse", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@fromWarehouse", SqlDbType.UniqueIdentifier, fromWarehouse);
				}
				if (toWarehouse == Guid.Empty)
				{
					command.AddParameter("@toWarehouse", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@toWarehouse", SqlDbType.UniqueIdentifier, toWarehouse);
				}
				command.ExecuteNonQuery();
			}
			return pk;
		}

		const string CreateJobDocAddressNumberSql = @"INSERT INTO dbo.JobDocAddressNumber(E2N_PK, E2N_E2, E2N_NumberType, E2N_Number, E2N_RN_NKCountryCode, E2N_SystemCreateTimeUtc, E2N_SystemCreateUser, E2N_SystemLastEditTimeUtc, E2N_SystemLastEditUser)
			VALUES (@pk, @addressPk, @numberType, @number, @countryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobDocAddressNumber(Guid addressPk, string numberType, string number, string countryCode)
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateJobDocAddressNumberSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@addressPk", SqlDbType.UniqueIdentifier, addressPk);
				command.AddParameter("@numberType", SqlDbType.VarChar, numberType);
				command.AddParameter("@number", SqlDbType.VarChar, number);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		(Guid, Guid) CreateVATOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH11", "CRA1", "TW");
			var orgArress = creator.CreateOrgAddress(org, "Address1", "OAAD1", "OAAD1", "TW");
			creator.CreateOrgCusCode(org, "VAT", "VAT11", "TW");
			creator.CreateOrgCusCode(org, "PID", "PID11", "TW");
			creator.CreateOrgCusCode(org, "PAS", "PAS11", "TW");
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreatePIDOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH12", "CRA2", "TW");
			var orgArress = creator.CreateOrgAddress(org, "Address2", "OAAD2", "OAAD2", "TW");
			creator.CreateOrgCusCode(org, "PID", "PID12", "TW");
			creator.CreateOrgCusCode(org, "PAS", "PAS12", "TW");
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreatePASOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH13", "CRA3", "TW");
			var orgArress = creator.CreateOrgAddress(org, "Address3", "OAAD3", "OAAD3", "TW");
			creator.CreateOrgCusCode(org, "PAS", "PAS13", "TW");
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateEPZOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH14", "CRA4", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address4", "OAAD4", "OAAD4", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "EPZ", "EPZ14", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CBF", "CBF14", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "FTZ", "FTZ14", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP14", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK14", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateCBFOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH15", "CRA5", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address5", "OAAD5", "OAAD5", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CBF", "CBF15", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "FTZ", "FTZ15", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP15", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK15", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateFTZOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH16", "CRA6", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address6", "OAAD6", "OAAD6", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "FTZ", "FTZ16", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP16", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK16", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateATPOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH17", "CRA7", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address7", "OAAD7", "OAAD7", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP17", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK17", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateSPKOrgForConsignorAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH18", "CRA8", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address8", "OAAD8", "OAAD8", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK18", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		Guid CreateCPWOrgForFromWarehouse(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH19", "CRA9", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address9", "OAAD9", "OAAD9", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CPW", "CPW19", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CCP", "CCP19", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return orgArress;
		}

		Guid CreateCCPOrgForFromWarehouse(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH110", "CRA10", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address10", "OAAD10", "OAAD10", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CCP", "CCP110", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return orgArress;
		}

		(Guid, Guid) CreateVATOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH21", "CEA1", "TW");
			var orgArress = creator.CreateOrgAddress(org, "Address1", "OAAD1", "OAAD1", "TW");
			creator.CreateOrgCusCode(org, "VAT", "VAT21", "TW");
			creator.CreateOrgCusCode(org, "PID", "PID21", "TW");
			creator.CreateOrgCusCode(org, "PAS", "PAS21", "TW");
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreatePIDOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH22", "CEA2", "TW");
			var orgArress = creator.CreateOrgAddress(org, "Address2", "OAAD2", "OAAD2", "TW");
			creator.CreateOrgCusCode(org, "PID", "PID22", "TW");
			creator.CreateOrgCusCode(org, "PAS", "PAS22", "TW");
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreatePASOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH23", "CEA3", "TW");
			var orgArress = creator.CreateOrgAddress(org, "Address3", "OAAD3", "OAAD3", "TW");
			creator.CreateOrgCusCode(org, "PAS", "PAS23", "TW");
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateCCCOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH24", "CEA4", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address4", "OAAD4", "OAAD4", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CCC", "CCC24", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "EPZ", "EPZ24", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CBF", "CBF24", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "FTZ", "FTZ24", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP24", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK24", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateEPZOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH25", "CEA5", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address5", "OAAD5", "OAAD5", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "EPZ", "EPZ25", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CBF", "CBF25", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "FTZ", "FTZ25", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP25", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK25", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateCBFOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH26", "CEA6", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address6", "OAAD6", "OAAD6", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CBF", "CBF26", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "FTZ", "FTZ26", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP26", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK26", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateFTZOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH27", "CEA7", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address7", "OAAD7", "OAAD7", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "FTZ", "FTZ27", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP27", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK27", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateATPOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH28", "CEA8", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address8", "OAAD8", "OAAD8", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "ATP", "ATP28", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK28", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		(Guid, Guid) CreateSPKOrgForConsigneeAddress(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH29", "CEA9", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address9", "OAAD9", "OAAD9", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "SPK", "SPK29", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return (org, orgArress);
		}

		Guid CreateCPWOrgForToWarehouse(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH210", "CEA10", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address10", "OAAD10", "OAAD10", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CPW", "CPW210", "TW", orgArress);
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CCP", "CCP210", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return orgArress;
		}

		Guid CreateOrgForToWarehouse2(TransformationTestDataCreator creator)
		{
			var org = creator.CreateOrg("OH211", "CEA11", "US");
			var orgArress = creator.CreateOrgAddress(org, "Address11", "OAAD11", "OAAD11", "TW");
			creator.CreateOrgCusCodeWithPremiseAddress(org, "CCP", "CCP211", "TW", orgArress);
			creator.CreateOrgAddressCapability(orgArress, "OFC");
			return orgArress;
		}
	}
}
