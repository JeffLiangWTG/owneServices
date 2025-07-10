using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportLineCollectionBase<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject, new()
	{
		public AccComplianceReportLineCollectionBase(AccComplianceReport report)
			: base(report?.Factory)
		{
			Report = Argument.NotNull(report, nameof(report));

			var getTableMethod = typeof(T).GetMember(GetDataTableMethodName, MemberTypes.Method,
				BindingFlags.Public | BindingFlags.Static);
			if (!getTableMethod.Any())
			{
				throw new ArgumentException(FormattableString.Invariant($"The {typeof(T).FullName} must implement public static {GetDataTableMethodName} method which returns ZDataTable."));
			}
		}

		AccComplianceReport Report { get; }

		const string GetDataTableMethodName = "GetDataTable";

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Allow new is false so shouldn't get called");
		}

		protected override ZDataTable Table => table ?? (table = GetDataTable());
		ZDataTable table;

		internal IReadOnlyList<string> GetColumnNames() => Table.Columns.OfType<DataColumn>().Where(x => x.ColumnName != AccComplianceReportLineBase.Schema.PK).Select(x => x.ColumnName).ToList().AsReadOnly();

		ZDataTable GetDataTable()
		{
			return (ZDataTable)typeof(T).InvokeMember(GetDataTableMethodName,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
				null, null, new object[] { Report }, CultureInfo.InvariantCulture);
		}
	}
}
