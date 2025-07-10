using System;
using System.Diagnostics;
using Enterprise.DocumentEngine.FlexCelInterface;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ExcelCellAnalyzerTest : TestCase
	{
		public void TestHasTextToBeTranslated()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<CurrentCompany>]
{B}-[Container]
{B}-[Container <CurrentCompany>]

{B}-[  <CurrentCompany>  ]
{B}-[       Container    ]
{B}-[<Modifiable(Phone: <NotifyParty.ContactPhone>)>]
{B}-[<Modifiable(<NotifyParty.ContactName> - <NotifyParty.ContactPhone>)>]
{B}-[<ShrinkToFit><Translate(ShipmentTransportMode.Description)> <If(""<ShipmentContainerMode.Code>"" == ""LSE"" || ""<ShipmentContainerMode.Code>"" == ""AIR"", """", ""<ShipmentContainerMode.Code> "")><If(""<DocumentDirection>""==""DEP"" && !""<Translate(ReportNameShort)>"".StartsWith(""Departure""),""Departure "","""")><IF(""<DocumentDirection>""==""ARV"" && !""<Translate(ReportNameShort)>"".StartsWith(""Arrival"") ,""Arrival "","""")><Translate(ReportNameShort)>]
{B}-[<Containers.Format(""{ContainerNo}"", Comma)>]
{B}-[<AutoHeight><Containers.Format(""{ContainerNo} ({Mode} / {SealNo} / {Type})"", Comma)>]
{B}-[<Containers.Format(""{Count} x {Type}"", Comma, """", ""{Type}"")>]
{B}-[<Containers.Format(""Count: {Count}"", Comma, """", ""{Type}"")>]
{B}-[<Containers.Format(""{Count}"", Comma, """", ""{Type}"")>  <Containers.Format(""{Count}"", Comma, """", ""{Type}"")>]
{B}-[<Containers.Format(""{Count}"", Comma, """", ""{Type}"")>  <Containers.Format(""Count: {Count}"", Comma, """", ""{Type}"")>]
{B}-F[=IF(AB123=""Y"", ""<Job.Charges.JR_AgentDeclaredSellAmt>"", """")]
{B}-F[=IF(AB123=""Y"", ""Yes"", ""No"")]
{B}-F[=IF(AB123=""N"", ""<Job.Charges.JR_AgentDeclaredSellAmt>"", """")]
{B}-F[=IF(AB123=""N"", ""Yes"", ""No"")]
{B}-[<HideRowIfCellIsEmpty><ModifiableField(""OtherDocumentsNote"")>]
{B}-[<ShrinkToFit><Translate(ShipmentTransportMode.Description)> <If(""<ShipmentContainerMode.Code>"" == ""LSE"" || ""<ShipmentContainerMode.Code>"" == ""AIR"", """", ""<ShipmentContainerMode.Code> "")><Translate(ReportName)>]
{B}-[<If(""<ShipmentContainerMode.Code>"" == ""LSE"", ""<TrueMacro>"", ""<FalseMacro>"")>]
{B}-[<If(""<ShipmentContainerMode.Code>"" == ""LSE"", ""True"", ""False"")>]
{B}-[<If(""<TransportCode>"" == ""AIR"", ""<TrueMacro>"", ""<FalseMacro>"")>]
{B}-[<If(""<ContainerType>"" == ""BAG"", ""<TrueMacro>"", ""<FalseMacro>"")>]
{B}-F[=IF(""<ARInvoice.LinesForInvoice.ChargeCode.ChargeType>"" = ""CMT"", """", AB123)]
{B}-[<ShipmentOuterPacksQty.ValueAndUnitCode> (OUTER), <ShipmentInnerPacksQty.ValueAndUnitCode> (INNER)]
{B}-[<ShipmentOuterPacksQty.ValueAndUnitCode> (OUTER), <ShipmentInnerPacksQty.ValueAndUnitCode> (INNER) ]
{B}-[<If(""<TransportCode>"" == ""AIR"", ""<A> of <B>"", ""<B> of <C>"")>]
{B}-[<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""IMP"", ""Origin"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""EXP"", ""Destination"", ""Origin - Destination"")>"")>]
{B}-[<ShipmentTransportMode.Description> <If(!""<ReportNameShort>"".StartsWith(""<DepartureOrArrivalText>""), ""<DepartureOrArrivalText> "", """")><ReportNameShort>]
{B}-[<DocDataValue(""Consignee Name"", ""<Consignee.CompanyNameAndAddress>"")>]
{B}-[Name: <If(<DocDataValue(""Consignee Name"")> == """", ""Blank"", <DocDataValue(""Consignee Name"")>)>]
{B}-[<If(""<Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Page.Entries[1].IfDisplayProvider>"" == ""True"", ""<Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Provider.TypeDescription>:"", """")>]
{B}-[<If(""<Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Label>"" == ""FreightRates"", ""<Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Mode> Freight from <Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.RateLine.Origin> to <Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.RateLine.Destination>"", """")>]
{B}-[<AutoHeight><PickupDeliveryConfirmations.Packages.FreightPackLine.PackLocations.Format(""{JQ_WarehouseLocation}, PACKS:  {JQ_NoPackages} <PickupDeliveryConfirmations.Packages.Packages.Unit.Code>"", NewLine)>]
{B}-[Container <CurrentCompany>]
{B}-[<ShipmentRoutes.Transport.VesselName> / <ShipmentRoutes.Transport.VoyageNo> / <ShipmentRoutes.Transport.LloydsNo>]
{B}-F[=BU3214 & "" PKG""]
{B}-F[=CONCATENATE(""<Job.Company.LocalCurrency.RX_Code> "", BU2136)]
{B}-[<ExpandToFit><Modifiable(<If(""<DocDataValue(""Consignee is 'To Order'"")>"" == ""Y"", ""<If(""<DocDataValue(""Consignee Name"")>"" == """", ""To Order"", ""To Order of <DocDataValue(""Consignee Name"", """")>"")>"", ""<DocDataValue(""Consignee Name"", ""<Consignee.CompanyNameAndAddress>"")>"")>]
{B}-[<JobHeader.JobCharges.Total(""LocalSellAmount"", 2, ""<ChargeCode.ChargeType>"" == ""DSB"")>]
{B}-[<FormatNumber(<If(""<JobHeader.JobCharges.Total(""LocalSellAmount"", 2, ""<ChargeCode.ChargeType>"" == ""DSB"")>"" == """", ""0"", ""< JobHeader.JobCharges.Total(""LocalSellAmount"", 2, ""<ChargeCode.ChargeType>"" == ""DSB"")>"")>, <Job.Company.LocalCurrency.RX_Code>)>]
{B}-[<JobHeader.JobCharges.Total(""LocalSellAmount"", 2, ""{ChargeCode.ChargeType}"" == ""Y"")>]
{B}-[<CustomisedColumn(""Organization"")>]
{B}-F[=IF(""<TransportCode>"" = ""AIR"", ""AIR"", ""Job Sales Rep: <TransportCode><AutoHeight>"")]
{B}-F[=IF(""<IsExport>""=""True"", ""<Format({B2C:N2})>"")]
{B}-[""<Format(""{B2C:N2}"")>""]
{B}-[<IsExport>Test<Format(""{B2C:N2}"")>]
{B}-F[=IF(""<IsExport>""=""True"", ""<B2C>"")]
{A}-[#EndOfReport]");

			CombineAssertions(delegate
			{
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 3, 1);
				AssertTextToBeTranslated(new string[] { "Container" }, workSheet, 4, 1, new string[] { "Zbiornik" }, "Zbiornik");
				AssertTextToBeTranslated(new string[] { "Container {0}" }, workSheet, 5, 1, new string[] { "Zbiornik {0}" }, "Zbiornik <CurrentCompany>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 6, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 7, 1);
				AssertTextToBeTranslated(new string[] { "Container" }, workSheet, 8, 1, new string[] { "Zbiornik" }, "       Zbiornik    ");
				AssertTextToBeTranslated(new string[] { "Phone: {0}" }, workSheet, 9, 1, new string[] { "Telefon : {0}" }, "<Modifiable(Telefon : <NotifyParty.ContactPhone>)>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 10, 1);
				AssertTextToBeTranslated(new string[] { "Departure", "Departure", "Arrival", "Arrival" }, workSheet, 11, 1, new string[] { "Odlot", "Odlot", "Przylot", "Przylot" }, @"<ShrinkToFit><Translate(ShipmentTransportMode.Description)> <If(""<ShipmentContainerMode.Code>"" == ""LSE"" || ""<ShipmentContainerMode.Code>"" == ""AIR"", """", ""<ShipmentContainerMode.Code> "")><If(""<DocumentDirection>""==""DEP"" && !""<Translate(ReportNameShort)>"".StartsWith(""Odlot""),""Odlot "","""")><IF(""<DocumentDirection>""==""ARV"" && !""<Translate(ReportNameShort)>"".StartsWith(""Przylot"") ,""Przylot "","""")><Translate(ReportNameShort)>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 12, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 13, 1);
				AssertTextToBeTranslated(new string[] { "{0} x {1}" }, workSheet, 14, 1, new string[] { "{1} z {0}" }, @"<Containers.Format(""{Type} z {Count}"", Comma, """", ""{Type}"")>");
				AssertTextToBeTranslated(new string[] { "Count: {0}" }, workSheet, 15, 1, new string[] { "Hrabia : {0}" }, @"<Containers.Format(""Hrabia : {Count}"", Comma, """", ""{Type}"")>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 16, 1);
				AssertTextToBeTranslated(new string[] { "Count: {0}" }, workSheet, 17, 1, new string[] { "Hrabia : {0}" }, @"<Containers.Format(""{Count}"", Comma, """", ""{Type}"")>  <Containers.Format(""Hrabia : {Count}"", Comma, """", ""{Type}"")>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 18, 1);
				AssertTextToBeTranslated(new string[] { "Yes", "No" }, workSheet, 19, 1, new string[] { "Tak", "Nie" }, @"=IF(AB123=""Y"", ""Tak"", ""Nie"")");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 20, 1);
				AssertTextToBeTranslated(new string[] { "Yes", "No" }, workSheet, 21, 1, new string[] { "Tak", "Nie" }, @"=IF(AB123=""N"", ""Tak"", ""Nie"")");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 22, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 23, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 24, 1);
				AssertTextToBeTranslated(new string[] { "True", "False" }, workSheet, 25, 1, new string[] { "Dokładny", "Fałszywy" }, @"<If(""<ShipmentContainerMode.Code>"" == ""LSE"", ""Dokładny"", ""Fałszywy"")>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 26, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 27, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 28, 1);
				AssertTextToBeTranslated(new string[] { "{0} (OUTER), {1} (INNER)" }, workSheet, 29, 1, new string[] { "{0} (X), {1} (Y)" }, @"<ShipmentOuterPacksQty.ValueAndUnitCode> (X), <ShipmentInnerPacksQty.ValueAndUnitCode> (Y)");
				AssertTextToBeTranslated(new string[] { "{0} (OUTER), {1} (INNER)" }, workSheet, 30, 1, new string[] { "{0} (X), {1} (Y)" }, @"<ShipmentOuterPacksQty.ValueAndUnitCode> (X), <ShipmentInnerPacksQty.ValueAndUnitCode> (Y) ");
				AssertTextToBeTranslated(new string[] { "{0} of {1}", "{0} of {1}" }, workSheet, 31, 1, new string[] { "{1} X {0}", "{1} X {0}" }, @"<If(""<TransportCode>"" == ""AIR"", ""<B> X <A>"", ""<C> X <B>"")>");
				AssertTextToBeTranslated(new string[] { "Destination", "Origin - Destination", "Origin", }, workSheet, 32, 1, new string[] { "Zielort", "Ursprung - Zielort", "Ursprung", }, @"<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""IMP"", ""Ursprung"", ""<If(""<Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.Row.Entries[1].Direction.Code>"" == ""EXP"", ""Zielort"", ""Ursprung - Zielort"")>"")>");
				AssertTextToBeTranslated(new string[] { "{0} {1}{2}" }, workSheet, 33, 1, new string[] { "{1}{2} {0}" }, @"<If(!""<ReportNameShort>"".StartsWith(""<DepartureOrArrivalText>""), ""<DepartureOrArrivalText> "", """")><ReportNameShort> <ShipmentTransportMode.Description>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 34, 1);
				AssertTextToBeTranslated(new string[] { "Blank", "Name: {0}" }, workSheet, 35, 1, new string[] { "Knalb", "Eman: {0}" }, @"Eman: <If(<DocDataValue(""Consignee Name"")> == """", ""Knalb"", <DocDataValue(""Consignee Name"")>)>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 36, 1);
				AssertTextToBeTranslated(new string[] { "{0} Freight from {1} to {2}" }, workSheet, 37, 1, new string[] { "{0} Tghierf morf {1} ot {2}" }, @"<If(""<Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Label>"" == ""FreightRates"", ""<Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Page.Entries[1].Mode> Tghierf morf <Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.RateLine.Origin> ot <Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.RateLine.Destination>"", """")>");
				AssertTextToBeTranslated(new string[] { "{0}, PACKS:  {1} {2}" }, workSheet, 38, 1, new string[] { "{0}, SKCEP:  {1} {2}" }, @"<AutoHeight><PickupDeliveryConfirmations.Packages.FreightPackLine.PackLocations.Format(""{JQ_WarehouseLocation}, SKCEP:  {JQ_NoPackages} <PickupDeliveryConfirmations.Packages.Packages.Unit.Code>"", NewLine)>");
				AssertTextToBeTranslated(new string[] { "Container {0}" }, workSheet, 39, 1, new string[] { "Zbiornik {0:U}" }, "Zbiornik <ChangeCase(\"<CurrentCompany>\",U)>");
				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Arabic))
				{
					AssertTextToBeTranslated(new string[] { "{0} / {1} / {2}" }, workSheet, 40, 1, new string[] { "{2} / {1} / {0}" }, "<ShipmentRoutes.Transport.LloydsNo> / <ShipmentRoutes.Transport.VoyageNo> / <ShipmentRoutes.Transport.VesselName>");
					AssertTextToBeTranslated(new string[] { "PKG", "{0} & {1}" }, workSheet, 41, 1, new string[] { "PKG", "{1} & {0}" }, "=\"PKG \" & BU3214");
					AssertTextToBeTranslated(new string[] { "{0}", "{2}, {0}" }, workSheet, 42, 1, new string[] { "{0}", "{0}, {2}", }, @"=CONCATENATE(BU2136, "" <Job.Company.LocalCurrency.RX_Code>"")");
				}
				AssertTextToBeTranslated(new string[] { "To Order", "To Order of {0}" }, workSheet, 43, 1, new string[] { "Order Zum", "Order of {0} Zum" }, @"<ExpandToFit><Modifiable(<If(""<DocDataValue(""Consignee is 'To Order'"")>"" == ""Y"", ""<If(""<DocDataValue(""Consignee Name"")>"" == """", ""Order Zum"", ""Order of <DocDataValue(""Consignee Name"", """")> Zum"")>"", ""<DocDataValue(""Consignee Name"", ""<Consignee.CompanyNameAndAddress>"")>"")>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 44, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 45, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 46, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 47, 1);
				AssertTextToBeTranslated(new string[] { "AIR", "Job Sales Rep: {0}{1}" }, workSheet, 48, 1, new string[] { "AIR", @"Job Sales Rep: {0}{1}" }, @"=IF(""<TransportCode>"" = ""AIR"", ""AIR"", ""Job Sales Rep: <TransportCode><AutoHeight>"")");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 49, 1);
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 50, 1);
				AssertTextToBeTranslated(new string[] { "{0}Test{1}" }, workSheet, 51, 1, new string[] { "{0}测试{1}" }, "<IsExport>测试<Format(\"{B2C:N2}\")>");
				AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 52, 1);
			});
		}

		public void TestGetTextToBeTranslatedWithNonTranslatableParameters()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<HideRowIfCellIsEmpty><BookingInstructions.Find(""{InstructionType}"" == ""Pickup"").ServiceInstruction><BookingInstructions.Find(""{InstructionType}"" == ""Delivery"").ServiceInstruction>]
{B}-[<DocDataValue(""Consignee Name"", ""<Consignee.CompanyNameAndAddress>"")>]
{B}-[Delivery: <BookingInstructions.Find(""{InstructionType}"" == ""Delivery"").ServiceInstruction>]
{B}-[Name: <If(<DocDataValue(""Consignee Name"")> == """", ""Blank"", <DocDataValue(""Consignee Name"")>)>]
{A}-[#EndOfReport]
");

			AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 3, 1);
			AssertTextToBeTranslated(Array.Empty<string>(), workSheet, 4, 1);
			AssertTextToBeTranslated(new string[] { "Delivery: {0}" }, workSheet, 5, 1, new string[] { "货运: {0}" }, "货运: <BookingInstructions.Find(\"{InstructionType}\" == \"Delivery\").ServiceInstruction>");
			AssertTextToBeTranslated(new string[] { "Blank", "Name: {0}" }, workSheet, 6, 1, new string[] { "Knalb", "Eman: {0}" }, @"Eman: <If(<DocDataValue(""Consignee Name"")> == """", ""Knalb"", <DocDataValue(""Consignee Name"")>)>");
		}

		public void TestNewlinesAreNormalized()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[]
{A}-[#EndOfReport]");

			var cell = workSheet.GetCell(3, 1);
			var normalizedText = "ABC \r\n DEF";
			cell.ValueSourceText = "ABC \n DEF";
			var analyzer = new ExcelCellAnalyzer(cell);
			AssertEquals("Newlines should have been normalized", normalizedText, analyzer.TranslatableRoot.TranslatableText);

			var analyzer2 = new ExcelCellAnalyzer("ABC \r\n DEF");
			AssertEquals("Newlines should have been normalized", normalizedText, analyzer2.TranslatableRoot.TranslatableText);
		}

		[DeveloperOnlyTest]
		public void TestGetTextToBeTranslated_Performance()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<CurrentCompany>]
{B}-[Container]
{B}-[Container <CurrentCompany>]

{B}-[  <CurrentCompany>  ]
{B}-[       Container    ]
{A}-[#EndOfReport]");

			var cell = workSheet.GetCell(4, 1);
			var analyzer = new ExcelCellAnalyzer(cell);
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			for (var i = 0; i < 1000000; i++)
			{
				_ = analyzer.GetTextToBeTranslated();
			}

			AssertLessThan(stopWatch.ElapsedMilliseconds, 10000);
		}

		#region Implementation

		void AssertTextToBeTranslated(string[] expected, ExcelWorkSheet workSheet, int row, int column, string[] translations = null, string expectedCellTranslation = null)
		{
			var cell = workSheet.GetCell(row, column);
			var analyzer = new ExcelCellAnalyzer(cell);
			var message = string.Format("[{0}:{1}] translatable elements.", cell.Reference, cell.ValueSourceText);
			ExcelCellAnalyzer.TranslatablePart[] translatable = analyzer.GetTextToBeTranslated();
			AssertArrayEqualsByElements<string>(message, expected, Array.ConvertAll(translatable, t => t.TranslatableText));
			if (expected.Length > 0)
			{
				AssertEquals(expected.Length, translations.Length);
				for (int i = 0; i < translatable.Length; i++)
				{
					translatable[i].TranslatableText = translations[i];
				}
				analyzer.SetTranslation();
				AssertEquals(expectedCellTranslation, cell.ValueSourceText);
			}
		}

		#endregion
	}
}
