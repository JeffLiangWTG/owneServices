using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.TW
{
	[TestedType(typeof(Report_TWEntriesAndLines))]
	class Report_TWEntriesAndLinesTest : DbCreateScriptTest
	{
		public void TestColumnsFromCusEntryInstruction()
		{
			Db.Connection.Command($@"
			UPDATE
				dbo.CusEntryInstruction
			SET
				CEI_Style = 'F1',
				CEI_AddInfo = 'PackageDescription=AB*ExamMode=8',
				CEI_SystemLastEditTimeUtc = GETUTCDATE(),
				CEI_SystemLastEditUser = '~BP'
			WHERE CEI_PK = '{cusEntryInstructionPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, DeclarationDate, DeclarationType, PackageDescription, ExamMode FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);

				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());

					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("DeclarationDate", new DateTime(2021, 12, 12), reader.GetDateTime(1));
					AssertEquals("DeclarationType", "F1", reader.GetString(2));
					AssertEquals("PackageDescription", "AB", reader.GetString(3));
					AssertEquals("ExamMode", "8", reader.GetString(4));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromJobDeclaration()
		{
			const int columnIDJobNumber = 0;
			const int columnIDJobBranch = 1;
			const int columnIDShipmentType = 2;
			const int columnIDTransportMode = 3;
			const int columnIDTransportCode = 4;
			const int columnIDMasterBill = 5;
			const int columnIDHouseBill = 6;
			const int columnIDVesselName = 7;
			const int columnIDVoyageFlightNo = 8;
			const int columnIDSoNoManifest = 9;
			const int columnIDPortOfOrigin = 10;
			const int columnIDFinalDestination = 11;
			const int columnIDImportDate = 12;
			const int columnIDTotalWeight = 13;
			const int columnIDTotalWeightUnit = 14;
			const int columnIDTotalPackages = 15;
			const int columnIDTotalPackagesUnit = 16;
			const int columnIDSummary = 17;
			const int columnIDPaymentMethod = 18;
			const int columnIDOwnersReference = 19;
			const int columnIDCreateUser = 20;

			Db.Connection.Command($@"
			UPDATE
				dbo.JobDeclaration
			SET
				JE_DeclarationReference = 'JOBNUMBER001',
				JE_MessageType = 'EXP',
				JE_TransportMode = 'AIR',
				JE_ContainerMode = 'LSE',
				JE_MasterBill = '123456789',
				JE_HouseBill = '987654321',
				JE_VesselName = '33456',
				JE_VoyageFlightNo = 'QF123',
				JE_AddInfo = 'SLD=5556',
				JE_RL_NKOrigin = 'NZAKL',
				JE_RL_NKFinalDestination = 'AUSYD',
				JE_DateAtFinalDestination = DATEFROMPARTS(2021, 1, 1),
				JE_TotalWeight = 220,
				JE_TotalWeightUnit = 'KG',
				JE_TotalNoOfPacks = 110,
				JE_TotalNoOfPacksPackType = 'PLT',
				JE_GoodsDescription = 'normal goods',
				JE_PaymentMethod = 'BRK',
				JE_OwnerRef = 'OWNERS REFERENCE',
				JE_SystemCreateUser = 'ABC',
				JE_SystemLastEditTimeUtc = GETUTCDATE(),
				JE_SystemLastEditUser = 'ABC'
			WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JobNumber, JobBranch, ShipmentType, TransportMode, TransportCode, MasterBill, HouseBill, VesselName, VoyageFlightNo, SoNoManifest,
								PortOfOrigin, FinalDestination, ImportDate, TotalWeight, TotalWeightUnit, TotalPackages, TotalPackagesUnit, Summary,
								PaymentMethod, OwnersReference, CreateUser FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "AIR");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("JobNumber", "JOBNUMBER001", reader.GetString(columnIDJobNumber));
						AssertEquals("JobBranch", branchPK, reader.GetGuid(columnIDJobBranch));
						AssertEquals("ShipmentType", "EXP", reader.GetString(columnIDShipmentType));
						AssertEquals("TransportMode", "AIR", reader.GetString(columnIDTransportMode));
						AssertEquals("TransportCode", "41", reader.GetString(columnIDTransportCode));
						AssertEquals("MasterBill", "123-456789", reader.GetString(columnIDMasterBill));
						AssertEquals("HouseBill", "987654321", reader.GetString(columnIDHouseBill));
						AssertEquals("VesselName", "33456", reader.GetString(columnIDVesselName));
						AssertEquals("VoyageFlightNo", "QF123", reader.GetString(columnIDVoyageFlightNo));
						AssertEquals("SoNoManifest", "5556", reader.GetString(columnIDSoNoManifest));
						AssertEquals("PortOfOrigin", "NZAKL", reader.GetString(columnIDPortOfOrigin));
						AssertEquals("FinalDestination", "AUSYD", reader.GetString(columnIDFinalDestination));
						AssertEquals("ImportDate", new DateTime(2021, 1, 1), reader.GetDateTime(columnIDImportDate));
						AssertEquals("TotalWeight", 220m, reader.GetDecimal(columnIDTotalWeight));
						AssertEquals("TotalWeightUnit", "KG", reader.GetString(columnIDTotalWeightUnit));
						AssertEquals("TotalPackages", 110, reader.GetInt32(columnIDTotalPackages));
						AssertEquals("TotalPackagesUnit", "PLT", reader.GetString(columnIDTotalPackagesUnit));
						AssertEquals("Summary", "normal goods", reader.GetString(columnIDSummary));
						AssertEquals("PaymentMethod", "BRK", reader.GetString(columnIDPaymentMethod));
						AssertEquals("OwnersReference", "OWNERS REFERENCE", reader.GetString(columnIDOwnersReference));
						AssertEquals("CreateUser", "ABC", reader.GetString(columnIDCreateUser));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
			UPDATE
				dbo.JobDeclaration
			SET
				JE_MessageType = 'IMP',
				JE_TransportMode = 'SEA',
				JE_ContainerMode = 'BBK',
				JE_SystemLastEditTimeUtc = GETUTCDATE(),
				JE_SystemLastEditUser = '~BP'
			WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("JobNumber", "JOBNUMBER001", reader.GetString(columnIDJobNumber));
						AssertEquals(branchPK, reader.GetGuid(columnIDJobBranch));
						AssertEquals("ShipmentType", "IMP", reader.GetString(columnIDShipmentType));
						AssertEquals("TransportMode", "SEA", reader.GetString(columnIDTransportMode));
						AssertEquals("TransportCode", "11", reader.GetString(columnIDTransportCode));
						AssertEquals("MasterBill", "123456789", reader.GetString(columnIDMasterBill));
						AssertEquals("HouseBill", "987654321", reader.GetString(columnIDHouseBill));
						AssertEquals("VesselName", "33456", reader.GetString(columnIDVesselName));
						AssertEquals("VoyageFlightNo", "QF123", reader.GetString(columnIDVoyageFlightNo));
						AssertEquals("SoNoManifest", "5556", reader.GetString(columnIDSoNoManifest));
						AssertEquals("PortOfOrigin", "NZAKL", reader.GetString(columnIDPortOfOrigin));
						AssertEquals("FinalDestination", "AUSYD", reader.GetString(columnIDFinalDestination));
						AssertEquals("ImportDate", new DateTime(2021, 1, 1), reader.GetDateTime(columnIDImportDate));
						AssertEquals("TotalWeight", 220m, reader.GetDecimal(columnIDTotalWeight));
						AssertEquals("TotalWeightUnit", "KG", reader.GetString(columnIDTotalWeightUnit));
						AssertEquals("TotalPackages", 110, reader.GetInt32(columnIDTotalPackages));
						AssertEquals("TotalPackagesUnit", "PLT", reader.GetString(columnIDTotalPackagesUnit));
						AssertEquals("GoodsDescription", "normal goods", reader.GetString(columnIDSummary));
						AssertEquals("PaymentMethod", "BRK", reader.GetString(columnIDPaymentMethod));
						AssertEquals("OwnersReference", "OWNERS REFERENCE", reader.GetString(columnIDOwnersReference));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestColumnsFromCusEntryHeader()
		{
			Db.Connection.Command($@"
				UPDATE
					dbo.CusEntryHeader
				SET
					CH_AddInfo = 'DeclarationIncoterm=FOB',
					CH_EntryReleaseDate = DATEFROMPARTS(2022, 10, 19),
					CH_SystemLastEditTimeUtc = GETUTCDATE(),
					CH_SystemLastEditUser = '~BP'
				WHERE CH_PK = '{entryHeaderPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT DeclarationIncoterm, ReleaseDate FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("DeclarationIncoterm", "FOB", reader.GetString(0));
						AssertEquals("ReleaseDate", new DateTime(2022, 10, 19), reader.GetDateTime(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestColumnsFromJobComInvoiceHeader()
		{
			Db.Connection.Command($@"
				UPDATE
					dbo.JobComInvoiceHeader
				SET
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.1,
					JZ_InvoiceNumber = 'Test123',
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE JZ_PK = '{invoiceHeaderPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT Currency, ExchangeRate FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("Currency", "USD", reader.GetString(0));
						AssertEquals("ExchangeRate", 30.1m, reader.GetDecimal(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestColumnsFromJobComInvoiceLine()
		{
			Db.Connection.Command($@"
				UPDATE
					dbo.JobComInvoiceLine
				SET
					JI_LineNo = 1,
					JI_Tariff = '86041000002',
					JI_Procedure = '31',
					JI_InvoiceQuantity = 13.5,
					JI_InvoiceUQ = 'ACR',
					JI_LinePrice = 39,
					JI_NetWeight = 125,
					JI_NetWeightUQ = 'KG',
					JI_CustomsSecondQuantity = 129,
					JI_CustomsSecondUnitQty = 'G',
					JI_BrandName = 'UNIQLO',
					JI_Model = 'EV123',
					JI_PreviousEntryNumber = '9753123',
					JI_PreviousEntryLineNumber = 5,
					JI_NDescription = N'Chinese描述',
					JI_Description = 'English',
					JI_CountryOfOrigin = 'CN',
					JI_CustomsUnitQty = 'TNE',
					JI_NAddInfo = N'Group=影像產品GroupingTest*Compositions=CompositionsTest',
					JI_AddInfo = 'EnteredUnitPrice=34*CVAfterRecon=teacher*CustomsSupplierPartNo=234*CustomsOwnerPartNo=345*ModelYear=2021*CarType=A1*NumberOfDoor=4*Displacement=1001*Cylinders=45*Seats=3*LHD=Y*EngineType=CG*Transmission=A*EquipmentPrintMode=EEC*TextileWidth=15*TextileWidthUQ=FT',
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JI_Procedure, Brand, Model, Specification, SupplierPartNo, OwnerPartNo, PreviousEntryNo, PreviousEntryLineNo, GoodsDescription, Grouping, GoodsOrigin, NetWeightUnit FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("JI_Procedure", "31", reader.GetString(0));
						AssertEquals("Brand", "UNIQLO", reader.GetString(1));
						AssertEquals("Model", "EV123", reader.GetString(2));
						AssertEquals("Specification", "CompositionsTest", reader.GetString(3));
						AssertEquals("SupplierPartNo", "234", reader.GetString(4));
						AssertEquals("OwnerPartNo", "345", reader.GetString(5));
						AssertEquals("PreviousEntryNo", "9753123", reader.GetString(6));
						AssertEquals("PreviousEntryLineNo", (Int16)5, reader.GetInt16(7));
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nChinese描述\nEnglish\nWIDTH: 15 '", reader.GetString(8));
						AssertEquals("Grouping", "影像產品GroupingTest", reader.GetString(9));
						AssertEquals("GoodsOrigin", "CN", reader.GetString(10));
						AssertEquals("NetWeightUnit", "KG", reader.GetString(11));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				UPDATE
					dbo.JobDeclaration
				SET
					JE_MessageType = 'EXP',
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE
					JE_PK = '{declarationPK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("JI_Procedure", "31", reader.GetString(0));
						AssertEquals("Brand", "UNIQLO", reader.GetString(1));
						AssertEquals("Model", "EV123", reader.GetString(2));
						AssertEquals("Specification", "CompositionsTest", reader.GetString(3));
						AssertEquals("SupplierPartNo", "234", reader.GetString(4));
						AssertEquals("OwnerPartNo", "345", reader.GetString(5));
						AssertEquals("PreviousEntryNo", "9753123", reader.GetString(6));
						AssertEquals("PreviousEntryLineNo", (Int16)5, reader.GetInt16(7));
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nChinese描述\nEnglish\nWIDTH: 15 '", reader.GetString(8));
						AssertEquals("Grouping", "影像產品GroupingTest", reader.GetString(9));
						AssertEquals("GoodsOrigin", "CN", reader.GetString(10));
						AssertEquals("NetWeightUnit", "KG", reader.GetString(11));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				UPDATE
					dbo.JobDeclaration
				SET
					JE_MessageType = 'IMP',
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			reportSql = @"SELECT GoodsDescription FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nChinese描述\nEnglish\nWIDTH: 15 '", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				UPDATE
					dbo.CusEntryLine
				SET
					CL_AdValoremTariff = '86041000002',
					CL_SystemLastEditTimeUtc = GETUTCDATE(),
					CL_SystemLastEditUser = '~BP'
				WHERE CL_PK = '{entryLinePK}'").ExecuteNonQuery();

			reportSql = @"SELECT GoodsDescription FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nChinese描述\nEnglish\nMODEL YEAR: 2021 CAR TYPE: BUS DOOR: 4\nBRAND: UNIQLO MODEL: EV123\nDISPLACEMENT: 1001 CYLINDER: 45 SEAT: 3\nLEFT SIDE STEERING: YES\nENGINE TYPE: NATURAL GAS\nTRANSMISSION: AUTOMATIC\nSTANDARD EQUIPMENT WITH EEC. Sai Systems Catalyst Converter\nWIDTH: 15 '", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			CreateJobComInvLineRefs(invoiceLinePK, "1", "CHAS", 1);
			CreateJobComInvLineRefs(invoiceLinePK, "2", "CHAS", 1);
			CreateJobComInvLineRefs(invoiceLinePK, "3", "CHAS", 1);
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nChinese描述\nEnglish\nMODEL YEAR: 2021 CAR TYPE: BUS DOOR: 4\nBRAND: UNIQLO MODEL: EV123\nDISPLACEMENT: 1001 CYLINDER: 45 SEAT: 3\nLEFT SIDE STEERING: YES\nENGINE TYPE: NATURAL GAS\nTRANSMISSION: AUTOMATIC\nSTANDARD EQUIPMENT WITH EEC. Sai Systems Catalyst Converter\nCHASSIS NO: 1;2;3\nWIDTH: 15 '", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			CreateStmNote(invoiceLinePK, "stmNoteText");
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nstmNoteText", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				UPDATE
					dbo.JobDeclaration
				SET
					JE_MergeBy = 'NON',
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "stmNoteText", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				DELETE
					dbo.StmNote
				WHERE
					ST_ParentID = '{invoiceLinePK}'
					AND ST_Table = 'JobComInvoiceLine'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription when not override default", "Chinese描述\nEnglish\nMODEL YEAR: 2021 CAR TYPE: BUS DOOR: 4\nBRAND: UNIQLO MODEL: EV123\nDISPLACEMENT: 1001 CYLINDER: 45 SEAT: 3\nLEFT SIDE STEERING: YES\nENGINE TYPE: NATURAL GAS\nTRANSMISSION: AUTOMATIC\nSTANDARD EQUIPMENT WITH EEC. Sai Systems Catalyst Converter\nCHASSIS NO: 1;2;3\nWIDTH: 15 '", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				UPDATE
					dbo.JobComInvoiceLine
				SET
					JI_NAddInfo = '',
					JI_NDescription = '',
					JI_BrandName = '',
					JI_AddInfo = 'EnteredUnitPrice=34*CVAfterRecon=teacher*CustomsSupplierPartNo=234*CustomsOwnerPartNo=345*CarType=A1*NumberOfDoor=4*Cylinders=45*Seats=3*LHD=Y*EngineType=CG*Transmission=A*EquipmentPrintMode=EEC*TextileWidth=15*TextileWidthUQ=FT',
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE JI_PK = '{invoiceLinePK}'").ExecuteNonQuery();

			reportSql = @"SELECT GoodsDescription FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions("Test For Trim()", () =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "English\nCAR TYPE: BUS DOOR: 4\nMODEL: EV123\nCYLINDER: 45 SEAT: 3\nLEFT SIDE STEERING: YES\nENGINE TYPE: NATURAL GAS\nTRANSMISSION: AUTOMATIC\nSTANDARD EQUIPMENT WITH EEC. Sai Systems Catalyst Converter\nCHASSIS NO: 1;2;3\nWIDTH: 15 '", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestColumnsFromCusEntryLine()
		{
			Db.Connection.Command($@"
				UPDATE
					dbo.CusEntryLine
				SET
					CL_LineNumber = 2,
					CL_AdValoremTariff = '02041000003',
					CL_CustomsValue = 12.25,
					CL_SystemLastEditTimeUtc = GETUTCDATE(),
					CL_SystemLastEditUser = '~BP'
				WHERE CL_PK = '{entryLinePK}'").ExecuteNonQuery();
			var reportSql = @"SELECT EntryLineNo, Tariff, CustomsValue FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("EntryLineNo", (Int16)2, reader.GetInt16(0));
						AssertEquals("Tariff", "02041000003", reader.GetString(1));
						AssertEquals("CustomsValue", 12.25m, reader.GetDecimal(2));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestColumnsFromJobDeclarationCusEntryNum()
		{
			var cusEntryNumPK = TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "ABAM10123BBBB2", "IMP", "CUS", "TW");

			Db.Connection.Command($@"
			UPDATE
				dbo.CusEntryNum
			SET
				CE_EntryStatus = 'C1',
				CE_SystemLastEditTimeUtc = GETUTCDATE(),
				CE_SystemLastEditUser = '~BP'
			WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT EntryNumber FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("EntryNumber", "ABAM10123BBBB2", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
			UPDATE
				dbo.CusEntryNum
			SET
				CE_EntryNum = '',
				CE_SystemLastEditTimeUtc = GETUTCDATE(),
				CE_SystemLastEditUser = '~BP'
			WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			reportSql = @"SELECT EntryNumber FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("EntryNumber", "", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestColumnsFromCusEntryHeaderCusEntryNum()
		{
			var cusEntryNumPKFromCusEntryHeader = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "BCBN10123BBBB2", "IMP", "CUS", "TW");

			Db.Connection.Command($@"
			UPDATE
				dbo.CusEntryNum
			SET
				CE_EntryStatus = 'C1',
				CE_SystemLastEditTimeUtc = GETUTCDATE(),
				CE_SystemLastEditUser = '~BP'
			WHERE CE_PK = '{cusEntryNumPKFromCusEntryHeader}'").ExecuteNonQuery();

			var reportSql = @"SELECT EntryNumber, ClearanceStatus FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("EntryNumber", "BCBN10123BBBB2", reader.GetString(0));
						AssertEquals("ClearanceStatus", "C1", reader.GetString(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestSupplier()
		{
			var supplierPK = TestDataCreator.CreateOrganisation("Code32", "SupplierName");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "AddressCode", "Address1");
			TestDataCreator.CreateDocAddress(supplierAddressPK, "Org Override SUD", declarationPK, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);

			var reportSql = @"SELECT SupplierCode, SupplierChineseName, SupplierEnglishName FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("SupplierCode", "Code32", reader.GetString(0));
						AssertEquals("SupplierChineseName", "", reader.GetString(1));
						AssertEquals("SupplierEnglishName", "Org Override SUD", reader.GetString(2));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override STA", declarationPK, "JE", "STA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("SupplierCode", "Code32", reader.GetString(0));
						AssertEquals("SupplierChineseName", "Org Override STA", reader.GetString(1));
						AssertEquals("SupplierEnglishName", "Org Override SUD", reader.GetString(2));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestImporter()
		{
			var importerPK = TestDataCreator.CreateOrganisation("Code12", "ImporterName");
			var importerAddressPK = TestDataCreator.CreateAddress(importerPK, "AddressCode", "Address1");
			TestDataCreator.CreateDocAddress(importerAddressPK, "Org Override IMD", declarationPK, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			var reportSql = @"SELECT ImporterCode, ImporterChineseName, ImporterEnglishName FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImporterCode", "Code12", reader.GetString(0));
						AssertEquals("ImporterChineseName", "", reader.GetString(1));
						AssertEquals("ImporterEnglishName", "Org Override IMD", reader.GetString(2));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override ITA", declarationPK, "JE", "ITA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImporterCode", "Code12", reader.GetString(0));
						AssertEquals("ImporterChineseName", "Org Override ITA", reader.GetString(1));
						AssertEquals("ImporterEnglishName", "Org Override IMD", reader.GetString(2));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestPortName()
		{
			Db.Connection.Command($@"
			UPDATE
				dbo.JobDeclaration
			SET
				JE_NAddInfo = 'Z99PortOfOrigin=Shanghai*Z99FinalDestination=Hsichih',
				JE_SystemLastEditTimeUtc = GETUTCDATE(),
				JE_SystemLastEditUser = '~BP'
			WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT OriginDescription, FinalDestinationDescription FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("OriginDescription", "Shanghai", reader.GetString(0));
						AssertEquals("FinalDestinationDescription", "Hsichih", reader.GetString(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
			UPDATE
				dbo.JobDeclaration
			SET
				JE_RL_NKOrigin = 'TWTPE',
				JE_RL_NKFinalDestination = 'TWKHH',
				JE_SystemLastEditTimeUtc = GETUTCDATE(),
				JE_SystemLastEditUser = '~BP'
			WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("OriginDescription", "Taipei", reader.GetString(0));
						AssertEquals("FinalDestinationDescription", "Kaohsiung", reader.GetString(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestCertificateOfOrigin()
		{
			CreateCussupportinginfo(invoiceLinePK, "1314520", 3, "COO", 1);

			var reportSql = @"SELECT CertificateOfOrigin, CertificateOfOriginLineNo FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("CertificateOfOrigin", "1314520", reader.GetString(0));
						AssertEquals("CertificateOfOriginLineNo", 3, reader.GetInt32(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestPreviousBondedEntryNo()
		{
			CreateCussupportinginfo(invoiceLinePK, "9487987", 9, "PBN", 1);

			var reportSql = @"SELECT PreviousBondedEntryNo, PreviousBondedEntryLineNo FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("PreviousBondedEntryNo", "9487987", reader.GetString(0));
						AssertEquals("PreviousBondedEntryLineNo", 9, reader.GetInt32(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestPermit()
		{
			CreateCussupportinginfo(invoiceLinePK, "11111", 1, "CPC", 1);
			CreateCussupportinginfo(invoiceLinePK, "22222", 2, "CPC", 2);
			CreateCussupportinginfo(invoiceLinePK, "33333", 3, "CPC", 3);
			CreateCussupportinginfo(invoiceLinePK, "44444", 4, "CPC", 4);
			CreateCussupportinginfo(invoiceLinePK, "55555", 5, "CPC", 5);
			CreateCussupportinginfo(invoiceLinePK, "66666", 6, "CPC", 6);
			CreateCussupportinginfo(invoiceLinePK, "ECA1", 1, "ECA", 1);
			CreateCussupportinginfo(invoiceLinePK, "ECA2", 2, "ECA", 2);
			CreateCussupportinginfo(invoiceLinePK, "ECA3", 3, "ECA", 3);

			var reportSql = @"SELECT FirstPermitNo, FirstPermitLineNo, SecondPermitNo, SecondPermitLineNo, ThirdPermitNo, ThirdPermitLineNo, FourthPermitNo, FourthPermitLineNo, FifthPermitNo, FifthPermitLineNo, PermitExemptionCodes FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("FirstPermitNo", "11111", reader.GetString(0));
						AssertEquals("FirstPermitLineNo", 1, reader.GetInt32(1));
						AssertEquals("SecondPermitNo", "22222", reader.GetString(2));
						AssertEquals("SecondPermitLineNo", 2, reader.GetInt32(3));
						AssertEquals("ThirdPermitNo", "33333", reader.GetString(4));
						AssertEquals("ThirdPermitLineNo", 3, reader.GetInt32(5));
						AssertEquals("FourthPermitNo", "44444", reader.GetString(6));
						AssertEquals("FourthPermitLineNo", 4, reader.GetInt32(7));
						AssertEquals("FifthPermitNo", "55555", reader.GetString(8));
						AssertEquals("FifthPermitLineNo", 5, reader.GetInt32(9));
						AssertEquals("PermitExemptionCodes", "ECA1;ECA2;ECA3", reader.GetString(10));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestNetWeight()
		{
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK, 1, 2);
			var invoiceLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK, 1, 3);

			Db.Connection.Command($@"
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_NetWeight = 10.1,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK}';
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_NetWeight = 20.2,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK2}';
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_NetWeight = 30.3,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK3}';").ExecuteNonQuery();

			var reportSql = @"SELECT NetWeight FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("NetWeight", 60.6m, reader.GetDecimal(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestQuantity()
		{
			Db.Connection.Command($@"
				UPDATE dbo.JobDeclaration
				SET
					JE_MergeBy = 'TRF',
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE
					JE_PK = '{declarationPK}'
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_LineNo = 1,
					JI_InvoiceQuantity = 10.1,
					JI_LinePrice = 23.7,
					JI_InvoiceUQ = 'ACR',
					JI_AddInfo = 'EnteredUnitPrice=35.6',
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK}';").ExecuteNonQuery();

			var reportSql = @"SELECT Quantity, UnitPrice, Price, QuantityUnit FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("Quantity", 10.1m, reader.GetDecimal(0));
						AssertEquals("UnitPrice", 35.6m, reader.GetDecimal(1));
						AssertEquals("Price", 359.56m, reader.GetDecimal(2));
						AssertEquals("QuantityUnit", "ACR", reader.GetString(3));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK, 1, 2);
			Db.Connection.Command($@"
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_LineNo = 2,
					JI_LinePrice = 23.7,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK2}';").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("Quantity", 1m, reader.GetDecimal(0));
						AssertEquals("UnitPrice", 47.4m, reader.GetDecimal(1));
						AssertEquals("Price", 47.4m, reader.GetDecimal(2));
						AssertEquals("QuantityUnit", "LOT", reader.GetString(3));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				UPDATE dbo.JobDeclaration
				SET
					JE_MergeBy = 'NON',
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE
					JE_PK = '{declarationPK}';
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_InvoiceUQ = 'YDS',
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK}';
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_InvoiceQuantity = 20.2,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK2}';").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("Quantity", 30.3m, reader.GetDecimal(0));
						AssertEquals("UnitPrice", 35.6m, reader.GetDecimal(1));
						AssertEquals("Price", 1078.68m, reader.GetDecimal(2));
						AssertEquals("QuantityUnit", "YDS", reader.GetString(3));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestImportDutyRate()
		{
			var tariffPk1 = Guid.NewGuid();
			var tariffPk2 = Guid.NewGuid();
			var rateCodePk1 = Guid.NewGuid();
			var rateCodePk2 = Guid.NewGuid();
			var preferencePK1 = Guid.NewGuid();
			var preferencePK2 = Guid.NewGuid();
			var ratePk1 = new Guid("00000000-0000-0000-0000-000000000001");
			var ratePk2 = new Guid("00000000-0000-0000-0000-000000000002");
			var ratePk3 = new Guid("00000000-0000-0000-0000-000000000003");
			var ratePk4 = new Guid("00000000-0000-0000-0000-000000000004");
			var ratePk5 = new Guid("00000000-0000-0000-0000-000000000005");
			var ratePk6 = new Guid("00000000-0000-0000-0000-000000000006");

			var prepareTestDataSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = newid();
				DECLARE @RateTypePK UNIQUEIDENTIFIER = newid();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupCountryPK2 UNIQUEIDENTIFIER = newid();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'TW', 'Taiwan');
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'TW');

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES ('{tariffPk1}', @TariffTypePK, 'TC1', 'ZZ Tariff1', 'TW', '2021-01-01', '2079-06-06', '')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES ('{tariffPk2}', @TariffTypePK, 'TC2', 'ZZ Tariff2', 'TW', '2021-01-01', '2079-06-06', '')

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK, 'DTY', 'RateTypeOne', 'TW', '');

				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES('{rateCodePk1}', 'DTA', @RateTypePK, 'ZZ RateCode1')

				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES('{rateCodePk2}', 'DTS', @RateTypePK, 'ZZ RateCode2')

				INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES('{preferencePK1}', 'PR1', 'ZZ Preference1', 'TW')

				INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES('{preferencePK2}', 'PR2', 'ZZ Preference2', 'TW')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk1}', '{rateCodePk1}', '{tariffPk1}', '{preferencePK1}', 'TW', 'ZZ RateFormula1', '0.35', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk2}', '{rateCodePk1}', '{tariffPk1}', '{preferencePK2}', 'TW', 'ZZ RateFormula2', '68/KG', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk3}', '{rateCodePk2}', '{tariffPk1}', '{preferencePK1}', 'TW', 'ZZ RateFormula3', '79/KGM', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk4}', '{rateCodePk2}', '{tariffPk1}', '{preferencePK2}', 'TW', 'ZZ RateFormula4', '0.46', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk5}', '{rateCodePk1}', '{tariffPk2}', '{preferencePK1}', 'TW', 'ZZ RateFormula1', '79/KG', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk6}', '{rateCodePk2}', '{tariffPk2}', '{preferencePK2}', 'TW', 'ZZ RateFormula4', '90/KGM', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES (@tradeGroupPK1, 'TG', 'ZZ TG1', '2021-01-01', '2079-06-06', 'TW')

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'CA')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk1}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk2}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk3}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk4}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk5}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk6}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')
			";
			TestConnection.ExecuteNonQuery(prepareTestDataSql);

			AssertImportDutyRate("TC1", "CA", "PR1", "35%\n79/KGM");
			AssertImportDutyRate("TC1", "CA", "PR2", "68/KG\n46%");
			AssertImportDutyRate("TC1", "US", "PR1", "");
			AssertImportDutyRate("TC1", "US", "PR2", "");
			AssertImportDutyRate("TC2", "CA", "PR1", "79/KG");
			AssertImportDutyRate("TC2", "CA", "PR2", "90/KGM");
			AssertImportDutyRate("TC2", "US", "PR1", "");
			AssertImportDutyRate("TC2", "US", "PR2", "");
		}

		void AssertImportDutyRate(string tariff, string countryOfOrigin, string primaryPreference, string expectImportDutyRate)
		{
			Db.Connection.Command($@"
				UPDATE
					dbo.JobComInvoiceLine
				SET
					JI_Tariff = '{tariff}',
					JI_CountryOfOrigin = '{countryOfOrigin}',
					JI_PrimaryPreference = '{primaryPreference}',
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE JI_PK = '{invoiceLinePK}'").ExecuteNonQuery();

			var reportSql = @"SELECT ImportDutyRate FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImportDutyRate", expectImportDutyRate, reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestTaxsAndFees()
		{
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK2, 1, 2);

			CreateCusEntryLineFee(entryLinePK, "DTS", 1.3m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK, "DTA", 1.4m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK, "TPF", 1.5m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK, "CTS", 1.6m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK, "CTA", 1.7m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK, "VAT", 1.8m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK, "TAT", 1.9m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK, "DTS", 2.6m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK, "DTA", 2.7m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK, "TPF", 2.8m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK, "CTS", 2.9m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK, "CTA", 3m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK, "VAT", 3.1m, "DEF", 1);

			CreateCusEntryLineFee(entryLinePK2, "DTS", 3.2m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK2, "DTA", 3.3m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK2, "TPF", 3.4m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK2, "CTS", 3.5m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK2, "CTA", 3.6m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK2, "VAT", 3.7m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK2, "TAT", 3.8m, "CAS", 1);
			CreateCusEntryLineFee(entryLinePK2, "DTS", 3.9m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK2, "DTA", 4m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK2, "TPF", 4.1m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK2, "CTS", 4.2m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK2, "CTA", 4.3m, "DEF", 1);
			CreateCusEntryLineFee(entryLinePK2, "VAT", 4.4m, "DEF", 1);

			var reportSql = @"SELECT ImportDutyCash, ImportDutyNonCash, TradePromotionServiceFeeCash, TradePromotionServiceFeeNonCash, CommodityTaxCash,
									CommodityTaxNonCash, BusinessTaxCash, BusinessTaxNonCash
								FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImportDutyCash", 9m, reader.GetDecimal(0));
						AssertEquals("ImportDutyNonCash", 13m, reader.GetDecimal(1));
						AssertEquals("TradePromotionServiceFeeCash", 4m, reader.GetDecimal(2));
						AssertEquals("TradePromotionServiceFeeNonCash", 6m, reader.GetDecimal(3));
						AssertEquals("CommodityTaxCash", 10m, reader.GetDecimal(4));
						AssertEquals("CommodityTaxNonCash", 14m, reader.GetDecimal(5));
						AssertEquals("BusinessTaxCash", 5m, reader.GetDecimal(6));
						AssertEquals("BusinessTaxNonCash", 7m, reader.GetDecimal(7));

						Assert("record 2", reader.Read());
						AssertEquals("ImportDutyCash", 9m, reader.GetDecimal(0));
						AssertEquals("ImportDutyNonCash", 13m, reader.GetDecimal(1));
						AssertEquals("TradePromotionServiceFeeCash", 4m, reader.GetDecimal(2));
						AssertEquals("TradePromotionServiceFeeNonCash", 6m, reader.GetDecimal(3));
						AssertEquals("CommodityTaxCash", 10m, reader.GetDecimal(4));
						AssertEquals("CommodityTaxNonCash", 14m, reader.GetDecimal(5));
						AssertEquals("BusinessTaxCash", 5m, reader.GetDecimal(6));
						AssertEquals("BusinessTaxNonCash", 7m, reader.GetDecimal(7));
					});
				}
			}

			CreateCusEntryPayInfo(entryHeaderPK, "111111", "A10", 10m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "A19", 11m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "B51", 12m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "B59", 13m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "B10", 14m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "B19", 15m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "B40", 16m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "B49", 17m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "A10", 10m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "A19", 11m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B51", 12m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B59", 13m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B10", 14m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B19", 15m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B40", 16m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B49", 17m, 1);
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImportDutyCash", 20m, reader.GetDecimal(0));
						AssertEquals("ImportDutyNonCash", 0m, reader.GetDecimal(1));
						AssertEquals("TradePromotionServiceFeeCash", 24m, reader.GetDecimal(2));
						AssertEquals("TradePromotionServiceFeeNonCash", 0m, reader.GetDecimal(3));
						AssertEquals("CommodityTaxCash", 28m, reader.GetDecimal(4));
						AssertEquals("CommodityTaxNonCash", 0m, reader.GetDecimal(5));
						AssertEquals("BusinessTaxCash", 32m, reader.GetDecimal(6));
						AssertEquals("BusinessTaxNonCash", 0m, reader.GetDecimal(7));

						Assert("record 2", reader.Read());
						AssertEquals("ImportDutyCash", 20m, reader.GetDecimal(0));
						AssertEquals("ImportDutyNonCash", 0m, reader.GetDecimal(1));
						AssertEquals("TradePromotionServiceFeeCash", 24m, reader.GetDecimal(2));
						AssertEquals("TradePromotionServiceFeeNonCash", 0m, reader.GetDecimal(3));
						AssertEquals("CommodityTaxCash", 28m, reader.GetDecimal(4));
						AssertEquals("CommodityTaxNonCash", 0m, reader.GetDecimal(5));
						AssertEquals("BusinessTaxCash", 32m, reader.GetDecimal(6));
						AssertEquals("BusinessTaxNonCash", 0m, reader.GetDecimal(7));
					});
				}
			}

			Db.Connection.Command($@"
			UPDATE
				dbo.JobDeclaration
			SET
				JE_MessageType = 'EXP',
				JE_SystemLastEditTimeUtc = GETUTCDATE(),
				JE_SystemLastEditUser = '~BP'
			WHERE JE_PK = '{declarationPK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImportDutyCash", 0m, reader.GetDecimal(0));
						AssertEquals("ImportDutyNonCash", 0m, reader.GetDecimal(1));
						AssertEquals("TradePromotionServiceFeeCash", 0m, reader.GetDecimal(2));
						AssertEquals("TradePromotionServiceFeeNonCash", 0m, reader.GetDecimal(3));
						AssertEquals("CommodityTaxCash", 0m, reader.GetDecimal(4));
						AssertEquals("CommodityTaxNonCash", 0m, reader.GetDecimal(5));
						AssertEquals("BusinessTaxCash", 0m, reader.GetDecimal(6));
						AssertEquals("BusinessTaxNonCash", 0m, reader.GetDecimal(7));

						Assert("record 2", reader.Read());
						AssertEquals("ImportDutyCash", 0m, reader.GetDecimal(0));
						AssertEquals("ImportDutyNonCash", 0m, reader.GetDecimal(1));
						AssertEquals("TradePromotionServiceFeeCash", 0m, reader.GetDecimal(2));
						AssertEquals("TradePromotionServiceFeeNonCash", 0m, reader.GetDecimal(3));
						AssertEquals("CommodityTaxCash", 0m, reader.GetDecimal(4));
						AssertEquals("CommodityTaxNonCash", 0m, reader.GetDecimal(5));
						AssertEquals("BusinessTaxCash", 0m, reader.GetDecimal(6));
						AssertEquals("BusinessTaxNonCash", 0m, reader.GetDecimal(7));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestInvoiceTotal()
		{
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1, 2);

			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK3, cusEntryInstructionPK, entryLinePK3, 1, 3);

			Db.Connection.Command($@"
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '1',
					JZ_InvoiceAmount = 1000.01,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '2',
					JZ_InvoiceAmount = 2000.88,
					JZ_RX_NKInvoice_Currency = 'TWD',
					JZ_InvoiceCurrExRate = 1,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK2}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '3',
					JZ_InvoiceAmount = 3000.99,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK3}';
				INSERT INTO dbo.JobComInvHeaderCharge (J7_PK, J7_ParentID, J7_ParentTableCode, J7_IsValid, J7_ChargeType, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_SystemCreateTimeUtc, J7_SystemCreateUser, J7_SystemLastEditTimeUtc, J7_SystemLastEditUser) VALUES
				(NEWID(), '{invoiceHeaderPK}' ,'JZ', 1,'OFT', 0, 1, 100, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK2}','JZ', 1,'ONS', 1, 0, 200, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'DED', 0, 1, 300, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'ADD', 1, 0, 400, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP');").ExecuteNonQuery();

			var reportSql = @"SELECT InvoiceTotal FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("InvoiceTotal", true, reader.IsDBNull(0));
						Assert("record 2", reader.Read());
						AssertEquals("InvoiceTotal", true, reader.IsDBNull(0));
						Assert("record 3", reader.Read());
						AssertEquals("InvoiceTotal", true, reader.IsDBNull(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command(@$"
UPDATE dbo.JobDeclaration
SET
	JE_MessageType = 'EXP',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{declarationPK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("InvoiceTotal", 4170.47m, reader.GetDecimal(0));
						Assert("record 2", reader.Read());
						AssertEquals("InvoiceTotal", 4170.47m, reader.GetDecimal(0));
						Assert("record 3", reader.Read());
						AssertEquals("InvoiceTotal", 4170.47m, reader.GetDecimal(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command(@$"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '1',
	JZ_RX_NKInvoice_Currency = 'TWD',
	JZ_InvoiceCurrExRate = 1,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK}';").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("InvoiceTotal", 96889.83m, reader.GetDecimal(0));
						Assert("record 2", reader.Read());
						AssertEquals("InvoiceTotal", 96889.83m, reader.GetDecimal(0));
						Assert("record 3", reader.Read());
						AssertEquals("InvoiceTotal", 96889.83m, reader.GetDecimal(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestInvoiceTotalWhenTotalEXPDisbursedAmountIsNull()
		{
			Db.Connection.Command($@"
				UPDATE dbo.JobDeclaration
				SET
					JE_MessageType = 'EXP',
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE
					JE_PK = '{declarationPK}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '1',
					JZ_InvoiceAmount = 1000.01,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT InvoiceTotal FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("InvoiceTotal", 1000.01m, reader.GetDecimal(0));
					});
				}
			}
		}

		public void TestImportFOBAmount()
		{
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1, 2);

			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK3, cusEntryInstructionPK, entryLinePK3, 1, 3);

			Db.Connection.Command($@"
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '1',
					JZ_InvoiceAmount = 1000.01,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '2',
					JZ_InvoiceAmount = 2000.88,
					JZ_RX_NKInvoice_Currency = 'TWD',
					JZ_InvoiceCurrExRate = 1,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK2}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '3',
					JZ_InvoiceAmount = 3000.99,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK3}';
				INSERT INTO dbo.JobComInvHeaderCharge (J7_PK, J7_ParentID, J7_ParentTableCode, J7_IsValid, J7_ChargeType, J7_IsStatisticalValueApplicable, J7_IsIncludedInITOT, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_SystemCreateTimeUtc, J7_SystemCreateUser, J7_SystemLastEditTimeUtc, J7_SystemLastEditUser) VALUES
				(NEWID(), '{invoiceHeaderPK}' ,'JZ', 1,'OFT', 0, 1, 100, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK2}','JZ', 1,'ONS', 1, 0, 200, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'DED', 0, 1, 300, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'ADD', 1, 0, 400, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP');").ExecuteNonQuery();

			var reportSql = @"SELECT ImportFOBAmount FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("InvoiceTotal", 4170.47m, reader.GetDecimal(0));
						Assert("record 2", reader.Read());
						AssertEquals("InvoiceTotal", 4170.47m, reader.GetDecimal(0));
						Assert("record 3", reader.Read());
						AssertEquals("InvoiceTotal", 4170.47m, reader.GetDecimal(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command(@$"
UPDATE dbo.JobDeclaration
SET
	JE_MessageType = 'EXP',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{declarationPK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("InvoiceTotal", true, reader.IsDBNull(0));
						Assert("record 2", reader.Read());
						AssertEquals("InvoiceTotal", true, reader.IsDBNull(0));
						Assert("record 3", reader.Read());
						AssertEquals("InvoiceTotal", true, reader.IsDBNull(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command(@$"
UPDATE dbo.JobDeclaration
SET
	JE_MessageType = 'IMP',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{declarationPK}';
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '1',
	JZ_RX_NKInvoice_Currency = 'TWD',
	JZ_InvoiceCurrExRate = 1,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK}';").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("InvoiceTotal", 96889.83m, reader.GetDecimal(0));
						Assert("record 2", reader.Read());
						AssertEquals("InvoiceTotal", 96889.83m, reader.GetDecimal(0));
						Assert("record 3", reader.Read());
						AssertEquals("InvoiceTotal", 96889.83m, reader.GetDecimal(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestFOBCIFAmountTWD_IMP()
		{
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1, 2);

			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK3, cusEntryInstructionPK, entryLinePK3, 1, 3);

			Db.Connection.Command($@"
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '1',
					JZ_InvoiceAmount = 1000.01,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '2',
					JZ_InvoiceAmount = 2000.88,
					JZ_RX_NKInvoice_Currency = 'TWD',
					JZ_InvoiceCurrExRate = 1,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK2}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '3',
					JZ_InvoiceAmount = 3000.99,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK3}';
				INSERT INTO dbo.JobComInvHeaderCharge (J7_PK, J7_ParentID, J7_ParentTableCode, J7_IsValid, J7_ChargeType,J7_IsDutiable, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsStatisticalValueApplicable, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_SystemCreateTimeUtc, J7_SystemCreateUser, J7_SystemLastEditTimeUtc, J7_SystemLastEditUser) VALUES
				(NEWID(), '{invoiceHeaderPK}' ,'JZ', 1,'OFT', 1, 1, 0, 0, 10, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK2}','JZ', 1,'ONS', 1, 1, 0, 0, 20, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 0, 0, 30, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 0, 1, 40, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 1, 0, 50, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 1, 1, 60, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 0, 0, 70, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 0, 1, 80, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 1, 0, 90, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 1, 1, 100, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 0, 0, 110, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 0, 1, 120, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 1, 0, 130, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 1, 1, 140, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 0, 0, 150, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 0, 1, 160, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 1, 0, 170, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 1, 1, 180, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP');").ExecuteNonQuery();

			var reportSql = $@"SELECT ImportCIFAmountTWD, ExportFOBAmountTWD, Additions, Deductions FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions("JE_MessageType IS 'IMP'", () =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImportCIFAmountTWD", 123978m, reader.GetDecimal(0));
						AssertEquals("ExportFOBAmountTWD", true, reader.IsDBNull(1));
						AssertEquals("Additions", 8.6m, reader.GetDecimal(2));
						AssertEquals("Deductions", 5.29m, reader.GetDecimal(3));
						Assert("record 2", reader.Read());
						AssertEquals("ImportCIFAmountTWD", 123978m, reader.GetDecimal(0));
						AssertEquals("ExportFOBAmountTWD", true, reader.IsDBNull(1));
						AssertEquals("Additions", 8.6m, reader.GetDecimal(2));
						AssertEquals("Deductions", 5.29m, reader.GetDecimal(3));
						Assert("record 3", reader.Read());
						AssertEquals("ImportCIFAmountTWD", 123978m, reader.GetDecimal(0));
						AssertEquals("ExportFOBAmountTWD", true, reader.IsDBNull(1));
						AssertEquals("Additions", 8.6m, reader.GetDecimal(2));
						AssertEquals("Deductions", 5.29m, reader.GetDecimal(3));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestFOBCIFAmountTWD_EXP()
		{
			Db.Connection.Command($@"
UPDATE dbo.JobDeclaration
SET
	JE_MessageType = 'EXP',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{declarationPK}';
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_IncoTerm = 'FOB',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK}';").ExecuteNonQuery();

			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1, 2);

			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK3, cusEntryInstructionPK, entryLinePK3, 1, 3);

			Db.Connection.Command($@"
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '1',
					JZ_InvoiceAmount = 1000.01,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '2',
					JZ_InvoiceAmount = 2000.88,
					JZ_RX_NKInvoice_Currency = 'TWD',
					JZ_InvoiceCurrExRate = 1,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK2}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '3',
					JZ_InvoiceAmount = 3000.99,
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK3}';
				INSERT INTO dbo.JobComInvHeaderCharge (J7_PK, J7_ParentID, J7_ParentTableCode, J7_IsValid, J7_ChargeType,J7_IsDutiable, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsStatisticalValueApplicable, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_SystemCreateTimeUtc, J7_SystemCreateUser, J7_SystemLastEditTimeUtc, J7_SystemLastEditUser) VALUES
				(NEWID(), '{invoiceHeaderPK}' ,'JZ', 1,'OFT', 0, 0, 0, 0, 10, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK2}','JZ', 1,'ONS', 0, 0, 0, 0, 20, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 0, 0, 30, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 0, 1, 40, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 1, 0, 50, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 0, 1, 1, 60, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 0, 0, 70, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 0, 1, 80, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 1, 0, 90, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 0, 1, 1, 1, 100, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 0, 0, 110, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 0, 1, 120, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 1, 0, 130, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 0, 1, 1, 140, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 0, 0, 150, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 0, 1, 160, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 1, 0, 170, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OTH', 1, 1, 1, 1, 180, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP');").ExecuteNonQuery();

			var reportSql = $@"SELECT ImportCIFAmountTWD, ExportFOBAmountTWD, Additions, Deductions FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions("JE_MessageType IS 'EXP' AND JZ_IncoTerm IS NOT EXW", () =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImportCIFAmountTWD", true, reader.IsDBNull(0));
						AssertEquals("ExportFOBAmountTWD", 122304m, reader.GetDecimal(1));
						AssertEquals("Additions", 4.96m, reader.GetDecimal(2));
						AssertEquals("Deductions", 8.93m, reader.GetDecimal(3));
						Assert("record 2", reader.Read());
						AssertEquals("ImportCIFAmountTWD", true, reader.IsDBNull(0));
						AssertEquals("ExportFOBAmountTWD", 122304m, reader.GetDecimal(1));
						AssertEquals("Additions", 4.96m, reader.GetDecimal(2));
						AssertEquals("Deductions", 8.93m, reader.GetDecimal(3));
						Assert("record 3", reader.Read());
						AssertEquals("ImportCIFAmountTWD", true, reader.IsDBNull(0));
						AssertEquals("ExportFOBAmountTWD", 122304m, reader.GetDecimal(1));
						AssertEquals("Additions", 4.96m, reader.GetDecimal(2));
						AssertEquals("Deductions", 8.93m, reader.GetDecimal(3));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_IncoTerm = 'EXW',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK}';").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions("JE_MessageType IS 'EXP' AND JZ_IncoTerm IS EXW", () =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImportCIFAmountTWD", true, reader.IsDBNull(0));
						AssertEquals("ExportFOBAmountTWD", 123331m, reader.GetDecimal(1));
						AssertEquals("Additions", 7.94m, reader.GetDecimal(2));
						AssertEquals("Deductions", 0m, reader.GetDecimal(3));
						Assert("record 2", reader.Read());
						AssertEquals("ImportCIFAmountTWD", true, reader.IsDBNull(0));
						AssertEquals("ExportFOBAmountTWD", 123331m, reader.GetDecimal(1));
						AssertEquals("Additions", 7.94m, reader.GetDecimal(2));
						AssertEquals("Deductions", 0m, reader.GetDecimal(3));
						Assert("record 3", reader.Read());
						AssertEquals("ImportCIFAmountTWD", true, reader.IsDBNull(0));
						AssertEquals("ExportFOBAmountTWD", 123331m, reader.GetDecimal(1));
						AssertEquals("Additions", 7.94m, reader.GetDecimal(2));
						AssertEquals("Deductions", 0m, reader.GetDecimal(3));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestFreightAndInsurance()
		{
			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1, 2);

			var invoiceHeaderPK3 = CreateJobComInvoiceHeader(declarationPK, 1);
			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK3, cusEntryInstructionPK, entryLinePK3, 1, 3);

			Db.Connection.Command($@"
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '1',
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '2',
					JZ_RX_NKInvoice_Currency = 'TWD',
					JZ_InvoiceCurrExRate = 1,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK2}';
				UPDATE dbo.JobComInvoiceHeader
				SET
					JZ_InvoiceNumber = '3',
					JZ_RX_NKInvoice_Currency = 'USD',
					JZ_InvoiceCurrExRate = 30.245,
					JZ_SystemLastEditTimeUtc = GETUTCDATE(),
					JZ_SystemLastEditUser = '~BP'
				WHERE
					JZ_PK = '{invoiceHeaderPK3}';
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_LinePrice = 100,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK}';
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_LinePrice = 200,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK2}';
				UPDATE dbo.JobComInvoiceLine
				SET
					JI_LinePrice = 303,
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK3}';
				INSERT INTO dbo.JobComInvHeaderCharge (J7_PK, J7_ParentID, J7_ParentTableCode, J7_IsValid, J7_ChargeType, J7_Amount, J7_RX_NKCurrency, J7_ExchangeRate, J7_SystemCreateTimeUtc, J7_SystemCreateUser, J7_SystemLastEditTimeUtc, J7_SystemLastEditUser) VALUES
				(NEWID(), '{invoiceHeaderPK}' ,'JZ', 1,'OFT', 10, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK2}','JZ', 1,'ONS', 20, 'USD', 30.245,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'OFT', 30, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), '{invoiceHeaderPK3}','JZ', 1,'ONS', 40, 'TWD', 1,  getutcdate(), '~BP', getutcdate(), '~BP');").ExecuteNonQuery();

			var reportSql = $@"SELECT Freight, Insurance FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("Freight", 10.99m, reader.GetDecimal(0));
						AssertEquals("Insurance", 21.32m, reader.GetDecimal(1));
						Assert("record 2", reader.Read());
						AssertEquals("Freight", 10.99m, reader.GetDecimal(0));
						AssertEquals("Insurance", 21.32m, reader.GetDecimal(1));
						Assert("record 3", reader.Read());
						AssertEquals("Freight", 10.99m, reader.GetDecimal(0));
						AssertEquals("Insurance", 21.32m, reader.GetDecimal(1));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestTotalTaxAmountCash()
		{
			var feePK11 = TestDataCreator.CreateCusEntryLineFee(entryLinePK, "A99", 100f, 1);
			var feePK12 = TestDataCreator.CreateCusEntryLineFee(entryLinePK, "A99", 200f, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var feePK21 = TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 30f, 1);
			TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 40f, 1);

			Db.Connection.Command($@"
INSERT INTO dbo.CusEntryHeaderCharges 
    (C1_PK, C1_CH, C1_ChargeAmount, C1_MethodOfPayment, C1_ClusterKey, C1_ChargeType, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser) 
VALUES
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

UPDATE dbo.CusEntryLineFee 
SET 
    CF_MethodOfPayment = 'CAS', 
    CF_SystemLastEditTimeUtc = GETUTCDATE(), 
    CF_SystemLastEditUser = '~BP' 
WHERE 
    CF_PK IN ('{feePK11}', '{feePK12}', '{feePK21}');").ExecuteNonQuery();

			var reportSql = @"SELECT TotalTaxAmountCash FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals("TotalTaxAmountCash", 430m, reader.GetDecimal(0));
					Assert("record 2", reader.Read());
					AssertEquals("TotalTaxAmountCash", 430m, reader.GetDecimal(0));
					Assert("There should be no other records", !reader.Read());
				}
			}

			CreateCusEntryPayInfo(entryHeaderPK, "111111", "A10", 1m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "A19", 2m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B40", 3m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B49", 4m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B51", 5m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B52", 6m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B59", 7m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "A20", 8m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "A30", 9m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "A40", 10m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "A50", 11m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B10", 12m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B31", 13m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B32", 14m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B60", 15m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "C10", 16m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "C20", 17m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B19", 18m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B69", 19m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B79", 20m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B89", 21m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "222222", "B29", 22m, 1);

			reportSql = @"SELECT TotalTaxAmountCash FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals("TotalTaxAmountCash", 253m, reader.GetDecimal(0));
					Assert("record 2", reader.Read());
					AssertEquals("TotalTaxAmountCash", 253m, reader.GetDecimal(0));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTotalTaxAmountNonCash()
		{
			var feePK11 = TestDataCreator.CreateCusEntryLineFee(entryLinePK, "A99", 100f, 1);
			var feePK12 = TestDataCreator.CreateCusEntryLineFee(entryLinePK, "A99", 200f, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var feePK21 = TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 30f, 1);
			TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 40f, 1);

			Db.Connection.Command($@"
INSERT INTO dbo.CusEntryHeaderCharges 
    (C1_PK, C1_CH, C1_ChargeAmount, C1_MethodOfPayment, C1_ClusterKey, C1_ChargeType, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser) 
VALUES
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

UPDATE dbo.CusEntryLineFee 
SET 
    CF_MethodOfPayment = 'DEF', 
    CF_SystemLastEditTimeUtc = GETUTCDATE(), 
    CF_SystemLastEditUser = '~BP' 
WHERE 
    CF_PK IN ('{feePK11}', '{feePK12}', '{feePK21}');").ExecuteNonQuery();

			var reportSql = @"SELECT TotalTaxAmountNonCash FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals("TotalTaxAmountNonCash", 530m, reader.GetDecimal(0));
					Assert("record 2", reader.Read());
					AssertEquals("TotalTaxAmountNonCash", 530m, reader.GetDecimal(0));
					Assert("There should be no other records", !reader.Read());
				}
			}

			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C21", 11m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C22", 12m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C23", 13m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C24", 14m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C25", 15m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C31", 16m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C32", 17m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C33", 18m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "C34", 19m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "D10", 20m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F10", 21m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F12", 22m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F13", 23m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F14", 24m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F15", 25m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F16", 26m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F20", 27m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F21", 28m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F22", 29m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F23", 30m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F30", 31m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F31", 32m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F40", 33m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F50", 34m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F51", 35m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F52", 36m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F55", 37m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F56", 38m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F57", 39m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F58", 40m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F88", 41m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "F99", 42m, 1);
			CreateCusEntryPayInfo(entryHeaderPK, "111111", "X00", 43m, 1);
			reportSql = @"SELECT TotalTaxAmountNonCash FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals("TotalTaxAmountNonCash", 891m, reader.GetDecimal(0));
					Assert("record 2", reader.Read());
					AssertEquals("TotalTaxAmountNonCash", 891m, reader.GetDecimal(0));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestBusinessTaxBase()
		{
			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK1 = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK1, 1, 2);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK2, 1, 3);
			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK, 1);
			CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK3, 1, 4);

			Db.Connection.Command($@"
UPDATE dbo.JobComInvoiceLine 
SET 
    JI_AddInfo = 'VatPymntMthd=CAS',
    JI_SystemLastEditTimeUtc = GetUtcDate(),
    JI_SystemLastEditUser = '~BP'
WHERE 
    JI_PK = '{invoiceLinePK1}';

UPDATE dbo.JobComInvoiceLine 
SET 
    JI_AddInfo = 'VatPymntMthd=CAS',
    JI_SystemLastEditTimeUtc = GetUtcDate(),
    JI_SystemLastEditUser = '~BP'
WHERE 
    JI_PK = '{invoiceLinePK2}';

UPDATE dbo.CusEntryLine 
SET 
    CL_ValueForVAT = 100.3, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK1}';

UPDATE dbo.CusEntryLine 
SET 
    CL_ValueForVAT = 200.4, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK2}';

UPDATE dbo.CusEntryLine 
SET 
    CL_ValueForVAT = 30, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK3}';").ExecuteNonQuery();

			var reportSql = @"SELECT BusinessTaxBase FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals("BusinessTaxBase", 300m, reader.GetDecimal(0));
					Assert("There should be other records", reader.Read());
					AssertEquals("BusinessTaxBase", 300m, reader.GetDecimal(0));
					Assert("There should be other records", reader.Read());
					AssertEquals("BusinessTaxBase", 300m, reader.GetDecimal(0));
					Assert("There should be other records", reader.Read());
					AssertEquals("BusinessTaxBase", 300m, reader.GetDecimal(0));
					Assert("There should be no other records", !reader.Read());
				}
			}

			var cusEntryPayInfoPK1 = CreateCusEntryPayInfo(entryHeaderPK, "111111", "A10", 10m, 1);
			CreateGenAddOnColumn(cusEntryPayInfoPK1, "OtherChargeDeductionAmount", 111m);
			var cusEntryPayInfoPK2 = CreateCusEntryPayInfo(entryHeaderPK, "111111", "A19", 10m, 1);
			CreateGenAddOnColumn(cusEntryPayInfoPK2, "OtherChargeDeductionAmount", 111m);
			var cusEntryPayInfoPK3 = CreateCusEntryPayInfo(entryHeaderPK, "222222", "B40", 10m, 1);
			CreateGenAddOnColumn(cusEntryPayInfoPK3, "OtherChargeDeductionAmount", 222m);
			var cusEntryPayInfoPK4 = CreateCusEntryPayInfo(entryHeaderPK, "222222", "B49", 10m, 1);
			CreateGenAddOnColumn(cusEntryPayInfoPK4, "OtherChargeDeductionAmount", 222m);

			reportSql = @"SELECT BusinessTaxBase FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals("BusinessTaxBase", 333m, reader.GetDecimal(0));
					Assert("There should be other records", reader.Read());
					AssertEquals("BusinessTaxBase", 333m, reader.GetDecimal(0));
					Assert("There should be other records", reader.Read());
					AssertEquals("BusinessTaxBase", 333m, reader.GetDecimal(0));
					Assert("There should be other records", reader.Read());
					AssertEquals("BusinessTaxBase", 333m, reader.GetDecimal(0));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestCompanyCondition()
		{
			var reportSql = @"SELECT JE_PK FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					Assert("Branch not match", !reader.Read());
				}
			}
		}

		public void TestGoodsDescription_AircraftParts()
		{
			Db.Connection.Command($@"
				UPDATE
					dbo.JobComInvoiceLine
				SET
					JI_NDescription = N'Chinese描述',
					JI_Description = 'English',
					JI_NAddInfo = N'Group=影像產品GroupingTest*Compositions=CompositionsTest',
					JI_SystemLastEditTimeUtc = GETUTCDATE(),
					JI_SystemLastEditUser = '~BP'
				WHERE
					JI_PK = '{invoiceLinePK}'").ExecuteNonQuery();

			var reportSql = @"SELECT GoodsDescription FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nChinese描述\nEnglish", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}

			Db.Connection.Command($@"
				UPDATE
					dbo.JobTWComInvoiceLine
				SET
					TWL_AircraftIPC = 'IPC',
					TWL_SystemLastEditTimeUtc = GETUTCDATE(),
					TWL_SystemLastEditUser = '~BP'
				WHERE
					TWL_PK = '{jobTWComInvoiceLinePK}'").ExecuteNonQuery();

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("GoodsDescription", "影像產品GroupingTest\n\nChinese描述English[||IPC||||]", reader.GetString(0));
						Assert("There should be no other records", !reader.Read());
					});
				}
			}
		}

		public void TestGoodsDescription()
		{
			var declarationPK654 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 654, "B00001519");
			var cusEntryInstructionPK654 = CreateCusEntryInstruction(declarationPK654, new DateTime(2023, 06, 26), 654);
			var entryHeaderPK654 = CreateCusEntryHeader(declarationPK654, "", 654);
			var entryLinePK654_1 = CreateCusEntryLine(entryHeaderPK654, 654, 1);
			var entryLinePK654_2 = CreateCusEntryLine(entryHeaderPK654, 654, 2);
			var invoiceHeaderPK654 = CreateJobComInvoiceHeader(declarationPK654, 654);
			var invoiceLinePK654_1 = CreateJobComInvoiceLine(invoiceHeaderPK654, cusEntryInstructionPK654, entryLinePK654_1, 654, 1, "22");
			var invoiceLinePK654_2 = CreateJobComInvoiceLine(invoiceHeaderPK654, cusEntryInstructionPK654, entryLinePK654_2, 654, 2, "22");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK654, "CusEntryHeader", "CABG1299900057", "IMP", "CUS", "TW");

			var declarationPK655 = CreateJobDeclaration(branchPK, companyPK, "EXP", "AIR", "E", "E", 655, "B00001520");
			var cusEntryInstructionPK655 = CreateCusEntryInstruction(declarationPK655, new DateTime(2023, 06, 26), 655);
			var entryHeaderPK655 = CreateCusEntryHeader(declarationPK655, "", 655);
			var entryLinePK655_1 = CreateCusEntryLine(entryHeaderPK655, 655, 1);
			var invoiceHeaderPK655 = CreateJobComInvoiceHeader(declarationPK655, 655);
			var invoiceLinePK655_1 = CreateJobComInvoiceLine(invoiceHeaderPK655, cusEntryInstructionPK655, entryLinePK655_1, 655, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK655, "CusEntryHeader", "CABG1299900031", "EXP", "CUS", "TW");

			var declarationPK656 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 656, "B00001521");
			var cusEntryInstructionPK656 = CreateCusEntryInstruction(declarationPK656, new DateTime(2023, 06, 29), 656);
			var entryHeaderPK656 = CreateCusEntryHeader(declarationPK656, "", 656);
			var entryLinePK656_1 = CreateCusEntryLine(entryHeaderPK656, 656, 1);
			var invoiceHeaderPK656 = CreateJobComInvoiceHeader(declarationPK656, 656);
			var invoiceLinePK656_1 = CreateJobComInvoiceLine(invoiceHeaderPK656, cusEntryInstructionPK656, entryLinePK656_1, 656, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK656, "CusEntryHeader", "CABG1299900058", "IMP", "CUS", "TW");

			var declarationPK657 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 657, "B00001522");
			var cusEntryInstructionPK657 = CreateCusEntryInstruction(declarationPK657, new DateTime(2023, 06, 30), 657);
			var entryHeaderPK657 = CreateCusEntryHeader(declarationPK657, "", 657);
			var entryLinePK657_1 = CreateCusEntryLine(entryHeaderPK657, 657, 1);
			var invoiceHeaderPK657 = CreateJobComInvoiceHeader(declarationPK657, 657);
			var invoiceLinePK657_1 = CreateJobComInvoiceLine(invoiceHeaderPK657, cusEntryInstructionPK657, entryLinePK657_1, 657, 1);

			var declarationPK661 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 661, "B00001526");
			var cusEntryInstructionPK661 = CreateCusEntryInstruction(declarationPK661, new DateTime(2023, 07, 10), 661);
			var entryHeaderPK661 = CreateCusEntryHeader(declarationPK661, "", 661);
			var entryLinePK661_1 = CreateCusEntryLine(entryHeaderPK661, 661, 1);
			var entryLinePK661_2 = CreateCusEntryLine(entryHeaderPK661, 661, 2);
			var entryLinePK661_3 = CreateCusEntryLine(entryHeaderPK661, 661, 3);
			var entryLinePK661_4 = CreateCusEntryLine(entryHeaderPK661, 661, 4);
			var invoiceHeaderPK661 = CreateJobComInvoiceHeader(declarationPK661, 661);
			var invoiceLinePK661_1 = CreateJobComInvoiceLine(invoiceHeaderPK661, cusEntryInstructionPK661, entryLinePK661_1, 661, 1, "CTA TAX");
			var invoiceLinePK661_2 = CreateJobComInvoiceLine(invoiceHeaderPK661, cusEntryInstructionPK661, entryLinePK661_2, 661, 2, "TATCTA TAX");
			var invoiceLinePK661_3 = CreateJobComInvoiceLine(invoiceHeaderPK661, cusEntryInstructionPK661, entryLinePK661_3, 661, 3, "TAT/ HWS TAX");
			var invoiceLinePK661_4 = CreateJobComInvoiceLine(invoiceHeaderPK661, cusEntryInstructionPK661, entryLinePK661_4, 661, 4, "SSG/ADD/ADT/CVD/RTDTAT/ HWS TAX");

			var declarationPK673 = CreateJobDeclaration(branchPK, companyPK, "EXP", "AIR", "E", "E", 673, "B00001535");
			var cusEntryInstructionPK673 = CreateCusEntryInstruction(declarationPK673, new DateTime(2023, 07, 27), 673);
			var entryHeaderPK673 = CreateCusEntryHeader(declarationPK673, "", 673);
			var entryLinePK673_1 = CreateCusEntryLine(entryHeaderPK673, 673, 1);
			var invoiceHeaderPK673 = CreateJobComInvoiceHeader(declarationPK673, 673);
			var invoiceLinePK673_1 = CreateJobComInvoiceLine(invoiceHeaderPK673, cusEntryInstructionPK673, entryLinePK673_1, 673, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK673, "CusEntryHeader", "CABF1299900032", "EXP", "CUS", "TW");

			var declarationPK674 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 674, "B00001536", "TRF");
			var cusEntryInstructionPK674 = CreateCusEntryInstruction(declarationPK674, new DateTime(2023, 07, 27), 674);
			var entryHeaderPK674 = CreateCusEntryHeader(declarationPK674, "", 674);
			var entryLinePK674_1 = CreateCusEntryLine(entryHeaderPK674, 674, 1);
			var invoiceHeaderPK674 = CreateJobComInvoiceHeader(declarationPK674, 674);
			var invoiceLinePK674_1 = CreateJobComInvoiceLine(invoiceHeaderPK674, cusEntryInstructionPK674, entryLinePK674_1, 674, 1, "2224");
			var invoiceLinePK674_2 = CreateJobComInvoiceLine(invoiceHeaderPK674, cusEntryInstructionPK674, entryLinePK674_1, 674, 2, "1111");
			TestDataCreator.CreateCusEntryNum(declarationPK674, "JobDeclaration", "CABG1299900074", "IMP", "CUS", "TW");

			Db.Connection.Command(@$"
UPDATE dbo.CusEntryLine
SET
	CL_Description = 'SUMMAR1, INCLUDING ITEMS 1-2',
	CL_SystemLastEditTimeUtc = GETUTCDATE(),
	CL_SystemLastEditUser = 'E'
WHERE
	CL_PK = '{entryLinePK674_1}'").ExecuteNonQuery();

			var declarationPK675 = CreateJobDeclaration(branchPK, companyPK, "EXP", "AIR", "E", "E", 675, "B00001537");
			var cusEntryInstructionPK675 = CreateCusEntryInstruction(declarationPK675, new DateTime(2023, 08, 03), 675);
			var entryHeaderPK675 = CreateCusEntryHeader(declarationPK675, "", 675);
			var entryLinePK675_1 = CreateCusEntryLine(entryHeaderPK675, 675, 1);
			var invoiceHeaderPK675 = CreateJobComInvoiceHeader(declarationPK675, 675);
			var invoiceLinePK675_1 = CreateJobComInvoiceLine(invoiceHeaderPK675, cusEntryInstructionPK675, entryLinePK675_1, 675, 1);
			TestDataCreator.CreateCusEntryNum(declarationPK675, "JobDeclaration", "AA  1288800038", "EXP", "CUS", "TW");

			var declarationPK676 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 676, "B00001538");
			var cusEntryInstructionPK676 = CreateCusEntryInstruction(declarationPK676, new DateTime(2023, 08, 04), 676);
			var entryHeaderPK676 = CreateCusEntryHeader(declarationPK676, "", 676);
			var entryLinePK676_1 = CreateCusEntryLine(entryHeaderPK676, 676, 1);
			var invoiceHeaderPK676 = CreateJobComInvoiceHeader(declarationPK676, 676);
			var invoiceLinePK676_1 = CreateJobComInvoiceLine(invoiceHeaderPK676, cusEntryInstructionPK676, entryLinePK676_1, 676, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK676, "CusEntryHeader", "AABE12999E0006", "IMP", "CUS", "TW");

			var declarationPK677 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 677, "B00001539");
			var cusEntryInstructionPK677 = CreateCusEntryInstruction(declarationPK677, new DateTime(2023, 08, 17), 677);
			var entryHeaderPK677 = CreateCusEntryHeader(declarationPK677, "", 677);
			var entryLinePK677_1 = CreateCusEntryLine(entryHeaderPK677, 677, 1);
			var invoiceHeaderPK677 = CreateJobComInvoiceHeader(declarationPK677, 677);
			var invoiceLinePK677_1 = CreateJobComInvoiceLine(invoiceHeaderPK677, cusEntryInstructionPK677, entryLinePK677_1, 677, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK677, "CusEntryHeader", "CA  1299900076", "IMP", "CUS", "TW");

			var declarationPK678 = CreateJobDeclaration(branchPK, companyPK, "EXP", "AIR", "E", "E", 678, "B00001540");
			var cusEntryInstructionPK678 = CreateCusEntryInstruction(declarationPK678, new DateTime(2023, 08, 18), 678);
			var entryHeaderPK678 = CreateCusEntryHeader(declarationPK678, "", 678);
			var entryLinePK678_1 = CreateCusEntryLine(entryHeaderPK678, 678, 1);
			var invoiceHeaderPK678 = CreateJobComInvoiceHeader(declarationPK678, 678);
			var invoiceLinePK678_1 = CreateJobComInvoiceLine(invoiceHeaderPK678, cusEntryInstructionPK678, entryLinePK678_1, 678, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK678, "CusEntryHeader", "CA  1299900039", "EXP", "CUS", "TW");

			var declarationPK679 = CreateJobDeclaration(branchPK, companyPK, "IMP", "AIR", "E", "E", 679, "B00001541");
			var cusEntryInstructionPK679 = CreateCusEntryInstruction(declarationPK679, new DateTime(2023, 08, 18), 679);
			var entryHeaderPK679 = CreateCusEntryHeader(declarationPK679, "", 679);
			var entryLinePK679_1 = CreateCusEntryLine(entryHeaderPK679, 679, 1);
			var invoiceHeaderPK679 = CreateJobComInvoiceHeader(declarationPK679, 679);
			var invoiceLinePK679_1 = CreateJobComInvoiceLine(invoiceHeaderPK679, cusEntryInstructionPK679, entryLinePK679_1, 679, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK679, "CusEntryHeader", "CA  1299900077", "IMP", "CUS", "TW");

			var reportSql = @"SELECT JobNumber, EntryLineNo, EntryNumber, GoodsDescription FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate) ORDER BY EntryNumber ASC";
			using var command = Db.Connection.Command(reportSql);
			command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
			command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
			command.AddParameter("@transportMode", SqlDbType.VarChar, "AIR");
			command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, new DateTime(2023, 06, 01));
			command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, new DateTime(2023, 08, 30));
			var data = DataUtils.GetDataTableFromCommand(command);
			CombineAssertions(() =>
			{
				AssertEquals("B00001526 Line 1", true, data.Select("JobNumber='B00001526' and EntryLineNo = 1 and GoodsDescription = 'CTA TAX'").Length != 0);
				AssertEquals("B00001526 Line 2", true, data.Select("JobNumber='B00001526' and EntryLineNo = 2 and GoodsDescription = 'TATCTA TAX'").Length != 0);
				AssertEquals("B00001526 Line 3", true, data.Select("JobNumber='B00001526' and EntryLineNo = 3 and GoodsDescription = 'TAT/ HWS TAX'").Length != 0);
				AssertEquals("B00001526 Line 4", true, data.Select("JobNumber='B00001526' and EntryLineNo = 4 and GoodsDescription = 'SSG/ADD/ADT/CVD/RTDTAT/ HWS TAX'").Length != 0);
			});
		}

		public void TestSupplierEnglishName_NonAddressOverride()
		{
			var supplierPK = TestDataCreator.CreateOrganisation("Code32", "SupplierName");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "AddressCode", "Address1");
			TestDataCreator.CreateDocAddress(supplierAddressPK, "Org Override SUD", declarationPK, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);

			var reportSql = @"SELECT SupplierEnglishName FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("SupplierEnglishName when OA_CompanyNameOverride is empty", "SupplierName", reader.GetString(0));
					});
				}
			}
		}

		public void TestImporterEnglishName_NonAddressOverride()
		{
			var importerPK = TestDataCreator.CreateOrganisation("Code12", "ImporterName");
			var importerAddressPK = TestDataCreator.CreateAddress(importerPK, "AddressCode", "Address1");
			TestDataCreator.CreateDocAddress(importerAddressPK, "Org Override IMD", declarationPK, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);
			var reportSql = @"SELECT ImporterEnglishName FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, "");
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("record 1", reader.Read());
						AssertEquals("ImporterEnglishName when OA_CompanyNameOverride is empty", "ImporterName", reader.GetString(0));
					});
				}
			}
		}

		public void TestEntryNumberCondition()
		{
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "ABAM10123BBBB2", "IMP", "CUS", "TW");

			declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 2);
			cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2021, 12, 12), 2);
			entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 2);
			entryLinePK = CreateCusEntryLine(entryHeaderPK, 2);
			invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 2);
			invoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK, 2, 2);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "BCBN21234CCCC3", "IMP", "CUS", "TW");

			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "ABAM10123BBBB2", 1);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "AB/AM/10/123/BBBB2", 1);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "BCBN21234CCCC3", 1);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "BC/BN/21/234/CCCC3", 1);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "", 2);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "ABAM10123BBBB3", 0);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "AB/AM/10/123/BBBB3", 0);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "BCBN21234CCCC4", 0);
			AssertTWCustomsEntriesInvoiceLinesCount(companyPK, "CC/BN/21/234/CCCC3", 0);
		}

		const string reportSql = @"select JE_PK FROM Report_TWEntriesAndLines(@companyPK, @entryNumber, @shipmentType, @transportMode, @declarationFromDate, @declarationToDate)";

		void AssertTWCustomsEntriesInvoiceLinesCount(Guid companyPK, string entryNumber, int expectCount)
		{
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryNumber", SqlDbType.VarChar, entryNumber);
				command.AddParameter("@shipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@transportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@declarationFromDate", SqlDbType.SmallDateTime, DBNull.Value);
				command.AddParameter("@declarationToDate", SqlDbType.SmallDateTime, DBNull.Value);
				using (var reader = command.ExecuteReader())
				{
					for (int i = 0; i < expectCount; i++)
					{
						Assert("There should be a record", reader.Read());
					}
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		Guid companyPK;
		Guid branchPK;
		Guid declarationPK;
		Guid cusEntryInstructionPK;
		Guid entryHeaderPK;
		Guid entryLinePK;
		Guid invoiceHeaderPK;
		Guid invoiceLinePK;
		Guid jobTWComInvoiceLinePK;

		protected override bool RequiresSchemaBinding => false;

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("TC1", "TW", "NTD");
			branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "TAJNB");
			declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2021, 12, 12), 1);
			entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			entryLinePK = CreateCusEntryLine(entryHeaderPK, 1);
			invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 1);
			invoiceLinePK = CreateJobComInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK, 1, 1);
			jobTWComInvoiceLinePK = CreateJobTWComInvoiceLine(invoiceLinePK, 1);
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string messageType, string transportMode, string createUser, string lastEditUser, int clusterKey, string declarationReference = "", string mergeBy = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationRef = string.IsNullOrEmpty(declarationReference) ? Guid.NewGuid().ToString("n") : declarationReference;
			var sql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_TransportMode, JE_SystemCreateUser, JE_SystemLastEditUser, JE_ClusterKey, JE_DeclarationReference, JE_MergeBy)
				VALUES (@declarationPK, 'TW', @messageType, @branchPK, @companyPK, @transportMode, @createUser, @lastEditUser, @clusterKey, @declarationReference, @mergeBy)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobDeclarationSchema.JE_TransportMode.MaxLength, transportMode);
				command.AddParameter("@createUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationRef);
				command.AddParameter("@mergeBy", SqlDbType.VarChar, mergeBy);

				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusEntryInstruction(Guid declarationPK, DateTime dateForDuty, int clusterKey)
		{
			var entryInstructionPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
				VALUES (@entryInstructionPK, 'TW', @declarationPK, @dateForDuty, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dateForDuty", SqlDbType.SmallDateTime, CusEntryInstructionSchema.CEI_DateForDuty.MaxLength, dateForDuty);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryInstructionPK;
		}

		Guid CreateJobComInvoiceHeader(Guid declarationPK, int clusterKey)
		{
			var invoiceHeaderPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_GroupInvoice)
				VALUES (@invoiceHeaderPK, 'TW', @declarationPK, @clusterKey, 0)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return invoiceHeaderPK;
		}

		Guid CreateJobComInvoiceLine(Guid invoiceHeaderPK, Guid entryInstructionPK, Guid entryLinePK, int clusterKey, int lineNo, string englishDesc = "")
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_CL, JI_CEI, JI_ClusterKey, JI_LineNo, JI_Description)
				VALUES (@invoiceLinePK, 'TW', @invoiceHeaderPK, @entryLinePK, @entryInstructionPK, @clusterKey, 1, @desc)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				if (entryLinePK == Guid.Empty)
				{
					command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				}
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNo", SqlDbType.Int, lineNo);
				command.AddParameter("@desc", SqlDbType.VarChar, englishDesc);
				command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}

		Guid CreateJobTWComInvoiceLine(Guid linePK, int clusterKey)
		{
			var jobTWComInvoiceLinePK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.JobTWComInvoiceLine (TWL_PK, TWL_JI, TWL_ClusterKey, TWL_SystemCreateTimeUtc, TWL_SystemCreateUser, TWL_SystemLastEditTimeUtc, TWL_SystemLastEditUser)
				VALUES (@invoiceTWLinePK, @invoiceLinePK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceTWLinePK", SqlDbType.UniqueIdentifier, jobTWComInvoiceLinePK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return jobTWComInvoiceLinePK;
		}

		Guid CreateCusEntryHeader(Guid declarationPK, string entryStatus, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_EntryStatus, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
				VALUES (@entryHeaderPK, 'TW', @declarationPK, @entryStatus, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, CusEntryHeaderSchema.CH_EntryStatus.MaxLength, entryStatus);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		Guid CreateStmNote(Guid invoiceLinePK, string noteText)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Description, ST_Table, ST_NoteText)
				VALUES (@stmNotePK, @invoiceLinePK, @description, 'JobComInvoiceLine', @noteText)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@stmNotePK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@description", SqlDbType.VarChar, "Declaration Goods Description");
				command.AddParameter("@noteText", SqlDbType.VarChar, noteText);
				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		void CreateJobComInvLineRefs(Guid invoiceLinePK, string referenceNumber, string referenceType, int clusterKey)
		{
			var sql = @"INSERT INTO dbo.JobComInvLineRefs (JG_PK, JG_JI, JG_ReferenceNumber, JG_ReferenceType, JG_ClusterKey)
				VALUES (@JobComInvLineRefsPK, @invoiceLinePK, @referenceNumber, @referenceType, @clusterKey)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JobComInvLineRefsPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@referenceNumber", SqlDbType.VarChar, referenceNumber);
				command.AddParameter("@referenceType", SqlDbType.VarChar, referenceType);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
		}

		void CreateCussupportinginfo(Guid invoiceLinePK, string referenceNumber, int lineNo, string type, int itemNumber)
		{
			var sql = @"INSERT INTO dbo.CusSupportinginfo (CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_DataModel, CSI_ReferenceNumber, CSI_LineNo, CSI_type, CSI_ItemNumber)
				VALUES (@cusSupportinginfoPK, @invoiceLinePK, 'JI', 'TW', @referenceNumber, @lineNo, @type, @itemNumber)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusSupportinginfoPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@referenceNumber", SqlDbType.VarChar, referenceNumber);
				command.AddParameter("@lineNo", SqlDbType.SmallInt, lineNo);
				command.AddParameter("@type", SqlDbType.VarChar, type);
				command.AddParameter("@itemNumber", SqlDbType.SmallInt, itemNumber);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateCusEntryLine(Guid entryHeaderPK, int clusterKey, int lineNumber = 1)
		{
			var cusEntryLinePK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_LineNumber, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
				VALUES (@cusEntryLinePK, 'TW', @entryHeaderPK, @clusterKey, @lineNumber, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNumber", SqlDbType.SmallInt, lineNumber);
				command.ExecuteNonQuery();
			}
			return cusEntryLinePK;
		}

		void CreateCusEntryLineFee(Guid cusEntryLinePK, string chargeType, Decimal chargeAmount, string methodOfPayment, int clusterKey)
		{
			var sql = @"INSERT INTO dbo.CusEntryLineFee (CF_PK, CF_CL, CF_ChargeType, CF_ChargeAmount, CF_MethodOfPayment, CF_ClusterKey, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser)
				VALUES (@cusEntryLineFeePK, @cusEntryLinePK, @chargeType, @chargeAmount, @methodOfPayment, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusEntryLineFeePK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmount", SqlDbType.Decimal, chargeAmount);
				command.AddParameter("@methodOfPayment", SqlDbType.VarChar, methodOfPayment);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateCusEntryPayInfo(Guid cusEntryHeaderPK, string incomingPayResponseNo, string chargeType, Decimal chargeAmount, int clusterKey)
		{
			var cusEntryPayInfoPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.CusEntryPayInfo (C9_PK, C9_CH, C9_IncomingPayResponseNo, C9_TransactionType, C9_PaymentAmount, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
				VALUES (@cusEntryPayInfoPK, @cusEntryHeaderPK, @incomingPayResponseNo, @chargeType, @chargeAmount, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusEntryPayInfoPK", SqlDbType.UniqueIdentifier, cusEntryPayInfoPK);
				command.AddParameter("@cusEntryHeaderPK", SqlDbType.UniqueIdentifier, cusEntryHeaderPK);
				command.AddParameter("@incomingPayResponseNo", SqlDbType.VarChar, incomingPayResponseNo);
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmount", SqlDbType.Decimal, chargeAmount);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return cusEntryPayInfoPK;
		}

		void CreateGenAddOnColumn(Guid cusEntryPayInfoPK, string name, decimal amount)
		{
			var sql = @"INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_ParentTableCode, XA_ParentID, XA_Name, XA_Type, XA_Data, XA_SystemCreateTimeUtc, XA_SystemCreateUser, XA_SystemLastEditTimeUtc, XA_SystemLastEditUser)
				VALUES (@genAddOnColumnPK, 'C9', @cusEntryPayInfoPK, @name, 'DEC', @amount, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@genAddOnColumnPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@cusEntryPayInfoPK", SqlDbType.UniqueIdentifier, cusEntryPayInfoPK);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.AddParameter("@amount", SqlDbType.Decimal, amount);
				command.ExecuteNonQuery();
			}
		}
	}
}
