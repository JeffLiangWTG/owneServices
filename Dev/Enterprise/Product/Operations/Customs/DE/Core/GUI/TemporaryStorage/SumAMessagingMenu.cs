using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class SumAMessagingMenu : ZMenuItem
	{
		public SumAMessagingMenu()
		{
			this.SetCaptionResourceString();
		}

		public CusTempStorageJobHeader Header
		{
			get => header;
			set
			{
				header = value;
				RefreshMenuItems();
			}
		}
		CusTempStorageJobHeader header;

		void RefreshMenuItems()
		{
			if (Header != null)
			{
				MenuItems.Clear();

				var finalSumAWithoutPreliminaryMenuItem = new ZMenuItem(ResString.GetMultilingualString("377c25da-71b8-4dbc-a6be-0199f05fb628", "Final SumA without Preliminary"));
				finalSumAWithoutPreliminaryMenuItem.Click += SendCUSPRL<FinalSumAWithoutPreliminarySender>;

				var finalSumAWithPreliminaryMenuItem = new ZMenuItem(ResString.GetMultilingualString("3e14446d-5365-4161-9f15-5ed514fa2ee7", "Final SumA with a Preliminary"), FinalSumAWithAPreliminaryClick);

				var preliminarySumAMenuItem = new ZMenuItem(ResString.GetMultilingualString("f6ac8432-2ef1-4429-a871-d64502f13198", "Preliminary SumA"));
				preliminarySumAMenuItem.Click += SendCUSPRL<PreliminarySumASender>;

				var amendmentOfPreliminarySumA = new ZMenuItem(ResString.GetMultilingualString("8213fd4c-fcca-48af-98ec-aeea25ea78e6", "Amendment of Preliminary SumA"));
				amendmentOfPreliminarySumA.Click += SendCUSPRL<AmendmentSumASender>;

				var sumADeclarationMenuItem = new ZMenuItem(ResString.GetMultilingualString("549474ae-16c2-4a0f-bd5b-27d12a094b83", "SumA Declaration"), new MenuItem[] { finalSumAWithoutPreliminaryMenuItem, finalSumAWithPreliminaryMenuItem, preliminarySumAMenuItem, amendmentOfPreliminarySumA });
				MenuItems.Add(sumADeclarationMenuItem);

				var changeCustodyInformationMenuItem = new ZMenuItem(ResString.GetMultilingualString("08D87D29-94F0-44C7-8EF2-C02D03BE3B35", "Change Custody Information"), ChangeCustodyInformationClick);
				var changeDisposalEntitledTraderMenuItem = new ZMenuItem(ResString.GetMultilingualString("58D8E4B9-F572-4505-9F65-B420E0902277", "Change Disposal Entitled Trader"), ChangeDisposalEntitledTraderClick);
				var changeOwnerReferenceMenuItem = new ZMenuItem(ResString.GetMultilingualString("43F4C29F-A924-4199-B103-B9D3497A44C9", "Change Owner Reference"), ChangeOwnerReferenceClick);
				var consolidationMenuItem = new ZMenuItem(ResString.GetMultilingualString("74B02A2C-3C9A-4B63-8CDF-8C44CE796F35", "Consolidation"), ConsolidationClick);
				var splitMenuItem = new ZMenuItem(ResString.GetMultilingualString("3854FF64-43CE-4429-ADF3-6503C7DF1165", "Split"), SplitClick);

				var amendmentsMenuItem = new ZMenuItem(ResString.GetMultilingualString("5BF27D63-2278-4D7D-8CE7-D6F89FDAF5AD", "Amendments"), new MenuItem[] { changeCustodyInformationMenuItem, changeDisposalEntitledTraderMenuItem, changeOwnerReferenceMenuItem, consolidationMenuItem, splitMenuItem });
				MenuItems.Add(amendmentsMenuItem);
			}
		}

		void SendCUSPRL<T>(object sender, EventArgs e)
			where T : TemporaryStorageSender
		{
			if (this.PreSaveMessage(Header))
			{
				try
				{
					var cusTempStorageDec = Header.CUSPRLCusTempStorageDec;
					if (Header.CanSendMessage(cusTempStorageDec))
					{
						var sumASender = (T)Activator.CreateInstance(typeof(T), cusTempStorageDec);
						var (canSend, whyCannotSend) = sumASender.CanSend;
						if (canSend)
						{
							sumASender.Send();
							Header.Factory.Save();
							Globals.Message.Show(MessagingMenuExtension.MessageHasBeenSent);
						}
						else
						{
							Globals.Message.Show(whyCannotSend);
						}
					}
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		void ChangeCustodyInformationClick(object sender, EventArgs e)
		{
			SendAmendmentMessage(Header.CHGTSTCusTempStorageDecs.Cast<CusTempStorageDec>(), Res.GetString("482E568F-BAD0-43C9-B6CB-7D81FBDBD305", "Change Custody Information"), (x) => new ChangeCustodyInformationSender(x));
		}

		void ChangeDisposalEntitledTraderClick(object sender, EventArgs e)
		{
			SendAmendmentMessage(Header.CHGOFFCusTempStorageDecs.Cast<CusTempStorageDec>(), Res.GetString("2A406889-F739-41A3-8DD7-0BE29FE19A3C", "Change Disposal Entitled Trader"), (x) => new ChangeDisposalEntitledTraderSender(x));
		}

		void ChangeOwnerReferenceClick(object sender, EventArgs e)
		{
			SendAmendmentMessage(Header.CHGSPOCusTempStorageDecs.Cast<CusTempStorageDec>(), Res.GetString("AAB36C67-4807-4D36-91F4-2D4644CA3909", "Change Owner Reference"), (x) => new ChangeOwnerReferenceSender(x));
		}

		void ConsolidationClick(object sender, EventArgs e)
		{
			SendAmendmentMessage(Header.PRLCONCusTempStorageDecs.Cast<CusTempStorageDec>(), Res.GetString("3CD2E4C3-A8EF-42D3-AEC3-E1A38144DE0B", "Consolidation"), (x) => new ConsolidationSender(x));
		}

		void SplitClick(object sender, EventArgs e)
		{
			SendAmendmentMessage(Header.CUSPCSCusTempStorageDecs.Cast<CusTempStorageDec>(), Res.GetString("2430485C-27EC-4C39-BB56-6AF267426766", "Split"), (x) => new SplitSender(x));
		}

		void FinalSumAWithAPreliminaryClick(object sender, EventArgs e)
		{
			SendFinalSumAWithPreliminaryMessage(Header.CUSPRLCusTempStorageDec, Res.GetString("99425AC3-C2B3-4500-85ED-7134F1CDA3C8", "Final SumA with a Preliminary"));
		}

		void SendAmendmentMessage(IEnumerable<CusTempStorageDec> decs, string messageType, Func<CusTempStorageDec, TemporaryStorageSender> createSendObject)
		{
			if (AreDeclarationsValid(decs, Res.GetString("10487481-8ce6-48ab-9706-cd33533ab0a8", "Amendments > {0}", messageType)) && this.PreSaveMessage(Header))
			{
				try
				{
					bool continueWithSend = false;
					var parent = new MessageSendingActionParent(Header, decs, x => ((CusTempStorageDec)x).STH_OwnerReferenceNumber, Env.Security.CustomsTemporaryStorageSendWithMessageErrors);
					using (var form = new MessageSendingForm<MessageSendingActionParent>(parent, messageType))
					{
						continueWithSend = GetMessageSendingFormDialogResult(form) == DialogResult.OK;
					}

					if (continueWithSend)
					{
						var cusTempStorageDec = (CusTempStorageDec)parent.SendingObjectsCollection.Cast<MessageSendingAction>().Single(x => x.ShouldSend).MessagingObject;
						if (cusTempStorageDec.CheckDeclarationStatusAndLines())
						{
							var sumASender = createSendObject(cusTempStorageDec);
							var (canSend, whyCannotSend) = sumASender.CanSend;
							if (canSend)
							{
								sumASender.Send();
								Header.Factory.Save();
								Globals.Message.Show(MessagingMenuExtension.MessageHasBeenSent);
							}
							else
							{
								Globals.Message.Show(whyCannotSend);
							}
						}
					}
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		void SendFinalSumAWithPreliminaryMessage(CusTempStorageDec dec, string messageType)
		{
			if (this.PreSaveMessage(Header))
			{
				try
				{
					if (dec.CheckDeclarationStatusAndLines())
					{
						bool continueWithSend = false;
						var parent = new FinalSumAWithAPreliminaryMessageSendingActionParent(Header, dec.CusTempStorageLines.Cast<CusTempStorageLine>().ToArray(), Env.Security.CustomsTemporaryStorageSendWithMessageErrors);
						using (var form = new MessageSendingForm<FinalSumAWithAPreliminaryMessageSendingActionParent>(parent, messageType, Res.GetString("B0F66D43-66C1-47A5-8546-BE44079E3AC7", "Lines to be sent")))
						{
							continueWithSend = GetMessageSendingFormDialogResult(form) == DialogResult.OK;
						}
						if (continueWithSend)
						{
							var linesToBeSentPKs = new HashSet<ZGuid>(parent.SelectedSendingObjects.Cast<FinalSumAWithAPreliminaryMessageSendingAction>().Select(x => x.MessagingObject.PK).ToArray());
							var sumASender = new FinalSumAWithAPreliminarySender(dec, linesToBeSentPKs);
							var (canSend, whyCannotSend) = sumASender.CanSend;
							if (canSend)
							{
								sumASender.Send();
								Header.Factory.Save();
								Globals.Message.Show(MessagingMenuExtension.MessageHasBeenSent);
							}
							else
							{
								Globals.Message.Show(whyCannotSend);
							}
						}
					}
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		DialogResult GetMessageSendingFormDialogResult(MessageSendingFormWithValidationDetails form)
		{
			return ZFormModaliser.ShowDialogWithoutDispose(form);
		}

		bool AreDeclarationsValid(IEnumerable<CusTempStorageDec> decs, string tabText)
		{
			if (!decs.Any())
			{
				Globals.Message.Show(Res.GetString("E5AF7F68-8A03-4F6A-B455-BFA6B64DC762", "Please enter at least one declaration in {0}.", tabText));
				return false;
			}
			return true;
		}
	}
}
