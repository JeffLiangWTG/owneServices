using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ReportWriter
{
	public class DataSource : AutoDataSource
	{
		public DataSource(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoDataSource.Schema
		{
			public const string FunctionName = "FunctionName";
			public const int FunctionNameMaxLength = 150;
		}

		public override ZBool IsMain
		{
			get { return base.IsMain; }
			set
			{
				var oldValue = IsMain;
				base.IsMain = value;
				if (!IsCopying && oldValue != IsMain && IsMain)
				{
					ClearIsMainOnOtherSource();
				}
			}
		}

		public override ZString Name
		{
			get { return base.Name; }
			set
			{
				var oldValue = Name;
				base.Name = value;
				if (!IsCopying && oldValue != Name && IsMain && !parent.IsSettingMainDataSourceSuspended)
				{
					using (parent.SuspendSettingMainDataSource())
					{
						parent.DataSourceName = Name;
					}
				}
			}
		}

		public override ZString SQL
		{
			get { return base.SQL; }
			set
			{
				var oldValue = SQL;
				base.SQL = value;
				if (!IsCopying && oldValue != SQL)
				{
					if (IsMain && !parent.IsSettingMainDataSourceSuspended)
					{
						using (parent.SuspendSettingMainDataSource())
						{
							parent.DataSourceSQL = SQL;
						}
					}
					SetFunctionName();
				}
			}
		}

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:ReportWriter.DataSource|FunctionName", Caption = "Function Name")]
		public ZString FunctionName
		{
			get { return functionName; }
			private set
			{
				var oldValue = FunctionName;
				if (SetNonPersistentPropertyValue(FunctionNameInfo, ref functionName, value) && oldValue != FunctionName)
				{
					LoadDataTable();
				}
			}
		}
		ZString functionName;

		public IEnumerable<DataColumn> GetFunctionColumns()
		{
			if (dataTable != null)
			{
				foreach (var column in dataTable.Columns.Cast<DataColumn>())
				{
					yield return column;
				}
			}
		}

		void LoadDataTable()
		{
			if (dataTable != null)
			{
				dataTable.Dispose();
				dataTable = null;
			}
			if (!FunctionName.IsEmpty && DBHelper.ExistsFunction(FunctionName))
			{
				dataTable = DBHelper.GetDataTable(FunctionName, isAFunction);
			}
		}
		DataTable dataTable;

		void SetFunctionName()
		{
			var match = FunctionNameRegex.Match(TrimSpaceBetweenFuctionName(SQL));
			ZString name = match.Success ? match.Groups[1].Value : string.Empty;
			name = name.Replace("dbo.", "").Replace("[", "").Replace("]", "");
			isAFunction = name.EndsWith("(", StringComparison.OrdinalIgnoreCase);
			FunctionName = isAFunction ? name.Left(name.Length - 1) : name;
		}

		ZString TrimSpaceBetweenFuctionName(ZString sql)
		{
			while (sql.Contains(" (", StringComparison.OrdinalIgnoreCase))
			{
				sql = sql.Replace(" (", "(");
			}
			return sql;
		}

		bool isAFunction;

		protected int FunctionName_MaxLength
		{
			get { return Schema.FunctionNameMaxLength; }
		}

		public ZPropertyInfo FunctionNameInfo
		{
			get { return GetZPropertyInfo(Schema.FunctionName); }
		}

		Regex FunctionNameRegex
		{
			get { return functionNameRegex ?? (functionNameRegex = new Regex(@".*[\s]*FROM[\s]*([\[|\w|-|_|\.|\]]*[\s|\(]).*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex functionNameRegex;

		void ClearIsMainOnOtherSource()
		{
			parent.MainDataSource = this;
			foreach (var other in parent.DataSources.Cast<DataSource>().Where(x => x != this && x.IsMain))
			{
				other.IsMain = false;
			}
		}

		readonly ReportBizObj parent;
	}
}
