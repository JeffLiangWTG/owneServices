using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Environment
{
	public interface IErrorReportStrategy
	{
		bool IsSilentReport { get; set; }
		IList<ZString> NotificationMessages { get; }
	}

	public class DefaultErrorReportStrategy : IErrorReportStrategy
	{
		public bool IsSilentReport { get; set; }
		public IList<ZString> NotificationMessages { get; protected set; } = new List<ZString>();
	}

	/// <summary>
	///		Encapsulates environment variables related to a user login.
	/// </summary>
	public class UserContext : IUserContext
	{
		const string GlbStaffTypeName = "IGlbStaff";
		const string GlbDepartmentTypeName = "IGlbDepartment";
		const string GlbBranchTypeName = "IGlbBranch";
		const string GlbCompanyTypeName = "IGlbCompany";

		BusinessObjectFactory contextFactory;

		public UserContext()
		{ }

		public UserContext(string staffLoginName, Guid branchPK, Guid departmentPK, IErrorReportStrategy errorReportStrategy, ILoginToken loginToken = null, bool reportWrongUser = true, BusinessObjectFactory factory = null)
		{
			this.errorReportStrategy = errorReportStrategy;
			LoadAndSetupUserContext(factory ?? NewFactory(), staffLoginName, branchPK, departmentPK, loginToken, reportWrongUser);
		}

		public UserContext(string staffLoginName, Guid branchPK, Guid departmentPK, ILoginToken loginToken = null, bool reportWrongUser = true, BusinessObjectFactory factory = null)
		{
			LoadAndSetupUserContext(factory ?? NewFactory(), staffLoginName, branchPK, departmentPK, loginToken, reportWrongUser);
		}

		public UserContext(Guid staffPK, Guid branchPK, Guid departmentPK, ILoginToken loginToken = null, bool reportWrongUser = true, BusinessObjectFactory factory = null)
		{
			LoadAndSetupUserContext(factory ?? NewFactory(), staffPK, branchPK, departmentPK, loginToken, reportWrongUser);
		}

		internal static UserContext SetupAndReturnUserContextForTemporarySwitch(string staffLoginName, Guid branchPK, Guid departmentPK, BusinessObjectFactory factory = null, bool reportMissingBranch = true)
		{
			var context = new UserContext();
			context.LoadAndSetupUserContext(factory ?? context.NewFactory(), staffLoginName, branchPK, departmentPK, null, true, reportMissingBranch: reportMissingBranch);
			return context;
		}

		internal static UserContext SetupAndReturnUserContextForTemporarySwitch(Guid staffPK, Guid branchPK, Guid departmentPK, BusinessObjectFactory factory = null)
		{
			var context = new UserContext();
			context.LoadAndSetupUserContext(factory ?? context.NewFactory(), staffPK, branchPK, departmentPK, null, true);
			return context;
		}

		public UserContext(IUser user, Guid branchPK, Guid departmentPK, bool reportWrongUser = true, LoginAuthenticationInfo loginAuthenticationInfo = null)
		{
			BusinessObjectFactory factory;
			var factoryProvider = user as IFactoryProvider;
			if (factoryProvider != null)
			{
				factory = factoryProvider.Factory;
			}
			else
			{
				factory = NewFactory();
			}
			LoadAndSetupUserContext(factory, user, branchPK, departmentPK, reportWrongUser, user.LoginName, loginAuthenticationInfo);
		}

		void LoadAndSetupUserContext(BusinessObjectFactory factory, Guid staffPK, Guid branchPK, Guid departmentPK, ILoginToken loginToken, bool reportWrongUser)
		{
			var user = (IUser)factory.Load(GetObjectFactoryType(GlbStaffTypeName), staffPK);
			if (user != null)
			{
				user.LoginToken = loginToken;
			}
			LoadAndSetupUserContext(factory, user, branchPK, departmentPK, reportWrongUser, staffPK.ToString(), LoginAuthenticationInfo.NewSuccessfulLogin(user));
		}

		void LoadAndSetupUserContext(BusinessObjectFactory factory, string staffLoginName, Guid branchPK, Guid departmentPK, ILoginToken loginToken, bool reportWrongUser, bool reportMissingBranch = true)
		{
			factory.SuspendValidation();
			try
			{
				var user = (IUser)factory.LoadFromNaturalKey(GetObjectFactoryType(GlbStaffTypeName), GlbStaffSchema.GS_LoginName, staffLoginName);
				if (user != null)
				{
					user.LoginToken = loginToken;
				}
				LoadAndSetupUserContext(factory, user, branchPK, departmentPK, reportWrongUser, staffLoginName, LoginAuthenticationInfo.NewSuccessfulLogin(user), reportMissingBranch: reportMissingBranch);
			}
			finally
			{
				factory.ResumeValidation();
			}
		}

		BusinessObjectFactory NewFactory()
		{
			var factory = new BusinessObjectFactory();
			factory.NameForDebugging = "UserContext";
			if (!Globals.IsUserInteractive)
			{
				factory.RefreshEnabled = false;
			}
			return factory;
		}

		Type GetObjectFactoryType(string typeName)
		{
			var type = ObjectFactory.GetType(typeName);
			if (type == null)
			{
				if (ObjectFactory.IsConfigured)
				{
					ErrorReporter.ReportOnce("UserContext.GetObjectFactoryType", "ObjectFactory is missing type " + typeName);
				}
				else
				{
					ErrorReporter.ReportOnce("UserContext.ObjectFactory", "ObjectFactory is not configured when getting type " + typeName);
				}
			}
			return type;
		}

#if DEBUG
		internal
#endif
		static bool CheckErrorEmailInterval(string userName)
		{
			if (userName == null)
			{
				userName = Res.GetString("48FDC602-C978-4535-9981-E96D91278C69", "<empty user>");
			}
			var timestamps = DataRegistry.Instance.UserContextErrorEmailTimestamps;
			if (!timestamps.ContainsKey(userName))
			{
				return true;
			}
			return ZDateTime.UtcNow - timestamps[userName] > TimeSpan.FromSeconds(DataRegistry.Instance.UserContextErrorEmailInterval);
		}

		static void UpdateErrorEmailTimestamp(string userName)
		{
			if (userName == null)
			{
				userName = Res.GetString("48FDC602-C978-4535-9981-E96D91278C69", "<empty user>");
			}
			var timestamps = DataRegistry.Instance.UserContextErrorEmailTimestamps;
			timestamps[userName] = ZDateTime.UtcNow;
			DataRegistry.Instance.UserContextErrorEmailTimestamps = timestamps;
		}

		void LoadAndSetupUserContext(BusinessObjectFactory factory, IUser user, Guid branchPK, Guid departmentPK, bool reportWrongUser, string staffLoginToReport, LoginAuthenticationInfo loginAuthenticationInfo, bool reportMissingBranch = true)
		{
			if (errorReportStrategy != null)
			{
				errorReportStrategy.NotificationMessages.Clear();
			}

			this.contextFactory = factory;
			User = user;
			Department = (IDepartment)factory.Load(GetObjectFactoryType(GlbDepartmentTypeName), departmentPK);
			Branch = (IBranch)factory.Load(GetObjectFactoryType(GlbBranchTypeName), branchPK);
			LoginAuthenticationInfo = loginAuthenticationInfo;

			if (Branch == null)
			{
				RowFactory.ClearSpecificTableFromUberFactory(GlbBranchSchema.Constants.TableName);

				var query = new ZQuery(GlbBranchSchema.PK, branchPK)
				{
					ReLoadExistingRows = true
				};
				Branch = (IBranch)factory.LoadTop1(GetObjectFactoryType(GlbBranchTypeName), query);
			}

			if (Branch != null)
			{
				var companyPK = Branch.CompanyPK;
				Company = (ICompany)factory.Load(GetObjectFactoryType(GlbCompanyTypeName), companyPK);

				if (Company == null)
				{
					RowFactory.ClearSpecificTableFromUberFactory(GlbCompanySchema.Constants.TableName);

					var query = new ZQuery(GlbCompanySchema.PK, companyPK)
					{
						ReLoadExistingRows = true
					};
					Company = (ICompany)factory.LoadTop1(GetObjectFactoryType(GlbCompanyTypeName), query);

					if (Company == null)
					{
						ErrorReporter.ReportOnce("LoadAndSetupUserContext_CannotLoadCompany", "Cannot load Company for PK '" + companyPK.ToString() + "' for branch '" + Branch.Code + "'.");
					}
				}
			}

			if (reportWrongUser && User == null
#if DEBUG
				&& (!Globals.IsTest || ForceToNotSkipForTest)
#endif
				)
			{
				if (Globals.IsUserInteractive)
				{
					var msg = Res.GetString("e9cc3bb6-9f7d-4247-9356-2d1c5c8e89db", "Could not find login name \"{0}\" in the database (Perhaps it has been changed or deleted). Some operations may not function properly. Please restart {1} to fix this. If this error persists, please contact your system administrator.", staffLoginToReport, BrandingFactory.Instance.ProductName);
					if (errorReportStrategy != null && errorReportStrategy.IsSilentReport)
					{
						errorReportStrategy.NotificationMessages.Add(msg);
					}
					else
					{
						Globals.Message.ShowError(msg);
					}
				}
				else
				{
					var bodyBuilder = new StringBuilder();
					bodyBuilder.AppendLine(Res.GetString("BBA4F139-8906-4005-85EE-98D505ECE679", "There is no User found for login \"{0}\".", staffLoginToReport));
					string serviceTaskCode = System.Environment.GetEnvironmentVariable("ServiceTaskCode");
					if (!string.IsNullOrEmpty(serviceTaskCode))
					{
						bodyBuilder.AppendLine(Res.GetString("FED62B72-8938-4949-ABD1-BA6E6811370D", "The current service task code is \"{0}\".", serviceTaskCode));
					}

					if (CheckErrorEmailInterval(staffLoginToReport))
					{
						try
						{
							var email = new EmailDef();
							email.Body = bodyBuilder.ToString();
							email.Subject = Res.GetString("3F664FB6-2940-4359-9EEF-06F547C263EC", "Missing User For Login");
							Env.OutgoingMailManager.CreateAndSaveToPostmasterGroup(email);
						}
						catch (EmailHasNoRecipientsException)
						{
							// It's the client's resposibility to ensure the PostMaster group has at least one email address configured.
						}
						UpdateErrorEmailTimestamp(staffLoginToReport);
					}

					if (!string.IsNullOrEmpty(staffLoginToReport))
					{
						if (Guid.TryParse(staffLoginToReport, out Guid _))
						{
							var count = (int)Db.Connection.ExecuteScalar(string.Format("select count(*) from dbo.glbstaff where GS_PK = '{0}'", staffLoginToReport));
							if (count == 0)
							{
								bodyBuilder.AppendLine((NoResString)"And it in fact, does not appear to be in the database. Check callstack for how this PK could have been attempted in non-interactive context.");
							}
							else
							{
								bodyBuilder.AppendLine("But checking the database directly, we find it! This suggests factory.LoadFromNaturalKey(ObjectFactory.GetType(GlbStaffTypeName), GlbStaffSchema.GS_LoginName, staffLoginName); somehow malfunctioned or a race condition occurred.");
							}
						}
						else
						{
							var count = (int)Db.Connection.ExecuteScalar(string.Format("select count(*) from dbo.glbstaff where GS_LoginName = '{0}'", staffLoginToReport));
							if (count == 0)
							{
								bodyBuilder.AppendLine((NoResString)"And it in fact, does not appear to be in the database. Check callstack for how this username could have been attempted in non-interactive context.");
							}
							else
							{
								bodyBuilder.AppendLine((NoResString)"But checking the database directly, we find it! This suggests factory.Load(ObjectFactory.GetType(GlbStaffTypeName), staffPK); somehow malfunctioned or a race condition occurred.");
							}
						}
					}
					ErrorReporter.ReportOnce($"LoadAndSetupUserContext_MissingUserForLogin_{serviceTaskCode}", bodyBuilder.ToString());
				}
			}

			if (Branch == null && new ZGuid(branchPK).IsValid && reportMissingBranch)
			{
				ErrorReporter.ReportOnce("LoadAndSetupUserContext_CannotLoadBranch", "Cannot load Branch for PK '" + branchPK.ToString() + "'.");
			}
		}

#if DEBUG
		static readonly Overridable<bool> forceToNotSkipForTest = new Overridable<bool>(false);

		public static bool ForceToNotSkipForTest
		{
			get { return forceToNotSkipForTest.Value; }
			set { forceToNotSkipForTest.Value = value; }
		}
#endif

		readonly IErrorReportStrategy errorReportStrategy;

		public ICompany Company { get; protected set; }
		public IBranch Branch { get; private set; }
		public IDepartment Department { get; private set; }
		public IUser User { get; protected set; }
		public LoginAuthenticationInfo LoginAuthenticationInfo { get; protected set; }

		ILicenceProxy IUserContext.Licence => Licence;

		#region Security

		public LoginSecurityCheckpoint LoginSecurityCheckpoint
		{
			get
			{
				return new LoginSecurityCheckpoint(this);
			}
		}

		#endregion

		#region Licences

		public Licences Licence
		{
			get
			{
				if (licence == null)
				{
					licence = new Licences(this);
				}
				return licence;
			}
#if DEBUG
			set
			{
				licence = value;
			}
#endif
		}
		Licences licence;

		#endregion

		#region Cloning

		UserContext(ICompany company, IBranch branch, IDepartment department, IUser user)
		{
			BusinessObjectFactory factory = null;
			Company = LoadOnNewFactory(ref factory, company);
			Branch = LoadOnNewFactory(ref factory, branch);
			Department = LoadOnNewFactory(ref factory, department);
			User = LoadOnNewFactory(ref factory, user);

			if (User != null)
			{
				User.LoginToken = user.LoginToken;
			}
		}

		public IUserContext ThreadSafeClone()
		{
			using (DataRefreshManager.BeginRefreshOverrideForServiceTask())
			using (Db.DisposableActionForDbConnection())
			{
				return new UserContext(Company, Branch, Department, User);
			}
		}

		public void EnsureCurrentThreadIsOwner()
		{
			contextFactory?.ThreadSentry.EnsureCurrentThreadIsOwner();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Developer only message")]
		T LoadOnNewFactory<T>(ref BusinessObjectFactory factory, T original)
		{
			if (original == null)
			{
				return original;
			}
			else if (factory == null)
			{
				factory = NewFactory();
			}

			var bizo = (BusinessObject)(object)original;
			var loadedValue = factory.Load(original.GetType(), bizo.PK);

			if (loadedValue == null)
			{
				ErrorReporter.ReportOnce("OriginalNotNullButCopyIs", $"When trying to reload Env.Current[Branch|Company|Department] the Load operation returned null. Please ensure your bizos are in the DB before setting the user context. TableName: {bizo.TableName}, IsInDb: {bizo.IsInDatabase}");
				return original;
			}
			return (T)(object)loadedValue;
		}

		#endregion

		public override bool Equals(object obj)
		{
			return obj != null && Equals(this, obj as IUserContext);
		}

		public static bool Equals(IUserContext a, IUserContext b)
		{
			return ReferenceEquals(a, b) ||
				a != null && b != null &&
				a.User?.PK == b.User?.PK &&
				a.Company?.PK == b.Company?.PK &&
				a.Department?.PK == b.Department?.PK &&
				a.Branch?.PK == b.Branch?.PK;
		}

		public override int GetHashCode()
		{
			return (User?.PK.GetHashCode() ?? 0) ^
				(Branch?.PK.GetHashCode() ?? 0) ^
				(Department?.PK.GetHashCode() ?? 0);
		}

		public T GetInstance<T>(Func<T> constructor)
		{
			if (instances == null)
			{
				instances = new Dictionary<Type, object>();
			}
			object instance;
			if (!instances.TryGetValue(typeof(T), out instance))
			{
				instance = constructor();
				instances.Add(typeof(T), instance);
			}
			return (T)instance;
		}

		Dictionary<Type, object> instances;
	}
}
