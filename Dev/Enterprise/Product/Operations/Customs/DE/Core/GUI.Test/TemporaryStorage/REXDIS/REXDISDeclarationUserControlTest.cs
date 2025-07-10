using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class REXDISDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestLineUserControl_AWB()
		{
			AssertLineUserControl(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
		}

		public void TestLineUserControl_REG()
		{
			AssertLineUserControl(TemporaryStorageIdentificationIndicatorList.Codes.REG);
		}

		public void TestLineUserControl_SIN()
		{
			AssertLineUserControl(TemporaryStorageIdentificationIndicatorList.Codes.SIN);
		}

		public void TestLinesGrid_ColumnVisibility_AWB()
		{
			AssertLinesGridColumnVisibility(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
		}

		public void TestLinesGrid_ColumnVisibility_REG()
		{
			AssertLinesGridColumnVisibility(TemporaryStorageIdentificationIndicatorList.Codes.REG);
		}

		public void TestLinesGrid_ColumnVisibility_SIN()
		{
			AssertLinesGridColumnVisibility(TemporaryStorageIdentificationIndicatorList.Codes.SIN);
		}

		public void TestLinesGrid_ColumnSizes()
		{
			using (var control = new REXDISDeclarationUserControl())
			{
				var grid = control.FindSingle<ZGrid>("LinesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("TSL_LineNo", 65, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_LineNo).Width);
					AssertEquals("SumALine + TSL_ReferenceNumberLine", 130, grid.GetColumnStyle("SumALine+TSL_ReferenceNumberLine").Width);
					AssertEquals("SumALine+ReferenceNumber", 187, grid.GetColumnStyle("SumALine+ReferenceNumber").Width);
					AssertEquals("TSL_OwnerReferenceType", 159, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OwnerReferenceType).Width);
					AssertEquals("TSL_OwnerReferenceNumber", 174, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OwnerReferenceNumber).Width);
					AssertEquals("TSL_PackageQty", 111, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_PackageQty).Width);
					AssertEquals("TSL_DestinationPlace", 106, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_DestinationPlace).Width);
					AssertEquals("TSL_CustomsStatus", 96, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_CustomsStatus).Width);
				});
			}
		}

		public void TestLinesGrid_CharacterCasing()
		{
			using (var control = new REXDISDeclarationUserControl())
			{
				var grid = control.FindSingle<ZGrid>("LinesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("SumALine+ReferenceNumber", CharacterCasing.Upper, grid.GetColumnStyle("SumALine+ReferenceNumber").CharacterCasing);
					AssertEquals("TSL_OwnerReferenceType", CharacterCasing.Upper, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OwnerReferenceType).CharacterCasing);
					AssertEquals("TSL_OwnerReferenceNumber", CharacterCasing.Normal, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OwnerReferenceNumber).CharacterCasing);
					AssertEquals("TSL_DestinationPlace", CharacterCasing.Normal, grid.GetColumnStyle(CusTempStorageLine.Schema.TSL_DestinationPlace).CharacterCasing);
				});
			}
		}

		public void TestReferenceNumberColumnCaption()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var storageDec = REXDISCusTempStorageDec.LoadOrCreate(header);
			using (var form = new ZForm(header))
			using (var declarationUserControl = new REXDISDeclarationUserControl())
			{
				form.Controls.Add(declarationUserControl);
				form.SetDataBinding(header, "REXDISCusTempStorageDec");
				form.Show();
				var grid = declarationUserControl.FindSingle<ZGrid>("LinesGrid");

				AssertEquals("Reference", grid.GetColumnCaption("SumALine+ReferenceNumber"));
			}
		}

		void AssertLineUserControl(ZString identificationIndicator)
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var storageDec = REXDISCusTempStorageDec.LoadOrCreate(header);
			using (var form = new ZForm(header))
			using (var declarationUserControl = new REXDISDeclarationUserControl())
			{
				form.Controls.Add(declarationUserControl);
				form.SetDataBinding(header, "REXDISCusTempStorageDec");
				form.Show();
				var controlAWB_SIN = declarationUserControl.FindSingle<REXDISAWBDeclarationUserControl>("REXDISAWBDeclarationUserControl");
				var controlREG = declarationUserControl.FindSingle<REXDISREGDeclarationUserControl>("REXDISREGDeclarationUserControl");
				CombineAssertions(() =>
				{
					storageDec.STH_IdentificationIndicator = identificationIndicator;
					var isAWB_SIN = storageDec.IsAWBDeclaration || storageDec.IsSINDeclaration;
					AssertEquals("AWB Control: visibility", isAWB_SIN, controlAWB_SIN.Visible);
					AssertEquals("REG Control: visibility", !isAWB_SIN, controlREG.Visible);
				});
			}
		}

		void AssertLinesGridColumnVisibility(ZString identificationIndicator)
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var storageDec = REXDISCusTempStorageDec.LoadOrCreate(header);
			using (var form = new ZForm(header))
			using (var declarationUserControl = new REXDISDeclarationUserControl())
			{
				form.Controls.Add(declarationUserControl);
				form.SetDataBinding(header, "REXDISCusTempStorageDec");
				form.Show();
				var grid = declarationUserControl.FindSingle<ZGrid>("LinesGrid");
				var sumALineNumber = grid.GetColumnStyle("SumALine+TSL_ReferenceNumberLine");
				var sumATNumber = grid.GetColumnStyle("SumALine+ReferenceNumber");

				CombineAssertions(() =>
				{
					storageDec.STH_IdentificationIndicator = identificationIndicator;
					var isAWB_SIN = storageDec.IsAWBDeclaration || storageDec.IsSINDeclaration;
					AssertEquals("SumA Line Number: unavailable", isAWB_SIN, sumALineNumber.IsUnavailable);
					AssertEquals("SumA ATB Number: unavailable", isAWB_SIN, sumATNumber.IsUnavailable);
				});
			}
		}
	}
}
