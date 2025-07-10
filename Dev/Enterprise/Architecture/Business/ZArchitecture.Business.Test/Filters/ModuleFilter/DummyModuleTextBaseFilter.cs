using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyModuleTextBaseFilter : ModuleTextBaseFilter
	{
		public DummyModuleTextBaseFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public DummyModuleTextBaseFilter(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public ZString DummyProperty
		{
			get { return fDummyProperty; }
			set
			{
				if (DummyProperty != value)
				{
					InvalidateCachedQuery();
				}

				fDummyProperty = value;
			}
		}

		ZString fDummyProperty;

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new DummyModuleFilter("", FilterColumn);
		}

		public int QueryDelegateCounter;
		protected override ZQuery RunQueryDelegate()
		{
			QueryDelegateCounter++;
			return base.RunQueryDelegate();
		}

		public int QueryUsingFilterColumnsCounter;
		protected override ZQuery GetQueryUsingFilterColumns()
		{
			QueryUsingFilterColumnsCounter++;
			return new ZQuery(FilterColumn, DummyProperty);
		}
	}
}
