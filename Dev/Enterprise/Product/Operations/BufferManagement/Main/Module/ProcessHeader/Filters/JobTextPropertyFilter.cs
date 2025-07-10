using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class JobTextPropertyFilter : ModuleTextBaseFilter
	{
		public JobTextPropertyFilter(JobTextPropertyQuery queryDelegate, GetList workflowTypeListDelegate)
			: base(ProcessHeader.ModuleFilterConstants.JobProperty, queryDelegate, workflowTypeListDelegate)
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|JobProperty", "Job Property");
		}

		public static class Schema
		{
			public const string JobPropertyName = "JobPropertyName";
			public const string JobPropertyNameList = "JobPropertyNameList";
			public const string WorkflowTypeCode = "WorkflowTypeCode";
		}

		#region Workflow Type Code

		[BusinessObjectTestExclude] // don't need a maxlength
		public ZString WorkflowTypeCode
		{
			get { return workflowTypeCode; }
			set { SetNonPersistentPropertyValue(WorkflowTypeCodeInfo, ref workflowTypeCode, value); }
		}
		ZString workflowTypeCode;
		public ZPropertyInfo WorkflowTypeCodeInfo { get { return GetZPropertyInfo(Schema.WorkflowTypeCode); } }

		#endregion

		#region Job Property Name

		[BusinessObjectTestExclude] // don't need a maxlength
		public ZString JobPropertyName
		{
			get { return jobPropertyName; }
			set { SetNonPersistentPropertyValue(JobPropertyNameInfo, ref jobPropertyName, value); }
		}
		ZString jobPropertyName;
		public ZPropertyInfo JobPropertyNameInfo { get { return GetZPropertyInfo(Schema.JobPropertyName); } }

		public CodeDescriptionPairList JobPropertyNameList
		{
			get
			{
				if (jobPropertyNameListWorkflowType != WorkflowTypeCode || jobPropertyNameList == null)
				{
					jobPropertyNameListWorkflowType = WorkflowTypeCode;
					jobPropertyNameList = CreateJobPropertyNameList();
				}
				return jobPropertyNameList;
			}
		}
		CodeDescriptionPairList jobPropertyNameList;
		string jobPropertyNameListWorkflowType;

		CodeDescriptionPairList CreateJobPropertyNameList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			string workflowType = WorkflowTypeCode;
			WorkflowDescriptor descriptor;
			if (WorkflowDescriptors.Instance.TryGetValue(workflowType, out descriptor))
			{
				var schema = SchemaFromType(descriptor.WorkflowProviderType);
				foreach (SchemaColumn column in schema.All)
				{
					if (column.ColumnType == SchemaColumnType.String)
					{
						result.AddPair(column.Name);
					}
				}
			}

			return result;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.WorkflowTypeCode, WorkflowTypeCode);
			writer.WriteElementString(Schema.JobPropertyName, JobPropertyName);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			WorkflowTypeCode = reader.ReadElementString(Schema.WorkflowTypeCode);
			JobPropertyName = reader.ReadElementString(Schema.JobPropertyName);
		}

		#endregion

		#region Implementation

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, WorkflowTypeCode, JobPropertyName, Property }; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return null;
		}

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new JobTextPropertyFilterValidation(this);
		}

		class JobTextPropertyFilterValidation : ModuleTextFilterValidation
		{
			public JobTextPropertyFilterValidation(JobTextPropertyFilter parent)
				: base(parent)
			{
				this.Parent = parent;
			}

			protected readonly new JobTextPropertyFilter Parent;

			protected override void CheckProperty()
			{
				if (Parent.JobPropertyName != ZString.Empty && Parent.JobPropertyName.Contains("_", StringComparison.Ordinal))
				{
					var schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(Parent.jobPropertyName.Substring(0, Parent.jobPropertyName.IndexOf('_')));
					var column = schema?.GetSchemaColumn(Parent.jobPropertyName);

					if (column != null && column.HasMaxLength && Parent.Property.Length > column.MaxLength)
					{
						var truncatedResult = Parent.Property.Substring(0, column.MaxLength);
						Parent.PropertyInfo.AddWarning(Res.GetString("abf88723-c573-4095-81b1-900dd90a9bba", "Job Property is too long, searching for: \"{0}\" instead.", truncatedResult));
					}
				}
			}

			public void ValidateWorkflowTypeCode()
			{
				ValidateCalculatedProperty(Parent.WorkflowTypeCodeInfo);
			}

			protected virtual void CheckWorkflowTypeCode()
			{
				MandatoryValidation.CheckEntered(Parent.WorkflowTypeCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.WorkflowTypeCodeInfo, (ICodeDescriptionPairList)Parent.List);
			}

			public void ValidateJobPropertyName()
			{
				ValidateCalculatedProperty(Parent.JobPropertyNameInfo);
			}

			protected virtual void CheckJobPropertyName()
			{
				MandatoryValidation.CheckEntered(Parent.JobPropertyNameInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JobPropertyNameInfo, Parent.JobPropertyNameList);
			}

			public override void ValidateAll()
			{
				ValidateWorkflowTypeCode();
				ValidateJobPropertyName();
				base.ValidateAll();
			}
		}

		#endregion

		#region Static Methods

		static ITableSchema SchemaFromType(Type type)
		{
			try
			{
				return BusinessObjectFactory.GetTableSchemaFromType(type);
			}
			catch (ZException)
			{
			}

			var codeColumn = SchemaColumnFromPropertyName(SafeCodePropertyNameFromType(type));
			return codeColumn != null ? codeColumn.TableSchema : null;
		}

		internal static SchemaColumn SchemaColumnFromPropertyName(string propertyName)
		{
			SchemaColumn result = null;

			if (propertyName != null && propertyName.Contains("_"))
			{
				var schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(propertyName.Substring(0, propertyName.IndexOf('_')));
				if (schema != null)
				{
					result = schema.GetSchemaColumn(propertyName);
				}
			}

			return result;
		}

		internal static string SafeCodePropertyNameFromType(Type type)
		{
			try
			{
				return CodePropertyAttribute.CodePropertyNameFromType(type);
			}
			catch (NoCodePropertyException)
			{
				return null;
			}
		}

		internal static string SafeDescriptionPropertyNameFromType(Type type)
		{
			try
			{
				return DescriptionPropertyAttribute.DescriptionPropertyNameFromType(type);
			}
			catch (NoCodePropertyException)
			{
				return null;
			}
		}

		#endregion
	}
}
