using System;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.CalendarArithmetic;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.TestFramework
{
	public static class TestDataCreator
	{
		public static Guid CreateEDIMessage(
			Guid branchPK,
			Guid departmentPK,
			Guid interchangePK,
			Guid linkUniqueID,
			DateTime createTimeUTC,
			string status = "",
			string applicationCode = "",
			string applicationReference = "",
			string direction = "",
			string linkTable = "",
			string createUser = "~BP",
			string messageNum = "",
			string messageType = "",
			string messageSubType = "",
			string messageText = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.EDIMessage(EM_PK, EM_GB, EM_GE, EM_EI, EM_ApplicationCode, EM_ApplicationReference, EM_ReceiveTransmit, EM_Status, EM_MessageText, EM_LinkTable, EM_LinkUniqueID, EM_SystemCreateTimeUTC, EM_SystemCreateUser, EM_MessageNum, EM_MessageType, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser, EM_MessageSubType)
VALUES (@pk, @branchPK, @departmentPK, @interchangePK, @applicationCode, @applicationReference, @direction, @status, @messageText, @linkTable, @linkUniqueID, @createTime, @createUser, @messageNum, @messageType, GetUtcDate(), '~BP', @messageSubType)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@interchangePK", SqlDbType.UniqueIdentifier, interchangePK == Guid.Empty ? DBNull.Value : interchangePK);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, EDIMessageSchema.EM_ApplicationCode.MaxLength, applicationCode);
				command.AddParameter("@applicationReference", SqlDbType.VarChar, EDIMessageSchema.EM_ApplicationReference.MaxLength, applicationReference);
				command.AddParameter("@direction", SqlDbType.VarChar, EDIMessageSchema.EM_ReceiveTransmit.MaxLength, direction);
				command.AddParameter("@status", SqlDbType.VarChar, EDIMessageSchema.EM_Status.MaxLength, status);
				command.AddParameter("@messageText", SqlDbType.VarChar, messageText);
				command.AddParameter("@linkTable", SqlDbType.VarChar, EDIMessageSchema.EM_LinkTable.MaxLength, linkTable);
				command.AddParameter("@linkUniqueID", SqlDbType.UniqueIdentifier, linkUniqueID == Guid.Empty ? DBNull.Value : linkUniqueID);
				command.AddParameter("@createTime", SqlDbType.DateTime, createTimeUTC);
				command.AddParameter("@createUser", SqlDbType.VarChar, EDIMessageSchema.EM_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@messageNum", SqlDbType.VarChar, EDIMessageSchema.EM_MessageNum.MaxLength, messageNum);
				command.AddParameter("@messageType", SqlDbType.VarChar, EDIMessageSchema.EM_MessageType.MaxLength, messageType);
				command.AddParameter("@messageSubType", SqlDbType.VarChar, EDIMessageSchema.EM_MessageSubType.MaxLength, messageSubType);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateJobOrderItem(
			Guid jobDocsAndCartagePK,
			string orderReference)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobOrderItem(JT_PK, JT_JP, JT_OrderReference, JT_SystemCreateTimeUtc, JT_SystemCreateUser, JT_SystemLastEditTimeUtc, JT_SystemLastEditUser)
VALUES (@pk, @jobDocsAndCartagePK, @orderReference, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobDocsAndCartagePK", SqlDbType.UniqueIdentifier, jobDocsAndCartagePK);
				command.AddParameter("@orderReference", SqlDbType.VarChar, orderReference);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateJobDocsAndCartage(
			Guid parentID,
			string parentTableCode)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDocsAndCartage(JP_PK, JP_ParentID, JP_ParentTableCode, JP_SystemCreateTimeUtc, JP_SystemCreateUser, JP_SystemLastEditTimeUtc, JP_SystemLastEditUser)
VALUES (@pk, @parentID, @parentTableCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateEDIInterchange(
			Guid branchPK,
			string applicationCode,
			DateTime createTimeUTC,
			string createUser,
			string from,
			string to,
			string interchangeNum,
			string status,
			DateTimeOffset? deliveredTime = null,
			string interchangeType = "",
			string receiveTransmit = "",
			bool isActive = true,
			Guid? sessionGUID = null)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.EDIInterchange(EI_PK, EI_GB, EI_ApplicationCode, EI_SystemCreateTimeUTC, EI_SystemCreateUser, EI_From, EI_To, EI_InterchangeNum, EI_Status, EI_DeliveredTime, EI_SystemLastEditTimeUtc, EI_SystemLastEditUser, EI_InterchangeType, EI_ReceiveTransmit, EI_IsActive, EI_SessionGUID)
VALUES (@pk, @branchPK, @applicationCode, @createTime, @createUser, @from, @to, @interchangeNum, @status, @deliveredTime, GetUtcDate(), '~BP', @interchangeType, @receiveTransmit, @isActive, @sessionGUID)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, EDIInterchangeSchema.EI_ApplicationCode.MaxLength, applicationCode);
				command.AddParameter("@createTime", SqlDbType.DateTime, createTimeUTC);
				command.AddParameter("@createUser", SqlDbType.VarChar, EDIInterchangeSchema.EI_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@from", SqlDbType.VarChar, EDIInterchangeSchema.EI_From.MaxLength, from);
				command.AddParameter("@to", SqlDbType.VarChar, EDIInterchangeSchema.EI_To.MaxLength, to);
				command.AddParameter("@interchangeNum", SqlDbType.VarChar, EDIInterchangeSchema.EI_InterchangeNum.MaxLength, interchangeNum);
				command.AddParameter("@status", SqlDbType.VarChar, EDIInterchangeSchema.EI_Status.MaxLength, status);
				command.AddParameter("@deliveredTime", SqlDbType.DateTimeOffset, deliveredTime == null ? DBNull.Value : deliveredTime);
				command.AddParameter("@interchangeType", SqlDbType.VarChar, EDIInterchangeSchema.EI_InterchangeType.MaxLength, interchangeType);
				command.AddParameter("@receiveTransmit", SqlDbType.VarChar, EDIInterchangeSchema.EI_ReceiveTransmit.MaxLength, receiveTransmit);
				command.AddParameter("@isActive", SqlDbType.Bit, EDIInterchangeSchema.EI_IsActive.MaxLength, isActive);
				command.AddParameter("@sessionGUID", SqlDbType.UniqueIdentifier, sessionGUID ?? (object)DBNull.Value);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateAddress(Guid organisationPK, string code, string address1)
		{
			var addressPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
VALUES (@addressPK, @organisationPK, @code, @address1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgAddressSchema.OA_Code.MaxLength, code);
				command.AddParameter("@address1", SqlDbType.VarChar, OrgAddressSchema.OA_Address1.MaxLength, address1);
				command.ExecuteNonQuery();
			}
			return addressPK;
		}

		public static Guid CreateAddress(Guid organisationPK, string code, string address1, string countryCode)
		{
			var addressPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1, OA_RN_NKCountryCode)
VALUES (@addressPK, @organisationPK, @code, @address1, @countryCode)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgAddressSchema.OA_Code.MaxLength, code);
				command.AddParameter("@address1", SqlDbType.VarChar, OrgAddressSchema.OA_Address1.MaxLength, address1);
				command.AddParameter("@countryCode", SqlDbType.VarChar, OrgAddressSchema.OA_RN_NKCountryCode.MaxLength, countryCode);
				command.ExecuteNonQuery();
			}
			return addressPK;
		}

		public static Guid CreateAddress(Guid organisationPK, string code, string address1, string address2, string city, string state, string postCode, string companyNameOverride, string countryCode = "")
		{
			var addressPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1, OA_Address2, OA_City, OA_State, OA_PostCode, OA_CompanyNameOverride, OA_RN_NKCountryCode, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
VALUES (@addressPK, @organisationPK, @code, @address1, @address2, @city, @state, @postCode, @overrideCompanyName, @countryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgAddressSchema.OA_Code.MaxLength, code);
				command.AddParameter("@address1", SqlDbType.VarChar, OrgAddressSchema.OA_Address1.MaxLength, address1);
				command.AddParameter("@address2", SqlDbType.VarChar, OrgAddressSchema.OA_Address2.MaxLength, address2);
				command.AddParameter("@city", SqlDbType.VarChar, OrgAddressSchema.OA_City.MaxLength, city);
				command.AddParameter("@state", SqlDbType.VarChar, OrgAddressSchema.OA_State.MaxLength, state);
				command.AddParameter("@postCode", SqlDbType.VarChar, OrgAddressSchema.OA_PostCode.MaxLength, postCode);
				command.AddParameter("@overrideCompanyName", SqlDbType.VarChar, OrgAddressSchema.OA_CompanyNameOverride.MaxLength, companyNameOverride);
				command.AddParameter("@countryCode", SqlDbType.VarChar, OrgAddressSchema.OA_RN_NKCountryCode.MaxLength, countryCode);
				command.ExecuteNonQuery();
			}
			return addressPK;
		}

		public static Guid CreateAddress(Guid organisationPK, string code, string address1, string address2, string city, string state, string postCode)
		{
			var addressPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1, OA_Address2, OA_City, OA_State, OA_PostCode, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
VALUES (@addressPK, @organisationPK, @code, @address1, @address2, @city, @state, @postCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgAddressSchema.OA_Code.MaxLength, code);
				command.AddParameter("@address1", SqlDbType.VarChar, OrgAddressSchema.OA_Address1.MaxLength, address1);
				command.AddParameter("@address2", SqlDbType.VarChar, OrgAddressSchema.OA_Address2.MaxLength, address2);
				command.AddParameter("@city", SqlDbType.VarChar, OrgAddressSchema.OA_City.MaxLength, city);
				command.AddParameter("@state", SqlDbType.VarChar, OrgAddressSchema.OA_State.MaxLength, state);
				command.AddParameter("@postCode", SqlDbType.VarChar, OrgAddressSchema.OA_PostCode.MaxLength, postCode);
				command.ExecuteNonQuery();
			}
			return addressPK;
		}

		public static Guid CreateOrgAddressCapability(Guid addressPK, string addressType, bool isMainAddress)
		{
			var orgAddressCapabilityPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_AddressType, PZ_OA, PZ_IsMainAddress)
VALUES (@orgAddressCapabilityPK, @addressType, @addressPK, @isMainAddress)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@orgAddressCapabilityPK", SqlDbType.UniqueIdentifier, orgAddressCapabilityPK);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@isMainAddress", SqlDbType.Bit, isMainAddress);
				command.ExecuteNonQuery();
			}

			return orgAddressCapabilityPK;
		}

		public static Guid CreateContact(Guid organisationPK, string contactName, string phone)
		{
			var contactPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Phone, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser)
VALUES (@contactPK, @organisationPK, @contactName, @phone, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@contactPK", SqlDbType.UniqueIdentifier, contactPK);
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@contactName", SqlDbType.VarChar, OrgContactSchema.OC_ContactName.MaxLength, contactName);
				command.AddParameter("@phone", SqlDbType.VarChar, OrgContactSchema.OC_Phone.MaxLength, phone);
				command.ExecuteNonQuery();
			}
			return contactPK;
		}

		public static Guid CreateOrgCountryData(string addInfo, Guid orgHeaderPK, string country)
		{
			var orgCountryDataPK = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.OrgCountryData (OV_PK, OV_ImportCustomsDefaultAddInfo, OV_OH_OrgHeader, OV_RN_NKClientCountryRelation)
			VALUES (@PK, @AddInfo, @OrgHeaderPK, @Country)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, orgCountryDataPK);
				command.AddParameter("@AddInfo", SqlDbType.VarChar, OrgCountryDataSchema.OV_ImportCustomsDefaultAddInfo.MaxLength, addInfo);
				command.AddParameter("@OrgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.AddParameter("@Country", SqlDbType.VarChar, OrgCountryDataSchema.OV_RN_NKClientCountryRelation.MaxLength, country);
				command.ExecuteNonQuery();
			}

			return orgCountryDataPK;
		}

		public static Guid CreateBranch(Guid companyPK, string branchCode, string homePort, string countryCode = null)
			=> CreateBranch(companyPK, branchCode, homePort, DateTime.Now, countryCode);

		public static Guid CreateBranch(Guid companyPK, string branchCode, string homePort, DateTime referenceDate, string countryCode = null)
		{
			var branchPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC, GB_RN_NKCountryCode, GB_SystemCreateUser, GB_SystemCreateTimeUtc, GB_SystemLastEditUser, GB_SystemLastEditTimeUtc)
VALUES (@branchPK, @branchCode, @homePort, @companyPK, @countryCode, '~BP', GetUtcDate(), '~BP', GetUtcDate())

INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc)
SELECT NEWID(), @homePort, @startDate, @endDate, offset
FROM dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO(@homePort, @timeNow, 0)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@branchCode", SqlDbType.VarChar, GlbBranchSchema.GB_Code.MaxLength, branchCode);
				command.AddParameter("@homePort", SqlDbType.VarChar, GlbBranchSchema.GB_RL_NKHomePort.MaxLength, homePort);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@startDate", SqlDbType.DateTime, new DateTime(referenceDate.Year, 1, 1));
				command.AddParameter("@endDate", SqlDbType.DateTime, new DateTime(referenceDate.Year, 12, 31, 23, 59, 59, 999));
				command.AddParameter("@timeNow", SqlDbType.DateTime, referenceDate);
				command.AddParameter("@countryCode", SqlDbType.VarChar, GlbBranchSchema.GB_RN_NKCountryCode.MaxLength, countryCode ?? "");
				command.ExecuteNonQuery();
			}
			return branchPK;
		}

		public static Guid CreateOrExistingDepartment(string departmentCode)
		{
			var sql = @"SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = @GE_Code";
			var result = Guid.Empty;

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@GE_Code", SqlDbType.VarChar, GlbDepartmentSchema.GE_Code.MaxLength, departmentCode);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result = reader.GetGuid(0);
					}
				}
			}
			if (result == Guid.Empty)
			{
				var referenceDate = DateTime.Now;
				result = CreateDepartment(departmentCode, referenceDate);
			}
			return result;
		}

		public static Guid CreateDepartment(string departmentCode, string mondayHours = "", string tuesdayHours = "", string wednesdayHours = "", string thursdayHours = "", string fridayHours = "", string saturdayHours = "", string sundayHours = "")
			=> CreateDepartment(departmentCode, DateTime.Now, mondayHours, tuesdayHours, wednesdayHours, thursdayHours, fridayHours, saturdayHours, sundayHours);

		public static Guid CreateDepartment(string departmentCode, DateTime referenceDate, string mondayHours = "", string tuesdayHours = "", string wednesdayHours = "", string thursdayHours = "", string fridayHours = "", string saturdayHours = "", string sundayHours = "")
		{
			var workTimes = LegacyWorkTimeConverter.ConvertStringToIntervals(mondayHours).Select(i => (CalendarCodes.Days.Monday, i.StartTime, i.EndTime))
				.Union(LegacyWorkTimeConverter.ConvertStringToIntervals(tuesdayHours).Select(i => (CalendarCodes.Days.Tuesday, i.StartTime, i.EndTime)))
				.Union(LegacyWorkTimeConverter.ConvertStringToIntervals(wednesdayHours).Select(i => (CalendarCodes.Days.Wednesday, i.StartTime, i.EndTime)))
				.Union(LegacyWorkTimeConverter.ConvertStringToIntervals(thursdayHours).Select(i => (CalendarCodes.Days.Thursday, i.StartTime, i.EndTime)))
				.Union(LegacyWorkTimeConverter.ConvertStringToIntervals(fridayHours).Select(i => (CalendarCodes.Days.Friday, i.StartTime, i.EndTime)))
				.Union(LegacyWorkTimeConverter.ConvertStringToIntervals(saturdayHours).Select(i => (CalendarCodes.Days.Saturday, i.StartTime, i.EndTime)))
				.Union(LegacyWorkTimeConverter.ConvertStringToIntervals(sundayHours).Select(i => (CalendarCodes.Days.Sunday, i.StartTime, i.EndTime)))
				.ToArray();

			return CreateDepartment(departmentCode, workTimes: workTimes.ToArray(), referenceDate);
		}

		static Guid CreateDepartment(string departmentCode, (string Day, DateTime StartTime, DateTime EndTime)[] workTimes, DateTime referenceDate)
		{
			var sql = @"
INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_SystemCreateUser, GE_SystemCreateTimeUtc, GE_SystemLastEditUser, GE_SystemLastEditTimeUtc)
VALUES (@departmentPK, @departmentCode, '~BP', GetUtcDate(), '~BP', GetUtcDate())
";
			var departmentPK = Guid.NewGuid();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@departmentCode", SqlDbType.VarChar, GlbDepartmentSchema.GE_Code.MaxLength, departmentCode);
				command.ExecuteNonQuery();
			}

			workTimes = workTimes ?? Array.Empty<(string Day, DateTime StartTime, DateTime EndTime)>();

			foreach (var workTime in workTimes)
			{
				AddDepartmentWorkTime(departmentPK, workTime.Day, workTime.StartTime, workTime.EndTime, referenceDate);
			}

			return departmentPK;
		}

		static void AddDepartmentWorkTime(Guid departmentPK, string day, DateTime startTime, DateTime endTime, DateTime referenceDate)
		{
			var sql = @"
INSERT dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser)
VALUES (@workTimePK, @departmentPK, 'GE', @day, @startTime, @endTime, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@workTimePK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@day", SqlDbType.Char, GlbWorkTimeSchema.GW_DayOfWeek.MaxLength, day);
				command.AddParameter("@startTime", SqlDbType.SmallDateTime, startTime);
				command.AddParameter("@endTime", SqlDbType.SmallDateTime, endTime);
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateCompany(string companyCode, string countryCode, string currencyCode)
		{
			var companyPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateUser, GC_SystemCreateTimeUtc, GC_SystemLastEditUser, GC_SystemLastEditTimeUtc)
VALUES (@companyPK, @companyCode, 'AU company', @countryCode, @currencyNK, '~BP', GetUtcDate(), '~BP', GetUtcDate())
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@companyCode", SqlDbType.VarChar, GlbCompanySchema.GC_Code.MaxLength, companyCode);
				command.AddParameter("@countryCode", SqlDbType.VarChar, GlbCompanySchema.GC_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@currencyNK", SqlDbType.VarChar, GlbCompanySchema.GC_RX_NKLocalCurrency.MaxLength, currencyCode);
				command.ExecuteNonQuery();
			}
			return companyPK;
		}

		public static Guid CreateCusEntryNum(Guid parentID,
			string parentTable,
			string entryNum,
			string entryType,
			string category,
			string countryCode,
			DateTime? issueDate = null,
			string entryLineReference = "",
			Guid? newPK = null,
			DateTime? createTime = null)
		{
			var cusEntryPK = newPK ?? Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_EntryType, CE_Category, CE_RN_NKCountryCode, CE_IssueDate,CE_EntryLineReference, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES (@cusEntryPK, @parentID, @parentTable, @entryNum, @entryType, @category, @countryCode, @issueDate, @entryLineReference, @systemCreateTimeUtc, '~BP', @systemCreateTimeUtc, '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusEntryPK", SqlDbType.UniqueIdentifier, cusEntryPK);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTable", SqlDbType.VarChar, CusEntryNumSchema.CE_ParentTable.MaxLength, parentTable);
				command.AddParameter("@entryNum", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryNum.MaxLength, entryNum);
				command.AddParameter("@entryType", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryType.MaxLength, entryType);
				command.AddParameter("@category", SqlDbType.VarChar, CusEntryNumSchema.CE_Category.MaxLength, category);
				command.AddParameter("@countryCode", SqlDbType.VarChar, CusEntryNumSchema.CE_RN_NKCountryCode.MaxLength, countryCode);
				if (issueDate.HasValue)
				{
					command.AddParameter("@issueDate", SqlDbType.SmallDateTime, issueDate.Value);
				}
				else
				{
					command.AddParameter("@issueDate", SqlDbType.SmallDateTime, DBNull.Value);
				}
				command.AddParameter("@entryLineReference", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryLineReference.MaxLength, entryLineReference);
				command.AddParameter("@systemCreateTimeUtc", SqlDbType.SmallDateTime, createTime ?? DateTime.UtcNow);
				command.ExecuteNonQuery();
			}
			return cusEntryPK;
		}

		public static Guid CreateCusEntryNum(Guid parentID, string parentTable, string entryNum, string entryType, string countryCode, Guid? newPK = null)
		{
			var cusEntryPK = newPK ?? Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryNum (
	CE_PK,
	CE_ParentID,
	CE_ParentTable,
	CE_EntryNum,
	CE_EntryType,
	CE_RN_NKCountryCode,
	CE_SystemCreateTimeUtc,
	CE_SystemCreateUser,
	CE_SystemLastEditTimeUtc,
	CE_SystemLastEditUser)
VALUES (
	@cusEntryPK,
	@parentID,
	@parentTable,
	@entryNum,
	@entryType,
	@countryCode,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusEntryPK", SqlDbType.UniqueIdentifier, cusEntryPK);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTable", SqlDbType.VarChar, CusEntryNumSchema.CE_ParentTable.MaxLength, parentTable);
				command.AddParameter("@entryNum", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryNum.MaxLength, entryNum);
				command.AddParameter("@entryType", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryType.MaxLength, entryType);
				command.AddParameter("@countryCode", SqlDbType.VarChar, CusEntryNumSchema.CE_RN_NKCountryCode.MaxLength, countryCode);
				_ = command.ExecuteNonQuery();
			}
			return cusEntryPK;
		}

		public static Guid CreateDocAddress(Guid addressPK, string companyName, Guid parentPK, string parentTableCode, string addressType, int addressSeq = 0)
		{
			var docAddressPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDocAddress(E2_PK, E2_AddressType, E2_OA_Address, E2_AddressOverride, E2_CompanyName, E2_ParentID, E2_ParentTableCode, E2_AddressSequence, E2_ValidationStatus, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES(@docAddressPK, @addressType, @addressPK, @addressOverride, @companyName, @parentPK, @parentTableCode, @addressSeq, @validationStatus, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@docAddressPK", SqlDbType.UniqueIdentifier, docAddressPK);
				command.AddParameter("@addressType", SqlDbType.VarChar, JobDocAddressSchema.E2_AddressType.MaxLength, addressType);
				if (addressPK == Guid.Empty)
				{
					command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
					command.AddParameter("@addressOverride", SqlDbType.Bit, true);
					command.AddParameter("@validationStatus", SqlDbType.Char, JobDocAddressSchema.E2_ValidationStatus.MaxLength, "NYV");
				}
				else
				{
					command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
					command.AddParameter("@addressOverride", SqlDbType.Bit, false);
					command.AddParameter("@validationStatus", SqlDbType.Char, JobDocAddressSchema.E2_ValidationStatus.MaxLength, "NRQ");
				}
				command.AddParameter("@companyName", SqlDbType.VarChar, JobDocAddressSchema.E2_CompanyName.MaxLength, companyName);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, JobDocAddressSchema.E2_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@addressSeq", SqlDbType.TinyInt, addressSeq);
				command.ExecuteNonQuery();
			}
			return docAddressPK;
		}

		public static Guid CreateDocAddress(Guid addressPK, string companyName, Guid parentPK, string parentTableCode, string addressType, string address1, string address2, string city, string state, string postCode, string contact, string phone, string govRegNumType, string govRegNum, bool addressOverride, string countryCode = "")
		{
			var docAddressPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDocAddress(E2_PK, E2_AddressType, E2_OA_Address, E2_AddressOverride, E2_CompanyName, E2_ParentID, E2_ParentTableCode, E2_Address1, E2_Address2, E2_City, E2_State, E2_Postcode, E2_Contact, E2_Phone, E2_GovRegNumType, E2_GovRegNum, E2_ValidationStatus, E2_RN_NKCountryCode, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES(@docAddressPK, @addressType, @addressPK, @addressOverride, @companyName, @parentPK, @parentTableCode, @address1, @address2, @city, @state, @postCode, @contact, @phone, @govRegNumType, @govRegNum, @validationStatus, @countryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@docAddressPK", SqlDbType.UniqueIdentifier, docAddressPK);
				command.AddParameter("@addressType", SqlDbType.VarChar, JobDocAddressSchema.E2_AddressType.MaxLength, addressType);
				if (addressPK == Guid.Empty)
				{
					command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				}
				command.AddParameter("@companyName", SqlDbType.VarChar, JobDocAddressSchema.E2_CompanyName.MaxLength, companyName);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, JobDocAddressSchema.E2_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@address1", SqlDbType.VarChar, JobDocAddressSchema.E2_Address1.MaxLength, address1);
				command.AddParameter("@address2", SqlDbType.VarChar, JobDocAddressSchema.E2_Address2.MaxLength, address2);
				command.AddParameter("@city", SqlDbType.VarChar, JobDocAddressSchema.E2_City.MaxLength, city);
				command.AddParameter("@state", SqlDbType.VarChar, JobDocAddressSchema.E2_State.MaxLength, state);
				command.AddParameter("@postCode", SqlDbType.VarChar, JobDocAddressSchema.E2_Postcode.MaxLength, postCode);
				command.AddParameter("@contact", SqlDbType.VarChar, JobDocAddressSchema.E2_Contact.MaxLength, contact);
				command.AddParameter("@phone", SqlDbType.VarChar, JobDocAddressSchema.E2_Phone.MaxLength, phone);
				command.AddParameter("@govRegNumType", SqlDbType.VarChar, JobDocAddressSchema.E2_GovRegNumType.MaxLength, govRegNumType);
				command.AddParameter("@govRegNum", SqlDbType.VarChar, JobDocAddressSchema.E2_GovRegNum.MaxLength, govRegNum);
				if (addressOverride)
				{
					command.AddParameter("@addressOverride", SqlDbType.Bit, true);
					command.AddParameter("@validationStatus", SqlDbType.Char, JobDocAddressSchema.E2_ValidationStatus.MaxLength, "NYV");
				}
				else
				{
					command.AddParameter("@addressOverride", SqlDbType.Bit, false);
					command.AddParameter("@validationStatus", SqlDbType.Char, JobDocAddressSchema.E2_ValidationStatus.MaxLength, "NRQ");
				}
				command.AddParameter("@countryCode", SqlDbType.VarChar, JobDocAddressSchema.E2_RN_NKCountryCode.MaxLength, countryCode);
				command.ExecuteNonQuery();
			}
			return docAddressPK;
		}

		public static Guid CreateJobComInvoiceHeader(Guid jobDeclarationPK, bool isGroupInvoice, int clusterKey, Guid? supplierPK = null, string originState = "", Guid? consigneePK = null, string addInfo = "", Guid? relatedHouseBillPK = null, DateTime? valuationDateOverride = null, string invoiceNumber = "", string countryOfOrigin = "", string dataModel = null)
		{
			var jobComInvoiceHeaderPK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_JE, JZ_GroupInvoice, JZ_OH_Supplier, JZ_RW_NKOriginState, JZ_OH_Consignee, JZ_AddInfo, JZ_ClusterKey, JZ_CU_RelatedHouseBill, JZ_ValuationDateOverride, JZ_InvoiceNumber, JZ_RN_NKDefaultOrigin, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				VALUES(@jobComInvoiceHeaderPK, @dataModel, @jobDeclarationPK, @groupInvoice, @supplierPK, @originState, @consignee, @addInfo, @clusterKey, @relatedHouseBill, @valuationDateOverride, @invoiceNumber, @countryOfOrigin, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel ?? "!!");
				command.AddParameter("@jobDeclarationPK", SqlDbType.UniqueIdentifier, jobDeclarationPK);
				command.AddParameter("@groupInvoice", SqlDbType.Bit, isGroupInvoice);
				command.AddParameter("@originState", SqlDbType.VarChar, originState);
				command.AddParameter("@invoiceNumber", SqlDbType.VarChar, invoiceNumber);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@countryOfOrigin", SqlDbType.VarChar, countryOfOrigin);

				if (supplierPK != null)
				{
					command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplierPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (consigneePK != null)
				{
					command.AddParameter("@consignee", SqlDbType.UniqueIdentifier, consigneePK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@consignee", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (relatedHouseBillPK != null)
				{
					command.AddParameter("@relatedHouseBill", SqlDbType.UniqueIdentifier, relatedHouseBillPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@relatedHouseBill", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (valuationDateOverride != null)
				{
					command.AddParameter("@valuationDateOverride", SqlDbType.DateTime, valuationDateOverride.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@valuationDateOverride", SqlDbType.DateTime, DBNull.Value);
				}
				command.ExecuteNonQuery();
			}
			return jobComInvoiceHeaderPK;
		}

		public static Guid CreateJobComInvoiceHeader(Guid jobDeclarationPK, int clusterKey, Guid? supplierPK = null, string invoiceNumber = "", string invoiceCurrency = "", string defaultOrigin = "", string incoTerm = "")
		{
			var jobComInvoiceHeaderPK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_JE, JZ_OH_Supplier, JZ_ClusterKey, JZ_InvoiceNumber, JZ_RX_NKInvoice_Currency, JZ_RN_NKDefaultOrigin, JZ_IncoTerm, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				VALUES(@jobComInvoiceHeaderPK, '!!', @jobDeclarationPK, @supplierPK, @clusterKey, @invoiceNumber, @invoiceCurrency, @defaultOrigin, @incoTerm, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@jobDeclarationPK", SqlDbType.UniqueIdentifier, jobDeclarationPK);
				command.AddParameter("@invoiceNumber", SqlDbType.VarChar, invoiceNumber);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@invoiceCurrency", SqlDbType.VarChar, invoiceCurrency);
				command.AddParameter("@defaultOrigin", SqlDbType.VarChar, defaultOrigin);
				command.AddParameter("@incoTerm", SqlDbType.VarChar, incoTerm);

				if (supplierPK != null)
				{
					command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplierPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				command.ExecuteNonQuery();
			}
			return jobComInvoiceHeaderPK;
		}

		public static Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, int clusterKey, string orignState = "", string addInfo = "", Guid? shipToPartyAddressPK = null, Guid? manufacturerAddressPK = null, int lineNo = 0, string partNo = "", string classification = "", string countryOfOrigin = "", Guid? entryLinePK = null,
			string dataModel = null)
		{
			var jobComInvoiceLinePK = Guid.NewGuid();
			const string sql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_CL, JI_StateOrRegionOfOrigin, JI_AddInfo, JI_OA_ShipToPartyAddress, JI_OA_ManufacturerAddress, JI_ClusterKey, JI_LineNo, JI_PartNo, JI_Tariff, JI_CountryOfOrigin, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES(@jobComInvoiceLinePK, @dataModel, @jobComInvoiceHeaderPK, @entryLinePK, @originState, @addInfo, @shipToPartyAddressPK, @manufacturerAddressPK, @clusterKey, @lineNo, @partNo, @classification, @countryOfOrigin, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceLinePK", SqlDbType.UniqueIdentifier, jobComInvoiceLinePK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel ?? "!!");
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK ?? (object)DBNull.Value);
				command.AddParameter("@originState", SqlDbType.VarChar, orignState);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNo", SqlDbType.Int, lineNo);
				command.AddParameter("@partNo", SqlDbType.VarChar, partNo);
				command.AddParameter("@classification", SqlDbType.VarChar, classification);
				command.AddParameter("@countryOfOrigin", SqlDbType.VarChar, countryOfOrigin);
				if (shipToPartyAddressPK != null)
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, shipToPartyAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				if (manufacturerAddressPK != null)
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, manufacturerAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				command.ExecuteNonQuery();
			}
			return jobComInvoiceLinePK;
		}

		public static Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, int clusterKey, int lineNo, int partNo, string description, decimal invoiceQuantity, string invoiceUQ, decimal customsQuantity, string customsUnitQty, decimal linePrice, decimal tariff, Guid cl_pk, Guid cc_pk, Guid? cei_pk = null)
		{
			var jobComInvoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey, JI_LineNo, JI_PartNo, JI_Description, JI_InvoiceQuantity, JI_InvoiceUQ, JI_CustomsQuantity, JI_CustomsUnitQty, JI_LinePrice, JI_Tariff, JI_CL, JI_CC, JI_CEI)
VALUES(@jobComInvoiceLinePK, '!!', @jobComInvoiceHeaderPK, @clusterKey, @lineNo, @partNo, @description, @invoiceQuantity, @invoiceUQ, @customsQuantity, @customsUnitQty, @linePrice, @tariff, @cl_pk, @cc_pk, @cei_pk)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceLinePK", SqlDbType.UniqueIdentifier, jobComInvoiceLinePK);
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNo", SqlDbType.Int, lineNo);
				command.AddParameter("@partNo", SqlDbType.Int, partNo);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@invoiceQuantity", SqlDbType.Decimal, invoiceQuantity);
				command.AddParameter("@invoiceUQ", SqlDbType.VarChar, invoiceUQ);
				command.AddParameter("@customsQuantity", SqlDbType.Decimal, customsQuantity);
				command.AddParameter("@customsUnitQty", SqlDbType.VarChar, customsUnitQty);
				command.AddParameter("@linePrice", SqlDbType.Decimal, linePrice);
				command.AddParameter("@tariff", SqlDbType.Decimal, tariff);
				command.AddParameter("@cl_pk", SqlDbType.UniqueIdentifier, cl_pk);
				command.AddParameter("@cc_pk", SqlDbType.UniqueIdentifier, cc_pk);
				if (cei_pk != null)
				{
					command.AddParameter("@cei_pk", SqlDbType.UniqueIdentifier, cei_pk);
				}
				else
				{
					command.AddParameter("@cei_pk", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				command.ExecuteNonQuery();
			}
			return jobComInvoiceLinePK;
		}

		public static Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, int clusterKey, int customsQty1, int customsQty2, int customsQty3, string customsUnitQty1, string customsUnitQty2, string customsUnitQty3, string orderNumber = "", string partNo = "", string countryOfOrigin = "", int lineNo = 0, Guid? entryLinePK = null, string dataModel = null)
		{
			var jobComInvoiceLinePK = Guid.NewGuid();
			const string sql = @"
INSERT INTO dbo.JobComInvoiceLine(
	JI_PK,
	JI_DataModel,
	JI_JZ,
	JI_CL,
	JI_CustomsQuantity,
	JI_CustomsSecondQuantity,
	JI_CustomsThirdQuantity,
	JI_CustomsUnitQty,
	JI_CustomsSecondUnitQty,
	JI_CustomsThirdUnitQty,
	JI_OrderNumber,
	JI_ClusterKey,
	JI_LineNo,
	JI_PartNo,
	JI_CountryOfOrigin,
	JI_SystemCreateTimeUtc,
	JI_SystemCreateUser,
	JI_SystemLastEditTimeUtc,
	JI_SystemLastEditUser)
VALUES(
	@jobComInvoiceLinePK,
	@dataModel,
	@jobComInvoiceHeaderPK,
	@entryLinePK,
	@customsQty1,
	@customsQty2,
	@customsQty3,
	@customsUnitQty1,
	@customsUnitQty2,
	@customsUnitQty3,
	@orderNumber,
	@clusterKey,
	@lineNo,
	@partNo,
	@countryOfOrigin,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceLinePK", SqlDbType.UniqueIdentifier, jobComInvoiceLinePK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel ?? "!!");
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK ?? (object)DBNull.Value);
				command.AddParameter("@customsQty1", SqlDbType.Int, customsQty1);
				command.AddParameter("@customsQty2", SqlDbType.Int, customsQty2);
				command.AddParameter("@customsQty3", SqlDbType.Int, customsQty3);
				command.AddParameter("@customsUnitQty1", SqlDbType.VarChar, JobComInvoiceLineSchema.JI_CustomsUnitQty.MaxLength, customsUnitQty1);
				command.AddParameter("@customsUnitQty2", SqlDbType.VarChar, JobComInvoiceLineSchema.JI_CustomsSecondUnitQty.MaxLength, customsUnitQty2);
				command.AddParameter("@customsUnitQty3", SqlDbType.VarChar, JobComInvoiceLineSchema.JI_CustomsThirdUnitQty.MaxLength, customsUnitQty3);
				command.AddParameter("@orderNumber", SqlDbType.VarChar, JobComInvoiceLineSchema.JI_OrderNumber.MaxLength, orderNumber);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNo", SqlDbType.Int, lineNo);
				command.AddParameter("@partNo", SqlDbType.VarChar, JobComInvoiceLineSchema.JI_PartNo.MaxLength, partNo);
				command.AddParameter("@countryOfOrigin", SqlDbType.VarChar, JobComInvoiceLineSchema.JI_CountryOfOrigin.MaxLength, countryOfOrigin);
				_ = command.ExecuteNonQuery();
			}
			return jobComInvoiceLinePK;
		}

		public static void CreateGenPivot(Guid relation1ID, Guid relation2ID, string relationType)
		{
			var sql = @"
INSERT INTO dbo.GenPivot(XX_PK,XX_Relation1ID,XX_Relation2ID,XX_RelationType)
VALUES (@genPivotPK, @relation1ID, @relation2ID, @relationType)
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@genPivotPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@relation1ID", SqlDbType.UniqueIdentifier, relation1ID);
				command.AddParameter("@relation2ID", SqlDbType.UniqueIdentifier, relation2ID);
				command.AddParameter("@relationType", SqlDbType.VarChar, relationType);
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string declarationReference, string messageType, int clusterKey, Guid? shipmentPK = null, Guid? supplierPK = null,
			string agentsReference = null, Guid? shipToPartyAddressPK = null, string ownerReference = null, string brokerCode = null, Guid? importerPK = null, Guid? forwarderPK = null,
			string addInfo = null, DateTime? customsCommencedDate = null, string customCommencedUser = null, string dataModel = null, string carrierCode = null, Guid? declarantAddressPK = null,
			DateTime? valuationDate = null, DateTime? entryAuthorisationDate = null)
		{
			var declarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_JS, JE_OH_Supplier, JE_DeclarationReference, JE_AgentsReference, JE_OA_ShipToPartyAddress, JE_ClusterKey, JE_OwnerRef, JE_GS_NKCusAgent, JE_OH_Importer, JE_OH_Forwarder, JE_CarrierCode, JE_AddInfo, JE_CustomsCommencedDate, JE_GS_NKCustomsCommencedUser, JE_OA_DeclarantAddress, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_ValuationDate, JE_EntryAuthorisationDate)
VALUES (@declarationPK, @dataModel, @messageType, @branchPK, @companyPK, @shipmentPK, @supplierPK, @declarationReference, @agentsReference, @shipToPartyAddressPK, @clusterKey, @ownerRef, @brokerCode, @importerPK, @forwarderPK, @carrierCode, @addInfo, @customsCommencedDate, @customCommencedUser, @declarantAddressPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @valuationDate, @entryAuthorisationDate)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel ?? "!!");
				command.AddParameter("@declarationReference", SqlDbType.VarChar, JobDeclarationSchema.JE_DeclarationReference.MaxLength, declarationReference ?? Guid.NewGuid().ToString("n"));
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType ?? string.Empty);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@carrierCode", SqlDbType.VarChar, carrierCode ?? string.Empty);
				command.AddParameter("@addInfo", SqlDbType.VarChar, JobDeclarationSchema.JE_AddInfo.MaxLength, addInfo ?? string.Empty);

				if (shipmentPK != null)
				{
					command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (supplierPK != null)
				{
					command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplierPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (shipToPartyAddressPK != null)
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, shipToPartyAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (forwarderPK != null)
				{
					command.AddParameter("@forwarderPK", SqlDbType.UniqueIdentifier, forwarderPK);
				}
				else
				{
					command.AddParameter("@forwarderPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (importerPK != null)
				{
					command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				}
				else
				{
					command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				if (customsCommencedDate != null)
				{
					command.AddParameter("@customsCommencedDate", SqlDbType.DateTime, customsCommencedDate);
				}
				else
				{
					command.AddParameter("@customsCommencedDate", SqlDbType.DateTime, DBNull.Value);
				}

				command.AddParameter("@declarantAddressPK", SqlDbType.UniqueIdentifier, declarantAddressPK == null ? DBNull.Value : declarantAddressPK);

				command.AddParameter("@customCommencedUser", SqlDbType.VarChar, JobDeclarationSchema.JE_GS_NKCustomsCommencedUser.MaxLength, customCommencedUser ?? string.Empty);
				command.AddParameter("@agentsReference", SqlDbType.VarChar, agentsReference ?? string.Empty);
				command.AddParameter("@brokerCode", SqlDbType.VarChar, brokerCode ?? string.Empty);
				command.AddParameter("@ownerRef", SqlDbType.VarChar, ownerReference ?? string.Empty);

				command.AddParameter("@valuationDate", SqlDbType.Date, valuationDate != null ? valuationDate : DBNull.Value);
				command.AddParameter("@entryAuthorisationDate", SqlDbType.SmallDateTime, entryAuthorisationDate != null ? entryAuthorisationDate : DBNull.Value);

				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		public static Guid CreateJobDeclaration(
			string declarationReference,
			Guid branchPK,
			Guid companyPK,
			string applicationCode,
			Guid importer,
			Guid supplier,
			string customsOffice,
			string masterBill,
			string portOfLoading,
			string portOfArrival,
			string transportMode,
			string voyageFlightNo,
			string houseBill,
			DateTime createTime,
			string messageType,
			int clusterKey
			)
		{
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobDeclaration(
	JE_PK,
	JE_DataModel,
	JE_DeclarationReference,
	JE_GB,
	JE_GC,
	JE_ApplicationCode,
	JE_OH_Importer,

	JE_OH_Supplier,
	JE_CustomsOffice,
	JE_MasterBill,
	JE_RL_NKPortOfLoading,
	JE_RL_NKPortOfArrival,

	JE_TransportMode,
	JE_VoyageFlightNo,
	JE_HouseBill,
	JE_SystemCreateTimeUtc,
	JE_MessageType,

	JE_ClusterKey,

	JE_SystemCreateUser,
	JE_SystemLastEditTimeUtc,
	JE_SystemLastEditUser
) VALUES (
	@je_pk,
	'!!',
	@declarationReference,
	@branchPK,
	@companyPK,
	@applicationCode,
	@importer,

	@supplier,
	@customsOffice,
	@masterBill,
	@portOfLoading,
	@portOfArrival,

	@transportMode,
	@voyageFlightNo,
	@houseBill,
	@createTime,
	@messageType,

	@clusterKey,

	'~BP',
	GetUtcDate(),
	'~BP'
)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@je_pk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationReference);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				command.AddParameter("@applicationCode", SqlDbType.VarChar, applicationCode);
				command.AddParameter("@importer", SqlDbType.UniqueIdentifier, importer);

				command.AddParameter("@supplier", SqlDbType.UniqueIdentifier, supplier);
				command.AddParameter("@customsOffice", SqlDbType.VarChar, customsOffice);
				command.AddParameter("@masterBill", SqlDbType.VarChar, masterBill);
				command.AddParameter("@portOfLoading", SqlDbType.VarChar, portOfLoading);
				command.AddParameter("@portOfArrival", SqlDbType.VarChar, portOfArrival);

				command.AddParameter("@transportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@voyageFlightNo", SqlDbType.VarChar, voyageFlightNo);
				command.AddParameter("@houseBill", SqlDbType.VarChar, houseBill);
				command.AddParameter("@createTime", SqlDbType.SmallDateTime, createTime);

				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);

				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobDeclaration(
			string declarationReference,
			Guid branchPK,
			Guid companyPK,
			string masterBill,
			string transportMode,
			string voyageFlightNo,
			string houseBill,
			DateTime exportDate,
			string messageType,
			string goodsDescription,
			string ownerReference,
			Guid importer,
			string vessel,
			Guid shipment,
			int clusterKey
			)
		{
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobDeclaration(
	JE_PK,
	JE_DataModel,
	JE_DeclarationReference,
	JE_GB,
	JE_GC,
	JE_MasterBill,
	JE_TransportMode,
	JE_VoyageFlightNo,
	JE_HouseBill,
	JE_ExportDate,
	JE_MessageType,
	JE_GoodsDescription,
	JE_OwnerRef,
	JE_OH_Importer,
	JE_VesselName,
	JE_JS,
	JE_ClusterKey,
	JE_SystemCreateTimeUtc,
	JE_SystemCreateUser,
	JE_SystemLastEditTimeUtc,
	JE_SystemLastEditUser

) VALUES (
	@je_pk,
	'!!',
	@declarationReference,
	@branchPK,
	@companyPK,
	@masterBill,
	@transportMode,
	@voyageFlightNo,
	@houseBill,
	@exportDate,
	@messageType,
	@goodsDescription,
	@ownerReference,
	@importer,
	@vessel,
	@shipment,
	@clusterKey,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@je_pk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationReference);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@masterBill", SqlDbType.VarChar, masterBill);
				command.AddParameter("@transportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@voyageFlightNo", SqlDbType.VarChar, voyageFlightNo);
				command.AddParameter("@houseBill", SqlDbType.VarChar, houseBill);
				command.AddParameter("@exportDate", SqlDbType.SmallDateTime, exportDate);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@goodsDescription", SqlDbType.VarChar, goodsDescription);
				command.AddParameter("@ownerReference", SqlDbType.VarChar, ownerReference);
				command.AddParameter("@importer", SqlDbType.UniqueIdentifier, importer);
				command.AddParameter("@vessel", SqlDbType.VarChar, vessel);
				command.AddParameter("@shipment", SqlDbType.UniqueIdentifier, shipment);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string declarationReference, string messageType, string transportMode, string vessel, string voyageFlightNo, DateTime dateOfArrival, int clusterKey, string ownerReference = "OwnerReference-1", string masterBill = "MasterBill-1", string houseBill = "HouseBill-1", string customsOffice = "OF1", DateTime? jobRegistrationDate = null, string dataModel = null, DateTime? createTime = null)
		{
			var declarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDeclaration (
	JE_PK,
	JE_DataModel,
	JE_GB,
	JE_GC,
	JE_DeclarationReference,
	JE_MessageType,
	JE_TransportMode,
	JE_VesselName,
	JE_VoyageFlightNo,
	JE_DateOfArrival,
	JE_OwnerRef,
	JE_MasterBill,
	JE_HouseBill,
	JE_CustomsOffice,
	JE_ClusterKey,
	JE_SystemCreateTimeUtc,
	JE_SystemCreateUser,
	JE_SystemLastEditTimeUtc,
	JE_SystemLastEditUser)
VALUES (
	@declarationPK,
	@dataModel,
	@branchPK,
	@companyPK,
	@declarationReference,
	@messageType,
	@transportMode,
	@vessel,
	@voyageFlightNo,
	@dateOfArrival,
	@ownerReference,
	@masterBill,
	@houseBill,
	@customsOffice,
	@clusterKey,
	@createTime,
	'~BP',
	GetUtcDate(),
	'~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, JobDeclarationSchema.JE_DeclarationReference.MaxLength, declarationReference ?? string.Empty);
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType ?? string.Empty);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobDeclarationSchema.JE_TransportMode.MaxLength, transportMode ?? string.Empty);
				command.AddParameter("@vessel", SqlDbType.VarChar, JobDeclarationSchema.JE_VesselName.MaxLength, vessel ?? string.Empty);
				command.AddParameter("@voyageFlightNo", SqlDbType.VarChar, JobDeclarationSchema.JE_VoyageFlightNo.MaxLength, voyageFlightNo ?? string.Empty);
				command.AddParameter("@dateOfArrival", SqlDbType.DateTime, dateOfArrival);
				command.AddParameter("@ownerReference", SqlDbType.VarChar, JobDeclarationSchema.JE_OwnerRef.MaxLength, ownerReference ?? string.Empty);
				command.AddParameter("@masterBill", SqlDbType.VarChar, JobDeclarationSchema.JE_MasterBill.MaxLength, masterBill ?? string.Empty);
				command.AddParameter("@houseBill", SqlDbType.VarChar, JobDeclarationSchema.JE_HouseBill.MaxLength, houseBill ?? string.Empty);
				command.AddParameter("@customsOffice", SqlDbType.VarChar, JobDeclarationSchema.JE_CustomsOffice.MaxLength, customsOffice ?? string.Empty);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@createTime", SqlDbType.DateTime, createTime ?? DateTime.UtcNow);

				if (jobRegistrationDate != null)
				{
					command.AddParameter("@jobRegistrationDate", SqlDbType.DateTime, jobRegistrationDate);
				}
				else
				{
					command.AddParameter("@jobRegistrationDate", SqlDbType.DateTime, DBNull.Value);
				}
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel ?? "!!");
				_ = command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		public static Guid CreateJobUSDeclaration(Guid declarationPK, Guid fPPIPK, int clusterKey)
		{
			var usDeclarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobUSDeclaration (USD_PK, USD_JE, USD_OH_ForeignPrincipalParty, USD_ClusterKey, USD_SystemCreateTimeUtc, USD_SystemCreateUser, USD_SystemLastEditTimeUtc, USD_SystemLastEditUser)
VALUES (@usDeclarationPK, @declarationPK, @fPPIPK, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@usDeclarationPK", SqlDbType.UniqueIdentifier, usDeclarationPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@fPPIPK", SqlDbType.UniqueIdentifier, fPPIPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
			return usDeclarationPK;
		}

		public static Guid CreateCusEntryInstruction(Guid je, string style, string description, DateTime assessmentDate, int clusterKey, string addInfo = "")
		{
			var ceiPk = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusEntryInstruction
				([CEI_PK]
				,[CEI_DataModel]
				,[CEI_Style]
				,[CEI_JE]
				,[CEI_Description]
				,[CEI_DateForDuty]
				,[CEI_ClusterKey]
				,[CEI_AddInfo]
				,[CEI_SystemCreateTimeUtc]
				,[CEI_SystemCreateUser]
				,[CEI_SystemLastEditTimeUtc]
				,[CEI_SystemLastEditUser])
			VALUES
				(@CEI_PK
				,'!!'
				,@CEI_Style
				,@CEI_JE
				,@CEI_Description
				,@CEI_DateForDuty
				,@CEI_ClusterKey
				,@CEI_AddInfo
				,GetUtcDate()
				,'~BP'
				,GetUtcDate()
				,'~BP')
			";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CEI_PK", SqlDbType.UniqueIdentifier, ceiPk);
				command.AddParameter("@CEI_Style", SqlDbType.VarChar, style);
				command.AddParameter("@CEI_JE", SqlDbType.UniqueIdentifier, je);
				command.AddParameter("@CEI_Description", SqlDbType.VarChar, description);
				command.AddParameter("@CEI_DateForDuty", SqlDbType.SmallDateTime, assessmentDate);
				command.AddParameter("@CEI_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@CEI_AddInfo", SqlDbType.VarChar, addInfo);
				command.ExecuteNonQuery();
			}

			return ceiPk;
		}

		public static Guid CreateCusEntryInstruction(
			string style, Guid je, Guid oaWarehouse, string description, string addInfo, Guid ohBondHolder, Guid ohCarrier, Guid oaWareHouse2, Guid ohOwner, DateTime assessmentDate, int clusterKey)
		{
			var ceiPk = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.CusEntryInstruction(
	[CEI_PK],
	[CEI_DataModel],
	[CEI_Style],
	[CEI_JE],
	[CEI_OA_Warehouse],
	[CEI_Description],

	[CEI_AddInfo],
	[CEI_OH_BondHolder],
	[CEI_OH_Carrier],
	[CEI_OA_Warehouse2],
	[CEI_OH_Owner],
	[CEI_DateForDuty],

	[CEI_ClusterKey],

	[CEI_SystemCreateTimeUtc],
	[CEI_SystemCreateUser],
	[CEI_SystemLastEditTimeUtc],
	[CEI_SystemLastEditUser])
VALUES (
	@ceiPk,
	'!!',
	@style,
	@je,
	@oaWarehouse,
	@description,

	@addInfo,
	@ohBondHolder,
	@ohCarrier,
	@oaWareHouse2,
	@ohOwner,
	@assessmentDate,

	@clusterKey,

	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP');
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ceiPk", SqlDbType.UniqueIdentifier, ceiPk);
				command.AddParameter("@style", SqlDbType.VarChar, style);
				command.AddParameter("@je", SqlDbType.UniqueIdentifier, je);
				command.AddParameter("@oaWarehouse", SqlDbType.UniqueIdentifier, oaWarehouse);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@ohBondHolder", SqlDbType.UniqueIdentifier, ohBondHolder);
				command.AddParameter("@ohCarrier", SqlDbType.UniqueIdentifier, ohCarrier);
				command.AddParameter("@oaWareHouse2", SqlDbType.UniqueIdentifier, oaWareHouse2);
				command.AddParameter("@ohOwner", SqlDbType.UniqueIdentifier, ohOwner);
				command.AddParameter("@assessmentDate", SqlDbType.SmallDateTime, assessmentDate);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return ceiPk;
		}

		public static Guid CreateCusEntryHeader(
			bool isValid,
			string messageType,
			string status,
			string entryStatus,

			string bgmReference,
			float totalPaid,
			int linenum,
			string addInfo,
			Guid jePk,

			DateTime? entrySubmittedDate,
			DateTime? entryReleaseDate,
			Guid instruction,
			string warehouseTransactionStatus,
			DateTime? warehouseReleaseDate,
			DateTime? bondAcquittedDate,
			DateTime? bondValidToDate,

			int clusterKey
			)
		{
			var result = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.CusEntryHeader (
		[CH_PK],
		[CH_DataModel],
		[CH_IsValid],
		[CH_MessageType],
		[CH_Status],
		[CH_EntryStatus],

		[CH_BGMReference],
		[CH_TotalPaid],
		[CH_HighestLineNumber],
		[CH_AddInfo],
		[CH_JE],

		[CH_EntrySubmittedDate],
		[CH_EntryReleaseDate],
		[CH_CEI_Instruction],
		[CH_WarehouseTransactionStatus],
		[CH_WarehouseReleaseDate],
		[CH_BondAcquittedDate],
		[CH_BondValidToDate],

		[CH_ClusterKey],
		[CH_SystemCreateTimeUtc],
		[CH_SystemCreateUser],
		[CH_SystemLastEditTimeUtc],
		[CH_SystemLastEditUser]
	) VALUES (
		@CH_PK,
		'!!',
		@isValid,
		@messageType,
		@status,
		@entryStatus,

		@bgmReference,
		@totalPaid,
		@linenum,
		@addInfo,
		@jePk,

		@entrySubmittedDate,
		@entryReleaseDate,
		@instruction,
		@warehouseTransactionStatus,
		@warehouseReleaseDate,
		@bondAcquittedDate,
		@bondValidToDate,

		@clusterKey,
		GetUtcDate(),
		'~BP',
		GetUtcDate(),
		'~BP'
	);
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CH_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@isValid", SqlDbType.Bit, isValid);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@status", SqlDbType.VarChar, status);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, entryStatus);

				command.AddParameter("@bgmReference", SqlDbType.VarChar, bgmReference);
				command.AddParameter("@totalPaid", SqlDbType.Money, totalPaid);
				command.AddParameter("@linenum", SqlDbType.SmallInt, linenum);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@jePk", SqlDbType.UniqueIdentifier, jePk);

				command.AddParameter("@entrySubmittedDate", SqlDbType.SmallDateTime, entrySubmittedDate ?? (object)DBNull.Value);
				command.AddParameter("@entryReleaseDate", SqlDbType.SmallDateTime, entryReleaseDate ?? (object)DBNull.Value);

				command.AddParameter("@instruction", SqlDbType.UniqueIdentifier, instruction != Guid.Empty ? instruction : DBNull.Value);
				command.AddParameter("@warehouseTransactionStatus", SqlDbType.VarChar, warehouseTransactionStatus);

				command.AddParameter("@warehouseReleaseDate", SqlDbType.SmallDateTime, warehouseReleaseDate ?? (object)DBNull.Value);
				command.AddParameter("@bondAcquittedDate", SqlDbType.Date, bondAcquittedDate ?? (object)DBNull.Value);
				command.AddParameter("@bondValidToDate", SqlDbType.Date, bondValidToDate ?? (object)DBNull.Value);

				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return result;
		}

		public static Guid CreateCusEntryHeader(
			Guid jePk,
			int clusterKey,
			string messageType = "",
			string addInfo = "",
			Guid? primaryEntry = null,
			DateTime? entryReleaseDate = null,
			string dataModel = null,
			string entryStatus = ""
			)
		{
			var result = Guid.NewGuid();
			const string sql = @"
	INSERT INTO dbo.CusEntryHeader (
		[CH_PK],
		[CH_DataModel],
		[CH_JE],
		[CH_ClusterKey],
		[CH_MessageType],
		[CH_AddInfo],
		[CH_CH_PrimeEntry],
		[CH_EntryReleaseDate],
		[CH_EntryStatus],
		[CH_SystemCreateTimeUtc],
		[CH_SystemCreateUser],
		[CH_SystemLastEditTimeUtc],
		[CH_SystemLastEditUser]
	) VALUES (
		@CH_PK,
		@dataModel,
		@jePk,
		@clusterKey,
		@messageType,
		@addInfo,
		@primaryEntry,
		@entryReleaseDate,
		@entryStatus,
		GetUtcDate(),
		'~BP',
		GetUtcDate(),
		'~BP'
	);
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CH_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel ?? "!!");
				command.AddParameter("@entryStatus", SqlDbType.VarChar, entryStatus);
				command.AddParameter("@jePk", SqlDbType.UniqueIdentifier, jePk);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@primaryEntry", SqlDbType.UniqueIdentifier, primaryEntry ?? (object)DBNull.Value);
				command.AddParameter("@entryReleaseDate", SqlDbType.SmallDateTime, entryReleaseDate ?? (object)DBNull.Value);
				command.ExecuteNonQuery();
			}
			return result;
		}

		public static Guid CreateCusEntryHeader(
			bool isValid,
			string messageType,
			string status,
			string entryStatus,
			int linenum,
			Guid jePk,
			DateTime? entrySubmittedDate,
			DateTime? entryReleaseDate,
			Guid instruction,
			DateTime? createTime,
			int clusterKey
			)
		{
			var result = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.CusEntryHeader (
		[CH_PK],
		[CH_DataModel],
		[CH_IsValid],
		[CH_MessageType],
		[CH_Status],
		[CH_EntryStatus],
		[CH_HighestLineNumber],
		[CH_JE],
		[CH_EntrySubmittedDate],
		[CH_EntryReleaseDate],
		[CH_CEI_Instruction],
		[CH_ClusterKey],
		[CH_SystemCreateTimeUtc],
		[CH_SystemCreateUser],
		[CH_SystemLastEditTimeUtc],
		[CH_SystemLastEditUser]
	) VALUES (
		@CH_PK,
		'!!',
		@isValid,
		@messageType,
		@status,
		@entryStatus,
		@linenum,
		@jePk,
		@entrySubmittedDate,
		@entryReleaseDate,
		@instruction,
		@clusterKey,
		@createTime,
		'~BP',
		GetUtcDate(),
		'~BP'
	);
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CH_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@isValid", SqlDbType.Bit, isValid);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@status", SqlDbType.VarChar, status);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, entryStatus);
				command.AddParameter("@linenum", SqlDbType.SmallInt, linenum);
				command.AddParameter("@jePk", SqlDbType.UniqueIdentifier, jePk);
				command.AddParameter("@entrySubmittedDate", SqlDbType.SmallDateTime, entrySubmittedDate ?? (object)DBNull.Value);
				command.AddParameter("@entryReleaseDate", SqlDbType.SmallDateTime, entryReleaseDate ?? (object)DBNull.Value);
				command.AddParameter("@instruction", SqlDbType.UniqueIdentifier, instruction != Guid.Empty ? instruction : DBNull.Value);
				command.AddParameter("@createTime", SqlDbType.Date, createTime ?? (object)DBNull.Value);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return result;
		}

		public static Guid CreateCusEntryLine(Guid clCh, int clusterKey, decimal customsValue = 0m)
		{
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.CusEntryLine([CL_PK], [CL_DataModel], [CL_IsValid], [CL_CH], [CL_ClusterKey], [CL_SystemCreateTimeUtc], [CL_SystemCreateUser], [CL_SystemLastEditTimeUtc], [CL_SystemLastEditUser], [CL_CustomsValue])
VALUES (@clPk, '!!', 1, @clCh, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @customsValue)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@clCh", SqlDbType.UniqueIdentifier, clCh);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@customsValue", SqlDbType.Decimal, customsValue);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusEntryLine(Guid clCh, int clusterKey, int lineNumber, int dutyPercent, int flatAmount, string flatAmountUQ)
		{
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.CusEntryLine([CL_PK], [CL_DataModel], [CL_CH], [CL_ClusterKey], [CL_LineNumber], [CL_DutyPercent], [CL_FlatAmount], [CL_FlatAmountUQ], [CL_SystemCreateTimeUtc], [CL_SystemCreateUser], [CL_SystemLastEditTimeUtc], [CL_SystemLastEditUser])
VALUES (@clPk, '!!', @clCh, @clusterKey, @lineNumber, @dutyPercent, @flatAmount, @flatAmountUQ, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@clCh", SqlDbType.UniqueIdentifier, clCh);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNumber", SqlDbType.Int, lineNumber);
				command.AddParameter("@dutyPercent", SqlDbType.Int, dutyPercent);
				command.AddParameter("@flatAmount", SqlDbType.Int, flatAmount);
				command.AddParameter("@flatAmountUQ", SqlDbType.VarChar, flatAmountUQ);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusEntryLine(Guid entryHeaderPK, int clusterKey, int lineNumber, decimal customsValue, string adValoremTariff)
		{
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.CusEntryLine (
	[CL_PK],
	[CL_DataModel],
	[CL_CH],
	[CL_ClusterKey],
	[CL_LineNumber],
	[CL_CustomsValue],
	[CL_AdValoremTariff],
	[CL_SystemCreateTimeUtc],
	[CL_SystemCreateUser],
	[CL_SystemLastEditTimeUtc],
	[CL_SystemLastEditUser])
VALUES (
	@clPk,
	'!!',
	@clCh,
	@clusterKey,
	@lineNumber,
	@customsValue,
	@adValoremTariff,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@clCh", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNumber", SqlDbType.Int, lineNumber);
				command.AddParameter("@customsValue", SqlDbType.Decimal, customsValue);
				command.AddParameter("@adValoremTariff", SqlDbType.VarChar, CusEntryLineSchema.CL_AdValoremTariff.MaxLength, adValoremTariff);
				_ = command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusEntryHeaderCharges(Guid c1CH, string chargeType, decimal chargeAmount, int clusterKey, string source = "CW1")
		{
			var result = Guid.NewGuid();

			const string sql = @"
	INSERT INTO dbo.CusEntryHeaderCharges(
		[C1_PK],
		[C1_IsValid],
		[C1_ChargeType],
		[C1_ChargeAmount],
		[C1_CH],
		[C1_IsLandedCostOnly],
		[C1_Source],
		[C1_ClusterKey], C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser
	) VALUES (
		@clPk,
		0,
		@chargeType,
		@chargeAmount,
		@c1CH,
		0,
		@c1Source,
		@clusterKey, getutcdate(), '~BP', getutcdate(), '~BP'
	);
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmount", SqlDbType.Money, chargeAmount);
				command.AddParameter("@c1CH", SqlDbType.UniqueIdentifier, c1CH);
				command.AddParameter("@c1Source", SqlDbType.VarChar, source);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateProcessTasks(int sequence, Guid parentPK, string parentTableCode, string type, DateTime actualDate, string description, bool isPublished)
		{
			var result = Guid.NewGuid();

			const string sql = @"
	INSERT INTO dbo.ProcessTasks(P9_PK, P9_Sequence, P9_ParentID, P9_ParentTableCode, P9_Type, P9_ActualDate, P9_Description, P9_IsPublished)
	values(@pk, @sequence, @parentPK, @parentTableCode, @type, @actualDate, @description, @isPublished);
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@sequence", SqlDbType.Int, sequence);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@type", SqlDbType.VarChar, type);
				command.AddParameter("@actualDate", SqlDbType.DateTime, actualDate);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@isPublished", SqlDbType.Bit, isPublished);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusLiquidation(Guid b8_je, Guid b8_gc, int b8_ClusterKey, DateTime b8_LiquidationDate, DateTime b8_SystemCreateDate)
		{
			var result = Guid.NewGuid();

			const string sql = @"
	INSERT dbo.CusLiquidation (B8_PK, B8_JE, B8_GC, B8_ClusterKey, B8_LiquidationDate, B8_SystemCreateDate, B8_SystemCreateTimeUtc, B8_SystemCreateUser, B8_SystemLastEditTimeUtc, B8_SystemLastEditUser)
VALUES(@pk, @B8_JE, @B8_GC, @B8_ClusterKey, @B8_LiquidationDate, @B8_SystemCreateDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@b8_je", SqlDbType.UniqueIdentifier, b8_je);
				command.AddParameter("@b8_gc", SqlDbType.UniqueIdentifier, b8_gc);
				command.AddParameter("@b8_ClusterKey", SqlDbType.Int, b8_ClusterKey);
				command.AddParameter("@b8_LiquidationDate", SqlDbType.DateTime, b8_LiquidationDate);
				command.AddParameter("@b8_SystemCreateDate", SqlDbType.DateTime, b8_SystemCreateDate);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusEntryLineFee(Guid cfCl, string chargeType, float chargeAmont, int clusterKey, string source = "CW1")
		{
			var result = Guid.NewGuid();

			var sql = @"
	INSERT INTO dbo.CusEntryLineFee(
		[CF_PK],
		[CF_IsValid],
		[CF_ChargeType],
		[CF_ChargeAmount],
		[CF_CL],
		[CF_Source],
		[CF_IsLandedCostOnly],

		[CF_ClusterKey],

		[CF_SystemCreateTimeUtc],
		[CF_SystemCreateUser],
		[CF_SystemLastEditTimeUtc],
		[CF_SystemLastEditUser]
	) VALUES (
		@clPk,
		1,
		@chargeType,
		@chargeAmont,
		@cfCl,
		@cfSource,
		0,

		@clusterKey,

		GetUtcDate(),
		'~BP',
		GetUtcDate(),
		'~BP'
	);
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@chargeType", SqlDbType.VarChar, chargeType);
				command.AddParameter("@chargeAmont", SqlDbType.Money, chargeAmont);
				command.AddParameter("@cfCl", SqlDbType.UniqueIdentifier, cfCl);
				command.AddParameter("@cfSource", SqlDbType.VarChar, source);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusEntryPayInfo(string incomingPayResponseNo, decimal paymentAmount, string transactionType, DateTime? paymentDate, string paymentReference, string paymentParty, string paymentStatus, Guid chPK, DateTime receiptDate, int clusterKey)
		{
			var pk = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusEntryPayInfo
				([C9_PK]
				,[C9_IsValid]
				,[C9_IncomingPayResponseNo]
				,[C9_PaymentAmount]
				,[C9_TransactionType]
				,[C9_PaymentDate]
				,[C9_PaymentReference]
				,[C9_PaymentParty]
				,[C9_PaymentStatus]
				,[C9_CH]
				,[C9_ReceiptDate]
				,[C9_ClusterKey]
				,[C9_SystemCreateTimeUtc]
				,[C9_SystemCreateUser]
				,[C9_SystemLastEditTimeUtc]
				,[C9_SystemLastEditUser])
			VALUES
				(@C9_PK
				,1
				,@C9_IncomingPayResponseNo
				,@C9_PaymentAmount
				,@C9_TransactionType
				,@C9_PaymentDate
				,@C9_PaymentReference
				,@C9_PaymentParty
				,@C9_PaymentStatus
				,@C9_CH
				,@C9_ReceiptDate
				,@C9_ClusterKey
				,GetUtcDate()
				,'~BP'
				,GetUtcDate()
				,'~BP')
			";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@C9_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@C9_IncomingPayResponseNo", SqlDbType.VarChar, incomingPayResponseNo);
				command.AddParameter("@C9_PaymentAmount", SqlDbType.Money, paymentAmount);
				command.AddParameter("@C9_TransactionType", SqlDbType.VarChar, transactionType);
				command.AddParameter("@C9_PaymentDate", SqlDbType.SmallDateTime, paymentDate ?? (object)DBNull.Value);
				command.AddParameter("@C9_PaymentReference", SqlDbType.VarChar, paymentReference);
				command.AddParameter("@C9_PaymentParty", SqlDbType.VarChar, paymentParty);
				command.AddParameter("@C9_PaymentStatus", SqlDbType.VarChar, paymentStatus);
				command.AddParameter("@C9_CH", SqlDbType.UniqueIdentifier, chPK);
				command.AddParameter("@C9_ReceiptDate", SqlDbType.Date, receiptDate);
				command.AddParameter("@C9_ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static void CreateCusUnderBondDec(Guid cusEntryLinePK, Guid invoiceLinePK, int clusterKey)
		{
			var sql = @"
			INSERT INTO dbo.CusUnderBondDec (BU_PK, BU_CL, BU_ClusterKey, BU_JI, BU_SequenceNumber, BU_Status, BU_SystemCreateTimeUtc, BU_SystemCreateUser, BU_SystemLastEditTimeUtc, BU_SystemLastEditUser)
			VALUES (NEWID(), @CusEntryLine, @clusterKey, @InvLine, 0, '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CusEntryLine", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@InvLine", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateCusStatementHeader(Guid companyPK, string statementNumber, string statementType, int isMonthlyStatement, string entryFilerCode, string accountNo, Guid? importerPK = null)
		{
			var result = Guid.NewGuid();

			var sql = @"INSERT INTO dbo.CusStatementHeader (B2_PK, B2_GC, B2_IsMonthlyStatement, B2_EntryFilerCode, B2_StatementType, B2_StatementNumber, B2_AccountNo, B2_OH_Importer, B2_SystemCreateTimeUtc, B2_SystemCreateUser, B2_SystemLastEditTimeUtc, B2_SystemLastEditUser)
VALUES (@B2PK, @CompanyPK, @IsMonthlyStatement, @EntryFilerCode, @StatementType, @StatementNo, @AccountNo, @ImporterPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@B2PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@IsMonthlyStatement", SqlDbType.Bit, isMonthlyStatement);
				command.AddParameter("@EntryFilerCode", SqlDbType.VarChar, entryFilerCode);
				command.AddParameter("@StatementType", SqlDbType.VarChar, statementType);
				command.AddParameter("@StatementNo", SqlDbType.VarChar, statementNumber);
				command.AddParameter("@AccountNo", SqlDbType.VarChar, accountNo);

				if (importerPK != null)
				{
					command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, importerPK);
				}
				else
				{
					command.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}

				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusStatementLine(Guid statementPK, string entryNumber, string entryType, int? amount = 0, string brokerReference = "")
		{
			var result = Guid.NewGuid();
			var statementLinePKSql = @"
INSERT INTO dbo.CusStatementLine (B3_PK, B3_B2, B3_EntryNum, B3_EntryType, B3_CustomsFeesTotal, B3_BrokerReference, B3_SystemCreateTimeUtc, B3_SystemCreateUser, B3_SystemLastEditTimeUtc, B3_SystemLastEditUser)
VALUES (@LinePK, @StatementPK, @EntryNum, @EntryType, @Amount, @BrokerReference, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";
			using (var command = Db.Connection.Command(statementLinePKSql))
			{
				command.AddParameter("@LinePK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@StatementPK", SqlDbType.UniqueIdentifier, statementPK);
				command.AddParameter("@EntryNum", SqlDbType.VarChar, entryNumber);
				command.AddParameter("@EntryType", SqlDbType.VarChar, entryType);
				command.AddParameter("@Amount", SqlDbType.Money, amount);
				command.AddParameter("@BrokerReference", SqlDbType.VarChar, CusStatementLineSchema.B3_BrokerReference.MaxLength, brokerReference);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static void CreateCusStatementLineCharge(Guid statementLinePK, string chargeType, int chargeAmount, string paymentParty = "")
		{
			var statementLinePKSql = @"
INSERT INTO dbo.CusStatementLineCharge (B4_PK, B4_B3, B4_ChargeType, B4_ChargeAmount, B4_PaymentParty, B4_SystemCreateTimeUtc, B4_SystemCreateUser, B4_SystemLastEditTimeUtc, B4_SystemLastEditUser)
VALUES (Newid(), @StatementLinePK, @ChargeType, @Amount, @PaymentParty, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";
			using (var command = Db.Connection.Command(statementLinePKSql))
			{
				command.AddParameter("@StatementLinePK", SqlDbType.UniqueIdentifier, statementLinePK);
				command.AddParameter("@ChargeType", SqlDbType.VarChar, CusStatementLineChargeSchema.B4_ChargeType.MaxLength, chargeType);
				command.AddParameter("@Amount", SqlDbType.Money, chargeAmount);
				command.AddParameter("@PaymentParty", SqlDbType.VarChar, paymentParty);
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateRefCusRateCode(string rateCode, Guid zzrRateType, string description)
		{
			var result = Guid.NewGuid();

			var sql = @"
	INSERT INTO RefDatabase_RefCusRateCode(
		   [ZY1_PK]
		  ,[ZY1_RateCode]
		  ,[ZY1_ZZR_RateType]
		  ,[ZY1_Description]
	) VALUES (
		@zy1_Pk,
		@rateCode,
		@zzrRateType,
		@description
	);
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@zy1_Pk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@rateCode", SqlDbType.VarChar, rateCode);
				command.AddParameter("@zzrRateType", SqlDbType.UniqueIdentifier, zzrRateType);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateRefCusRateType(string rateType, string description, int isPayable, string zzzNKDataGrouping, string customsValueFormula)
		{
			var result = Guid.NewGuid();

			var sql = @"
	INSERT INTO RefDatabase_RefCusRateType(
			[ZZR_PK]
		   ,[ZZR_RateType]
		   ,[ZZR_Description]
		   ,[ZZR_IsPayable]
		   ,[ZZR_ZZZ_NKDataGrouping]
		   ,[ZZR_CustomsValueFormula]
	) VALUES (
			@zzr_PK,
			@rateType,
			@description,
			@isPayable,
			@zzzNKDataGrouping,
			@customsValueFormula
	);
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@zzr_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@rateType", SqlDbType.VarChar, rateType);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@isPayable", SqlDbType.Bit, isPayable);
				command.AddParameter("@zzzNKDataGrouping", SqlDbType.VarChar, zzzNKDataGrouping);
				command.AddParameter("@customsValueFormula", SqlDbType.VarChar, customsValueFormula);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusCodeData(string cyType, string cyCode, string data, Guid cyParentId, string parentTableCode = null)
		{
			var result = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusCodeData(
	[CY_PK],
	[CY_Type],
	[CY_Code],
	[CY_Data],
	[CY_ParentID],

	[CY_ParentTableCode],
	[CY_IsValid],
	[CY_Order],
	[CY_IsOverridden],
	[CY_SystemCreateTimeUtc],
	[CY_SystemCreateUser],
	[CY_SystemLastEditTimeUtc],
	[CY_SystemLastEditUser]
) VALUES (
	@cyPk,
	@cyType,
	@cyCode,
	@cyData,
	@cyParentId,
	@cyParentTableCode,

	1,
	0,
	0,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
);";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cyType", SqlDbType.VarChar, cyType);
				command.AddParameter("@cyPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@cyCode", SqlDbType.VarChar, cyCode);
				command.AddParameter("@cyData", SqlDbType.VarChar, data);
				command.AddParameter("@cyParentId", SqlDbType.UniqueIdentifier, cyParentId);
				command.AddParameter("@cyParentTableCode", SqlDbType.VarChar, parentTableCode ?? "CH");
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateOrganisation(string code, string name, string closestPort = "", Guid? orgHeaderPK = null)
		{
			var organisationPK = orgHeaderPK ?? Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
VALUES (@organisationPK, @code, @name, @closestPort, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgHeaderSchema.OH_Code.MaxLength, code);
				command.AddParameter("@name", SqlDbType.VarChar, OrgHeaderSchema.OH_FullName.MaxLength, name);
				command.AddParameter("@closestPort", SqlDbType.VarChar, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength, closestPort);
				command.ExecuteNonQuery();
			}
			return organisationPK;
		}

		public static Guid CreateCrmOpportunity(string oppID, string oppName, Guid orgPk, Guid companyPk, string startDate = null, string endDate = null, string salesPerson = "", bool isRestricted = false)
		{
			var oppPK = Guid.NewGuid();

			var sql = @$"
INSERT INTO dbo.CrmOpportunity(COP_PK, COP_OpportunityID, COP_OpportunityName, COP_OH_Organization, COP_GC_Company, COP_GS_NKSalesPerson, COP_IsRestricted, COP_RX_NKOverallCurrency, COP_EffectiveStartDate, COP_EffectiveEndDate, COP_SystemCreateTimeUtc, COP_SystemCreateUser, COP_SystemLastEditTimeUtc, COP_SystemLastEditUser)
VALUES (@oppPK, @oppID, @oppName, @orgPk, @companyPk, @salesPerson, @isRestricted, @currency, @startDate, @endDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@oppPK", oppPK, CrmOpportunitySchema.PK);
				command.AddParameterBasedOnDbColumn("@oppID", oppID, CrmOpportunitySchema.COP_OpportunityID);
				command.AddParameterBasedOnDbColumn("@oppName", oppName, CrmOpportunitySchema.COP_OpportunityName);
				command.AddParameterBasedOnDbColumn("@orgPk", orgPk, CrmOpportunitySchema.COP_OH_Organization);
				command.AddParameterBasedOnDbColumn("@companyPk", companyPk, CrmOpportunitySchema.COP_GC_Company);
				command.AddParameterBasedOnDbColumn("@salesPerson", salesPerson, CrmOpportunitySchema.COP_GS_NKSalesPerson);
				command.AddParameterBasedOnDbColumn("@isRestricted", isRestricted, CrmOpportunitySchema.COP_IsRestricted);
				command.AddParameterBasedOnDbColumn("@currency", "AUD", CrmOpportunitySchema.COP_RX_NKOverallCurrency);

				if (startDate != null)
				{
					command.AddParameterBasedOnDbColumn("@startDate", startDate, CrmOpportunitySchema.COP_EffectiveStartDate);
				}
				else
				{
					command.AddParameterBasedOnDbColumn("@startDate", DBNull.Value, CrmOpportunitySchema.COP_EffectiveStartDate);
				}

				if (endDate != null)
				{
					command.AddParameterBasedOnDbColumn("@endDate", endDate, CrmOpportunitySchema.COP_EffectiveEndDate);
				}
				else
				{
					command.AddParameterBasedOnDbColumn("@endDate", DBNull.Value, CrmOpportunitySchema.COP_EffectiveEndDate);
				}
				command.ExecuteNonQuery();
			}
			return oppPK;
		}

		public static Guid CreateCrmOpportunityContact(Guid oppPK, Guid contactPK, string role, bool primary)
		{
			var opportunityContactPK = Guid.NewGuid();

			var sql = @$"
INSERT INTO dbo.CrmOpportunityContact(COC_PK, COC_COP_Opportunity, COC_OC_Contact, COC_Role, COC_Primary, COC_SystemCreateTimeUtc, COC_SystemCreateUser, COC_SystemLastEditTimeUtc, COC_SystemLastEditUser)
VALUES (@opportunityContactPK, @oppPK, @contactPK, @role, @primary, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@opportunityContactPK", SqlDbType.UniqueIdentifier, opportunityContactPK);
				command.AddParameter("@oppPK", SqlDbType.UniqueIdentifier, oppPK);
				command.AddParameter("@contactPK", SqlDbType.UniqueIdentifier, contactPK);
				command.AddParameter("@role", SqlDbType.VarChar, 3, role);
				command.AddParameter("@primary", SqlDbType.Bit, primary);
				command.ExecuteNonQuery();
			}
			return opportunityContactPK;
		}

		public static Guid CreateCrmOpportunityScope(Guid oppPK, byte scopeID, Guid? refContainer = null, Guid? pickupAddress = null, Guid? deliveryAddress = null, string productCode = "FWD", string origin = "AUSYD", string destination = "BDUSA", string transportMode = "AIR", string containerMode = "ULD",
			string pickupAddressPostCode = "", string deliveryAddressPostCode = "", string commodityCode = "", string incoTerm = "", string serviceLevel = "", string notes = "NOTES")
		{
			var oppScopePK = Guid.NewGuid();

			var sql = @"INSERT INTO dbo.CrmOpportunityScope (COS_PK, COS_COP_Opportunity, COS_RC_ContainerType, COS_ProductCode, COS_Origin, COS_Destination, COS_TransportMode, COS_ContainerMode, COS_PickupAddressPostCode, COS_DeliveryAddressPostCode, COS_OA_PickupAddress, COS_OA_DeliveryAddress, COS_RH_NKCommodityCode, COS_IncoTerm, COS_RS_NKServiceLevel,
						COS_Enabled, COS_DisabledStatus, COS_Notes, COS_EstimatedRevenue, COS_EstimatedProfit, COS_EstimatedVolume, COS_EstimatedVolumeUQ, COS_EstimatedWeight, COS_EstimatedWeightUQ, COS_RX_NKScopeCurrency, COS_ScopeID, COS_SystemCreateTimeUtc, COS_SystemLastEditTimeUtc, COS_SystemCreateUser, COS_SystemLastEditUser)
					VALUES (@oppScopePK, @oppPK, @refContainer, @productCode, @origin, @destination, @transportMode, @containerMode, @pickupAddressPostCode, @deliveryAddressPostCode, @pickupAddress, @deliveryAddress, @commodityCode, @incoTerm, @serviceLevel, 1, '', @notes,
						100, 20, 30, 'M3', 40, 'KG', 'AUD', @scopeID, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@oppScopePK", oppScopePK, CrmOpportunityScopeSchema.PK);
				command.AddParameterBasedOnDbColumn("@oppPK", oppPK, CrmOpportunityScopeSchema.COS_COP_Opportunity);
				command.AddParameterBasedOnDbColumn("@productCode", productCode, CrmOpportunityScopeSchema.COS_ProductCode);
				command.AddParameterBasedOnDbColumn("@origin", origin, CrmOpportunityScopeSchema.COS_Origin);
				command.AddParameterBasedOnDbColumn("@destination", destination, CrmOpportunityScopeSchema.COS_Destination);
				command.AddParameterBasedOnDbColumn("@transportMode", transportMode, CrmOpportunityScopeSchema.COS_TransportMode);
				command.AddParameterBasedOnDbColumn("@containerMode", containerMode, CrmOpportunityScopeSchema.COS_ContainerMode);
				command.AddParameterBasedOnDbColumn("@pickupAddressPostCode", pickupAddressPostCode, CrmOpportunityScopeSchema.COS_PickupAddressPostCode);
				command.AddParameterBasedOnDbColumn("@deliveryAddressPostCode", deliveryAddressPostCode, CrmOpportunityScopeSchema.COS_DeliveryAddressPostCode);
				command.AddParameterBasedOnDbColumn("@commodityCode", commodityCode, CrmOpportunityScopeSchema.COS_RH_NKCommodityCode);
				command.AddParameterBasedOnDbColumn("@incoTerm", incoTerm, CrmOpportunityScopeSchema.COS_IncoTerm);
				command.AddParameterBasedOnDbColumn("@serviceLevel", serviceLevel, CrmOpportunityScopeSchema.COS_RS_NKServiceLevel);
				command.AddParameterBasedOnDbColumn("@refContainer", refContainer == null ? DBNull.Value : refContainer, CrmOpportunityScopeSchema.COS_RC_ContainerType);
				command.AddParameterBasedOnDbColumn("@pickupAddress", pickupAddress == null ? DBNull.Value : pickupAddress, CrmOpportunityScopeSchema.COS_OA_PickupAddress);
				command.AddParameterBasedOnDbColumn("@deliveryAddress", deliveryAddress == null ? DBNull.Value : deliveryAddress, CrmOpportunityScopeSchema.COS_OA_DeliveryAddress);
				command.AddParameterBasedOnDbColumn("@notes", (byte[])ZBlob.FromUTF8(notes), CrmOpportunityScopeSchema.COS_Notes);
				command.AddParameterBasedOnDbColumn("@scopeID", scopeID, CrmOpportunityScopeSchema.COS_ScopeID);

				command.ExecuteNonQuery();
			}
			return oppScopePK;
		}

		public static Guid CreateCrmOpportunityScope(int scopeID, Guid oppPK, string origin, string destination, string productCode = "FWD")
		{
			var scopePK = Guid.NewGuid();

			var sql = @$"
INSERT INTO dbo.CrmOpportunityScope(COS_PK, COS_ScopeID, COS_COP_Opportunity, COS_ProductCode, COS_Origin, COS_Destination, COS_RX_NKScopeCurrency, COS_Enabled, COS_SystemCreateTimeUtc, COS_SystemCreateUser, COS_SystemLastEditTimeUtc, COS_SystemLastEditUser)
VALUES (@scopePK, @scopeID, @oppPK, @productCode, @origin, @destination, @currency, @enabled, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@scopePK", scopePK, CrmOpportunityScopeSchema.PK);
				command.AddParameterBasedOnDbColumn("@scopeID", scopeID, CrmOpportunityScopeSchema.COS_ScopeID);
				command.AddParameterBasedOnDbColumn("@oppPK", oppPK, CrmOpportunityScopeSchema.COS_COP_Opportunity);
				command.AddParameterBasedOnDbColumn("@productCode", productCode, CrmOpportunityScopeSchema.COS_ProductCode);
				command.AddParameterBasedOnDbColumn("@origin", origin, CrmOpportunityScopeSchema.COS_Origin);
				command.AddParameterBasedOnDbColumn("@destination", destination, CrmOpportunityScopeSchema.COS_Destination);
				command.AddParameterBasedOnDbColumn("@currency", "AUD", CrmOpportunityScopeSchema.COS_RX_NKScopeCurrency);
				command.AddParameterBasedOnDbColumn("@enabled", true, CrmOpportunityScopeSchema.COS_Enabled);
				command.ExecuteNonQuery();
			}
			return scopePK;
		}

		public static Guid CreateCrmOpportunityStageProgress(Guid oppPK, string stage, string description, DateTimeOffset dateStarted, DateTimeOffset? dateCompleted = null)
		{
			var stagePK = Guid.NewGuid();

			var sql = @$"
INSERT INTO dbo.CrmOpportunityStageProgress(CSP_PK, CSP_COP_Opportunity, CSP_Stage, CSP_StageDescription, CSP_DateStarted, CSP_DateCompleted, CSP_SystemCreateTimeUtc, CSP_SystemCreateUser, CSP_SystemLastEditTimeUtc, CSP_SystemLastEditUser)
VALUES (@stagePK, @oppPK, @stage, @description, @dateStarted, @dateCompleted, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@stagePK", SqlDbType.UniqueIdentifier, stagePK);
				command.AddParameter("@oppPK", SqlDbType.UniqueIdentifier, oppPK);
				command.AddParameter("@stage", SqlDbType.VarChar, 3, stage);
				command.AddParameter("@description", SqlDbType.VarChar, 50, description);
				command.AddParameter("@dateStarted", SqlDbType.DateTimeOffset, dateStarted);
				command.AddParameter("@dateCompleted", SqlDbType.DateTimeOffset, dateCompleted == null ? DBNull.Value : dateCompleted);
				command.ExecuteNonQuery();
			}
			return stagePK;
		}

		public static void CreateEntityStaffRestriction(string parentTableCode, Guid parentID, string staffCode)
		{
			var sql = @$"
INSERT INTO dbo.EntityStaffRestriction(ESR_PK, ESR_ParentTableCode, ESR_ParentID, ESR_GS_NKStaff, ESR_SystemCreateTimeUtc, ESR_SystemCreateUser, ESR_SystemLastEditTimeUtc, ESR_SystemLastEditUser)
VALUES (@pk, @parentTableCode, @parentID, @staffCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@pk", Guid.NewGuid(), EntityStaffRestrictionSchema.PK);
				command.AddParameterBasedOnDbColumn("@parentTableCode", parentTableCode, EntityStaffRestrictionSchema.ESR_ParentTableCode);
				command.AddParameterBasedOnDbColumn("@parentID", parentID, EntityStaffRestrictionSchema.ESR_ParentID);
				command.AddParameterBasedOnDbColumn("@staffCode", staffCode, EntityStaffRestrictionSchema.ESR_GS_NKStaff);
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateCusAuthorizationUsage(Guid premisesPK, string parentTableCode, Guid orgHeader, string code, string number, int clusterKey)
		{
			var result = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusAuthorizationUsage(AGC_PK, AGC_Code, AGC_Number, AGC_OH_Owner, AGC_ParentID, AGC_ParentTableCode, AGC_ClusterKey, AGC_SystemCreateTimeUtc, AGC_SystemCreateUser, AGC_SystemLastEditTimeUtc, AGC_SystemLastEditUser)
VALUES (@result, @code, @number, @orgHeader, @premisesPK, @parentTableCode, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@result", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@code", SqlDbType.VarChar, CusAuthorizationUsageSchema.AGC_Code.MaxLength, code);
				command.AddParameter("@number", SqlDbType.VarChar, CusAuthorizationUsageSchema.AGC_Number.MaxLength, number);
				command.AddParameter("@orgHeader", SqlDbType.UniqueIdentifier, orgHeader);
				command.AddParameter("@premisesPK", SqlDbType.UniqueIdentifier, premisesPK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, CusAuthorizationUsageSchema.AGC_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return result;
		}

		public static Guid CreateOrgCusCode(Guid orgHeaderPK, string codeType, string customsRegNo, string countryCode, Guid? premisesAddressPK = null)
		{
			var orgCusCodePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgCusCode(OK_PK, OK_IsValid, OK_OH, OK_CodeType, OK_CustomsRegNo, OK_RN_NKCodeCountry, OK_OA_PremisesAddress)
VALUES(@orgCusCodePK, 1, @orgHeaderPK, @codeType, @customsRegNo, @countryCode, @premisesAddressPK)";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@orgCusCodePK", SqlDbType.UniqueIdentifier, orgCusCodePK);
				command.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.AddParameter("@codeType", SqlDbType.VarChar, OrgCusCodeSchema.OK_CodeType.MaxLength, codeType);
				command.AddParameter("@customsRegNo", SqlDbType.VarChar, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, customsRegNo);
				command.AddParameter("@countryCode", SqlDbType.VarChar, OrgCusCodeSchema.OK_RN_NKCodeCountry.MaxLength, countryCode);
				if (premisesAddressPK != null)
				{
					command.AddParameter("@premisesAddressPK", SqlDbType.UniqueIdentifier, premisesAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@premisesAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				command.ExecuteNonQuery();
			}
			return orgCusCodePK;
		}

		public static Guid CreateOrgCusCode(Guid orgHeaderPK, Guid orgAddressPK, string codeType, string customsRegNo, string countryCode)
		{
			var orgCusCodePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgCusCode(OK_PK, OK_IsValid, OK_OH, OK_OA_PremisesAddress, OK_CodeType, OK_CustomsRegNo, OK_RN_NKCodeCountry)
VALUES(@orgCusCodePK, 1, @orgHeaderPK, @orgAddressPK, @codeType, @customsRegNo, @countryCode)";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@orgCusCodePK", SqlDbType.UniqueIdentifier, orgCusCodePK);
				command.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.AddParameter("@orgAddressPK", SqlDbType.UniqueIdentifier, orgAddressPK);
				command.AddParameter("@codeType", SqlDbType.VarChar, OrgCusCodeSchema.OK_CodeType.MaxLength, codeType);
				command.AddParameter("@customsRegNo", SqlDbType.VarChar, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, customsRegNo);
				command.AddParameter("@countryCode", SqlDbType.VarChar, OrgCusCodeSchema.OK_RN_NKCodeCountry.MaxLength, countryCode);
				command.ExecuteNonQuery();
			}
			return orgCusCodePK;
		}

		public static Guid CreateShipment(string shipmentNumber)
		{
			var shipmentPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES (@shipmentPK, @shipmentNumber, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@shipmentNumber", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, shipmentNumber);
				command.ExecuteNonQuery();
			}
			return shipmentPK;
		}

		public static Guid CreateShipmentWithRatingHeader(string shipmentNumber, Guid oneTimeQuote, bool isConvertedToShipment)
		{
			var shipmentPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_TH_OneTimeQuote, JS_IsBooking, JS_IsForwardRegistered, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
						VALUES (@shipmentPK, @shipmentNumber, @oneTimeQuote, 1, @isForwarding, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@shipmentNumber", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, shipmentNumber);
				command.AddParameter("@oneTimeQuote", SqlDbType.UniqueIdentifier, oneTimeQuote);
				command.AddParameter("@isForwarding", SqlDbType.Bit, isConvertedToShipment);
				command.ExecuteNonQuery();
			}
			return shipmentPK;
		}

		public static Guid CreateJobCartage(Guid branchPK, string consignmentID)
		{
			var cartagePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobCartage (JJ_PK, JJ_GB, JJ_ConsignmentID, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser)
VALUES (@cartagePK, @branchPK, @consignmentID, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cartagePK", SqlDbType.UniqueIdentifier, cartagePK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@consignmentID", SqlDbType.VarChar, consignmentID);
				command.ExecuteNonQuery();
			}

			return cartagePK;
		}

		public static Guid CreateCusAddInfo(string type, string addInfoData, string parentTableCode, Guid parentID)
		{
			var cusAddInfoPK = Guid.NewGuid();
			return CreateCusAddInfo(cusAddInfoPK, type, addInfoData, parentTableCode, parentID);
		}

		public static Guid CreateCusAddInfo(Guid cusAddInfoPK, string type, string addInfoData, string parentTableCode, Guid parentID)
		{
			var sql = @"
INSERT INTO dbo.CusAddInfo (B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID, B7_SystemCreateTimeUtc, B7_SystemCreateUser, B7_SystemLastEditTimeUtc, B7_SystemLastEditUser)
VALUES (@cusAddInfoPK, @type, @addInfoData, @parentTableCode, @parentID, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusAddInfoPK", SqlDbType.UniqueIdentifier, cusAddInfoPK);
				command.AddParameter("@type", SqlDbType.VarChar, CusAddInfoSchema.B7_Type.MaxLength, type);
				command.AddParameter("@addInfoData", SqlDbType.VarChar, CusAddInfoSchema.B7_AddInfoData.MaxLength, addInfoData);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, CusAddInfoSchema.B7_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, CusAddInfoSchema.B7_ParentID.MaxLength, parentID);
				command.ExecuteNonQuery();
			}
			return cusAddInfoPK;
		}

		public static Guid CreateBookingConsolidation(string jobID, string jobType)
		{
			return new DtbBookingConsolidation()
			{
				KB_JobID = jobID,
				KB_JobType = jobType
			}.InsertAndReturnObject(Db.Connection).PK;
		}

		public static Guid CreateBooking(Guid branchPk, Guid consolPk, string jobId, string jobType)
		{
			var bookingPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.DtbBooking (KM_PK, KM_GB_Branch, KM_KB_Booking, KM_JobID, KM_JobType, KM_Status, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser)
VALUES (@bookingPK, @branchPk, @consolPk, @jobId, @jobType, 'AVL', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@bookingPK", SqlDbType.UniqueIdentifier, bookingPK);
				command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPk);
				command.AddParameter("@consolPk", SqlDbType.UniqueIdentifier, consolPk);
				command.AddParameter("@jobId", SqlDbType.VarChar, jobId);
				command.AddParameter("@jobType", SqlDbType.VarChar, jobType);
				command.ExecuteNonQuery();
			}
			return bookingPK;
		}

		public static Guid CreateRefDatabaseRefDataGrouping(string dataGrouping, string description)
		{
			var zzzPk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (@zzzPk, @dataGrouping, @description)
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@zzzPk", SqlDbType.UniqueIdentifier, zzzPk);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, dataGrouping);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.ExecuteNonQuery();
			}
			return zzzPk;
		}

		public static Guid CreateRefDatabaseRefDataGrouping(string dataGrouping, string description, Guid parentPk)
		{
			var zzzPk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
VALUES (@zzzPk, @dataGrouping, @description, @parentPk)
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@zzzPk", SqlDbType.UniqueIdentifier, zzzPk);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, dataGrouping);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@parentPk", SqlDbType.UniqueIdentifier, parentPk);
				command.ExecuteNonQuery();
			}
			return zzzPk;
		}

		public static Guid CreateRefDbEntZZRefCusCodeType(string codeType, string description, string dataGrouping)
		{
			var zzkPK = Guid.NewGuid();
			var sql = @"
				insert into dbo.RefDatabase_RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
				values (@ZZK_PK, @CodeType, @Description, 1,10, @DataGrouping)
			";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ZZK_PK", SqlDbType.UniqueIdentifier, zzkPK);
				command.AddParameter("@CodeType", SqlDbType.VarChar, codeType);
				command.AddParameter("@Description", SqlDbType.VarChar, description);
				command.AddParameter("@DataGrouping", SqlDbType.VarChar, dataGrouping);
				command.ExecuteNonQuery();
			}
			return zzkPK;
		}

		public static void CreateRefDbDataGroupingCodeTypeAndListItems(string dataGrouping, string codeType, params (string Code, string Description)[] codeListItems)
		{
			CreateRefDatabaseRefDataGrouping(dataGrouping, $"{dataGrouping} - Desc");
			CreateRefDbEntZZRefCusCodeType(codeType, $"{codeType} - Desc", dataGrouping);
			Array.ForEach(codeListItems, cli => CreateRefDbEntZZRefCusCodeList(codeType, cli.Code, cli.Description, dataGrouping));
		}

		public static void CreateRefDbDataGroupingCodeTypeAndListAttributeItems(string dataGrouping, string[] codeTypes, params (string Code, string Description, string zzeName, string zzeValue)[] codeListAttributeItems)
		{
			CreateRefDatabaseRefDataGrouping(dataGrouping, $"{dataGrouping} - Desc");
			foreach (string codeType in codeTypes)
			{
				CreateRefDbEntZZRefCusCodeType(codeType, $"{codeType} - Desc", dataGrouping);
			}
			Array.ForEach(codeListAttributeItems, cliAttr => CreateRefDbEntZZRefCusCodeListAttribute(codeTypes, cliAttr.Code, cliAttr.Description, dataGrouping, cliAttr.zzeName, cliAttr.zzeValue));
		}

		static Guid[] CreateRefDbEntZZRefCusCodeListAttribute(string[] codeTypes, string code, string description, string dataGrouping, string zzeName, string zzeValue)
		{
			var zzdPKs = Array.Empty<Guid>();
			foreach (string codeType in codeTypes)
			{
				var zzdPK = CreateRefDbEntZZRefCusCodeList(codeType, code, description, dataGrouping);
				var zzePK = Guid.NewGuid();
				var zxePK = Guid.NewGuid();
				var sql = @"
				INSERT into dbo.RefDatabase_RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
					values (@ZXE_PK, @ZZE_Name, 'Desc.', @CodeType, @DataGrouping);
				insert into dbo.RefDatabase_RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
					values (@ZZE_PK, @ZZD_PK, @ZZE_Name, @ZZE_Value);
			";

				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@ZXE_PK", SqlDbType.UniqueIdentifier, zxePK);
					command.AddParameter("@ZZE_PK", SqlDbType.UniqueIdentifier, zzePK);
					command.AddParameter("@ZZD_PK", SqlDbType.UniqueIdentifier, zzdPK);
					command.AddParameter("@CodeType", SqlDbType.VarChar, codeType);
					command.AddParameter("@DataGrouping", SqlDbType.VarChar, dataGrouping);
					command.AddParameter("@ZZE_Name", SqlDbType.VarChar, zzeName);
					command.AddParameter("@ZZE_Value", SqlDbType.NVarChar, zzeValue);
					command.ExecuteNonQuery();
				}

				zzdPKs.Append(zzdPK);
			}

			return zzdPKs;
		}

		public static Guid CreateRefCusCodeListAttributeName(string attributeName, string codeType, string dataGrouping)
		{
			var sql = @"
				INSERT INTO dbo.RefDatabase_RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
					VALUES(@ZXE_PK, @ZZE_Name, 'Desc.', @CodeType, @DataGrouping);";
			var zxePK = Guid.NewGuid();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ZXE_PK", SqlDbType.UniqueIdentifier, zxePK);
				command.AddParameter("@ZZE_Name", SqlDbType.VarChar, attributeName);
				command.AddParameter("@CodeType", SqlDbType.VarChar, codeType);
				command.AddParameter("@DataGrouping", SqlDbType.VarChar, dataGrouping);
				command.ExecuteNonQuery();
			}
			return zxePK;
		}

		public static Guid CreateRefCusCodeListAttribute(Guid codePK, string zzeName, string zzeValue)
		{
			var sql = @"
				INSERT INTO dbo.RefDatabase_RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
					VALUES(@ZXE_PK, @ZZD_PK, @ZZE_Name, @ZZE_Value);";
			var zzePK = Guid.NewGuid();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ZXE_PK", SqlDbType.UniqueIdentifier, zzePK);
				command.AddParameter("@ZZD_PK", SqlDbType.UniqueIdentifier, codePK);
				command.AddParameter("@ZZE_Name", SqlDbType.VarChar, zzeName);
				command.AddParameter("@ZZE_Value", SqlDbType.NVarChar, zzeValue);
				command.ExecuteNonQuery();
			}
			return zzePK;
		}

		public static Guid CreateRefDbEntZZRefCusCodeList(string codeType, string code, string description, string dataGrouping, string startDate = "1900-01-01", string endDate = "2079-06-06")
		{
			var zzdPK = Guid.NewGuid();
			var sql = @"
				insert into dbo.RefDatabase_RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
				values (@ZZD_PK, @CodeType, @Code, @Description, @startDate, @endDate, @DataGrouping)
			";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ZZD_PK", SqlDbType.UniqueIdentifier, zzdPK);
				command.AddParameter("@CodeType", SqlDbType.VarChar, codeType);
				command.AddParameter("@Code", SqlDbType.VarChar, code);
				command.AddParameter("@Description", SqlDbType.VarChar, description);
				command.AddParameter("@DataGrouping", SqlDbType.VarChar, dataGrouping);
				command.AddParameter("@startDate", SqlDbType.SmallDateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.SmallDateTime, endDate);
				command.ExecuteNonQuery();
			}
			return zzdPK;
		}

		public static Guid CreateRefDbEntZZRefCusRateType(string rateType, string description, string grouping, bool isPayable = true, string customsValueFormula = "")
		{
			var typePk = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusRateType(
		[ZZR_PK],
		[ZZR_RateType],
		[ZZR_Description],
		[ZZR_IsPayable],
		[ZZR_ZZZ_NKDataGrouping],

		[ZZR_CustomsValueFormula]
	) VALUES (
		@typePk,
		@rateType,
		@description,
		1,
		@dataGrouping,

		@CustomsValueFormula
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@typePk", SqlDbType.UniqueIdentifier, typePk);
				command.AddParameter("@rateType", SqlDbType.VarChar, rateType);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@isPayable", SqlDbType.Bit, isPayable);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, grouping);

				command.AddParameter("@CustomsValueFormula", SqlDbType.VarChar, customsValueFormula);
				command.ExecuteNonQuery();
			}
			return typePk;
		}

		public static Guid CreateRefDbEntZZRefCusTariffType(string tariffType, string description, string groupType, string grouping)
		{
			var typePk = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusTariffType(
			[ZZI_PK]
		   ,[ZZI_TariffType]
		   ,[ZZI_Description]
		   ,[ZZI_ZZ9_NKNomenclatureGroupType]
		   ,[ZZI_ZZZ_NKDataGrouping]
	) VALUES (
		@typePk,
		@tariffType,
		@description,
		@groupType,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@typePk", SqlDbType.UniqueIdentifier, typePk);
				command.AddParameter("@tariffType", SqlDbType.VarChar, tariffType);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@groupType", SqlDbType.VarChar, groupType);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, grouping);
				command.ExecuteNonQuery();
			}
			return typePk;
		}

		public static void CreateTariffDataForTesting()
		{
			var sql = @"DECLARE @rateTypeDTYPK UNIQUEIDENTIFIER, @rateTypeEX1PK UNIQUEIDENTIFIER, @rateTypeLVYPK UNIQUEIDENTIFIER, @rateTypeREFPK UNIQUEIDENTIFIER, @1P1PK UNIQUEIDENTIFIER, @12BPK UNIQUEIDENTIFIER, @13BPK UNIQUEIDENTIFIER
SET @rateTypeDTYPK = NEWID()
SET @rateTypeEX1PK = NEWID()
SET @rateTypeLVYPK = NEWID()
SET @rateTypeREFPK = NEWID()

SET @1P1PK = NEWID()
SET @12BPK = NEWID()
SET @13BPK = NEWID()

IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES('595F5657-9AC9-4381-B990-F37D1940FCBE', 'ZA', 'South Africa', NULL)

IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'DTY')
INSERT RefDatabase_RefCusRateType (ZZR_PK,	ZZR_RateType,	ZZR_Description,	ZZR_IsPayable,	ZZR_ZZZ_NKDataGrouping,	ZZR_CustomsValueFormula) VALUES(@rateTypeDTYPK, 'DTY', 'Duty', 1, 'ZA', 'CV')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'REF')
INSERT RefDatabase_RefCusRateType (ZZR_PK,	ZZR_RateType,	ZZR_Description,	ZZR_IsPayable,	ZZR_ZZZ_NKDataGrouping,	ZZR_CustomsValueFormula) VALUES(@rateTypeREFPK, 'REF', 'Refund', 0, 'ZA', 'CV')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'EX1')
INSERT RefDatabase_RefCusRateType (ZZR_PK,	ZZR_RateType,	ZZR_Description,	ZZR_IsPayable,	ZZR_ZZZ_NKDataGrouping,	ZZR_CustomsValueFormula) VALUES(@rateTypeEX1PK, 'EX1', 'Ad Valorem Excise',1,'ZA','(CV * 1.15) + 1P1 + 2P1 + 2P2 + 2P3 - 3P1 - 3P2 - 4P1 - 4P2 - 4P3 - 4P4 - 4P5 - 4P6')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'LVY')
INSERT RefDatabase_RefCusRateType (ZZR_PK,	ZZR_RateType,	ZZR_Description,	ZZR_IsPayable,	ZZR_ZZZ_NKDataGrouping,	ZZR_CustomsValueFormula) VALUES(@rateTypeLVYPK, 'LVY', 'Levy', 1, 'ZA', '')

IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = '1P1')
INSERT RefDatabase_RefCusRateCode (ZY1_PK,	ZY1_RateCode,	ZY1_ZZR_RateType,	ZY1_Description) VALUES(newid(), '1P1', @rateTypeDTYPK,'Sched 1 Part 1')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = '12B')
INSERT RefDatabase_RefCusRateCode (ZY1_PK,	ZY1_RateCode,	ZY1_ZZR_RateType,	ZY1_Description) VALUES(newid(), '12B', @rateTypeEX1PK,'Sched 12B')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = '13B')
INSERT RefDatabase_RefCusRateCode (ZY1_PK,	ZY1_RateCode,	ZY1_ZZR_RateType,	ZY1_Description) VALUES(newid(), '13B', @rateTypeLVYPK,'Sched 13B')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = 'A00')
INSERT RefDatabase_RefCusRateCode (ZY1_PK,	ZY1_RateCode,	ZY1_ZZR_RateType,	ZY1_Description) VALUES(newid(), 'A00', @rateTypeDTYPK, 'A00')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = 'A20')
INSERT RefDatabase_RefCusRateCode (ZY1_PK,	ZY1_RateCode,	ZY1_ZZR_RateType,	ZY1_Description) VALUES(newid(), 'A20', @rateTypeDTYPK, 'A20')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = '990')
INSERT RefDatabase_RefCusRateCode (ZY1_PK,	ZY1_RateCode,	ZY1_ZZR_RateType,	ZY1_Description) VALUES(newid(), '990', @rateTypeLVYPK, '990')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = '3P1')
INSERT RefDatabase_RefCusRateCode (ZY1_PK,	ZY1_RateCode,	ZY1_ZZR_RateType,	ZY1_Description) VALUES(newid(), '3P1', @rateTypeREFPK, 'Schedule 3 Part 1')

