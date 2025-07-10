using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDesc))]
	class NctsDepartureCargoDescTest : EU.NCTS.Business.Testing.NctsDepartureCargoDescAbstractTest<NctsHeader>
	{
		public void TestSupportingDocuments()
		{
			AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(GetNewBusinessObject(Factory).SupportingDocuments);
		}

		public void TestGetEntryNumberFormatter()
		{
			AssertType<EntryNumberFormatterForNctsAndDeclarationIntegration>(GetNewBusinessObject(Factory).GetEntryNumberFormatter());
		}

		NctsDepartureCargoDesc GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			return goodsItem;
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
