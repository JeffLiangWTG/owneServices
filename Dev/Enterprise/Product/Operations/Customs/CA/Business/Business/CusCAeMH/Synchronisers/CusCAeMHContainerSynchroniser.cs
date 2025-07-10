using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHContainerSynchroniser : BusinessObjectSynchroniser
	{
		public CusCAeMHContainerSynchroniser(CusCAeMHContainer destination, ForwardingContainer source)
			: base(destination, source)
		{ }

		protected new CusCAeMHContainer Destination
		{
			get { return (CusCAeMHContainer)base.Destination; }
		}

		protected new ForwardingContainer Source
		{
			get { return (ForwardingContainer)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.BQ_ContainerNumberInfo, Source.JC_ContainerNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BQ_RC_NKContainerTypeInfo,
				() => { return Source.Container != null ? Source.Container.RC_Code : ZString.Empty; },
				() => { return new[] { Source.JC_RCInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.BQ_Seal1Info, Source.JC_SealNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BQ_Seal2Info, Source.JC_AdditionalSealNumInfo));
		}
	}
}
