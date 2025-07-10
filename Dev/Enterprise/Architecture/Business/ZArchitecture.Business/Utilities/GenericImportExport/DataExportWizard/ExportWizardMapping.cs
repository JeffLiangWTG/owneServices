using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ExportWizardMapping : ImportExportMapping<ExportWizardMapping, ExportWizard>
	{
		public ExportWizardMapping(IImportPropertyInfo property, ExportWizardMappingCollection parentCollection)
			: base(property, parentCollection)
		{
			Order = (from m in parentCollection.Cast<ExportWizardMapping>() select m.Order).DefaultIfEmpty().Max() + 1;
			Alignment = typeof(INumericZType).IsAssignableFrom(PropertyType) ? FieldAlignment.Right : FieldAlignment.Left;
		}

		public ExportWizardMapping(RowType rowType, ExportWizardMappingCollection parentCollection)
			: this((IImportPropertyInfo)null, parentCollection)
		{
			this.rowType = rowType;
		}

		#region Properties

		#region Header

		[MaxLength(100)]
		public ZString Header
		{
			get
			{
				if (!headerSet)
				{
					header = Text;
					headerSet = true;
				}

				return header;
			}
			set
			{
				if (header != value)
				{
					SetNonPersistentPropertyValue(HeaderInfo, ref header, value);
				}
			}
		}

		ZString header;
		bool headerSet;

		public ZPropertyInfo HeaderInfo
		{
			get { return GetZPropertyInfo(nameof(Header)); }
		}

		public string GetHeaderString()
		{
			return FormatForFixedWidth(Header.TrimEnd((char)31, ' '));
		}

		#endregion

		#region Width

		public ZInt Width
		{
			get { return width; }
			set
			{
				if (width != value)
				{
					SetNonPersistentPropertyValue(WidthInfo, ref width, value);

					if (!IsValidationSuspended)
					{
						ValidateWidth();
					}
				}
			}
		}
		ZInt width;

		public ZPropertyInfo WidthInfo
		{
			get { return GetZPropertyInfo(nameof(Width)); }
		}

		void ValidateWidth()
		{
			WidthInfo.ClearAllNotifications();
			if (ParentCollection.Parent.FixedWidth)
			{
				MandatoryValidation.CheckEntered(WidthInfo);
			}
		}

		protected bool Width_ReadOnly
		{
			get { return !ParentCollection.Parent.FixedWidth; }
		}

		#endregion

		#region Alignment

		public FieldAlignment Alignment
		{
			get { return alignment ?? FieldAlignment.Left; }
			set { alignment = value; }
		}
		FieldAlignment? alignment = FieldAlignment.Left;

		[List("Alignments")]
		[MaxLength(10)]
		public ZString AlignmentString
		{
			get
			{
				return alignment.HasValue ? (ZString)alignment.Value.ToString() : alignmentString;
			}
			set
			{
				if (alignmentString != value)
				{
					SetNonPersistentPropertyValue(AlignmentStringInfo, ref alignmentString, value);

					try
					{
						alignment = (FieldAlignment)Enum.Parse(typeof(FieldAlignment), alignmentString, true);
					}
					catch (ArgumentException)
					{
						alignment = null;
					}

					if (!IsValidationSuspended)
					{
						ValidateAlignmentString();
					}
				}
			}
		}
		ZString alignmentString;

		public ZPropertyInfo AlignmentStringInfo
		{
			get { return GetZPropertyInfo(nameof(AlignmentString)); }
		}

		void ValidateAlignmentString()
		{
			AlignmentStringInfo.ClearAllNotifications();
			if (ParentCollection.Parent.FixedWidth)
			{
				MandatoryValidation.CheckEntered(AlignmentStringInfo);
				ListValidation.ErrorIfInvalidCode(AlignmentStringInfo);
			}
		}

		protected bool AlignmentString_ReadOnly
		{
			get { return !ParentCollection.Parent.FixedWidth; }
		}

		#endregion

		#region Order

		public int Order { get; set; }

		#endregion

		#region Expression

		protected override bool Expression_ReadOnly
		{
			get { return !String.IsNullOrEmpty(MappingName); }
		}

		protected override void OnExpressionChanged()
		{
			exprDelegate = null;
		}

		[BusinessObjectTestExclude]
		public Func<BusinessObject, string> ExprDelegate
		{
			get
			{
				if (exprDelegate == null && Expression.Length > 0)
				{
					exprDelegate = CreateDelegate<string>(Expression);
				}

				return exprDelegate;
			}
		}
		Func<BusinessObject, string> exprDelegate;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "iron python script")]
		Func<BusinessObject, T> CreateDelegate<T>(string expr)
		{
			Func<BusinessObject, T> result;
			string funName = "f" + Guid.NewGuid().ToString("N");
			List<IImportPropertyInfo> properties = new List<IImportPropertyInfo>();
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("def {0}(obj, properties, getResultFromFindBoxListProvider):", funName);
			sb.AppendLine();
			foreach (var p in RowType.Properties)
			{
				if (expr.Contains(p.MappingName))
				{
					if (typeof(ZDateTime).IsAssignableFrom(p.PropertyType))
					{
						sb.AppendFormat("  {0} = obj['{0}'].ToDateTime() if obj['{0}'].IsValid else DateTime.MinValue", p.MappingName);
					}
					else if (typeof(ZGuid).IsAssignableFrom(p.PropertyType))
					{
						properties.Add(p);
						sb.AppendFormat("  {0} = (", p.MappingName);
						IFindBoxListProvider findBoxListProvider = p.GetFindBoxListProvider(ParentCollection.Parent.CollectionInfo.BusinessObjects.FirstOrDefault(bo => RowType.Type.IsAssignableFrom(bo.GetType())));
						if (findBoxListProvider != null)
						{
							sb.AppendFormat("getResultFromFindBoxListProvider(properties[{0}], obj, 'Code')", properties.Count - 1);
							IFindBoxListProviderEx findBoxListProviderEx = findBoxListProvider as IFindBoxListProviderEx;
							if (findBoxListProviderEx != null)
							{
								foreach (var ak in findBoxListProviderEx.AlternateKeys)
								{
									sb.AppendFormat(", getResultFromFindBoxListProvider(properties[{0}], obj, '{1}')", properties.Count - 1, ak.ColumnDescription);
								}
							}
						}
						sb.AppendLine(")");
					}
					else if (typeof(ZBool).IsAssignableFrom(p.PropertyType))
					{
						sb.AppendFormat("  {0} = obj['{0}'].Equals(True)", p.MappingName);
					}
					else
					{
						if (typeof(IZType).IsAssignableFrom(p.PropertyType))
						{
							sb.AppendFormat("  {0} = clr.Convert(obj['{0}'], {1})", p.MappingName, ZDataType.ZTypeToBaseType(p.PropertyType).Name);
						}
						else
						{
							sb.AppendFormat("  {0} = obj['{0}']", p.MappingName);
						}
					}
					sb.AppendLine();
				}
			}
			sb.AppendFormat("  retVal = {0}", expr);
			sb.AppendLine();
			if (typeof(T).Equals(typeof(string)))
			{
				sb.AppendLine("  return retVal.ToString()");
			}
			else
			{
				sb.AppendLine("  return retVal");
			}

			ParentCollection.Parent.DlrProxy.RunScript(sb.ToString());
			var fun = ParentCollection.Parent.DlrProxy.CreateLambda
				<Func<
					BusinessObject,
					IList<IImportPropertyInfo>,
					Func<IImportPropertyInfo, BusinessObject, string, string>,
					T
				>>
				(funName + "(row, properties, getResultFromFindBoxListProvider)", "row", "properties", "getResultFromFindBoxListProvider");
			result = bizObj => fun(bizObj, properties, GetResultFromFindBoxListProvider);

			return result;
		}

		#endregion

		#region Condition

		[MaxLength(300)]
		public ZString ConditionExpr
		{
			get { return conditionExpr; }
			set
			{
				if (conditionExpr != value)
				{
					SetNonPersistentPropertyValue(ConditionExprInfo, ref conditionExpr, value);
					conditionDelegate = null;
				}
			}
		}
		ZString conditionExpr;

		public ZPropertyInfo ConditionExprInfo
		{
			get { return GetZPropertyInfo(nameof(ConditionExpr)); }
		}

		[BusinessObjectTestExclude]
		public Func<BusinessObject, bool> ConditionDelegate
		{
			get
			{
				if (conditionDelegate == null && ConditionExpr.Length > 0)
				{
					conditionDelegate = CreateDelegate<bool>(ConditionExpr);
				}

				return conditionDelegate;
			}
		}
		Func<BusinessObject, bool> conditionDelegate;

		public bool SatisfiesCondition(BusinessObject bizObj, out string errorMessage)
		{
			errorMessage = null;
			if (!ConditionExpr.IsEmpty)
			{
				try
				{
					return ConditionDelegate(bizObj);
				}
				catch (Exception ex) when (ParentCollection.Parent.DlrProxy.IsDlrException(ex) || ex is SystemException)
				{
					errorMessage = "ERROR: " + ex.Message; // exception message
				}
			}

			return true;
		}

		#endregion

		#region RowType

		public RowType RowType
		{
			get
			{
				if (rowType == null)
				{
					rowType = (from rt in ParentCollection.Parent.CollectionInfo.RowTypes
							   from p in rt.Properties
							   where p == Property
							   select rt).First();
				}

				return rowType;
			}
		}
		RowType rowType;

		#endregion

		#endregion

		#region Lookups

		#region MapAsLookup

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Format Identifier")]
		public override CodeDescriptionPairList MapAsLookup
		{
			get
			{
				if (IsType(null, typeof(ZDateTime)))
				{
					if (mapAsLookup == null)
					{
						mapAsLookup = new CodeDescriptionPairList();
						mapAsLookup.AddPair(Res.GetString("510a97c4-038f-4d25-91e9-3dc3dcea8b0b", "Short Date"), "d");
						mapAsLookup.AddPair(Res.GetString("2d54e7aa-286e-479d-a471-9a97182768b8", "Long Date"), "D");
						mapAsLookup.AddPair(Res.GetString("aa769dc1-4a80-4343-9402-77929b2ac226", "Long Date and Short Time"), "f");
						mapAsLookup.AddPair(Res.GetString("700b417b-5de6-4ef8-833e-4e065dba40cb", "Long Date and Long Time"), "F");
						mapAsLookup.AddPair(Res.GetString("0bac4348-11c3-4cb6-a141-b49762d380c3", "Short Date and Short Time"), "g");
						mapAsLookup.AddPair(Res.GetString("5236a183-cb5b-49ef-8efc-048f36238da6", "Short date and Long Time"), "G");
						mapAsLookup.AddPair(Res.GetString("aa5abb89-6c57-44db-94eb-c3086e810efa", "Short Time"), "t");
						mapAsLookup.AddPair(Res.GetString("934bc6db-9dbd-4f42-b4ef-079992ff3581", "Long Time"), "T");
					}

					return mapAsLookup;
				}
				else
				{
					return base.MapAsLookup;
				}
			}
		}

		CodeDescriptionPairList mapAsLookup;

		#endregion

		#region Alignments

		public CodeDescriptionPairList Alignments => alignments ?? (alignments = new FieldAlignments());
		CodeDescriptionPairList alignments;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateWidth();
			ValidateAlignmentString();
		}

		#endregion

		public string ToString(BusinessObject bizObj, bool showDefaultValue = false)
		{
			string result = null;

			if (String.IsNullOrEmpty(MappingName))
			{
				if (Expression.IsEmpty)
				{
					result = "";
				}
				else
				{
					try
					{
						result = ExprDelegate(bizObj);
					}
					catch (Exception ex) when (ParentCollection.Parent.DlrProxy.IsDlrException(ex) || ex is SystemException)
					{
						return "ERROR: " + ex.Message; // exception message
					}
				}
			}

			if (result == null)
			{
				result = GetResultFromFindBoxListProvider(Property, bizObj, MapAs);
			}

			if (result == null && IsType(bizObj, typeof(ZDateTime)))
			{
				string dateFormat = MapAsLookup.GetDescriptionFromCode(MapAs);
				if (!String.IsNullOrEmpty(dateFormat))
				{
					result = ((ZDateTime)bizObj[MappingName]).ToString(dateFormat);
				}
			}

			if (result == null)
			{
				if (!showDefaultValue)
				{
					if (((IZType)bizObj[MappingName]).IsEmpty)
					{
						result = "";
					}
					else
					{
						result = bizObj[MappingName].ToString();
					}
				}
				else
				{
					result = bizObj[MappingName].ToString();
				}
			}

			result = FormatForFixedWidth(result);

			return result;
		}

		static string GetResultFromFindBoxListProvider(IImportPropertyInfo property, BusinessObject bizObj, string mapAs)
		{
			string result = null;
			IFindBoxListProvider findBoxListProvider = property.GetFindBoxListProvider(bizObj);
			if (findBoxListProvider != null)
			{
				ZGuid pk;
				if (property.IsType(bizObj, typeof(ZGuid)))
				{
					pk = (ZGuid)bizObj[property.MappingName];
				}
				else if (property.IsType(bizObj, typeof(ZString)))
				{
					pk = findBoxListProvider.PrimaryKeyFromCode((ZString)bizObj[property.MappingName]);
				}
				else
				{
					pk = ZGuid.Invalid;
				}

				object value = null;
				IFindBoxListProviderEx findBoxListProviderEx = findBoxListProvider as IFindBoxListProviderEx;
				string alternateKey;
				if (findBoxListProviderEx != null && !String.IsNullOrEmpty(alternateKey = findBoxListProviderEx.AlternateKeys.FirstOrDefault(ak => ak.ColumnDescription.Equals(mapAs)).ColumnName))
				{
					value = findBoxListProviderEx.AlternateKeyFromPrimaryKey(alternateKey, pk);
				}
				else
				{
					value = findBoxListProvider.CodeFromPrimaryKey(pk);
				}

				if (value != null)
				{
					result = value.ToString();
				}
			}

			return result;
		}

		protected override IFindBoxListProvider GetFindBoxListProvider()
		{
			BusinessObject firstObj = ParentCollection.Parent.CollectionInfo.BusinessObjects.FirstOrDefault(bo => RowType.Type.IsAssignableFrom(bo.GetType()));
			if (firstObj != null)
			{
				return Property.GetFindBoxListProvider(firstObj);
			}

			return null;
		}

		string FormatForFixedWidth(string str)
		{
			if (ParentCollection.Parent.FixedWidth && Width > 0)
			{
				if (str.Length > Width)
				{
					str = str.Substring(0, Width);
				}
				else if (str.Length < Width)
				{
					if (Alignment == FieldAlignment.Left)
					{
						str = str.PadRight(Width);
					}
					else if (Alignment == FieldAlignment.Right)
					{
						str = str.PadLeft(Width);
					}
				}
			}

			return str;
		}

		public enum FieldAlignment { Left, Right }
	}
}
