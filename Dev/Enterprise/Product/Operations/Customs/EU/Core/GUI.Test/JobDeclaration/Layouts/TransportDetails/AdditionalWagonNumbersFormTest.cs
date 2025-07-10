using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(AdditionalWagonNumbersForm))]
	sealed class AdditionalWagonNumbersFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestCloseButton()
		{
			CombineAssertions(() =>
			{
				var collection = PrepareDataForTest();
				var item = collection.AddNew();
				item.Nationality = "DE";
				item.CY_Data = "DE001";
				using (var form = new AdditionalWagonNumbersForm(collection))
				{
					form.Show();
					AssertEquals("Caption", "Cancel", form.CloseButton.CaptionResourceString.Caption);
					AssertEquals("DialogResult", DialogResult.Cancel, form.CloseButton.DialogResult);

					var item2 = collection.AddNew();
					item2.Nationality = "ES";
					item2.CY_Data = "ES003";
					collection.RemoveAndDelete(item);
					form.CloseButton.PerformClick();
					AssertEquals("Form is closed after clicking button", false, form.Visible);
					AssertEquals("No change", "DE", collection.Cast<InlandTransport>().Single().Nationality);
				}
			});
		}

		public void TestCloseButton_NoChanges()
		{
			var collection = PrepareDataForTest();
			var item = collection.AddNew();
			item.Nationality = "DE";
			item.CY_Data = "DE001";
			Factory.Save();

			using (var form = new AdditionalWagonNumbersForm(collection))
			{
				form.Show();
				form.CloseButton.PerformClick();
				AssertEquals(false, collection.HasChanges);
			}
		}

		public void TestOKButton()
		{
			CombineAssertions(() =>
			{
				var collection = PrepareDataForTest();
				var item = collection.AddNew();
				item.Nationality = "DE";
				item.CY_Data = "DE001";
				using (var form = new AdditionalWagonNumbersForm(collection))
				{
					form.Show();
					AssertEquals("Caption", "OK", form.OKButton.CaptionResourceString.Caption);
					AssertEquals("DialogResult", DialogResult.OK, form.OKButton.DialogResult);

					var item2 = collection.AddNew();
					item2.Nationality = "ES";
					item2.CY_Data = "ES003";
					collection.RemoveAndDelete(item);
					form.OKButton.PerformClick();
					AssertEquals("Form is closed after clicking button", false, form.Visible);
					AssertEquals("Was changed", "ES", collection.Cast<InlandTransport>().Single().Nationality);
				}
			});
		}

		[RequiresSTA]
		public void TestCloseForm()
		{
			CombineAssertions(() =>
			{
				var collection = PrepareDataForTest();
				var item = collection.AddNew();
				item.Nationality = "DE";
				item.CY_Data = "DE001";
				using (var form = new AdditionalWagonNumbersForm(collection))
				{
					form.Show();
					var item2 = collection.AddNew();
					item2.Nationality = "ES";
					item2.CY_Data = "ES003";
					collection.RemoveAndDelete(item);
					form.Close();
					AssertEquals("Form is closed with X button", false, form.Visible);
					AssertEquals("No change", "DE", collection.Cast<InlandTransport>().Single().Nationality);
				}
			});
		}

		public void TestCaption()
		{
			using (var form = GetFormToBashCore())
			{
				AssertEquals("Additional Wagon Numbers", ((AdditionalWagonNumbersForm)form).CaptionResourceString.Caption);
			}
		}

		public void TestAdditionalWagonNumbersGridColumnStyleInfos()
		{
			var collection = PrepareDataForTest();
			using (var form = new AdditionalWagonNumbersForm(collection))
			{
				var wagonNumberColumnInfo = form.AdditionalWagonNumbersGrid.GetColumnStyle(Customs.Business.AutoCusCodeData.Schema.CY_Data);
				AssertType<ZTextBoxColumnStyleInfo>("CY_Data column type", wagonNumberColumnInfo);
				AssertEquals("CY_Data column CharacterCasing", CharacterCasing.Upper, wagonNumberColumnInfo.CharacterCasing);

				var nationalityColumnInfo = form.AdditionalWagonNumbersGrid.GetColumnStyle("Nationality");
				AssertType<ZCodeFindBoxColumnStyleInfo>("CY_Code column type", nationalityColumnInfo);
			}
		}

		public void TestOKButton_NoErrorExists()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var collection = PrepareDataForTest();
			using (var form = new AdditionalWagonNumbersForm(collection))
			{
				form.Show();
				var item = collection.AddNew();
				item.Nationality = "DE";
				item.CY_Data = "DE001";
				form.OKButton.PerformClick();
				AssertEquals("Form is closed after clicking button", false, form.Visible);
			}
		}

		public void TestOKButton_MessageErrorExists()
		{
			var collection = PrepareDataForTest();
			using (var form = new AdditionalWagonNumbersForm(collection))
			{
				form.Show();
				var item = collection.AddNew();
				item.Nationality = "";
				item.CY_Data = "DE001";
				form.OKButton.PerformClick();
				AssertEquals("Form is closed after clicking button", false, form.Visible);
			}
		}

		public void TestOKButton_ErrorExists()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var collection = PrepareDataForTest();
			using (var form = new AdditionalWagonNumbersForm(collection))
			{
				form.Show();
				var item = collection.AddNew();
				item.Nationality = "DE";
				item.CY_Data = "敬礼";
				form.OKButton.PerformClick();
				AssertEquals("Pop-up window shows", "The form has errors. Please fix them before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form isn't closed after clicking button", true, form.Visible);

				form.Close();
				AssertEquals("Collection was reverted", 0, collection.Count);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var collection = PrepareDataForTest();
			return new AdditionalWagonNumbersForm(collection);
		}

		InlandTransportCollection PrepareDataForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			return new InlandTransportCollection(declaration);
		}
	}
}
