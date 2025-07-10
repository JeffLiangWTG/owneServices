using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class ClientRateRunDocsTest : BaseRunDocumentsTest
	{
		public ClientRateRunDocsTest() { }

		public void TestTemplate_PricingPage_DocBuilder()
		{
			RunDocumentWithAllSections = ZBool.True;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					(IDocumentSupportable)GetBusinessObject,
					@"{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Charge.Description>]   {AA}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>]   {AE}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {AQ}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {AW}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {BI}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {BO}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]

{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]




{AA}-[Cur.]   {AE}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1].Heading>]   {AK}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2].Heading>]   {AQ}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3].Heading>]   {AW}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4].Heading>]   {BC}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5].Heading>]   {BI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6].Heading>]   {BO}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7].Heading>]


{C}-[<AddTitleIfNotEmpty("" From "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Origin>"", """")><AddTitleIfNotEmpty("" To "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Destination>"", """")><AddTitleIfNotEmpty("" Via "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Via>"", """")>]   {AZ}-[Validity: <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ValidFrom>', 'dd-MMM-yy')>  -  <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ValidUntil>', 'dd-MMM-yy')>]

{C}-[Consignor]   {AK}-[Consignee]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AK}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignee.CompanyName>"")>]

{C}-[Airline]   {Q}-[Frequency]   {AA}-[Transit Time]   {AK}-[Service Level]   {AW}-[Commodity Code]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Provider.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Provider.CompanyName>"")>]   {Q}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Frequency>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Frequency>"")>]   {AA}-[<AutoHeight><if(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].TransitTime>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].TransitTime>"")>]   {AK}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ServiceLevel.Code>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ServiceLevel.Code>"")>]   {AW}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].CommodityCode.Code>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].CommodityCode.Code>"")>]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1].Heading>]   {AK}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2].Heading>]   {AQ}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3].Heading>]   {AW}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4].Heading>]   {BC}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5].Heading>]   {BI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6].Heading>]   {BO}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7].Heading>]




{C}-[<ShrinkToFit><Rating.PrimarySource>]


{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]






{C}-[CFX Information]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]




{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.TypeDescription>]   {O}-[Service |>Level]   {S}-[Comm. |>Code]   {V}-[Cur.]   {Y}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2].Heading>]   {AK}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3].Heading>]   {AQ}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4].Heading>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5].Heading>]   {BC}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6].Heading>]   {BI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7].Heading>]   {BO}-[W/V Conv.]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{O}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{Q}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{S}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{U}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{W}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.CompanyName>"", """")>]   {O}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].ServiceLevel.Code>]   {S}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].CommodityCode.Code>]   {V}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>]   {Y}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AE}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AQ}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AW}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {BI}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {BO}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.ConversionFactor>]
{O}-[Consignor:]   {V}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AQ}-[Consignee:]   {AX}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"")>]
{O}-[Frequency:]   {V}-[<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"")>]   {AQ}-[Transit Time:]   {AX}-[<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"")>]



{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]
{O}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]

{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]




{C}-[<AddTitleIfNotEmpty("" From "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Origin>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].PickUpAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" To "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Destination>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].DeliveryAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" Via "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Via>"", """")>]   {BA}-[<AutoHeight>Validity: <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidFrom>', 'dd-MMM-yy')>  -  <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidUntil>', 'dd-MMM-yy')>]




{C}-[<ShrinkToFit><Rating.PrimarySource>]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.TypeDescription>]   {O}-[Service|>Level]   {S}-[Comm. |>Code]   {V}-[Cur.]   {Y}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2].Heading>]   {AK}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3].Heading>]   {AQ}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4].Heading>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5].Heading>]   {BC}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6].Heading>]   {BI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7].Heading>]   {BO}-[W/V Conv.]






{C}-[CFX Information]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]


{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]
{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{O}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{Q}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{S}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{U}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{W}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.CompanyName>"", """")>]   {O}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].ServiceLevel.Code>]   {S}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].CommodityCode.Code>]   {V}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>]   {Y}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[1]>]   {AE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[2]>]   {AK}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[3]>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[4]>]   {AW}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[5]>]   {BC}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[6]>]   {BI}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[7]>]   {BO}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.ConversionFactor>]


{O}-[Consignor:]   {V}-[<AutoHeight><If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AQ}-[Consignee:]   {AX}-[<AutoHeight><If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"")>]
{O}-[Frequency:]   {V}-[<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"")>]   {AQ}-[Transit Time:]   {AX}-[<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"")>]



