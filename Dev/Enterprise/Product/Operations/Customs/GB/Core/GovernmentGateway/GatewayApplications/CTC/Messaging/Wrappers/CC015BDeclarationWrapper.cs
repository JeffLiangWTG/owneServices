using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public class CC015BDeclarationWrapper : EU.NCTS.Business.CC015BDeclarationWrapper, ICC015BDeclaration
	{
		public CC015BDeclarationWrapper(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public ITrader Carrier => CachedValueHelper.GetValue(ref carrier, () => EU.NCTS.Business.TraderWrapper.New(nctsHeader.Carrier?.MainAddress, false, IsAddressExtended));
		CachedValue<ITrader> carrier;

		public ZBool IsSimplifiedNctsProcedure => nctsHeader.MovementHeader.IsSimplifiedNctsProcedure;

		public ZBool IsConsignorDefinedAtGoodsItemLevel => NctsHeaderHelper.IsConsignorDefinedAtGoodsItemLevel(nctsHeader);

		public ZBool IsConsigneeDefinedAtGoodsItemLevel => NctsHeaderHelper.IsConsigneeDefinedAtGoodsItemLevel(nctsHeader);

		public ZBool HasSecurityAtGoodsItemLevel => NctsHeaderHelper.HasSecurityAtGoodsItemLevel(nctsHeader);

		IReadOnlyCollection<ISealID> EU.NCTS.Messaging.ICC015BDeclaration.Seals
		{
			get
			{
				if (seals == null)
				{
					if (nctsHeader.MovementHeader.BM_SealType.IsEmpty)
					{
						seals = SealsSelected.Select(x => new EU.NCTS.Business.SealWrapper(x)).Cast<ISealID>().ToList();
					}
					else
					{
						seals = EU.NCTS.Business.WrapperHelper.GetSealIDs(nctsHeader, SealsSelected);
					}
				}
				return seals;
			}
		}
		IReadOnlyCollection<ISealID> seals;
	}
}
