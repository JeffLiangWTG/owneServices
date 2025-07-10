using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PlaceOfUseOrProcessingColumnStyleTest : TestCaseWithFactory
	{
		public void TestEditControl()
		{
			using (var style = new PlaceOfUseOrProcessingColumnStyle(new PlaceOfUseOrProcessingColumnStyleInfo()))
			{
				AssertType<PlaceOfUseOrProcessingGridFindBox>(style.EditControl);
			}
		}

		public void TestEditControlShownForReadOnly()
		{
			var propertyInfo = typeof(ZCustomControlColumnStyle).GetProperty("EditControlShownForReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			using (var style = new PlaceOfUseOrProcessingColumnStyle(new PlaceOfUseOrProcessingColumnStyleInfo()))
			{
				AssertEquals(true, (bool)propertyInfo.GetValue(style));
			}
		}

		public void TestCommitEditingRow() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(instruction))
			{
				var grid = new ZGrid();
				grid.BindTo = nameof(CusEntryInstruction.PlaceOfUseOrProcessingCollection);
				var columnInfo = new PlaceOfUseOrProcessingColumnStyleInfo { ColumnName = nameof(PlaceOfUseOrProcessing.DisplayText) };
				grid.ColumnStyles.Add(columnInfo);
				form.Controls.Add(grid);
				form.Show();
				var columnStyle = (PlaceOfUseOrProcessingColumnStyle)grid.GetCurrentColumnStyle();

				var placeOfUseOrProcessing = instruction.PlaceOfUseOrProcessingCollection[0];
				AssertEquals("An uncommitted row", true, ((IBusinessObjectInternals)placeOfUseOrProcessing).IsUnCommittedRow);
				var findBox = (PlaceOfUseOrProcessingGridFindBox)columnStyle.EditControl;
				AssertNotNull("FindBox has PlaceOfUseOrProcessing set", findBox.PlaceOfUseOrProcessing);
				var propertyInfo = typeof(PlaceOfUseOrProcessingGridFindBox).GetProperty("PopupForm", BindingFlags.NonPublic | BindingFlags.Instance);
				var popupForm = (PlaceOfUseOrProcessingForm)propertyInfo.GetValue(findBox);
				using (popupForm)
				{
					placeOfUseOrProcessing.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
					placeOfUseOrProcessing.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					placeOfUseOrProcessing.CGL_AdditionalIdentifier = "123";
					var methodInfo = typeof(PlaceOfUseOrProcessingGridFindBox).GetMethod("ClosePopupForm", BindingFlags.Instance | BindingFlags.NonPublic);
					methodInfo.Invoke(findBox, null);
				}
				AssertEquals("committed", false, ((IBusinessObjectInternals)placeOfUseOrProcessing).IsUnCommittedRow);
			}
		});
	}
}
