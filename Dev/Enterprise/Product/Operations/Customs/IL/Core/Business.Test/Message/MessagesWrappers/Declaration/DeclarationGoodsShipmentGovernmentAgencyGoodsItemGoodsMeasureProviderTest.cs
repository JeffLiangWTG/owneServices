using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("invoiceLine is must", () => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureProvider(null));
		}

		public void TestMeasureQualifierInvoiceQuantity()
		{
			invoiceLine.JI_CustomsQuantity = 2.3m;
			var provider = GetProvider();
			AssertEquals("When CustomsQuantity is initialize", 0, provider.Count);
			invoiceLine.JI_CustomsUnitQty = "05";
			provider = GetProvider();
			AssertEquals("When CustomsQuantity and CustomsUnitQty are initialize", 1, provider.Count);
			var wrapper = provider.Single();
			AssertEquals("1", wrapper.DmExtensions.MeasureQualifier.Value);
			AssertEquals(2.3m, wrapper.TariffQuantity.Value);
			AssertEquals(MeasurementUnitCommonCodeContentType.Item05, wrapper.TariffQuantity.UnitCode);
		}

		public void TestMeasureQualifierStatisticQty()
		{
			invoiceLine.JI_CustomsSecondQuantity = 3.3m;
			var provider = GetProvider();
			AssertEquals("When CustomsSecondQuantity is initialize", 0, provider.Count);
			invoiceLine.JI_CustomsSecondUnitQty = "06";
			provider = GetProvider();
			AssertEquals("When CustomsSecondQuantity and CustomsSecondUnitQty are initialize", 1, provider.Count);
			var wrapper = provider.Single();
			AssertEquals("2", wrapper.DmExtensions.MeasureQualifier.Value);
			AssertEquals(3.3m, wrapper.TariffQuantity.Value);
			AssertEquals(MeasurementUnitCommonCodeContentType.Item06, wrapper.TariffQuantity.UnitCode);
		}

		public void TestMeasureQualifierAdditionalQty()
		{
			invoiceLine.JI_CustomsThirdQuantity = 4.3m;
			var provider = GetProvider();
			AssertEquals("When CustomsThirdQuantity is initialize", 0, provider.Count);
			invoiceLine.JI_CustomsThirdUnitQty = "08";
			provider = GetProvider();
			AssertEquals("When CustomsThirdQuantity and CustomsThirdUnitQty are initialize", 1, provider.Count);
			var wrapper = provider.Single();
			AssertEquals("3", wrapper.DmExtensions.MeasureQualifier.Value);
			AssertEquals(4.3m, wrapper.TariffQuantity.Value);
			AssertEquals(MeasurementUnitCommonCodeContentType.Item08, wrapper.TariffQuantity.UnitCode);
		}

		public ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure> GetProvider() => new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureProvider(invoiceLine).GetGoodsMeasure().ToArray();

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
		}

		JobComInvoiceLine invoiceLine;
	}
}
