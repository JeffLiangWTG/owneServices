namespace Enterprise.Customs.CA.Business
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Freight.Forwarding.Business;

	public class CusSCAContainerSynchroniser : BusinessObjectSynchroniser
	{
		public CusSCAContainerSynchroniser(CusSCAContainer destination, ForwardingContainer source)
			: base(destination, source)
		{
		}

		public new ForwardingContainer Source
		{
			get { return (ForwardingContainer)base.Source; }
		}

		public new CusSCAContainer Destination
		{
			get { return (CusSCAContainer)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Destination.HookedContainer = Source;
			Synchronisers.Add(new FieldSynchroniser(Destination.CN_ContainerNumberInfo, Source.JC_ContainerNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.CN_RC_NKContainerTypeInfo,
				delegate
				{ return Source.Container != null ? Source.Container.RC_Code : ZString.Empty; },
				delegate
				{ return new List<ZPropertyInfo>() { Source.JC_RCInfo }; }));
			Synchronisers.Add(new FieldSynchroniser(Destination.CN_ContainerModeInfo,
				delegate
				{ return GetContainerMode(Source); },
				delegate
				{ return new List<ZPropertyInfo>() { Source.JC_IsEmptyContainerInfo }; }));
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Destination.HookedContainer = null;
		}

		internal static ZString GetContainerMode(ForwardingContainer source)
		{
			return source.JC_IsEmptyContainer ? Core.Constants.ContainerModes.Empty : Core.Constants.ContainerModes.Containerised;
		}
	}
}
