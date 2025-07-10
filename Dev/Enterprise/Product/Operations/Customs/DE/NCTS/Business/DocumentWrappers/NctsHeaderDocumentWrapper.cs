using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.DocumentWrappers
{
	public class NctsHeaderDocumentWrapper : SecurityNctsHeaderDocumentWrapper
	{
		protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public static SecurityNctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));
			SecurityNctsHeaderDocumentWrapper nctsHeaderDocumentWrapper;

			if (nctsHeader.IsSecurityDeclaration)
			{
				nctsHeaderDocumentWrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, factoryToWrap);
			}
			else
			{
				nctsHeaderDocumentWrapper = new NctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
			}
			return nctsHeaderDocumentWrapper;
		}

		public ZString CUSTOMERREFERENCE => MovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty;

		public ZString CONSIGNOREORI
		{
			get
			{
				var eori = BOX2CONSIGNOREORI;
				return !eori.IsEmpty ? eori : GetEoriFallback(NctsHeader.Consignor);
			}
		}

		public ZString CONSIGNORBRANCH => GetBranchFromAddress(NctsHeader.Consignor);

		public ZString COUNTRYOFDISPATCH => MovementHeader?.BM_RN_NKCountryOfDispatch ?? ZString.Empty;

		public ZString CONSIGNEEEORI
		{
			get
			{
				var eori = BOX8CONSIGNEEEORI;
				return !eori.IsEmpty ? eori : GetEoriFallback(NctsHeader.Consignee);
			}
		}

		public ZString CONSIGNEEBRANCH => GetBranchFromAddress(NctsHeader.Consignee);

		public ZString TRANSPORTATDEPARTURE => MovementHeader?.BM_TransportAtDeparture ?? ZString.Empty;

		public ZString TOTALSEALCOUNT => NctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Sum(x => x.TotalSealCount).ToString();

		public ZString PRINCIPAL => Principal?.Name ?? ZString.Empty;

		public ZString OFFICEOFDESTINATIONCODE => GetOfficeCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

		public ZString OFFICEOFDESTINATION => GetOfficeDescription(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

		public ZString GOODSITEMSCOUNT
		{
			get
			{
				var goodsItemsCount = NctsHeader.Bills.Cast<NctsBill>().Sum(x => x.GoodsItems.Count);
				return goodsItemsCount.ToString();
			}
		}

		public ZString PACKAGESCOUNT
		{
			get
			{
				ZLong packagesCount = 0;
				foreach (var bill in NctsHeader.Bills)
				{
					foreach (var goodsItem in bill.GoodsItems)
					{
						packagesCount = packagesCount + goodsItem.Packages.Cast<NctsPackage>().Sum(x => x.B5_UnitCount);
					}
				}
				return packagesCount.ToString();
			}
		}

		protected override ZString GetNotReleasedWatermark() => NctsHeader.FallBackIsActive ? ZString.Empty : base.GetNotReleasedWatermark();

		public NctsContainerWrapperCollection Containers => containers ??= new NctsContainerWrapperCollection(NctsHeader, Factory);
		NctsContainerWrapperCollection containers;

		protected override ZString GetGuarantee() => Guarantees.ToStringCertainNumberGuarantees(9);

		protected new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

		ZString GetBranchFromAddress(JobDocAddress address) => address?.Organisation?.CustomsCodes?.GetCustomsRegNoPremiseAddressOnly(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, address.E2_RN_NKCountryCode, address.Address.PK) ?? ZString.Empty;

		ZString GetEoriFallback(JobDocAddress address) => address?.Organisation?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU) ?? ZString.Empty;
	}
}