{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]



{O}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]

{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]
{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]




{C}-[<AddTitleIfNotEmpty("" From "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Origin>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].PickUpAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" To "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Destination>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].DeliveryAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" Via "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Via>"", """")>]   {BA}-[<AutoHeight>Validity: <DateTimeAsString('<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidFrom>', 'dd-MMM-yy')>  -  <DateTimeAsString('<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidUntil>', 'dd-MMM-yy')>]

{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.TypeDescription>]   {O}-[<AutoHeight>Serv. Level]   {S}-[<AutoHeight>Comm. Code]   {V}-[<AutoHeight>Cur.]   {Y}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading>]   {AE}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[2].Heading>]   {AK}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[3].Heading>]   {AQ}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[4].Heading>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[5].Heading>]   {BC}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[6].Heading>]   {BI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[7].Heading>]   {BO}-[<AutoHeight>W/V Conv.]
{C}-[<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Label>"" == ""OriginRates"", ""Origin"", ""???"")>"")> Charges]




{C}-[<AutoHeight><Rating.PrimarySource>]


{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-"", """")>Freight Rates]

{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.OpeningText>]



{C}-[CFX Information]
{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.ClosingText>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]



{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]
{C}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""0"")>]
{E}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""1"")>]
{G}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""2"")>]
{I}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""3"")>]
{K}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""4"")>]



{AW}-[Commodity Code: <If(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.ParentEntry.CommodityCode>"" == """", ""Not Specified"", ""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.ParentEntry.CommodityCode.Description>"")>]

{C}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.ParentEntry.Mode>]

{C}-[<If(""<Rating.PageSets[CFS].OriginAndDestinationRates.Label>"" == ""OriginRates"", ""Packing Charges"", ""<If(""<Rating.PageSets[CFS].OriginAndDestinationRates.Label>"" == ""DestinationRates"", ""Unpacking Charges"", ""???"")>"")><If(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Origin>"" == """", """", "" from <Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Origin>"")><If(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Destination>"" == """", """", "" to <Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Destination>"")>]



{C}-[<AutoHeight><Rating.PrimarySource>]
{C}-[CFS Rates]
{C}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode> DETAILS FOR <JobHeaderLocalClient.CompanyName>]

{AU}-[Page <CurrentPage> of <TotalPages>]


{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.Page.ClosingText>]
{BM}-[<HideRowIfCellIsEmpty><If(""<CurrentPage>"" == ""<TotalPages>"", """", ""Continued Over…"")>]
{C}-[<Image(.Rating.Logo, 1, 47)>]


{C}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""0"")>]
{E}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""1"")>]
{G}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""2"")>]
{I}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""3"")>]
{K}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""4"")>]



{C}-[<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Label>"" == ""OriginRates"", ""Export Detention Charges<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Origin>"" == """", """", "" from <Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Origin>"")>"", ""<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Label>"" == ""DestinationRates"", ""Import Detention Charges<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Destination>"" == """", """", "" to <Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Destination>"")>"", ""???"")>"")>]




{C}-[<AutoHeight><Rating.PrimarySource>]
{C}-[Shipping Detention Rates]
{C}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode> DETAILS FOR <JobHeaderLocalClient.CompanyName>]

