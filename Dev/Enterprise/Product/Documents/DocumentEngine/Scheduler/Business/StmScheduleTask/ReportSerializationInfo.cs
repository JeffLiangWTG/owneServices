using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportSerializationInfo : IJsonSerializable
	{
		readonly Guid branch;
		readonly Guid department;
		readonly DeliveryInstructions instructions;
		readonly Report report;
		readonly string user;
		readonly string language;

#if DEBUG
		public ReportSerializationInfo(string user, ZGuid branch, ZGuid department)
		{
			this.branch = branch.ToGuid();
			this.department = department.ToGuid();
			this.user = user;
		}
#endif

		public ReportSerializationInfo(DeliveryInstructions instructions, Report report)
			: this(instructions, report, Env.CurrentUser.LoginName)
		{ }

		public ReportSerializationInfo(DeliveryInstructions instructions, Report report, string userLogin)
		{
			this.report = report;
			this.user = userLogin;
			this.department = GlbDepartment.CurrentDepartment.PK.ToGuid();
			this.branch = GlbBranch.CurrentBranch.PK.ToGuid();
			this.instructions = instructions;
			this.language = report?.Language;
		}

		#region Constructor For IJsonSerializable

		internal ReportSerializationInfo(ReportSerializationInfoJsonData data)
		{
			user = data.User;
			branch = data.Branch;
			department = data.Department;
			report = data.Report != null ? new Report(data.Report) : null;
			language = data.Language;
			instructions = data.Instructions != null ? new DeliveryInstructions(data.Instructions) : null;
		}

		#endregion

		public Guid Branch
		{
			get { return branch; }
		}

		public Guid Department
		{
			get { return department; }
		}

		public DeliveryInstructions Instructions
		{
			get { return instructions; }
		}

		public Report Report
		{
			get { return report; }
		}

		public string User
		{
			get { return user; }
		}

		public string Language
		{
			get { return language; }
		}

		public IDisposable SwitchUserTemporarily(ZString printUserCode)
		{
			return new UserSwitcher(this, printUserCode);
		}

		internal class UserSwitcher : IDisposable
		{
			readonly IDisposable userContextChange;

			public UserSwitcher(ReportSerializationInfo info, ZString printUserCode)
			{
				var factory = (GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.Factory : null) ?? new BusinessObjectFactory();
				var user = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, printUserCode))
							?? factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, info.User));

				if (user == null)
				{
					throw new InvalidPrintUserException(string.Format(CultureInfo.InvariantCulture, "{0} or {1}", printUserCode, info.User), false);
				}
				else if (!user.GS_IsActive)
				{
					throw new InvalidPrintUserException(user.GS_LoginName, true);
				}

				userContextChange = Env.SetTemporaryUserContext(user.GS_LoginName, Env.CurrentBranch.PK, info.Department);
				if (Env.CurrentDepartment == null)
				{
					userContextChange.Dispose();
					userContextChange = Env.SetTemporaryUserContext(user.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				}
			}

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				if (userContextChange != null)
				{
					userContextChange.Dispose();
				}
			}

			#endregion
		}

		[Serializable]
		public class InvalidPrintUserException : Exception
		{
#if NETFRAMEWORK
			protected InvalidPrintUserException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

			public InvalidPrintUserException(string user, bool isInactive)
				: base(string.Format(isInactive ? (NoResString)"User '{0}' is inactive." : (NoResString)"Cannot find user {0}", user))
			{
				User = user;
				IsInactive = isInactive;
			}

			public string User { get; private set; }
			public bool IsInactive { get; private set; }
		}

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new ReportSerializationInfoJsonData
			{
				User = User,
				Branch = Branch,
				Department = Department,
				Report = (DocumentJsonData)Report?.GetJsonData(),
				Language = Language,
				Instructions = (DeliveryInstructionsJsonData)Instructions?.GetJsonData()
			};

		#endregion
	}
}
