using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class SaveLayoutUserQueryHandlerTest : TestCaseWithFactory
	{
		public void TestShowErrors()
		{
			var testFilter = new ModuleTextFilter("Test", DummyBizoSchema.GenericStringSchemaColumn);

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);
			AssertEquals("PreCondition", 0, filterBizO.ActiveModuleFilters.Count);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new FilterStripControlTest.DummyZFilterStripControl(collection, filterBizO);

				form.Controls.Add(filterControl);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertNull(new SaveLayoutUserQueryHandler().QueryUser(filterBizO, true, true));
				AssertEquals(FilterStripBusinessObject.NothingToSave, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testFilter.IsActive = true;
				new SaveLayoutUserQueryHandler().QueryUser(filterBizO, true, true);
				AssertNotEquals(FilterStripBusinessObject.NothingToSave, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWhenUsersCancelOnSaveLayoutForm()
		{
			var testFilter = new ModuleTextFilter("Test", DummyBizoSchema.GenericStringSchemaColumn);
			testFilter.Visibility = FilterVisibility.AlwaysVisible;
			testFilter.IsActive = true;
			testFilter.Property = "ABC";

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var filterControl = new FilterStripControlTest.DummyZFilterStripControl(collection, filterBizO);

				form.Controls.Add(filterControl);
				form.Show();

				var mockHandler = new Mock<SaveLayoutUserQueryHandler>() { CallBase = true };
				var mockSaveLayoutBizObj = new Mock<SaveLayoutBizO>(filterBizO) { CallBase = true };
				mockSaveLayoutBizObj.Object.LayoutName = "hiwre7984wrhuj";

				using (var layoutForm = new SaveLayoutForm(mockSaveLayoutBizObj.Object, true, true))
				{
					layoutForm.IsOkToSave = false;

					mockHandler.Protected().Setup<SaveLayoutBizO>("GetSaveLayoutBizObj", ItExpr.IsAny<IModifyModuleAndGridLayout>()).Returns(mockSaveLayoutBizObj.Object);
					mockHandler.Protected().Setup<SaveLayoutForm>("GetSaveLayoutForm", ItExpr.IsAny<SaveLayoutBizO>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns(layoutForm);

					AssertNull("users have cancelled on layoutForm and it should not return anything", mockHandler.Object.QueryUser(filterBizO, true, true));

					layoutForm.IsOkToSave = true;
					AssertEquals(mockSaveLayoutBizObj.Object, mockHandler.Object.QueryUser(filterBizO, true, true));
				}
			}
		}
	}
}
