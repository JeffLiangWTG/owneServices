using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	public class RNSSendsMessagesToCustomsGUI : SendsMessagesToCustomsGUI
	{
		public RNSSendsMessagesToCustomsGUI(IRNSRequestParent rnsRequestParent)
			: base()
		{
			Argument.NotNull(rnsRequestParent, "rnsRequestParent");

			shouldManagerDefaultToSendDelegate = new MessageChooserNonPersistent.ShouldManagerDefaultToSendDelegate(
				(manager) => rnsRequestParent.ShouldDefaultChooseToSent(manager));
		}

		readonly MessageChooserNonPersistent.ShouldManagerDefaultToSendDelegate shouldManagerDefaultToSendDelegate;

		protected override MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, CargoWise.Types.ZString question)
		{
			var chooser = base.CreateMessageChooser(managers, question);
			chooser.SelectDefaultToSendManagers(shouldManagerDefaultToSendDelegate);

			return chooser;
		}

		protected override MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, CargoWise.Types.ZString question, CargoWise.Types.ZString action)
		{
			var chooser = base.CreateMessageChooser(managers, question, action);
			chooser.SelectDefaultToSendManagers(shouldManagerDefaultToSendDelegate);

			return chooser;
		}

		protected override SingleMessageManager[] GetManagers(MessageChooserNonPersistent chooser)
		{
			var messageManagers = base.GetManagers(chooser);
			return ProcessAndGetManagers(messageManagers);
		}

		protected SingleMessageManager[] ProcessAndGetManagers(SingleMessageManager[] messageManagers)
		{
			if (messageManagers.Length > 0)
			{
				if (((RNSMessageManager)messageManagers[0]).DataWrapper is RNSRequestBO)
				{
					RNSRequestBO dummyRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, messageManagers[0].BusinessObject.Factory, false, false);
					dummyRequestBO.OfficeCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.CustomsOfficeCode).SubstringSafe(0, 4);
					var dialog = new RNSRequestForm(dummyRequestBO);
					dialog.AutoSendMessage = false;
					if (ZFormModaliser.ShowDialogAndDispose(dialog) == DialogResult.Cancel)
					{
						return Array.Empty<SingleMessageManager>();
					}
					else
					{
						foreach (RNSMessageManager manager in messageManagers)
						{
							var rnsRequestBO = (RNSRequestBO)manager.DataWrapper;
							if (rnsRequestBO != null)
							{
								rnsRequestBO.DateOfArrival = dummyRequestBO.DateOfArrival;
								rnsRequestBO.OfficeCode = dummyRequestBO.OfficeCode;
								rnsRequestBO.SubLocationCode = dummyRequestBO.SubLocationCode;
							}
						}
					}
				}
			}
			return messageManagers;
		}
	}
}
