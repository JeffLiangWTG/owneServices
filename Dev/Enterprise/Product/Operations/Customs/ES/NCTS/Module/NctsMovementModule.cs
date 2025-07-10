using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.Module
{
	public class NctsMovementModule : EU.NCTS.Module.NctsMovementModule
	{
		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NctsMovementFilterStripBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem(ResString.GetMultilingualString("29B713A4-FC62-4F84-B193-353DAE665FAE", "Guarantee – Check transit status"), CheckTransitStatusAction));

			return result.ToArray();
		}

		void CheckTransitStatusAction(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
				if (nctsSettings.IsUsingPhase5(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					CheckTransitStatusWhenAddIsPhase5();
				}
				else
				{
					CheckTransitStatusWhenAddIsPhase4();
				}
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("6AEF8F22-5A9D-4020-AAE6-3187BF9228C9", "Please select at least one movement"));
			}
		}

		void CheckTransitStatusWhenAddIsPhase5()
		{
			var messagesSent = 0;
			var declarationsWithoutBroker = new List<ZString>();
			foreach (NctsHeader header in SelectedBusinessObjects)
			{
				var result = header.SendTQUForGuaranteeWriteOffIfPossible();
				header.Factory.Save();
				if (result.MessageSentCorrectly)
				{
					messagesSent++;
				}
				declarationsWithoutBroker.Add(result.DeclarationWithBrokerError);
			}

			ShowResults(messagesSent, declarationsWithoutBroker.Distinct());
		}

		void ShowResults(int messagesSent, IEnumerable<ZString> declarationsWithoutBroker)
		{
			if (messagesSent > 0)
			{
				Globals.Message.Show(GetMessageSentSuccessfulMessage(messagesSent));
			}
			else if (declarationsWithoutBroker.Any(x => !x.IsEmpty))
			{
				Globals.Message.ShowError(Res.GetString("DC46DFB7-319C-4F9A-A24F-6ECAAB836118", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate in the Details tab in the following declarations: {0}.", string.Join(", ", declarationsWithoutBroker.Where(x => !x.IsEmpty))));
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("26277110-C6F8-4F09-881E-613F6B3FD9A4", "None of the selected movements match criteria to check accounting status"));
			}
		}

		ZString GetMessageSentSuccessfulMessage(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("100C512F-3452-4754-B112-2E9906951978", "{0} Messages sent successfully.", numberOfMessages) : Res.GetString("001BE167-74A9-427C-80A6-58206A1E4CA8", "{0} Message sent successfully.", numberOfMessages);

		void CheckTransitStatusWhenAddIsPhase4()
		{
			var writeOffResultCollection = new ES.Business.WriteOffResultCollection(Factory);

			foreach (NctsHeader header in SelectedBusinessObjects)
			{
				writeOffResultCollection.Add(header.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded());
				header.Factory.Save();
			}

			ShowTransitResultsGrid(writeOffResultCollection);
		}

		protected virtual void ShowTransitResultsGrid(ES.Business.WriteOffResultCollection writeOffResultCollection)
		{
			var resultsForm = new ShowResultsGridForm(writeOffResultCollection);
			resultsForm.ShowDialog();
		}
	}
}
