using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	sealed class RNSSendsMessagesToCustomsGUITest : TestCaseWithFactory
	{
		public void TestCreateMessageChooser()
		{
			var rnsRequestParent = new DummyRNSRequestParent(Factory.New<TallyContainer>());
			rnsRequestParent.ShouldDefaultChooseToSentForTesting = true;

			var sender = new RNSSendsMessagesToCustomsGUIForTesting(rnsRequestParent);
			var singleMessageManagers = createSingleManager(rnsRequestParent).ToArray();

			var chooser = sender.CreateMessageChooser_Exposed(singleMessageManagers, "Query");

			AssertEquals("There should be choose 2 managers selected to sent", 2, chooser.SelectedManagers.Length);
			AssertCollectionContains("The 1st MessageManager should be selected", singleMessageManagers[0], chooser.SelectedManagers);
			AssertCollectionContains("The 2nd MessageManager should be selected", singleMessageManagers[1], chooser.SelectedManagers);

			rnsRequestParent.ShouldDefaultChooseToSentForTesting = false;
			chooser = sender.CreateMessageChooser_Exposed(singleMessageManagers.ToArray(), "Query");

			AssertEquals("There should be no manager selected to sent", 0, chooser.SelectedManagers.Length);
		}

		public void TestGetManagers()
		{
			var rnsRequestParent = new DummyRNSRequestParent(Factory.New<TallyContainer>());
			rnsRequestParent.ShouldDefaultChooseToSentForTesting = true;

			var sender = new RNSSendsMessagesToCustomsGUIForTesting(rnsRequestParent);
			var singleMessageManagers = createSingleManager(rnsRequestParent).ToArray();

			var chooser = sender.CreateMessageChooser_Exposed(singleMessageManagers, "Query");
			var messageManagers = sender.GetManagers_Exposed(chooser);
			var dataWrapper = ((RNSMessageManager)messageManagers[0]).DataWrapper;

			var requestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, messageManagers[0].BusinessObject.Factory, false, false);
			AssertEquals("OfficeCode should be set", requestBO.OfficeCode, dataWrapper.OfficeCode);
			AssertEquals("SubLocationCode should be set", requestBO.SubLocationCode, dataWrapper.SubLocationCode);
		}

		IEnumerable<SingleMessageManager> createSingleManager(IRNSRequestParent rnsRequestParent)
		{
			foreach (IRNSRequest rnsRequest in rnsRequestParent.GetRNSRequestCollections())
			{
				yield return new RNSMessageManager(rnsRequest, rnsRequestParent.Notification, rnsRequestParent.IsStatusQuery);
			}
		}

		sealed class DummyRNSRequestParent : IRNSRequestParent
		{
			public DummyRNSRequestParent(TallyContainer container)
			{
				parent = container;
			}
			readonly TallyContainer parent;

			public Customs.Business.MessageManagers.IUserNotification Notification
			{
				get { return null; }
				set { }
			}

			public bool IsStatusQuery
			{
				get;
				set;
			}

			public CargoWise.EntityFramework.BusinessObject ParentBusinessObject => parent;

			public IEnumerable<IRNSRequest> GetRNSRequestCollections()
			{
				var messageingBOs = new List<RNSMessagingBO>();
				messageingBOs.Add(new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(parent.PackUnpackShipments.AddNew())));
				messageingBOs.Add(new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(parent.PackUnpackShipments.AddNew())));

				return messageingBOs;
			}

			public bool ShouldDefaultChooseToSent(SingleMessageManager messageManager) => ShouldDefaultChooseToSentForTesting;

			public bool ShouldDefaultChooseToSentForTesting;

			public bool ShouldWaitUntilResponded => true;

			public IMessageManager GetMessageManagerForAmendmentDetection() => throw new NotImplementedException();

			public bool IsInAStatusAmendmentSendable => throw new NotImplementedException();

			public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue() => throw new NotImplementedException();
		}

		sealed class RNSSendsMessagesToCustomsGUIForTesting : RNSSendsMessagesToCustomsGUI
		{
			public RNSSendsMessagesToCustomsGUIForTesting(IRNSRequestParent rnsRequestParent)
				: base(rnsRequestParent)
			{
			}

			internal MessageChooserNonPersistent CreateMessageChooser_Exposed(SingleMessageManager[] managers, CargoWise.Types.ZString question)
				=> CreateMessageChooser(managers, question);

			internal SingleMessageManager[] GetManagers_Exposed(MessageChooserNonPersistent chooser)
			{
				var messageManager = Array.Empty<SingleMessageManager>();
				using (var dialog = CreateMessageChooseDialog(chooser))
				{
					dialog.SendPressed = true;
					DataBoundControl.Get(CreateControlToBind(dialog)).SetDataBinding(chooser, "");
					ShowMessagesToSendDialogWithoutDispose(chooser, dialog);
					messageManager = dialog.SendPressed ? chooser.SelectedManagers : Array.Empty<SingleMessageManager>();
				}
				return ProcessAndGetManagers(messageManager);
			}
		}
	}
}
