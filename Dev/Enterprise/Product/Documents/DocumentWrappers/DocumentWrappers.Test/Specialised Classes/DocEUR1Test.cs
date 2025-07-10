using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocEUR1))]
	sealed class DocEUR1Test : DocumentWrapperTestCase
	{
		#region TestShipmentMarksAndNumbers

		public void TestShipmentMarksAndNumbers()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_MarksAndNumbers = "Some Marks";
			DocEUR1 docEUR1 = DocEUR1.New(shipment, Factory);
			AssertEquals("Should be Some Marks", "Some Marks", docEUR1.ShipmentMarksAndNumbers);
		}

		#endregion

		#region Test TotalWeight / TotalVolume

		public void TestTotalWeight()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = 10;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			DocEUR1 docEUR1 = DocEUR1.New(shipment, Factory);
			AssertEquals("10000 KG", docEUR1.TotalWeight);
		}

		public void TestTotalVolume()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualVolume = 1000;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			DocEUR1 docEUR1 = DocEUR1.New(shipment, Factory);
			AssertEquals("1 M3", docEUR1.TotalVolume);
		}

		public void TestShipmentHasWeight()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualVolume = 1000;
			DocEUR1 docEUR1 = DocEUR1.New(shipment, Factory);
			AssertEquals("Show volume info", false, docEUR1.ShipmentHasWeight);

			shipment.JS_ActualWeight = 1000;
			AssertEquals("Show weight info", true, docEUR1.ShipmentHasWeight);
		}

		#endregion

		#region TestFormattedBody_WithItemNumbers

		public void TestFormattedBody_WithItemNumbers()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Iceland;

				const string expectedBody1 =
					//         1    1    2    2    3    3    4    4    5    5
					//1...5....0....5....0....5....0....5....0....5....0....5
					" 1) 6 BOX Random Description              Invoice1  \n" + // 0
					"    line2                                 Invoice2  \n" + // 1
					"    line3                                 Invoice3  \n" + // 2
					"    line4                                 Invoice4  \n" + // 3
					" 2) 3 PKG Random Description    23.000 CF           \n" + // 4
					"    line2                                           \n" + // 5
					"    line3                                           \n" + // 6
					"    line4                                           \n" + // 7            
					"11) 2 BAG Nucular Weapons                           \n" + // 8
					"----------------------------------------------------\n" + // 9
					"\\         \\         \\         \\         \\         \\ \n" + // 10
					" \\         \\         \\         \\         \\         \\\n" + // 11
					"  \\         \\         \\         \\         \\         \n" + // 12
					"   \\         \\         \\         \\         \\        \n" + // 13
					"    \\         \\         \\         \\         \\       \n" + // 14
					"     \\         \\         \\         \\         \\      \n" + // 15
					"      \\         \\         \\         \\         \\     \n" + // 16
					"       \\         \\         \\         \\         \\    \n" + // 17
					"        \\         \\         \\         \\         \\   \n" + // 18
					"         \\         \\         \\         \\         \\  " + // 19
					"";

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;

				DocumentNote note = DocumentNote.LoadNote(shipment);
				note.SetSystemDefinedFieldValue("Invoices", "Invoice1\r\nInvoice2\r\nInvoice3\r\nInvoice4");

				PackLine packline0 = shipment.OuterPackLines.AddNew();
				packline0.JL_ItemNo = 1;
				packline0.JL_PackageCount = 6;
				packline0.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
				packline0.JL_ActualWeight = 550.75;
				packline0.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline0.JL_ActualVolume = 0;
				packline0.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
				packline0.JL_Description = "Random Description\nline2\nline3\nline4";

				PackLine packline1 = shipment.OuterPackLines.AddNew();
				packline1.JL_ItemNo = 2;
				packline1.JL_PackageCount = 3;
				packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
				packline1.JL_ActualVolume = 23;
				packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
				packline1.JL_MarksAndNumbers = "Random Marks\nline2\nline3";
				packline1.JL_Description = "Random Description\nline2\nline3\nline4";

				PackLine packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_ItemNo = 11;
				packline2.JL_PackageCount = 2;
				packline2.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
				packline2.JL_ActualWeight = 100;
				packline2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline2.JL_Description = "Nucular Weapons";

				Dictionary<string, object> constants = new Dictionary<string, object>();
				constants[DocumentEngineIntegration.Constants.TemplateDefined.DetailWidth] = 30;
				constants[DocumentEngineIntegration.Constants.TemplateDefined.DetailMeasureGap] = 1;
				constants[DocumentEngineIntegration.Constants.TemplateDefined.MeasureWidth] = 10;
				constants[DocumentEngineIntegration.Constants.TemplateDefined.MeasureInvoiceGap] = 1;
				constants[DocumentEngineIntegration.Constants.TemplateDefined.InvoiceWidth] = 10;
				constants[DocumentEngineIntegration.Constants.TemplateDefined.BodyHeight] = 20;

				DocEUR1 wrapper = DocEUR1.New(shipment, Factory);
				wrapper.SetTemplateConstants(constants);

				AssertMultilineASCIIEquals("", expectedBody1, wrapper.FormattedBody);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = oldCountry;
			}
		}

		#endregion

		#region TestFormattedBody_WithoutItemNumbers

		public void TestFormattedBody_WithoutItemNumbers()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Iceland;

				const string expectedBody1 =
					//         1    1    2    2    3    3    4    4    5    5
					//1...5....0....5....0....5....0....5....0....5....0....5
					"6 BOX line1                      2.000 M3 Invoice1  \n" + // 0
					"line2                                     Invoice2  \n" + // 1
					"line3                                     Invoice3  \n" + // 2
					"line4                                     Invoice4  \n" + // 3  
					"3 PKG Random Description        23.000 CF           \n" + // 2
					"line2                                               \n" + // 3
					"line3                                               \n" + // 4
					"line4                                               \n" + // 
					"2 BAG Nucular Weapons                               \n" + // 8
					"----------------------------------------------------\n" + // 9
					"\\         \\         \\         \\         \\         \\ \n" + // 10
					" \\         \\         \\         \\         \\         \\\n" + // 11
					"  \\         \\         \\         \\         \\         \n" + // 12
					"   \\         \\         \\         \\         \\        \n" + // 13
					"    \\         \\         \\         \\         \\       \n" + // 14
					"     \\         \\         \\         \\         \\      \n" + // 15
					"      \\         \\         \\         \\         \\     \n" + // 16
					"       \\         \\         \\         \\         \\    \n" + // 17
					"        \\         \\         \\         \\         \\   \n" + // 18
					"         \\         \\         \\         \\         \\  " + // 19
					"";

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;

				DocumentNote note = DocumentNote.LoadNote(shipment);
				note.SetSystemDefinedFieldValue("Invoices", "Invoice1\r\nInvoice2\r\nInvoice3\r\nInvoice4");

				PackLine packline0 = shipment.OuterPackLines.AddNew();
				packline0.JL_PackageCount = 6;
				packline0.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
				packline0.JL_ActualWeight = 550.75;
				packline0.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline0.JL_ActualVolume = 2;
				packline0.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
				packline0.JL_Description = "line1\nline2\nline3\nline4";

				PackLine packline1 = shipment.OuterPackLines.AddNew();
				packline1.JL_PackageCount = 3;
				packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
				packline1.JL_ActualVolume = 23;
				packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
				packline1.JL_MarksAndNumbers = "Random Marks\nline2\nline3";
				packline1.JL_Description = "Random Description\nline2\nline3\nline4";

				PackLine packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_PackageCount = 2;
				packline2.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
				packline2.JL_ActualWeight = 100;
				packline2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline2.JL_Description = "Nucular Weapons";

				Dictionary<string, object> constants = new Dictionary<string, object>();
				constants["DetailWidth"] = 30;
				constants["DetailMeasureGap"] = 1;
				constants["MeasureWidth"] = 10;
				constants["MeasureInvoiceGap"] = 1;
				constants["InvoiceWidth"] = 10;
				constants["BodyHeight"] = 20;

				DocEUR1 wrapper = DocEUR1.New(shipment, Factory);
				wrapper.SetTemplateConstants(constants);

				AssertMultilineASCIIEquals("", expectedBody1, wrapper.FormattedBody);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = oldCountry;
			}
		}

		#endregion

		#region TestFormattedBody_WithoutDiagionalLinesWhenNonFCL

		public void TestFormattedBody_WithoutDiagionalLinesWhenNonFCL()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Iceland;

				const string expectedBody1 =
					//         1    1    2    2    3    3    4    4    5    5
					//1...5....0....5....0....5....0....5....0....5....0....5
					" 1) 6 BOX                        2.000 M3 Invoice1  \n" + // 0
					" 2) 3 PKG Random Description    23.000 CF Invoice2  \n" + // 1
					"    line2                                 Invoice3  \n" + // 2
					"    line3                                 Invoice4  \n" + // 3
					"11) 2 BAG Nucular Weapons                           \n" + // 4
					"----------------------------------------------------\n" + // 5
					"                                                    \n" + // 6
					"                                                    \n" + // 7
					"                                                    \n" + // 8
					"                                                    \n" + // 9
					"                                                    \n" + // 10
					"                                                    \n" + // 11
					"                                                    \n" + // 12
					"                                                    \n" + // 13
					"                                                    \n" + // 14
					"                                                    \n" + // 15
					"                                                    \n" + // 16
					"                                                    \n" + // 17
					"                                                    \n" + // 18
					"                                                    " + // 19
					"";

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;

				DocumentNote note = DocumentNote.LoadNote(shipment);
				note.SetSystemDefinedFieldValue("Invoices", "Invoice1\r\nInvoice2\r\nInvoice3\r\nInvoice4");

				PackLine packline0 = shipment.OuterPackLines.AddNew();
				packline0.JL_ItemNo = 1;
				packline0.JL_PackageCount = 6;
				packline0.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
				packline0.JL_ActualWeight = 550.75;
				packline0.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline0.JL_ActualVolume = 2;
				packline0.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

				PackLine packline1 = shipment.OuterPackLines.AddNew();
				packline1.JL_ItemNo = 2;
				packline1.JL_PackageCount = 3;
				packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
				packline1.JL_ActualVolume = 23;
				packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
				packline1.JL_MarksAndNumbers = "Random Marks\nline2\nline3";
				packline1.JL_Description = "Random Description\nline2\nline3";

				PackLine packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_ItemNo = 11;
				packline2.JL_PackageCount = 2;
				packline2.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
				packline2.JL_ActualWeight = 100;
				packline2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline2.JL_Description = "Nucular Weapons";

				Dictionary<string, object> constants = new Dictionary<string, object>();
				constants["DetailWidth"] = 30;
				constants["DetailMeasureGap"] = 1;
				constants["MeasureWidth"] = 10;
				constants["MeasureInvoiceGap"] = 1;
				constants["InvoiceWidth"] = 10;
				constants["BodyHeight"] = 20;
				constants["DiagionalLinesGap"] = 0;

				DocEUR1 wrapper = DocEUR1.New(shipment, Factory);
				wrapper.SetTemplateConstants(constants);

				AssertMultilineASCIIEquals("", expectedBody1, wrapper.FormattedBody);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = oldCountry;
			}
		}

		#endregion

		#region TestFormattedBody_WithoutDiagionalLinesWhenFCL()

		public void TestFormattedBody_WithoutDiagionalLinesWhenFCL()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Iceland;

				const string expectedBody1 =
					//         1    1    2    2    3    3    4    4    5    5
					//1...5....0....5....0....5....0....5....0....5....0....5
					" 1) 6 BOX                                 Invoice1  \n" + // 0
					" 2) 3 PKG Random Description              Invoice2  \n" + // 1
					"    line2                                 Invoice3  \n" + // 2
					"    line3                                 Invoice4  \n" + // 3
					"11) 2 BAG Nucular Weapons                           \n" + // 4
					"----------------------------------------------------\n" + // 5
					"                                                    \n" + // 6
					"                                                    \n" + // 7
					"                                                    \n" + // 8
					"                                                    \n" + // 9
					"                                                    \n" + // 10
					"                                                    \n" + // 11
					"                                                    \n" + // 12
					"                                                    \n" + // 13
					"                                                    \n" + // 14
					"                                                    \n" + // 15
					"                                                    \n" + // 16
					"                                                    \n" + // 17
					"                                                    \n" + // 18
					"                                                    " + // 19
					"";

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

				DocumentNote note = DocumentNote.LoadNote(shipment);
				note.SetSystemDefinedFieldValue("Invoices", "Invoice1\r\nInvoice2\r\nInvoice3\r\nInvoice4");

				PackLine packline0 = shipment.OuterPackLines.AddNew();
				packline0.JL_ItemNo = 1;
				packline0.JL_PackageCount = 6;
				packline0.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
				packline0.JL_ActualWeight = 550.75;
				packline0.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline0.JL_ActualVolume = 2;
				packline0.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

				PackLine packline1 = shipment.OuterPackLines.AddNew();
				packline1.JL_ItemNo = 2;
				packline1.JL_PackageCount = 3;
				packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
				packline1.JL_ActualVolume = 23;
				packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
				packline1.JL_MarksAndNumbers = "Random Marks\nline2\nline3";
				packline1.JL_Description = "Random Description\nline2\nline3";

				PackLine packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_ItemNo = 11;
				packline2.JL_PackageCount = 2;
				packline2.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
				packline2.JL_ActualWeight = 100;
				packline2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline2.JL_Description = "Nucular Weapons";

				Dictionary<string, object> constants = new Dictionary<string, object>();
				constants["DetailWidth"] = 30;
				constants["DetailMeasureGap"] = 1;
				constants["MeasureWidth"] = 10;
				constants["MeasureInvoiceGap"] = 1;
				constants["InvoiceWidth"] = 10;
				constants["BodyHeight"] = 20;
				constants["DiagionalLinesGap"] = 0;

				DocEUR1 wrapper = DocEUR1.New(shipment, Factory);
				wrapper.SetTemplateConstants(constants);

				AssertMultilineASCIIEquals("", expectedBody1, wrapper.FormattedBody);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = oldCountry;
			}
		}

		#endregion

		#region TestFormattedBody_ShowWeightOrVolume

		public void TestFormattedBody_ShowWeightOrVolume()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Iceland;

				const string expectedBodyWeight =
					//         1    1    2    2    3    3    4    4    5    5
					//1...5....0....5....0....5....0....5....0....5....0....5
					"0) 0 PLT                                  Invoice1  \n" + // 0
					"1) 6 BOX                       150.200 KG Invoice2  \n" + // 1
					"2) 3 PKG                                  Invoice3  \n" + // 2
					"----------------------------------------------------\n" + // 3
					"                                                    \n" + // 4
					"";

				const string expectedBodyVolume =
					//         1    1    2    2    3    3    4    4    5    5
					//1...5....0....5....0....5....0....5....0....5....0....5
					"0) 0 PLT                                  Invoice1  \n" + // 0
					"1) 6 BOX                         2.000 M3 Invoice2  \n" + // 1
					"2) 3 PKG                        23.000 CF Invoice3  \n" + // 2
					"----------------------------------------------------\n" + // 3
					"                                                    \n" + // 4
					"";

				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
				shipment.JS_ActualWeight = 10;
				shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;

				DocumentNote note = DocumentNote.LoadNote(shipment);
				note.SetSystemDefinedFieldValue("Invoices", "Invoice1\r\nInvoice2\r\nInvoice3");

				PackLine packline0 = shipment.OuterPackLines.AddNew();
				packline0.JL_ItemNo = 1;
				packline0.JL_PackageCount = 6;
				packline0.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
				packline0.JL_ActualWeight = 150.20;
				packline0.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline0.JL_ActualVolume = 2;
				packline0.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

				PackLine packline1 = shipment.OuterPackLines.AddNew();
				packline1.JL_ItemNo = 2;
				packline1.JL_PackageCount = 3;
				packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
				packline1.JL_ActualVolume = 23;
				packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;

				Dictionary<string, object> constants = new Dictionary<string, object>();
				constants["DetailWidth"] = 30;
				constants["DetailMeasureGap"] = 1;
				constants["MeasureWidth"] = 10;
				constants["MeasureInvoiceGap"] = 1;
				constants["InvoiceWidth"] = 10;
				constants["BodyHeight"] = 5;
				constants["DiagionalLinesGap"] = 0;

				DocEUR1 wrapper = DocEUR1.New(shipment, Factory);
				wrapper.SetTemplateConstants(constants);
				AssertMultilineASCIIEquals("", expectedBodyWeight, wrapper.FormattedBody);

				shipment.JS_ActualWeight = 0;
				wrapper = DocEUR1.New(shipment, Factory);
				AssertMultilineASCIIEquals("", expectedBodyVolume, wrapper.FormattedBody);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = oldCountry;
			}
		}

		#endregion

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			return new DocumentWrapper[]
			{
				DocEUR1.New(shipment, Factory),
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return DocEUR1.New(shipment, Factory);
		}

		#endregion
	}
}
