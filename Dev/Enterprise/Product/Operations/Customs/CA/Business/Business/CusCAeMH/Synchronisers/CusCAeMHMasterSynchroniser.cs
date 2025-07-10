using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterSynchroniser : BusinessObjectSynchroniser
	{
		public CusCAeMHMasterSynchroniser(CusCAeMHMaster destination, ForwardingConsol source)
			: base(destination, source)
		{ }

		public new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		public new CusCAeMHMaster Destination
		{
			get { return (CusCAeMHMaster)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BP_ModeOfTransportInfo, Source.JK_TransportModeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BP_MasterBillInfo, Source.JK_MasterBillNumInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BP_RL_NKDiscPortInfo, Calculator.GetDischargePort, Calculator.GetInfosAffectingFirstCountryPortOfDischarge));
				Synchronisers.Add(new FieldSynchroniser(Destination.BP_ETAInfo, () => Calculator.FirstCountryETADate, Calculator.GetInfosAffectingFirstCountryDischargeDate));
				Synchronisers.Add(new FieldSynchroniser(Destination.BP_ATAInfo, () => Calculator.FirstCountryATADate, Calculator.GetInfosAffectingFirstCountryDischargeDate));
				primaryCCNSynchronsier = new FieldSynchroniser(Destination.BP_PrimaryCCNInfo, Calculator.GetPrimaryCCN, GetAdditionNumberInfo);
				Synchronisers.Add(primaryCCNSynchronsier);
				subMasterCCNSynchroniser = new FieldSynchroniser(Destination.BP_MasterHouseCCNInfo, Calculator.GetSubMasterCCN, GetAdditionNumberInfo);
				Synchronisers.Add(subMasterCCNSynchroniser);
				Synchronisers.Add(new FieldSynchroniser(Destination.BP_CBSADischargePortInfo, Calculator.GetCBSADischargePort, GetInfosAffectingTransportsOrder));
				Synchronisers.Add(new FieldSynchroniser(Destination.BP_CBSADischargeSubLocationInfo, Calculator.GetSubLocation, GetInfosAffectingTransportsOrder));
				Synchronisers.Add(new CusCAeMHContainerCollectionSynchroniser(Source, Destination));
				Source.Numbers.CountChanged -= NumberCount_Changed;
				Source.Numbers.CountChanged += NumberCount_Changed;
			}
		}
		FieldSynchroniser primaryCCNSynchronsier;
		FieldSynchroniser subMasterCCNSynchroniser;

		public IEnumerable<ZPropertyInfo> GetInfosAffectingTransportsOrder()
		{
			yield return Source.JK_OA_ArrivalCTOAddressInfo;
			yield return Source.JK_OA_UnpackDepotAddressInfo;
			foreach (var info in Calculator.GetInfosAffectingFirstCountryPortOfDischarge())
			{
				yield return info;
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			Source.Numbers.CountChanged -= NumberCount_Changed;
			UnHookConsolSynchronisers();
		}

		void UnHookConsolSynchronisers()
		{
			foreach (var synchroniser in Synchronisers)
			{
				synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
			}
			Synchronisers.Clear();
		}

		void NumberCount_Changed(object sender, CollectionCountChangedEventArgs e)
		{
			if (primaryCCNSynchronsier != null)
			{
				primaryCCNSynchronsier.UpdateInfoEventsAndReSynchronise();
			}
			if (subMasterCCNSynchroniser != null)
			{
				subMasterCCNSynchroniser.UpdateInfoEventsAndReSynchronise();
			}
		}

		IEnumerable<ZPropertyInfo> GetAdditionNumberInfo()
		{
			foreach (CusEntryNumber number in Source.Numbers)
			{
				yield return number.CE_EntryNumInfo;
				yield return number.CE_EntryTypeInfo;
				yield return number.CE_RN_NKCountryCodeInfo;
			}
		}

		ConsolDataCalculator Calculator
		{
			get { return fCalculator ?? (fCalculator = new ConsolDataCalculator(Source, Destination)); }
		}
		ConsolDataCalculator fCalculator;
	}
}
