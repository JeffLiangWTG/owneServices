using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(EditClearanceInfoForm))]
	class EditClearanceInfoFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore() => new EditClearanceInfoForm(ClearanceInfo);

		#endregion

		ClearanceInfo ClearanceInfo
		{
			get
			{
				if (clearanceInfo == null)
				{
					clearanceInfo = ClearanceInfo.LoadNew(EntryNumber);
				}
				return clearanceInfo;
			}
		}
		ClearanceInfo clearanceInfo;

		CusEntryNumber EntryNumber
		{
			get
			{
				if (entryNumber == null)
				{
					entryNumber = CreateNCTS();
				}
				return entryNumber;
			}
		}
		CusEntryNumber entryNumber;

		[RequiresSTA]
		public void TestClearanceNumber()
		{
			CombineAssertions(() =>
			{
				var newEntryNumber0 = CreateNCTS(mrncode: "1234567890123456789");
				using (var editClearanceInfoForm0 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber0)))
				{
					var clearanceNumberControl = (ZTextBox)(editClearanceInfoForm0.Controls.Find("ClearanceNumber", true).Single());
					editClearanceInfoForm0.Show();
					AssertEquals("ClearanceNumber expected length when the length is more", 16, clearanceNumberControl.Text.Length);
				}
				var newEntryNumber1 = CreateNCTS();
				using (var editClearanceInfoForm1 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber1)))
				{
					var clearanceNumberControl = (ZTextBox)(editClearanceInfoForm1.Controls.Find("ClearanceNumber", true).Single());
					editClearanceInfoForm1.Show();
					AssertEquals("ClearanceNumber expected when when the value has MRN", MRNCode, clearanceNumberControl.Text);
				}
				var newEntryNumber2 = CreateNCTS(mrncode: "123---..01");
				using (var editClearanceInfoForm2 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber2)))
				{
					var clearanceNumberControl = (ZTextBox)(editClearanceInfoForm2.Controls.Find("ClearanceNumber", true).Single());
					editClearanceInfoForm2.Show();
					AssertEquals("ClearanceNumber expected when the value has symbols", "123---..01", clearanceNumberControl.Text);
				}
			});
		}

		public void TestClearanceDate()
		{
			CombineAssertions(() =>
			{
				var newEntryNumber0 = CreateNCTS(clearanceDate: ZDateTime.Empty);
				using (var editClearanceInfoForm0 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber0)))
				{
					var clearanceDateControl = (ZDateEdit)(editClearanceInfoForm0.Controls.Find("ClearanceDate", true).Single());
					editClearanceInfoForm0.Show();
					AssertEquals("ClearanceDate expected when value is empty", ZDateTime.Empty.ToString(), clearanceDateControl.Text);
				}
				var newEntryNumber1 = CreateNCTS(clearanceDate: ZDateTime.Today);
				using (var editClearanceInfoForm1 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber1)))
				{
					var clearanceDateControl = (ZDateEdit)(editClearanceInfoForm1.Controls.Find("ClearanceDate", true).Single());
					editClearanceInfoForm1.Show();
					AssertEquals("ClearanceDate expected when value is today", ZDateTime.Today.ToString().ToUpper(), clearanceDateControl.Text);
				}
			});
		}

		public void TestArrivalLimitDate()
		{
			CombineAssertions(() =>
			{
				var newEntryNumber0 = CreateNCTS(clearanceDate: ZDateTime.Today, arrivalLimitDate: ZDateTime.Empty);
				using (var editClearanceInfoForm0 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber0)))
				{
					var arrivalLimitDateControl = (ZDateEdit)(editClearanceInfoForm0.Controls.Find("ArrivalLimitDate", true).Single());
					editClearanceInfoForm0.Show();
					AssertEquals("ArrivalLimitDate expected when value is empty", ZDateTime.Empty.ToString(), arrivalLimitDateControl.Text);
				}
				var newEntryNumber1 = CreateNCTS(clearanceDate: ZDateTime.Today, arrivalLimitDate: ZDateTime.Today.AddDays(-1));
				using (var editClearanceInfoForm1 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber1)))
				{
					var arrivalLimitDateControl = (ZDateEdit)(editClearanceInfoForm1.Controls.Find("ArrivalLimitDate", true).Single());
					editClearanceInfoForm1.Show();
					AssertEquals("ArrivalLimitDate expected when value is equal or less than ClearanceDate", ZDateTime.Today.AddDays(-1).ToString().ToUpper(), arrivalLimitDateControl.Text);
				}
				var newEntryNumber3 = CreateNCTS(clearanceDate: ZDateTime.Today, arrivalLimitDate: ZDateTime.Today.AddDays(1));
				using (var editClearanceInfoForm3 = new EditClearanceInfoForm(ClearanceInfo.LoadNew(newEntryNumber3)))
				{
					var arrivalLimitDateControl = (ZDateEdit)(editClearanceInfoForm3.Controls.Find("ArrivalLimitDate", true).Single());
					editClearanceInfoForm3.Show();
					AssertEquals("ArrivalLimitDate expected when value is larger than ClearanceDate", ZDateTime.Today.AddDays(1).ToString().ToUpper(), arrivalLimitDateControl.Text);
				}
			});
		}

		CusEntryNumber CreateNCTS(string mrncode = MRNCode, ZDateTime? clearanceDate = null, ZDateTime? arrivalLimitDate = null)
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = ApplicationReference;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.EffectiveMessageStatus = OriginalMessageStatus;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			return SetEntryNumber(nctsHeader, mrncode, clearanceDate, arrivalLimitDate);
		}
		CusEntryNumber SetEntryNumber(NctsHeader nctsHeader, string mrnCode, ZDateTime? clearanceDate = null, ZDateTime? arrivalLimitDate = null)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = mrnCode;
			newEntryNumber.CE_IssueDate = (ZDateTime)(clearanceDate == null ? ZDateTime.Empty : clearanceDate);
			newEntryNumber.CE_ExpiryDate = (ZDateTime)(arrivalLimitDate == null ? ZDateTime.Empty : arrivalLimitDate);

			return newEntryNumber;
		}

		const string MRNCode = "20ES009998300012";
		ZString OriginalMessageStatus => "INI";
		ZString OriginalEntryStatus => "INI";
		ZString ApplicationReference => "Reference";
	}
}
