using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public abstract class SeaCargoPlugIn : CustomsManifestPlugIn
	{
		public SeaCargoPlugIn(IManifestProvider manifestProvider)
			: base(manifestProvider)
		{
		}

		protected abstract CusSCAHouse CreateNewSeaCargoJobForShipment(CommonShipment shipment);

		protected abstract void CreateSeaCargoJobIfRequired();

		protected ZString CoveringLabelText
		{
			get { return fCoveringLabelText; }
			set { fCoveringLabelText = value; }
		}
		ZString fCoveringLabelText;

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return BusinessEntity != null;
		}

		internal bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			CreateSeaCargoJobIfRequired();
			return BusinessEntity != null;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabelText; }
		}
	}
}
