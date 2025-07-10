using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDJobDeclarationValidation : ImportJobDeclarationValidation
	{
		public IMDJobDeclarationValidation(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ValidateJE_TotalNoOfPacks();
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
				if (!JobDeclaration.AddInfo.ZA_HART_Hidden.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(JobDeclaration.JE_AmberStatementInfo, "Amber Statement which is required when a Header Amber Reason Type is entered.");
				}
				JobDeclaration.AddInfo.Validation.ValidateZA_HART_Hidden();
			}
		}

		protected override void CheckJE_PaidUnderProtestStatement()
		{
			base.CheckJE_PaidUnderProtestStatement();

			if (JobDeclaration.JE_PaidUnderProtestStatement.Length > 4000)
			{
				JobDeclaration.JE_PaidUnderProtestStatementInfo.AddError("Paid Under Protest Statement can only be 4000 characters long");
			}
			else
			{
				bool found = false;
				foreach (JobComInvoiceLine line in JobDeclaration.InvoiceLines)
				{
					if (line.AddInfo != null && line.AddInfo.ZA_PUP == "Y")
					{
						found = true;
						break;
					}
				}

				if (found && JobDeclaration.JE_PaidUnderProtestStatement.IsEmpty && JobDeclaration.AddInfo.ZA_FPUP_Hidden.IsEmpty)
				{
					JobDeclaration.JE_PaidUnderProtestStatementInfo.AddMessageError("You must enter the Paid Under Protest Statement when at least one Invoice Line has been marked as Paid Under Protest.");
				}

				if (!JobDeclaration.JE_PaidUnderProtestStatement.IsEmpty)
				{
					if (!found)
					{
						JobDeclaration.JE_PaidUnderProtestStatementInfo.AddMessageError("At least one Invoice Line must be marked as Paid Under Protest.");
					}

					if (!JobDeclaration.AddInfo.ZA_FPUP_Hidden.IsEmpty)
					{
						JobDeclaration.JE_PaidUnderProtestStatementInfo.AddMessageError("Paid Under Protest Statement is not allowed when First Paid Under Protest is entered.");
					}
				}
			}
		}

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();

			if (!JobDeclaration.IsExWarehouse && !JobDeclaration.IsTransportModeOther && (JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.FCL || JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.FCLMixedShipper || JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.LCL))
			{
				var containers = JobDeclaration.CusContainers;

				if (containers.Count == 0 || (containers.Count > 0 && containers[0].CO_ContainerNumber.IsEmpty))
				{
					JobDeclaration.JE_ContainerModeInfo.AddMessageError(EnterContainerMessageError);
				}
			}
		}

		protected override bool ShouldCheckCMRImporter
		{
			get { return true; }
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			if (!JobDeclaration.IsExWarehouse)
			{
				MandatoryValidation.MessageErrorIfNotEntered(JobDeclaration.JE_RL_NKPortOfArrivalInfo);
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_PaymentMethodInfo);

			PaymentDetailRetriever paymentDetailRetriever = new PaymentDetailRetriever(JobDeclaration);
			BankDetails bankDetail = paymentDetailRetriever.GetBankDetails();

			if ((bankDetail.AccountNumber.IsEmpty || bankDetail.BSBNumber.IsEmpty) && Parent.JE_PaymentMethod != JobDeclaration.PaymentMethods.Cash)
			{
				string party = paymentDetailRetriever.PartyToPayEntry == PaymentParty.Broker || paymentDetailRetriever.PartyToPayEntry == PaymentParty.SecondBroker ? "Broker" : "Importer";

				StringBuilder message = new StringBuilder();
				message.Append(party + "'s Bank details are not complete. Please do the following.");
				if (party == "Broker")
				{
					message.Append("\r\nGo to Registry -> Customs -> Australia -> Payment Bank Account and select a bank account and fill in BSB and Account Number.");
				}
				else
				{
					message.Append("\r\nPress F3 in the Importer field in Declaration tab and go to Consignee on Organisation form. Fill in BSB and Account Number there.");
				}

				ZString finalMessage = message.ToString();
				if (!finalMessage.IsEmpty)
				{
					JobDeclaration.JE_PaymentMethodInfo.AddMessageError(finalMessage);
				}
			}
		}

		protected override void CheckJE_TotalNoOfPacks()
		{
			base.CheckJE_TotalNoOfPacks();

			if (JobDeclaration != null && JobDeclaration.IsPackingInformationRelevant)
			{
				if (JobDeclaration.JE_TotalNoOfPacks.IsEmpty)
				{
					JobDeclaration.JE_TotalNoOfPacksInfo.AddMessageError(EnterPackagesMessageError);
				}
				else if (JobDeclaration.Packages.Count == 0)
				{
					JobDeclaration.JE_TotalNoOfPacksInfo.AddMessageError(PackingDetailsLineMessageError);
				}
				else
				{
					var totalPackages = JobDeclaration.PackingGroups.TotalNumberOfPackages;
					if (totalPackages != JobDeclaration.JE_TotalNoOfPacks)
					{
						JobDeclaration.JE_TotalNoOfPacksInfo.AddWarning("The Total Number of Packages on the Declaration does not equal the total Number of Packages entered on the Packing tab.");
					}
					else if (totalPackages == int.MaxValue)
					{
						JobDeclaration.JE_TotalNoOfPacksInfo.AddMessageError("The total number of packages exceeds the maximum allowed amount. Check packing lines for data entry errors.");
					}
				}
			}
		}

		public const string EnterContainerMessageError = "Please enter at least one container.";
		public const string EnterPackagesMessageError = "Please enter the total number of packages for the declaration.";
		public const string PackingDetailsLineMessageError = "Please enter at least one Packing Details line.";

		public const string ValidEDIFACTCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,-()=+:'/";
		public const string InvalidEDIFACTCharactersWarning = "An invalid character was entered. You can only enter characters, numbers or the following symbols .,-()=+:'/. The invalid characters will be removed before sending the message to Customs.";

		protected override void CheckJE_AgentsReference()
		{
			base.CheckJE_AgentsReference();

			ZString parsedAgentReference = JobDeclaration.JE_AgentsReference.ToUpper().ExcludeChars(ValidEDIFACTCharacters);

			if (!parsedAgentReference.IsEmpty)
			{
				JobDeclaration.JE_AgentsReferenceInfo.AddWarning(InvalidEDIFACTCharactersWarning);
			}

			if (Env.Registry.AUCustoms.AgentsReferenceDefaulting == Core.Constants.AgentsReferenceDefaulting.NSR)
			{
				JobDeclaration.JE_AgentsReferenceInfo.AddWarning("The Agents Reference Defaulting registry setting has been set to 'NSR'. Agents Reference will not be sent in the message.");
			}
		}

		protected override void CheckJE_OwnerRef()
		{
			base.CheckJE_OwnerRef();
			if (!JobDeclaration.JE_OwnerRef.ToUpper().ExcludeChars(ValidEDIFACTCharacters).IsEmpty)
			{
				JobDeclaration.JE_OwnerRefInfo.AddWarning(InvalidEDIFACTCharactersWarning);
			}
			if (JobDeclaration.JE_OwnerRef.Length > 20)
			{
				JobDeclaration.JE_OwnerRefInfo.AddWarning(OwnersRefWillBeTruncatedWarning);
			}
		}
		public const string OwnersRefWillBeTruncatedWarning = "The Owners Reference is longer then 20 characters, only the first 20 characters will be sent in the message to Customs.";
	}
}
