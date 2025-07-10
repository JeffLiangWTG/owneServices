using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionFieldDescriptor : AutoOperationalActionFieldDescriptor, IXmlSerializable
	{
		public OperationalActionFieldDescriptor(OperationalAction action)
			: base(action.Factory)
		{
			this.action = Argument.NotNull(action, "action");
			this.context = Argument.NotNull(action.Context, "action.Context");
		}

		public OperationalAction Action
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return action; }
		}

		public OperationalActionContext Context
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return context; }
		}

		public ICollection<ZString> TemplateNamesForCustomFieldNotInCurrentContext
		{
			get
			{
				if (string.IsNullOrEmpty(context.WorkflowType) || FieldName.IsEmpty)
				{
					templateNamesForCustomFieldNotInCurrentContext = null;

					return new List<ZString>();
				}

				if (templateNamesForCustomFieldNotInCurrentContext == null)
				{
					var queryDefinitions = new ZDBOnlySubQuery(typeof(GenCustomColumnDefinition), GenCustomColumnDefinitionSchema.XC_ParentID);
					queryDefinitions.AddToFilter(GenCustomColumnDefinitionSchema.XC_ParentTableCode, ProcessTaskTemplateSchema.Constants.Prefix);
					queryDefinitions.AddToFilter(GenCustomColumnDefinitionSchema.XC_Name, FieldName);

					var queryTemplates = new ZDBOnlyQuery(typeof(ProcessTaskTemplate));
					queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, context.WorkflowType);
					queryTemplates.AddSubQuery(ProcessTaskTemplateSchema.PK, queryDefinitions, JoinCondition.And);

					var newFactory = new BusinessObjectFactory();
					var processTaskTemplates = newFactory.Load<ProcessTaskTemplate>(queryTemplates);

					return processTaskTemplates.Select(s => s.P0_Name).ToList();
				}

				return templateNamesForCustomFieldNotInCurrentContext;
			}
		}

		ICollection<ZString> templateNamesForCustomFieldNotInCurrentContext;

		public OperationalActionFieldSupporter FieldSupporter
		{
			get
			{
				if (fieldSupporter == null && context != null)
				{
					fieldSupporter = context.FieldSupporters[FieldName];
				}
				return fieldSupporter;
			}
		}

		public TValue GetDefaultValue<TValue>()
			where TValue : struct, IZType
		{
			IZType value = Lookups.DefaultStrategy_List.GetDefaultValue(DefaultingStrategy, DefaultValue);
			return (value == null) ? default(TValue) : (TValue)value;
		}

		#region Strategies

		public OperationalActionFieldDescriptorLookups Lookups
		{
			get { return lookups ?? (lookups = new OperationalActionFieldDescriptorLookups(this)); }
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			foreach (IBusinessObjectCollection collection in ParentCollections)
			{
				((IBusinessObjectCollectionInternals)collection).HasChangesFromDelete = true;
			}
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EmptyBehaviour = EmptyBehaviourList.Codes.Skip;
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || OperationalActionMenuEditableHelper.ReadOnly(action); }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Bound Properties

		public override ZString FieldName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.FieldName; }
			set
			{
				var oldValue = base.FieldName;
				fieldSupporter = null;
				base.FieldName = value;

				if (oldValue != value.Trim())
				{
					DefaultingStrategy = ZString.Empty;
					templateNamesForCustomFieldNotInCurrentContext = null;
				}

				if (FieldCaption.IsEmpty || FieldCaption == GetDefaultCaption(oldValue))
				{
					FieldCaption = GetDefaultCaption(value);
				}
			}
		}

		[ReadOnlyMemberAttribute(nameof(DefaultingStrategy_ReadOnly))]
		[List("Lookups.DefaultStrategy_List")]
		public override ZString DefaultingStrategy
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.DefaultingStrategy; }
			set
			{
				base.DefaultingStrategy = value;
				DefaultValue = ZString.Empty;
			}
		}

		bool DefaultingStrategy_ReadOnly
		{
			get { return Lookups.DefaultStrategy_List.IsReadOnly || Lookups.DefaultStrategy_List.Count <= 0; }
		}

		[List("Lookups.DefaultValue_List")]
		public override ZString DefaultValue
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.DefaultValue; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.DefaultValue = value; }
		}
		protected override bool DefaultValue_ReadOnly
		{
			get { return !Lookups.DefaultStrategy_List.UsesDetail(DefaultingStrategy); }
		}

		protected override int DefaultValue_MaxLength
		{
			get
			{
				var maxLength = Lookups.DefaultStrategy_List.DetailMaxLength(DefaultingStrategy);

				if (maxLength > 0)
				{
					return maxLength;
				}

				if (!DefaultingStrategy.IsEmpty && DefaultValue_ReadOnly)
				{
					return DefaultValue.IsEmpty ? int.MaxValue : DefaultValue.Length;
				}

				return 1;
			}
		}

		public override ZString DefaultValueFieldType
		{
			get { return Lookups.DefaultStrategy_List.DetailFieldType(DefaultingStrategy).ToString(); }
		}

		public override ZByte Order
		{
			get => base.Order;

			set
			{
				if (value != base.Order)
				{
					var oldOrder = base.Order;
					base.Order = value;

					foreach (var collection in ParentCollections)
					{
						if (collection is OperationalActionFieldDescriptorCollection operationalActionFieldDescriptorCollection)
						{
							foreach (var field in operationalActionFieldDescriptorCollection.FindElementsByOrder(oldOrder))
							{
								((IBusinessObjectInternals)field).Validate(OperationalActionFieldDescriptor.Schema.Order);
							}
							foreach (var field in operationalActionFieldDescriptorCollection.FindElementsByOrder(value))
							{
								((IBusinessObjectInternals)field).Validate(OperationalActionFieldDescriptor.Schema.Order);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Implementation

		string GetDefaultCaption(string field)
		{
			string result = null;

			if (Context != null)
			{
				PropertyInfo[] path = ReflectionHelper.FieldTextToPath(Context.Supporter.RootType, field);
				if (path != null)
				{
					PropertyInfo lastInfo = path[path.Length - 1];
					var data = ResourceStringHelper.GetData(lastInfo);

					if (data != null)
					{
						result = data.Caption;
					}
				}
				else
				{
					result = field;
				}
			}

			return result ?? "";
		}

		#endregion

		OperationalActionFieldDescriptorLookups lookups;
		OperationalActionFieldSupporter fieldSupporter;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalAction action;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalActionContext context;
	}
}
