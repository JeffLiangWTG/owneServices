using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoBusinessObject : AutoSourceFile
	{
		public AutoBusinessObject(BusinessObjectInfo info)
		{
			this.Info = info;
			this.isCargoWiseEntityFrameworkNamespace = (
				string.Compare(info.Namespace, "CargoWise.EntityFramework.Testing", StringComparison.OrdinalIgnoreCase) == 0);
		}

		public string ClassName
		{
			get { return Info.ClassNames.BusinessObject; }
		}

		protected internal virtual AutoPropertyList AutoProperties
		{
			get
			{
				if (fAutoProperties == null)
				{
					fAutoProperties = new AutoPropertyList(Info);
				}
				return fAutoProperties;
			}
		}

		protected virtual string InheritsFrom
		{
			get { return Info.BaseClassName; }
		}

		protected virtual string ImplementsInterfaces
		{
			get
			{
				string result = "";

				if (AutoProperties.HasIsValidProperty)
				{
					result += ", ILightValidationInternals";
				}

				if (HasSupportsIsActive)
				{
					result += ", ICancellable";
				}

				if (HasAuditColumns)
				{
					if (HasAuditContextColumns)
					{
						result += ", IAuditDetailsWithContext";
					}
					else
					{
						result += ", IAuditDetails";
					}
				}

				return result;
			}
		}

		#region Code for Schema

		/// <summary>
		/// Don't know why is called OLD. Source control history doesn't go far enough to show it.
		/// It certainly doesn't look old to me as it's still in use to date.
		/// </summary>
		public virtual string SchemaOLD
		{
			get { return new AutoSchema_OLD(Info).ToString(); }
		}

		#endregion

		#region Code / Description / Is Active / Is Cancelled Attributes

		protected virtual string CodeAttribute
		{
			get { return HasCodeColumn ? ", CodeProperty(" + Info.ClassNames.Schema + ".Constants." + CodeColumnName + ")" : ""; }
		}

		protected virtual string DescriptionAttribute
		{
			get { return HasDescriptionColumn ? ", DescriptionProperty(" + Info.ClassNames.Schema + ".Constants." + DescriptionColumnName + ")" : ""; }
		}

		protected virtual string IsActiveAttribute
		{
			get { return HasIsActiveColumn ? ", IsActiveProperty(" + Info.ClassNames.Schema + ".Constants." + IsActiveColumnName + ")" : ""; }
		}

		protected virtual string IsCancelledAttribute
		{
			get { return HasIsCancelledColumn ? ", IsCancelledProperty(" + Info.ClassNames.Schema + ".Constants." + IsCancelledColumnName + ")" : ""; }
		}

		protected virtual string PreventDeleteAttribute
		{
			get { return HasSupportsIsActive ? ", PreventDelete(" + (Info.PreventDelete ? "true" : "false") + ")" : ""; }
		}

		protected string Prefix
		{
			get { return this.Info.PKColumnName.Substring(0, 3).TrimEnd('_'); }
		}

		protected string CodeColumnName
		{
			get { return Prefix + "_Code"; }
		}

		protected string DescriptionColumnName
		{
			get { return Prefix + "_Description"; }
		}

		protected string IsActiveColumnName
		{
			get { return Prefix + CargoWise.Schema.Schema.IsActiveColumnSuffix; }
		}

		protected string IsCancelledColumnName
		{
			get { return Prefix + "_IsCancelled"; }
		}

		protected bool HasCodeColumn
		{
			get
			{
				foreach (AutoProperty property in AutoProperties.Properties)
				{
					if (property.ColumnName == CodeColumnName)
					{
						return true;
						//return !(Property is IDeprecatedProperty);
					}
				}
				return false;
			}
		}

		protected bool HasDescriptionColumn
		{
			get
			{
				foreach (AutoProperty property in AutoProperties.Properties)
				{
					if (property.ColumnName == DescriptionColumnName)
					{
						return true;
						//return !(Property is IDeprecatedProperty);
					}
				}
				return false;
			}
		}

		protected bool HasSupportsIsActive
		{
			get
			{
				return HasIsActiveColumn || HasIsCancelledColumn;
			}
		}

		protected bool HasIsActiveColumn
		{
			get
			{
				foreach (AutoProperty property in AutoProperties.Properties)
				{
					if (property.ColumnName == IsActiveColumnName)
					{
						return true;
					}
				}
				return false;
			}
		}

		[ThreadSafe]
		static readonly List<(string type, string columnName)> auditColumns = new List<(string, string)>
		{
			("ZDateTime", "SystemCreateTimeUtc"),
			("ZDateTime","SystemLastEditTimeUtc"),
			("ZString", "SystemCreateUser"),
			("ZString", "SystemLastEditUser"),
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected IEnumerable<((string type, string columnName) column, AutoProperty property)> AuditColumns
		{
			get
			{
				foreach (var property in AutoProperties.Properties)
				{
					for (int i = 0; i < auditColumns.Count; i++)
					{
						var column = auditColumns[i];
						if (property.ColumnName.EndsWith(column.columnName, StringComparison.OrdinalIgnoreCase))
						{
							yield return (column, property);
						}
					}
				}
			}
		}

		protected bool HasAuditColumns => AuditColumns.Count() == 4;

		static readonly ImmutableList<(string type, string interfaceProperty)> auditContext =
			ImmutableList.Create(
				("ZString", "SystemCreateBranch"),
				("ZString", "SystemCreateDepartment"));

		protected IEnumerable<(string type, string interfaceProperty, string columnName)> AuditContextColumns
		{
			get
			{
				foreach (var property in AutoProperties.Properties)
				{
					foreach (var prop in auditContext)
					{
						if (property.ColumnName.EndsWith(prop.interfaceProperty, StringComparison.OrdinalIgnoreCase))
						{
							yield return (prop.type, prop.interfaceProperty, property.ColumnName);
						}
					}
				}
			}
		}

		protected bool HasAuditContextColumns => AuditContextColumns.Count() == 2;

		protected bool HasIsCancelledColumn
		{
			get
			{
				foreach (AutoProperty property in AutoProperties.Properties)
				{
					if (property.ColumnName == IsCancelledColumnName)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Code for Body

		protected override string Body
		{
			get { return BodyOfSourceCode; }
		}

		protected string BodyOfSourceCode
		{
			get
			{
				if (fBodyOfSourceCode == null)
				{
					fBodyOfSourceCode = LinesOfCode(
						UsingClause,
						"",
						"namespace " + Info.Namespace,
						"{",
						"	[" + AutoGeneratedCodeAttribute + CodeAttribute + DescriptionAttribute + IsActiveAttribute + IsCancelledAttribute + PreventDeleteAttribute + MiscAttributes + "]",
						"	public abstract class " + Info.ClassNames.BusinessObjectAuto + " : " + InheritsFrom + ImplementsInterfaces,
						"	{",
								BodyOfClass,
						"	}",
						"}"
						);
				}

				return fBodyOfSourceCode;
			}
		}

		protected virtual string MiscAttributes => "";

		protected virtual string BodyOfClass
		{
			get
			{
				return LinesOfCode(
					SchemaOLD,
					"",
					CodeForConstructor,
					CodeForDataTable,
					CodeForPK,
					"",
					CodeForProperties,
					"",
					CodeForSetDefaultValues,
					"",
					CodeForLookupsProperty,
					"",
					CodeForValidation,
					CodeForAuditColumns,
					CodeForAuditContext
					);
			}
		}

		protected string fBodyOfSourceCode;

		#endregion

		#region Code for Using Clause

		protected virtual string UsingClause
		{
			get
			{
				return LinesOfCode(
					"using System;",
					"using System.Collections;",
					"using System.Data;",
					"",
					"using CargoWise.ComponentModel;",
					UsingCargoWiseEntityFramework,
					"using CargoWise.Schema;",
					"using CargoWise.Types;",
					UsingMasterFiles,
					UsingZArchitecture
					);
			}
		}

		protected string UsingMasterFiles
		{
			get
			{
				foreach (AutoProperty property in AutoProperties.Properties)
				{
					if (property is AutoPropertyFK && !Info.MasterFileReference)
					{
						return LinesOfCode("using Enterprise.MasterFiles.Business;");
					}
				}

				return null;
			}
		}

		protected string UsingCargoWiseEntityFramework
		{
			get
			{
				return (isCargoWiseEntityFrameworkNamespace) ?
					null
					: LinesOfCode("using CargoWise.EntityFramework;");
			}
		}

		protected string UsingZArchitecture
		{
			get
			{
				if (isCargoWiseEntityFrameworkNamespace)
				{
					return LinesOfCode(
						"using Enterprise.ZArchitecture.Schema;");
				}
				else
				{
					return LinesOfCode(
						"using Enterprise.ZArchitecture;",
						"using Enterprise.ZArchitecture.Business;",
						"using Enterprise.ZArchitecture.Schema;"
						);
				}
			}
		}

		protected string AutoGeneratedCodeAttribute
		{
			get { return "System.CodeDom.Compiler.GeneratedCode(\"CargoWise.EntityFramework\", \"1.0\")"; }
		}

		#endregion

		#region Code for PK

		protected virtual string CodeForPK
		{
			get
			{
				return LinesOfCode(
					"		#region PK",
					"",
					AutoProperties.CodeForPK,
					"",
					"		#endregion"
					);
			}
		}

		#endregion

		#region Code For Constructor

		protected string CodeForConstructor
		{
			get
			{
				if (Info.IsPersistent)
				{
					return LinesOfCode(
						"		protected " + Info.ClassNames.BusinessObjectAuto + "(BusinessObjectFactory Factory, DataRow Row) : base(Factory, Row)",
						"		{",
									CodeInConstructor,
						"		}");
				}
				else
				{
					return LinesOfCode(
						"		protected " + Info.ClassNames.BusinessObjectAuto + "(BusinessObjectFactory Factory) : base(Factory, GetNewRow(Factory))",
						"		{",
									CodeInConstructor,
						"		}",
						"",
						"		#region GetNewRow",
						"		protected static DataRow GetNewRow(BusinessObjectFactory Factory)",
						"		{",
						"			DataTable Table = ((INeedDataSet)Factory).Data.Tables[Schema.TableName];",
						"			if (Table == null)",
						"			{",
						"				Table = new NonPersistentDataTable();",
						"				((INeedDataSet)Factory).Data.Tables.Add(Table);",
						"			}",
						"			return Table.NewRow();",
						"		}",
						"		#endregion",
						"");
				}
			}
		}

		protected virtual string CodeInConstructor
		{
			get
			{
				var ccIgnoredProperties = AutoProperties.Properties.Where(x => !x.RequiresConcurrencyCheck);
				if (ccIgnoredProperties.Any())
				{
					return ccIgnoredProperties.Select(x =>
					FormattableString.Invariant($"			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof({x.ColumnName}), ConcurrencyPolicy.Ignore);")).Aggregate((x, y) => x + "\r\n" + y);
				}

				return null;
			}
		}

		#endregion

		#region Code For DataTable

		protected string CodeForDataTable
		{
			get
			{
				if (Info.IsPersistent)
				{
					return "";
				}
				else
				{
					return LinesOfCode(
						"		#region NonPersistentDataTable class (Including Column Definitions)",
						"		class NonPersistentDataTable : ZDataTable",
						"		{",
						"			public NonPersistentDataTable() : base(Schema.TableName)",
						"			{",
						CodeForColumns,
						"			}",
						"		}",
						"		#endregion",
						"");
				}
			}
		}

		protected string CodeForColumns
		{
			get
			{
				string result = "				DataColumn column;" + System.Environment.NewLine;
				foreach (DataColumn column in Info.Table.Columns)
				{
					string columnName = column.ColumnName;
					if (columnName.EndsWith("_PK"))
					{
						columnName = "PK";
					}
					result += "				column = Columns.Add(Schema." + columnName + ", typeof(" + column.DataType.ToString() + "));" + System.Environment.NewLine;
					if (column.MaxLength != -1)
					{
						result += "				column.MaxLength = " + column.MaxLength.ToString() + ";" + System.Environment.NewLine;
					}
					result += "				column.AllowDBNull = " + column.AllowDBNull.ToString().ToLower() + ";" + System.Environment.NewLine;
				}
				return result;
			}
		}
		#endregion

		#region Code for Properties

		protected virtual string CodeForProperties
		{
			get
			{
				return LinesOfCode(
					"		#region Properties",
							AutoProperties.CodeForProperties,
					"",
					"		#endregion"
					);
			}
		}

		#endregion

		#region Code for Set Default Values

		protected virtual string CodeForSetDefaultValues
		{
			get
			{
				return LinesOfCode(
					"		#region Set Default Values",
					"",
					"		protected override void SetDefaultValues()",
					"		{",
					"			base.SetDefaultValues();",
					"",
					"			DataRow row = ((IBusinessObjectInternals)this).Row;",
								AutoProperties.CodeForDefaultValues,
					"		}",
					"",
					"		#endregion"
					);
			}
		}

		#endregion

		#region Code for Lookups Property

		protected virtual string CodeForLookupsProperty
		{
			get
			{
				return LinesOfCode(
					"		#region Lookups",
					"",
					"		public " + Info.ClassNames.Lookups + " Lookups",
					"		{",
					"			get",
					"			{",
					"				if (fLookups == null || !IsLookupsCachedInBase)",
					"				{",
					"					fLookups = GetNewLookups();",
					"				}",
					"",
					"				return fLookups;",
					"			}",
					"		}",
					"",
					"		protected virtual " + Info.ClassNames.Lookups + " GetNewLookups()",
					"		{",
					"			return new " + Info.ClassNames.Lookups + "(this);",
					"		}",
					"",
					"		" + Info.ClassNames.Lookups + " fLookups;",
					"",
					"		#endregion"
					);
			}
		}

		#endregion

		#region Code for Validation

		protected virtual string CodeForValidation
		{
			get
			{
				return LinesOfCode(
					"		#region Validation",
					"",
					"		protected override void RunPreSaveValidationCore()",
					"		{",
					"			Validation.ValidateAll();",
					"			base.RunPreSaveValidationCore(); // call RunPreSaveValidationCore() on all children then fire OnNotificationsChanged()",
					"		}",
					"",
					"		public " + Info.ClassNames.Validation + " Validation",
					"		{",
					"			get { return GetNewValidation(); }",
					"		}",
					"",
					"		protected virtual " + Info.ClassNames.Validation + " GetNewValidation()",
					"		{",
					"			return new " + Info.ClassNames.Validation + "(this);",
					"		}",
					"",
					"		#endregion"
					);
			}
		}

		#endregion

		#region CodeForAuditColumns

		protected virtual string CodeForAuditColumns
		{
			get
			{
				if (HasAuditColumns)
				{
					return LinesOfCode(
						"",
						"		#region IAuditDetails",
						LinesOfCode(CodeForAuditColumnsCore),
						"",
						"		#endregion"
						);
				}
				else
				{
					return null;
				}
			}
		}

		string[] CodeForAuditColumnsCore
		{
			get
			{
				var result = new string[4];
				var i = 0;
				foreach (var col in AuditColumns.OrderBy(e => e.column.columnName))
				{
					result[i] = LinesOfCode(
							"",
							$"		{col.column.type} IAuditDetails.{col.column.columnName}",
							"		{",
							$"			get {{ return {col.property.ColumnName}; }}",
							"		}");

					i++;
				}

				return result;
			}
		}

		protected virtual string CodeForAuditContext
		{
			get
			{
				if (HasAuditContextColumns)
				{
					return LinesOfCode(
						"",
						"		#region IAuditDetailsWithContext",
						LinesOfCode(CodeForAuditContextColumnsCore),
						"",
						"		#endregion");
				}
				else
				{
					return null;
				}
			}
		}

		string[] CodeForAuditContextColumnsCore
		{
			get
			{
				var result = new string[2];
				var i = 0;
				foreach (var col in AuditContextColumns.OrderBy(e => e.columnName))
				{
					result[i] = LinesOfCode(
						"",
						$"		{col.type} IAuditDetailsWithContext.{col.interfaceProperty}",
						"		{",
						$"			get {{ return {col.columnName}; }}",
						"		}");
					i++;
				}

				return result;
			}
		}

		#endregion

		protected readonly BusinessObjectInfo Info;
		protected AutoPropertyList fAutoProperties;
		protected readonly bool isCargoWiseEntityFrameworkNamespace;
	}
}
