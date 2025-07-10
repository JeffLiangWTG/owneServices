using System.Windows.Forms;
using CargoWise.Main.Diagnostics;
using NUnit.Framework;

namespace Enterprise.Main.Diagnostics.Testing
{
	class TestRigFormTest : TestCase
	{
		[RequiresSTA]
		public void TestDecodeDataButton_Click_UsageTransaction()
		{
			TestDecodeDataButton_Click("svldw4p4VnYmXMI3lexGQQEtq6zO6X29ImMpnqzBL4VlLBNoT3aHgEdjr1VP4G3ahidN/V84/5QZi8X5UwAFmqyfKjp/G6O4iWsA/dyEeDxqqFJTGVjrTzP0ZqqoHgzW6nHT/U9PuKTYTIVwBVsdwPxRyIUZXlRDo4sR96eF5C8blBL76Ijjo5VFBIrCPTM9yA44jjqDmK5WD4Dkwa6zc2dUujezCKVC4GqY8sBH/sHKR79dhS6hO7KzzOhL9lhulSovs7x9qOht6aRJ8iioqov//icBgAjNgm8EcZwtWg57CAOePKiZ+EZgkIvBFS8EZAsQU5z4k5DIx9+4ULOIswz63Fq0t+qaGB43IJLCKP9Oe47PY/7MC1bIrZvqRdRRwobevszeztPe3IR2qiMQpij8+KGY/63BRo6XOPkm0LOD6CNBzU18iLFH9ndnZpUkNNVshESxa+aeWJXJJM01ogrSlH5ALuYOoGyrZ7PNbQ+0dUeo6KSNcvloguO67rBW69xR24Yn1B7Po51zCrGpj4EUMRFbZmtzlooUD4C19140VQf23wPJPz+E5qv298jGEkULC8FSGvsDQqWpznYKTr1e0WGnE6Cb7lfPcBdbjL7uzOAktp08HwWDvvldwvjdjALl4xfh1EV2uLygufsNbmwPnP0YuH0R5F5VUKpZDKk=",
				@"BillableCount: 1
ClientStaffCode: 
Reference1: 
Reference2: 
Reference3: 
Reference4: 
Reference5: 
ServiceOccuredUTC: 13/01/2025 6:42:06 AM
AdditionalRefs: {""Reference1"":""SB00000119"",""Reference5"":""9496615A-5352-421E-B214-FFB82645C5F7"",""ClientStaffCode"":""SC ""}
EnterpriseCode: WTL
ServerCode: S37
Environment: TST
CompanyCode: DAU
CompanyName: 
BranchCode: SYD
UsageCode: SBA");
		}

		[RequiresSTA]
		public void TestDecodeDataButton_Click_BillingTransaction()
		{
			TestDecodeDataButton_Click(
				"r/DexUF2gPT+r5Hu612M6qSBA63LeFtm3JnX4hXn99Xnqj8qNJxxoUQn88dQZDGwtnHniYSPd0MEff3Cfp5EE6Ox0sbE1IYqGBZ3iOlIZjJ+L/gfHHDQeqMDDlEtnf+BAZredkgIqMjExSKEyjlHLuepe2EEUMUWN8DT5SlAgha3gBxXKQMInmPsaozo+86Z6XY9mBcAaAeYmgKn6oLleayQy53QTw1a+48YeD5xFSnoLaXpXvWOJFW9hIaIWaV+znJEnWmAo9NKEN++lvDIR9VAaaILarCcsxoTLNyCljJTrPVQgRhe/pl8HuMGj2hbS5xD4NwRB8eMIF6KiTFW1CQcmqBno3amgOeaC/jabnyeMRLkMQzKFdWcgnpdsV9JRWDPuCFOLtTCmViVp/RmR6FWq2oqbJrNzoNRwqgey4yKoUlCAmUpA/VoqK2DNTfshx84UU5PNaY3xBi5ttXYur4WDeadx08PvRNi0gJ+MjYThQSuGjaWYZDaCaf89i6rDDm4dQ5cmAeojHZVHUZeDB4ZkFUNzsWUoMBgR7BXcqpsJ+0LIM6Zu4TG3Sj0mZ8B1/BIqS9IIRB3Z3rXdRlmO0W/Zu0m8DmFcOC77dDPK+ivlMIFRt7Ne5Mhb5ttXB++picWeq7pHjACMnTRUgG48Q==",
				@"BillableCount: 1
Branch: 
Category: 
ClientID: ABCDEFXYZ
ClientNumber: CN
ClientStaffCode: CSC
PriceItemCode: TST
Reference1: REF 1
Reference2: REF 2
Reference3: REF 3
Reference4: REF 4
Reference5: 
ReportingSource: ENT
ServiceOccuredUTC: 14/07/2021 12:01:09 AM
Version: 0
MessageTrackingID: 
AdditionalRefs: Data");
		}

		void TestDecodeDataButton_Click(string encryptedData, string expectedResult)
		{
			AssertEquals(1, 1);
			var form = new TestRigForm();
			form.Show();
			form.Controls.Find("BillingEncodedDataTextBox", true)[0].Text = encryptedData;
			ClickButton(form, "BillingDecodeDataButton");

			AssertEquals(expectedResult, form.Controls.Find("BillingDataTextBox", true)[0].Text);
			form.Close();
			form.Dispose();
		}

		static void ClickButton(Form form, string buttonName)
		{
			var button = form.Controls.Find(buttonName, true)[0] as Button;
			var onClickMethod = typeof(Button).GetMethod(
				"OnClick",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			onClickMethod.Invoke(button, new object[] { System.EventArgs.Empty });
		}
	}
}
