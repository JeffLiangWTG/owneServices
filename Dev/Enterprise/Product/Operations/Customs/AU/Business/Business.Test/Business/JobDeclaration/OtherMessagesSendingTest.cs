using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class OtherMessagesSendingTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendWarrelMessage();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("only be sent for Export Declarations") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingWARRELOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWhenDeclarationIsNotClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Export Declaration is not clear") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWhenNoDeportID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Bonded Warehouse and either a CFS or CTO must be specified") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWhenNoWarehouseID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1 = declaration.Factory.New<OrgAddress>();
			OrgHeader header1 = declaration.Factory.New<OrgHeader>();
			address1.OA_OH = header1.PK;
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = address1.PK;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Bonded Warehouse and either a CFS or CTO must be specified") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWhenNoEstimatedPickUpDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1 = declaration.Factory.New<OrgAddress>();
			OrgHeader header1 = declaration.Factory.New<OrgHeader>();
			address1.OA_OH = header1.PK;
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = address1.PK;
			OrgAddress address2 = declaration.Factory.New<OrgAddress>();
			OrgHeader header2 = declaration.Factory.New<OrgHeader>();
			address2.OA_OH = header2.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = address2.PK;
			header2.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "2222Z");
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Estimated Pickup Date and Time is required") != -1);
				throw;
			}
		}

		[TestDate(2007, 6, 24)]
		public void TestSendWARRELOriginal()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRELOriginal.Code, declaration.JE_MessageStatus);
		}

		[TestDate(2007, 6, 24)]
		public void TestSendWARRELReplacement()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRELReplacement.Code, declaration.JE_MessageStatus);
		}

		[TestDate(2007, 6, 24)]
		public void TestAnswerYesToSendWARRELWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelMessage();
			AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRELOriginal.Code, declaration.JE_MessageStatus);
		}

		[TestDate(2007, 6, 24)]
		public void TestSendWARRELTwice()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARREL();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelMessage();
			AssertEquals("MessageState", CustomsEntryStatus.AwaitingWARRELOriginal.Code, declaration.JE_MessageStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendWarrelMessage();
			AssertEquals("Invalid Op Text", "It is not possible to send this message as another person in your company or a batch processor has changed the message status.", messageInitiator2.InvalidOperationText);
		}

		[TestDate(2007, 6, 24)]
		public void TestAnswerNoToSendWARRELWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelMessage();
			AssertEquals("MessageState", ZString.Empty, declaration.JE_MessageStatus);
		}

		[TestDate(2007, 6, 24)]
		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendWarrelWithdrawl();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("only be sent for Export Declarations") != -1);
				throw;
			}
		}

		[TestDate(2007, 6, 24)]
		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWhenNoOriginal()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("There is no message to withdraw") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingWARRELOriginal.Code;
			EDIMessage message1 = AddValidWARRELRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWhenDeclarationIsNotClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			EDIMessage message1 = AddValidWARRELRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Export Declaration is not clear") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWhenNoDeportID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			EDIMessage message1 = AddValidWARRELRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Bonded Warehouse and either a CFS or CTO must be specified") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWhenNoWarehouseID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			EDIMessage message1 = AddValidWARRELRToDeclaration(declaration);
			OrgAddress address1 = declaration.Factory.New<OrgAddress>();
			OrgHeader header1 = declaration.Factory.New<OrgHeader>();
			address1.OA_OH = header1.PK;
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = address1.PK;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Bonded Warehouse and either a CFS or CTO must be specified") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRELWithdrawlWhenNoEstimatedPickUpDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			EDIMessage message1 = AddValidWARRELRToDeclaration(declaration);
			OrgAddress address1 = declaration.Factory.New<OrgAddress>();
			OrgHeader header1 = declaration.Factory.New<OrgHeader>();
			address1.OA_OH = header1.PK;
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = address1.PK;
			OrgAddress address2 = declaration.Factory.New<OrgAddress>();
			OrgHeader header2 = declaration.Factory.New<OrgHeader>();
			address2.OA_OH = header2.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = address2.PK;
			header2.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "2222Z");
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarrelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Estimated Pickup Date and Time is required") != -1);
				throw;
			}
		}

		[TestDate(2007, 6, 24)]
		public void TestSendWARRELWithdrawl()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelWithdrawl();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRELWithdrawal.Code, declaration.JE_MessageStatus);
		}

		[TestDate(2007, 6, 24)]
		public void TestAnswerYesToSendWARRELWithdrawlWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelWithdrawl();
			AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRELWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendWARRELWithdrawlTwice()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRELAmendment();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelWithdrawl();
			AssertEquals("MessageState", CustomsEntryStatus.AwaitingWARRELWithdrawal.Code, declaration.JE_MessageStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendWarrelWithdrawl();
			AssertEquals("Invalid Op Text", "It is not possible to send this message as another person in your company or a batch processor has changed the message status.", messageInitiator2.InvalidOperationText);
		}

		[TestDate(2007, 6, 24)]
		public void TestAnswerNoToSendWARRELWithdrawlWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelWithdrawl();
			AssertEquals("MessageState", ZString.Empty, declaration.JE_MessageStatus);
		}

		[TestDate(2007, 6, 24)]
		public void TestWARRELShouldSendMessagesInTestMode()
		{
			var declaration = PrepareDeclarationReadyForWARREL();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelMessage();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			Env.Registry.CMRTestMode = true;
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelMessage();
			Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		[TestDate(2007, 6, 24)]
		public void TestWARRELWithdrawlShouldSendMessagesInTestMode()
		{
			var declaration = PrepareDeclarationReadyForWARRELAmendment();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelWithdrawl();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			Env.Registry.CMRTestMode = true;
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarrelWithdrawl();
			Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRETWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendWarretMessage();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRETWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarretMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("only be sent for Export Declarations") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRETWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingWARRETOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarretMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRETWhenDeclarationIsNotClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarretMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Export Declaration is not clear") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendWARRETWhenNoWarehouseID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendWarretMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("The Bonded Warehouse must be specified") != -1);
				throw;
			}
		}

		public void TestSendWARRETOriginal()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRET();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarretMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRETOriginal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendWARRETReplacement()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRETAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarretMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRETReplacement.Code, declaration.JE_MessageStatus);
		}

		public void TestAnswerYesToSendWARRETWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRET();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarretMessage();
			AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingWARRETOriginal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendWARRETTwice()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRET();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarretMessage();
			AssertEquals("MessageState", CustomsEntryStatus.AwaitingWARRETOriginal.Code, declaration.JE_MessageStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendWarretMessage();
			AssertEquals("Invalid Op Text", "It is not possible to send this message as another person in your company or a batch processor has changed the message status.", messageInitiator2.InvalidOperationText);
		}

		public void TestAnswerNoToSendWARRETWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRET();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarretMessage();
			AssertEquals("MessageState", ZString.Empty, declaration.JE_MessageStatus);
		}

		public void TestWARRETShouldSendMessagesInTestMode()
		{
			var declaration = PrepareDeclarationReadyForWARRET();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarretMessage();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			Env.Registry.CMRTestMode = true;
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendWarretMessage();
			Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendDeprecMessage();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("only be sent for Export Declarations") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingDEPRECOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWhenDeclarationIsNotClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Export Declaration is not clear") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWhenNoDeportID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("The Deport (CFS) must be specified") != -1);
				throw;
			}
		}

		public void TestSendDEPRECOriginal()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREC();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRECOriginal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendDEPRECReplacement()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRECAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRECReplacement.Code, declaration.JE_MessageStatus);
		}

		public void TestAnswerYesToSendDEPRECWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREC();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecMessage();
			AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRECOriginal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendDEPRECTwice()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREC();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecMessage();
			AssertEquals("MessageState", CustomsEntryStatus.AwaitingDEPRECOriginal.Code, declaration.JE_MessageStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendDeprecMessage();
			AssertEquals("Invalid Op Text", "It is not possible to send this message as another person in your company or a batch processor has changed the message status.", messageInitiator2.InvalidOperationText);
		}

		public void TestAnswerNoToSendDEPRECWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREC();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecMessage();
			AssertEquals("MessageState", ZString.Empty, declaration.JE_MessageStatus);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWithdrawlWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendDeprecWithdrawl();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWithdrawlWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("only be sent for Export Declarations") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWithdrawlWhenNoOriginal()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREC();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("There is no message to withdraw") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWithdrawlWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingDEPRECOriginal.Code;
			EDIMessage message1 = AddValidDEPRECRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWithdrawlWhenDeclarationIsNotClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			EDIMessage message1 = AddValidDEPRECRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Export Declaration is not clear") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRECWithdrawlWhenNoDeportID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			EDIMessage message1 = AddValidDEPRECRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprecWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("The Deport (CFS) must be specified") != -1);
				throw;
			}
		}

		public void TestSendDEPRECWithdrawl()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRECAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecWithdrawl();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRECWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestAnswerYesToSendDEPRECWithdrawlWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRECAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecWithdrawl();
			AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRECWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendDEPRECWithdrawlTwice()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRECAmendment();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecWithdrawl();
			AssertEquals("MessageState", CustomsEntryStatus.AwaitingDEPRECWithdrawal.Code, declaration.JE_MessageStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendDeprecWithdrawl();
			AssertEquals("Invalid Op Text", "It is not possible to send this message as another person in your company or a batch processor has changed the message status.", messageInitiator2.InvalidOperationText);
		}

		public void TestAnswerNoToSendDEPRECWithdrawlWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRECAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecWithdrawl();
			AssertEquals("MessageState", ZString.Empty, declaration.JE_MessageStatus);
		}

		public void TestDEPRECShouldSendMessagesInTestMode()
		{
			var declaration = PrepareDeclarationReadyForDEPREC();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecMessage();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			Env.Registry.CMRTestMode = true;
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecMessage();
			Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		public void TestDEPRECWithdrawlShouldSendMessagesInTestMode()
		{
			var declaration = PrepareDeclarationReadyForDEPRECAmendment();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecWithdrawl();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			Env.Registry.CMRTestMode = true;
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprecWithdrawl();
			Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendDeprelMessage();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("only be sent for Export Declarations") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingDEPRELOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWhenDeclarationIsNotClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Export Declaration is not clear") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWhenNoDeportID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("The Deport (CFS) must be specified") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWhenNoCTOID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1 = declaration.Factory.New<OrgAddress>();
			OrgHeader header1 = declaration.Factory.New<OrgHeader>();
			address1.OA_OH = header1.PK;
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.DepotDocAddress.E2_OA_Address = address1.PK;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelMessage();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("The CTO must be specified") != -1);
				throw;
			}
		}

		public void TestSendDEPRELOriginal()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRELOriginal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendDEPRELReplacement()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelMessage();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRELReplacement.Code, declaration.JE_MessageStatus);
		}

		public void TestAnswerYesToSendDEPRELWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelMessage();
			AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRELOriginal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendDEPRELTwice()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREL();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelMessage();
			AssertEquals("MessageState", CustomsEntryStatus.AwaitingDEPRELOriginal.Code, declaration.JE_MessageStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendDeprelMessage();
			AssertEquals("Invalid Op Text", "It is not possible to send this message as another person in your company or a batch processor has changed the message status.", messageInitiator2.InvalidOperationText);
		}

		public void TestAnswerNoToSendDEPRELWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelMessage();
			AssertEquals("MessageState", ZString.Empty, declaration.JE_MessageStatus);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithdrawlWithNoCustomsRegistrationNumber()
		{
			ZString oldCusRegNo = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			try
			{
				Env.Registry.AUCustomsSenderID = ZString.Empty;
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.MessageInitiator = messageInitiator;
				try
				{
					declaration.SendDeprelWithdrawl();
				}
				catch (ApplicationException e)
				{
					Assert("CorrectError", e.Message.IndexOf("Customs Registration Number") != -1);
					throw;
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldCusRegNo;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithdrawlWhenModeIsImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("only be sent for Export Declarations") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithdrawlWhenNoOriginal()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREL();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("There is no message to withdraw") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithdrawlWhenWaitingForResponse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingDEPRELOriginal.Code;
			EDIMessage message1 = AddValidDEPRELRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("waiting for responses") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithdrawlWhenDeclarationIsNotClear()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.FailOriginal.Code;
			EDIMessage message1 = AddValidDEPRELRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("Export Declaration is not clear") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithdrawlWhenNoDeportID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			EDIMessage message1 = AddValidDEPRELRToDeclaration(declaration);
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("The Deport (CFS) must be specified") != -1);
				throw;
			}
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestSendDEPRELWithdrawlWhenNoCTOID()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1 = declaration.Factory.New<OrgAddress>();
			OrgHeader header1 = declaration.Factory.New<OrgHeader>();
			address1.OA_OH = header1.PK;
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.DepotDocAddress.E2_OA_Address = address1.PK;
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			try
			{
				declaration.SendDeprelWithdrawl();
			}
			catch (ApplicationException e)
			{
				Assert("CorrectError", e.Message.IndexOf("The CTO must be specified") != -1);
				throw;
			}
		}

		public void TestSendDEPRELWithdrawl()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelWithdrawl();
			Assert("Message Sent", declaration.Messages.Count > 0);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRELWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestAnswerYesToSendDEPRELWithdrawlWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelWithdrawl();
			AssertEquals("Message Inititator Text", "The declaration has message errors. Customs will almost certainly reject the message.  Are you sure you want to continue?", messageInitiator.ContinueWithActionMessage);
			AssertEquals("DeclarationState", CustomsEntryStatus.AwaitingDEPRELWithdrawal.Code, declaration.JE_MessageStatus);
		}

		public void TestSendDEPRELWithdrawlTwice()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRELAmendment();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;
			JobDeclaration secondFactoryDec = secondFactory.Load<JobDeclaration>(declaration.PK);

			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelWithdrawl();
			AssertEquals("MessageState", CustomsEntryStatus.AwaitingDEPRELWithdrawal.Code, declaration.JE_MessageStatus);

			SendsMessagesToCustomsShutterUpperer messageInitiator2 = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator2.ThrowExceptionOnInvalidOperation = false;
			secondFactoryDec.MessageInitiator = messageInitiator2;
			secondFactoryDec.SendDeprelWithdrawl();
			AssertEquals("Invalid Op Text", "It is not possible to send this message as another person in your company or a batch processor has changed the message status.", messageInitiator2.InvalidOperationText);
		}

		public void TestAnswerNoToSendDEPRELWithdrawlWithMessageErrors()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPRELAmendment();
			SendsMessagesToCustomsShutterUpperer messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelWithdrawl();
			AssertEquals("MessageState", ZString.Empty, declaration.JE_MessageStatus);
		}

		public void TestDEPRELShouldSendMessagesInTestMode()
		{
			var declaration = PrepareDeclarationReadyForDEPREL();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelMessage();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			Env.Registry.CMRTestMode = true;
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelMessage();
			Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		public void TestDEPRELWithdrawlShouldSendMessagesInTestMode()
		{
			var declaration = PrepareDeclarationReadyForDEPRELAmendment();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelWithdrawl();
			Assert(!messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));

			Env.Registry.CMRTestMode = true;
			messageInitiator.AnswerToContinueWithAction = true;
			declaration.JE_MessageStatus = CustomsEntryStatus.NotSent.Code;
			declaration.MessageInitiator = messageInitiator;
			declaration.SendDeprelWithdrawl();
			Assert(messageInitiator.ContinueWithActionMessage.Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		[TestDate(2007, 6, 24)]
		public void TestIsWARRELMessageLodged()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARREL();
			AssertIsWARRELMessageLodged("Not lodged yet", false, declaration);
			EDIMessage message0 = AddValidWARRETRToDeclaration(declaration);
			AssertIsWARRELMessageLodged("Still not lodged, no WARREL reply exists yet", false, declaration);
			EDIMessage message1 = AddValidWARRELRToDeclaration(declaration);
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 10, 0, 0);
			AssertIsWARRELMessageLodged("Now lodged", true, declaration);
			message1.EM_Status = EDIMessage.Status.Discarded;
			AssertIsWARRELMessageLodged("Now not lodged, message discarded", false, declaration);
			message1.EM_Status = EDIMessage.Status.Received;
			AssertIsWARRELMessageLodged("Now lodged again", true, declaration);
			message1.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsWARRELMessageLodged("Now not lodged, message rejected", false, declaration);
			message1.EM_MessageSubType = "CLR";
			AssertIsWARRELMessageLodged("Now lodged again 2", true, declaration);
			EDIMessage message2 = AddValidWARRELRToDeclaration(declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 11, 0, 0);
			message2.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsWARRELMessageLodged("Latter message rejected, but still lodged", true, declaration);
			message2.EM_MessageSubType = "CLR";
			AssertIsWARRELMessageLodged("Latter message accepted, still lodged", true, declaration);
			message2.EM_MessageSubType = EDIMessage.Status.Withdrawn;
			AssertIsWARRELMessageLodged("Now not lodged, latter withdrawl", false, declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 9, 0, 0);
			AssertIsWARRELMessageLodged("Now lodged, withdrawl is before last accepted", true, declaration);
		}

		void AssertIsWARRELMessageLodged(string text, bool expectedResult, JobDeclaration declaration)
		{
			declaration.ResetfLastWarrelReplyForTesting();
			AssertEquals(text, expectedResult, declaration.IsWARRELMessageLodged);
		}

		public void TestIsWARRETMessageLodged()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRET();
			AssertIsWARRETMessageLodged("Not lodged yet", false, declaration);
			EDIMessage message0 = AddValidWARRELRToDeclaration(declaration);
			AssertIsWARRETMessageLodged("Still not lodged, no WARRET reply exists yet", false, declaration);
			EDIMessage message1 = AddValidWARRETRToDeclaration(declaration);
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 10, 0, 0);
			AssertIsWARRETMessageLodged("Now lodged", true, declaration);
			message1.EM_Status = EDIMessage.Status.Discarded;
			AssertIsWARRETMessageLodged("Now not lodged, message discarded", false, declaration);
			message1.EM_Status = EDIMessage.Status.Received;
			AssertIsWARRETMessageLodged("Now lodged again", true, declaration);
			message1.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsWARRETMessageLodged("Now not lodged, message rejected", false, declaration);
			message1.EM_MessageSubType = "CLR";
			AssertIsWARRETMessageLodged("Now lodged again 2", true, declaration);
			EDIMessage message2 = AddValidWARRETRToDeclaration(declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 11, 0, 0);
			message2.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsWARRETMessageLodged("Latter message rejected, but still lodged", true, declaration);
			message2.EM_MessageSubType = "CLR";
			AssertIsWARRETMessageLodged("Latter message accepted, still lodged", true, declaration);
			message2.EM_MessageSubType = EDIMessage.Status.Withdrawn;
			AssertIsWARRETMessageLodged("Now not lodged, latter withdrawl", false, declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 9, 0, 0);
			AssertIsWARRETMessageLodged("Now lodged, withdrawl is before last accepted", true, declaration);
		}

		void AssertIsWARRETMessageLodged(string text, bool expectedResult, JobDeclaration declaration)
		{
			declaration.ResetfLastWarretReplyForTesting();
			AssertEquals(text, expectedResult, declaration.IsWARRETMessageLodged);
		}

		public void TestIsDEPRECMessageLodged()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREC();
			AssertIsDEPRECMessageLodged("Not lodged yet", false, declaration);
			EDIMessage message0 = AddValidDEPRELRToDeclaration(declaration);
			AssertIsDEPRECMessageLodged("Still not lodged, no DEPREC reply exists yet", false, declaration);
			EDIMessage message1 = AddValidDEPRECRToDeclaration(declaration);
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 10, 0, 0);
			AssertIsDEPRECMessageLodged("Now lodged", true, declaration);
			message1.EM_Status = EDIMessage.Status.Discarded;
			AssertIsDEPRECMessageLodged("Now not lodged, message discarded", false, declaration);
			message1.EM_Status = EDIMessage.Status.Received;
			AssertIsDEPRECMessageLodged("Now lodged again", true, declaration);
			message1.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsDEPRECMessageLodged("Now not lodged, message rejected", false, declaration);
			message1.EM_MessageSubType = "CLR";
			AssertIsDEPRECMessageLodged("Now lodged again 2", true, declaration);
			EDIMessage message2 = AddValidDEPRECRToDeclaration(declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 11, 0, 0);
			message2.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsDEPRECMessageLodged("Latter message rejected, but still lodged", true, declaration);
			message2.EM_MessageSubType = "CLR";
			AssertIsDEPRECMessageLodged("Latter message accepted, still lodged", true, declaration);
			message2.EM_MessageSubType = EDIMessage.Status.Withdrawn;
			AssertIsDEPRECMessageLodged("Now not lodged, latter withdrawl", false, declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 9, 0, 0);
			AssertIsDEPRECMessageLodged("Now lodged, withdrawl is before last accepted", true, declaration);
		}

		void AssertIsDEPRECMessageLodged(string text, bool expectedResult, JobDeclaration declaration)
		{
			declaration.ResetfLastDeprecReplyForTesting();
			AssertEquals(text, expectedResult, declaration.IsDEPRECMessageLodged);
		}

		public void TestIsDEPRELMessageLodged()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREL();
			AssertIsDEPRELMessageLodged("Not lodged yet", false, declaration);
			EDIMessage message0 = AddValidDEPRECRToDeclaration(declaration);
			AssertIsDEPRELMessageLodged("Still not lodged, no DEPREL reply exists yet", false, declaration);
			EDIMessage message1 = AddValidDEPRELRToDeclaration(declaration);
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 10, 0, 0);
			AssertIsDEPRELMessageLodged("Now lodged", true, declaration);
			message1.EM_Status = EDIMessage.Status.Discarded;
			AssertIsDEPRELMessageLodged("Now not lodged, message discarded", false, declaration);
			message1.EM_Status = EDIMessage.Status.Received;
			AssertIsDEPRELMessageLodged("Now lodged again", true, declaration);
			message1.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsDEPRELMessageLodged("Now not lodged, message rejected", false, declaration);
			message1.EM_MessageSubType = "CLR";
			AssertIsDEPRELMessageLodged("Now lodged again 2", true, declaration);
			EDIMessage message2 = AddValidDEPRELRToDeclaration(declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 11, 0, 0);
			message2.EM_MessageSubType = EDIMessage.Status.Rejected;
			AssertIsDEPRELMessageLodged("Latter message rejected, but still lodged", true, declaration);
			message2.EM_MessageSubType = "CLR";
			AssertIsDEPRELMessageLodged("Latter message accepted, still lodged", true, declaration);
			message2.EM_MessageSubType = EDIMessage.Status.Withdrawn;
			AssertIsDEPRELMessageLodged("Now not lodged, latter withdrawl", false, declaration);
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2007, 1, 1, 9, 0, 0);
			AssertIsDEPRELMessageLodged("Now lodged, withdrawl is before last accepted", true, declaration);
		}

		void AssertIsDEPRELMessageLodged(string text, bool expectedResult, JobDeclaration declaration)
		{
			declaration.ResetfLastDeprelReplyForTesting();
			AssertEquals(text, expectedResult, declaration.IsDEPRELMessageLodged);
		}

		public void TestSendingManualREXAmendmentValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.NonConfirming;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			var exDocHeader = declaration.QuarantineInvoice?.QuarantineExDocHeader;

			Assert("QuarantineExDocHeader exists", exDocHeader != null);

			var message = exDocHeader.Messages.AddNew();
			exDocHeader.AddInfo.ZH_AmendmentResponseStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Messages sent", declaration.DeclarationMessagesHaveBeenSent());
			AssertEquals("QuarantineExDocHeader has sent a manual amendment and is waiting a response.", true, declaration.PendingManualAmendmentResponse);

			exDocHeader.AddInfo.ZH_AmendmentResponseStatus = "";
			AssertEquals("No responses pending", false, declaration.PendingManualAmendmentResponse);
		}

		#region Implementation

		protected JobDeclaration PrepareDeclarationReadyForWARREL()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1;
			var header1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			if (header1.Addresses != null && header1.Addresses.Count > 0)
			{
				address1 = header1.Addresses[0];
			}
			else
			{
				address1 = Factory.New<OrgAddress>();
				address1.OA_OH = header1.PK;
			}
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = address1.PK;
			OrgAddress address2;
			var header2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, header1.PK));
			if (header2.Addresses != null && header2.Addresses.Count > 0)
			{
				address2 = header2.Addresses[0];
			}
			else
			{
				address2 = Factory.New<OrgAddress>();
				address2.OA_OH = header2.PK;
			}
			declaration.WarehouseDocAddress.E2_OA_Address = address2.PK;
			header2.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "2222Z");
			declaration.JE_EstimatedDeliveryOrPickup = new ZDateTime(2007, 6, 24, 22, 29, 0);
			return declaration;
		}

		protected JobDeclaration PrepareDeclarationReadyForWARRELAmendment()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARREL();
			EDIMessage message1 = AddValidWARRELRToDeclaration(declaration);
			return declaration;
		}

		protected EDIMessage AddValidWARRELRToDeclaration(JobDeclaration declaration)
		{
			EDIMessage message = declaration.Messages.AddNew();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.WARREL;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = "CLR";
			return message;
		}

		protected JobDeclaration PrepareDeclarationReadyForWARRET()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1;
			var header1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			if (header1.Addresses != null && header1.Addresses.Count > 0)
			{
				address1 = header1.Addresses[0];
			}
			else
			{
				address1 = Factory.New<OrgAddress>();
				address1.OA_OH = header1.PK;
			}
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.WarehouseDocAddress.E2_OA_Address = address1.PK;
			return declaration;
		}

		protected JobDeclaration PrepareDeclarationReadyForWARRETAmendment()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForWARRET();
			EDIMessage message1 = AddValidWARRETRToDeclaration(declaration);
			return declaration;
		}

		protected EDIMessage AddValidWARRETRToDeclaration(JobDeclaration declaration)
		{
			EDIMessage message = declaration.Messages.AddNew();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.WARRET;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = "CLR";
			return message;
		}

		protected JobDeclaration PrepareDeclarationReadyForDEPREC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1;
			var header1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			if (header1.Addresses != null && header1.Addresses.Count > 0)
			{
				address1 = header1.Addresses[0];
			}
			else
			{
				address1 = Factory.New<OrgAddress>();
				address1.OA_OH = header1.PK;
			}
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.DepotDocAddress.E2_OA_Address = address1.PK;
			return declaration;
		}

		protected JobDeclaration PrepareDeclarationReadyForDEPRECAmendment()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREC();
			EDIMessage message1 = AddValidDEPRECRToDeclaration(declaration);
			return declaration;
		}

		protected EDIMessage AddValidDEPRECRToDeclaration(JobDeclaration declaration)
		{
			EDIMessage message = declaration.Messages.AddNew();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.DEPREC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = "CLR";
			return message;
		}

		protected JobDeclaration PrepareDeclarationReadyForDEPREL()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			OrgAddress address1;
			var header1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			if (header1.Addresses != null && header1.Addresses.Count > 0)
			{
				address1 = header1.Addresses[0];
			}
			else
			{
				address1 = Factory.New<OrgAddress>();
				address1.OA_OH = header1.PK;
			}
			header1.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1111Z");
			declaration.DepotDocAddress.E2_OA_Address = address1.PK;
			OrgAddress address2;
			var header2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, header1.PK));
			if (header2.Addresses != null && header2.Addresses.Count > 0)
			{
				address2 = header2.Addresses[0];
			}
			else
			{
				address2 = Factory.New<OrgAddress>();
				address2.OA_OH = header2.PK;
			}
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = address2.PK;
			header2.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "2222Z");
			return declaration;
		}

		protected JobDeclaration PrepareDeclarationReadyForDEPRELAmendment()
		{
			JobDeclaration declaration = PrepareDeclarationReadyForDEPREL();
			EDIMessage message1 = AddValidDEPRELRToDeclaration(declaration);
			return declaration;
		}

		protected EDIMessage AddValidDEPRELRToDeclaration(JobDeclaration declaration)
		{
			EDIMessage message = declaration.Messages.AddNew();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.DEPREL;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = "CLR";
			return message;
		}

		#endregion
	}
}
