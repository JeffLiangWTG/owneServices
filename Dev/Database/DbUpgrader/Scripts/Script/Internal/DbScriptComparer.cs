using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Abstractions;

namespace Enterprise.Build.Database.Script
{
	public static class DbScriptComparer
	{
		static readonly Regex CreateCmdRegex = new Regex(@"(?<=(^|\n)\s*)CREATE(?=\s+(FUNCTION|PROC(EDURE)?|VIEW|TRIGGER|MESSAGE|QUEUE|SERVICE)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static IEqualityComparer<IDbScript> SchemaName
		{
			get { return new DbScriptComparerSchemaName(); }
		}

		public static IEqualityComparer<IDbScript> SchemaNameType
		{
			get { return new DbScriptComparerSchemaNameType(); }
		}

		public static IEqualityComparer<IDbScript> SchemaNameTypeText
		{
			get { return new DbScriptComparerSchemaNameTypeText(); }
		}

		#region Implementation

		class DbScriptComparerSchemaName : IEqualityComparer<IDbScript>
		{
			public bool Equals(IDbScript x, IDbScript y)
			{
				return
					x.SchemaName.Equals(y.SchemaName, StringComparison.OrdinalIgnoreCase)
					&& x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
			}

			public int GetHashCode(IDbScript script)
			{
				return
					script.SchemaName.ToUpper().GetHashCode()
					^ script.Name.ToUpper().GetHashCode();
			}
		}

		class DbScriptComparerSchemaNameType : DbScriptComparerSchemaName, IEqualityComparer<IDbScript>
		{
			bool IEqualityComparer<IDbScript>.Equals(IDbScript x, IDbScript y)
			{
				return
					base.Equals(x, y)
					&& x.ObjectType == y.ObjectType;
			}

			int IEqualityComparer<IDbScript>.GetHashCode(IDbScript script)
			{
				return
					base.GetHashCode(script)
					^ script.ObjectType.GetHashCode();
			}
		}

		class DbScriptComparerSchemaNameTypeText : DbScriptComparerSchemaNameType, IEqualityComparer<IDbScript>
		{
			bool IEqualityComparer<IDbScript>.Equals(IDbScript x, IDbScript y)
			{
				if (base.Equals(x, y))
				{
					if (x.ObjectType == DbRoutineType.SqlQueueTypeDesc || x.ObjectType == DbRoutineType.SqlMessageTypeDesc)
					{
						return true;
					}

					return DbScriptComparer.CreateCmdRegex.Replace(x.Text, "CREATE", 1) == DbScriptComparer.CreateCmdRegex.Replace(y.Text, "CREATE", 1);
				}

				return false;
			}

			int IEqualityComparer<IDbScript>.GetHashCode(IDbScript script)
			{
				if (script.ObjectType == DbRoutineType.SqlQueueTypeDesc || script.ObjectType == DbRoutineType.SqlMessageTypeDesc)
				{
					return base.GetHashCode(script);
				}

				return
					base.GetHashCode(script)
					^ DbScriptComparer.CreateCmdRegex.Replace(script.Text, "CREATE", 1).GetHashCode();
			}
		}

		#endregion // Implementation
	}
}
