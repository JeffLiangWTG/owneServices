using System.Collections.Generic;

namespace Enterprise.BusinessObjectGenerator
{
	/// <summary>
	/// Don't know why is called OLD. Source control history doesn't go far enough to show it.
	/// It certainly doesn't look old to me as it's still in use to date.
	/// </summary>
	public class AutoSchema_OLD : AutoCode
	{
		public AutoSchema_OLD(BusinessObjectInfo info)
			: base()
		{
			this.Info = info;
		}

		public readonly BusinessObjectInfo Info;

		public override string ToString()
		{
			return LinesOfCode(
				"		#region Schema",
				"",
				"		public abstract class Schema",
				"		{",
				"			public const string TableName = \"" + Info.TableName + "\";",
				"			public const string PK        = \"" + Info.PKColumnName + "\";",
				"",
							CodeForProperties,
				"		}",
				"",
				"		#endregion");
		}

		#region Code for Properties

		protected string CodeForProperties
		{
			get
			{
				if (fCodeForProperties == null)
				{
					string codeForColumns = "";
					string codeForMaxLengths = "";

					foreach (AutoProperty property in Properties)
					{
						codeForColumns += string.Format(
							"			public const string {0} {1}= \"{0}\";", property.ColumnName, property.ColumnNameWhiteSpace) + System.Environment.NewLine;

						if (PropertySupportsMaxLength(property))
						{
							codeForMaxLengths += string.Format(
								"			public const int {0}MaxLength = {1};", property.ColumnName, property.MaxLength) + System.Environment.NewLine;
						}
					}

					fCodeForProperties = codeForColumns.TrimEnd(Whitespace);
					if (!string.IsNullOrEmpty(codeForMaxLengths))
					{
						fCodeForProperties += System.Environment.NewLine + System.Environment.NewLine + codeForMaxLengths.TrimEnd();
					}
				}

				return fCodeForProperties;
			}
		}

		protected string fCodeForProperties;

		protected virtual IEnumerable<AutoProperty> Properties => new AutoPropertyList(Info).Properties;

		protected bool PropertySupportsMaxLength(AutoProperty property)
		{
			return property.MaxLength > 1;
		}

		#endregion
	}
}
