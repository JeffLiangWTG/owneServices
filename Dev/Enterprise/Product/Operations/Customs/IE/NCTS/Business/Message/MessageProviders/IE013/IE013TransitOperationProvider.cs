using System.Linq;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.NCTS;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC013C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC015C;
using CargoWise.Types;
using Enterprise.Customs.IE.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE013TransitOperationProvider : IE013AndIE015TransitOperationProvider, IIE013TransitOperation
	{
		public IE013TransitOperationProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public string MRN => NctsHeader.MovementReferenceNumber;

		public bool AmendmentTypeFlag
		{
			get
			{
				var linkUniqueId = NctsHeader.IsDepartureMovement ? NctsHeader.MovementHeader.PK : NctsHeader.PK;
				var previousMessage = NCTSInboundEDIMessage.GetPreviousDeclarationMessage(NctsHeader.Factory, linkUniqueId, ZDateTime.UtcNow);
				if (previousMessage != null)
				{
					if (GuaranteesHaveChanged(previousMessage))
					{
						return true;
					}
				}
				return false;
			}
		}

		bool GuaranteesHaveChanged(NCTSOutboundEDIMessage previousMessage)
		{
			var result = false;
			if (previousMessage.EM_MessageType == NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData)
			{
				result = GuaranteesHaveChanged(previousMessage.GetDataProvider<Cc015CType>());
			}
			else if (previousMessage.EM_MessageType == NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationAmendment)
			{
				result = GuaranteesHaveChanged(previousMessage.GetDataProvider<Cc013CType>());
			}
			return result;
		}

		bool GuaranteesHaveChanged(IGuaranteesSupporter supporter)
		{
			if (supporter != null)
			{
				var guarantees = supporter.Guarantees;
				var currentGuaranteeCount = NctsHeader.MovementHeader.Guarantees.Count;
				if (currentGuaranteeCount != (guarantees?.Count ?? 0))
				{
					return true;
				}
				else if (currentGuaranteeCount > 0)
				{
					var currentGuarantees = NctsHeader.MovementHeader.Guarantees.Cast<EU.NCTS.Business.NctsGuarantee>()
						.OrderBy(x => x.PW_BondType)
						.ThenBy(x => x.PW_BondNumber2)
						.ThenBy(x => x.PW_BondNumber)
						.ThenBy(x => x.PW_BondAmount)
						.ToArray();
					var i = 0;
					foreach (var previousGuarantee in guarantees)
					{
						var currentGuarantee = currentGuarantees[i++];

						var currentGuaranteeTypeValue = currentGuarantee.PW_BondType.ToUpperInvariant();
						if (!currentGuaranteeTypeValue.EqualsIgnoringCase(previousGuarantee.GuaranteeType))
						{
							return true;
						}

						if (!currentGuaranteeTypeValue.IsEmpty)
						{
							if (currentGuaranteeTypeValue == NctsGuaranteeTypeList.Codes.CashDepositGuarantee ||
								currentGuaranteeTypeValue == NctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies)
							{
								if (!currentGuarantee.PW_BondNumber2.EqualsIgnoringCase(previousGuarantee.OtherGuaranteeReference))
								{
									return true;
								}
							}

							if (MessageStaticHelper.IsGuaranteeTypeWithReferences(currentGuaranteeTypeValue))
							{
								var previousGuaranteeReferences = previousGuarantee.GuaranteeReferences;
								if ((previousGuaranteeReferences?.Count ?? 0) != 1)
								{
									return true;
								}

								var previousGuaranteeReference = previousGuaranteeReferences.First();
								if (!currentGuarantee.PW_BondNumber.EqualsIgnoringCase(previousGuaranteeReference.Grn))
								{
									return true;
								}

								if (currentGuarantee.PW_BondAmount != previousGuaranteeReference.AmountToBeCovered.GetValueOrDefault())
								{
									return true;
								}
							}
						}
					}
				}
			}

			return false;
		}
	}
}
