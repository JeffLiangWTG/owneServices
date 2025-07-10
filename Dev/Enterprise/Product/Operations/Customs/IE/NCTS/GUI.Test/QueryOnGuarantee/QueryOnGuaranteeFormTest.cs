using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	[TestedType(typeof(QueryOnGuaranteeForm))]
	class QueryOnGuaranteeFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = GetForm() as ZForm)
			{
				AssertEquals("FormHeading", "Query on Guarantee", form.FormHeading);
				form.Show();
				AssertEquals("Text", "Query on Guarantee", form.Text);
			}
		}

		public void TestSize()
		{
			using (var form = GetForm())
			{
				form.Show();
				AssertEquals("MinimumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 293, true), form.MinimumSize);
			}
		}

		[RequiresSTA]
		public void TestShowFormType()
		{
			QueryOnGuaranteeForm.ShowForm(header);
			AssertType<QueryOnGuaranteeForm>("Dialog form type = QueryOnGuaranteeForm", ZFormModaliser.LastFormShownDialogForTest);
		}

		[RequiresSTA]
		public void TestQueryIdentifierDropEdit()
		{
			using (var form = new QueryOnGuaranteeForm(messageSendingObjectParent))
			{
				form.Show();
				var queryIdentifierDropEdit = form.FindSingle<ZDropEditWithFixedWidth>("QueryIdentifierDropEdit");
				CombineAssertions(() =>
				{
					AssertNotNull(queryIdentifierDropEdit);
					AssertEquals("QueryIdentifierDropEdit.Caption", "Query Identifier", queryIdentifierDropEdit.CaptionResourceString.Caption);
					AssertEquals("QueryIdentifierDropEdit.Location", ControlDpiScalingHelper.NewScaledPoint(91, 17, true), queryIdentifierDropEdit.Location);
				});
			}
		}

		public void TestPeriodFromDateEdit()
		{
			using (var form = new QueryOnGuaranteeForm(messageSendingObjectParent))
			{
				form.Show();
				var periodFromDateEdit = form.FindSingle<ZDateEdit>("PeriodFromDateEdit");
				CombineAssertions(() =>
				{
					AssertNotNull(periodFromDateEdit);
					AssertEquals("PeriodFromDateEdit.Caption", "Period From", periodFromDateEdit.CaptionResourceString.Caption);
					AssertEquals("PeriodFromDateEdit.Location", ControlDpiScalingHelper.NewScaledPoint(343, 17), periodFromDateEdit.Location);
					AssertEquals("PeriodFromDateEdit.Format", ZArchitecture.Core.ZDateTimePickerFormat.Short, periodFromDateEdit.DateTimeFormat);
				});
			}
		}

		[RequiresSTA]
		public void TestPeriodToDateEdit()
		{
			using (var form = new QueryOnGuaranteeForm(messageSendingObjectParent))
			{
				form.Show();
				var periodToDateEdit = form.FindSingle<ZDateEdit>("PeriodToDateEdit");
				CombineAssertions(() =>
				{
					AssertNotNull(periodToDateEdit);
					AssertEquals("PeriodToDateEdit.Caption", "Period To", periodToDateEdit.CaptionResourceString.Caption);
					AssertEquals("PeriodToDateEdit.Location", ControlDpiScalingHelper.NewScaledPoint(485, 17), periodToDateEdit.Location);
					AssertEquals("PeriodToDateEdit.Format", ZArchitecture.Core.ZDateTimePickerFormat.Short, periodToDateEdit.DateTimeFormat);
				});
			}
		}

		[RequiresSTA]
		public void TestGuaranteesGrid_AvailableColumns()
		{
			using (var form = new QueryOnGuaranteeForm(messageSendingObjectParent))
			{
				AssertSequencesEqual("GuaranteesUserControl available Columns", new[] { nameof(QueryOnGuaranteeSendingObject.GuaranteeType), nameof(QueryOnGuaranteeSendingObject.GuaranteeReferenceNumber), nameof(QueryOnGuaranteeSendingObject.AccessCode), nameof(QueryOnGuaranteeSendingObject.ShouldSend), }, form.GuaranteesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => !col.IsUnavailable).Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestGuaranteesGroupBox_Caption()
		{
			using (var form = new QueryOnGuaranteeForm(messageSendingObjectParent))
			{
				AssertEquals("GuaranteesGroupBox.CaptionResourceString", "Guarantees", form.GuaranteesGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestHidePW_Password()
		{
			using (var form = new QueryOnGuaranteeForm(messageSendingObjectParent))
			{
				var passwordColumn = (ZTextBoxColumnStyleInfo)form.GuaranteesGrid.GetColumnStyle(nameof(QueryOnGuaranteeSendingObject.AccessCode));
				AssertEquals('*', passwordColumn.PasswordChar);
			}
		}

		public void TestButtonsCaptions()
		{
			using (var form = new QueryOnGuaranteeForm(messageSendingObjectParent))
			{
				form.Show();
				CombineAssertions(() =>
				{
					var clearAllButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "ClearAllButton");
					AssertNotNull(clearAllButton);
					AssertEquals("ClearAllButton.Caption", "Clear Selection", clearAllButton.CaptionResourceString.Caption);

					var selectAllButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SelectAllButton");
					AssertNotNull(selectAllButton);
					AssertEquals("SelectAllButton.Caption", "Select All", selectAllButton.CaptionResourceString.Caption);

					var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
					AssertNotNull(sendButton);
					AssertEquals("SendButton.Caption", "OK", sendButton.CaptionResourceString.Caption);

					var cancelButton2 = form.FindSingleOrDefault<ZButton>(c => c.Name == "CancelButton2");
					AssertNotNull(cancelButton2);
					AssertEquals("CancelButton2.Caption", "Cancel", cancelButton2.CaptionResourceString.Caption);
				});
			}
		}

		public void TestSelectAllAndClearAll()
		{
			var guarantee1 = header.MovementHeader.Guarantees.AddNew();
			var guarantee2 = header.MovementHeader.Guarantees.AddNew();
			var guarantee3 = header.MovementHeader.Guarantees.AddNew();

			Factory.Save();
			using (var form = GetForm())
			{
				var sendingAction = messageSendingObjectParent.SendingObjectsCollection.Cast<QueryOnGuaranteeSendingAction>().First();
				var sendingObjects = sendingAction.AllGuarantees.Cast<QueryOnGuaranteeSendingObject>();
				form.Show();

				AssertEquals("Nothing is selected on form opening", true, sendingObjects.All(x => x.ShouldSend == false));

				var selectAllButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SelectAllButton");
				selectAllButton.PerformClick();
				AssertEquals("All are selected after Select All Button click", true, sendingObjects.All(x => x.ShouldSend == true));

				var clearAllButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "ClearAllButton");
				clearAllButton.PerformClick();
				AssertEquals("Nothing is selected after Clear All Button click", true, sendingObjects.All(x => x.ShouldSend == false));
			}
		}

		protected override Form GetFormToBashCore()
		{
			SetUp();
			return GetForm();
		}

		QueryOnGuaranteeForm GetForm()
		{
			return new QueryOnGuaranteeForm(messageSendingObjectParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var guarantee = header.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = "1";
			guarantee.PW_BondNumber = "ABC123";
			guarantee.PW_Password = "DEF";
			messageSendingObjectParent = new QueryOnGuaranteeSendingActionParent(header);
		}
		NctsHeader header;
		QueryOnGuaranteeSendingActionParent messageSendingObjectParent;
	}
}
