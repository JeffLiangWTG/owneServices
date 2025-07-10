using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ContainerSynchroniser : BusinessObjectSynchroniser
	{
		public ContainerSynchroniser(CusSCAContainer destination, CommonContainer source, CommonConsol parentConsol)
			: base(destination, source)
		{
			fParentConsolBizO = parentConsol;
		}

		public new CusSCAContainer Destination
		{
			get { return (CusSCAContainer)base.Destination; }
		}

		public new CommonContainer Source
		{
			get { return (CommonContainer)base.Source; }
		}

		CommonConsol fParentConsolBizO;
		public CommonConsol Consol
		{
			get
			{
				return fParentConsolBizO;
			}
			set
			{
				fParentConsolBizO = value;
			}
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.CN_ContainerNumberInfo, Source.JC_ContainerNumInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CN_SealNumberInfo, Source.JC_SealNumInfo, true));
			FieldSynchroniser containerTypeSynchroniser = new FieldSynchroniser(Destination.CN_RC_NKContainerTypeInfo, Source.JC_RCInfo, true);
			containerTypeSynchroniser.Format += ContainerTypeSynchroniser_Format;
			Synchronisers.Add(containerTypeSynchroniser);
			FieldSynchroniser containerModeSynchroniser = new FieldSynchroniser(Destination.CN_ContainerModeInfo, Source.JC_ContainerModeInfo, true);
			containerModeSynchroniser.Format += ContainerModeSynchroniser_Format;
			Synchronisers.Add(containerModeSynchroniser);
			if (Consol != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.CN_OA_UnderbondFromInfo, Consol.JK_OA_ArrivalCTOAddressInfo, true));
				Synchronisers.Add(new FieldSynchroniser(Destination.CN_OA_UnderbondToInfo, Consol.JK_OA_UnpackDepotAddressInfo, true));
			}
		}

		#endregion

		void ContainerTypeSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZGuid)
			{
				ZGuid refContainerPK = (ZGuid)e.Value;
				RefContainer containerType = Source.Factory.Load<RefContainer>(refContainerPK);
				e.Value = containerType != null ? containerType.RC_Code : ZString.Empty;
			}
		}

		protected virtual void ContainerModeSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZString)
			{
				ZString containerMode = new ZString(e.Value);
				if (Destination.OceanBill != null && Destination.OceanBill.Consol != null)
				{
					ZString consolContainerMode = Destination.OceanBill.Consol.JK_ConsolMode;
					if (consolContainerMode == Enterprise.Core.Constants.ContainerModes.Groupage)
					{
						if (containerMode == Enterprise.Core.Constants.ContainerModes.FreightAllKind
							|| containerMode == Enterprise.Core.Constants.ContainerModes.FCL
							|| containerMode == Enterprise.Core.Constants.ContainerModes.Groupage)
						{
							e.Value = new ZString(Enterprise.Core.Constants.ContainerModes.FreightAllKind);
						}
					}
					else if (consolContainerMode == Enterprise.Core.Constants.ContainerModes.BuyersConsol)
					{
						if (containerMode == Enterprise.Core.Constants.ContainerModes.FreightAllKind
							|| containerMode == Enterprise.Core.Constants.ContainerModes.FCL
							|| containerMode == Enterprise.Core.Constants.ContainerModes.BuyersConsol)
						{
							e.Value = new ZString(Enterprise.Core.Constants.ContainerModes.FCLMixedShipper);
						}
					}
				}
			}
		}
	}
}