{C}-[<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Page.Entries[1].Provider>"" == """", """", ""Principal:"")>]   {I}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Page.Entries[1].Provider.CompanyName>]   {AU}-[Page <CurrentPage> of <TotalPages>]


{BM}-[<HideRowIfCellIsEmpty><If(""<CurrentPage>"" == ""<TotalPages>"", """", ""Continued Over…"")>]
{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Page.ClosingText>]
",
					message: "CoverPageText and CoverPageFooterText should not be shown"
				);
			}
		}

		public void TestTemplate_PricingPageWithCoverPage_DocBuilder()
		{
			RunDocumentWithAllSections = ZBool.True;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page with Cover Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					(IDocumentSupportable)GetBusinessObject,
					@"{C}-[<ShrinkToFit><ReportName>]   {AO}-[Page <Current Page> of <TotalPages>]



{C}-[<ShrinkToFit><ReportName>]   {AO}-[Page <Current Page> of <TotalPages>]


{G}-[<ExpandToFit(Last)><RecipientNameAndAddress>]   {AE}-[DATE]   {AO}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]




{C}-[<Upper(""<Rating.QuotationTitle>"")> <JobNumber> <JobHeaderLocalClient.CompanyName>]

{C}-[<RecipientSalutation>]

{C}-[<AutoHeight><Rating.CoverPageText>]



{C}-[<SignOffText>]

{C}-[<Image(SalesRep.SignatureForQuoteDocuments, 3, 19, Y)>]   {AC}-[<Image(Rating.SecondSignatory.SignatureForQuoteDocuments, 3, 19, Y)>]


{C}-[<SalesRep.FullName>]   {AC}-[<Rating.SecondSignatory.FullName>]
{C}-[<SalesRep.Title>]   {AC}-[<Rating.SecondSignatory.Title>]

{C}-[<BrandName>]



{C}-[<AutoHeight><Rating.CoverPageFooterText>]


{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[<If(""<CurrentPage>"" != ""<TotalPages>"", ""Continued Over…"", """")>]

{C}-[END OF DOCUMENT]

{C}-[END OF DOCUMENT]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Description>]   {AI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.Units>]
{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Charge.Description>]   {AA}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>]   {AE}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {AQ}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {AW}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {BI}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]   {BO}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7]>, <Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Currency.Code>)>]

{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]




{AA}-[Cur.]   {AE}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1].Heading>]   {AK}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2].Heading>]   {AQ}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3].Heading>]   {AW}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4].Heading>]   {BC}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5].Heading>]   {BI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6].Heading>]   {BO}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7].Heading>]


{C}-[<AddTitleIfNotEmpty("" From "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Origin>"", """")><AddTitleIfNotEmpty("" To "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Destination>"", """")><AddTitleIfNotEmpty("" Via "", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Via>"", """")>]   {AZ}-[Validity: <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ValidFrom>', 'dd-MMM-yy')>  -  <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ValidUntil>', 'dd-MMM-yy')>]

{C}-[Consignor]   {AK}-[Consignee]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AK}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Consignee.CompanyName>"")>]

{C}-[Airline]   {Q}-[Frequency]   {AA}-[Transit Time]   {AK}-[Service Level]   {AW}-[Commodity Code]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Provider.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Provider.CompanyName>"")>]   {Q}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Frequency>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].Frequency>"")>]   {AA}-[<AutoHeight><if(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].TransitTime>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].TransitTime>"")>]   {AK}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ServiceLevel.Code>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].ServiceLevel.Code>"")>]   {AW}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].CommodityCode.Code>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].CommodityCode.Code>"")>]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[1].Heading>]   {AK}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[2].Heading>]   {AQ}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[3].Heading>]   {AW}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[4].Heading>]   {BC}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[5].Heading>]   {BI}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[6].Heading>]   {BO}-[<ShrinkToFit><Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.SubRow.Columns[7].Heading>]




{C}-[<ShrinkToFit><Rating.PrimarySource>]


{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]






{C}-[CFX Information]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]




{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.TypeDescription>]   {O}-[Service |>Level]   {S}-[Comm. |>Code]   {V}-[Cur.]   {Y}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2].Heading>]   {AK}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3].Heading>]   {AQ}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4].Heading>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5].Heading>]   {BC}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6].Heading>]   {BI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7].Heading>]   {BO}-[W/V Conv.]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{O}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{Q}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{S}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{U}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{W}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.Units>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.CompanyName>"", """")>]   {O}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].ServiceLevel.Code>]   {S}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].CommodityCode.Code>]   {V}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>]   {Y}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AE}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AK}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AQ}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {AW}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {BC}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {BI}-[<AutoHeight><FormatNumber(<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7]>, <Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>)>]   {BO}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.ConversionFactor>]
{O}-[Consignor:]   {V}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AQ}-[Consignee:]   {AX}-[<AutoHeight><If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"")>]
{O}-[Frequency:]   {V}-[<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"")>]   {AQ}-[Transit Time:]   {AX}-[<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"" == """", ""Not Specified"", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"")>]



{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]
{O}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]

{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]




{C}-[<AddTitleIfNotEmpty("" From "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Origin>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].PickUpAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" To "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Destination>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].DeliveryAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" Via "", ""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Via>"", """")>]   {BA}-[<AutoHeight>Validity: <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidFrom>', 'dd-MMM-yy')>  -  <DateTimeAsString('<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidUntil>', 'dd-MMM-yy')>]




{C}-[<ShrinkToFit><Rating.PrimarySource>]


{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" == ""OriginRates"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-Freight"", ""Freight"")>"")>"")> <If(""<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Label>"" ==  ""TableRows"", ""Rates"", ""Charges"")>]

