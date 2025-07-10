using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMComponent)]
	public class BMComponentCollection : ActiveBusinessObjectCollection<BMComponent>, IBMComponentCollection
	{
		public BMComponentCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public BMComponentCollection(BusinessObjectFactory factory, ZQuery additionalFilter, bool excludeRelationships = true)
			: base(factory, excludeRelationships ? AddTypeFilter(additionalFilter) : additionalFilter)
		{
		}

		public BMComponentCollection(BMSystem system)
			: base(system.Factory, system, new ZQuery(BMComponentSchema.FC_FC_ParentComponent, null), BMComponentSchema.FC_FS_System)
		{
		}

		public BMComponentCollection(BMComponent parent)
			: base(parent.Factory, parent, new ZQuery(), BMComponentSchema.FC_FC_ParentComponent)
		{
		}

		protected override void SetDefaultsForNewElementCore(BMComponent newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (newElement.FC_FC_ParentComponent.IsValid)
			{
				newElement.FC_Type = BMComponentTypeList.Codes.Buffer;
			}
		}

		static ZQuery AddTypeFilter(ZQuery additionalFilter)
		{
			ZQuery result = new ZQuery(BMComponentSchema.FC_Type, SQLComparisonOperator.NotEqual, BMComponentTypeList.Codes.ComponentRelationship);

			if (additionalFilter != null && !additionalFilter.IsEmpty)
			{
				result.AddToFilter(additionalFilter, JoinCondition.And);
			}

			return result;
		}

		internal void AddComponentLookupFilterBusinessObjectDefaults(BMComponent component, ZGuid? boardSystemPK)
		{
			ZGuid? systemPK = boardSystemPK;

			if (component != null)
			{
				var code = CodePropertyAttribute.CodeFromBusinessObject(component);
				if (!code.IsEmpty)
				{
					systemPK = component.FC_FS_System;
				}
			}

			if (systemPK != null)
			{
				var componentDefault = new FilterBusinessObjectDefault("BMSystem", "Property", systemPK, true);
				this.FilterBusinessObjectDefaults.Add(componentDefault);
			}
		}

		internal void AddComponentLookupFilterBusinessObjectDefaults(ZString type)
		{
			if (!type.IsEmpty)
			{
				var componentDefault = new FilterBusinessObjectDefault("Component Type", "Property", type, true);
				this.FilterBusinessObjectDefaults.Add(componentDefault);
			}
		}
	}
}
