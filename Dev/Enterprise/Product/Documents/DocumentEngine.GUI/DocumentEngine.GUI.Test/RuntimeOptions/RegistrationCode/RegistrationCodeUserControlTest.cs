using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(RegistrationCodeUserControl))]
	sealed class RegistrationCodeUserControlTest : RuntimeOptionUserControlBaseTest<RegistrationCodeUserControl>
	{
		public override void TestChangeLabelSizeForAlignment()
		{
			using (var control = new RegistrationCodeUserControl())
			{
				control.Width = 500;
				control.ChangeLabelSizeForAlignment(200);

				control.Controls.Cast<Control>().ForEach(c => Assert("Not the expected distance", c.Left == 200));
				control.Controls.Cast<Control>().ForEach(c => Assert("Not the expected width", c.Width == 300));
			}
		}

		public override void TestDesiredCaptionWidth()
		{
			using (var control = new RegistrationCodeUserControl())
			{
				var testText = "Some text that determines desired captionwidth";
				control.Controls.OfType<ZCodeFindBox>().Single().CaptionResourceString = Res.GetData("d660ffa5-b349-442d-8090-ec635555fea8", testText);
				AssertEquals("Does not match the expected desired caption width", TextRenderer.MeasureText(testText, control.Controls[0].GetExtension<ILabelCaptionRenderer>().Font).Width, control.DesiredCaptionWidth);
			}
		}

		public void TestSetFilter()
		{
			using (var userControl = new RegistrationCodeUserControlForTest())
			{
				var filter = new RegistrationCodeField(Factory);
				userControl.SetFilter(filter);
				AssertEquals("CountryFindBox.BindToList", "Countries", userControl.CountryFindBox_Exposed.BindToList);
				AssertEquals("CountryFindBox.ModuleID", ModuleIDs.RefCountry, userControl.CountryFindBox_Exposed.ModuleID);
				AssertEquals("CustomTypeDropEdit.BindToList", "CustomTypes", userControl.CustomTypeDropEdit_Exposed.BindToList);
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestBindingWithCountriesAndCustomCodes()
		{
			using (var form = new ZForm())
			using (var userControl = new RegistrationCodeUserControlForTest())
			{
				var filter = new RegistrationCodeField(Factory);
				userControl.SetFilter(filter);
				form.Controls.Add(userControl);
				form.Show();

				var countries = userControl.CountryFindBox_Exposed.List as RefCountryCollection;
				AssertNotNull("CountryFindBox's list should be a RefCountryCollection", countries);
				Assert("CountryFindBox's list should not be empty", countries.Count > 0);

				var customTypes = userControl.CustomTypeDropEdit_Exposed.List as CodeDescriptionPairList;
				AssertNotNull("CustomTypeDropEdit's list should be a CodeDescriptionPairList", customTypes);
				Assert("CustomTypeDropEdit's list should not be empty", customTypes.Count > 0);
				Assert(!customTypes.ContainsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber));

				userControl.CountryFindBox_Exposed.CodeBox.Text = Core.Constants.CountryCodes.Australia;
				userControl.CustomTypeDropEdit_Exposed.Focus();
				customTypes = userControl.CustomTypeDropEdit_Exposed.List as CodeDescriptionPairList;
				Assert(customTypes.ContainsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber));
			}
		}

		public void TestExpectedFilterType()
		{
			using (var control = new RegistrationCodeUserControl())
			{
				AssertEquals("Expected filter type should be a RegistrationCodeField", typeof(RegistrationCodeField), control.ExpectedFilterType());
			}
		}

		class RegistrationCodeUserControlForTest : RegistrationCodeUserControl
		{
			public ZCodeFindBox CountryFindBox_Exposed
			{
				get { return base.CountryFindBox; }
			}

			public ZDropEdit CustomTypeDropEdit_Exposed
			{
				get { return base.CustomTypeDropEdit; }
			}
		}
	}
}
