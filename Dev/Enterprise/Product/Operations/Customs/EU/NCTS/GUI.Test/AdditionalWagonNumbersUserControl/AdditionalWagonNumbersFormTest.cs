using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(AdditionalWagonNumbersForm))]
	sealed class AdditionalWagonNumbersFormTest : ZFormBasherTest
	{
		public void TestCloseButton()
		{
			CombineAssertions(() =>
			{
				var header = PrepareDataForTest();
				var collection = header.MovementHeader.InlandTransportList;
				var item = collection.AddNew("DE", "DE001");
				using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
				{
					form.Show();
					AssertEquals("Caption", "Cancel", form.CloseButton.CaptionResourceString.Caption);
					AssertEquals("DialogResult", DialogResult.Cancel, form.CloseButton.DialogResult);

					collection.AddNew("ES", "ES003");
					collection.RemoveAndDelete(item);
					form.CloseButton.PerformClick();
					AssertEquals("Form is closed after clicking button", false, form.Visible);
					AssertEquals("No change", "DE", collection.Cast<InlandTransport>().Single().CY_Code);
				}
			});
		}

		public void TestCloseButton_NoChanges()
		{
			var header = PrepareDataForTest();
			var collection = header.MovementHeader.InlandTransportList;
			collection.AddNew("DE", "DE001");
			Factory.Save();

			using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
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
				var header = PrepareDataForTest();
				var collection = header.MovementHeader.InlandTransportList;
				var item = collection.AddNew("DE", "DE001");
				using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
				{
					form.Show();
					AssertEquals("Caption", "OK", form.OKButton.CaptionResourceString.Caption);
					AssertEquals("DialogResult", DialogResult.OK, form.OKButton.DialogResult);

					collection.AddNew("ES", "ES003");
					collection.RemoveAndDelete(item);
					form.OKButton.PerformClick();
					AssertEquals("Form is closed after clicking button", false, form.Visible);
					AssertEquals("Was changed", "ES", collection.Cast<InlandTransport>().Single().CY_Code);
				}
			});
		}

		[RequiresSTA]
		public void TestCloseForm()
		{
			CombineAssertions(() =>
			{
				var header = PrepareDataForTest();
				var collection = header.MovementHeader.InlandTransportList;
				var item = collection.AddNew("DE", "DE001");
				using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
				{
					form.Show();
					collection.AddNew("ES", "ES003");
					collection.RemoveAndDelete(item);
					form.Close();
					AssertEquals("Form is closed with X button", false, form.Visible);
					AssertEquals("No change", "DE", collection.Cast<InlandTransport>().Single().CY_Code);
				}
			});
		}

		public void TestText()
		{
			using (var form = GetFormToBashCore())
			{
				AssertEquals("Additional Wagon Numbers", ((ZForm)form).CaptionResourceString.Caption);
			}
		}

		public void TestAdditionalWagonNumbersGridColumnStyleInfos()
		{
			var header = PrepareDataForTest();
			using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
			{
				var wagonNumberColumnInfo = form.AdditionalWagonNumbersGrid.GetColumnStyle(nameof(InlandTransport.WagonNumber));
				AssertType<ZTextBoxColumnStyleInfo>("WagonNumber column type", wagonNumberColumnInfo);
				AssertEquals("WagonNumber column CharacterCasing", CharacterCasing.Upper, wagonNumberColumnInfo.CharacterCasing);

				var nationalityColumnInfo = form.AdditionalWagonNumbersGrid.GetColumnStyle(nameof(InlandTransport.WagonNationality));
				AssertType<ZCodeFindBoxColumnStyleInfo>("WagonNationality column type", nationalityColumnInfo);
			}
		}

		public void TestOKButton_NoErrorExists()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = PrepareDataForTest();
			var collection = header.MovementHeader.InlandTransportList;
			using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
			{
				form.Show();
				collection.AddNew("DE", "DE002");
				form.OKButton.PerformClick();
				AssertEquals("Form is closed after clicking button", false, form.Visible);
			}
		}

		public void TestOKButton_MessageErrorExists()
		{
			var header = PrepareDataForTest();
			var collection = header.MovementHeader.InlandTransportList;
			using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
			{
				form.Show();
				collection.AddNew("", "DE001");
				form.OKButton.PerformClick();
				AssertEquals("Form is closed after clicking button", false, form.Visible);
			}
		}

		public void TestOKButton_ErrorExists()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = PrepareDataForTest();
			var collection = header.MovementHeader.InlandTransportList;
			using (var form = new AdditionalWagonNumbersForm(header.MovementHeader))
			{
				form.Show();
				collection.AddNew("我向你敬礼啊", "DE003");
				form.OKButton.PerformClick();
				AssertEquals("Pop-up window shows", "The form has errors. Please fix them before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form isn't closed after clicking button", true, form.Visible);

				form.Close();
				AssertEquals("Collection was reverted", 0, collection.Count);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = PrepareDataForTest();
			header.MovementHeader.InlandTransportList.AddNew();
			Factory.Save();
			return new AdditionalWagonNumbersForm(header.MovementHeader);
		}

		NctsHeader PrepareDataForTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}
	}
}
