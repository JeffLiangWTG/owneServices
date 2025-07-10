using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.Module.Testing
{
	[TestedType(typeof(MailItemFilterBusinessObject))]
	public class MailItemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilter()
		{
			TestCaseHelper.ClearTable(Enterprise.ZArchitecture.Schema.MailDBItemsSchema.Constants.TableName);
			MailItemFilterBusinessObject filterBizObj = new MailItemFilterBusinessObject();
			StandardMailItemCollection mailItems = new StandardMailItemCollection(Factory);
			MailItem item1 = GetNewMailItem(MailDirection.Transmit, 1);
			MailItem item2 = GetNewMailItem(MailDirection.Receive, 2);
			Factory.Save();
			ModuleTextFilter fltDirection = (ModuleTextFilter)filterBizObj["Direction"];
			ModuleTextFilter fltStatus = (ModuleTextFilter)filterBizObj["Status"];
			ModuleTextFilter fltSubject = (ModuleTextFilter)filterBizObj["Subject"];
			ModuleTextFilter fltSender = (ModuleTextFilter)filterBizObj["Sender"];
			ModuleTextFilter fltRecipients = (ModuleTextFilter)filterBizObj["Recipients"];
			ModuleTextFilter fltMailBody = (ModuleTextFilter)filterBizObj["Mail Body"];
			ModuleDateFilter fltSend = (ModuleDateFilter)filterBizObj["Send"];
			ModuleDateFilter fltReceived = (ModuleDateFilter)filterBizObj["Received"];
			fltDirection.Property = MailDirection.Transmit;
			fltDirection.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item", mailItems.Contains(item1));
			MailItem item3 = GetNewMailItem(MailDirection.Transmit, 3);
			item3.MI_Status = MailStatus.Failed;
			Factory.Save();
			fltStatus.Property = MailStatus.Failed;
			fltStatus.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item3 Status Failed", mailItems.Contains(item3));
			MailItem item4 = GetNewMailItem(MailDirection.Transmit, 4);
			Factory.Save();
			fltStatus.Property = MailStatus.Sent;
			fltSubject.Property = "4";
			fltSubject.SqlComparisonOperator = SQLComparisonOperator.Contains;
			fltSubject.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item4 Subject", mailItems.Contains(item4));
			MailItem item5 = GetNewMailItem(MailDirection.Transmit, 5);
			Factory.Save();
			fltSubject.Property = "5";
			fltSender.Property = "5";
			fltSender.SqlComparisonOperator = SQLComparisonOperator.Contains;
			fltSender.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item5 Sender", mailItems.Contains(item5));
			MailItem item6 = GetNewMailItem(MailDirection.Transmit, 6);
			Factory.Save();
			fltSubject.Property = "6";
			fltSender.Property = "6";
			fltRecipients.Property = "6";
			fltRecipients.SqlComparisonOperator = SQLComparisonOperator.Contains;
			fltRecipients.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item6 Recipients", mailItems.Contains(item6));
			MailItem item7 = GetNewMailItem(MailDirection.Transmit, 7);
			Factory.Save();
			fltSubject.Property = "7";
			fltSender.Property = "7";
			fltRecipients.Property = "7";
			fltMailBody.Property = "7";
			fltMailBody.SqlComparisonOperator = SQLComparisonOperator.Contains;
			fltMailBody.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item7 Body", mailItems.Contains(item7));
			fltSubject.Property = ZString.Empty;
			fltSender.Property = ZString.Empty;
			fltRecipients.Property = ZString.Empty;
			fltMailBody.Property = ZString.Empty;
			MailItem item8 = GetNewMailItem(MailDirection.Transmit, 8);
			item8.MI_SendDateTime = ZDateTime.UtcNow.AddDays(-2);
			item8.MI_ReceivedDateTime = ZDateTime.UtcNow.AddDays(2);
			Factory.Save();
			fltSend.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			fltSend.Property1 = ZDateTime.Today.AddDays(-3);
			fltSend.Property2 = ZDateTime.Today.AddDays(-1);
			fltSend.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item8 SentDateTime", mailItems.Contains(item8));
			fltReceived.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			fltReceived.Property1 = ZDateTime.Today.AddDays(1);
			fltReceived.Property2 = ZDateTime.Today.AddDays(3);
			fltReceived.IsActive = true;
			mailItems.Load(filterBizObj.Filter);
			AssertEquals(1, mailItems.Count);
			Assert("Transmit Item8 ReceivedDateTime", mailItems.Contains(item8));
		}

#region Implementation
		protected MailItem GetNewMailItem(string direction, int number)
		{
			MailItem item = Factory.New<MailItem>();
			if (direction == MailDirection.Transmit)
			{
				item.MI_Direction = MailDirection.Transmit;
				item.MI_Status = MailStatus.Sent;
			}
			else
			{
				item.MI_Direction = MailDirection.Receive;
				item.MI_Status = MailStatus.Processed;
			}

			item.MI_From = "from" + number.ToString() + "@edi.com";
			item.MI_Subject = "Subject" + number.ToString();
			item.AddRecipientForUserCommunication("Recipient" + number.ToString() + "@edi.com", MailRecipient.RecipientTypes.TO);
			item.MI_SendDateTime = ZDateTime.UtcNow;
			item.MI_ReceivedDateTime = ZDateTime.UtcNow;
			item.MI_Body = new ZString("Message Body" + number.ToString());
			return item;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new MailItemFilterBusinessObject();
		}
#endregion
	}
}
