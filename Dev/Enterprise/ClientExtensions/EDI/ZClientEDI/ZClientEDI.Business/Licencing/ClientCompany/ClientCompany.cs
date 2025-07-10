using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Licencing.Business
{
	[CodeProperty(ClientCompany.Schema.LCC_Code), DescriptionProperty(ClientCompany.Schema.RelatedOrgName)]
	public class ClientCompany : AutoClientCompany
	{
		public new class Schema : AutoClientCompany.Schema
		{
			public const string LicenceCode = "LicenceCode";
			public const string RelatedOrgName = "RelatedOrgName";
		}

		public ClientCompany(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => propertyInfo.Name == nameof(LCC_OH);

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		internal class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(ClientCompany company)
				: base(company)
			{
				this.company = company;
			}
			readonly ClientCompany company;

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				foreach (var column in columns)
				{
					if (column.ColumnName.StartsWith("Org", StringComparison.Ordinal) && !company.LCC_OH.IsEmpty)
					{
						Factory.AddFetchHint(OrgHeaderSchema.PK, company.LCC_OH);
					}
				}
			}
		}

		#endregion

		static readonly MutexID ImportClientCompanyMutexID = new MutexID("ImportClientCompany", "Ensure same client company only be imported once.");

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase)
			{
				if (LCC_CreateTimeUtc.IsEmpty)
				{
					LCC_CreateTimeUtc = ZDateTime.UtcNow;
				}
				if (LCC_CodeValidFromUtc.IsEmpty)
				{
					LCC_CodeValidFromUtc = LCC_CreateTimeUtc;
				}

				new EdiCommissionAgreementCustomizationAutoAdder(Factory).Execute(this);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static ZGlobalMutex CreateAndLockImportMutex(ZGuid databasePk, int maxTries = 3, int millisecondsBetweenTries = 5000)
		{
			ZGlobalMutex result = null;
			int failureCount = 0;
			do
			{
				ZGlobalMutex mutex = new ZGlobalMutex(ClientCompany.ImportClientCompanyMutexID, databasePk.ToString());
				try
				{
					if (mutex.Lock())
					{
						result = mutex;
						mutex = null;
						break;
					}
				}
				catch (SqlLockLostException)
				{
				}
				finally
				{
					if (mutex != null)
					{
						try
						{
							((IDisposable)mutex).Dispose();
						}
						catch (Exception ex) when (!ex.IsCriticalException()) { }
					}
				}

				failureCount++;
				if (failureCount < maxTries)
				{
					Thread.Sleep(millisecondsBetweenTries);
				}
			}
			while (failureCount < maxTries);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static bool Sync(BusinessObjectFactory factory, LicenceDatabase database, ZDateTime reportTimeUtc, List<EDIVersionReport.EdiCompanyReport> companyReports,
			int maxTries = 3, int millisecondsBetweenTries = 5000)
		{
			var mutex = CreateAndLockImportMutex(database.PK, maxTries, millisecondsBetweenTries);
			if (mutex == null)
			{
				return false;
			}

			using (mutex)
			{
				SyncInternal(factory, database, reportTimeUtc, companyReports);
			}

			return true;
		}

		void PopulateFrom(EDIVersionReport.EdiCompanyReport reportCompany, ZDateTime reportTimeUtc)
		{
			if (reportCompany.IsActive)
			{
				LCC_DeactivateTimeUtc = ZDateTime.Empty;
			}
			else if (LCC_DeactivateTimeUtc.IsEmpty)
			{
				LCC_DeactivateTimeUtc = reportTimeUtc;
			}

			LCC_Address1 = reportCompany.Address1;
			LCC_Address2 = reportCompany.Address2;
			LCC_BusinessRegNo = reportCompany.BusinessRegNo;
			LCC_BusinessRegNo2 = reportCompany.BusinessRegNo2;
			LCC_City = reportCompany.City;
			if (reportCompany.PK != Guid.Empty)
			{
				LCC_ClientPK = reportCompany.PK;
			}
			if (LCC_Code != reportCompany.Code)
			{
				LCC_Code = reportCompany.Code;
				LCC_CodeValidFromUtc = reportTimeUtc;
			}
			LCC_CustomsRegistrationNo = reportCompany.CustomsRegistrationNo;
			LCC_IsGSTCashBasis = reportCompany.IsGSTCashBasis;
			LCC_IsGSTRegistered = reportCompany.IsGSTRegistered;
			LCC_IsReciprocal = reportCompany.IsReciprocal;
			LCC_IsWHTCashBasis = reportCompany.IsWHTCashBasis;
			LCC_IsWHTRegistered = reportCompany.IsWHTRegistered;
			LCC_Name = reportCompany.Name;
			LCC_Phone = reportCompany.Phone;
			LCC_PostCode = reportCompany.PostCode;
			LCC_RN_NKCountryCode = reportCompany.CountryCode;
			LCC_RX_NKLocalCurrency = reportCompany.CurrencyCode;
			LCC_State = reportCompany.State;
			LCC_WebAddress = reportCompany.WebAddress;
		}

		public class CodeHistory
		{
			public CodeHistory(Guid clientCompanyPk, string code, DateTime validFromUtc)
			{
				ClientCompanyPk = clientCompanyPk;
				Code = code;
				ValidFromUtc = validFromUtc;
			}

			public Guid ClientCompanyPk { get; private set; }
			public string Code { get; private set; }
			public DateTime ValidFromUtc { get; private set; }
		}

		public static List<CodeHistory> LoadOldCodes(Guid databasePk)
		{
			var result = new List<CodeHistory>();

			const string sql = @"select CCH_LCC, CCH_Code, CCH_CodeValidFromUtc from dbo.ClientCompanyCodeHistory join dbo.ClientCompany on CCH_LCC = LCC_PK where LCC_LD = @DatabasePk";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@DatabasePk", SqlDbType.UniqueIdentifier, databasePk);
				using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var pk = reader.GetGuid(0);
						var code = reader.GetString(1);
						var validFromUtc = reader.GetDateTime(2);
						result.Add(new CodeHistory(pk, code, validFromUtc));
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal static void SyncInternal(BusinessObjectFactory factory, LicenceDatabase database, ZDateTime reportTimeUtc, List<EDIVersionReport.EdiCompanyReport> companyReports)
		{
			var query = new ZQuery(ClientCompanySchema.LCC_LD, database.PK);
			query.OrderBy = ClientCompanySchema.Constants.LCC_OH;
			var dbCompanies = factory.Load<ClientCompany>(query);
			var existing = dbCompanies.ToArray();
			var existingOrgPKs = LoadExistingOrgPks(companyReports.Where(x => x.OrgPKThatGeneratedThisLicence != Guid.Empty).Select(x => x.OrgPKThatGeneratedThisLicence));
			var newCompanies = new List<ClientCompany>();
			var codesInUse = new HashSet<string>(dbCompanies.Select(x => x.LCC_Code.ToString()));
			var newCodeToCompany = new Dictionary<string, ClientCompany>();
			var reportPks = new HashSet<Guid>(companyReports.Select(x => x.PK));

			bool hasChanges = false;
			foreach (var reportCompany in companyReports.Where(x => !string.IsNullOrEmpty(x.Code)))
			{
				codesInUse.Add(reportCompany.Code);
				ClientCompany matchByPk = null;
				ClientCompany matchByCode = null;
				ClientCompany matchByOrgPk = null;
				int matchByPkIndex = -1;
				int matchByCodeIndex = -1;
				int matchByOrgPkIndex = -1;

				for (int i = 0; i < existing.Length; ++i)
				{
					var elem = existing[i];
					if (elem != null)
					{
						if (!elem.LCC_ClientPK.IsEmpty && elem.LCC_ClientPK == reportCompany.PK)
						{
							matchByPk = elem;
							matchByPkIndex = i;
						}

						if (elem.LCC_Code == reportCompany.Code)
						{
							matchByCode = elem;
							matchByCodeIndex = i;
						}

						if (!elem.LCC_OH.IsEmpty && elem.LCC_OH == reportCompany.OrgPKThatGeneratedThisLicence)
						{
							matchByOrgPk = elem;
							matchByOrgPkIndex = i;
						}
					}
				}

				ClientCompany clientCompany = matchByPk ?? matchByCode ?? matchByOrgPk;

				if (clientCompany == null)
				{
					clientCompany = factory.New<ClientCompany>();
					clientCompany.LCC_CreateTimeUtc = ZDateTime.UtcNow;
					clientCompany.LCC_LD = database.PK;
					newCompanies.Add(clientCompany);
					hasChanges = true;
				}
				else if (matchByPk != null)
				{
					if (clientCompany.LCC_Code != reportCompany.Code)
					{
						clientCompany.EnsureCodeIsInHistory(clientCompany.LCC_Code, clientCompany.LCC_CodeValidFromUtc);

						if (matchByCode != null && matchByCodeIndex != matchByPkIndex
							&& (matchByCode.LCC_ClientPK.IsEmpty || !reportPks.Contains(matchByCode.LCC_ClientPK.ToGuid())))
						{
							// Edge case 1: user changes company code and sends an eRequest before there's a version report
							//	or sync failed previously and the company was created by LicenceUsage processor without a ClientPK.
							// We create a ClientCompany for the new code without a ClientPK
							// The VersionReport needs to merge the new ClientCompany into the old one.
							// Case 2: company was created, then removed by restoring DB and the code is now on another company
							// Merge the now dead company into the live company with the same code
							matchByCode.MergeInto(matchByPk);
							existing[matchByCodeIndex] = null;
						}
					}
				}
				else if (matchByCode == null && matchByOrgPk != null)
				{
					// code must have changed on LicenceKey
					clientCompany.EnsureCodeIsInHistory(clientCompany.LCC_Code, clientCompany.LCC_CodeValidFromUtc);
					existing[matchByOrgPkIndex] = null;
				}

				if (matchByPkIndex >= 0)
				{
					existing[matchByPkIndex] = null;
				}
				else if (matchByCodeIndex >= 0)
				{
					existing[matchByCodeIndex] = null;
				}

				if (reportCompany.OrgPKThatGeneratedThisLicence != Guid.Empty
					&& existingOrgPKs.Contains(reportCompany.OrgPKThatGeneratedThisLicence))
				{
					if (matchByOrgPk != null && matchByCode != null && matchByCodeIndex != matchByOrgPkIndex)
					{
						// Matches exist on two different ClientCompanies.
						// Match on the code and don't set the OrgPK since we want only one record per DB and OrgPK
					}
					else
					{
						clientCompany.LCC_OH = reportCompany.OrgPKThatGeneratedThisLicence;
					}
				}

				clientCompany.PopulateFrom(reportCompany, reportTimeUtc);
				if (!newCodeToCompany.ContainsKey(clientCompany.LCC_Code))
				{
					newCodeToCompany.Add(clientCompany.LCC_Code, clientCompany);
				}
				hasChanges |= clientCompany.HasChanges;

				if (clientCompany.IsInDatabase
					&& clientCompany.LCC_DeactivateTimeUtc.IsEmpty
					&& database.LD_LicenceType == DatabaseTypes.Codes.Production)
				{
					clientCompany.EnsureActiveStatusIsInHistory(reportTimeUtc);
				}
			}

			foreach (var unmatched in existing)
			{
				if (unmatched != null && !unmatched.IsDeleted && unmatched.LCC_DeactivateTimeUtc.IsEmpty)
				{
					unmatched.LCC_DeactivateTimeUtc = reportTimeUtc;
					hasChanges = true;
				}
			}

			if (hasChanges)
			{
				// Check for clash between new and old codes, e.g., when two companies swap codes.
				// Need to save in two steps with a temporary code in between.
				var clashCompanyToFinalCode = new Dictionary<ClientCompany, string>();
				foreach (var companyInDb in dbCompanies.Where(x => !x.IsDeleted))
				{
					var codeInDb = (ZString)companyInDb.LCC_CodeInfo.OriginalValue;
					if (newCodeToCompany.TryGetValue(codeInDb, out var codeClash) && codeClash.PK != companyInDb.PK)
					{
						clashCompanyToFinalCode.Add(codeClash, codeClash.LCC_Code);
						codeClash.LCC_Code = GenerateUniqueCode(codesInUse);
					}
				}

				if (clashCompanyToFinalCode.Count == 0)
				{
					factory.Save();
				}
				else
				{
					using (var transactionManager = ((ITransactionParticipant)factory).BeginTransactionWithManager())
					{
						factory.Save();
						foreach (var pair in clashCompanyToFinalCode)
						{
							pair.Key.LCC_Code = pair.Value;
						}
						factory.Save();
						transactionManager.CommitTransaction();
					}
				}
			}
		}

		public static string GenerateUniqueCode(HashSet<string> codesInUse)
		{
			const int N = 36;
			int i = 0;
			while (i < N * N * N)
			{
				var code = IntToCode(i);
				if (!codesInUse.Contains(code))
				{
					codesInUse.Add(code);
					return code;
				}
				++i;
			}

			throw new InvalidOperationException();
		}

		static string IntToCode(int i)
		{
			const int N = 36;
			return new string(new[]
			{
				Int36ToChar(i / (N * N)),
				Int36ToChar((i / N) % N),
				Int36ToChar(i % N)
			});
		}

		static char Int36ToChar(int i)
		{
			if (i < 10)
			{
				return Convert.ToChar('0' + i);
			}
			else
			{
				return Convert.ToChar('A' + i - 10);
			}
		}

		static HashSet<Guid> LoadExistingOrgPks(IEnumerable<Guid> orgPksToCheckForExistence)
		{
			var result = new HashSet<Guid>();
			if (orgPksToCheckForExistence.Any())
			{
				using (var dataTable = new DataTable())
				{
					dataTable.Locale = CultureInfo.InvariantCulture;
					dataTable.Columns.Add("Value", typeof(Guid));

					foreach (var item in orgPksToCheckForExistence)
					{
						dataTable.Rows.Add(new object[] { item });
					}

					using (var cmd = Db.Connection.Command("select OH_PK from dbo.OrgHeader where OH_PK in (select Value from @PkList)"))
					{
						cmd.AddTableValuedParameter("@PkList", TVPHelper.TVP_uniqueidentifier, dataTable);
						using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							while (reader.Read())
							{
								result.Add(reader.GetGuid(0));
							}
						}
					}
				}
			}

			return result;
		}

		void MergeInto(ClientCompany dest)
		{
			using (var cmd = Db.Connection.Command("dbo.EdiClientCompanyMerge"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@OldPk", SqlDbType.UniqueIdentifier, PK.ToGuid());
				cmd.AddParameter("@NewPk", SqlDbType.UniqueIdentifier, dest.PK.ToGuid());

				cmd.ExecuteNonQuery();
			}

			if (!LCC_OH.IsEmpty && dest.LCC_OH.IsEmpty)
			{
				dest.LCC_OH = LCC_OH;
			}

			this.Delete();
		}

		internal void EnsureCodeIsInHistory(string code, ZDateTime validFromUtc)
		{
			const string sql = @"
if @Code != ISNULL((select top 1 CCH_Code from dbo.ClientCompanyCodeHistory where CCH_LCC = @LCC_PK and CCH_CodeValidFromUtc <= @ValidFromUtc order by CCH_CodeValidFromUtc desc), '')
	insert dbo.ClientCompanyCodeHistory(CCH_LCC, CCH_Code, CCH_CodeValidFromUtc)
	values (@LCC_PK, @Code, @ValidFromUtc)
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@LCC_PK", SqlDbType.UniqueIdentifier, PK.ToGuid());
				cmd.AddParameter("@Code", SqlDbType.VarChar, code);
				cmd.AddParameter("@ValidFromUtc", SqlDbType.DateTime, validFromUtc.ToSmallDateTimeFloor().ToDateTime());

				cmd.ExecuteNonQuery();
			}
		}

		void EnsureActiveStatusIsInHistory(ZDateTime reportTimeUtc)
		{
			var firstDayOfMonth = new ZDateTime(reportTimeUtc.Year, reportTimeUtc.Month, 1, reportTimeUtc.Kind);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			//only include reports that are at least 2 days from the beginning/end of month.
			if (reportTimeUtc >= firstDayOfMonth.AddDays(2) && reportTimeUtc < lastDayOfMonth.AddDays(-1))
			{
				// No need to convert UTC to Sydney time since we're 2 days inside the month
				var billingPeriod = reportTimeUtc.Year * 100 + reportTimeUtc.Month;

				const string sql = @"
if not exists(select top 1 CSH_LCC from dbo.ClientCompanyActiveStatusHistory where CSH_LCC = @LCC_PK and CSH_Period = @Period)
	insert dbo.ClientCompanyActiveStatusHistory(CSH_LCC, CSH_Period)
	values (@LCC_PK, @Period);
";
				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.AddParameter("@LCC_PK", SqlDbType.UniqueIdentifier, PK.ToGuid());
					cmd.AddParameter("@Period", SqlDbType.Int, billingPeriod);

					cmd.ExecuteNonQuery();
				}
			}
		}

		public static ClientCompany FindClosestMatch(BusinessObjectFactory factory, LicenceHeader licHeader)
		{
			var query = new ZDBOnlyQuery(typeof(ClientCompany));

			// prefer in order
			// - same org
			// - active and same country
			// - active
			// - inactive and same country
			// - first created
			ZString sql = @"
LCC_PK IN
(
	SELECT top 1 LCC_PK
	FROM
		dbo.ClientCompany
	WHERE
		LCC_LD = @LD_PK
	ORDER BY
		case when LCC_OH is not null and LCC_OH = @LC_OH then 0 else 1 end,
		case when LCC_OH is null and LCC_RN_NKCountryCode != '' and LCC_RN_NKCountryCode = @LC_CompanyCountry and LCC_DeactivateTimeUtc is null then 0 else 1 end,
		case when LCC_DeactivateTimeUtc is null then 0 else 1 end,
		case when LCC_RN_NKCountryCode != '' and LCC_RN_NKCountryCode = @LC_CompanyCountry then 0 else 1 end,
		LCC_CreateTimeUtc

);";
			ZSqlParameterCollection paramList = new ZSqlParameterCollection();
			paramList.Add("@LA_PK", licHeader.PK, LicenceHeaderSchema.PK);
			paramList.Add("@LD_PK", licHeader.LA_LD, LicenceHeaderSchema.LA_LD);
			paramList.Add("@LC_OH", licHeader.Company.LC_OH, LicenceCompanySchema.LC_OH);
			paramList.Add("@LC_CompanyCountry", licHeader.Company.LC_CompanyCountry, LicenceCompanySchema.LC_CompanyCountry);
			query.AddFilterAndZSQLParameterCollection(sql, paramList);

			return factory.LoadTop1<ClientCompany>(query);
		}

		public static ClientCompany FindOrCreate(BusinessObjectFactory factory,
			ZString companyCode,
			ZGuid databasePk,
			ZGuid orgPk,
			string name,
			string countryCode)
		{
			var query = new ZQuery(ClientCompanySchema.LCC_LD, databasePk);
			var codeOrPkQuery = new ZQuery(ClientCompanySchema.LCC_Code, companyCode);
			if (!orgPk.IsEmpty)
			{
				codeOrPkQuery.AddToFilter(JoinCondition.Or, ClientCompanySchema.LCC_OH, orgPk);
			}
			query.AddToFilter(codeOrPkQuery, JoinCondition.And);

			var clientCompanyList = factory.Load<ClientCompany>(query);
			ClientCompany clientCompany = null;
			if (clientCompanyList.Length > 0)
			{
				clientCompany = clientCompanyList
					.OrderBy(x => x.LCC_Code == companyCode ? 0 : 1)
					.ThenBy(x => x.LCC_OH == orgPk && !orgPk.IsEmpty ? 0 : 1)
					.First();
			}

			if (clientCompany == null)
			{
				var mutex = CreateAndLockImportMutex(databasePk);
				if (mutex != null)
				{
					using (mutex)
					{
						var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
						clientCompany = newFactory.LoadTop1<ClientCompany>(query);
						if (clientCompany == null)
						{
							clientCompany = newFactory.New<ClientCompany>();
							clientCompany.LCC_Code = companyCode;
							clientCompany.LCC_LD = databasePk;
							clientCompany.LCC_OH = orgPk;
							clientCompany.LCC_Name = name ?? ZString.Empty;
							clientCompany.LCC_RN_NKCountryCode = countryCode ?? ZString.Empty;

							newFactory.Save();
							clientCompany = (ClientCompany)factory.ImportFromAnotherFactory(clientCompany);
						}
					}
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(name) && clientCompany.LCC_Name != name)
				{
					clientCompany.LCC_Name = name;
				}

				if (!string.IsNullOrEmpty(countryCode) && clientCompany.LCC_RN_NKCountryCode != countryCode)
				{
					clientCompany.LCC_RN_NKCountryCode = countryCode;
				}
			}

			return clientCompany;
		}

		#region Load from code

		public static ClientCompany LoadFromLicenceCode(BusinessObjectFactory factory, string enterpriseCode, string companyCode, string serverCode)
		{
			var query = new ZDBOnlyQuery(typeof(ClientCompany));
			query.AddToFilter(ClientCompanySchema.LCC_Code, companyCode);
			var dbQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), ClientCompanySchema.LCC_LD);
			dbQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);
			var entQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
			entQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);
			dbQuery.AddSubQuery(entQuery, JoinCondition.And);
			query.AddSubQuery(dbQuery, JoinCondition.And);

			return factory.LoadTop1<ClientCompany>(query);
		}

		#endregion

		#region Properties

		#region LCC_LD

		[RelatedBusinessObject("Database")]
		[List("Lookups.Databases")]
		public override ZGuid LCC_LD
		{
			get { return base.LCC_LD; }
			set { base.LCC_LD = value; }
		}

		public LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(LCC_LD); }
		}

		#endregion

		#region Org

		public EDIOrgHeader Org
		{
			get { return Factory.Load<EDIOrgHeader>(LCC_OH); }
		}

		[List("Lookups.Organisations")]
		public override ZGuid LCC_OH
		{
			get { return base.LCC_OH; }
			set
			{
				if (base.LCC_OH != value)
				{
					base.LCC_OH = value;
					licHeaderLoaded = false;
				}
			}
		}

		#endregion

		#region Licence Code

		public ZString LicenceCode
		{
			get
			{
				var database = Database;
				return database.EnterpriseCode + LCC_Code + database.LD_ServerCode;
			}
		}

		#endregion

		#region Local Times

		public ZDateTime CreateTimeLocal
		{
			get { return LCC_CreateTimeUtc.ToLocalBranchTime();  }
		}

		public ZDateTime DeactivateTimeLocal
		{
			get { return LCC_DeactivateTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region RelatedOrgName

		public ZString RelatedOrgName
		{
			get
			{
				var licence = UsageOwnerLicence;

				var countryCode = LCC_RN_NKCountryCode;
				if (countryCode.IsEmpty && licence != null)
				{
					countryCode = licence.Company.LC_CompanyCountry;
				}

				var orgName = ZString.Empty;
				if (!LCC_Name.IsEmpty)
				{
					orgName = LCC_Name;
				}
				else if (Org != null)
				{
					orgName = Org.OH_FullName;
				}
				else if (licence != null)
				{
					orgName = licence.Company.Header.OH_FullName;
				}

				return "[" + countryCode + "] " + orgName;
			}
		}

		#endregion

		#endregion

		/// <summary>
		/// Return the Licence that owns the usage for a ClientCompany.
		/// Same logic as SQL ViewClientCompanyLicence 
		/// <summary>
		public LicenceHeader UsageOwnerLicence
		{
			get
			{
				return LicHeader ?? Database.UsageOwnerOrFirstLicence;
			}
		}

		/// <summary>
		/// Licence directly linked to the organization (if not null) and database
		/// </summary>
		public LicenceHeader LicHeader
		{
			get
			{
				if (!licHeaderLoaded)
				{
					licHeaderLoaded = true;
					licHeader = null;
					var owner = Org;
					if (owner != null && owner.LicCompany != null)
					{
						var licCompany = owner.LicCompany;
						licHeader = Database.LicHeadersForAllCompanies.Cast<LicenceHeader>().FirstOrDefault(x => x.LA_LC == licCompany.PK);
					}
				}

				return licHeader;
			}
		}
		LicenceHeader licHeader;
		bool licHeaderLoaded;

		public ZString LicenceEdition
		{
			get
			{
				var licence = LicHeader;
				return licence != null ? licence.Edition : ZString.Empty;
			}
		}

		public ZString IsLiveAsText
		{
			get
			{
				var licence = LicHeader;
				return licence != null ? (licence.IsLive ? "Y" : "N") : string.Empty;
			}
		}

		public ClientBranchCollection ClientBranches
		{
			get
			{
				if (clientBranches == null)
				{
					clientBranches = new ClientBranchCollection(Factory, new ZQuery(ClientBranchSchema.LCB_LD, LCC_LD)
						.AddToFilter(ClientBranchSchema.LCB_LCC_Code, SQLComparisonOperator.Equal, LCC_Code));
				}

				return clientBranches;
			}
		}

		ClientBranchCollection clientBranches;
	}
}

