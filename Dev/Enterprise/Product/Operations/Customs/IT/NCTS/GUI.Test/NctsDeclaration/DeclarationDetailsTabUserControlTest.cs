using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class DeclarationDetailsTabUserControlTest : TestCaseWithFactory
{
	public void TestMessageStatusDropEdit()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			control.AssertContainsControl<ZDropEdit>("MessageStatusDropEdit", x => x.WithBindTo(nameof(NctsHeader.EffectiveMessageStatus)));
		}
	}

	public void TestGoodsDescriptionDropEdit()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var dropEdit = control.FindSingleOrDefault<ZDropEdit>("GoodsDescriptionDropEdit");
			AssertNotNull("Drop edit field not found", dropEdit);
		}
	}

	public void TestGoodsDescriptionDropEditForSimplifiedProcedure()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var dropEdit = control.FindSingleOrDefault<ZDropEdit>("GoodsDescriptionDropEditForSimplifiedProcedure");
			AssertNotNull("Drop edit field not found", dropEdit);
		}
	}

	public void TestAuthorisedLocationCodeTextBox()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var textBox = control.FindSingleOrDefault<ZTextBox>("AuthorisedLocationCodeTextBox");
			AssertNotNull("Text box field not found", textBox);
			Assert("Text field must be hidden", !textBox.Visible);
		}
	}

	public void TestAgreedLocationOfGoodsNormalTextField()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var field = control.FindSingleOrDefault<ZTextBox>("AgreedLocationOfGoodsCodeNormalTextBox");
			AssertNotNull("Text field not found", field);
			Assert("Text field not hidden", !field.Visible);
		}
	}

	[RequiresSTA]
	public void TestControlResultDateLimitDateEditIsAlwaysVisible()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		header.MovementHeader.IsSimplifiedNctsProcedure = false;
		AssertControlResultDateLimitDateEditVisibility("Assert ControlResultDateLimitDateEdit when is not a Simplified Ncts Procedure", isSimplifiedNctsProcedure: false);

		header.MovementHeader.IsSimplifiedNctsProcedure = true;
		AssertControlResultDateLimitDateEditVisibility("Assert ControlResultDateLimitDateEdit when is a Simplified Ncts Procedure", isSimplifiedNctsProcedure: true);

		void AssertControlResultDateLimitDateEditVisibility(string assertionMessage, bool isSimplifiedNctsProcedure)
		{
			using (var form = new ZForm(header))
			using (var control = new DeclarationDetailsTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(assertionMessage, () =>
				{
					var controlResultDateLimitDateEdit = control.FindSingle<ZDateEdit>("ControlResultDateLimitDateEdit");
					var controlResultDateLimitForNormalDeclarationDateEdit = control.FindSingle<ZDateEdit>("ControlResultDateLimitForNormalDeclarationDateEdit");

					AssertNotNull("ControlResultDateLimitDateEdit", controlResultDateLimitDateEdit);
					AssertNotNull("ControlResultDateLimitForNormalDeclarationDateEdit", controlResultDateLimitForNormalDeclarationDateEdit);

					AssertEquals("ControlResultDateLimitDateEdit is visible", isSimplifiedNctsProcedure, controlResultDateLimitDateEdit.Visible);
					AssertEquals("ControlResultDateLimitDateEdit is visible", !isSimplifiedNctsProcedure, controlResultDateLimitForNormalDeclarationDateEdit.Visible);
				});
			}
		}
	}

	public void TestPreLodgedForAgreedLocationOfGoodsCodeTickBoxVisibility()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var preLodgedForAgreedLocationOfGoodsCodeTickBox = control.FindSingleOrDefault<ZCheckBox>("PreLodgedForAgreedLocationOfGoodsCodeTickBox");
			AssertNotNull("PreLodgedForAgreedLocationOfGoodsCodeTickBox", preLodgedForAgreedLocationOfGoodsCodeTickBox);
			AssertEquals("PreLodgedForAgreedLocationOfGoodsCodeTickBox.Visible", false, preLodgedForAgreedLocationOfGoodsCodeTickBox.Visible);
		}
	}

	public void TestPlaceOfLoadingNormalCodeFindBoxDescriptionBoxVisibility()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var placeOfLoadingFindBox = control.FindSingleOrDefault<ZCodeFindBox>("PlaceOfLoadingNormalCodeFindBox");
			AssertEquals("The description box should be hidden", false, placeOfLoadingFindBox.DescriptionBox.Visible);
		}
	}

	[RequiresSTA]
	public void TestPlaceOfLoadingNormalCodeFindBox()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var placeOfLoadingFindBox = control.FindSingleOrDefault<ZCodeFindBox>("PlaceOfLoadingNormalCodeFindBox");
			AssertNotNull("Find box field not found", placeOfLoadingFindBox);
			AssertEquals("Find box field has to have the pre bound max length of 17", 17, placeOfLoadingFindBox.PreBoundMaxLength);
		}
	}

	public void TestPlaceOfLoadingSimplifiedCodeFindBoxDescriptionBoxVisibility()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var placeOfLoadingFindBox = control.FindSingleOrDefault<ZCodeFindBox>("PlaceOfLoadingSimplifiedCodeFindBox");
			AssertEquals("The description box should be hidden", false, placeOfLoadingFindBox.DescriptionBox.Visible);
		}
	}

	public void TestPlaceOfLoadingSimplifiedCodeFindBox()
	{
		using (var control = new DeclarationDetailsTabUserControl())
		{
			var placeOfLoadingFindBox = control.FindSingleOrDefault<ZCodeFindBox>("PlaceOfLoadingSimplifiedCodeFindBox");
			AssertNotNull("Find box field not found", placeOfLoadingFindBox);
			AssertEquals("Find box field has to have the pre bound max length of 17", 17, placeOfLoadingFindBox.PreBoundMaxLength);
		}
	}
}
