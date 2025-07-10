using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels
{
	public abstract class OverrideLevelWithDepartments : IOverrideLevel
	{
		readonly BusinessObjectFactory factory = new BusinessObjectFactory();

		readonly Guid companyPK;
		readonly Guid branchPK;
		readonly string pathRelativeToParent;
		readonly string description;
		readonly RegistryStorageFlags departmentStorageFlag;
		protected readonly bool hideInactiveChildren;

		internal OverrideLevelWithDepartments(string description, string pathRelativeToParent, Guid companyPK, Guid branchPK, RegistryStorageFlags departmentStorageFlag, bool hideInactiveChildren)
		{
			this.description = description;
			this.pathRelativeToParent = pathRelativeToParent;
			this.companyPK = companyPK;
			this.branchPK = branchPK;
			this.departmentStorageFlag = departmentStorageFlag;
			this.hideInactiveChildren = hideInactiveChildren;
		}

		public IEnumerable<IOverrideLevel> Children
		{
			get
			{
				var query = hideInactiveChildren ? new ZQuery(GlbDepartmentSchema.GE_IsActive, true) : new ZQuery();
				var departments = factory.Load(ObjectFactory.GetType<IGlbDepartment>(), query);
				var departmentOverrides = departments.Select(d => new DepartmentOverrideLevel(this, (IDepartment)d, companyPK, branchPK, departmentStorageFlag));
				return OtherChildren.Concat(departmentOverrides);
			}
		}

		public object GetValueOf(IRegistryItem item)
		{
			return item.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		public void SetValueOf(IRegistryItem item, object value)
		{
			var companyPktoUse = (branchPK != Guid.Empty) ? Guid.Empty : companyPK;
			item.SetValue(companyPktoUse, branchPK, Guid.Empty, value);
		}

		public FallbackLevel GetFallbackLevel()
		{
			return new FallbackLevel(companyPK, branchPK, Guid.Empty);
		}

		public Guid GetPkOfEntry(IRegistryItem item)
		{
			var companyPktoUse = (branchPK != Guid.Empty) ? Guid.Empty : companyPK;
			return ((IRegistryItemInternals)item).GetRegistryItemPK(companyPktoUse, branchPK, Guid.Empty);
		}

		public string PathRelativeToParent => pathRelativeToParent;
		public string Description => description;

		protected abstract IEnumerable<IOverrideLevel> OtherChildren { get; }
		public abstract bool CanSetValueOf(IRegistryItem item);
		public abstract IOverrideLevel Parent { get; }
	}

	class DepartmentOverrideLevel : IOverrideLevel
	{
		readonly IOverrideLevel parent;
		readonly Guid companyPK;
		readonly Guid branchPK;
		readonly IDepartment department;
		readonly RegistryStorageFlags storage;

		public DepartmentOverrideLevel(IOverrideLevel parent, IDepartment department, Guid companyPk, Guid branchPk, RegistryStorageFlags storage)
		{
			this.parent = parent;
			this.department = department;
			companyPK = companyPk;
			branchPK = branchPk;
			this.storage = storage;
		}

		public string PathRelativeToParent => Res.GetString("8AAD3F11-9747-4B8E-8E60-F8029E2F0B3B", "Departments");
		public string Description => department.Description;
		public IOverrideLevel Parent => parent;
		public IEnumerable<IOverrideLevel> Children => Enumerable.Empty<IOverrideLevel>();

		public bool CanSetValueOf(IRegistryItem item) => item.Storage.HasFlag(storage);
		public FallbackLevel GetFallbackLevel() => new FallbackLevel(companyPK, branchPK, department.PK);

		public object GetValueOf(IRegistryItem item)
		{
			return item.GetFallBackValueAtAllLevels(companyPK, branchPK, department.PK);
		}

		public void SetValueOf(IRegistryItem item, object value)
		{
			var companyPktoUse = (branchPK != Guid.Empty) ? Guid.Empty : companyPK;
			item.SetValue(companyPktoUse, branchPK, department.PK, value);
		}

		public Guid GetPkOfEntry(IRegistryItem item)
		{
			var companyPktoUse = (branchPK != Guid.Empty) ? Guid.Empty : companyPK;
			return ((IRegistryItemInternals)item).GetRegistryItemPK(companyPktoUse, branchPK, department.PK);
		}
	}
}
