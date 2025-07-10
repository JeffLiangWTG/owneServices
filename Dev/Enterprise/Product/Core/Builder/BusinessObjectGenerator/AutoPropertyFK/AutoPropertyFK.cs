using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoPropertyFK : AutoProperty
	{
		public AutoPropertyFK(BusinessObjectInfo info, DataColumn column, int maxColumnLength, string propertyType, bool isMasterFileFK)
			: base(info, column, maxColumnLength)
		{
			this.fPropertyType = propertyType;
			this.fIsMasterFile = isMasterFileFK;
		}

		public string PropertyType
		{
			get { return fPropertyType; }
		}

		readonly string fPropertyType;

		public bool IsMasterFileFK
		{
			get { return fIsMasterFile; }
		}

		readonly bool fIsMasterFile;

		#region Code for Property

		protected override string CodeForProperty
		{
			get
			{
				return LinesOfCode(
					CodeForRelatedBusinessObjectAttribute,
					CodeForListAttribute,
					base.CodeForProperty,
					CodeForRelatedBusinessObject);
			}
		}

		#endregion

		#region Code for Related Business Object Attribute

		protected internal virtual string CodeForRelatedBusinessObjectAttribute
		{
			get
			{
				string result = null;
				if (!Info.MasterFileReference)
				{
					result = "		[RelatedBusinessObject(\"" + RelatedBizObjName + "\")]";
				}
				return result;
			}
		}

		#endregion

		#region Code for Related Business Object

		public string RelatedBizObjName
		{
			get { return (DataType == typeof(Guid)) ? BizObjName : BizObjNameWithoutNK; }
		}

		protected string BizObjName
		{
			get
			{
				string result;

				string[] colNameParts = ColumnName.Split('_');

				if (colNameParts.Length > 2)
				{
					result = String.Join("_", colNameParts.Skip(2));
				}
				else
				{
					if (PropertyType == "JobHeader")
					{
						result = "Job";
					}
					else if (codes.Contains(PropertyType.Substring(0, 3)))
					{
						result = PropertyType.Substring(3);
					}
					else
					{
						result = PropertyType;
					}
				}
				return FixObjNameStartWithNumeric(result);
			}
		}

		readonly string[] codes = new string[] { "ABC", "Acc", "Cus", "Gen", "Glb", "Job", "Org", "Ref", "Stm", "Whs" };

		protected string BizObjNameWithoutNK
		{
			get
			{
				var bizObjName = BizObjName;

				if (bizObjName.StartsWith("NK", StringComparison.Ordinal))
				{
					return (bizObjName == "NKCountryCode")
						? "Country"
						: FixObjNameStartWithNumeric(bizObjName.Remove(0, 2));
				}

				return bizObjName;
			}
		}

		protected string FixObjNameStartWithNumeric(string result)
		{
			return result.Length > 1 && char.IsDigit(result, 0) ? "_" + result : result;
		}

		protected internal virtual string CodeForRelatedBusinessObject
		{
			get
			{
				string code = null;

				if (!Info.MasterFileReference)
				{
					if (DataType == typeof(Guid))
					{
						code = LinesOfCode(
							"",
							CodeForGuidRelatedBusinessObject);
					}
					else
					{
						code = LinesOfCode(
							"",
							CodeForStringRelatedBusinessObject);
					}
				}
				return code;
			}
		}

		protected virtual string CodeForGuidRelatedBusinessObject
		{
			get
			{
				return LinesOfCode(
					"		public virtual " + PropertyType + " " + BizObjName,
					"		{",
					"			get { return (" + PropertyType + ") Factory.Load(typeof(" + PropertyType + "), " + ColumnName + "); }",
					"		}");
			}
		}

		protected virtual string CodeForStringRelatedBusinessObject
		{
			get
			{
				var tablePrefix = ColumnName.Split('_')[1];

				var schemaAssembly = Assembly.Load("CargoWise.Odyssey.Schema");
				var schemaFullName = PropertyType + "Schema";
				var schemaType = schemaAssembly.DefinedTypes.Single(x => x.Name.Equals(schemaFullName));
				var nkWithCode = schemaType.GetField(tablePrefix + "_Code");
				var nkWithoutCode = schemaType.GetField(tablePrefix + "_" + BizObjNameWithoutNK);
				var nkRefColumn = nkWithCode ?? nkWithoutCode;

				return LinesOfCode(
					"		public virtual " + PropertyType + " " + BizObjNameWithoutNK + "",
					"		{",
					"			get { return (" + PropertyType + ") Factory.LoadFromNaturalKey(typeof(" + PropertyType + "), " + schemaFullName + "." + nkRefColumn.Name + ", " + ColumnName + "); }",
					"		}");
			}
		}

		#endregion

		#region Code for List Attribute

		protected internal virtual string CodeForListAttribute
		{
			get
			{
				return IsLookupShouldBeGenerated
					? ("		[List(\"Lookups." + PluralNameForCollection + "\")]")
					: null;
			}
		}

		public bool IsLookupShouldBeGenerated
		{
			get { return IsMasterFileFK && !IsReplacement && !Info.MasterFileReference && !IsComputed; }
		}

		protected virtual bool IsReplacement
		{
			get { return false; }
		}

		#endregion

		#region Support

		public string PluralNameForCollection
		{
			get { return NamingProvider.PluralizePropertyName(RelatedBizObjName); }
		}

		#endregion
	}
}
