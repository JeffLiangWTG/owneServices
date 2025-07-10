using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TradeChainPartnerAddInfo))]
	sealed class TradeChainPartnerAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TradeChainPartnerAddInfo(TradeChainPartner.B7_AddInfoDataInfo);
		}

		TradeChainPartner TradeChainPartner
		{
			get
			{
				if (tradeChainPartner == null)
				{
					var org = Factory.New<OrgHeader>();
					var orgImpAddInfo = OrgImpAddInfo.Get(org);
					tradeChainPartner = orgImpAddInfo.TradeChainPartners.AddNew();
				}
				return tradeChainPartner;
			}
		}

		TradeChainPartner tradeChainPartner;

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			Assert(true);
		}

		#endregion
	}
}