{C}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.TypeDescription>]   {O}-[Service|>Level]   {S}-[Comm. |>Code]   {V}-[Cur.]   {Y}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading>]   {AE}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[2].Heading>]   {AK}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[3].Heading>]   {AQ}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[4].Heading>]   {AW}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[5].Heading>]   {BC}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[6].Heading>]   {BI}-[<AutoHeight><Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[7].Heading>]   {BO}-[W/V Conv.]






{C}-[CFX Information]
{C}-[<Rating.PageSets[ForwardingLandscapeSimpleNonLoose].OriginDestinationAndContainerTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]


{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]
{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{E}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{G}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{I}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{K}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {AM}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{O}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{Q}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{S}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{U}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{W}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Description>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>]   {BA}-[<AutoHeight><FormatNumber(<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Amount>, <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"" != """", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Currency>"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.DecimalPlaces>"")>)>]   {BE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.Units>]
{C}-[<AutoHeight><If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Index>"" == ""0"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.CompanyName>"", """")>]   {O}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].ServiceLevel.Code>]   {S}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].CommodityCode.Code>]   {V}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Currency.Code>]   {Y}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[1]>]   {AE}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[2]>]   {AK}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[3]>]   {AQ}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[4]>]   {AW}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[5]>]   {BC}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[6]>]   {BI}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[7]>]   {BO}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.ConversionFactor>]


{O}-[Consignor:]   {V}-[<AutoHeight><If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignor.CompanyName>"")>]   {AQ}-[Consignee:]   {AX}-[<AutoHeight><If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Consignee.CompanyName>"")>]
{O}-[Frequency:]   {V}-[<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Frequency>"")>]   {AQ}-[Transit Time:]   {AX}-[<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"" == """", ""Not Specified"", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].TransitTime>"")>]



{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]



{O}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Mode>]

{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin><AddTitleIfNotEmpty("" (to "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]
{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Destination><AddTitleIfNotEmpty("" (from "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Origin>"", "")"")><AddTitleIfNotEmpty("" - "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.Provider.CompanyName>"", """")>]   {AS}-[<AddTitleIfNotEmpty(""Service Level: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.ServiceLevel.Code>"", """")>]   {BG}-[<AddTitleIfNotEmpty(""Commodity Code: "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.RateLine.ParentEntry.CommodityCode.Code>"", """")>]




{C}-[<AddTitleIfNotEmpty("" From "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Origin>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].PickUpAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" To "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Destination>"", """")><AddTitleIfNotEmpty("" ("", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].DeliveryAddressPostCode>"", "")"")><AddTitleIfNotEmpty("" Via "", ""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Via>"", """")>]   {BA}-[<AutoHeight>Validity: <DateTimeAsString('<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidFrom>', 'dd-MMM-yy')>  -  <DateTimeAsString('<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].ValidUntil>', 'dd-MMM-yy')>]

{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Row.Entries[1].Provider.TypeDescription>]   {O}-[<AutoHeight>Serv. Level]   {S}-[<AutoHeight>Comm. Code]   {V}-[<AutoHeight>Cur.]   {Y}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading>]   {AE}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[2].Heading>]   {AK}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[3].Heading>]   {AQ}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[4].Heading>]   {AW}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[5].Heading>]   {BC}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[6].Heading>]   {BI}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.SubRow.Columns[7].Heading>]   {BO}-[<AutoHeight>W/V Conv.]
{C}-[<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Label>"" == ""DestinationRates"", ""Destination"", ""<If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Label>"" == ""OriginRates"", ""Origin"", ""???"")>"")> Charges]




