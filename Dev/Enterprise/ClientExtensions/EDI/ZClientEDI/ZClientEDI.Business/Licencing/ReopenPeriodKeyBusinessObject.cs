using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class ReopenPeriodKeyBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ReopenPeriodKeyBusinessObject(EDIOrgHeader organisation)
			: base(new BusinessObjectFactory())
		{
			Organisation = Factory.Load<EDIOrgHeader>(organisation.PK);

			var defaultDatabase = Organisation.LicCompany.ActiveOrAllLicDatabases.Cast<LicenceDatabase>().FirstOrDefault(x => x.LD_LicenceType == DatabaseTypes.Codes.Production);
			if (defaultDatabase != null)
			{
				DatabaseServerCode = defaultDatabase.LD_ServerCode;
				var defaultCompany = ClientCompanyList.FirstOrDefault(x => x.LCC_OH == Organisation.PK);
				if (defaultCompany != null)
				{
					ClientCompanyCode = defaultCompany.LCC_Code;
				}
			}
		}

		#region Organisation

		readonly EDIOrgHeader Organisation;

		#endregion

		#region UserStaffCode

		[MaxLength(3)]
		public ZString UserStaffCode
		{
			get { return fUserStaffCode; }
			set
			{
				if (fUserStaffCode != value)
				{
					SetNonPersistentPropertyValue(UserStaffCodeInfo, ref fUserStaffCode, value);
					ValidateUserStaffCode();
				}
			}
		}
		ZString fUserStaffCode;

		public ZPropertyInfo UserStaffCodeInfo
		{
			get { return GetZPropertyInfo(nameof(UserStaffCode)); }
		}

		public void ValidateUserStaffCode()
		{
			UserStaffCodeInfo.ClearAllNotifications();
			if (UserStaffCode.Length < 1)
			{
				UserStaffCodeInfo.AddError("UserStaffCode must be between 1 and 3 characters long inclusive.");
			}
		}

		#endregion

		#region Period

		[MaxLength(6)]
		public ZString Period
		{
			get
			{
				return fPeriod;
			}
			set
			{
				if (fPeriod != value)
				{
					SetNonPersistentPropertyValue(PeriodInfo, ref fPeriod, value);
					ValidatePeriod();
				}
			}
		}
		ZString fPeriod;

		public ZPropertyInfo PeriodInfo
		{
			get { return GetZPropertyInfo(nameof(Period)); }
		}

		public void ValidatePeriod()
		{
			PeriodInfo.ClearAllNotifications();
			if (Period.Length != 6)
			{
				PeriodInfo.AddError("Period must be 6 digits long.");
			}
			int result;
			if (!int.TryParse(Period, out result))
			{
				PeriodInfo.AddError("Period must be an integer.");
			}
		}

		#endregion

		#region Enterprise Licence Code

		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString EnterpriseLicenceCode
		{
			get
			{
				return Organisation.LicenceEnterpriseCode;
			}
		}

		public ZPropertyInfo EnterpriseLicenceCodeInfo
		{
			get { return GetZPropertyInfo(nameof(EnterpriseLicenceCode)); }
		}

		public void ValidateEnterpriseLicenceCode()
		{
			EnterpriseLicenceCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EnterpriseLicenceCodeInfo);
		}

		#endregion

		#region Database Server Code

		[MaxLength(3)]
		[List("DatabaseCodeDescriptionPairList")]
		public ZString DatabaseServerCode
		{
			get { return databaseServerCode; }
			set { SetNonPersistentPropertyValue(DatabaseServerCodeInfo, ref databaseServerCode, value); }
		}
		ZString databaseServerCode;

		public ZPropertyInfo DatabaseServerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseServerCode)); }
		}

		public void ValidateDatabaseServerCode()
		{
			DatabaseServerCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DatabaseServerCodeInfo);
			ListValidation.ErrorIfInvalidCode(DatabaseServerCodeInfo);
		}

		public CodeDescriptionPairList DatabaseCodeDescriptionPairList
		{
			get
			{
				return Factory.GetCachedValue("ReopenPeriodKeyBusinessObject.DatabaseCodeDescriptionPairList",
					() =>
					{
						return LicenceDatabaseLookups.BuildDatabaseCategoryCodeDescriptionList(DatabaseList);
					});
			}
		}

		LicenceDatabase[] DatabaseList
		{
			get
			{
				return Factory.GetCachedValue("ReopenPeriodKeyBusinessObject.DatabaseList",
					() =>
					{
						var enterprise = Organisation.LicEnterprise;
						var query = new ZQuery(LicenceDatabaseSchema.LD_LE, enterprise.PK);
						query.AddToFilter(LicenceDatabaseSchema.LD_IsActive, true);
						return Factory.Load<LicenceDatabase>(query);
					});
			}
		}

		#endregion

		#region Client Company Code

		[MaxLength(3)]
		[List("ClientCompanyCodeDescriptionPairList")]
		public ZString ClientCompanyCode
		{
			get { return clientCompanyCode; }
			set { SetNonPersistentPropertyValue(ClientCompanyCodeInfo, ref clientCompanyCode, value); }
		}
		ZString clientCompanyCode;

		public ZPropertyInfo ClientCompanyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ClientCompanyCode)); }
		}

		public void ValidateClientCompanyCode()
		{
			ClientCompanyCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ClientCompanyCodeInfo);
		}

		public CodeDescriptionPairList ClientCompanyCodeDescriptionPairList
		{
			get
			{
				return Factory.GetCachedValue("ReopenPeriodKeyBusinessObject.ClientCompanyCodeDescriptionPairList." + DatabaseServerCode,
					() =>
					{
						return ClientCompanyLookups.BuildCompanyCategoryCodeDescriptionList(ClientCompanyList, Organisation);
					});
			}
		}

		ClientCompany[] ClientCompanyList
		{
			get
			{
				return Factory.GetCachedValue("ReopenPeriodKeyBusinessObject.ClientCompanyList." + DatabaseServerCode,
					() =>
					{
						var query = new ZQuery();
						var selectedDb = DatabaseList.Cast<LicenceDatabase>().FirstOrDefault(x => x.LD_ServerCode == databaseServerCode);
						if (selectedDb != null)
						{
							query.AddToFilter(ClientCompanySchema.LCC_LD, selectedDb.PK);
							query.AddToFilter(ClientCompanySchema.LCC_DeactivateTimeUtc, ZDateTime.Empty);
						}
						else
						{
							query.IsNoResultQuery = true;
						}

						return Factory.Load<ClientCompany>(query);
					});
			}
		}

		#endregion

		#region GenerateKey

		public string GenerateKey()
		{
			ReopenPeriodKeyGenerator generator = new ReopenPeriodKeyGenerator();
			RunPreSaveValidation();
			if (HasErrors)
			{
				throw new InvalidOperationException("Please correct the errors before continuing.");
			}
			int period = 0;
			try
			{
				period = Convert.ToInt32(Period, CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				throw new InvalidOperationException("Period is invalid.");
			}
			Key = generator.GenerateKey(EnterpriseLicenceCode, ClientCompanyCode, UserStaffCode, period);
			StmALog log = Organisation.Logs.AddNew(Events.PeriodReopened, string.Format(CultureInfo.CurrentCulture, "Period Reopen Key Generated by {0} for user staff code {1} at {2} GMT for period {3}", GlbStaff.CurrentUser.GS_LoginName, UserStaffCode, Key.Substring(0, 13), Period));
			log.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			Organisation.Factory.Save();
			return Key;
		}

		#endregion

		#region Key

		[MaxLength(20)]
		public ZString Key
		{
			get
			{
				return fKey;
			}
			set
			{
				if (fKey != value)
				{
					SetNonPersistentPropertyValue(KeyInfo, ref fKey, value);
					KeyOutputInfo.RefreshBinding();
				}
			}
		}
		ZString fKey;

		public ZPropertyInfo KeyInfo
		{
			get { return GetZPropertyInfo(nameof(Key)); }
		}

		#endregion

		#region KeyOutput

		[MaxLength(63)]
		public ZString KeyOutput
		{
			get
			{
				if (Key != ZString.Empty)
				{
					return "The Period Reopen Key for Period " + Period + " is " + Key;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo KeyOutputInfo
		{
			get { return GetZPropertyInfo(nameof(KeyOutput)); }
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUserStaffCode();
			ValidatePeriod();
			ValidateEnterpriseLicenceCode();
			ValidateDatabaseServerCode();
			ValidateClientCompanyCode();
		}

		#endregion
	}
}

