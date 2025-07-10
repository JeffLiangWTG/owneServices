using System.Data;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoPropertyOrgAddressFK : AutoPropertyFK
	{
		public AutoPropertyOrgAddressFK(BusinessObjectInfo info, DataColumn column, int maxColumnLength, string propertyType, bool isMasterFileFK)
			: base(info, column, maxColumnLength, propertyType, isMasterFileFK)
		{
		}

		#region Implementation

		string ZAddressPropertyName
		{
			get { return ColumnName + "_ZAddress"; }
		}

		protected override string CodeForProperty
		{
			get
			{
				return base.CodeForProperty +
						CodeForZAddress;
			}
		}

		protected virtual string CodeForZAddress
		{
			get
			{
				string getNewMethod = "GetNew" + ZAddressPropertyName + "()";
				return LinesOfCode(
					"",
					"",
					"		#region ZAddress",
					"",
					"		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]",
					"		public ZAddress " + ZAddressPropertyName,
					"		{",
					"			get",
					"			{",
					"				if (f" + ZAddressPropertyName + " == null)",
					"				{",
					"					f" + ZAddressPropertyName + " = " + getNewMethod + ";",
					"				}",
					"				return f" + ZAddressPropertyName + ";",
					"			}",
					"		}",
					"		private ZAddress f" + ZAddressPropertyName + ";",
					"",
					"		protected virtual ZAddress " + getNewMethod,
					"		{",
					"			return new ZAddress(" + ColumnName + "Info);",
					"		}",
					"",
					"		#endregion");
			}
		}

		#endregion
	}
}