using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsDepartureCargoDesc))]
	public class NctsDepartureCargoDescTest : EU.NCTS.Business.Testing.NctsDepartureCargoDescAbstractTest<NctsHeader>
	{
		protected override ZString CountryCode => Core.Constants.CountryCodes.France;

		public void TestContainersPivots()
		{
			AssertType<NonPersistentDepartureContainerPivotCollection>(goodsItem.ContainersPivots);
		}

		public void TestHarbourTaxAmountInLocalCurrency()
		{
			var fee1 = goodsItem.Fees.AddNew();
			fee1.BFE_ChargeType = HarbourFeeCodes.Codes.V905;
			fee1.BFE_ChargeAmount = 11.11m;
			var fee2 = goodsItem.Fees.AddNew();
			fee2.BFE_ChargeType = HarbourFeeCodes.Codes.P635;
			fee2.BFE_ChargeAmount = 12m;
			var fee3 = goodsItem.Fees.AddNew();
			fee3.BFE_ChargeType = "V11";
			fee3.BFE_ChargeAmount = 10m;
			var fee4 = goodsItem.Fees.AddNew();
			fee4.BFE_ChargeType = "VAT";
			fee4.BFE_ChargeAmount = 13m;
			AssertEquals("Only harbour fees will be included in the calculation.", 23m, goodsItem.HarbourTaxAmountInLocalCurrency);
		}

		public void TestFees()
		{
			AssertType<CusInBondFeeCollection<NctsCargoDescFee>>(goodsItem.Fees);
		}

		public void TestValidation()
		{
			AssertType<NctsDepartureCargoDescPhase4Validation>(goodsItem.Validation);
		}

		public void TestValidation_NCTS5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsDepartureCargoDescPhase5Validation>(goodsItem.Validation);
		}

		public void TestPackages()
		{
			AssertType<EU.NCTS.Business.NctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>>(goodsItem.Packages);
		}

		public void TestGetEntryNumberFormatter()
		{
			AssertType<EntryNumberFormatterForNctsAndDeclarationIntegration>(goodsItem.GetEntryNumberFormatter());
		}

		public void TestConsignor_BothEmpty()
		{
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(goodsItem.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");

			nctsHeader.Consignor.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(nctsHeader.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");

			goodsItem.Consignor.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(goodsItem.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");

			goodsItem.Consignor.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(goodsItem.Consignor.OrganisationPKInfo, "Please specify the Consignor, either in Departure Declaration or Goods Items tab.");
		}

		public void TestConsignor_BothFilled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(nctsHeader.Consignor.OrganisationPKInfo, "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs.");

			goodsItem.Consignor.OrganisationPK = orgHeader.PK;
			AssertHasWarningContaining(goodsItem.Consignor.OrganisationPKInfo, "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs.");

			nctsHeader.Consignor.OrganisationPK = ZGuid.Empty;
			AssertNoWarningContaining(nctsHeader.Consignor.OrganisationPKInfo, "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs.");
		}

		public void TestConsignee_BothEmpty()
		{
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");

			nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");

			goodsItem.Consignee.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(goodsItem.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");

			goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
			AssertHasWarningContaining(goodsItem.Consignee.OrganisationPKInfo, "Please specify the Consignee, either in Departure Declaration or Goods Items tab.");
		}

		public void TestConsignee_BothFilled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
			AssertNoWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs.");

			goodsItem.Consignee.OrganisationPK = orgHeader.PK;
			AssertHasWarningContaining(goodsItem.Consignee.OrganisationPKInfo, "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs.");

			nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
			AssertNoWarningContaining(nctsHeader.Consignee.OrganisationPKInfo, "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs.");
		}

		public void TestBY_DescriptionMaxLength()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();

			goodsItem.BY_Description = GenerateDescription(70);
			AssertEquals("BY_Description length should be 512 char - value initial is less than 512", 70, goodsItem.BY_Description.Length);

			goodsItem.BY_Description = GenerateDescription(700);
			AssertEquals("BY_Description length should be cut to 512 char - value initial is greater than 512.", 512, goodsItem.BY_Description.Length);

			ZString GenerateDescription(int length)
			{
				string result = "";
				for (int i = 0; i < length; i++)
				{
					result += "a";
				}

				return result;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return header.MovementHeader.GoodsItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			Factory.Save();
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
	}
}