{C}-[<AutoHeight><Rating.PrimarySource>]


{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.Entries[1].TransportMode> <If(""<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.Entries[1].IsSupplementary>"" == ""Y"", ""Non-"", """")>Freight Rates]

{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.OpeningText>]



{C}-[CFX Information]
{C}-[<Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.CFX.Format(""{NameAndValue}"", Newline)>]

{C}-[<AutoHeight><Rating.PageSets[ShippingLandscapeSimple].OriginDestinationAndContainerTableRows.Page.ClosingText>]

{C}-[A local Value Added Tax charge (equivalent to <if(""<JobHeaderLocalClient.TaxCode>"" != """", ""<JobHeaderLocalClient.TaxCode>"", ""<TaxCode>"")>) may apply to all items marked with an asterisk (*).]



{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[Continued Over… ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]

{S}-[END OF DOCUMENT ]   {BE}-[<ShrinkToFit>Page <Current Page> of <TotalPages>]
{C}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""0"")>]
{E}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""1"")>]
{G}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""2"")>]
{I}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""3"")>]
{K}-[<AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""4"")>]



{AW}-[Commodity Code: <If(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.ParentEntry.CommodityCode>"" == """", ""Not Specified"", ""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.ParentEntry.CommodityCode.Description>"")>]

{C}-[<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.ParentEntry.Mode>]

{C}-[<If(""<Rating.PageSets[CFS].OriginAndDestinationRates.Label>"" == ""OriginRates"", ""Packing Charges"", ""<If(""<Rating.PageSets[CFS].OriginAndDestinationRates.Label>"" == ""DestinationRates"", ""Unpacking Charges"", ""???"")>"")><If(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Origin>"" == """", """", "" from <Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Origin>"")><If(""<Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Destination>"" == """", """", "" to <Rating.PageSets[CFS].OriginAndDestinationRates.RateLine.Destination>"")>]



{C}-[<AutoHeight><Rating.PrimarySource>]
{C}-[CFS Rates]
{C}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode> DETAILS FOR <JobHeaderLocalClient.CompanyName>]

{AU}-[Page <CurrentPage> of <TotalPages>]


{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[CFS].OriginAndDestinationRates.Page.ClosingText>]
{BM}-[<HideRowIfCellIsEmpty><If(""<CurrentPage>"" == ""<TotalPages>"", """", ""Continued Over…"")>]
{C}-[<Image(.Rating.Logo, 1, 47)>]


{C}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""0"")>]
{E}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""1"")>]
{G}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""2"")>]
{I}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""3"")>]
{K}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Description>]   {AI}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Currency>]   {AM}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Amount>]   {AS}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Units>]   {BA}-[<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Validity>]   {BU}-[<HideRowIf(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.NumOfIndents>"" != ""4"")>]



