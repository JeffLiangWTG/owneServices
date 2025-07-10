using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsPermitHelper
	{
		public static ZString GetPermitAppIdForMessage(EDIMessage message) => message?.EM_MessageNum ?? ZString.Empty;

		public static ZDecimal GetCustomsQuantity(ZString permitNumber, PermitRecord permitRecord, NCTSPermitRecord permit)
		{
			var result = ZDecimal.Zero;
			var unitOfMeasure = permitRecord.PermitHeader?.CPH_UnitOfMeasure ?? ZString.Empty;
			if (!unitOfMeasure.IsEmpty)
			{
				if (unitOfMeasure == permit.QuantityUnit || permit.Quantity == 0.0m)
				{
					result = permit.Quantity;
				}
				else
				{
					permitRecord.ErrorMessages.Add(UnitOfMeasureNotValidForTariff(permitNumber, unitOfMeasure));
				}
			}
			else if (permitRecord.PermitHeader?.IsQTY ?? false)
			{
				permitRecord.ErrorMessages.Add(UnitOfMeasureNotSetupOnPermit(permitNumber));
			}
			return result;
		}

		internal static string UnitOfMeasureNotValidForTariff(ZString permitNumber, ZString unitOfMeasure)
		{
			return Res.GetString("25D79EA2-C846-4371-A051-D5A2AA1A9980", "The Unit of Measure '{1}' setup against Permit '{0}' is not valid.", permitNumber, unitOfMeasure);
		}

		internal static string UnitOfMeasureNotSetupOnPermit(ZString permitNumber)
		{
			return Res.GetString("D7B5737D-13BF-4AB7-83FF-8EE6DA68CBD3", "No Unit of Measure has been setup against Permit '{0}'.", permitNumber);
		}

		public static void UpdateGuaranteeAndPwBondAmount(NctsHeader header, EDIMessage incomingMessage, Money incomingAmount)
		{
			if (incomingAmount != null)
			{
				var reference = header.GetPermitReference();
				var relatedPermitHeaders = PermitHelper.GetRelatedPermitTransactions(header.Factory, header.CountryCode, reference, 0).Select(x => x.PermitHeader).Distinct();

				foreach (CusGuaranteeHeader permitHeader in relatedPermitHeaders)
				{
					var numberGuarantee = permitHeader.CPH_Number;
					var nctsGuarantee = header.GetEffectiveGuarantees().Cast<CusBondDetail>().FirstOrDefault(x => x.PW_BondNumber == numberGuarantee);
					var transactionpending = permitHeader.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending && x.CPL_AppId == incomingMessage.EM_MessageNum);

					if (nctsGuarantee != null)
					{
						var newAmount = incomingAmount.Amount;
						var newValue = newAmount - (transactionpending?.CPL_TranValue ?? 0m);
						nctsGuarantee.PW_BondAmount = newAmount;
						if (newValue != 0)
						{
							permitHeader.AddTransaction(header.BH_JobReference, Res.GetString("b3cb2943-e3eb-43ff-97c7-57bc01894546", "NCTS departure adjustment {0}", header.LocalReferenceNumber), incomingMessage.EM_MessageNum, "", newValue, 0, status: PermitTransactionStatusList.Codes.Confirmed);
						}
					}
				}
			}
		}
	}
}
