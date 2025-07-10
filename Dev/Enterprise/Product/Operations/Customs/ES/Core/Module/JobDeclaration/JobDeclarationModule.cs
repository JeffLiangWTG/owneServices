using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem(ResString.GetMultilingualString("51B8817A-2F63-4DB7-93BB-A5C86D7A5569", "Guarantee – Check accounting status"), CheckAccountingStatusAction));

			return result.ToArray();
		}

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		void CheckAccountingStatusAction(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				var messagesSent = 0;
				var declarationsWithoutBroker = new List<ZString>();
				foreach (JobDeclaration declaration in SelectedBusinessObjects)
				{
					foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
					{
						var result = entryHeader.SendIQUForGuaranteeWriteOffIfPossible();
						entryHeader.Factory.Save();
						if (result.MessageSentCorrectly)
						{
							messagesSent++;
						}
						declarationsWithoutBroker.Add(result.DeclarationWithBrokerError);
					}
				}

				ShowResults(messagesSent, declarationsWithoutBroker.Distinct());
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("FC30BC2A-7482-4D7E-93F3-22F8A5BEE121", "Please select at least one declaration"));
			}
		}

		void ShowResults(int messagesSent, IEnumerable<ZString> declarationsWithoutBroker)
		{
			if (messagesSent > 0)
			{
				Globals.Message.Show(GetMessageSentSuccessfulMessage(messagesSent));
			}
			else if (declarationsWithoutBroker.Any(x => !x.IsEmpty))
			{
				Globals.Message.ShowError(Res.GetString("82EC932A-5140-491B-858F-AFD927D923B7", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options in the following declarations: {0}.", string.Join(", ", declarationsWithoutBroker.Where(x => !x.IsEmpty))));
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("B3EEFAF3-3CBF-49F8-9168-2EF2222BA5FB", "No entries in declarations selected match criteria to check accounting status"));
			}
		}

		ZString GetMessageSentSuccessfulMessage(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("159AAC37-CA6F-4375-A8B1-9B10CA4A8666", "{0} Messages sent successfully.", numberOfMessages) : Res.GetString("9F275D1E-F72E-4262-8D1E-F6F4B14DCBA7", "{0} Message sent successfully.", numberOfMessages);
	}
}
