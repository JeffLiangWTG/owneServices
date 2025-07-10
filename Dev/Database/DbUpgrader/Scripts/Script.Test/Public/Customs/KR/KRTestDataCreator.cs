using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Build.Database.Script.Public.Customs.KR.Testing
{
	public static class KRTestDataCreator
	{
		public static Guid CreateJobDeclaration(int clusterKey, string messageType, Guid branchPK, Guid companyPK, List<QueryItem> queryItems = null, string declarationReference = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationRef = string.IsNullOrEmpty(declarationReference) ? Guid.NewGuid().ToString("n") : declarationReference;
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey, JE_MessageType, JE_SystemCreateUser, JE_SystemLastEditUser, JE_DeclarationReference");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@declarationPK, 'KR', @branchPK, @companyPK, @clusterKey, @messageType, '~BP', '~BP', @declarationReference");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationRef);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		public static Guid CreateJobService(Guid parentPK, string parentTableName, string serviceCode, DateTime bookedTime)
		{
			var servicePK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.JobService(ES_PK, ES_ParentID, ES_ParentTableCode, ES_ServiceCode, ES_BookedDateTimeOffset)
				VALUES (@servicePK, @parentPK, @parentTableName, @serviceCode, @bookedTime)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@servicePK", SqlDbType.UniqueIdentifier, servicePK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableName", SqlDbType.VarChar, parentTableName);
				command.AddParameter("@serviceCode", SqlDbType.VarChar, serviceCode);
				command.AddParameter("@bookedTime", SqlDbType.DateTimeOffset, bookedTime);
				command.ExecuteNonQuery();
			}
			return servicePK;
		}

		public static Guid CreateJobConsolTransport(Guid declarationPK, DateTime actualDateOfLoading, string declaration_PortOfLoading, string declaration_TransportMode)
		{
			var result = Guid.NewGuid();

			var sql = @"
				INSERT INTO dbo.JobConsolTransport(JW_PK, JW_ParentGUID, JW_ATD, JW_RL_NKLoadPort, JW_TransportMode, JW_SystemCreateTimeUtc, JW_SystemCreateUser, JW_SystemLastEditTimeUtc, JW_SystemLastEditUser)
				VALUES (@JW_PK, @JW_ParentGUID, @actualDateOfLoading, @portOfLoading, @transportMode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JW_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@JW_ParentGUID", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@actualDateOfLoading", SqlDbType.DateTime, actualDateOfLoading);
				command.AddParameter("@portOfLoading", SqlDbType.VarChar, declaration_PortOfLoading);
				command.AddParameter("@transportMode", SqlDbType.VarChar, declaration_TransportMode);
				command.ExecuteNonQuery();
			}
			return result;
		}

		public static Guid CreateCusEntryHeader(Guid declarationPK, int clusterKey, List<QueryItem> queryItems = null)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES(@entryHeaderPK, 'KR', @declarationPK, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		public static Guid CreateCusEntryLine(Guid entryHeaderPK, int clusterKey, List<QueryItem> queryItems = null)
		{
			var entryLinePK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES(@entryLinePK, 'KR', @entryHeaderPK, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return entryLinePK;
		}

		public static Guid CreateCusEntryNum(Guid parentPK, string parentTableName, string entryNum, string type, string category, DateTime issueDate, DateTime expiryDate)
		{
			var entryNumPK = TestDataCreator.CreateCusEntryNum(parentPK, parentTableName, entryNum, type, category, "KR", issueDate);
			var sql = @"
UPDATE dbo.CusEntryNum 
SET 
    CE_ExpiryDate = @expiryDate, 
    CE_SystemLastEditTimeUtc = GETUTCDATE(), 
    CE_SystemLastEditUser = '~BP' 
WHERE 
    CE_PK = @entryNumPK";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryNumPK", SqlDbType.UniqueIdentifier, entryNumPK);
				command.AddParameter("@expiryDate", SqlDbType.DateTime, expiryDate);
				command.ExecuteNonQuery();
			}
			return entryNumPK;
		}

		public static Guid CreateCusEntryNum(Guid parentPK, string parentTableName, string entryNum, string type, string category, DateTime issueDate, DateTime expiryDate, string entryLineReference)
		{
			var entryNumPK = TestDataCreator.CreateCusEntryNum(parentPK, parentTableName, entryNum, type, category, "KR", issueDate, entryLineReference);
			var sql = @"
UPDATE dbo.CusEntryNum 
SET 
    CE_ExpiryDate = @expiryDate, 
    CE_SystemLastEditTimeUtc = GETUTCDATE(), 
    CE_SystemLastEditUser = '~BP' 
WHERE 
    CE_PK = @entryNumPK;";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryNumPK", SqlDbType.UniqueIdentifier, entryNumPK);
				command.AddParameter("@expiryDate", SqlDbType.DateTime, expiryDate);
				command.ExecuteNonQuery();
			}
			return entryNumPK;
		}

		public static Guid CreateRefExchangeRate(Guid companyPK, string currency, decimal exchangeRate, string rateType, DateTime startDate, DateTime endDate)
		{
			var exRatePK = Guid.NewGuid();

			var sql = @"INSERT INTO dbo.RefExchangeRate
							   (RE_PK, RE_GC, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_RX_NKExCurrency, RE_AsPublished)
						VALUES (@exRatePK1, @companyPK, @rateType, @startDate, @endDate, @exchangeRate, @currency, '');";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@exRatePK1", SqlDbType.UniqueIdentifier, exRatePK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@currency", SqlDbType.VarChar, currency);
				command.AddParameter("@exchangeRate", SqlDbType.Decimal, exchangeRate);
				command.AddParameter("@rateType", SqlDbType.VarChar, rateType);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.ExecuteNonQuery();
			}
			return exRatePK;
		}

		public static Guid CreateJobComInvoiceHeader(Guid declarationPK, int clusterKey, List<QueryItem> queryItems = null)
		{
			var invoiceHeaderPK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@invoiceHeaderPK, 'KR', @declarationPK, @clusterKey");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return invoiceHeaderPK;
		}

		public static Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, int clusterKey, Guid entryLinePK, List<QueryItem> queryItems = null)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey, JI_CL");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@invoiceLinePK, 'KR', @jobComInvoiceHeaderPK, @clusterKey, @entryLinePK");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}

		public static Guid CreateJobComInvHeaderCharge(Guid parentPK, string parentTableType, List<QueryItem> queryItems = null)
		{
			Guid chargePK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentID, J7_ParentTableCode, J7_DistributeBy, J7_IsValid, J7_FullOrPartialApportionment, J7_IsGSTApplicable");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@chargePK, @parentPK, @parentTableType, 'VAL', 1, 'PAA', 1");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@chargePK", SqlDbType.UniqueIdentifier, chargePK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableType", SqlDbType.VarChar, parentTableType);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return chargePK;
		}

		public static Guid CreateStmData(Guid ownerPK, string registryName, string sdBinaryValue)
		{
			var result = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.StmData (SD_PK, SD_Owner, SD_BinaryValue, SD_Name)
				VALUES (@DataPK, @OwnerPK, @SDBinaryValue, @registryName)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@DataPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@OwnerPK", SqlDbType.UniqueIdentifier, ownerPK);
				command.AddParameter("@registryName", SqlDbType.VarChar, registryName);
				command.AddParameter("@SDBinaryValue", SqlDbType.VarBinary, Encoding.Unicode.GetBytes(sdBinaryValue));
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateOrgAddress(Guid orgHeaderPK, string code, bool isMainAddress, string addressType, List<QueryItem> queryItems = null)
		{
			var addressPK = Guid.NewGuid();
			var orgAddressCapabilityPK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Code");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@addressPK, @orgHeaderPK, @code");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));
			sql.AppendLine(@"INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_AddressType, PZ_OA, PZ_IsMainAddress)
					VALUES (@orgAddressCapabilityPK, @addressType, @addressPK, @isMainAddress)");
			using (DbCommand command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@orgAddressCapabilityPK", SqlDbType.UniqueIdentifier, orgAddressCapabilityPK);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@isMainAddress", SqlDbType.Bit, isMainAddress ? 1 : 0);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return addressPK;
		}

		public static Guid CreateContact(Guid organizationPK, string representativeName, string phone, bool isRepresentative)
		{
			var contactPK = Guid.NewGuid();
			var contactAttributePK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Phone, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser)
				VALUES (@contactPK, @organizationPK, @representativeName, @phone, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";
			if (isRepresentative)
			{
				sql += @"
				INSERT INTO dbo.OrgContactAttribute (PC_PK, PC_IsValid, PC_Type, PC_OC, PC_IsAllocatedContact, PC_AutoVersion, PC_SystemCreateTimeUtc, PC_SystemCreateUser, PC_SystemLastEditTimeUtc, PC_SystemLastEditUser)
				VALUES(@contactAttributePK, 1, 'KRC', @contactPK, 1, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";
			}
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@contactPK", SqlDbType.UniqueIdentifier, contactPK);
				command.AddParameter("@organizationPK", SqlDbType.UniqueIdentifier, organizationPK);
				command.AddParameter("@representativeName", SqlDbType.VarChar, representativeName);
				command.AddParameter("@phone", SqlDbType.VarChar, phone);
				if (isRepresentative)
				{
					command.AddParameter("@contactAttributePK", SqlDbType.UniqueIdentifier, contactAttributePK);
				}
				command.ExecuteNonQuery();
			}
			return contactPK;
		}

		public static Guid CreateCusSupportingInfo(Guid parentPK, string parentTableCode, string type, List<QueryItem> queryItems = null)
		{
			var supportingInfoPK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.CusSupportingInfo(CSI_PK, CSI_ParentID, CSI_ParentTableCode, CSI_DataModel, CSI_Type, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc ,CSI_SystemLastEditUser");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@supportingInfoPK, @parentPK, @parentTableCode, 'KR', @type, GetUtcDate(), '~BE', GetUtcDate(), '~BE'");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (DbCommand command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@supportingInfoPK", SqlDbType.UniqueIdentifier, supportingInfoPK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@type", SqlDbType.VarChar, type);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return supportingInfoPK;
		}

		public static Guid CreateRefCusTariffType()
		{
			var refCusTariffTypePK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZ9_NKNomenclatureGroupType, ZZI_ZZZ_NKDataGrouping)");
			sql.AppendLine("VALUES (@supportingInfoPK, 'HSN', 'Korean Harmonized Tariff Codes', 'KR', 'KR')");

			using (DbCommand command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@supportingInfoPK", SqlDbType.UniqueIdentifier, refCusTariffTypePK);
				command.ExecuteNonQuery();
			}
			return refCusTariffTypePK;
		}

		public static Guid CreateRefCusTariff(Guid parentPK, List<QueryItem> queryItems = null)
		{
			Guid refCusTariffPK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO RefDatabase_RefCusTariff(ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZF_NKTaxOrFeeCode");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@refCusTariffPK, @parentPK, 'KR', ''");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@refCusTariffPK", SqlDbType.UniqueIdentifier, refCusTariffPK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return refCusTariffPK;
		}

		public static Guid CreateRefCusTariffAttribute(Guid parentPK, string name)
		{
			Guid refCusTariffAttributePK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO RefDatabase_RefCusTariffAttribute(ZZ3_PK, ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value)");
			sql.AppendLine("VALUES (@refCusTariffAttributePK, @parentPK, @name, 'Y')");

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@refCusTariffAttributePK", SqlDbType.UniqueIdentifier, refCusTariffAttributePK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@name", SqlDbType.VarChar, name);

				command.ExecuteNonQuery();
			}
			return refCusTariffAttributePK;
		}

		public static Guid CreateTariffView(List<QueryItem> queryItems)
		{
			var tariffPK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.TariffView (ZZ1_PK, ZZ1_ZZI_NKTariffType, ZZ1_CRT_NKTariffVersion, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_SystemCreateTimeUtc, ZZ1_SystemCreateUser, ZZ1_SystemLastEditTimeUtc, ZZ1_SystemLastEditUser");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@tariffPK, 'HSN', '', '', GetUtcDate(), '~BE', GetUtcDate(), '~BE'");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (DbCommand command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@tariffPK", SqlDbType.UniqueIdentifier, tariffPK);

				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return tariffPK;
		}

		public static Guid CreateRefVessel(string code, string radioCallSign)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.RefVessel([RV_PK], [RV_Code], [RV_RadioCallSign])
			VALUES (@rvPk, @code, @radioCallSign)
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@rvPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@radioCallSign", SqlDbType.VarChar, radioCallSign);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobDecRefs(Guid declarationPK, int clusterKey, string referenceNumber, string referenceType)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobDecRefs([J3_PK], [J3_JE], [J3_ClusterKey], [J3_ReferenceNumber], [J3_ReferenceType])
			VALUES (@j3Pk, @declarationPK, @clusterKey, @referenceNumber, @referenceType)
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@j3Pk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@referenceNumber", SqlDbType.VarChar, referenceNumber);
				command.AddParameter("@referenceType", SqlDbType.VarChar, referenceType);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusStatementHeader(Guid companyPK, List<QueryItem> queryItems = null)
		{
			var statementHeaderPK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.CusStatementHeader(B2_PK, B2_GC");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@statementHeaderPK, @companyPK");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@statementHeaderPK", SqlDbType.UniqueIdentifier, statementHeaderPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return statementHeaderPK;
		}

		public static Guid CreateCusStatementLine(Guid statementHeaderPK, List<QueryItem> queryItems = null)
		{
			var statementLinePK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("INSERT INTO dbo.CusStatementLine(B3_PK, B3_B2");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@statementLinePK, @statementHeaderPK");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@statementLinePK", SqlDbType.UniqueIdentifier, statementLinePK);
				command.AddParameter("@statementHeaderPK", SqlDbType.UniqueIdentifier, statementHeaderPK);
				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return statementLinePK;
		}

		public static Guid CreateCusStatementLineCharge(Guid statementLinePK, List<QueryItem> queryItems = null)
		{
			var statementLineChargePK = Guid.NewGuid();
			var sql = new StringBuilder();
			sql.Append("Insert Into CusStatementLineCharge (B4_PK, B4_B3,B4_SystemCreateTimeUtc, B4_SystemCreateUser, B4_SystemLastEditTimeUtc, B4_SystemLastEditUser");
			sql.Append(GenerateSetQueryFromItems(", ", queryItems));
			sql.AppendLine("VALUES (@statementLineChargePK, @statementLinePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'");
			sql.Append(GenerateSetQueryFromItems(", @", queryItems));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.AddParameter("@statementLineChargePK", SqlDbType.UniqueIdentifier, statementLineChargePK);
				command.AddParameter("@statementLinePK", SqlDbType.UniqueIdentifier, statementLinePK);
				AddParametersFromItems(command, queryItems);

				command.ExecuteNonQuery();
			}
			return statementLineChargePK;
		}

		static StringBuilder GenerateSetQueryFromItems(string splitter, List<QueryItem> queryItems)
		{
			var result = new StringBuilder();
			if (queryItems != null)
			{
				foreach (var item in queryItems)
				{
					result.AppendFormat("{0}{1}", splitter, item.ColumnName);
				}
			}
			result.Append(")");

			return result;
		}
		static void AddParametersFromItems(DbCommand command, List<QueryItem> queryItems)
		{
			if (queryItems != null)
			{
				foreach (var item in queryItems)
				{
					command.AddParameter("@" + item.ColumnName, item.DBType, item.Value);
				}
			}
		}

		public static string GenerateAddInfoData(Dictionary<string, string> dics)
		{
			var result = new StringBuilder();
			if (dics != null)
			{
				foreach (var pair in dics)
				{
					result.AppendFormat("*{0}={1}", pair.Key, pair.Value);
				}
			}
			return result.Length > 0 ? result.ToString().Substring(1) : result.ToString();
		}

		public static List<QueryItem> GetItemList(List<string> columnName, List<object> value)
		{
			var itemList = new List<QueryItem>();
			if (columnName.Count == value.Count)
			{
				for (int i = 0; i < columnName.Count; i++)
				{
					if (value[i] is string)
					{
						itemList.Add(new QueryItem(columnName[i], SqlDbType.VarChar, value[i]));
					}
					else if (value[i] is decimal)
					{
						itemList.Add(new QueryItem(columnName[i], SqlDbType.Decimal, value[i]));
					}
					else if (value[i] is int)
					{
						itemList.Add(new QueryItem(columnName[i], SqlDbType.Int, value[i]));
					}
					else if (value[i] is DateTime)
					{
						itemList.Add(new QueryItem(columnName[i], SqlDbType.DateTime, value[i]));
					}
					else if (value[i] is Guid)
					{
						itemList.Add(new QueryItem(columnName[i], SqlDbType.UniqueIdentifier, value[i]));
					}
					else if (value[i] is bool)
					{
						itemList.Add(new QueryItem(columnName[i], SqlDbType.Bit, (bool)value[i] ? 1 : 0));
					}
				}
			}

			return itemList;
		}
	}
	public struct QueryItem
	{
		public string ColumnName { get; }
		public SqlDbType DBType { get; }
		public object Value { get; }
		public QueryItem(string columnName, SqlDbType dbType, object value)
		{
			ColumnName = columnName;
			DBType = dbType;
			Value = value;
		}
	}
}
