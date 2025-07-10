using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CA.Services
{
	class ValidateIIDTransactionProto : IValidateTransaction
	{
		public ValidateIIDTransactionProto(ValidateIIDTransaction transaction)
		{
			this.transaction = transaction;
		}
		readonly ValidateIIDTransaction transaction;

		IValueObject IValidateTransaction.Transaction => this.transaction;

		public int CommoditiesNumber
		{
			get { return transaction.CommodityGroup.Cast<ValidateIIDTransactionCommodityGroup>().Sum(g => g.Commodity.Count); }
		}

		ValidateIIDTransactionCommodityGroup FindCommodityGroup(string commodityGroupId)
		{
			return transaction.CommodityGroup.Cast<ValidateIIDTransactionCommodityGroup>().FirstOrDefault(g => g.commodityGroupId == commodityGroupId);
		}

		public void AddNewCommodity(IAIRSValidationQueriedLine line)
		{
			var iIDLine = (IAIRSIIDValidationQueriedLine)line;
			if (iIDLine != null)
			{
				var commodityGroup = FindCommodityGroup(line.CommodityGroup);
				if (commodityGroup == null)
				{
					commodityGroup = transaction.CommodityGroup.AddNew();
					commodityGroup.commodityGroupId = line.CommodityGroup;
				}
				var commodity = commodityGroup.Commodity.AddNew();
				commodity.commodityId = line.Commodity;
				commodity.HSNumber = line.HSNumber;
				commodity.DeliveryPartyProvince = iIDLine.DeliveryPartyProvince;

				if (line.Registrations != null)
				{
					var registrationsList = new List<IValueObject>();
					foreach (var registration in line.Registrations.Where(x => x.RegistrationType == AIRSValidationQueriedLineRegistrationTypes.Normal))
					{
						registrationsList.Add(new ValidateIIDTransactionCommodityGroupCommodityRegistrationsRegistration { registrationId = registration.RegistrationId });
					}
					foreach (var registration in line.Registrations.Where(x => x.RegistrationType == AIRSValidationQueriedLineRegistrationTypes.MaterializedLpco))
					{
						registrationsList.Add(new ValidateIIDTransactionCommodityGroupCommodityRegistrationsMaterializedLpco { registrationId = registration.RegistrationId });
					}
					foreach (var registration in line.Registrations.Where(x => x.RegistrationType == AIRSValidationQueriedLineRegistrationTypes.DematerializedLpco))
					{
						registrationsList.Add(new ValidateIIDTransactionCommodityGroupCommodityRegistrationsDematerializedLpco { registrationId = registration.RegistrationId });
					}
					commodity.Registrations.Items = registrationsList.ToArray();
				}
				commodity.AirsCode = line.AirsCode;
				commodity.OriginCountry = line.OriginCountry;
				commodity.OriginState = line.OriginState;
				commodity.Enduse = line.EndUse;
				commodity.Miscellaneous = line.Miscellaneous;
			}
		}

		public void RemoveLastCommodity(string commodityGroupId)
		{
			var commodityGroup = FindCommodityGroup(commodityGroupId);
			if (commodityGroup != null)
			{
				commodityGroup.Commodity.RemoveAt(commodityGroup.Commodity.Count - 1);
				if (commodityGroup.Commodity.Count == 0)
				{
					transaction.CommodityGroup.RemoveAt(transaction.CommodityGroup.Count - 1);
				}
			}
		}

		public IEnumerable<ZString> GetAllCommodityGroupIds()
		{
			foreach (ValidateIIDTransactionCommodityGroup commodityGroup in transaction.CommodityGroup)
			{
				foreach (ValidateIIDTransactionCommodityGroupCommodity commodity in commodityGroup.Commodity)
				{
					yield return commodity.commodityId;
				}
			}
		}
	}
}
