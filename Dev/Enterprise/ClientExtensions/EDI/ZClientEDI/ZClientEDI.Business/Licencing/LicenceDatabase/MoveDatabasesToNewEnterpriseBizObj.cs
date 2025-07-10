using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class MoveDatabasesToNewEnterpriseBizObj : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MoveDatabasesToNewEnterpriseBizObj(BusinessObjectFactory factory, IEnumerable<ZGuid> databasePKs, ILogger logger)
			: base(factory)
		{
			DatabasePKs = databasePKs;
			Logger = logger;
			DefaultEnterprise = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value));
		}

		readonly IEnumerable<ZGuid> DatabasePKs;
		readonly ILogger Logger;
		readonly LicenceEnterprise DefaultEnterprise;

		public LicenceEnterpriseCollection LicenceEnterpriseList
		{
			get
			{
				if (fLicenceEnterpriseList == null)
				{
					fLicenceEnterpriseList = DefaultEnterprise != null ? new LicenceEnterpriseCollection(Factory, new ZQuery(LicenceEnterpriseSchema.PK, SQLComparisonOperator.NotEqual, DefaultEnterprise.PK)) : new LicenceEnterpriseCollection(Factory);
				}
				return fLicenceEnterpriseList;
			}
		}

		LicenceEnterpriseCollection fLicenceEnterpriseList;

		[BusinessObjectTestExclude]
		[MaxLength(7)]
		[ResourceStringData("MoveDatabasesToNewEnterpriseBizObj.Enterprise", Caption = "Enterprise")]
		public ZString LicenceEnterpriseID
		{
			get => LicenceEnterpriseInternal?.LE_EnterpriseID ?? "";
			set
			{
				var result = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, value));
				if (result != null)
				{
					LicenceEnterpriseInternal = result;
				}
				else
				{
					LicenceEnterpriseInternal = null;
					Logger.Error("Invalid EnterpriseID");
				}
				ValidateLicenceEnterpriseID();
				LicenceEnterpriseIDInfo.RefreshBinding();
			}
		}

		LicenceEnterprise LicenceEnterpriseInternal;

		public ZPropertyInfo LicenceEnterpriseIDInfo => GetZPropertyInfo(nameof(LicenceEnterpriseID));

		#region Flags

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AllowWebAutoLogin = true;
			RegistrationStatus = DatabaseStatusList.Codes.REG;
		}

		[ResourceStringData("MoveDatabasesToNewEnterpriseBizObj.AllowWebAutoLogin", Caption = "Allow Web Auto Login")]
		public ZBool AllowWebAutoLogin
		{
			get => allowWebAutoLogin;
			set => SetNonPersistentPropertyValue(AllowWebAutoLoginInfo, ref allowWebAutoLogin, value);
		}

		ZBool allowWebAutoLogin;

		public ZPropertyInfo AllowWebAutoLoginInfo => GetZPropertyInfo(nameof(AllowWebAutoLogin));

		[ResourceStringData("MoveDatabasesToNewEnterpriseBizObj.RegistrationStatus", Caption = "Registration")]
		[List("RegistrationStatusList")]
		[MaxLength(AutoLicenceDatabase.Schema.LD_StatusMaxLength)]
		public ZString RegistrationStatus
		{
			get => registrationStatus;
			set
			{
				SetNonPersistentPropertyValue(RegistrationStatusInfo, ref registrationStatus, value);
				ValidateRegistrationStatus();
			}
		}

		public CodeDescriptionPairList RegistrationStatusList { get; } = new DatabaseStatusList();

		ZString registrationStatus;

		public ZPropertyInfo RegistrationStatusInfo => GetZPropertyInfo(nameof(RegistrationStatus));

		protected override void RunPreSaveValidationCore()
		{
			ValidateLicenceEnterpriseID();
			ValidateRegistrationStatus();
			base.RunPreSaveValidationCore();
		}

		void ValidateLicenceEnterpriseID()
		{
			LicenceEnterpriseIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LicenceEnterpriseIDInfo);
		}

		void ValidateRegistrationStatus()
		{
			RegistrationStatusInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RegistrationStatusInfo);
			ListValidation.ErrorIfInvalidCode(RegistrationStatusInfo);
		}

		#endregion Flags

		public void MoveDatabasesToNewEnterprise()
		{
			if (DefaultEnterprise == null)
			{
				Logger.Error($"Invalid Default Enterprise (Registry -> {EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Caption})");
				return;
			}

			if (LicenceEnterpriseInternal == null || LicenceEnterpriseInternal.PK == DefaultEnterprise.PK)
			{
				Logger.Error("Invalid EnterpriseID");
				return;
			}

			var databases = Factory.Load<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.PK, DatabasePKs));
			var invalidDatabases = databases.Where(x => x.LD_TenantID.IsEmpty || x.IsEnterpriseFamilyDatabase
					|| x.LD_LE != DefaultEnterprise.PK || x.LicHeadersForAllCompanies.Any());

			if (invalidDatabases.Any())
			{
				var sb = new ZStringBuilder();
				sb.AppendLine(@"The following database(s) don't meet the requirements, you might have to move them manually.

Please use the filter 'Databases Not registered with an Tenant ID' to list applicable databases.

Operation terminated, no databases have been moved.
");
				sb.AppendLine("ServerCode\tProduct\tTenantID");
				foreach (var db in invalidDatabases.OrderBy(x => x.LD_ServerCode).ThenBy(x => x.LD_Product).ThenBy(x => x.LD_TenantID))
				{
					sb.AppendLine(FormattableString.Invariant($"{db.LD_ServerCode}\t\t{db.LD_Product}\t{db.LD_TenantID}"));
				}

				Logger.Error(sb.ToString());
				return;
			}

			try
			{
				var serverCodes = GetServerCodes(LicenceEnterpriseInternal.PK);
				foreach (var db in databases)
				{
					db.LD_LE = LicenceEnterpriseInternal.PK;
					db.LD_OH_WebAccessOrg = LicenceEnterpriseInternal.LE_OH;
					LicenceEnterpriseInternal.Organisation.LicCompany?.LicDatabases.Add(db); //Org > Licence > Databases > Add
					db.LD_Status = RegistrationStatus;
					db.LD_AllowAutoLogin = AllowWebAutoLogin;

					if (serverCodes.Contains(db.LD_ServerCode))
					{
						db.LD_ServerCode = GetNextServerCode(serverCodes);
					}

					serverCodes.Add(db.LD_ServerCode);
				}
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				Logger.Error("Operation was unsuccessful, please try again later.", ex);
				return;
			}

			Logger.Information("Operation was successful, please reopen all related forms to view the changes.");
		}

		static string GetNextServerCode(HashSet<string> codes)
		{
			foreach (var i in Enumerable.Range(729, 27 * 27 * 27))
			{
				var newCode = Base27Encoding.Encode(i);
				if (!codes.Contains(newCode))
				{
					return newCode;
				}
			}

			throw new InvalidOperationException("Can not get next ServerCode.");
		}

		static HashSet<string> GetServerCodes(ZGuid enterprisePK)
		{
			var sql = "SELECT LD_ServerCode FROM dbo.LicenceDatabase WHERE LD_LE = @LE";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@LE", SqlDbType.UniqueIdentifier, enterprisePK.ToGuid());
				using (var dt = DataUtils.GetDataTableFromCommand(cmd))
				{
					return dt.Rows.OfType<DataRow>().Select(x => x[0].ToString()).ToHashSet();
				}
			}
		}
	}
}
