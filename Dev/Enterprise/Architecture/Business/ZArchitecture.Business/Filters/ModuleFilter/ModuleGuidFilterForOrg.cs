using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public class ModuleGuidFilterForOrg : ModuleGuidFilter
	{
		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, IBusinessObjectCollection list) : base(description, id, filterColumn, list)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, GetGuidQuery queryDelegate, IBusinessObjectCollection list) : base(description, id, queryDelegate, list)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, GetList listDelegate) : base(description, id, filterColumn, listDelegate)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, GetGuidQuery queryDelegate, GetList listDelegate) : base(description, id, queryDelegate, listDelegate)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, SchemaColumn filterColumn, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate) : base(description, id, filterColumn, listUsingCurrentModuleFilterDelegate)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, GetGuidQueryWithOperator queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate = null) : base(description, id, queryDelegate, list, filterColumnForDelegate)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, GetGuidQueryWithOperatorSupportsFiltersMatch queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate) : base(description, id, queryDelegate, list, filterColumnForDelegate)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, GetGuidQueryWithOperatorAndOption queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate = null) : base(description, id, queryDelegate, list, filterColumnForDelegate)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, GetGuidQueryWithOperator queryDelegate, GetList listDelegate, SchemaColumn filterColumnForDelegate = null) : base(description, id, queryDelegate, listDelegate, filterColumnForDelegate)
		{
		}

		public ModuleGuidFilterForOrg(ZString description, ModuleIdentifier id, GetGuidQueryWithOperatorSupportsFiltersMatch queryDelegate, GetList listDelegate, SchemaColumn filterColumnForDelegate) : base(description, id, queryDelegate, listDelegate, filterColumnForDelegate)
		{
		}

		protected ModuleGuidFilterForOrg(FilterCategory category, ModuleFilterCollection parentCollection) : base(category, parentCollection)
		{
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleGuidFilterForOrgValidation(this);
		}
	}

	public class ModuleGuidFilterForOrgValidation : ModuleGuidFilterValidation
	{
		public ModuleGuidFilterForOrgValidation(ModuleGuidFilterForOrg parent)
			: base(parent)
		{
		}

		ReadOnlyBusinessObjectFactory Factory => factory ?? (factory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory factory;

		#region ValidateProperty

		protected override void CheckProperty()
		{
			base.CheckProperty();

			if (Parent.Property.IsValid)
			{
				var org = ((Parent.List as IBusinessObjectCollection)?.Factory ?? Factory).Load<IOrgHeader>(Parent.Property);
				if (org != null && !org.OH_IsActive)
				{
					Parent.PropertyInfo.AddWarning(CommonValidationMessage.WarningMessage);
				}
			}
		}

		#endregion
	}

	public class ModuleGuidsFilterForOrg : ModuleGuidsFilter
	{
		public ModuleGuidsFilterForOrg(ZString description, ModuleIdentifier iD, GetGuidsQuery queryDelegate, IBusinessObjectCollection list1, IBusinessObjectCollection list2) : base(description, iD, queryDelegate, list1, list2)
		{
		}

		public ModuleGuidsFilterForOrg(ZString description, ModuleIdentifier iD, SchemaGuidColumn filterColumn1, IBusinessObjectCollection list1, SchemaGuidColumn filterColumn2, IBusinessObjectCollection list2) : base(description, iD, filterColumn1, list1, filterColumn2, list2)
		{
		}

		protected ModuleGuidsFilterForOrg(FilterCategory category, ModuleFilterCollection parentCollection) : base(category, parentCollection)
		{
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleGuidsFilterValidationForOrg(this);
		}
	}

	public class ModuleGuidsFilterValidationForOrg : ModuleGuidsFilterValidation
	{
		public ModuleGuidsFilterValidationForOrg(ModuleGuidsFilterForOrg parent) : base(parent)
		{
		}

		ReadOnlyBusinessObjectFactory Factory => factory ?? (factory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory factory;

		protected override void CheckProperty1()
		{
			base.CheckProperty1();
			CheckInactiveOrg(Parent.Property1, Parent.Property1Info, Parent.List1);
		}

		protected override void CheckProperty2()
		{
			base.CheckProperty2();
			CheckInactiveOrg(Parent.Property2, Parent.Property2Info, Parent.List2);
		}

		void CheckInactiveOrg(ZGuid property, ZPropertyInfo info, IList list)
		{
			if (property.IsValid)
			{
				var org = ((list as IBusinessObjectCollection)?.Factory ?? Factory).Load<IOrgHeader>(property);
				if (org != null && !org.OH_IsActive)
				{
					info.AddWarning(CommonValidationMessage.WarningMessage);
				}
			}
		}
	}

	static class CommonValidationMessage
	{
		internal static string WarningMessage => Res.GetString("1c4ff2a0-d81a-4d2a-ba64-038a9136c941", "Organization is in-active.");
	}
}
