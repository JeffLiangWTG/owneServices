using System;
using System.Data;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoIsCancelledProperty : AutoProperty
	{
		#region Fields

		readonly string CancelRelatedColumnName;
		readonly DataColumn Column;
		readonly bool NeedInverseValues;
		readonly string Prefix;

		#endregion

		#region Constructors

		public AutoIsCancelledProperty(BusinessObjectInfo info, DataColumn column, int maxColumnLength)
			: base(info, column, maxColumnLength)
		{
			this.Column = column;
			Prefix = info.PKColumnName.Substring(0, 3).TrimEnd('_');

			if (IsActiveColumn)
			{
				CancelRelatedColumnName = Prefix + CargoWise.Schema.Schema.IsActiveColumnSuffix;
				NeedInverseValues = true;
			}
			else if (IsCancelledColumn)
			{
				CancelRelatedColumnName = Prefix + "_IsCancelled";
			}
			else
			{
				throw new Exception("IsCancelled property can be generated only for BO which have either XX_IsActive or XX_IsCancelled column");
			}
		}

		#endregion	   

		public static bool MatchesNamingRequirements(string columnName)
		{
			return
				columnName.EndsWith(CargoWise.Schema.Schema.IsActiveColumnSuffix, StringComparison.CurrentCultureIgnoreCase) ||
				columnName.EndsWith("_IsCancelled", StringComparison.CurrentCultureIgnoreCase);
		}

		#region Properties

		protected override string  CodeForProperty
		{
			get
			{
				return
					LinesOfCode(
					"		public virtual bool IsCancelled",
					"		{",
					"			get { return " + InverseSign + CancelRelatedColumnName + "; }",
					"			set { " + CancelRelatedColumnName + " = " + InverseSign + "value; }",
					"		}",
					"",
					"		public virtual bool IsCancelledHasChanged",
					"		{",
					"			get { return " + CancelRelatedColumnName + "Info.HasChanges; }",
					"		}",
					"",
					"		public virtual string CanCancel()",
					"		{",
					"			return null;",
					"		}",
					"",
					"		public virtual string CanReactivate()",
					"		{",
					"			return null;",
					"		}"

					) + System.Environment.NewLine + System.Environment.NewLine +
					base.CodeForProperty;
			}
		}

		#endregion

		#region Implementation

		string InverseSign
		{
			get { return NeedInverseValues ? "!" : ""; }
		}

		bool IsActiveColumn
		{
			get
			{
				return Column.ColumnName == Prefix + CargoWise.Schema.Schema.IsActiveColumnSuffix;
			}
		}

		bool IsCancelledColumn
		{
			get
			{
				return Column.ColumnName == Prefix + "_IsCancelled";
			}
		}

		#endregion
	}
}
