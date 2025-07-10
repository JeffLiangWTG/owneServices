using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionOrgProfitShareDetailsValidator
	{
		public GatewayProfitRedistributionOrgProfitShareDetailsValidator(IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList, IDisposableLogger logger)
		{
			Argument.NotNull(orgProfitShareDetailsList, nameof(orgProfitShareDetailsList));
			Argument.NotNull(logger, nameof(logger));

			this.orgProfitShareDetailsList = orgProfitShareDetailsList;
			this.logger = logger;
		}

		readonly IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList;
		readonly IDisposableLogger logger;

		public bool IsValid()
		{
			var result = true;

			if (!orgProfitShareDetailsList.AllSame(x => x.O4_O3_OrgProfitShareHeader))
			{
				logger.Error((NoResString)"Multiple Profit Share Agreements are not supported!");
				result = false;
			}

			if (!orgProfitShareDetailsList.AllSame(x => x.O4_ShareLosses))
			{
				logger.Error((NoResString)"All Profit Share Details do not share same losses.");
				result = false;
			}

			if (!orgProfitShareDetailsList.AllSame(x => x.O4_GatewayProfitApportionmentMethod))
			{
				logger.Error((NoResString)"All Profit Share Details do not share same Apportionment Method.");
				result = false;
			}

			if (!orgProfitShareDetailsList.AllSame(x => x.O4_AgreementType))
			{
				logger.Error((NoResString)"All Profit Share Details do not share same Apply To.");
				result = false;
			}
			else if (orgProfitShareDetailsList.Any(x => x.O4_AgreementType == OrgProfitShareDetailsLookups.AgreementTypeUserDefined)
				&& !AreAllValuesSame(orgProfitShareDetailsList.Select(x => x.GetUserDefinedChargeCodes())))
			{
				logger.Error((NoResString)"All Profit Share Details do not share same Apply To.");
				result = false;
			}

			return result;
		}

		bool AreAllValuesSame(IEnumerable<AccChargeCode[]> items)
		{
			var container1 = new HashSet<ZGuid>();
			var container2 = new HashSet<ZGuid>();
			bool firstItem = true;

			foreach (var chargeCodes in items)
			{
				if (firstItem)
				{
					for (int i = 0; i < chargeCodes.Length; i++)
					{
						container1.Add(chargeCodes[i].PK);
					}
					firstItem = false;
				}
				else
				{
					container2 = new HashSet<ZGuid>();
					for (int i = 0; i < chargeCodes.Length; i++)
					{
						if (container1.Contains(chargeCodes[i].PK))
						{
							container2.Add(chargeCodes[i].PK);
						}
						else
						{
							return false;
						}
					}

					if (container1.Count != container2.Count)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
