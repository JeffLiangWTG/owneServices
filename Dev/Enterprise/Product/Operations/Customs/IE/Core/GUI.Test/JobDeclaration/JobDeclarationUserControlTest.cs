using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestStyleOfEntrySOEDropDownVisible()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("StyleOfEntrySOEDropDown should not be visible", false, control.FindSingle<ZDropEdit>("StyleOfEntrySOEDropDown").Visible);
			}
		}

		public void TestExportUCC6ControlsCaptions()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() => AssertCaptions(control));
			}
		}

		void AssertCaptions(JobDeclarationUserControl control)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertControlCaption(control.IncoTermDropEdit, "Incoterm");
			AssertControlCaption(control.JE_ShipmentIncoTermPlaceTextBox, "Place");
			AssertControlCaption(control.FinalDestinationFindBox, "Destination");
			AssertControlCaption(control.OriginFindBox, "Dispatch");
			AssertControlCaption(control, "EntryStyleDropEdit", "Declaration Type", "[11 01 001 000] Declaration Type");
			AssertControlCaption(control, "TransportModeDropEdit", "Trans. Mode", "[19 03 001 000] Transport Mode at Border");
			AssertControlCaption(control, "BorderTransportMeansDropEdit", "Trans. ID.", "[19 08 061 000] Transport Identification Means at Border");
			AssertControlCaption(control, "InlandModeOfTransportDropEdit", "Trans. Mode", "[19 04 001 000] Inland Mode of Transport");
			AssertControlCaption(control, "ContainerModeDropEdit", "Container", "[19 01 001 000] Container Indicator");
			AssertControlCaption(control, "OwnersReferenceTextBox", "Owner Ref.");
			AssertControlCaption(control, "TotalNoOfPacksCalcDropEdit", "No. Pkgs.");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertControlCaption(control, "FlightNumberTextBox", "Flight No.", "[19 08 017 000] Flight Number");
			AssertControlCaption(control, "TransportNationalityFindBox", "Nationality", "[19 08 062 000] Nationality", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			AssertControlCaption(control, "TransportIDTextBox", "Transport ID");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertControlCaption(control, "TransportNationalityFindBox", "Nationality", "[19 08 062 000] Nationality", parentControlName: nameof(Customs.GUI.VoyageAndNationalityUserControl));
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertControlCaption(control, "TransportIDTextBox", "Wagon Number", "[19 08 017 000] Wagon Number", parentControlName: nameof(EU.GUI.TransportIDAndNationalityRailUserControl));

			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			AssertControlCaption(control, "TransportIDTextBox", "Registration No.", "[19 05 017 000] Vehicle Registration Number", parentControlName: nameof(Customs.GUI.TransportInlandRoadUserControl));
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
			declaration.JE_TransportMeans = TransportMeansList.Codes.IataFlightNumber;
			AssertControlCaption(control, "TransportIDTextBox", "Flight No.", "[19 05 017 000] Flight No.", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportMeans = TransportMeansList.Codes.RegistrationNumberOfTheAircraft;
			AssertControlCaption(control, "TransportIDTextBox", "Registration No.", "[19 05 017 000] Aircraft Registration Number", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
			AssertControlCaption(control, "TransportIDTextBox", "Transport ID", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportModeInland = TransportTypeList.Codes.InlandWaterwayTransport;
			declaration.JE_TransportMeans = TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode;
			AssertControlCaption(control, "TransportIDTextBox", "ENI Code", "[19 05 017 000] European Vessel Identification Number", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel;
			AssertControlCaption(control, "TransportIDTextBox", "Vessel Name", "[19 05 017 000] Vessel Name", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportModeInland = TransportTypeList.Codes.OwnPropulsion;
			AssertControlCaption(control, "TransportIDTextBox", "Transport ID", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
			AssertControlCaption(control, "TransportIDTextBox", "Transport ID", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
			declaration.JE_TransportMeans = TransportMeansList.Codes.WagonNumber;
			AssertControlCaption(control, "TransportIDTextBox", "Wagon No.", "[19 05 017 000] Wagon Number", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportMeans = TransportMeansList.Codes.TrainNumber;
			AssertControlCaption(control, "TransportIDTextBox", "Train No.", "[19 05 017 000] Train Number", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
			AssertControlCaption(control, "VesselIDCodeFindBox", "Lloyds No.", "[19 05 017 000] Lloyds Number", parentControlName: nameof(Customs.GUI.TransportInlandSeaUserControl));
			declaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;
			AssertControlCaption(control, "VesselIDCodeFindBox", "Vessel Name", "[19 05 017 000] Vessel Name", parentControlName: nameof(Customs.GUI.TransportInlandSeaUserControl));

			declaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
			AssertControlCaption(control, "TransportIDTextBox", "Transport ID", parentControlName: nameof(Customs.GUI.TransportInlandIDAndNationalityUserControl));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertControlCaption(control.IncoTermDropEdit, "Incoterm");
			AssertControlCaption(control.JE_ShipmentIncoTermPlaceTextBox, "Place");
			AssertControlCaption(control.FinalDestinationFindBox, "Destination");
			AssertControlCaption(control.OriginFindBox, "Dispatch");
			AssertControlCaption(control, "OwnersReferenceTextBox", "Declarant's Ref");
			AssertControlCaption(control, "TotalNoOfPacksCalcDropEdit", "No. Pkgs.");
		}

		public void TestJobDeclarationUserControlSize()
		{
			using (var control = new JobDeclarationUserControl())
			{
				AssertEquals("JobDeclarationUserControl.Size", ControlDpiScalingHelper.NewScaledSize(1114, 857, true), control.Size);
			}
		}

		public void TestCustomsOfficesUserControlSize()
		{
			using (var control = new JobDeclarationUserControl())
			{
				var customsOfficesUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("CustomsOfficesUserControl");
				AssertEquals("CustomsOfficesUserControl.Size", ControlDpiScalingHelper.NewScaledSize(469, 155, true), customsOfficesUserControl.Size);
			}
		}

		public void TestPresentationUserControlLocation()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				declaration.JE_MessageType = "EXP";
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("PresentationUserControl.Location", ControlDpiScalingHelper.NewScaledPoint(255, 837, true), control.FindSingle<PresentationUserControl>("PresentationUserControl").Location);
			}
		}

		public void TestCustomsOfficesUserControlType()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				var customsOfficesUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("CustomsOfficesUserControl");
				AssertType<CustomsOfficesUserControl>(customsOfficesUserControl.HostedControl);
			}
		}

		public void TestPresentationUserControlVisible()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				declaration.JE_MessageType = "EXP";
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				Assert("PresentationUserControl.Visible", control.FindSingle<PresentationUserControl>("PresentationUserControl").Visible);
				declaration.JE_MessageType = "IMP";
				Assert("PresentationUserControl.Visible", !control.FindSingle<PresentationUserControl>("PresentationUserControl").Visible);
			}
		}

		public void TestSupplierDocAddress_Caption()
		{
			using (var control = new JobDeclarationUserControl())
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				control.JobDeclaration = declaration;

				var supplierDocAddressCaption = control.SupplierDocAddress.CaptionResourceString;
				CombineAssertions("Export", () =>
				{
					AssertEquals("Supplier/Exporter", supplierDocAddressCaption.Caption);
					AssertEquals("[13 01 000 000] Supplier/Exporter", supplierDocAddressCaption.FullDescription);
				});

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				supplierDocAddressCaption = control.SupplierDocAddress.CaptionResourceString;
				CombineAssertions("Import", () =>
				{
					AssertEquals("Supplier", supplierDocAddressCaption.Caption);
					AssertEquals(ZString.Empty, supplierDocAddressCaption.FullDescription);
				});

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				supplierDocAddressCaption = control.SupplierDocAddress.CaptionResourceString;
				CombineAssertions("Import V1", () =>
				{
					AssertEquals("Exporter", supplierDocAddressCaption.ShortCaption);
					AssertEquals("[3/1] Exporter", supplierDocAddressCaption.MediumCaption);
					AssertEquals("[3/1 && 3/2] Exporter (ID)", supplierDocAddressCaption.Caption);
					AssertEquals("[3/1] Exporter & [3/2] Exporter Identification Number", supplierDocAddressCaption.FullDescription);
				});
			}
		}

		public void TestImporterDocAddress_Caption()
		{
			using (var control = new JobDeclarationUserControl())
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				control.JobDeclaration = declaration;

				var importerDocAddressCaption = control.ImporterDocAddress.CaptionResourceString;
				CombineAssertions("Export", () =>
				{
					AssertEquals("Importer/Consignee", importerDocAddressCaption.Caption);
					AssertEquals("[13 03 000 000] Importer/Consignee", importerDocAddressCaption.FullDescription);
				});

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				importerDocAddressCaption = control.ImporterDocAddress.CaptionResourceString;
				CombineAssertions("Import", () =>
				{
					AssertEquals("Importer", importerDocAddressCaption.Caption);
					AssertEquals(ZString.Empty, importerDocAddressCaption.FullDescription);
				});
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				importerDocAddressCaption = control.ImporterDocAddress.CaptionResourceString;
				CombineAssertions("Import V1", () =>
				{
					AssertEquals("Importer", importerDocAddressCaption.ShortCaption);
					AssertEquals("[3/15] Importer", importerDocAddressCaption.MediumCaption);
					AssertEquals("[3/15 && 3/16] Importer (ID)", importerDocAddressCaption.Caption);
					AssertEquals("[3/15] Importer & [3/16] Importer Identification Number", importerDocAddressCaption.FullDescription);
				});
			}
		}

		public void TestLocationOfGoodsFields()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var goodsLocationDropEdit = control.FindSingle<ZDropEdit>("GoodsLocationDropEdit");
				var locationOfGoodsCodeFindBox = control.JE_LocationOfGoodsCodeFindBox;
				var locationQualifierDropEdit = control.JE_LocationQualifierDropEdit;
				var locationOtherInformationDropEdit = control.JE_LocationOtherInformationDropEdit;
				var subLocationOfGoodsTextBox = control.JE_SubLocationOfGoodsTextBox;
				var locationOfGoodsCtryTextBox = control.JE_Calc_LocationOfGoodsCtryTextBox;

				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;

				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("JE_LocationOfGoodsCodeFindBox.Location", ControlDpiScalingHelper.NewScaledPoint(119, 111), locationOfGoodsCodeFindBox.Location);
					AssertEquals("JE_LocationOfGoodsCodeFindBox.TabIndex", 9, locationOfGoodsCodeFindBox.TabIndex);

					AssertEquals("JE_LocationQualifierDropEdit.Location", ControlDpiScalingHelper.NewScaledPoint(194, 136), locationQualifierDropEdit.Location);
					AssertEquals("JE_LocationQualifierDropEdit.TabIndex", 11, locationQualifierDropEdit.TabIndex);

					AssertEquals("JE_LocationOtherInformationDropEdit.Location", ControlDpiScalingHelper.NewScaledPoint(336, 136), locationOtherInformationDropEdit.Location);
					AssertEquals("JE_LocationOtherInformationDropEdit.TabIndex", 12, locationOtherInformationDropEdit.TabIndex);

					AssertEquals("GoodsLocationDropEdit.Visible", false, goodsLocationDropEdit.Visible);
					AssertEquals("JE_SubLocationOfGoodsTextBox.Visible", true, subLocationOfGoodsTextBox.Visible);
					AssertEquals("JE_Calc_LocationOfGoodsCtryTextBox.Visible", true, locationOfGoodsCtryTextBox.Visible);
					AssertEquals("JE_LocationOfGoodsCodeFindBox.Visible", true, locationOfGoodsCodeFindBox.Visible);
					AssertEquals("JE_LocationQualifierDropEdit.Visible", true, locationQualifierDropEdit.Visible);
					AssertEquals("JE_LocationOtherInformationDropEdit.Visible", true, locationOtherInformationDropEdit.Visible);
				});

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("JE_LocationOfGoodsCodeFindBox.Location", ControlDpiScalingHelper.NewScaledPoint(95, 137), locationOfGoodsCodeFindBox.Location);
					AssertEquals("JE_LocationOfGoodsCodeFindBox.TabIndex", 13, locationOfGoodsCodeFindBox.TabIndex);

					AssertEquals("JE_LocationQualifierDropEdit.Location", ControlDpiScalingHelper.NewScaledPoint(217, 113), locationQualifierDropEdit.Location);
					AssertEquals("JE_LocationQualifierDropEdit.TabIndex", 12, locationQualifierDropEdit.TabIndex);

					AssertEquals("JE_LocationOtherInformationDropEdit.Location", ControlDpiScalingHelper.NewScaledPoint(95, 113), locationOtherInformationDropEdit.Location);
					AssertEquals("JE_LocationOtherInformationDropEdit.TabIndex", 11, locationOtherInformationDropEdit.TabIndex);

					AssertEquals("GoodsLocationDropEdit.Visible", false, goodsLocationDropEdit.Visible);
					AssertEquals("JE_SubLocationOfGoodsTextBox.Visible", false, subLocationOfGoodsTextBox.Visible);
					AssertEquals("JE_Calc_LocationOfGoodsCtryTextBox.Visible", false, locationOfGoodsCtryTextBox.Visible);
					AssertEquals("JE_LocationOfGoodsCodeFindBox.Visible", true, locationOfGoodsCodeFindBox.Visible);
					AssertEquals("JE_LocationQualifierDropEdit.Visible", true, locationQualifierDropEdit.Visible);
					AssertEquals("JE_LocationOtherInformationDropEdit.Visible", true, locationOtherInformationDropEdit.Visible);
				});

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("JE_LocationOfGoodsCodeFindBox.Location", ControlDpiScalingHelper.NewScaledPoint(119, 111), locationOfGoodsCodeFindBox.Location);
					AssertEquals("JE_LocationOfGoodsCodeFindBox.TabIndex", 9, locationOfGoodsCodeFindBox.TabIndex);

					AssertEquals("JE_LocationQualifierDropEdit.Location", ControlDpiScalingHelper.NewScaledPoint(217, 113), locationQualifierDropEdit.Location);
					AssertEquals("JE_LocationQualifierDropEdit.TabIndex", 12, locationQualifierDropEdit.TabIndex);

					AssertEquals("JE_LocationOtherInformationDropEdit.Location", ControlDpiScalingHelper.NewScaledPoint(95, 113), locationOtherInformationDropEdit.Location);
					AssertEquals("JE_LocationOtherInformationDropEdit.TabIndex", 11, locationOtherInformationDropEdit.TabIndex);

					AssertEquals("GoodsLocationDropEdit.Visible", false, goodsLocationDropEdit.Visible);
					AssertEquals("JE_SubLocationOfGoodsTextBox.Visible", false, subLocationOfGoodsTextBox.Visible);
					AssertEquals("JE_Calc_LocationOfGoodsCtryTextBox.Visible", false, locationOfGoodsCtryTextBox.Visible);
					AssertEquals("JE_LocationOfGoodsCodeFindBox.Visible", false, locationOfGoodsCodeFindBox.Visible);
					AssertEquals("JE_LocationQualifierDropEdit.Visible", false, locationQualifierDropEdit.Visible);
					AssertEquals("JE_LocationOtherInformationDropEdit.Visible", false, locationOtherInformationDropEdit.Visible);
				});
			}
		}

		public void TestJE_ExportDateBoundDateEdit2Length()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var exportDateBoundDateEdit = control.JE_ExportDateBoundDateEdit2;

				declaration.JE_MessageType = "EXP";

				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("JE_ExportDateBoundDateEdit2.Size", ZArchitecture.Core.ZDateTimePickerFormat.Long, exportDateBoundDateEdit.DateTimeFormat);
				});

				declaration.JE_MessageType = "IMP";

				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("JE_ExportDateBoundDateEdit2.Size", ZArchitecture.Core.ZDateTimePickerFormat.Short, exportDateBoundDateEdit.DateTimeFormat);
				});
			}
		}

		public void TestJE_DateOfArrivalBoundDateEdit2Length()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var dateOfArrivalBoundDateEdit2 = control.JE_DateOfArrivalBoundDateEdit2;

				declaration.JE_MessageType = "EXP";

				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("JE_DateOfArrivalBoundDateEdit2.Size", ZArchitecture.Core.ZDateTimePickerFormat.Short, dateOfArrivalBoundDateEdit2.DateTimeFormat);
				});

				declaration.JE_MessageType = "IMP";

				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("JE_DateOfArrivalBoundDateEdit2.Size", ZArchitecture.Core.ZDateTimePickerFormat.Long, dateOfArrivalBoundDateEdit2.DateTimeFormat);
				});
			}
		}

		public void TestSupportMultipleResourceStringData()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = "EXP";
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;

				ISupportMultipleResourceStringDataSupporter multipleResourceStringSupporter = control;
				AssertSequencesEqual("JE_LocationOtherInformationDropEdit.TabIndex", new[] { JobDeclaration.CaptionKeyExportUCC6 }, multipleResourceStringSupporter.SupportMultipleResourceStringData.MultipleKeysToUse);
			}
		}

		public void TestIncoTermLocation()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var incoTermDropEdit = control.IncoTermDropEdit;
				var incoTermExplainButton = control.IncoTermExplainButton;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("PresentationUserControl.IncoTermDropEdit", ControlDpiScalingHelper.NewScaledPoint(95, 184, true), incoTermDropEdit.Location);
					AssertEquals("PresentationUserControl.IncoTermDropEdit", 16, incoTermDropEdit.TabIndex);

					AssertEquals("PresentationUserControl.IncoTermExplainButton", ControlDpiScalingHelper.NewScaledPoint(231, 183, true), incoTermExplainButton.Location);
					AssertEquals("PresentationUserControl.IncoTermDropEdit", 17, incoTermExplainButton.TabIndex);
				});

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("PresentationUserControl.IncoTermDropEdit", ControlDpiScalingHelper.NewScaledPoint(95, 161, true), incoTermDropEdit.Location);
					AssertEquals("PresentationUserControl.IncoTermDropEdit", 13, incoTermDropEdit.TabIndex);

					AssertEquals("PresentationUserControl.IncoTermExplainButton", ControlDpiScalingHelper.NewScaledPoint(231, 161, true), incoTermExplainButton.Location);
					AssertEquals("PresentationUserControl.IncoTermDropEdit", 14, incoTermExplainButton.TabIndex);
				});

				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("PresentationUserControl.IncoTermDropEdit", ControlDpiScalingHelper.NewScaledPoint(298, 184, true), incoTermDropEdit.Location);
					AssertEquals("PresentationUserControl.IncoTermDropEdit", 17, incoTermDropEdit.TabIndex);

					AssertEquals("PresentationUserControl.IncoTermExplainButton", ControlDpiScalingHelper.NewScaledPoint(434, 183, true), incoTermExplainButton.Location);
					AssertEquals("PresentationUserControl.IncoTermDropEdit", 18, incoTermExplainButton.TabIndex);
				});
			}
		}

		public void TestShipmentIncoTermPlaceLocation()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var shipmentIncoTermPlaceTextBox = control.JE_ShipmentIncoTermPlaceTextBox;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("PresentationUserControl.JE_ShipmentIncoTermPlaceTextBox", ControlDpiScalingHelper.NewScaledPoint(346, 184, true), shipmentIncoTermPlaceTextBox.Location);
					AssertEquals("PresentationUserControl.JE_ShipmentIncoTermPlaceTextBox", 18, shipmentIncoTermPlaceTextBox.TabIndex);
				});

				declaration.JE_MessageType = "IMP";
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					AssertEquals("PresentationUserControl.JE_ShipmentIncoTermPlaceTextBox", ControlDpiScalingHelper.NewScaledPoint(95, 137, true), shipmentIncoTermPlaceTextBox.Location);
					AssertEquals("PresentationUserControl.JE_ShipmentIncoTermPlaceTextBox", 10, shipmentIncoTermPlaceTextBox.TabIndex);
				});
			}
		}

		public void TestWeightzCalcDropEditLocation()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = "EXP";
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					var weightzCalcDropEdit = control.WeightzCalcDropEdit;
					AssertEquals("PresentationUserControl.WeightzCalcDropEdit", ControlDpiScalingHelper.NewScaledPoint(346, 207, true), weightzCalcDropEdit.Location);
					AssertEquals("PresentationUserControl.WeightzCalcDropEdit", 20, weightzCalcDropEdit.TabIndex);
				});

				declaration.JE_MessageType = "IMP";
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					var weightzCalcDropEdit = control.WeightzCalcDropEdit;
					AssertEquals("PresentationUserControl.WeightzCalcDropEdit", ControlDpiScalingHelper.NewScaledPoint(95, 207, true), weightzCalcDropEdit.Location);
					AssertEquals("PresentationUserControl.WeightzCalcDropEdit", 19, weightzCalcDropEdit.TabIndex);
				});
			}
		}

		public void TestGoodsDescriptionTextBoxLocation()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = "EXP";
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					var weightzCalcDropEdit = control.GoodsDescriptionTextBox;
					AssertEquals("PresentationUserControl.GoodsDescriptionTextBox", ControlDpiScalingHelper.NewScaledPoint(95, 88, true), weightzCalcDropEdit.Location);
					AssertEquals("PresentationUserControl.GoodsDescriptionTextBox", 11, weightzCalcDropEdit.TabIndex);
				});

				declaration.JE_MessageType = "IMP";
				CombineAssertions($"Message type {declaration.JE_MessageType}", () =>
				{
					var weightzCalcDropEdit = control.GoodsDescriptionTextBox;
					AssertEquals("PresentationUserControl.GoodsDescriptionTextBox", ControlDpiScalingHelper.NewScaledPoint(95, 113, true), weightzCalcDropEdit.Location);
					AssertEquals("PresentationUserControl.GoodsDescriptionTextBox", 9, weightzCalcDropEdit.TabIndex);
				});
			}
		}

		public void TestZG_AgreedPlaceCodeFindBoxVisible()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var agreedPlaceCodeDropEdit = control.ZG_AgreedPlaceCodeDropEdit;
				var agreedPlaceCodeFindBox = control.ZG_AgreedPlaceCodeFindBox;

				declaration.JE_MessageType = "EXP";
				AssertEquals("AgreedPlaceCodeDropEdit.Visibility", false, agreedPlaceCodeDropEdit.Visible);
				AssertEquals("AgreedPlaceCodeFindBox.Visibility", false, agreedPlaceCodeFindBox.Visible);

				declaration.JE_MessageType = "IMP";
				declaration.JE_ShipmentIncoTermPlace = "Dublin";
				AssertEquals("AgreedPlaceCodeDropEdit.Visibility", true, agreedPlaceCodeDropEdit.Visible);
				AssertEquals("AgreedPlaceCodeFindBox.Visibility", false, agreedPlaceCodeFindBox.Visible);

				declaration.JE_ShipmentIncoTermPlace = "";
				AssertEquals("AgreedPlaceCodeFindBox.Visibility", true, agreedPlaceCodeFindBox.Visible);
				AssertEquals("AgreedPlaceCodeDropEdit.Visibility", false, agreedPlaceCodeDropEdit.Visible);
			}
		}

		public void TestJE_UCRTextBoxVisible()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var ucrTextBox = control.JE_UCRTextBox;
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("Import", true, ucrTextBox.Visible);

				declaration.JE_MessageTypeInfo.RefreshBinding();
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("Import", true, ucrTextBox.Visible);

				declaration.JE_MessageTypeInfo.RefreshBinding();
				declaration.JE_ApplicationCode = "#@#";
				AssertEquals("Import", true, ucrTextBox.Visible);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("Export", false, ucrTextBox.Visible);
			}
		}

		public void TestZG_RegionOfDestinationVisible()
		{
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var regionOfDestinationDropEdit = control.RegionOfDestinationDropEdit;

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("Import", true, regionOfDestinationDropEdit.Visible);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("Export", false, regionOfDestinationDropEdit.Visible);
			}
		}

		public void TestTabPagesOrder()
		{
			using (var form = new ZForm())
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				form.Show();
				var tabPages = control.RightTabControl.TabPages.Cast<ZTabPage>().ToArray();

				CombineAssertions(() =>
				{
					AssertEquals("OrganisationsTabPage should be the first tab page.", "OrganisationsTabPage", tabPages[0].Name);
					AssertEquals("DocsTabPage should be the second tab page.", "DocsTabPage", tabPages[1].Name);
					AssertEquals("LineChargesTabPage should be the third tab page.", "OrdersTabPage", tabPages[2].Name);
					AssertEquals("LineDetailsTabPage should be the fourth tab page.", "ShipmentCustomFieldsPage", tabPages[3].Name);
					AssertEquals("ContainersTabPage should be the fifth tab page.", "NumbersTabPage", tabPages[4].Name);

					AssertEquals("OrganisationsTabPage should be the selected tab page by default.", "OrganisationsTabPage", control.RightTabControl.SelectedTab.Name);
				});
			}
		}

		void AssertControlCaption(JobDeclarationUserControl rootControl, string testControlName, string expectedCaption, string expectedFullDescription = null, string expectedShortCaption = null, string parentControlName = null)
		{
			var testControl = rootControl.FindSingleOrDefault<Control>(c => c.Name.Equals(testControlName) && c.Visible && (parentControlName == null || c.Parent.Name == parentControlName));

			if (testControl != null)
			{
				AssertControlCaption(testControl, expectedCaption, expectedFullDescription, expectedShortCaption);
			}
			else
			{
				Fail($"Could not find a visible control with name: {testControlName}");
			}
		}

		void AssertControlCaption(Control testControl, string expectedCaption, string expectedFullDescription = null, string expectedShortCaption = null)
		{
			AssertEquals(testControl.Name + ".Caption", expectedCaption, testControl.GetExtension<IHintExtension>()?.Caption);
			if (expectedFullDescription != null)
			{
				AssertEquals(testControl.Name + ".FullDescription", expectedFullDescription, testControl.GetExtension<IHintExtension>()?.Description);
			}
			if (expectedShortCaption != null)
			{
				AssertEquals(testControl.Name + ".ShortCaption", expectedShortCaption, testControl.GetExtension<IHintExtension>()?.ShortCaption);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
