using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.DocumentAcknowledgement.Testing
{
	sealed class EmailProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			Guid logGuid = Guid.NewGuid();
			MailItem mailItem = AddNewMailItem(EmailProcessor.SubjectIdentifier + " " + logGuid.ToString() + " Failure", MailDirection.Receive, MailStatus.Queued);

			var helper = new MessageFilterTestHelper<EmailProcessorForTesting, MailItem>();
			var ctx = helper.ProcessorFactory.GetContext(helper.Log);
			Assert(helper.Process(ctx, mailItem));

			EmailProcessorForTesting processor = ctx.GetFilterInstance<EmailProcessorForTesting>();
			Assert("Processed", processor.AckProcessor.HasProcessed);
			AssertEquals("Processed correct PK", logGuid, processor.AckProcessor.LastPK);
			AssertEquals("Processed correct status", DocumentAcknowledgementStatus.Failure, processor.AckProcessor.LastStatus);
		}

		public void TestProcessCorrectlyFormattedEmail()
		{
			Guid logGuid = Guid.NewGuid();
			MailItem mailItem = AddNewMailItem(EmailProcessor.SubjectIdentifier + " " + logGuid.ToString() + " Failure", MailDirection.Receive, MailStatus.Queued);
			Factory.Save();

			Processor.ProcessAcknowledgementMail(mailItem);

			Assert("Processed", Processor.AckProcessor.HasProcessed);
			AssertEquals("Processed correct PK", logGuid, Processor.AckProcessor.LastPK);
			AssertEquals("Processed correct status", DocumentAcknowledgementStatus.Failure, Processor.AckProcessor.LastStatus);
		}

		public void TestProcessCorrectlyFormattedEmailWithOtherStuffInSubject_ShouldProcess()
		{
			var subjects = new[]
			{
				"{0} {1} Failure - EDITradeToCargowise Error - Cannot Find The Cargowise Shipment Number In The Subject Line",
				"{0} {1} Failure [Our Ref:EXD31313]",
				"{0} {1} Failure - EDITradeToCargowise Error - Cannot Find The Cargowise Shipment Number In The Subject Line - EDITradeToCargowise Error - Cannot Find The Cargowise Shipment Number In The Subject Line",
				"{0}  {1}     Failure  "
			};

			var logGuid = Guid.NewGuid();
			var mailItems = subjects.Select(s => AddNewMailItem(string.Format(s, EmailProcessor.SubjectIdentifier, logGuid), MailDirection.Receive, MailStatus.Queued)).ToArray();
			Factory.Save();

			foreach (var item in mailItems)
			{
				Processor.ProcessAcknowledgementMail(item);

				Assert(string.Format("Should have processed item with subject [{0}]", item.MI_Subject), Processor.AckProcessor.HasProcessed);
				AssertEquals("Processed correct PK", logGuid, Processor.AckProcessor.LastPK);
				AssertEquals("Processed correct status", DocumentAcknowledgementStatus.Failure, Processor.AckProcessor.LastStatus);
			}
		}

		public void TestProcessIncorrectlyFormattedEmail()
		{
			MailItem mailItem = AddNewMailItem(EmailProcessor.SubjectIdentifier + " rubbish", MailDirection.Receive, MailStatus.Queued);
			Factory.Save();

			var helper = new MessageFilterTestHelper<EmailProcessor, MailItem>();
			helper.Process(mailItem);

			AssertEquals("Processed", false, Processor.AckProcessor.HasProcessed);
			Assert("Report developer error", ErrorReporter.LastMessageReported.StartsWith("Bad ack"));
			ErrorReporter.Clear();
		}

		MailItem AddNewMailItem(string subject, string direction, string status)
		{
			MailItem mailItem = Factory.New(typeof(MailItem)) as MailItem;
			mailItem.MI_ReceivedDateTime = mailItem.MI_SendDateTime = new ZDateTime(2000, 1, 1);
			mailItem.MI_Subject = subject;
			mailItem.MI_Direction = direction;
			mailItem.MI_Status = status;

			return mailItem;
		}

		class EmailProcessorForTesting : EmailProcessor
		{
			public EmailProcessorForTesting()
			{
				_acknowledgementProcessor = new MockDocumentAcknowledgementProcessor();
			}

			protected override IDocumentAcknowledgementProcessor GetAcknowledgementProcessor()
			{
				return _acknowledgementProcessor;
			}

			public MockDocumentAcknowledgementProcessor AckProcessor
			{
				get { return _acknowledgementProcessor; }
			}

			readonly MockDocumentAcknowledgementProcessor _acknowledgementProcessor;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
		}

		EmailProcessorForTesting Processor
		{
			get
			{
				if (_processor == null)
				{
					_processor = new EmailProcessorForTesting();
				}

				return _processor;
			}
		}

		EmailProcessorForTesting _processor;
	}
}
