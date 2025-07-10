
namespace Enterprise.Customs.CA.Business
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Common;
	using Enterprise.Freight.Forwarding.Business;

	public class CusSCAOceanBillSynchroniser : BusinessObjectSynchroniser
	{
		public CusSCAOceanBillSynchroniser(CusSCAOceanBill destination, ForwardingConsol source)
			: base(destination, source)
		{
		}

		public new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		public new CusSCAOceanBill Destination
		{
			get { return (CusSCAOceanBill)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				SetCCN();
				Synchronisers.Add(new FieldSynchroniser(Destination.CB_ApplicationCodeInfo, GetApplicationCode,
								delegate
								{ return new List<ZPropertyInfo>() { Source.JK_TransportModeInfo }; }));
				Synchronisers.Add(new FieldSynchroniser(Destination.CB_OceanBillInfo, Source.JK_MasterBillNumInfo));
				Synchronisers.Add(new CusSCAContainerCollectionSynchroniser(Source, Destination));
			}
		}

		IZType GetApplicationCode()
		{
			string result = string.Empty;
			if (Source.IsAir)
			{
				result = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
			}
			else if (Source.IsRail)
			{
				result = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail;
			}
			else if (Source.IsRoad)
			{
				result = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			}
			else
			{
				result = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
			}
			return new ZString(result);
		}

		void SetCCN()
		{
			if (Destination.OriginalCCN.IsEmpty)
			{
				var ccn = ZString.Empty;
				foreach (CusEntryNumber cusEntryNumber in Source.Numbers)
				{
					if (cusEntryNumber.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.PCN)
					{
						ccn = cusEntryNumber.CE_EntryNum;
						break;
					}
					if (cusEntryNumber.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN)
					{
						ccn = cusEntryNumber.CE_EntryNum;
					}
				}

				if (!ccn.IsEmpty)
				{
					ccn = ccn.ToUpper().Replace(" ", "");
					if (Source.IsAir && ccn.SubstringSafe(3, 1) != "-")
					{
						ccn = Source.JK_MasterBillNum.SubstringSafe(0, 3) + "-" + ccn;
					}
					Destination.OriginalCCN = ccn.Left(Destination.OriginalCCNInfo.MaxLength);
				}
			}
		}
	}
}
