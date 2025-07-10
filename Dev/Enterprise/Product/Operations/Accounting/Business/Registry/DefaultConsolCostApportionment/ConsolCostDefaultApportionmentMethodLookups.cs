using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business
{
	public class ConsolCostDefaultApportionmentMethodLookups : ZLookups
	{
		public ConsolCostDefaultApportionmentMethodLookups(ConsolCostDefaultApportionmentMethod parent) : base(parent)
		{
		}

		new ConsolCostDefaultApportionmentMethod Parent => (ConsolCostDefaultApportionmentMethod)base.Parent;

		public CodeDescriptionPairList ApportionmentList => ApportionmentMethodOverrideLookupsHelper.ApportionmentList(Parent.Module);

		public CodeDescriptionPairList ConsolTypeList => ApportionmentMethodOverrideLookupsHelper.ConsolTypeList(Parent.TransportMode, Parent.Module);

		public CodeDescriptionPairList DirectionList => ApportionmentMethodOverrideLookupsHelper.DirectionList(Parent.Module);

		public CodeDescriptionPairList ModuleList => ApportionmentMethodOverrideLookupsHelper.ModuleList;

		public CodeDescriptionPairList ContainerModeList => ApportionmentMethodOverrideLookupsHelper.ContainerModeList(Parent.ConsolType, Parent.TransportMode, Parent.Module);

		public CodeDescriptionPairList TransportModeList => ApportionmentMethodOverrideLookupsHelper.TransportModeList(Parent.Module);
	}
}
