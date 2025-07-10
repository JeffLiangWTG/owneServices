using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CA.Services
{
	class ValidateOGDTransactionProto : IValidateTransaction
	{
		public ValidateOGDTransactionProto(ValidateTransaction transaction)
		{
			this.transaction = transaction;
		}
		readonly ValidateTransaction transaction;

		IValueObject IValidateTransaction.Transaction => this.transaction;

		public int CommoditiesNumber
		{
			get { return transaction.CommodityGroup.Cast<ValidateTransactionCommodityGroup>().Sum(g => g.Commodity.Count); }
		}

		ValidateTransactionCommodityGroup FindCommodityGroup(string commodityGroupId)
		{
			return transaction.CommodityGroup.Cast<ValidateTransactionCommodityGroup>().FirstOrDefault(g => g.commodityGroupId == commodityGroupId);
		}

		public void AddNewCommodity(IAIRSValidationQueriedLine baseLine)
		{
			var line = baseLine as IAIRSOGDValidationQueriedLine;
			if (line != null)
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
				commodity.RequirementId = line.RequirementId;
				commodity.RequirementVersion = line.RequirementVersion;
				commodity.AirsCode = line.AirsCode;
				commodity.DestinationProvince = line.DestinationProvince;
				commodity.OriginCountry = line.OriginCountry;
				commodity.OriginState = line.OriginState;
				commodity.Enduse = line.EndUse;
				commodity.Miscellaneous = line.Miscellaneous;
				foreach (var registration in line.Registrations)
				{
					commodity.Registration.AddNew().registrationId = registration.RegistrationId;
				}
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
			foreach (ValidateTransactionCommodityGroup commodityGroup in transaction.CommodityGroup)
			{
				foreach (ValidateTransactionCommodityGroupCommodity commodity in commodityGroup.Commodity)
				{
					yield return commodity.commodityId;
				}
			}
		}
	}
}
