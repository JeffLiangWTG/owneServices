using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public delegate string GetPropertyName(Type workflowType);

	public abstract class JobDetailFilter : ModuleTextFilter
	{
		internal JobDetailFilter(ZString description, GetPropertyName propertyNameDelegate, GetList workflowTypeListDelegate)
			: base(description, (a, b) => new ZQuery(), workflowTypeListDelegate)
		{
			this.propertyNameDelegate = propertyNameDelegate;
		}

		readonly GetPropertyName propertyNameDelegate;

		public static class Schema
		{
			public const string WorkflowTypeCode = "WorkflowTypeCode";
		}

		#region Workflow Type Code

		[BusinessObjectTestExclude] // maxlength comes from lookups.
		public ZString WorkflowTypeCode
		{
			get { return workflowTypeCode; }
			set { SetNonPersistentPropertyValue(WorkflowTypeCodeInfo, ref workflowTypeCode, value); }
		}
		ZString workflowTypeCode;
		public ZPropertyInfo WorkflowTypeCodeInfo { get { return GetZPropertyInfo(Schema.WorkflowTypeCode); } }

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.WorkflowTypeCode, WorkflowTypeCode);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			WorkflowTypeCode = reader.ReadElementString(Schema.WorkflowTypeCode);
		}

		#endregion

		protected override ZQuery RunQueryDelegate()
		{
			var job = Property;

			WorkflowDescriptor descriptor;
			if (WorkflowDescriptors.Instance.TryGetValue(WorkflowTypeCode, out descriptor))
			{
				var workflowType = descriptor.WorkflowProviderType;
				var columnName = JobTextPropertyFilter.SchemaColumnFromPropertyName(propertyNameDelegate(workflowType));
				if (columnName != null)
				{
					if (job.Length > columnName.MaxLength)
					{
						job = job.Substring(0, columnName.MaxLength);
					}
					var result = new ZDBOnlyQuery(typeof(ProcessHeader));
					var jobSubQuery = new ZDBOnlySubQuery(workflowType, ProcessHeaderSchema.FH_ParentId);
					jobSubQuery.AddToFilter(columnName, SqlComparisonOperator, job);
					result.AddSubQuery(jobSubQuery, JoinCondition.And);
					return result;
				}
			}

			return new ZQuery();
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new JobDetailFilterValidation(this);
		}

		class JobDetailFilterValidation : ModuleTextFilterValidation
		{
			internal JobDetailFilterValidation(ModuleTextBaseFilter parent)
				: base(parent)
			{
			}

			protected override void CheckProperty()
			{
				var converted = (JobDetailFilter)Parent;
				WorkflowDescriptor descriptor;
				if (WorkflowDescriptors.Instance.TryGetValue(converted.WorkflowTypeCode, out descriptor))
				{
					var column = JobTextPropertyFilter.SchemaColumnFromPropertyName(converted.propertyNameDelegate(descriptor.WorkflowProviderType));

					if (column != null && column.HasMaxLength && Parent.Property.Length > column.MaxLength)
					{
						var truncatedResult = Parent.Property.Substring(0, column.MaxLength);
						Parent.PropertyInfo.AddWarning(Res.GetString("f63053d1-cae9-4512-9f18-e9b4034ba3b2", "Field input is too long, searching for: \"{0}\" instead.", truncatedResult));
					}
				}
			}
		}
	}
}
