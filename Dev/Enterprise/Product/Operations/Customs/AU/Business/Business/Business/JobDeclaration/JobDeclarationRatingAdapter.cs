using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationRatingAdapter<T> : BaseJobDeclarationRatingAdapter<T>
		where T : JobDeclaration
	{
		public JobDeclarationRatingAdapter(T parent) : base(parent)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		readonly T parent;

		protected override AutoRatingStatusInfo GetStatusInformationCore()
		{
			var existingStatus = base.GetStatusInformationCore();
			if (!existingStatus.CanExecute || !existingStatus.Message.IsEmpty)
			{
				return existingStatus;
			}

			ZBool lodgementPending = (parent.JE_EntryStatus != CustomsEntryStatus.DeclarationWorkComplete.Code
									&& (parent.CustomsEntryHeaders.Count == 0 || !parent.HaveAllEntriesBeenLodged));

			var message = new ZStringBuilder();

			if (parent.IsImport)
			{
				if (parent.HasMixedPaymentModesBeenUsed)
				{
					message.Append(mixedPaymentModesWarning);
				}

				if (lodgementPending)
				{
					message.Append("The Declaration has not been lodged or received a lodgement response." + GetRecommendDelayAutoratingMessage());
				}
				else if (CMRImportMessageStatusList.IsAwaitingResponse(parent.JE_MessageStatus))
				{
					message.Append("The Declaration is waiting for a response. " + GetRecommendDelayAutoratingMessage());
				}
			}

			if (!string.IsNullOrEmpty(message.ToString()))
			{
				message.Append("Do you wish to continue AutoRating?");
			}

			return new AutoRatingStatusInfo(true, message.ToStringWithNewLineBetweenAppends());
		}

		string GetRecommendDelayAutoratingMessage()
		{
			return System.Environment.NewLine +
				"The customs disbursement information presented on the invoice should not be treated as final until a response is received." +
				System.Environment.NewLine +
				"We suggest that rating and posting of this amount is postponed until a response has been received." +
				System.Environment.NewLine;
		}

		readonly string mixedPaymentModesWarning = "Customs Payment Responses from different bank accounts have been detected. This suggests that payments have been made from both the Broker and Importer accounts. Autorating will post amounts depending on the current Payment Mode Setting on the Misc Options Tab. If you choose to continue you should carefully check the Autorating postings. Landed Costing may also be effected, so again you should carefully check the Landed Costing postings.";
	}
}
