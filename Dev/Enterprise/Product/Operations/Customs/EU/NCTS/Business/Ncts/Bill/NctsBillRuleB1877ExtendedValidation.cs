using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsBillRuleB1877ExtendedValidation
	{
		public NctsBillRuleB1877ExtendedValidation(NctsBill nctsBill)
		{
			this.nctsBill = Argument.NotNull(nctsBill, nameof(nctsBill));
		}

		public void Validate()
		{
			var header = nctsBill.Header;

			if (nctsBill.GoodsItems.Count > 1
				&& header.Configuration.ValidationRuleConfiguration.IsRuleB1877_1Active
				&& nctsBill.IsInPhase5TransitionPeriod
				&& header.IsPhase5Departure)
			{
				ValidateConsignee();
				ValidateTransportChargesMethodOfPayment();
			}
		}

		void ValidateConsignee()
		{
			var consigneeIdCollection = new HashSet<(ZString Type, ZString Value)>();
			foreach (var goodsItem in nctsBill.GoodsItems)
			{
				var (identificationType, identification) = GetOrganisationId(goodsItem.Consignee);
				consigneeIdCollection.Add((identificationType, identification));

				if (consigneeIdCollection.Skip(1).Any())
				{
					break;
				}
			}

			if (consigneeIdCollection.Count == 1)
			{
				var idType = consigneeIdCollection.Single().Type;
				if (!idType.IsEmpty)
				{
					var header = nctsBill.Header;
					var attributeDescription = GetOrganisationIdDescription(idType);
					nctsBill.AddRowMessageError(header.Configuration.ValidationRuleConfiguration.Messages.GetB1877_1_2Message(attributeDescription));
				}
			}
		}

		void ValidateTransportChargesMethodOfPayment()
		{
			var transportChargesList = nctsBill.GoodsItems
				.Select(x => x.BY_TransportChargesMethodOfPayment)
				.Distinct();

			if (!transportChargesList.Skip(1).Any() && transportChargesList.Any(x => !x.IsEmpty))
			{
				var header = nctsBill.Header;
				nctsBill.AddRowMessageError(header.Configuration.ValidationRuleConfiguration.Messages.B1877_1_1Message);
			}
		}

		(ZString IdentificationType, ZString Identification) GetOrganisationId(JobDocAddress consgnee)
		{
			var resultId = (ZString.Empty, ZString.Empty);

			if (consgnee != null)
			{
				var consigneeOrg = consgnee.Organisation;
				var eori = consigneeOrg.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				if (!eori.IsEmpty)
				{
					resultId = (OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eori);
				}
				else
				{
					var tcu = consigneeOrg.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
					if (!tcu.IsEmpty)
					{
						resultId = (OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, tcu);
					}
					else
					{
						var addressFull = consgnee.AddressFull;
						if (!addressFull.IsEmpty)
						{
							resultId = (OrganisationIDAddress, addressFull);
						}
					}
				}
			}

			return resultId;
		}

		string GetOrganisationIdDescription(string idType)
		{
			switch (idType)
			{
				case OrgCusCode.EuropeanUnionSharedCodeTypes.Eori:
				case OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU:
					return Res.GetString("99B5EF95-E567-479A-8C2A-3289CCE18342", "EORI/TCU code");

				case OrganisationIDAddress:
					return Res.GetString("A8A2AD54-B57A-4207-8489-F7FC9523E6D5", "name and address");

				default:
					return "";
			}
		}

		readonly NctsBill nctsBill;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string OrganisationIDAddress = "Address";
	}
}
