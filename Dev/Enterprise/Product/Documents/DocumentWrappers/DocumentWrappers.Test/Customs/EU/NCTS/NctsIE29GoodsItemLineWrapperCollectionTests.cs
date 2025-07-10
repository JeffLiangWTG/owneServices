using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	[TestedType(typeof(NctsIE29GoodsItemLineWrapperCollection))]
	sealed class NctsIE29GoodsItemLineWrapperCollectionTests : NonPersistentBusinessObjectCollectionTestCase<NctsIE29GoodsItemLineWrapperCollection>
	{
		protected override NctsIE29GoodsItemLineWrapperCollection GetCollectionToTest()
		{
			return (NctsIE29GoodsItemLineWrapperCollection)NctsIE29DocumentWrapperTests.NctsIE29DocumentWrapperWithoutSecurityForTest(Factory).Lines;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(NctsIE29GoodsItemLineWrapperCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			NctsGoodsItemResponseData line = new NctsGoodsItemResponseData();
			var ediMessage = Factory.New<NctsEdiMessage>();
			return NctsIE29GoodsItemLineWrapper.New(line, ediMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
		}
		ZString oldCountryCode;

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(oldCountryCode);
		}
	}
}
