using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DrawbackJobDeclarationValidation : JobDeclarationValidation
	{
		public DrawbackJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckJE_OwnerRef()
		{
			base.CheckJE_OwnerRef();
			if (JobDeclaration.JE_OwnerRef.IsEmpty && !JobDeclaration.AutoAssignImporterRef)
			{
				JobDeclaration.JE_OwnerRefInfo.AddMessageError("Owner reference required for sending Drawback Message.");
			}
			if (!JobDeclaration.JE_OwnerRef.ToUpper().ExcludeChars(IMDJobDeclarationValidation.ValidEDIFACTCharacters).IsEmpty)
			{
				JobDeclaration.JE_OwnerRefInfo.AddWarning(IMDJobDeclarationValidation.InvalidEDIFACTCharactersWarning);
			}
		}

		protected override void CheckJE_AmberStatement()
		{
			base.CheckJE_AmberStatement();
			if (JobDeclaration.JE_AmberStatement.Length > 2560)
			{
				JobDeclaration.JE_AmberStatementInfo.AddError("Amber Statement can only be 2560 characters long");
			}
			else
			{
				if (!JobDeclaration.DrawbackHeaderAmberReasonCode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(JobDeclaration.JE_AmberStatementInfo, "Amber Statement which is required when a Header Amber Reason Type is entered.");
				}
				JobDeclaration.AddInfo.Validation.ValidateZA_DARC_Hidden();
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			PaymentDetailRetriever paymentDetailRetriever = new PaymentDetailRetriever(JobDeclaration);
			BankDetails bankDetail = paymentDetailRetriever.GetBankDetails();

			if ((bankDetail.AccountNumber.IsEmpty || bankDetail.BSBNumber.IsEmpty) && Parent.JE_PaymentMethod != JobDeclaration.PaymentMethods.Cash)
			{
				string party = paymentDetailRetriever.PartyToPayEntry == PaymentParty.Broker || paymentDetailRetriever.PartyToPayEntry == PaymentParty.SecondBroker ? "Broker" : "Drawback Claimant";

				StringBuilder message = new StringBuilder();
				message.Append(party + "'s Bank details are not complete. Please do the following.");
				if (party == "Broker")
				{
					message.Append("\r\nGo to Registry -> Customs -> Australia -> Payment Bank Account and select a bank account and fill in BSB and Account Number.");
				}
				else
				{
					message.Append("\r\nPress F3 in the Drawback Claimant field in Declaration tab and go to Consignee on Organisation form. Fill in BSB and Account Number there.");
				}

				ZString finalMessage = message.ToString();
				if (!finalMessage.IsEmpty)
				{
					JobDeclaration.JE_PaymentMethodInfo.AddMessageError(finalMessage);
				}
			}
		}

		protected override bool JE_MergeByRequired
		{
			get { return false; }
		}
	}
}