{C}-[<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Label>"" == ""OriginRates"", ""Export Detention Charges<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Origin>"" == """", """", "" from <Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Origin>"")>"", ""<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Label>"" == ""DestinationRates"", ""Import Detention Charges<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Destination>"" == """", """", "" to <Rating.PageSets[ShippingDetention].OriginAndDestinationRates.RateLine.Destination>"")>"", ""???"")>"")>]




{C}-[<AutoHeight><Rating.PrimarySource>]
{C}-[Shipping Detention Rates]
{C}-[<JobNumber> - <JobHeaderLocalClient.CompanyCode> DETAILS FOR <JobHeaderLocalClient.CompanyName>]

{C}-[<If(""<Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Page.Entries[1].Provider>"" == """", """", ""Principal:"")>]   {I}-[<AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Page.Entries[1].Provider.CompanyName>]   {AU}-[Page <CurrentPage> of <TotalPages>]


{BM}-[<HideRowIfCellIsEmpty><If(""<CurrentPage>"" == ""<TotalPages>"", """", ""Continued Over…"")>]
{C}-[<HideRowIfCellIsEmpty><AutoHeight><Rating.PageSets[ShippingDetention].OriginAndDestinationRates.Page.ClosingText>]
",
					message: "CoverPageText and CoverPageFooterText should be shown"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_PricingPage_DocBuilder()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "CHARGE1", 100m, currency: "LYD");
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					clientRate,
					expectedOutput: @"{C}-[Test Client #2 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Los Angeles]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Airline]   {Q}-[Frequency]   {AA}-[Transit Time]   {AK}-[Service Level]   {AW}-[Commodity Code]
{C}-[Not Specified]   {Q}-[Not Specified]   {AA}-[Not Specified]   {AK}-[Not Specified]   {AW}-[GEN]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[Charge Description 1]   {AA}-[LYD]   {AE}-[100.000]





{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


",
					message: "CoverPageText and CoverPageFooterText should not be shown"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_PricingPage_DocBuilder_Rollup()
		{
			var client = TestHelper.NewOrgHeader(1);

			QuotationRunDocForwardingStandardPricingPageRollUpSortTest.SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpCharges);
			QuotationRunDocForwardingStandardPricingPageRollUpSortTest.SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Style, DocRollupOrSortStyleList.Codes.All);

			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "FRT", 100m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "FRT", 101m);
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					clientRate,
					expectedOutput: @"{C}-[Test Client #2 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Los Angeles]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Airline]   {Q}-[Frequency]   {AA}-[Transit Time]   {AK}-[Service Level]   {AW}-[Commodity Code]
{C}-[Not Specified]   {Q}-[Not Specified]   {AA}-[Not Specified]   {AK}-[Not Specified]   {AW}-[GEN]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[100.00]
{C}-[International Freight]   {AA}-[AUD]   {AE}-[101.00]





{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


",
					message: "Non Quotation should not have rollup"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_PricingPageWithCoverPage_DocBuilder()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "CHARGE1", 100m, currency: "LYD");
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page With Cover Page");

			var ratingRegistry = new RatingRegistry(RawDataRegistry.Instance);
			ratingRegistry.GRIUpdateCoverPageText = "COVER PAGE TEXT TEST";
			ratingRegistry.GRIUpdateFooterPageText = "FOOTER PAGE TEXT TEST";
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					clientRate,
					expectedOutput: @"{C}-[Rate Update Notification]   {AO}-[Page 1 of 1]

{G}-[*** NO ORGANIZATION DETAILS FOUND ***]   {AE}-[DATE]   {AO}-[01-Jan-20 00:00]



{C}-[RATE UPDATE NOTIFICATION BXCPI2VTM38SN4162V TEST CLIENT #2]



{C}-[COVER PAGE TEXT TEST]


{C}-[Yours Sincerely,]






{C}-[EAGLE DATAMATION INTERNATIONAL]


{C}-[FOOTER PAGE TEXT TEST]



{C}-[END OF DOCUMENT]
{C}-[Test Client #2 Client Rate]


{C}-[AIR - Air Freight Rates]

{C}-[ From Sydney To Los Angeles]   {AZ}-[Validity: 01-Jan-20  -  01-Jul-20]

{C}-[Airline]   {Q}-[Frequency]   {AA}-[Transit Time]   {AK}-[Service Level]   {AW}-[Commodity Code]
{C}-[Not Specified]   {Q}-[Not Specified]   {AA}-[Not Specified]   {AK}-[Not Specified]   {AW}-[GEN]

{C}-[Charge Description]   {AA}-[Cur.]   {AE}-[Flat]
{C}-[Charge Description 1]   {AA}-[LYD]   {AE}-[100.000]





{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]
{S}-[END OF DOCUMENT ]   {BE}-[Page 1 of 1]


",
					message: "CoverPageText and CoverPageFooterText should be shown"
				);
			}
		}

		#region Decimal Comma

		[TestDate(2020, 1, 1)]
		public void TestDocument_PricingPage_DocBuilder_DecimalCommaPrintLanguage_DecimalPointLoginCountry()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);
			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "CHARGE1", 1000.11m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "SGSIN", "CHARGE2", 2000.22m, currency: "COP");
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					clientRate,
					expectedOutput: @"{C}-[Test Client #2 Tarifa Cliente]


{C}-[AIR - Aéreo Flete Tarifas]

{C}-[ Del Sydney Hasta Los Angeles]   {AZ}-[Validez: 01-ene.-20 - 01-jul.-20]

{C}-[Aerolínea]   {Q}-[Frecuencia]   {AA}-[Tiempo de tránsito]   {AK}-[Nivel de servicio]   {AW}-[Código mercancía]
{C}-[No especificado]   {Q}-[No especificado]   {AA}-[No especificado]   {AK}-[No especificado]   {AW}-[GEN]

{C}-[Descripción cargos]   {AA}-[Div.]   {AE}-[Piso]
{C}-[Charge Description 1]   {AA}-[AUD]   {AE}-[1,000.11]


{C}-[ Del Sydney Hasta Singapore]   {AZ}-[Validez: 01-ene.-20 - 01-jul.-20]

{C}-[Aerolínea]   {Q}-[Frecuencia]   {AA}-[Tiempo de tránsito]   {AK}-[Nivel de servicio]   {AW}-[Código mercancía]
{C}-[No especificado]   {Q}-[No especificado]   {AA}-[No especificado]   {AK}-[No especificado]   {AW}-[GEN]

{C}-[Descripción cargos]   {AA}-[Div.]   {AE}-[Piso]
{C}-[Charge Description 2]   {AA}-[COP]   {AE}-[2,000.22]





{S}-[FIN DEL DOCUMENTO ]   {BE}-[Página 1 de 1]
{S}-[FIN DEL DOCUMENTO ]   {BE}-[Página 1 de 1]


",
					language: "ES-ES", // Language with decimal comma
					message: "Decimal-point login company: WHEN printing in decimal-comma language THEN should have correct zeros with login company formatting"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_PricingPage_DocBuilder_DecimalCommaPrintLanguage_DecimalCommaLoginCountry()
		{
			TestHelper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code);
			TestHelper.ChargeCodes.New("CHARGE2", "Charge Description 2", FlatCalculator.Code);
			Factory.Save();

			var client = TestHelper.NewOrgHeader(1);
			var clientRate = TestHelper.NewClientRate(client);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "CHARGE1", 1000.11m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "SGSIN", "CHARGE2", 2000.22m, currency: "COP");
			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pricing Page");

			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Spain))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertRunDocument
				(
					clientRate,
					expectedOutput: @"{C}-[Test Client #2 Tarifa Cliente]


{C}-[AIR - Aéreo Flete Tarifas]

{C}-[ Del Sydney Hasta Los Angeles]   {AZ}-[Validez: 01-ene.-20 - 01-jul.-20]

{C}-[Aerolínea]   {Q}-[Frecuencia]   {AA}-[Tiempo de tránsito]   {AK}-[Nivel de servicio]   {AW}-[Código mercancía]
{C}-[No especificado]   {Q}-[No especificado]   {AA}-[No especificado]   {AK}-[No especificado]   {AW}-[GEN]

{C}-[Descripción cargos]   {AA}-[Div.]   {AE}-[Piso]
{C}-[Charge Description 1]   {AA}-[AUD]   {AE}-[1.000,11]


{C}-[ Del Sydney Hasta Singapore]   {AZ}-[Validez: 01-ene.-20 - 01-jul.-20]

{C}-[Aerolínea]   {Q}-[Frecuencia]   {AA}-[Tiempo de tránsito]   {AK}-[Nivel de servicio]   {AW}-[Código mercancía]
{C}-[No especificado]   {Q}-[No especificado]   {AA}-[No especificado]   {AK}-[No especificado]   {AW}-[GEN]

{C}-[Descripción cargos]   {AA}-[Div.]   {AE}-[Piso]
{C}-[Charge Description 2]   {AA}-[COP]   {AE}-[2.000,22]





{S}-[FIN DEL DOCUMENTO ]   {BE}-[Página 1 de 1]
{S}-[FIN DEL DOCUMENTO ]   {BE}-[Página 1 de 1]


",
					language: "ES-ES", // Language with decimal comma
					message: "Decimal-comma login company: WHEN printing in decimal-comma language THEN should have correct zeros with login company formatting"
				);
			}
		}

		#endregion

		#region Implementation

		public override BusinessObject GetBusinessObject => ClientRate;

		public override BusinessContext BusinessContext => BusinessContext.Rating;

		protected override void SetUp()
		{
			base.SetUp();
			var client = TestHelper.NewOrgHeader(1);
			ClientRate = TestHelper.NewClientRate(client);
		}

		ClientRate ClientRate;

		TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion
	}
}
