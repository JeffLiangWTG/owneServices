using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CuscarWrapperFromSplitHouse : CuscarWrapperFromCusHawb
	{
		public CuscarWrapperFromSplitHouse(SplitHouse splitToWrap) : base(splitToWrap.HAWB)
		{
			this.splitToWrap = splitToWrap;
		}

		protected override ZString SplitReferenceCore
		{
			get { return splitToWrap.SplitReference; }
		}

		protected override ZShort NumberOfPiecesReceivedCore
		{
			get { return splitToWrap.NumberOfPiecesReceived; }
		}

		protected override ZShort NumberOfPiecesExpectedCore
		{
			get { return splitToWrap.NumberOfPiecesExpected; }
		}

		protected override ZDecimal WeightCore
		{
			get { return splitToWrap.Weight; }
		}

		protected override ZString AgentBrokerConsolidatorCodeCore
		{
			get { return splitToWrap.AgentBadge; }
		}

		protected override List<ZString> CommunityHandlingCodesCore
		{
			get
			{
				var list = new List<ZString>();
				list.AddRange(from CusAddInfo<CommunityHandlingCode> chc
									in splitToWrap.AWB.CommunityHandlingCodes
							  where chc.Data.C4_SplitReferenceToWhichThisPertains == splitToWrap.SplitReference
							  select chc.Data.C4_CommunityHandlingCode
								);
				return list;
			}
		}

		protected override ZDateTime Status1DateCore
		{
			get { return splitToWrap.Status1Date; }
		}

		readonly SplitHouse splitToWrap;
	}

	class CuscarWrapperFromSplitBasic : CuscarWrapperFromCusMawb
	{
		public CuscarWrapperFromSplitBasic(SplitBasic splitToWrap)
			: base(splitToWrap.Basic)
		{
			this.splitToWrap = splitToWrap;
		}

		protected override ZString SplitReferenceCore
		{
			get { return splitToWrap.SplitReference; }
		}

		protected override ZShort NumberOfPiecesReceivedCore
		{
			get { return splitToWrap.NumberOfPiecesReceived; }
		}

		protected override ZShort NumberOfPiecesExpectedCore
		{
			get { return splitToWrap.NumberOfPiecesExpected; }
		}

		protected override ZDecimal WeightCore
		{
			get { return splitToWrap.Weight; }
		}

		protected override ZString AgentBrokerConsolidatorCodeCore
		{
			get { return splitToWrap.AgentBadge; }
		}

		protected override List<ZString> CommunityHandlingCodesCore
		{
			get
			{
				var list = new List<ZString>();
				list.AddRange(from CusAddInfo<CommunityHandlingCode> chc
								  in splitToWrap.AWB.CommunityHandlingCodes
							  where chc.Data.C4_SplitReferenceToWhichThisPertains == splitToWrap.SplitReference
							  select chc.Data.C4_CommunityHandlingCode
								);
				return list;
			}
		}

		protected override ZDateTime Status1DateCore
		{
			get { return splitToWrap.Status1Date; }
		}

		readonly SplitBasic splitToWrap;
	}
}
