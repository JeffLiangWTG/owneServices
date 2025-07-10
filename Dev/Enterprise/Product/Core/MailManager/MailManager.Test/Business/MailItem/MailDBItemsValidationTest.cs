using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using MailManager;

namespace Enterprise.MailManager.Business.Testing
{
	sealed class MailDBItemsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMI_From()
		{
			var item = Factory.New<MailItem>();
			item.Validation.ValidateMI_From();
			AssertHasErrors(item.MI_FromInfo);
			item.MI_From = "foo"; // Mail servers accept unformatted senders (try it yourself!) so we should too. 
			AssertNoErrors(item.MI_FromInfo);
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_Direction = DirectionList.Codes.Receive;
			Factory.Save();
			item.MI_From = "";
			AssertNoErrors(item.MI_FromInfo);
		}

		public void TestMI_Direction()
		{
			var item = Factory.New<MailItem>();
			item.Validation.ValidateMI_Direction();
			AssertHasErrorContaining(item.MI_DirectionInfo, MandatoryValidation.MustBeEntered);

			item.MI_Direction = "??";
			AssertNoErrorContaining(item.MI_DirectionInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(item.MI_DirectionInfo, ListValidation.InvalidCodeError);

			item.MI_Direction = DirectionList.Codes.Transmit;
			AssertNoErrorContaining(item.MI_DirectionInfo, ListValidation.InvalidCodeError);
			item.MI_ReceivedDateTime = ZDateTime.Now;
			Factory.Save();
			item.MI_Direction = ZString.Empty;
			AssertNoErrors(item.MI_DirectionInfo);
		}

		public void TestMI_Status()
		{
			var item = Factory.New<MailItem>();
			item.MI_Status = ZString.Empty;
			item.MI_Direction = DirectionList.Codes.Receive;
			AssertHasErrorContaining(item.MI_StatusInfo, MandatoryValidation.MustBeEntered);

			item.MI_Status = "??";
			AssertNoErrorContaining(item.MI_StatusInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(item.MI_StatusInfo, ListValidation.InvalidCodeError);

			item.MI_Status = StatusCodeList.Codes.Queued;
			AssertNoErrorContaining(item.MI_StatusInfo, ListValidation.InvalidCodeError);
			item.MI_ReceivedDateTime = ZDateTime.Now;
			Factory.Save();
			item.MI_Status = ZString.Empty;
			AssertNoErrors(item.MI_StatusInfo);
		}
	}
}
