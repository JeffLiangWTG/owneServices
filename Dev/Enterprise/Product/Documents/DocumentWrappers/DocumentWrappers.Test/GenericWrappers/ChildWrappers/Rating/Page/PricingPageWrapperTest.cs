using System;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageWrapper))]
	sealed class PricingPageWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL", "ALL", "AUBNE", "NLAMS");

			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			PricingPageWrapper wrapper = new PricingPageWrapper(page, 1, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapper.IndexNum", 1, wrapper.IndexNum);
				AssertEquals("wrapper.Index", "00001", wrapper.Index);
				AssertEquals("wrapper.OpeningText", "", wrapper.OpeningText);
				AssertEquals("wrapper.ClosingText", "", wrapper.ClosingText);
				AssertEquals("wrapper.Entries.Count", 1, wrapper.Entries.Count);
				AssertEquals("wrapper.CFX.Count", 0, wrapper.CFX.Count);
				AssertEquals("wrapper.OriginRates.Count", 0, wrapper.OriginRates.Count);
				AssertEquals("wrapper.DestinationRates.Count", 0, wrapper.DestinationRates.Count);
				AssertEquals("wrapper.FreightRates.Count", 0, wrapper.FreightRates.Count);
				AssertEquals("wrapper.ContainerTableRows.Count", 0, wrapper.ContainerTableRows.Count);
			});
		}

		public void TestOpeningClosingText()
		{
			DocumentsDataRegistry.Instance.QuoteOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			DocumentsDataRegistry.Instance.QuoteClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry entry1 = tariff.AddRateEntry("FCL", "ALL", "AU", "NL", "", "20GP");
			entry1.TI_PageOpeningText = "";
			entry1.TI_PageClosingText = "";

			RateEntry entry2 = tariff.AddRateEntry("FCL", "ALL", "AU", "US", "", "20RE");
			entry2.TI_PageOpeningText = "";
			entry2.TI_PageClosingText = "";

			RateEntry entry3 = tariff.AddRateEntry("FCL", "ALL", "AU", "NL", "", "40GP");
			entry3.TI_PageOpeningText = "";
			entry3.TI_PageClosingText = "";

			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);

			PricingPageWrapper wrapper;

			wrapper = new PricingPageWrapper(page, 1, Factory);
			AssertEquals("", wrapper.OpeningText);
			AssertEquals("", wrapper.ClosingText);

			DocumentsDataRegistry.Instance.QuoteOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "registry opening text");
			DocumentsDataRegistry.Instance.QuoteClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "registry closing text");

			wrapper = new PricingPageWrapper(page, 1, Factory);
			AssertEquals("registry opening text", wrapper.OpeningText);
			AssertEquals("registry closing text", wrapper.ClosingText);

			entry1.TI_PageOpeningText = "entry opening text";
			entry1.TI_PageClosingText = "entry closing text";

			wrapper = new PricingPageWrapper(page, 1, Factory);
			AssertEquals("entry opening text", wrapper.OpeningText);
			AssertEquals("entry closing text", wrapper.ClosingText);

			entry2.TI_PageOpeningText = "entry opening text";
			entry2.TI_PageClosingText = "entry closing text";

			wrapper = new PricingPageWrapper(page, 1, Factory);
			AssertEquals("entry opening text", wrapper.OpeningText);
			AssertEquals("entry closing text", wrapper.ClosingText);

			entry2.TI_PageOpeningText = "different entry opening text";
			entry2.TI_PageClosingText = "different entry closing text";

			wrapper = new PricingPageWrapper(page, 1, Factory);
			AssertEquals("entry opening text\r\n\r\ndifferent entry opening text", wrapper.OpeningText);
			AssertEquals("entry closing text\r\n\r\ndifferent entry closing text", wrapper.ClosingText);
		}

		public void TestClosingTaxText()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry1 = tariff.AddRateEntry("FCL", "ALL", "AU", "NL", "", "20GP");
			var page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);

			var wrapper = new PricingPageWrapper(page, 1, Factory);
			wrapper.IsGSTApplicable = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals("A local Value Added Tax charge (equivalent to VAT) may apply to all items marked with an asterisk (*).", wrapper.ClosingTaxText);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("A local Value Added Tax charge (equivalent to GST) may apply to all items marked with an asterisk (*).", wrapper.ClosingTaxText);
			}
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			PricingPage page = new PricingPage(tariff.AddRateEntry("LCL", "ALL", "AUBNE", "NLAMS"), Factory, PricingPageStyle.Standard);

			return new PricingPageWrapper(page, 1, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Pricing Page
======================================================================
Name                                    Type
----------------------------------------------------------------------
ClosingTaxText                          String
ClosingText                             String
Index                                   String
IndexNum                                Int
IsGSTApplicable                         Bool
OpeningText                             String
RollUpDisplay                           String
RollUpStyle                             String

CFX                                     Pricing Page CFX Collection
DestinationRates                        Pricing Page Line Collection
FreightRates                            Pricing Page Line Collection
OriginRates                             Pricing Page Line Collection
RollUpSortRates                         Pricing Page Line Collection
ChargeableTableRows                     Pricing Page Table Row Collection
CompactDestinationTableRows             Pricing Page Table Row Collection
CompactFreightTableRows                 Pricing Page Table Row Collection
CompactOriginTableRows                  Pricing Page Table Row Collection
ContainerTableRows                      Pricing Page Table Row Collection
Entries                                 Rate Entry Collection
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			PricingPage page = new PricingPage(tariff.AddRateEntry("LCL", "ALL", "AUBNE", "NLAMS"), Factory, PricingPageStyle.Standard);

			return new PricingPageWrapper(page, 1, Factory);
		}

		#endregion
	}
}
