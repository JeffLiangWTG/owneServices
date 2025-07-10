using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	public class CACFilterStripBusinessObject : FilterStripBusinessObject
	{
		public CACFilterStripBusinessObject(AddFiltersDelegate addFiltersDelegate) : base()
		{
			fAddFiltersDelegate = addFiltersDelegate;
		}
		readonly AddFiltersDelegate fAddFiltersDelegate;

		public CACFilterStripBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			if (fAddFiltersDelegate != null)
			{
				fAddFiltersDelegate(result);
			}
			return result;
		}

		public delegate void AddFiltersDelegate(ModuleFilterCollection moduleFilters);

		protected override void ApplyInitialCode(ZString code, string propertyName)
		{
			if (propertyName == CACClassSchema.CT_Tariff.Name)
			{
				ApplyInitialCode(CACClassModule.TariffCaption, code);
			}
			else if (propertyName == CACExportTariffSchema.CE_Code.Name)
			{
				ApplyInitialCode(CACExportTariffModule.TariffCaption, code);
			}
			else
			{
				base.ApplyInitialCode(code, propertyName);
			}
		}

		void ApplyInitialCode(string filterName, string property)
		{
			foreach (var moduleFilter in ModuleFilters.ToSortedArrayWithIsExclusiveLast())
			{
				if (moduleFilter.Code == filterName)
				{
					var moduleTextFilter = moduleFilter as ModuleTextFilter;
					if (moduleTextFilter != null)
					{
						moduleTextFilter.Property = property;
						moduleTextFilter.Visibility = FilterVisibility.AlwaysVisible;
					}
				}
			}
		}
	}
}
