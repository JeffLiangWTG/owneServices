using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FilterDepartmentConstraint : IFilterConstraint
	{
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string IFilterConstraint.Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return FilterConstants.Department; }
		}

		public string SingularValueName
		{
			get { return Res.GetString("f191c2a8-ccf9-4a40-b0d9-db25e1ffb2a7", "department"); }
		}

		public string PluralValueName
		{
			get { return Res.GetString("0179cf27-51ff-4834-90d0-22a81111f393", "departments"); }
		}

		string IFilterConstraint.Description
		{
			get
			{
				return Res.GetString("OperationalActionsFilter|DepartmentConstraint|Description",
					"Matches on the department code of the current login branch.\r\ne.g. 'Department == \"FES\"' will only match if the code of the current login department is 'Forward Export Sea'.");
			}
		}

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			GlbDepartment department = GlbDepartment.CurrentDepartment;
			return department == null ? null : department.GE_Code;
		}
	}
}
