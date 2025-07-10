using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Business
{
	public class MoveInDestinationLookups : CusSupportingInfoLookups
	{
		public MoveInDestinationLookups(MoveInDestination parent) : base(parent)
		{
		}

		new MoveInDestination Parent => (MoveInDestination)base.Parent;

		public override ICollection CodeList => GetRefCusCodeLisCollection(Factory, Parent.Parent, false);

		public ICollection ViaList => GetRefCusCodeLisCollection(Factory, Parent.Parent, true);

		public static ZZRefCusCodeListCombinedCollection GetRefCusCodeLisCollection(BusinessObjectFactory factory, CusEntryInstruction cusEntryInstruction, bool isCountryAndTypeRemovable)
		{
			var today = ZDateTime.Today;
			var jobDeclaration = cusEntryInstruction.JobDeclaration;
			var customsOfficeFirstChar = jobDeclaration.JE_CustomsOffice.SubstringSafe(0, 1);
			var transportMode = jobDeclaration.JE_TransportMode;

			var result = factory.GetCachedValue($"JP.MoveInDestinationLookups.GetRefCusCodeLisCollection-{today.ToShortDateString()}-{customsOfficeFirstChar}", () =>
			{
				var refCusCodeListCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, today);

				refCusCodeListCollection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
				filterName: Universal.Constants.ZZRefCusCodeListFilters.Code,
				propertyName: "Property",
				value: customsOfficeFirstChar,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.StartsWith));

				return refCusCodeListCollection;
			});

			result.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
				filterName: Universal.Constants.ZZRefCusCodeListFilters.TransportMode,
				propertyName: "Property",
				value: transportMode,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact));

			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode), isCountryAndTypeRemovable));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.Japan), isCountryAndTypeRemovable));

			return result;
		}
	}
}
