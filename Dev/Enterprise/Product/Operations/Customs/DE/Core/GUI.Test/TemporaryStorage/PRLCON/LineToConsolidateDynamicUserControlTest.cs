using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class LineToConsolidateDynamicUserControlTest : TestCaseWithFactory
	{
		public void TestLineToConsolidateDynamicUserControlWithIdentificationTypeREG()
		{
			AssertEqualsCorrectUserControlVisible(TemporaryStorageIdentificationIndicatorList.Codes.REG, false, true);
		}

		public void TestLineToConsolidateDynamicUserControlWithIdentificationTypeAWB()
		{
			AssertEqualsCorrectUserControlVisible(TemporaryStorageIdentificationIndicatorList.Codes.AWB, true, false);
		}

		public void TestLinesToConsolidateGridImportFromSumARegister()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			var cusTempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader.SRH_Reference = "TEST1234";

			Factory.Save();

			var cusTempStorageRegLine = cusTempStorageRegHeader.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine.SRL_PackagesRemaining = 678;
			cusTempStorageRegLine.SRL_LineNumber = 52;

			Factory.Save();

			var lineToConsolidate = storageDec.CusTempStorageLines.AddNew();

			using (var form = new ZForm())
			using (var control = new LineToConsolidateDynamicUserControl())
			{
				control.SetDataBinding(storageDec, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				var regLineToConsolidateControl = control.REGLineToConsolidateUserControl;
				var linesGrid = regLineToConsolidateControl.LinesGrid;

				var styleInfo = (ZCodeFindBoxWithSelectedEventColumnStyleInfo)linesGrid.GetColumnStyle(nameof(PRLCONCusTempStorageLineToConsolidate.FormattedReferenceNumber));

				using (var col = new ZCodeFindBoxWithSelectedEventColumnStyleForTest(styleInfo))
				{
					col.SetParentGrid(linesGrid);
					form.Show();
					linesGrid.Focus();

					var findBox = col.FindBoxForTesting;

					var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
					col.CallEnterEditControlForTesting();
					popupDecisionProvider.HandleFindBoxOKButton(new[] { cusTempStorageRegLine });

					CombineAssertions(() =>
					{
						AssertEquals(cusTempStorageRegLine.HeaderRegNo, findBox.Code);
						AssertEquals(cusTempStorageRegLine.SRL_PackagesRemaining, lineToConsolidate.TSL_PackageQty);
						AssertEquals(cusTempStorageRegLine.SRL_LineNumber, lineToConsolidate.TSL_ReferenceNumberLine);
					});
				}
			}
		}

		public void TestLinesToConsolidateFindBoxImportFromSumARegister()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			var cusTempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader.SRH_Reference = "TEST1234";

			Factory.Save();

			var cusTempStorageRegLine = cusTempStorageRegHeader.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine.SRL_PackagesRemaining = 678;
			cusTempStorageRegLine.SRL_LineNumber = 52;

			Factory.Save();

			var lineToConsolidate = storageDec.CusTempStorageLines.AddNew();

			using (var form = new ZForm())
			using (var control = new LineToConsolidateDynamicUserControl())
			{
				control.SetDataBinding(storageDec, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				var regLineToConsolidateControl = control.REGLineToConsolidateUserControl;
				var findBox = regLineToConsolidateControl.AtbNumberZCodeFindBox;

				findBox.Select();

				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { cusTempStorageRegLine });

				CombineAssertions(() =>
				{
					AssertEquals("{None Selected}", findBox.DescriptionBox.Text);
					AssertEquals(cusTempStorageRegLine.SRL_PackagesRemaining, lineToConsolidate.TSL_PackageQty);
					AssertEquals(cusTempStorageRegLine.SRL_LineNumber, lineToConsolidate.TSL_ReferenceNumberLine);
				});
			}
		}

		public void TestAtbNumberZCodeFindBox()
		{
			using (var control = new LineToConsolidateDynamicUserControl())
			{
				var regLineToConsolidateControl = control.REGLineToConsolidateUserControl;

				var findBox = regLineToConsolidateControl.AtbNumberZCodeFindBox;
				CombineAssertions(() =>
				{
					AssertEquals("BindToList", nameof(PRLCONCusTempStorageDec.Lookups) + "." + nameof(PRLCONCusTempStorageDecLookups.CusTempStorageRegLineCollection), findBox.BindToList);
					AssertEquals("ModuleID", ModuleIDs.Customs.EU.DE.ImportFromSumARegister, findBox.ModuleID);
					AssertEquals("ShowDescriptionBox", false, findBox.ShowDescriptionBox);
				});
			}
		}

		void AssertEqualsCorrectUserControlVisible(ZString identificationIndicator, ZBool isAWBUserControlVisible, ZBool isREGUserControlVisible)
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.PRLCONCusTempStorageDecs.AddNew();
			storageDec.STH_IdentificationIndicator = identificationIndicator;
			var lineToConsolidated = storageDec.CusTempStorageLines.AddNew();
			using (var form = new ZForm())
			using (var control = new LineToConsolidateDynamicUserControl())
			{
				control.SetDataBinding(storageDec, ZString.Empty);
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("AWB User Control", isAWBUserControlVisible, control.AWBLineToConsolidateUserControl.Visible);
					AssertEquals("REG User Control", isREGUserControlVisible, control.REGLineToConsolidateUserControl.Visible);
				});
			}
		}

		sealed class ZCodeFindBoxWithSelectedEventColumnStyleForTest : ZCodeFindBoxWithSelectedEventColumnStyle
		{
			public ZCodeFindBoxWithSelectedEventColumnStyleForTest(ZCodeFindBoxWithSelectedEventColumnStyleInfo columnInfo)
				: base(columnInfo)
			{
			}
			public void CallEnterEditControlForTesting()
			{
				EnterEditControl(this, EventArgs.Empty);
			}

			public IFindBox FindBoxForTesting
			{
				get
				{
					return FindBox;
				}
			}
		}
	}
}
