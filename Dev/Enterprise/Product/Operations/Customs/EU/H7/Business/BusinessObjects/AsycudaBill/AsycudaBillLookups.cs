using System.Collections;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(ASYCUDA.Business.AsycudaBill parent)
			: base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override CodeDescriptionPairList CustomsEntryNumberTypes =>
			MessagingProvider.GetCachedBillEntryNumberTypeList(Factory);

		public new ICollection ShipperCountries => CountryList;

		public new ICollection ConsigneeCountries => CountryList;

		public new ICollection SellerCountries => CountryList;

		protected virtual ICollection CountryList => new RefCountryCollection(Factory);

		public override CodeDescriptionPairList IncotermList => Factory.GetCachedValue("Enterprise.Customs.EU.H7.Business.AsycudaBillLookups.IncotermCodeList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));

		public virtual CodeDescriptionPairList AdditionalProcedureList => Factory.GetCachedValue<EUH7AdditionalProcedureCodeList>();

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				return Factory.GetCachedValue("EUH7AsycudaBill.Lookups.ABL_ContainerMode." + Parent.Header.AMA_TransportMode, delegate
				{
					var list = FreightCodePairLists.JS_PackingModeList(Parent.Header.AMA_TransportMode);

					if (Parent.Header.AMA_TransportMode != Core.Constants.TransportModes.Air)
					{
						list.AddPairIfNotExist(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					}

					list.AddPairIfNotExist(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					list.RemoveCode(Core.Constants.ContainerModes.BuyersConsol);
					list.RemoveCode(Core.Constants.ContainerModes.ShippersConsol);
					list.RemoveCode(Core.Constants.ContainerModes.AgentConsol);
					return list;
				});
			}
		}
	}
}
