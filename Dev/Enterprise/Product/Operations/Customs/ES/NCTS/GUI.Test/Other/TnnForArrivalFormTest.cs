using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(TnnForArrivalForm))]
	class TnnForArrivalFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore() => new TnnForArrivalForm(TnnDataCodeInfo);

		#endregion

		TnnDataCodeInfo TnnDataCodeInfo
		{
			get
			{
				if (tnnDataCodeInfo == null)
				{
					CreateNCTS();
					tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
				}
				return tnnDataCodeInfo;
			}
		}
		TnnDataCodeInfo tnnDataCodeInfo;

		public void TestForCaptions()
		{
			using (var tnnForArrivalForm = new TnnForArrivalForm(TnnDataCodeInfo))
			{
				var acceptanceDateEdit = (ZDateEdit)(tnnForArrivalForm.Controls.Find("AcceptanceDate", true).Single());
				var clearanceDateEdit = (ZDateEdit)(tnnForArrivalForm.Controls.Find("ClearanceDate", true).Single());

				tnnForArrivalForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Acceptance Date Caption", "Acceptance Date", acceptanceDateEdit.CaptionResourceString.Caption);
					AssertEquals("Clearance Date Caption", "Clearance Date", clearanceDateEdit.CaptionResourceString.Caption);
				});
			}
		}

		[TestDate(2023, 12, 20)]
		[RequiresSTA]
		public void TestEnableOKButtonAndDateColor()
		{
			var acceptanceDate = ZDateTime.Now.AddDays(-3);
			var clearanceDate = ZDateTime.Now.AddDays(-2);

			var acceptanceDateInFuture = ZDateTime.Now.AddMonths(+2);
			var clearanceDateInFuture = ZDateTime.Now.AddMonths(+2);

			var acceptanceDateOlder31Days = ZDateTime.Now.AddDays(-31);
			var acceptanceDateOlder30Days = ZDateTime.Now.AddDays(-30);

			CombineAssertions(() =>
			{
				using (var tnnForArrivalForm = new TnnForArrivalForm(TnnDataCodeInfo))
				{
					var acceptanceDateEdit = (ZDateEdit)(tnnForArrivalForm.Controls.Find("AcceptanceDate", true).Single());
					var clearanceDateEdit = (ZDateEdit)(tnnForArrivalForm.Controls.Find("ClearanceDate", true).Single());
					var okButton = (ZButton)(tnnForArrivalForm.Controls.Find("OKButton", true).Single());
					clearanceDateEdit.DateTimeValue = ZDateTime.Invalid;
					acceptanceDateEdit.DateTimeValue = ZDateTime.Invalid;
					tnnForArrivalForm.Show();

					AssertEquals("Acceptance and Clearance are Empty so OkButton is not Enabled", false, okButton.Enabled);
					clearanceDateEdit.DateTimeValue = clearanceDate;
					AssertEquals("Clearance date is not in future so clearanceDateEdit has the default ForeColor", tnnForArrivalForm.ForeColor, clearanceDateEdit.DateTextBox.ForeColor);

					AssertEquals("Acceptance is Empty so OkButton is not Enabled", false, okButton.Enabled);

					acceptanceDateEdit.DateTimeValue = acceptanceDate;
					AssertEquals("Acceptance date is not in future so acceptanceDateEdit has the default ForeColor", tnnForArrivalForm.ForeColor, acceptanceDateEdit.DateTextBox.ForeColor);
					AssertEquals("Acceptance and clearance date are not Empty so OkButton is Enabled", true, okButton.Enabled);

					clearanceDateEdit.DateTimeValue = clearanceDateInFuture;
					AssertEquals("Clearance date is in future so clearanceDateEdit has the Red ForeColor", Color.Red, clearanceDateEdit.DateTextBox.ForeColor);
					AssertEquals("Clearance date is in future so OkButton is not Enabled", false, okButton.Enabled);

					acceptanceDateEdit.DateTimeValue = acceptanceDateInFuture;
					AssertEquals("Acceptance date is in future so acceptanceDateEdit has the Red ForeColor", Color.Red, acceptanceDateEdit.DateTextBox.ForeColor);
					AssertEquals("Acceptance date is in future so OkButton is not Enabled", false, okButton.Enabled);

					acceptanceDateEdit.DateTimeValue = acceptanceDateOlder30Days;
					clearanceDateEdit.DateTimeValue = acceptanceDateOlder30Days;
					AssertEquals("Acceptance date is 30 days older so acceptanceDateEdit has the default ForeColor", tnnForArrivalForm.ForeColor, acceptanceDateEdit.DateTextBox.ForeColor);
					AssertEquals("Acceptance date is 30 days older so OkButton is Enabled", true, okButton.Enabled);

					acceptanceDateEdit.DateTimeValue = acceptanceDateOlder31Days;
					AssertEquals("Acceptance date is 31 days older so acceptanceDateEdit has the Red ForeColor", Color.Red, acceptanceDateEdit.DateTextBox.ForeColor);
					AssertEquals("Acceptance date is 31 days older so OkButton is Enabled", true, okButton.Enabled);
					AssertEquals("Acceptance date is 31 days older so message", "Acceptance date is older than 30 days from current date", acceptanceDateEdit.CaptionResourceString.FullDescription);

					clearanceDateEdit.DateTimeValue = acceptanceDateOlder30Days;
					acceptanceDateEdit.DateTimeValue = acceptanceDateOlder31Days;
					AssertEquals("Clearance date is greater than Acceptance date so OkButton is Enabled", true, okButton.Enabled);

					acceptanceDateEdit.DateTimeValue = acceptanceDateOlder30Days;
					clearanceDateEdit.DateTimeValue = acceptanceDateOlder31Days;
					AssertEquals("Clearance date is greater than Acceptance Date so clearanceDateEdit has the Red ForeColor", Color.Red, clearanceDateEdit.DateTextBox.ForeColor);
					AssertEquals("Clearance date is greater than Acceptance Date so OkButton is not Enabled", false, okButton.Enabled);
					AssertEquals("Clearance date is greater than Acceptance Date so message", "Clearance Date cannot be older than Acceptance Date", clearanceDateEdit.CaptionResourceString.FullDescription);
				}
			});
		}

		[RequiresSTA]
		public void TestFields()
		{
			using (var tnnForArrivalForm = new TnnForArrivalForm(TnnDataCodeInfo))
			{
				var messageLabel = (ZLabel)tnnForArrivalForm.Controls.Find("MessageLabel", true).Single();
				var acceptanceDateEdit = (ZDateEdit)tnnForArrivalForm.Controls.Find("AcceptanceDate", true).Single();
				var clearanceDateEdit = (ZDateEdit)tnnForArrivalForm.Controls.Find("clearanceDate", true).Single();
				tnnForArrivalForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the text we expect", "Acceptance Date and Clearance Date are mandatory to create a TNN declaration:", messageLabel.Text);
					AssertEquals("Accetance Date has the text we expect", ZDateTime.Empty, acceptanceDateEdit.DateTimeValue);
					AssertEquals("Accetance Date has TabStop", true, acceptanceDateEdit.TabStop);
					AssertEquals("Accetance Date has TabIndex", 0, acceptanceDateEdit.TabIndex);

					AssertEquals("Clearance Date has the text we expect", ZDateTime.Empty, clearanceDateEdit.DateTimeValue);
					AssertEquals("Clearance Date has TabStop", true, clearanceDateEdit.TabStop);
					AssertEquals("Clearance Date has TabIndex", 1, clearanceDateEdit.TabIndex);
				});
			}
		}

		void CreateNCTS()
		{
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = ApplicationReference;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.EffectiveMessageStatus = OriginalMessageStatus;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
		}
		NctsHeader nctsHeader;

		ZString OriginalMessageStatus => "INI";
		ZString OriginalEntryStatus => "INI";
		ZString ApplicationReference => "Reference";
	}
}