IF NOT EXISTS(Select 1 from RefDatabase_RefCusTariffType where ZZI_TariffType = '1P1')
INSERT RefDatabase_RefCusTariffType(ZZI_PK,	ZZI_TariffType,	ZZI_Description,	ZZI_ZZZ_NKDataGrouping) VALUES(@1P1PK,	'1P1',	'Sched 1 Part 1',	'ZA')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusTariffType where ZZI_TariffType = '12B')
INSERT RefDatabase_RefCusTariffType(ZZI_PK,	ZZI_TariffType,	ZZI_Description,	ZZI_ZZZ_NKDataGrouping) VALUES(@12BPK,	'12B',	'Sched 12B',	'ZA')
IF NOT EXISTS(Select 1 from RefDatabase_RefCusTariffType where ZZI_TariffType = '13B')
INSERT RefDatabase_RefCusTariffType(ZZI_PK,	ZZI_TariffType,	ZZI_Description,	ZZI_ZZZ_NKDataGrouping) VALUES(@13BPK,	'13B',	'Sched 13B',	'ZA')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateRefDbEntZZRefCusTariff(Guid tariffTypePK, string tariffCode, string description, string grouping, DateTime startDate, DateTime endDate)
		{
			var tariffPk = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusTariff(
			[ZZ1_PK]
		   ,[ZZ1_ZZI_TariffType]
		   ,[ZZ1_TariffCode]
		   ,[ZZ1_Description]
		   ,[ZZ1_StartDate]
		   ,[ZZ1_EndDate]
		   ,[ZZ1_ZZZ_NKDataGrouping]
		   ,[ZZ1_ZZF_NKTaxOrFeeCode]
	) VALUES (
		@tariffPk,
		@tariffTypePK,
		@tariffCode,
		@description,
		@startDate,
		@endDate,
		@dataGrouping,
		''
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@tariffPk", SqlDbType.UniqueIdentifier, tariffPk);
				command.AddParameter("@tariffTypePK", SqlDbType.UniqueIdentifier, tariffTypePK);
				command.AddParameter("@tariffCode", SqlDbType.VarChar, tariffCode);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, grouping);
				command.ExecuteNonQuery();
			}
			return tariffPk;
		}

		public static Guid CreateRefDbEntZZRefCusTradeGroup(string tradeGroup, string description, string grouping, DateTime startDate, DateTime endDate)
		{
			var tradeGroupPK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusTradeGroup(
			[ZZA_PK]
		   ,[ZZA_TradeGroup]
		   ,[ZZA_Description]
		   ,[ZZA_StartDate]
		   ,[ZZA_EndDate]
		   ,[ZZA_ZZZ_NKDataGrouping]
	) VALUES (
		@tradeGroupPK,
		@tradeGroup,
		@description,
		@startDate,
		@endDate,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@tradeGroupPK", SqlDbType.UniqueIdentifier, tradeGroupPK);
				command.AddParameter("@tradeGroup", SqlDbType.VarChar, tradeGroup);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, grouping);
				command.ExecuteNonQuery();
			}
			return tradeGroupPK;
		}

		public static Guid CreateRefDbEntZZRefCusConditionType(string conditionClass, string conditionType, string description, string grouping)
		{
			var conditionTypePK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusConditionType(
			[ZX2_PK]
		   ,[ZX2_ConditionClass]
		   ,[ZX2_ConditionType]
		   ,[ZX2_Description]
		   ,[ZX2_ZZZ_NKDataGrouping]
	) VALUES (
		@conditionTypePK,
		@conditionClass,
		@conditionType,
		@description,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@conditionTypePK", SqlDbType.UniqueIdentifier, conditionTypePK);
				command.AddParameter("@conditionClass", SqlDbType.VarChar, conditionClass);
				command.AddParameter("@conditionType", SqlDbType.VarChar, conditionType);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, grouping);
				command.ExecuteNonQuery();
			}
			return conditionTypePK;
		}

		public static Guid CreateRefDbEntZZRefCusTradeGroupCountry(Guid tradeGroupPK, string tradeGroupCountry, string description, DateTime startDate, DateTime endDate)
		{
			var tradeGroupCountryPK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusTradeGroupCountry(
			[ZZB_PK]
		   ,[ZZB_ZZA_TradeGroup]
		   ,[ZZB_Description]
		   ,[ZZB_StartDate]
		   ,[ZZB_EndDate]
		   ,[ZZB_RN_NKTradeGroupCountryCode]
	) VALUES (
		@tradeGroupCountryPK,
		@tradeGroupPK,
		@description,
		@startDate,
		@endDate,
		@tradeGroupCountry
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@tradeGroupCountryPK", SqlDbType.UniqueIdentifier, tradeGroupCountryPK);
				command.AddParameter("@tradeGroupPK", SqlDbType.UniqueIdentifier, tradeGroupPK);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.AddParameter("@tradeGroupCountry", SqlDbType.VarChar, tradeGroupCountry);
				command.ExecuteNonQuery();
			}
			return tradeGroupCountryPK;
		}

		public static Guid CreateRefDbEntZZRefCusPreference(string preference, string description, string dataGrouping)
		{
			var preferencePK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusPreference(
			[ZZS_PK]
		   ,[ZZS_Preference]
		   ,[ZZS_Description]
		   ,[ZZS_ZZZ_NKDataGrouping]
	) VALUES (
		@preferencePK,
		@preference,
		@description,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@preferencePK", SqlDbType.UniqueIdentifier, preferencePK);
				command.AddParameter("@preference", SqlDbType.VarChar, preference);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, dataGrouping);
				command.ExecuteNonQuery();
			}
			return preferencePK;
		}

		public static Guid CreateRefDbEntZZRefCusCondition(Guid conditionTypePK, Guid tariffPK, DateTime startDate, DateTime endDate, int isImport, int isExport, Guid preferencePK, string grouping)
		{
			var conditionPK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusCondition(
			[ZX1_PK]
		   ,[ZX1_ZX2_ConditionType]
		   ,[ZX1_ZZ1_Tariff]
		   ,[ZX1_StartDate]
		   ,[ZX1_EndDate]
		   ,[ZX1_IsImport]
		   ,[ZX1_IsExport]
		   ,[ZX1_ZZS_Preference]
		   ,[ZX1_ZZZ_NKDataGrouping]
	) VALUES (
		@conditionPK,
		@conditionTypePK,
		@tariffPK,
		@startDate,
		@endDate,
		@isImport,
		@isExport,
		@preferencePK,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@conditionPK", SqlDbType.UniqueIdentifier, conditionPK);
				command.AddParameter("@conditionTypePK", SqlDbType.UniqueIdentifier, conditionTypePK);
				command.AddParameter("@tariffPK", SqlDbType.UniqueIdentifier, tariffPK == Guid.Empty ? DBNull.Value : tariffPK);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.AddParameter("@isImport", SqlDbType.VarChar, isImport);
				command.AddParameter("@isExport", SqlDbType.VarChar, isExport);
				command.AddParameter("@preferencePK", SqlDbType.UniqueIdentifier, preferencePK == Guid.Empty ? DBNull.Value : preferencePK);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, grouping);
				command.ExecuteNonQuery();
			}
			return conditionPK;
		}

		public static Guid CreateRefDbEntZZRefCusApplicability(Guid ratePK, Guid conditionsPK, DateTime startDate, DateTime endDate, Guid tradeGroupPK, string additionalCode, string orderNumber, Guid secondTradeGroupPK)
		{
			var applicabilityPK = Guid.NewGuid();
			const string sql = @"
	INSERT INTO dbo.RefDatabase_RefCusApplicability(
			[ZZT_PK]
		   ,[ZZT_ZZ2_Rate]
		   ,[ZZT_ZX1_Conditions]
		   ,[ZZT_StartDate]
		   ,[ZZT_EndDate]
		   ,[ZZT_ZZA_TradeGroup]
		   ,[ZZT_AdditionalCode]
		   ,[ZZT_OrderNumber]
		   ,[ZZT_ZZA_SecondTradeGroup]
	) VALUES (
		@applicabilityPK,
		@ratePK,
		@conditionsPK,
		@startDate,
		@endDate,
		@tradeGroupPK,
		@additionalCode,
		@orderNumber,
		@secondTradeGroup
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@applicabilityPK", SqlDbType.UniqueIdentifier, applicabilityPK);
				command.AddParameter("@ratePK", SqlDbType.UniqueIdentifier, ratePK == Guid.Empty ? DBNull.Value : ratePK);
				command.AddParameter("@conditionsPK", SqlDbType.UniqueIdentifier, conditionsPK == Guid.Empty ? DBNull.Value : conditionsPK);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.AddParameter("@tradeGroupPK", SqlDbType.UniqueIdentifier, tradeGroupPK);
				command.AddParameter("@additionalCode", SqlDbType.VarChar, additionalCode);
				command.AddParameter("@orderNumber", SqlDbType.VarChar, orderNumber);
				command.AddParameter("@secondTradeGroup", SqlDbType.UniqueIdentifier, secondTradeGroupPK == Guid.Empty ? DBNull.Value : secondTradeGroupPK);
				command.ExecuteNonQuery();
			}
			return applicabilityPK;
		}

		public static Guid CreateRefDbEntZZRefCusApplicability(Guid ratePK, Guid conditionsPK, Guid tariffAdditionalCodePK, DateTime startDate, DateTime endDate, Guid tradeGroupPK, string additionalCode, string orderNumber, Guid secondTradeGroupPK)
		{
			var applicabilityPK = Guid.NewGuid();
			const string sql = @"
	INSERT INTO dbo.RefDatabase_RefCusApplicability(
			[ZZT_PK]
		   ,[ZZT_ZZ2_Rate]
		   ,[ZZT_ZX1_Conditions]
		   ,[ZZT_ZY2_AdditionalCode]
		   ,[ZZT_StartDate]
		   ,[ZZT_EndDate]
		   ,[ZZT_ZZA_TradeGroup]
		   ,[ZZT_AdditionalCode]
		   ,[ZZT_OrderNumber]
		   ,[ZZT_ZZA_SecondTradeGroup]
	) VALUES (
		@applicabilityPK,
		@ratePK,
		@conditionsPK,
		@tariffAdditionalCodePK,
		@startDate,
		@endDate,
		@tradeGroupPK,
		@additionalCode,
		@orderNumber,
		@secondTradeGroup
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@applicabilityPK", SqlDbType.UniqueIdentifier, applicabilityPK);
				command.AddParameter("@ratePK", SqlDbType.UniqueIdentifier, ratePK == Guid.Empty ? DBNull.Value : ratePK);
				command.AddParameter("@conditionsPK", SqlDbType.UniqueIdentifier, conditionsPK == Guid.Empty ? DBNull.Value : conditionsPK);
				command.AddParameter("@tariffAdditionalCodePK", SqlDbType.UniqueIdentifier, tariffAdditionalCodePK == Guid.Empty ? DBNull.Value : tariffAdditionalCodePK);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.AddParameter("@tradeGroupPK", SqlDbType.UniqueIdentifier, tradeGroupPK);
				command.AddParameter("@additionalCode", SqlDbType.VarChar, additionalCode);
				command.AddParameter("@orderNumber", SqlDbType.VarChar, orderNumber);
				command.AddParameter("@secondTradeGroup", SqlDbType.UniqueIdentifier, secondTradeGroupPK == Guid.Empty ? DBNull.Value : secondTradeGroupPK);
				command.ExecuteNonQuery();
			}
			return applicabilityPK;
		}

		public static Guid CreateRefDbEntZZRefCusTariffAdditionalCodeCategory(string category, string description, string dataGrouping)
		{
			var categoryPK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusTariffAdditionalCodeCategory
	(
			[ZY3_PK]
		   ,[ZY3_Category]
		   ,[ZY3_Description]
		   ,[ZY3_ZZZ_NKDataGrouping]
	) VALUES (
		@categoryPK,
		@category,
		@description,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@categoryPK", SqlDbType.UniqueIdentifier, categoryPK);
				command.AddParameter("@category", SqlDbType.VarChar, category);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, dataGrouping);
				command.ExecuteNonQuery();
			}
			return categoryPK;
		}

		public static Guid CreateRefDbEntZZRefCusTariffAdditionalCode(Guid tariffPK, Guid nationalCodePK, string additionalCode, string description, string category, bool isMandatory, string dataGrouping)
		{
			var tariffAdditionalCodePK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusTariffAdditionalCode
	(
			[ZY2_PK]
		   ,[ZY2_ZZ1_Tariff]
		   ,[ZY2_ZZW_NationalCode]
		   ,[ZY2_AdditionalCode]
		   ,[ZY2_Description]
		   ,[ZY2_ZY3_NKCategory]
		   ,[ZY2_IsMandatory]
		   ,[ZY2_ZZZ_NKDataGrouping]
	) VALUES (
		@tariffAdditionalCodePK,
		@tariffPK,
		@nationalCodePK,
		@additionalCode,
		@description,
		@category,
		@isMandatory,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@tariffAdditionalCodePK", SqlDbType.UniqueIdentifier, tariffAdditionalCodePK);
				command.AddParameter("@tariffPK", SqlDbType.UniqueIdentifier, tariffPK == Guid.Empty ? DBNull.Value : tariffPK);
				command.AddParameter("@nationalCodePK", SqlDbType.UniqueIdentifier, nationalCodePK == Guid.Empty ? DBNull.Value : nationalCodePK);
				command.AddParameter("@additionalCode", SqlDbType.VarChar, additionalCode);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@category", SqlDbType.VarChar, category);
				command.AddParameter("@isMandatory", SqlDbType.Bit, isMandatory);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, dataGrouping);
				command.ExecuteNonQuery();
			}
			return tariffAdditionalCodePK;
		}

		public static Guid CreateRefDbEntZZRefCusTariffNationalCode(Guid tariffPK, string nationalCode, string description, string taxOrFeeCode, DateTime startDate, DateTime endDate, DateTime publishedDate, string dataGrouping)
		{
			var tariffNationalCodePK = Guid.NewGuid();
			var sql = @"
	INSERT INTO dbo.RefDatabase_RefCusTariffNationalCode(
			[ZZW_PK]
		   ,[ZZW_ZZ1_Tariff]
		   ,[ZZW_NationalCode]
		   ,[ZZW_Description]
		   ,[ZZW_ZZF_NKTaxOrFeeCode]
		   ,[ZZW_StartDate]
		   ,[ZZW_EndDate]
		   ,[ZZW_PublishedDate]
		   ,[ZZW_ZZZ_NKDataGrouping]
	) VALUES (
		@tariffNationalCodePK,
		@tariffPK,
		@nationalCode,
		@description
		@taxOrFeeCode,
		@startDate,
		@endDate,
		@publishedDate,
		@dataGrouping
	)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@tariffNationalCodePK", SqlDbType.UniqueIdentifier, tariffNationalCodePK);
				command.AddParameter("@tariffPK", SqlDbType.UniqueIdentifier, tariffPK);
				command.AddParameter("@nationalCode", SqlDbType.VarChar, nationalCode);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@taxOrFeeCode", SqlDbType.VarChar, taxOrFeeCode);
				command.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				command.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				command.AddParameter("@publishedDate", SqlDbType.DateTime, publishedDate);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, dataGrouping);
				command.ExecuteNonQuery();
			}
			return tariffNationalCodePK;
		}

		public static Guid CreateGlbDevice()
		{
			var devicePK = Guid.NewGuid();
			var sql = @"
	INSERT INTO [dbo].[GlbDevice]
		   ([V3_PK], [V3_Model], [V3_SystemCreateTimeUtc], [V3_SystemCreateUser], [V3_MobileServicesIdentifier], [V3_HumanReadableIdentifier], [V3_SystemLastEditTimeUtc], [V3_SystemLastEditUser])
	 VALUES
		   (@devicePK , 'Test Device',  @createdTime, '~BP', @mobileServicesID, 'Test Device', GetUtcDate(), '~BP')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@devicePK", SqlDbType.UniqueIdentifier, devicePK);
				command.AddParameter("@createdTime", SqlDbType.DateTime, DateTime.UtcNow);
				command.AddParameter("@mobileServicesID", SqlDbType.VarBinary, Guid.NewGuid().ToByteArray());
				command.ExecuteNonQuery();
			}
			return devicePK;
		}

		public static Guid CreateGlbDeviceWithHardwareId(string hardwareId)
		{
			var devicePK = CreateGlbDevice();
			var sql = $@"UPDATE [dbo].[GlbDevice] SET V3_HardwareIdentifier = @hardwareId, V3_SystemLastEditUser = 'E', V3_SystemLastEditTimeUtc = GetDate() WHERE V3_PK = @devicePK";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId);
				command.AddParameter("@devicePK", SqlDbType.UniqueIdentifier, devicePK);
				command.ExecuteNonQuery();
			}
			return devicePK;
		}

		public static Guid CreateRefEquipment()
		{
			var refEquipmentPK = Guid.NewGuid();
			var sql = @"
	INSERT INTO [dbo].[RefEquipment]
		   ([RQ_PK],[RQ_Description], [RQ_ShortCode], [RQ_Registration])
	 VALUES
		   (@refEquipmentPK ,'Test Equipmment', 'TD-01', 'TDX01')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@refEquipmentPK", SqlDbType.UniqueIdentifier, refEquipmentPK);
				command.ExecuteNonQuery();
			}
			return refEquipmentPK;
		}

		public static void CreateLocations(Guid devicePK)
		{
			var sql = $@"
INSERT dbo.GlbDeviceLocation (V2_PK, V2_V3_Device, V2_MeasurementTimeUtc, V2_Location, V2_Speedkmh, V2_CompassHeadingDegrees, V2_SpeedLimitState, V2_SpeedLimitKmh)
VALUES
('00000001-0000-0000-0000-000000000000', @devicePK, '2015-05-01 08:59:59', geography::Point(50.000001, 100.2, 4326), 45, 24, 'U', 123),
('00000002-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:00:00', geography::Point(50.1, 100.2, 4326), 50, 25, 'U', 124),
('00000003-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:02:15', geography::Point(50.100135, 100.200004, 4326), 20, 30, 'S', 125),
('00000004-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:03:04', geography::Point(50.1002, 100.200001, 4326), 2.1, 32, 'U', 0),
('00000005-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:03:55', geography::Point(50.1003347, 100.2001, 4326), 2, 32.5, 'U', 0),
('00000006-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:04:30', geography::Point(50.1004654, 100.2001, 4326), 2, 5, 'U', 0),
('00000007-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:05:15', geography::Point(50.1002001, 100.2001, 4326), 0.3, 7, 'U', 0),
('00000008-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:08:20', geography::Point(50.1001999, 100.2001, 4326), 1.9, 10, 'U', 0),
('00000009-0000-0000-0000-000000000000', @devicePK, '2015-05-01 09:08:21', geography::Point(50.100198, 100.20015, 4326), 11.3, 10, 'U', 0);";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@devicePK", SqlDbType.UniqueIdentifier, devicePK);
				command.ExecuteNonQuery();
			}
		}

		public static void CreateAssignmentDivot(Guid devicePK, Guid refEquipmentPK, DateTime? endDateTime = null)
		{
			string sql = $@"INSERT INTO dbo.GlbDeviceAssignmentDivot (V7_PK, V7_ParentTableCode, V7_ParentID, V7_V3_Device, V7_StartTimeUtc, V7_EndTimeUtc, V7_SystemCreateTimeUtc, V7_SystemCreateUser, V7_SystemLastEditTimeUtc, V7_SystemLastEditUser)
VALUES";
			if (endDateTime.HasValue)
			{
				sql += $@" (NEWID(), 'RQ', @refEquipmentPK , @devicePK, '2015-05-01 09:05:00', @endDateTime, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";
			}
			else
			{
				sql += $@" (NEWID(), 'RQ', @refEquipmentPK , @devicePK, '2015-05-01 09:05:00', NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";
			}
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@refEquipmentPK", SqlDbType.UniqueIdentifier, refEquipmentPK);
				command.AddParameter("@devicePK", SqlDbType.UniqueIdentifier, devicePK);
				if (endDateTime.HasValue)
				{
					command.AddParameter("@endDateTime", SqlDbType.DateTime, endDateTime.Value);
				}
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateRefDbEntZZRefCarrier(string code, string description, string dataGrouping, string attribute = null)
		{
			var zz4Pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.RefDatabase_RefCarrierCode(ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_ZZZ_NKDataGrouping)
VALUES (@zz4Pk, @zz4Code, @zz4Description, @dataGrouping)
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@zz4Pk", SqlDbType.UniqueIdentifier, zz4Pk);
				command.AddParameter("@zz4Code", SqlDbType.VarChar, code);
				command.AddParameter("@zz4Description", SqlDbType.VarChar, description);
				command.AddParameter("@dataGrouping", SqlDbType.VarChar, dataGrouping);
				command.ExecuteNonQuery();
			}

			if (attribute != null)
			{
				var attrsql = @"
INSERT INTO dbo.RefDatabase_RefCarrierCodeAttribute(ZZG_PK, ZZG_ZZ4_CarrierCode, ZZG_Name, ZZG_Value)
VALUES (@zzgPk, @zz4pk, @zzgName, @zzgValue)
";

				using (var command = Db.Connection.Command(attrsql))
				{
					command.AddParameter("@zzgPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
					command.AddParameter("@zz4pk", SqlDbType.UniqueIdentifier, zz4Pk);
					command.AddParameter("@zzgName", SqlDbType.VarChar, attribute);
					command.AddParameter("@zzgValue", SqlDbType.VarChar, attribute);
					command.ExecuteNonQuery();
				}
			}

			return zz4Pk;
		}

		public static Guid CreateCusTempStorageJobHeader(Guid branchPK, string jobReference, string referenceNumber, Guid customer, DateTime createTimeUTC)
		{
			var jobHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageJobHeader
(SJH_PK, SJH_GB, SJH_JobReference, SJH_ReferenceNumber, SJH_OH_Customer, SJH_SystemCreateTimeUtc, SJH_SystemCreateUser, SJH_SystemLastEditTimeUtc, SJH_SystemLastEditUser)
VALUES
(@SJH_PK, @SJH_GB, @SJH_JobReference, @SJH_ReferenceNumber, @SJH_OH_Customer, @SJH_SystemCreateTimeUtc, @SJH_SystemCreateUser, @SJH_SystemCreateTimeUtc, @SJH_SystemCreateUser)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@SJH_PK", SqlDbType.UniqueIdentifier, jobHeaderPK);
				command.AddParameter("@SJH_GB", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@SJH_JobReference", SqlDbType.VarChar, CusTempStorageJobHeaderSchema.SJH_JobReference.MaxLength, jobReference);
				command.AddParameter("@SJH_ReferenceNumber", SqlDbType.VarChar, CusTempStorageJobHeaderSchema.SJH_ReferenceNumber.MaxLength, referenceNumber);
				command.AddParameter("@SJH_OH_Customer", SqlDbType.UniqueIdentifier, customer);
				command.AddParameter("@SJH_SystemCreateTimeUtc", SqlDbType.DateTime, createTimeUTC);
				command.AddParameter("@SJH_SystemCreateUser", SqlDbType.VarChar, CusTempStorageJobHeaderSchema.SJH_SystemCreateUser.MaxLength, "~BP");
				command.ExecuteNonQuery();
			}
			return jobHeaderPK;
		}

		public static Guid CreateCusTempStorageDec(Guid jobHeaderPK, DateTime createTimeUTC)
		{
			var decPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageDec
(STH_PK, STH_SJH, STH_DeclarationType, STH_SystemCreateTimeUtc, STH_SystemCreateUser, STH_SystemLastEditTimeUtc, STH_SystemLastEditUser)
VALUES
(@STH_PK, @STH_SJH, @STH_DeclarationType, @STH_SystemCreateTimeUtc, @STH_SystemCreateUser, @STH_SystemCreateTimeUtc, @STH_SystemCreateUser)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@STH_PK", SqlDbType.UniqueIdentifier, decPK);
				command.AddParameter("@STH_SJH", SqlDbType.UniqueIdentifier, jobHeaderPK);
				command.AddParameter("@STH_DeclarationType", SqlDbType.VarChar, CusTempStorageDecSchema.STH_DeclarationType.MaxLength, "XXX");
				command.AddParameter("@STH_SystemCreateTimeUtc", SqlDbType.DateTime, createTimeUTC);
				command.AddParameter("@STH_SystemCreateUser", SqlDbType.VarChar, CusTempStorageDecSchema.STH_SystemCreateUser.MaxLength, "~BP");
				command.ExecuteNonQuery();
			}
			return decPK;
		}

		public static Guid CreateCusTempStorageLine(Guid storageDecPK, int lineNo, decimal grossWeight, int packageQty, string referenceNumberType, int referenceNumberLine, int referenceNumber2Line, DateTime createTimeUTC)
		{
			var storageLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageLine
(TSL_PK, TSL_STH, TSL_LineNo, TSL_GrossWeight, TSL_PackageQty, TSL_ReferenceNumberType, TSL_ReferenceNumberLine, TSL_ReferenceNumber2Line, TSL_SystemCreateTimeUtc, TSL_SystemCreateUser, TSL_SystemLastEditTimeUtc, TSL_SystemLastEditUser)
VALUES
(@TSL_PK, @TSL_STH, @TSL_LineNo, @TSL_GrossWeight, @TSL_PackageQty, @TSL_ReferenceNumberType, @TSL_ReferenceNumberLine, @TSL_ReferenceNumber2Line, @TSL_SystemCreateTimeUtc, '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@TSL_PK", SqlDbType.UniqueIdentifier, storageLinePK);
				command.AddParameter("@TSL_STH", SqlDbType.UniqueIdentifier, storageDecPK);
				command.AddParameter("@TSL_LineNo", SqlDbType.Int, lineNo);
				command.AddParameter("@TSL_GrossWeight", SqlDbType.Decimal, grossWeight);
				command.AddParameter("@TSL_PackageQty", SqlDbType.Int, packageQty);
				command.AddParameter("@TSL_ReferenceNumberType", SqlDbType.VarChar, referenceNumberType);
				command.AddParameter("@TSL_ReferenceNumberLine", SqlDbType.Int, referenceNumberLine);
				command.AddParameter("@TSL_ReferenceNumber2Line", SqlDbType.Int, referenceNumber2Line);
				command.AddParameter("@TSL_SystemCreateTimeUtc", SqlDbType.DateTime, createTimeUTC);
				command.ExecuteNonQuery();
			}
			return storageLinePK;
		}

		public static Guid CreateCusTempStorageLineItem(Guid storageLinePK, DateTime createTimeUTC)
		{
			var storageLineItemPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageLineItem
(TSI_PK, TSI_TSL, TSI_SystemCreateTimeUtc, TSI_SystemCreateUser, TSI_SystemLastEditTimeUtc, TSI_SystemLastEditUser)
VALUES
(@TSI_PK, @TSI_TSL, @TSI_SystemCreateTimeUtc, '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@TSI_PK", SqlDbType.UniqueIdentifier, storageLineItemPK);
				command.AddParameter("@TSI_TSL", SqlDbType.UniqueIdentifier, storageLinePK);
				command.AddParameter("@TSI_SystemCreateTimeUtc", SqlDbType.DateTime, createTimeUTC);
				command.ExecuteNonQuery();
			}
			return storageLineItemPK;
		}

		public static Guid CreateCusTempStorageRegHeader(string reference, string appCode, DateTime createTimeUTC, Guid? premisesPK = null)
		{
			var pk = Guid.NewGuid();

			if (premisesPK == null)
			{
				var sql = @"
INSERT INTO dbo.CusTempStorageRegHeader
(SRH_PK, SRH_Reference, SRH_AppCode, SRH_SystemCreateTimeUTC, SRH_SystemCreateUser, SRH_SystemLastEditTimeUtc, SRH_SystemLastEditUser)
VALUES
(@SRH_PK, @SRH_Reference, @SRH_AppCode, @SRH_SystemCreateTimeUTC, @SRH_SystemCreateUser, GetUtcDate(), '~BP')
";
				using (DbCommand command = Db.Connection.Command(sql))
				{
					command.AddParameter("@SRH_PK", SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@SRH_Reference", SqlDbType.VarChar, CusTempStorageRegHeaderSchema.SRH_Reference.MaxLength, reference);
					command.AddParameter("@SRH_AppCode", SqlDbType.VarChar, CusTempStorageRegHeaderSchema.SRH_AppCode.MaxLength, appCode);
					command.AddParameter("@SRH_SystemCreateTimeUTC", SqlDbType.DateTime, createTimeUTC);
					command.AddParameter("@SRH_SystemCreateUser", SqlDbType.VarChar, CusTempStorageRegHeaderSchema.SRH_SystemCreateUser.MaxLength, "~BP");
					command.ExecuteNonQuery();
				}
			}
			else
			{
				var sql = @"
INSERT INTO dbo.CusTempStorageRegHeader
(SRH_PK, SRH_Reference, SRH_AppCode, SRH_SRP_Premises, SRH_SystemCreateTimeUTC, SRH_SystemCreateUser, SRH_SystemLastEditTimeUtc, SRH_SystemLastEditUser)
VALUES
(@SRH_PK, @SRH_Reference, @SRH_AppCode, @SRH_SRP_Premises, @SRH_SystemCreateTimeUTC, @SRH_SystemCreateUser, GetUtcDate(), '~BP')
";
				using (DbCommand command = Db.Connection.Command(sql))
				{
					command.AddParameter("@SRH_PK", SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@SRH_Reference", SqlDbType.VarChar, CusTempStorageRegHeaderSchema.SRH_Reference.MaxLength, reference);
					command.AddParameter("@SRH_AppCode", SqlDbType.VarChar, CusTempStorageRegHeaderSchema.SRH_AppCode.MaxLength, appCode);
					command.AddParameter("@SRH_SRP_Premises", SqlDbType.UniqueIdentifier, premisesPK);
					command.AddParameter("@SRH_SystemCreateTimeUTC", SqlDbType.DateTime, createTimeUTC);
					command.AddParameter("@SRH_SystemCreateUser", SqlDbType.VarChar, CusTempStorageRegHeaderSchema.SRH_SystemCreateUser.MaxLength, "~BP");
					command.ExecuteNonQuery();
				}
			}

			return pk;
		}

		public static Guid CreateCusTempStorageRegLine(Guid regHeaderPK, int lineNumber, int packagesRemaining = 0)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageRegLine
(SRL_PK, SRL_SRH, SRL_LineNumber, SRL_PackagesRemaining, SRL_SystemCreateTimeUTC, SRL_SystemCreateUser, SRL_SystemLastEditTimeUtc, SRL_SystemLastEditUser)
VALUES
(@SRL_PK, @SRL_SRH, @SRL_LinNumber, @SRL_PackagesRemaining, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@SRL_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@SRL_SRH", SqlDbType.UniqueIdentifier, regHeaderPK);
				command.AddParameter("@SRL_LinNumber", SqlDbType.Int, lineNumber);
				command.AddParameter("@SRL_PackagesRemaining", SqlDbType.Int, packagesRemaining);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusTempStorageRegLineTransaction(Guid regLinePK, string internalReference, DateTime createTimeUTC, string transactionStatus, int packageQty = 0)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageRegLineTransaction
(SRT_PK, SRT_SRL, SRT_TransactionType, SRT_InternalReferenceNumber, SRT_TransactionStatus, SRT_PackageQty, SRT_SystemCreateTimeUTC, SRT_SystemCreateUser, SRT_SystemLastEditTimeUtc, SRT_SystemLastEditUser)
VALUES
(@SRT_PK, @SRT_SRL, @SRT_TransactionType, @SRT_InternalReferenceNumber, @SRT_TransactionStatus, @SRT_PackageQty, @SRT_SystemCreateTimeUTC, @SRT_SystemCreateUser, GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@SRT_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@SRT_SRL", SqlDbType.UniqueIdentifier, regLinePK);
				command.AddParameter("@SRT_TransactionType", SqlDbType.VarChar, CusTempStorageRegLineTransactionSchema.SRT_TransactionType.MaxLength, "OBL");
				command.AddParameter("@SRT_InternalReferenceNumber", SqlDbType.VarChar, CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber.MaxLength, internalReference);
				command.AddParameter("@SRT_TransactionStatus", SqlDbType.VarChar, CusTempStorageRegLineTransactionSchema.SRT_TransactionStatus.MaxLength, transactionStatus);
				command.AddParameter("@SRT_PackageQty", SqlDbType.Int, packageQty);
				command.AddParameter("@SRT_SystemCreateTimeUTC", SqlDbType.DateTime, createTimeUTC);
				command.AddParameter("@SRT_SystemCreateUser", SqlDbType.VarChar, CusTempStorageRegHeaderSchema.SRH_SystemCreateUser.MaxLength, "~BP");
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusTempStorageRegPremises(string code, string customsLocation, Guid addressPK, DateTime createTimeUTC)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageRegPremises
(SRP_PK, SRP_OA_PremisesAddress, SRP_Code, SRP_CustomsLocation, SRP_Description, SRP_SystemCreateTimeUtc, SRP_SystemCreateUser, SRP_SystemLastEditTimeUtc, SRP_SystemLastEditUser)
VALUES
(@SRP_PK, @SRP_OA_PremisesAddress, @SRP_Code, @SRP_CustomsLocation, 'TEST', @SRP_SystemCreateTimeUtc, @SRP_SystemCreateUser, GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@SRP_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@SRP_OA_PremisesAddress", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@SRP_Code", SqlDbType.VarChar, CusTempStorageRegPremisesSchema.SRP_Code.MaxLength, code);
				command.AddParameter("@SRP_CustomsLocation", SqlDbType.VarChar, CusTempStorageRegPremisesSchema.SRP_CustomsLocation.MaxLength, customsLocation);
				command.AddParameter("@SRP_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTimeUTC);
				command.AddParameter("@SRP_SystemCreateUser", SqlDbType.VarChar, CusTempStorageRegPremisesSchema.SRP_SystemCreateUser.MaxLength, "~BP");
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusTempStorageRegLineItem(int itemNumber, string tariff, DateTime createTimeUTC)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageRegLineItem
(SRI_PK, SRI_GoodsItemNumber, SRI_Tariff, SRI_SystemCreateTimeUtc, SRI_SystemCreateUser, SRI_SystemLastEditTimeUtc, SRI_SystemLastEditUser)
VALUES
(@SRI_PK, @SRI_GoodsItemNumber, @SRI_Tariff, @SRI_SystemCreateTimeUtc, @SRI_SystemCreateUser, GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@SRI_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@SRI_GoodsItemNumber", SqlDbType.Int, itemNumber);
				command.AddParameter("@SRI_Tariff", SqlDbType.VarChar, CusTempStorageRegLineItemSchema.SRI_Tariff.MaxLength, tariff);
				command.AddParameter("@SRI_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTimeUTC);
				command.AddParameter("@SRI_SystemCreateUser", SqlDbType.VarChar, CusTempStorageRegPremisesSchema.SRP_SystemCreateUser.MaxLength, "~BP");
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusTempStorageRegLineItemPivot(Guid regLinePK, Guid regLineItemPK, decimal grossWeight, DateTime createTimeUTC)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusTempStorageRegLineItemPivot
(SRV_PK, SRV_SRL_Line, SRV_SRI_Item, SRV_GrossWeight, SRV_SystemCreateTimeUtc, SRV_SystemCreateUser, SRV_SystemLastEditTimeUtc, SRV_SystemLastEditUser)
VALUES
(@SRV_PK, @SRV_SRL_Line, @SRV_SRI_Item, @SRV_GrossWeight, @SRV_SystemCreateTimeUtc, @SRV_SystemCreateUser, GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@SRV_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@SRV_SRL_Line", SqlDbType.UniqueIdentifier, regLinePK);
				command.AddParameter("@SRV_SRI_Item", SqlDbType.UniqueIdentifier, regLineItemPK);
				command.AddParameter("@SRV_GrossWeight", SqlDbType.Decimal, grossWeight);
				command.AddParameter("@SRV_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTimeUTC);
				command.AddParameter("@SRV_SystemCreateUser", SqlDbType.VarChar, CusTempStorageRegPremisesSchema.SRP_SystemCreateUser.MaxLength, "~BP");
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusInbondHeader(string jobNumber, Guid branchPK, string applicationCode = "", string headerType = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInbondHeader
(BH_PK, BH_JobReference, BH_GB, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_ApplicationCode, BH_HeaderType)
VALUES
(@BH_PK, @BH_JobReference, @BH_GB, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @BH_ApplicationCode, @BH_HeaderType)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@BH_JobReference", SqlDbType.VarChar, CusInBondHeaderSchema.BH_JobReference.MaxLength, jobNumber);
				command.AddParameter("@BH_GB", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@BH_ApplicationCode", SqlDbType.VarChar, applicationCode);
				command.AddParameter("@BH_HeaderType", SqlDbType.VarChar, headerType);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusInBondMoveHeader(Guid bondHeaderPK, string subApplicationCode = "", string customsStatus = "", DateTime? createTimeUtc = null, string messageStatus = "", string gONumber = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInbondMoveHeader
(BM_PK, BM_BH, BM_SubApplicationCode, BM_CustomsStatus, BM_MessageStatus, BM_GONumber, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
VALUES
(@BM_PK, @BM_BH, @BM_SubApplicationCode, @BM_CustomsStatus, @BM_MessageStatus, @BM_GONumber, @BM_SystemCreateTimeUtc, '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BM_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@BM_BH", SqlDbType.UniqueIdentifier, bondHeaderPK);
				command.AddParameter("@BM_SubApplicationCode", SqlDbType.VarChar, subApplicationCode);
				command.AddParameter("@BM_CustomsStatus", SqlDbType.VarChar, customsStatus);
				command.AddParameter("@BM_SystemCreateTimeUtc", SqlDbType.SmallDateTime, createTimeUtc ?? DateTime.UtcNow);
				command.AddParameter("@BM_MessageStatus", SqlDbType.VarChar, messageStatus);
				command.AddParameter("@BM_GONumber", SqlDbType.VarChar, gONumber);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusBondDetail(Guid parentID, string parentTableCode, string bondNumber = "", decimal bondAmount = 0m)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusBondDetail
(PW_PK, PW_ParentID, PW_ParentTableCode, PW_BondNumber, PW_BondAmount)
VALUES
(@PW_PK, @PW_ParentID, @PW_ParentTableCode, @PW_BondNumber, @PW_BondAmount)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PW_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@PW_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@PW_ParentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@PW_BondNumber", SqlDbType.VarChar, bondNumber);
				command.AddParameter("@PW_BondAmount", SqlDbType.Decimal, bondAmount);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusBondDetail(Guid parentID, string applicationCode, DateTime? bondEffectiveDate = null, DateTime? bondExpiryDate = null, string bondNumber = "", string bondType = "", string suretyCode = "", string parentTableCode = "OH")
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusBondDetail
(PW_PK, PW_ParentID, PW_ParentTableCode, PW_ApplicationCode, PW_BondNumber, PW_BondType, PW_SuretyCode, PW_BondEffectiveDate, PW_BondExpiryDate)
VALUES
(@PW_PK, @PW_ParentID, @PW_ParentTableCode, @PW_ApplicationCode, @PW_BondNumber, @PW_BondType, @PW_SuretyCode, @PW_BondEffectiveDate, @PW_BondExpiryDate)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PW_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@PW_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@PW_ParentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@PW_ApplicationCode", SqlDbType.VarChar, applicationCode);
				command.AddParameter("@PW_BondNumber", SqlDbType.VarChar, bondNumber);
				command.AddParameter("@PW_BondType", SqlDbType.VarChar, bondType);
				command.AddParameter("@PW_SuretyCode", SqlDbType.VarChar, suretyCode);
				command.AddParameter("@PW_BondEffectiveDate", SqlDbType.DateTime, bondEffectiveDate);
				command.AddParameter("@PW_BondExpiryDate", SqlDbType.DateTime, bondExpiryDate);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusInBondMoveHeader(Guid headerPK)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInBondMoveHeader
(BM_PK, BM_BH, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
VALUES
(@BM_PK, @BM_BH, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BM_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@BM_BH", SqlDbType.UniqueIdentifier, headerPK);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusInBondMoveDetail(Guid bm_pk)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInBondMoveDetail
(B9_PK, B9_BM, B9_SystemCreateTimeUtc, B9_SystemCreateUser, B9_SystemLastEditTimeUtc, B9_SystemLastEditUser)
VALUES
(@B9_PK, @B9_BM, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@B9_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@B9_BM", SqlDbType.UniqueIdentifier, bm_pk);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusInBondPayInfo(Guid moveHeaderPK, string incomingPayResponseNo, string methodOfPayment, decimal paymentAmount, DateTime? paymentDate, string paymentStatus, string transactionType)
		{
			var pk = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.CusInBondPayInfo
(BPI_PK, BPI_BM, BPI_IncomingPayResponseNo, BPI_MethodOfPayment, BPI_PaymentAmount, BPI_PaymentDate, BPI_PaymentStatus, BPI_TransactionType, BPI_IsValid, BPI_SystemCreateTimeUtc, BPI_SystemCreateUser, BPI_SystemLastEditTimeUtc, BPI_SystemLastEditUser)
VALUES
(@BPI_PK, @BPI_BM, @BPI_IncomingPayResponseNo, @BPI_MethodOfPayment, @BPI_PaymentAmount, @BPI_PaymentDate, @BPI_PaymentStatus, @BPI_TransactionType, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BPI_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@BPI_BM", SqlDbType.UniqueIdentifier, moveHeaderPK);
				command.AddParameter("@BPI_IncomingPayResponseNo", SqlDbType.VarChar, incomingPayResponseNo);
				command.AddParameter("@BPI_MethodOfPayment", SqlDbType.VarChar, methodOfPayment);
				command.AddParameter("@BPI_PaymentAmount", SqlDbType.Money, paymentAmount);
				command.AddParameter("@BPI_PaymentDate", SqlDbType.SmallDateTime, paymentDate ?? (object)DBNull.Value);
				command.AddParameter("@BPI_PaymentStatus", SqlDbType.VarChar, paymentStatus);
				command.AddParameter("@BPI_TransactionType", SqlDbType.VarChar, transactionType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateCusPermitHeader(string cphNumber, string unitOfMeasure, DateTime startDate, Guid permitHolder, string applicationCode, string type, string countryCode, string subType = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO CusPermitHeader
(CPH_PK, CPH_Number, CPH_UnitOfMeasure, CPH_StartDate, CPH_SubType, CPH_OH_PermitHolder, CPH_ApplicationCode, CPH_Type, CPH_RN_NKCountryCode)
VALUES
(@CPH_PK, @CPH_Number, @CPH_UnitOfMeasure, @CPH_StartDate, @CPH_SubType, @CPH_OH_PermitHolder, @CPH_ApplicationCode, @CPH_Type, @CPH_RN_NKCountryCode)";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CPH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@CPH_Number", SqlDbType.VarChar, cphNumber);
				command.AddParameter("@CPH_UnitOfMeasure", SqlDbType.VarChar, unitOfMeasure);
				command.AddParameter("@CPH_StartDate", SqlDbType.Date, startDate.Date);
				command.AddParameter("@CPH_SubType", SqlDbType.VarChar, subType);
				command.AddParameter("@CPH_OH_PermitHolder", SqlDbType.UniqueIdentifier, permitHolder);
				command.AddParameter("@CPH_ApplicationCode", SqlDbType.VarChar, applicationCode);
				command.AddParameter("@CPH_Type", SqlDbType.VarChar, type);
				command.AddParameter("@CPH_RN_NKCountryCode", SqlDbType.Char, countryCode);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateGenAddOnColumn(Guid parentID, string parentTableCode, string name, string type, string data)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.GenAddOnColumn
(XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID)
VALUES
(@XA_PK, @XA_Name, @XA_Type, @XA_Data, @XA_ParentTableCode, @XA_ParentID)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@XA_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@XA_Name", SqlDbType.VarChar, GenAddOnColumnSchema.XA_Name.MaxLength, name);
				command.AddParameter("@XA_Type", SqlDbType.VarChar, GenAddOnColumnSchema.XA_Type.MaxLength, type);
				command.AddParameter("@XA_Data", SqlDbType.VarChar, GenAddOnColumnSchema.XA_Data.MaxLength, data);
				command.AddParameter("@XA_ParentTableCode", SqlDbType.VarChar, GenAddOnColumnSchema.XA_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@XA_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static void CreateOrUpdateGenAddOnColumn(Guid declarationPK, string parentTableCode, string fieldType, string fieldName, string fieldData)
		{
			var genAddOnSql = @"
IF EXISTS(SELECT 1 FROM dbo.GenAddOnColumn WHERE XA_ParentTableCode = @parentTableCode AND XA_ParentID = @declarationPK AND XA_Name = @fieldName)
	BEGIN
	UPDATE dbo.GenAddOnColumn
	SET XA_Data = @fieldData, XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate()
	WHERE XA_ParentTableCode = @parentTableCode AND XA_ParentID = @declarationPK AND XA_Name =  @fieldName
	END
ELSE
	BEGIN
	INSERT INTO dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID)
	VALUES (@genAddOnPK, @fieldName, @fieldType, @fieldData, @parentTableCode, @declarationPK)
	END
";
			using (var command = Db.Connection.Command(genAddOnSql))
			{
				command.AddParameter("@genAddOnPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@fieldName", SqlDbType.VarChar, fieldName);
				command.AddParameter("@fieldType", SqlDbType.VarChar, fieldType);
				command.AddParameter("@fieldData", SqlDbType.VarChar, fieldData);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.ExecuteNonQuery();
			}
		}

		public static Guid CreateEdiMessageRegistrationNumber(Guid ediMessagePK, string registrationNumber)
		{
			return CreateNote(ediMessagePK, registrationNumber, "RegistrationNumber");
		}

		public static Guid CreateEdiMessageLocalReferenceNumber(Guid ediMessagePK, string localReferenceNumber)
		{
			return CreateNote(ediMessagePK, localReferenceNumber, "LocalReferenceNumber");
		}

		static Guid CreateNote(Guid ediMessagePK, string localReferenceNumber, string description)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.StmNote
(ST_PK, ST_ParentID, ST_Table, ST_NoteType, ST_Description, ST_IsCustomDescription, ST_NoteContext, ST_NoteText)
VALUES
(@ST_PK, @ST_ParentID, 'EDIMessage', 'INT', @ST_Description, 0, 'AAA', @ST_NoteText)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ST_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ST_ParentID", SqlDbType.UniqueIdentifier, ediMessagePK);
				command.AddParameter("@ST_Description", SqlDbType.VarChar, description);
				command.AddParameter("@ST_NoteText", SqlDbType.NVarChar, localReferenceNumber);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid InsertTESJobConfig(DbConnection connection, Guid companyPK, string parentPrefix, Guid parentPK, string templateCode, string ledger = LedgerTypeCodes.AccountsReceivable)
		{
			var filePk = InsertTemplateFile(connection, companyPK, templateCode, ledger);
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentId, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Code, JCF_SystemCreateTimeUtc, JCF_SystemCreateUser, JCF_SystemLastEditTimeUtc, JCF_SystemLastEditUser)
VALUES(@PK, @ConfigType, @GC, @Ledger, @ParentPrefix, @ParentPK, @JobType, @Direction, @Mode, @Code, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@ConfigType", SqlDbType.Char, "TES");
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@ParentPrefix", SqlDbType.VarChar, parentPrefix);
				cmd.AddParameter("@ParentPK", SqlDbType.UniqueIdentifier, parentPK == Guid.Empty ? DBNull.Value : parentPK);
				cmd.AddParameter("@JobType", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Direction", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Mode", SqlDbType.Char, "ALL");
				cmd.AddParameter("@Code", SqlDbType.Char, templateCode);
				cmd.ExecuteNonQuery();
			}
			return filePk;
		}

		public static Guid InsertTemplateFile(DbConnection connection, Guid companyPK, string templateCode, string ledger = LedgerTypeCodes.AccountsReceivable)
		{
			var pk = Guid.NewGuid();
			var testValue = "This is a test value.";
			var fileData = Encoding.UTF8.GetBytes(testValue);

			var sql = @"INSERT INTO dbo.AccTemplateFileStorage (TFS_PK, TFS_GC, TFS_Code, TFS_Ledger, TFS_Description, TFS_FileName, TFS_IsActive, TFS_FileData, TFS_SystemCreateTimeUtc, TFS_SystemCreateUser, TFS_SystemLastEditTimeUtc, TFS_SystemLastEditUser)
														VALUES(@PK, @GC, @Code, @Ledger, @Description, @FileName, @IsActive, @FileData, @SystemCreateTime, @SystemCreateUser, @SystemLastEditTime, @SystemLastEditUser)";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK == Guid.Empty ? DBNull.Value : companyPK);
				cmd.AddParameter("@Code", SqlDbType.Char, templateCode);
				cmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
				cmd.AddParameter("@Description", SqlDbType.VarChar, "Test");
				cmd.AddParameter("@FileName", SqlDbType.VarChar, "TestFile.xslt");
				cmd.AddParameter("@IsActive", SqlDbType.Bit, 1);
				cmd.AddParameter("@FileData", SqlDbType.VarBinary, fileData);
				cmd.AddParameter("@SystemCreateTime", SqlDbType.SmallDateTime, DateTime.UtcNow);
				cmd.AddParameter("@SystemCreateUser", SqlDbType.Char, "~BP");
				cmd.AddParameter("@SystemLastEditTime", SqlDbType.SmallDateTime, DateTime.UtcNow);
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.Char, "~BP");
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateJobHeader(Guid branchPK, Guid companyPK, Guid jobParentID, Guid departmentPK, string parentTableCode, string jobParentNum, string jobLocalReference, string status, Guid? localChargesAddressPK = null)
		{
			var jobHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobHeader
(JH_PK, JH_GB, JH_GE, JH_GC, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_HeaderType, JH_JobLocalReference, JH_Status, JH_OA_LocalChargesAddr, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_SystemLastEditTimeUtc, JH_SystemLastEditUser)
VALUES
(@JH_PK, @JH_GB, @JH_GE, @JH_GC, @JH_ParentID, @JH_ParentTableCode, @JH_JobNum, 'JOB', @JH_JobLocalReference, @JH_Status, @JH_OA_LocalChargesAddr, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JH_PK", SqlDbType.UniqueIdentifier, jobHeaderPK);
				command.AddParameter("@JH_GB", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@JH_GE", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@JH_GC", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JH_ParentID", SqlDbType.UniqueIdentifier, jobParentID);
				command.AddParameter("@JH_ParentTableCode", SqlDbType.VarChar, JobHeaderSchema.JH_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@JH_JobNum", SqlDbType.VarChar, JobHeaderSchema.JH_JobNum.MaxLength, jobParentNum);
				command.AddParameter("@JH_JobLocalReference", SqlDbType.VarChar, JobHeaderSchema.JH_JobLocalReference.MaxLength, jobLocalReference);
				command.AddParameter("@JH_Status", SqlDbType.VarChar, JobHeaderSchema.JH_Status.MaxLength, status);
				command.AddParameter("@JH_OA_LocalChargesAddr", SqlDbType.UniqueIdentifier, localChargesAddressPK == null ? DBNull.Value : localChargesAddressPK);
				command.ExecuteNonQuery();
			}
			return jobHeaderPK;
		}

		public static Guid CreateJobCharge(Guid branchPK, Guid companyPK, Guid departmentPK, Guid jobHeaderPK, Guid accChargeCodePK, string invoiceNumber, DateTime invoiceDate)
		{
			var jobChargePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobCharge
(JR_PK, JR_GB, JR_GC, JR_GE, JR_JH, JR_AC, JR_APInvoiceNum, JR_APInvoiceDate, JR_SystemCreateTimeUtc, JR_SystemCreateUser, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser)
VALUES
(@jobChargePK, @branchPK, @companyPK, @departmentPK, @jobHeaderPK, @accChargeCodePK, @invoiceNumber, @invoiceDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobChargePK", SqlDbType.UniqueIdentifier, jobChargePK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@jobHeaderPK", SqlDbType.UniqueIdentifier, jobHeaderPK);
				command.AddParameter("@accChargeCodePK", SqlDbType.UniqueIdentifier, accChargeCodePK);
				command.AddParameter("@invoiceNumber", SqlDbType.VarChar, JobChargeSchema.JR_APInvoiceNum.MaxLength, invoiceNumber);
				command.AddParameter("@invoiceDate", SqlDbType.SmallDateTime, invoiceDate);
				command.ExecuteNonQuery();
			}
			return jobChargePK;
		}

		public static Guid CreateAccTransactionHeader(
			Guid branchPK, Guid companyPK, Guid departmentPK, Guid jobPK,
			string ledger, string transactionType, string jobParentNum,
			decimal invoiceAmount = 0m, decimal gstAmount = 0m, decimal ostTotal = 0m, decimal exchangeRate = 1m, decimal outstandingAmount = 0m,
			string transactionNumber = "")
		{
			var transHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.AccTransactionHeader
(AH_PK, AH_GB, AH_GC, AH_GE, AH_JH, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_Desc, AH_InvoiceAmount, AH_GSTAmount, AH_OSTotal, AH_ExchangeRate, AH_OutstandingAmount, AH_InvoiceDate, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
VALUES
(@AH_PK, @AH_GB, @AH_GC, @AH_GE, @AH_JH, @AH_Ledger, @AH_TransactionType, @AH_TransactionNum, @AH_Desc, @AH_InvoiceAmount, @AH_GSTAmount, @AH_OSTotal, @AH_ExchangeRate, @AH_OutstandingAmount, @AH_InvoiceDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@AH_PK", SqlDbType.UniqueIdentifier, transHeaderPK);
				command.AddParameter("@AH_GB", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@AH_GC", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@AH_GE", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@AH_JH", SqlDbType.UniqueIdentifier, jobPK);
				command.AddParameter("@AH_Ledger", SqlDbType.Char, AccTransactionHeaderSchema.AH_Ledger.MaxLength, ledger);
				command.AddParameter("@AH_TransactionType", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_TransactionType.MaxLength, transactionType);
				command.AddParameter("@AH_TransactionNum", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength, string.IsNullOrEmpty(transactionNumber) ? transHeaderPK.ToString() : transactionNumber);
				command.AddParameter("@AH_Desc", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_Desc.MaxLength, jobParentNum);
				command.AddParameter("@AH_InvoiceAmount", SqlDbType.Money, invoiceAmount);
				command.AddParameter("@AH_GSTAmount", SqlDbType.Money, gstAmount);
				command.AddParameter("@AH_OSTotal", SqlDbType.Money, ostTotal);
				command.AddParameter("@AH_ExchangeRate", SqlDbType.Decimal, exchangeRate);
				command.AddParameter("@AH_OutstandingAmount", SqlDbType.Money, outstandingAmount);
				command.AddParameter("@AH_InvoiceDate", SqlDbType.SmallDateTime, DateTime.Today);

				command.ExecuteNonQuery();
			}
			return transHeaderPK;
		}

		public static Guid CreateAccTransactionLine(
			Guid branchPK, Guid companyPK, Guid jobHeaderPK, Guid transactionHeaderPK, Guid? taxPK, Guid departmentPK,
			int sequenceNum, string desc, string lineType,
			decimal lineAmount = 0m, decimal gstVat = 0m, decimal osAmount = 0m, decimal exchangeRate = 1m, int taxrateNumerator = 1, Guid? chargePK = null)
		{
			var transLinePK = Guid.NewGuid();
			const string sql = @"
INSERT INTO dbo.AccTransactionLines
(AL_PK, AL_GB, AL_GC, AL_JH, AL_AC, AL_AH, AL_AT, AL_GE,
AL_Sequence, AL_Desc, AL_LineAmount, AL_OSAmount, AL_GSTVAT, AL_ExchangeRate, AL_TaxRateNumerator, AL_LineType, AL_SystemCreateTimeUtc, AL_SystemCreateUser, AL_SystemLastEditTimeUtc, AL_SystemLastEditUser)
VALUES
(@AL_PK, @AL_GB, @AL_GC, @AL_JH, @AL_AC, @AL_AH, @AL_AT, @AL_GE, @AL_Sequence, @AL_Desc, @AL_LineAmount, @AL_OSAmount, @AL_GSTVAT, @AL_ExchangeRate, @AL_TaxRateNumerator, @AL_LineType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@AL_PK", SqlDbType.UniqueIdentifier, transLinePK);
				command.AddParameter("@AL_GB", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@AL_GC", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@AL_JH", SqlDbType.UniqueIdentifier, jobHeaderPK);
				command.AddParameter("@AL_AC", SqlDbType.UniqueIdentifier, chargePK ?? (object)DBNull.Value);
				command.AddParameter("@AL_AH", SqlDbType.UniqueIdentifier, transactionHeaderPK);
				command.AddParameter("@AL_AT", SqlDbType.UniqueIdentifier, taxPK ?? (object)DBNull.Value);
				command.AddParameter("@AL_GE", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@AL_Sequence", SqlDbType.Int, sequenceNum);
				command.AddParameter("@AL_Desc", SqlDbType.VarChar, AccTransactionLinesSchema.AL_Desc.MaxLength, desc);
				command.AddParameter("@AL_LineAmount", SqlDbType.Money, lineAmount);
				command.AddParameter("@AL_OSAmount", SqlDbType.Money, osAmount);
				command.AddParameter("@AL_GSTVAT", SqlDbType.Money, gstVat);
				command.AddParameter("@AL_ExchangeRate", SqlDbType.Decimal, exchangeRate);
				command.AddParameter("@AL_TaxRateNumerator", SqlDbType.Int, taxrateNumerator);
				command.AddParameter("@AL_LineType", SqlDbType.Char, AccTransactionLinesSchema.AL_Desc.MaxLength, lineType);

				command.ExecuteNonQuery();
			}
			return transLinePK;
		}

		public static Guid CreateAccCharges(Guid companyPK,
			string code, string desc, string chargeType, string chargeGroup = "")
		{
			var chargePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.AccChargeCode
(AC_PK, AC_GC, AC_Code, AC_Desc, AC_ChargeType, AC_ChargeGroup, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser)
VALUES
(@AC_PK, @AC_GC, @AC_Code, @AC_Desc, @AC_ChargeType, @AC_ChargeGroup, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@AC_PK", SqlDbType.UniqueIdentifier, chargePK);
				command.AddParameter("@AC_GC", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@AC_Code", SqlDbType.VarChar, AccChargeCodeSchema.AC_Code.MaxLength, code);
				command.AddParameter("@AC_Desc", SqlDbType.VarChar, AccChargeCodeSchema.AC_Desc.MaxLength, desc);
				command.AddParameter("@AC_ChargeType", SqlDbType.VarChar, AccChargeCodeSchema.AC_ChargeType.MaxLength, chargeType);
				command.AddParameter("@AC_ChargeGroup", SqlDbType.VarChar, AccChargeCodeSchema.AC_ChargeGroup.MaxLength, chargeGroup);

				command.ExecuteNonQuery();
			}
			return chargePK;
		}

		public static Guid CreateOrExistingAccTaxRate(string code, string desc, string type, string country)
		{
			var sql = @"SELECT AT_PK FROM dbo.AccTaxRate WHERE AT_RN_NKCounty = @AT_RN_NKCountry AND AT_Code = @AT_Code AND AT_Type = @AT_Type";
			var result = Guid.Empty;

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@AT_Code", SqlDbType.Char, AccTaxRateSchema.AT_Code.MaxLength, code);
				command.AddParameter("@AT_Type", SqlDbType.VarChar, AccTaxRateSchema.AT_Type.MaxLength, type);
				command.AddParameter("@AT_RN_NKCountry", SqlDbType.VarChar, AccTaxRateSchema.AT_RN_NKCountry.MaxLength, country);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result = reader.GetGuid(0);
					}
				}
			}
			if (result == Guid.Empty)
			{
				result = CreateAccTaxRate(code, desc, type, country);
			}
			return result;
		}

		public static Guid CreateAccTaxRate(string code, string desc, string type, string country)
		{
			var atPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.AccTaxRate
(AT_PK, AT_Code, AT_Type, AT_Description, AT_RN_NKCountry, AT_SystemCreateTimeUtc, AT_SystemCreateUser, AT_SystemLastEditTimeUtc, AT_SystemLastEditUser)
VALUES
(@AT_PK, @AT_Code, @AT_Type, @AT_Description, @AT_RN_NKCountry, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@AT_PK", SqlDbType.UniqueIdentifier, atPK);
				command.AddParameter("@AT_Code", SqlDbType.VarChar, AccTaxRateSchema.AT_Code.MaxLength, code);
				command.AddParameter("@AT_Type", SqlDbType.VarChar, AccTaxRateSchema.AT_Type.MaxLength, type);
				command.AddParameter("@AT_Description", SqlDbType.VarChar, AccTaxRateSchema.AT_Description.MaxLength, desc);
				command.AddParameter("@AT_RN_NKCountry", SqlDbType.VarChar, AccTaxRateSchema.AT_RN_NKCountry.MaxLength, country);
				command.ExecuteNonQuery();
			}
			return atPK;
		}

		public static Guid CreateStmNote(string parentTable, Guid parentPK, string noteData, string description)
		{
			var pk = Guid.NewGuid();
			var sql = $@"INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Table, ST_Description, ST_NoteData) VALUES (@pk, @parentPK, @parentTable, @description,  CONVERT(varbinary(max), '{noteData}'))";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@description", SqlDbType.VarChar, StmNoteSchema.ST_Description.MaxLength, description);
				command.AddParameter("@parentTable", SqlDbType.VarChar, StmNoteSchema.ST_Table.MaxLength, parentTable);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateStmALog(string parentTable, Guid parentPK, string reference, DateTime eventTime, string @event, DateTime? postedTime = null)
		{
			var stmALogPK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_Reference, SL_EventTime, SL_SE_NKEvent, SL_PostedTimeUtc)
				VALUES (@pk, @parentTable, @parentPK, @reference, @eventTime, @event, @postedTime)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, stmALogPK);
				command.AddParameter("@parentTable", SqlDbType.VarChar, StmALogSchema.SL_Table.MaxLength, parentTable);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@reference", SqlDbType.VarChar, StmALogSchema.SL_Reference.MaxLength, reference);
				command.AddParameter("@eventTime", SqlDbType.DateTime, StmALogSchema.SL_EventTime.MaxLength, eventTime);
				command.AddParameter("@event", SqlDbType.VarChar, StmALogSchema.SL_SE_NKEvent.MaxLength, @event);
				command.AddParameter("@postedTime", SqlDbType.DateTime, postedTime ?? DateTime.UtcNow);
				command.ExecuteNonQuery();
			}
			return stmALogPK;
		}

		public static Guid CreateStmData(Guid ownerPK, string sdBinaryValue)
		{
			var result = Guid.NewGuid();
			var stmDataPKSql = @"
INSERT INTO dbo.StmData (SD_PK, SD_Owner, SD_BinaryValue, SD_Name)
VALUES (@DataPK, @OwnerPK, @SDBinaryValue, 'AccountSecurityNo');";
			using (var command = Db.Connection.Command(stmDataPKSql))
			{
				command.AddParameter("@DataPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@OwnerPK", SqlDbType.UniqueIdentifier, ownerPK);
				command.AddParameter("@SDBinaryValue", SqlDbType.VarBinary, Encoding.UTF8.GetBytes(sdBinaryValue));
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusSupportingInfo(string csiType, string csiCode, string csiSubType, string csiReferenceNumber, Guid csiParentID, string csiParentTableCode, string csiDataModel = "", decimal csiQuantity = 0, string csiUnitOfQuantity = "", decimal csiQuantity2 = 0, string csiUnitOfQuantity2 = "", int csiPackQty = 0, string csiPackType = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.CusSupportingInfo (CSI_PK, CSI_Type, CSI_Code, CSI_SubType, CSI_ReferenceNumber, CSI_ParentID, CSI_ParentTableCode, CSI_DataModel, CSI_Quantity, CSI_UnitOfQuantity, CSI_Quantity2, CSI_UnitOfQuantity2, CSI_PackQty, CSI_PackType, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser)
				VALUES (@pk, @csiType, @csiCode, @csiSubType, @csiReferenceNumber, @csiParentID, @csiParentTableCode, @csiDataModel, @csiQuantity, @csiUnitOfQuantity, @csiQuantity2, @csiUnitOfQuantity2, @csiPackQty, @csiPackType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@csiType", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_Type.MaxLength, csiType);
				command.AddParameter("@csiCode", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_Code.MaxLength, csiCode);
				command.AddParameter("@csiSubType", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_SubType.MaxLength, csiSubType);
				command.AddParameter("@csiReferenceNumber", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_ReferenceNumber.MaxLength, csiReferenceNumber);
				command.AddParameter("@csiParentID", SqlDbType.UniqueIdentifier, csiParentID);
				command.AddParameter("@csiParentTableCode", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_ParentTableCode.MaxLength, csiParentTableCode);
				command.AddParameter("@csiDataModel", SqlDbType.VarChar, csiDataModel);
				command.AddParameter("@csiQuantity", SqlDbType.Decimal, csiQuantity);
				command.AddParameter("@csiUnitOfQuantity", SqlDbType.VarChar, csiUnitOfQuantity);
				command.AddParameter("@csiQuantity2", SqlDbType.Decimal, csiQuantity2);
				command.AddParameter("@csiUnitOfQuantity2", SqlDbType.VarChar, csiUnitOfQuantity2);
				command.AddParameter("@csiPackQty", SqlDbType.Int, csiPackQty);
				command.AddParameter("@csiPackType", SqlDbType.VarChar, csiPackType);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusContainer(string containerNumber, Guid declarationPK, int clusterKey, string dataModel, Guid? coJC = null)
		{
			var pk = Guid.NewGuid();
			const string sql = @"
				INSERT INTO dbo.CusContainer (CO_PK, CO_ContainerNumber, CO_JE, CO_ClusterKey, CO_DataModel, CO_JC, CO_SystemCreateTimeUtc, CO_SystemCreateUser, CO_SystemLastEditTimeUtc, CO_SystemLastEditUser)
				VALUES (@pk, @containerNumber, @declarationPK, @clusterKey, @dataModel, @coJC, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@containerNumber", SqlDbType.VarChar, CusContainerSchema.CO_ContainerNumber.MaxLength, containerNumber);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@coJC", SqlDbType.UniqueIdentifier, coJC ?? (object)DBNull.Value);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateJobContainer(string containerNum, Guid? refContainerPk, Guid? consolPK = null)
		{
			var pk = Guid.NewGuid();
			const string sql = @"INSERT INTO dbo.JobContainer (JC_PK, JC_ContainerNum, JC_RC, JC_JK) VALUES (@pk, @containerNum, @refContainerPk, @consolPK)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@containerNum", SqlDbType.VarChar, JobContainerSchema.JC_ContainerNum.MaxLength, containerNum);
				command.AddParameter("@refContainerPk", SqlDbType.UniqueIdentifier, refContainerPk ?? (object)DBNull.Value);
				command.AddParameter("@consolPK", SqlDbType.UniqueIdentifier, consolPK ?? (object)DBNull.Value);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateJobBookedCtgMove(Guid cartagePK, Guid? containerPK = null)
		{
			var pk = Guid.NewGuid();
			const string sql = @"INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_JC_Container, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser) VALUES (@pk, @cartagePK, @containerPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@cartagePK", SqlDbType.UniqueIdentifier, cartagePK);
				command.AddParameter("@containerPK", SqlDbType.UniqueIdentifier, containerPK ?? (object)DBNull.Value);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateJobContainerLeg(Guid jobBookedCtgMovePK, string splitDeliverySuffix)
		{
			var pk = Guid.NewGuid();
			const string sql = @"INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser) VALUES (@pk, @jobBookedCtgMovePK, @splitDeliverySuffix, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobBookedCtgMovePK", SqlDbType.UniqueIdentifier, jobBookedCtgMovePK);
				command.AddParameter("@splitDeliverySuffix", SqlDbType.VarChar, JobContainerLegsSchema.JU_SplitDeliverySuffix.MaxLength, splitDeliverySuffix ?? string.Empty);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateJobPackLines(Guid shipmentPK, string packLineId = null)
		{
			var pk = Guid.NewGuid();
			const string sql = @"
				INSERT INTO dbo.JobPackLines (JL_PK, JL_JS, JL_PackLineId, JL_SystemCreateTimeUtc, JL_SystemCreateUser, JL_SystemLastEditTimeUtc, JL_SystemLastEditUser)
				VALUES (@JL_PK, @JL_JS, @JL_PackLineId, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JL_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@JL_JS", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@JL_PackLineId", SqlDbType.VarChar, packLineId ?? string.Empty);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateJobContainerPackPivot(Guid jobContainerPK, Guid jobPackLinesPK)
		{
			var pk = Guid.NewGuid();
			const string sql = @"
				INSERT INTO dbo.JobContainerPackPivot (J6_PK, J6_JC, J6_JL, J6_SystemCreateTimeUtc, J6_SystemCreateUser, J6_SystemLastEditTimeUtc, J6_SystemLastEditUser)
				VALUES (@J6_PK, @J6_JC, @J6_JL, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@J6_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@J6_JC", SqlDbType.UniqueIdentifier, jobContainerPK);
				command.AddParameter("@J6_JL", SqlDbType.UniqueIdentifier, jobPackLinesPK);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateRefContainer(string code)
		{
			var pk = Guid.NewGuid();
			const string sql = @"INSERT INTO dbo.RefContainer (RC_PK, RC_Code) VALUES (@pk, @code)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefContainerSchema.RC_Code.MaxLength, code);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusContainerInvoiceLinePivot(Guid containerPK, Guid invoiceLinePK, int clusterKey)
		{
			var pk = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.CusContainerInvoiceLinePivot (C2_PK, C2_CO, C2_JI, C2_ClusterKey, C2_SystemCreateTimeUtc, C2_SystemCreateUser, C2_SystemLastEditTimeUtc, C2_SystemLastEditUser)
				VALUES (@pk, @containerPK, @invoiceLinePK, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@containerPK", SqlDbType.UniqueIdentifier, containerPK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		public static Guid CreateCusDecHouseBill(bool isValid, string addInfo, Guid declarationPK, int clusterKey, string messageStatus = "", string billNum = "", string billType = "")
		{
			var billPK = Guid.NewGuid();
			const string sql = @"
				INSERT INTO dbo.CusDecHouseBill
				(CU_PK, CU_IsValid, CU_AddInfo, CU_MessageStatus, CU_BillNum, CU_BillType, CU_JE, CU_ClusterKey, CU_SystemCreateTimeUtc, CU_SystemCreateUser, CU_SystemLastEditTimeUtc, CU_SystemLastEditUser)
				VALUES
				(@CU_PK, @CU_IsValid, @CU_AddInfo, @CU_MessageStatus, @CU_BillNum, @CU_BillType, @CU_JE, @CU_ClusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CU_PK", SqlDbType.UniqueIdentifier, billPK);
				command.AddParameter("@CU_IsValid", SqlDbType.Bit, isValid);
				command.AddParameter("@CU_AddInfo", SqlDbType.VarChar, CusDecHouseBillSchema.CU_AddInfo.MaxLength, addInfo);
				command.AddParameter("@CU_MessageStatus", SqlDbType.VarChar, CusDecHouseBillSchema.CU_MessageStatus.MaxLength, messageStatus);
				command.AddParameter("@CU_BillNum", SqlDbType.VarChar, CusDecHouseBillSchema.CU_BillNum.MaxLength, billNum);
				command.AddParameter("@CU_BillType", SqlDbType.VarChar, CusDecHouseBillSchema.CU_BillType.MaxLength, billType);
				command.AddParameter("@CU_JE", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@CU_ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return billPK;
		}

		public static Guid CreateCusClassification(string lookupCode, string countryCode, string classificationType)
		{
			var result = Guid.NewGuid();

			var sql = @"
				INSERT INTO dbo.CusClassification([CC_PK], [CC_LookupCode], [CC_Description], [CC_RN_NKCountryCode], [CC_ClassificationType], [CC_SystemCreateTimeUtc], [CC_SystemCreateUser], [CC_SystemLastEditTimeUtc], [CC_SystemLastEditUser])
				VALUES (@ccPk, @lookupCode, @lookupCode, @countryCode, @classificationType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ccPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@lookupCode", SqlDbType.VarChar, CusClassificationSchema.CC_LookupCode.MaxLength, lookupCode);
				command.AddParameter("@countryCode", SqlDbType.VarChar, CusClassificationSchema.CC_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@classificationType", SqlDbType.VarChar, CusClassificationSchema.CC_ClassificationType.MaxLength, classificationType);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateRefVessel(string code, string lloydsnumber, int clusterKey)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.RefVessel([RV_PK], [RV_Code], [RV_LloydsNumber])
			VALUES (@rvPk, @code, @lloydsnumber)
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@rvPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@lloydsnumber", SqlDbType.VarChar, lloydsnumber);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobShipment(string uniqueConsignRef = "")
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobShipment([JS_PK], [JS_UniqueConsignRef], [JS_SystemCreateTimeUtc], [JS_SystemCreateUser], [JS_SystemLastEditTimeUtc], [JS_SystemLastEditUser])
			VALUES (@jsPk, @jsUniqueConsignRef, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jsPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@jsUniqueConsignRef", SqlDbType.VarChar, uniqueConsignRef);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobConsol(string consolNumber = "")
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobConsol([JK_PK], [JK_UniqueConsignRef], [JK_SystemCreateTimeUtc], [JK_SystemCreateUser], [JK_SystemLastEditTimeUtc], [JK_SystemLastEditUser])
			VALUES (@JK_PK, @consolNumber, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JK_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@consolNumber", SqlDbType.VarChar, consolNumber ?? string.Empty);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobConShipLink(Guid shipmentPK, Guid consolPK)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobConShipLink([JN_PK], [JN_JS], [JN_JK], [JN_SystemCreateTimeUtc], [JN_SystemCreateUser], [JN_SystemLastEditTimeUtc], [JN_SystemLastEditUser])
			VALUES (@JN_PK, @JN_JS, @JN_JK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JN_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@JN_JS", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@JN_JK", SqlDbType.UniqueIdentifier, consolPK);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobConsolTransport(Guid parentPK, string vessel = null, string voyageFlight = null)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobConsolTransport([JW_PK], [JW_ParentGUID], [JW_Vessel], [JW_VoyageFlight], [JW_SystemCreateTimeUtc], [JW_SystemCreateUser], [JW_SystemLastEditTimeUtc], [JW_SystemLastEditUser])
			VALUES (@JW_PK, @JW_ParentGUID, @JW_Vessel, @JW_VoyageFlight, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@JW_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@JW_ParentGUID", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@JW_Vessel", SqlDbType.VarChar, vessel ?? "");
				command.AddParameter("@JW_VoyageFlight", SqlDbType.VarChar, voyageFlight ?? "");
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobOrderHeader(
			string orderNumber,
			Guid buyerAddress,
			byte orderNumberSplit = 0,
			Guid? shipment = null,
			Guid? supplierAddress = null,
			string orderStatus = "INC",
			bool isReleased = false,
			bool isCancelled = false)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobOrderHeader([JD_PK], [JD_JS], [JD_OrderNumber], [JD_OrderNumberSplit],  [JD_OA_BuyerAddress], [JD_OA_SupplierAddress], [JD_OrderStatus], [JD_IsReleased], [JD_IsCancelled], [JD_SystemCreateTimeUtc], [JD_SystemCreateUser], [JD_SystemLastEditTimeUtc], [JD_SystemLastEditUser])
			VALUES (@jdPk, @shipment, @orderNumber, @orderNumberSplit, @buyerAddress, @supplierAddress, @orderStatus, @isReleased, @isCancelled, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jdPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@shipment", SqlDbType.UniqueIdentifier, shipment ?? (object)DBNull.Value);
				command.AddParameter("@orderNumber", SqlDbType.VarChar, orderNumber);
				command.AddParameter("@orderNumberSplit", SqlDbType.TinyInt, orderNumberSplit);
				command.AddParameter("@buyerAddress", SqlDbType.UniqueIdentifier, buyerAddress);
				command.AddParameter("@supplierAddress", SqlDbType.UniqueIdentifier, supplierAddress ?? (object)DBNull.Value);
				command.AddParameter("@orderStatus", SqlDbType.VarChar, orderStatus);
				command.AddParameter("@isReleased", SqlDbType.Bit, isReleased);
				command.AddParameter("@isCancelled", SqlDbType.Bit, isCancelled);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateRatingHeader(string quoteNumber, Guid company, Guid? organisation = null, string quoteDate = null, string quoteEndDate = null)
		{
			var ratngHeaderPk = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.RatingHeader([TH_PK], [TH_RateType], [TH_QuoteNumber], [TH_QuoteDate], [TH_QuoteEndDate], [TH_IsCancelled], [TH_OH], [TH_GC], [TH_SystemLastEditTimeUtc], [TH_SystemLastEditUser], [TH_SystemCreateTimeUtc], [TH_SystemCreateUser])
			VALUES(@thPK, @thRateType, @quoteNumber, @quoteDate, @quoteEndDate, @thIsCancelled, @organisation, @company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@thPK", SqlDbType.UniqueIdentifier, ratngHeaderPk);
				command.AddParameter("@thRateType", SqlDbType.VarChar, "QTE");
				command.AddParameter("@quoteNumber", SqlDbType.VarChar, quoteNumber);

				if (quoteDate != null)
				{
					command.AddParameter("@quoteDate", SqlDbType.Date, quoteDate);
				}
				else
				{
					command.AddParameter("@quoteDate", SqlDbType.Date, DateTime.UtcNow);
				}

				if (quoteEndDate != null)
				{
					command.AddParameter("@quoteEndDate", SqlDbType.Date, quoteEndDate);
				}
				else
				{
					command.AddParameter("@quoteEndDate", SqlDbType.Date, DateTime.UtcNow);
				}

				command.AddParameter("@organisation", SqlDbType.UniqueIdentifier, organisation ?? (object)DBNull.Value);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company);
				command.AddParameter("@thIsCancelled", SqlDbType.Bit, 0);
				command.ExecuteNonQuery();
			}

			return ratngHeaderPk;
		}

		public static Guid CreateQuoteScope(Guid ratingHeaderPk, byte iDNumber, string productCode = "FWD", string transportMode = "AIR", string containerMode = "ULD", string origin = "AUSYD", string transshipment = "AUSYD", string destination = "BDUSA", Guid? pickupAddress = null, Guid? deliveryAddress = null,
																					string pickupAddressPostCode = "", string deliveryAddressPostCode = "", Guid? refContainer = null, string commodityCode = "", string incoTerm = "", string serviceLevel = "", string notes = "NOTES")
		{
			var quoteScopePk = Guid.NewGuid();

			var sql = @"INSERT INTO dbo.QuoteScope (QS_PK, QS_TH_RatingHeader, QS_ProductCode, QS_TransportMode, QS_ContainerMode, QS_Origin, QS_Transshipment, QS_Destination, QS_OA_PickupAddress, QS_OA_DeliveryAddress, QS_PickupAddressPostCode, QS_DeliveryAddressPostCode,  QS_RC_ContainerType, QS_RH_NKCommodityCode, QS_IncoTerm, QS_RS_NKServiceLevel,
						QS_RateOrigin, QS_RateDestination, QS_PlannedDischarge, QS_PlannedLoad, QS_FirstLoad, QS_FirstRouteSetLoadPort, QS_LastDischarge, QS_LastRouteSetDischargePort, QS_HBLDeliveryMode, QS_PaymentTerm, QS_PL_NKCarrierServiceLevel, QS_MatchContainerRateClass, QS_AircraftType, QS_TransitTime,
						QS_Frequency, QS_FrequencyUnit, QS_ContractNumber, QS_SystemCreateTimeUtc, QS_SystemCreateUser, QS_SystemLastEditTimeUtc, QS_SystemLastEditUser,
						QS_IDNumber, QS_Quantity, QS_Volume, QS_VolumeUnit, QS_Weight, QS_WeightUnit, QS_Notes)
					VALUES (@quoteScopePK, @ratingHeaderPK, @productCode, @transportMode, @containerMode, @origin, @transshipment, @destination, @pickupAddress, @deliveryAddress, @pickupAddressPostCode, @deliveryAddressPostCode, @refContainer, @commodityCode, @incoTerm, @serviceLevel,
						'', '', '', '', '', '', '', '', '', '', '', '', '', '',
						0, '', '', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP',
						@iDNumber, 10, 20, 'M3', 40, 'KG', @notes)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@quoteScopePK", quoteScopePk, QuoteScopeSchema.PK);
				command.AddParameterBasedOnDbColumn("@ratingHeaderPK", ratingHeaderPk, QuoteScopeSchema.QS_TH_RatingHeader);
				command.AddParameterBasedOnDbColumn("@productCode", productCode, QuoteScopeSchema.QS_ProductCode);
				command.AddParameterBasedOnDbColumn("@transportMode", transportMode, QuoteScopeSchema.QS_TransportMode);
				command.AddParameterBasedOnDbColumn("@containerMode", containerMode, QuoteScopeSchema.QS_ContainerMode);
				command.AddParameterBasedOnDbColumn("@origin", origin, QuoteScopeSchema.QS_Origin);
				command.AddParameterBasedOnDbColumn("@transshipment", transshipment, QuoteScopeSchema.QS_Transshipment);
				command.AddParameterBasedOnDbColumn("@destination", destination, QuoteScopeSchema.QS_Destination);
				command.AddParameterBasedOnDbColumn("@pickupAddress", pickupAddress == null ? DBNull.Value : pickupAddress, QuoteScopeSchema.QS_OA_PickupAddress);
				command.AddParameterBasedOnDbColumn("@deliveryAddress", deliveryAddress == null ? DBNull.Value : deliveryAddress, QuoteScopeSchema.QS_OA_DeliveryAddress);
				command.AddParameterBasedOnDbColumn("@pickupAddressPostCode", pickupAddressPostCode, QuoteScopeSchema.QS_PickupAddressPostCode);
				command.AddParameterBasedOnDbColumn("@deliveryAddressPostCode", deliveryAddressPostCode, QuoteScopeSchema.QS_DeliveryAddressPostCode);
				command.AddParameterBasedOnDbColumn("@refContainer", refContainer == null ? DBNull.Value : refContainer, QuoteScopeSchema.QS_RC_ContainerType);
				command.AddParameterBasedOnDbColumn("@commodityCode", commodityCode, QuoteScopeSchema.QS_RH_NKCommodityCode);
				command.AddParameterBasedOnDbColumn("@incoTerm", incoTerm, QuoteScopeSchema.QS_IncoTerm);
				command.AddParameterBasedOnDbColumn("@serviceLevel", serviceLevel, QuoteScopeSchema.QS_RS_NKServiceLevel);
				command.AddParameterBasedOnDbColumn("@iDNumber", iDNumber, QuoteScopeSchema.QS_IDNumber);
				command.AddParameterBasedOnDbColumn("@notes", (byte[])ZBlob.FromUTF8(notes), QuoteScopeSchema.QS_Notes);

				command.ExecuteNonQuery();
			}

			return quoteScopePk;
		}

		public static Guid CreateJobOrderLine(Guid orderHeaderPk, int lineNumber = 1)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobOrderLine([JO_PK], [JO_JD], [JO_LineNo], [JO_SystemCreateTimeUtc], [JO_SystemCreateUser], [JO_SystemLastEditTimeUtc], [JO_SystemLastEditUser])
			VALUES (@joPk, @orderPK, @lineNumber, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@joPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@orderPK", SqlDbType.UniqueIdentifier, orderHeaderPk);
				command.AddParameter("@lineNumber", SqlDbType.Int, lineNumber);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobComInvoiceLineTax(decimal baseValue, decimal amount, Guid invoiceLine, string type, int clusterKey)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobComInvoiceLineTax([JLT_PK], [JLT_BaseValue], [JLT_Amount], [JLT_JI], [JLT_Type], [JLT_ClusterKey])
			VALUES (@jltPk, @baseValue, @amount, @invoiceLine, @type, @clusterKey)
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jltPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@baseValue", SqlDbType.Decimal, baseValue);
				command.AddParameter("@amount", SqlDbType.VarChar, amount);
				command.AddParameter("@invoiceLine", SqlDbType.UniqueIdentifier, invoiceLine);
				command.AddParameter("@type", SqlDbType.VarChar, type);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobSupplierBooking(string bookingID, Guid bookingPartyPK, string status = null, string transportMode = null, Guid? cfdAddressPK = null)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobSupplierBooking([JSB_PK], [JSB_BookingID], [JSB_OH_BookingParty], [JSB_OA_CFSAddress], [JSB_Status], [JSB_LoadMode], [JSB_TransportMode], [JSB_SystemCreateTimeUtc], [JSB_SystemCreateUser], [JSB_SystemLastEditTimeUtc], [JSB_SystemLastEditUser])
			VALUES (@jsbPK, @bookingID, @bookingPartyPK, @cfdAddressPK, @status, 'CY', @transportMode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jsbPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@bookingID", SqlDbType.VarChar, bookingID);
				command.AddParameter("@bookingPartyPK", SqlDbType.UniqueIdentifier, bookingPartyPK);
				command.AddParameter("@status", SqlDbType.VarChar, status ?? "INC");
				command.AddParameter("@transportMode", SqlDbType.Char, transportMode ?? "SEA");
				command.AddParameter("@cfdAddressPK", SqlDbType.UniqueIdentifier, (object)cfdAddressPK ?? DBNull.Value);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCYContainerLoadListLine(Guid loadListHeaderPK, Guid bookingLinePK, Guid containerPK, Guid? packLinePK)
			=> CreateContainerLoadListLine(loadListHeaderPK, bookingLinePK, containerPK, packLinePK, "CY");

		public static Guid CreateCFSContainerLoadListLine(Guid loadListHeaderPK, Guid bookingLinePK, Guid containerPK, Guid? packLinePK)
			=> CreateContainerLoadListLine(loadListHeaderPK, bookingLinePK, containerPK, packLinePK, "CFS");

		public static Guid CreateContainerLoadListLine(Guid loadListHeaderPK, Guid bookingLinePK, Guid containerPK, Guid? packLinePK, string loadMode)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.ContainerLoadListLine(CLL_PK, CLL_AutoVersion, CLL_IsValid, CLL_CLH_LoadListHeader, CLL_JSL_BookingLine, CLL_JC_Container, CLL_JL_PackLine, CLL_LoadMode, CLL_SystemCreateTimeUtc, CLL_SystemCreateUser, CLL_SystemLastEditTimeUtc, CLL_SystemLastEditUser)
			VALUES(@CLL_PK, 0, 1, @CLL_CLH_LoadListHeader, @CLL_JSL_BookingLine, @CLL_JC_Container, @CLL_JL_PackLine, @CLL_LoadMode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CLL_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@CLL_CLH_LoadListHeader", SqlDbType.UniqueIdentifier, loadListHeaderPK);
				command.AddParameter("@CLL_JSL_BookingLine", SqlDbType.UniqueIdentifier, bookingLinePK);

				command.AddParameter("@CLL_JC_Container", SqlDbType.UniqueIdentifier, containerPK == Guid.Empty ? DBNull.Value : containerPK);
				command.AddParameter("@CLL_LoadMode", SqlDbType.VarChar, ContainerLoadListLineSchema.CLL_LoadMode.MaxLength, loadMode);
				if (packLinePK != null)
				{
					command.AddParameter("@CLL_JL_PackLine", SqlDbType.UniqueIdentifier, packLinePK);
				}
				else
				{
					command.AddParameter("@CLL_JL_PackLine", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateJobSupplierBookingLine(Guid orderLinePk, Guid supplierBookingPk, string bookingLineId)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.JobSupplierBookingLine([JSL_PK], [JSL_JO_OrderLine], [JSL_JSB_Booking], [JSL_BookingLineId], [JSL_SystemCreateTimeUtc], [JSL_SystemCreateUser], [JSL_SystemLastEditTimeUtc], [JSL_SystemLastEditUser])
			VALUES (@joPk, @linePK, @bookingPK, @bookingLineId, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@joPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@linePK", SqlDbType.UniqueIdentifier, orderLinePk);
				command.AddParameter("@bookingPK", SqlDbType.UniqueIdentifier, supplierBookingPk);
				command.AddParameter("@bookingLineId", SqlDbType.VarChar, bookingLineId);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateGlbPerson(string fullName)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser)
			VALUES (@PERPK, @PERFullName, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			";

			using var command = Db.Connection.Command(sql);	
			command.AddParameter("@PERPK", SqlDbType.UniqueIdentifier, result);
			command.AddParameter("@PERFullName", SqlDbType.VarChar, fullName);
			command.ExecuteNonQuery();

			return result;
		}

		public static Guid CreateGlbStaff(string staffCode, string staffFullName, bool isController = false, Guid? personPK = null)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_IsController, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
			VALUES (@GSPK, @GSCode, @GSFullName, @GSCode, @GSIsController, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@GSPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@GSCode", SqlDbType.VarChar, staffCode);
				command.AddParameter("@GSFullName", SqlDbType.VarChar, staffFullName);
				command.AddParameter("@GSIsController", SqlDbType.Bit, isController);
				command.ExecuteNonQuery();
			}

			if (personPK != null)
			{
				sql = @"
				UPDATE dbo.GlbStaff
				SET
					GS_PER = @GSPER,
					GS_SystemLastEditTimeUtc = GETUTCDATE(),
					GS_SystemLastEditUser = 'E'
				WHERE
					GS_Code = @GSCode
				";
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@GSCode", SqlDbType.VarChar, staffCode);
					command.AddParameter("@GSPER", SqlDbType.UniqueIdentifier, personPK);
					command.ExecuteNonQuery();
				}
			}

			return result;
		}
		public static Guid CreateGlbGroup(string groupCode, Guid? parentGroup, bool isSales)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.GlbGroup(GG_PK, GG_Code, GG_GG_ParentGroup, GG_IsSales, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser)
			VALUES (@GGPK, @GGCode, @GGParentGroup, @GGIsSales, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@GGPK", result, GlbGroupSchema.PK);
				command.AddParameterBasedOnDbColumn("@GGCode", groupCode, GlbGroupSchema.GG_Code);
				command.AddParameterBasedOnDbColumn("@GGParentGroup", parentGroup == null ? DBNull.Value : parentGroup, GlbGroupSchema.GG_GG_ParentGroup);
				command.AddParameterBasedOnDbColumn("@GGIsSales", isSales, GlbGroupSchema.GG_IsSales);

				command.ExecuteNonQuery();
			}
			return result;
		}

		public static Guid CreateGlbGroupLink(Guid groupPK, Guid staffPK)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.GlbGroupLink(GK_PK, GK_GG, GK_GS, GK_SystemCreateTimeUtc, GK_SystemCreateUser, GK_SystemLastEditTimeUtc, GK_SystemLastEditUser)
			VALUES (@GKPK, @GKGG, @GKGS, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@GKPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@GKGG", SqlDbType.UniqueIdentifier, groupPK);
				command.AddParameter("@GKGS", SqlDbType.UniqueIdentifier, staffPK);
				command.ExecuteNonQuery();
			}
			return result;
		}

		public static Guid CreateCYContainerLoadList(string loadListId, Guid supplierBookingPK, Guid loadListParty, string status = null)
			=> CreateContainerLoadList(loadListId, supplierBookingPK, loadListParty, status, "CY");

		public static Guid CreateCFSContainerLoadList(string loadListId, Guid supplierBookingPK, Guid loadListParty, string status = null)
			=> CreateContainerLoadList(loadListId, supplierBookingPK, loadListParty, status, "CFS");

		static Guid CreateContainerLoadList(string loadListId, Guid supplierBookingPK, Guid loadListParty, string status, string loadMode)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.ContainerLoadListHeader([CLH_PK], [CLH_LoadListId], [CLH_JSB_Booking], [CLH_Status], [CLH_OH_LoadListParty], [CLH_LoadMode], [CLH_PlannedTransportMode], [CLH_SystemCreateTimeUtc], [CLH_SystemCreateUser], [CLH_SystemLastEditTimeUtc], [CLH_SystemLastEditUser])
			VALUES (@clhPK, @loadListID, @supplierBookingPK, @status, @loadListParty, @loadMode, 'AIR', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clhPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@loadListID", SqlDbType.VarChar, loadListId);
				command.AddParameter("@supplierBookingPK", SqlDbType.UniqueIdentifier, supplierBookingPK);
				command.AddParameter("@loadListParty", SqlDbType.UniqueIdentifier, loadListParty);
				command.AddParameter("@status", SqlDbType.VarChar, status ?? "INC");
				command.AddParameter("@loadMode", SqlDbType.VarChar, ContainerLoadListHeaderSchema.CLH_LoadMode.MaxLength, loadMode);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCYContainerLoadListLine(Guid loadListHeaderPK, Guid supplierBookingLinePK, Guid jobContainerPK)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.ContainerLoadListLine([CLL_PK], [CLL_CLH_LoadListHeader], [CLL_JSL_BookingLine], [CLL_JC_Container], [CLL_LoadMode], [CLL_SystemCreateTimeUtc], [CLL_SystemCreateUser], [CLL_SystemLastEditTimeUtc], [CLL_SystemLastEditUser])
			VALUES (@cllPK, @loadListHeaderPK, @supplierBookingLinePK, @jobContainerPK, @loadMode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cllPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@loadListHeaderPK", SqlDbType.UniqueIdentifier, loadListHeaderPK);
				command.AddParameter("@supplierBookingLinePK", SqlDbType.UniqueIdentifier, supplierBookingLinePK);
				command.AddParameter("@jobContainerPK", SqlDbType.UniqueIdentifier, jobContainerPK);
				command.AddParameter("@loadMode", SqlDbType.VarChar, ContainerLoadListLineSchema.CLL_LoadMode.MaxLength, "CY");
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusTransportMeans(Guid parentID, string parentTableCode, string identificationNumber)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusTransportMeans([TPM_PK], [TPM_ParentID], [TPM_ParentTableCode], [TPM_IdentificationNumber], [TPM_SystemCreateTimeUtc], [TPM_SystemCreateUser], [TPM_SystemLastEditTimeUtc], [TPM_SystemLastEditUser])
			VALUES (@tpmPK, @parentID, @parentTableCode, @identificationNumber, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@tpmPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@identificationNumber", SqlDbType.VarChar, identificationNumber);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusInBondContainer(Guid parentID, string parentTableCode, string dataModel, string containerNum, string seal1 = "", string seal2 = "")
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusInBondContainer([BC_PK], [BC_ContainerNum], [BC_ParentID], [BC_ParentTableCode], [BC_DataModel], [BC_Seal1], [BC_Seal2], [BC_SystemCreateTimeUtc], [BC_SystemCreateUser], [BC_SystemLastEditTimeUtc], [BC_SystemLastEditUser])
			VALUES (@bcPK, @containerNum, @parentID, @parentTableCode, @dataModel, @seal1, @seal2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@bcPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.AddParameter("@containerNum", SqlDbType.VarChar, containerNum);
				command.AddParameter("@seal1", SqlDbType.VarChar, seal1);
				command.AddParameter("@seal2", SqlDbType.VarChar, seal2);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusSeal(Guid parentID, string parentTableCode, string sealNumber)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusSeal([BK_PK], [BK_SealNumber], [BK_ParentID], [BK_ParentTableCode], [BK_SystemCreateTimeUtc], [BK_SystemCreateUser], [BK_SystemLastEditTimeUtc], [BK_SystemLastEditUser])
			VALUES (@bkPK, @sealNumber, @parentID, @parentTableCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@bkPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@sealNumber", SqlDbType.VarChar, sealNumber);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusInbondBill(Guid b0_BH)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusInBondBill([B0_PK], [B0_BH], [B0_SystemCreateTimeUtc], [B0_SystemCreateUser], [B0_SystemLastEditTimeUtc], [B0_SystemLastEditUser])
			VALUES (@b0PK, @b0_BH, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@b0PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@b0_BH", SqlDbType.UniqueIdentifier, b0_BH);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusInBondCargoDesc(Guid parentID, string parentTableCode, string description, string unloadedState, decimal nettWeight, decimal grossWeight, string harmonisedTariff = "")
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusInBondCargoDesc([BY_PK], [BY_Description], [BY_ParentTableCode], [BY_ParentID], [BY_UnloadedState], [BY_NetWeight], [BY_NetWeightUnit], [BY_GrossWeight], [BY_GrossWeightUnit], [BY_HarmonisedTariff], [BY_SystemCreateTimeUtc], [BY_SystemCreateUser], [BY_SystemLastEditTimeUtc], [BY_SystemLastEditUser])
			VALUES (@byPK, @description, @parentTableCode, @parentID, @unloadedState, @nettWeight, 'KG', @grossWeight, 'KG', @harmonisedTariff, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@byPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@unloadedState", SqlDbType.VarChar, unloadedState);
				command.AddParameter("@nettWeight", SqlDbType.Decimal, nettWeight);
				command.AddParameter("@grossWeight", SqlDbType.Decimal, grossWeight);
				command.AddParameter("@harmonisedTariff", SqlDbType.VarChar, harmonisedTariff);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusInvPack(Guid parentID, string parentTableCode, string unitType, int unitCount)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.CusInvPack([B5_PK], [B5_UnitType], [B5_UnitCount], [B5_ParentID], [B5_ParentTableCode], [B5_SystemCreateTimeUtc], [B5_SystemCreateUser], [B5_SystemLastEditTimeUtc], [B5_SystemLastEditUser])
			VALUES (@b5PK, @unitType, @unitCount, @parentID, @parentTableCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@b5PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@unitType", SqlDbType.VarChar, unitType);
				command.AddParameter("@unitCount", SqlDbType.BigInt, unitCount);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateAsycudaManifestHeader(Guid branchPK, string jobReference, string country, int clusterKey, string applicationCode = null)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.AsycudaManifestHeader([AMA_PK], [AMA_GB], [AMA_JobReference], [AMA_RN_NKCountry], [AMA_ClusterKey], [AMA_SystemCreateTimeUtc], [AMA_SystemCreateUser], [AMA_SystemLastEditTimeUtc], [AMA_SystemLastEditUser], [AMA_ApplicationCode])
			VALUES(@amaPK, @branchPK, @jobReference, @country, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @applicationCode)
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@amaPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@jobReference", SqlDbType.VarChar, jobReference);
				command.AddParameter("@country", SqlDbType.Char, country);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, applicationCode ?? "NVC");
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateAsycudaBill(Guid ama_PK, int clusterKey)
		{
			var result = Guid.NewGuid();

			var sql = @"
			INSERT INTO dbo.AsycudaBill([ABL_PK], [ABL_AMA], [ABL_ClusterKey], [ABL_SystemCreateTimeUtc], [ABL_SystemCreateUser], [ABL_SystemLastEditTimeUtc], [ABL_SystemLastEditUser])
			VALUES(@ablPK, @ama_PK, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ablPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@ama_PK", SqlDbType.UniqueIdentifier, ama_PK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return result;
		}

		public static Guid CreateCusInbondFee(Guid parentID, bool isValid, decimal baseValue, string methodOfCalculation, decimal rate, decimal chargeAmount, string methodOfPayment, string rateOverrideReasonCode, string chargeType)
		{
			var result = Guid.NewGuid();

			var sql = @$"
			INSERT INTO dbo.CusInbondFee ([{CusInBondFeeSchema.PK.Name}], [{CusInBondFeeSchema.BFE_BY.Name}], [{CusInBondFeeSchema.BFE_IsValid.Name}], [{CusInBondFeeSchema.BFE_BaseValue.Name}], [{CusInBondFeeSchema.BFE_MethodOfCalculation.Name}],
			[{CusInBondFeeSchema.BFE_Rate.Name}], [{CusInBondFeeSchema.BFE_ChargeAmount.Name}], [{CusInBondFeeSchema.BFE_MethodOfPayment.Name}], [{CusInBondFeeSchema.BFE_RateOverrideReasonCode.Name}], [{CusInBondFeeSchema.BFE_SystemCreateTimeUtc.Name}], [{CusInBondFeeSchema.BFE_SystemCreateUser.Name}],
			[{CusInBondFeeSchema.BFE_SystemLastEditTimeUtc.Name}], [{CusInBondFeeSchema.BFE_SystemLastEditUser.Name}], [{CusInBondFeeSchema.BFE_ChargeType.Name}])
			VALUES(@BFE_PK, @BFE_BY, @BFE_IsValid, @BFE_BaseValue, @BFE_MethodOfCalculation, @BFE_Rate, @BFE_ChargeAmount, @BFE_MethodOfPayment, @BFE_RateOverrideReasonCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @BFE_ChargeType)
			";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BFE_PK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@BFE_BY", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@BFE_IsValid", SqlDbType.Bit, isValid);
				command.AddParameter("@BFE_BaseValue", SqlDbType.Decimal, CusInBondFeeSchema.BFE_BaseValue.MaxLength, CusInBondFeeSchema.BFE_BaseValue.Precision, CusInBondFeeSchema.BFE_BaseValue.Scale, baseValue);
				command.AddParameter("@BFE_MethodOfCalculation", SqlDbType.VarChar, CusInBondFeeSchema.BFE_MethodOfCalculation.MaxLength, methodOfCalculation);
				command.AddParameter("@BFE_Rate", SqlDbType.Decimal, CusInBondFeeSchema.BFE_Rate.MaxLength, CusInBondFeeSchema.BFE_Rate.Precision, CusInBondFeeSchema.BFE_Rate.Scale, rate);
				command.AddParameter("@BFE_ChargeAmount", SqlDbType.Money, chargeAmount);
				command.AddParameter("@BFE_MethodOfPayment", SqlDbType.VarChar, CusInBondFeeSchema.BFE_MethodOfPayment.MaxLength, methodOfPayment);
				command.AddParameter("@BFE_RateOverrideReasonCode", SqlDbType.VarChar, CusInBondFeeSchema.BFE_RateOverrideReasonCode.MaxLength, rateOverrideReasonCode);
				command.AddParameter("@BFE_ChargeType", SqlDbType.VarChar, CusInBondFeeSchema.BFE_ChargeType.MaxLength, chargeType);
				command.ExecuteNonQuery();
			}
			return result;
		}
	}
}
