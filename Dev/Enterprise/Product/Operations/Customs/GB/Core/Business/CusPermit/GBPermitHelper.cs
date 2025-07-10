using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business
{
	public static class GBPermitHelper
	{
		public static ZString GetPermitAppIdForMessage(EDIMessage message) => message?.EM_MessageNum ?? ZString.Empty;

		public static ZDecimal GetCustomsQuantity(ZString permitNumber, PermitRecord permitRecord, CusEntryPermitRecord permit)
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
			return Res.GetString("46AF9EDE-E35B-46CB-BCF1-479285300845", "The Unit of Measure '{1}' setup against Permit '{0}' is not valid.", permitNumber, unitOfMeasure);
		}

		internal static string UnitOfMeasureNotSetupOnPermit(ZString permitNumber)
		{
			return Res.GetString("81494BFB-2699-4C26-B015-4FC16A39B9DC", "No Unit of Measure has been setup against Permit '{0}'.", permitNumber);
		}

		public static void UpdateGuaranteeAndPwBondAmount(NctsHeader header, EDIMessage incomingMessage, Money incomingAmount)
		{
			var reference = header.GetPermitReference();
			var relatedPermitHeaders = PermitHelper.GetRelatedPermitTransactions(header.Factory, header.CountryCode, reference, 0).Select(x => x.PermitHeader).Distinct();

			foreach (EU.Business.CusGuaranteeHeader permitHeader in relatedPermitHeaders)
			{
				var numberGuarantee = permitHeader.CPH_Number;
				var nctsGuarantee = header.GetEffectiveGuarantees().Cast<CusBondDetail>().FirstOrDefault(x => x.PW_BondNumber == numberGuarantee);
				if (nctsGuarantee != null)
				{
					if (nctsGuarantee.PW_BondAmount != incomingAmount.Amount)
					{
						EU.NCTS.Business.NctsPermitHelper.UpdateGuaranteeAndPwBondAmount(header, incomingMessage, incomingAmount);
					}
				}
			}
		}
	}

	public class CusEntryPermitRecord
	{
		public EU.Business.Declaration.MultiLineAddInfos.SupportingDocument SupportingDocument { get; set; }
		public ZDecimal CustomsValue { get; set; }
		public ZDecimal Quantity { get; set; }
		public ZString QuantityUnit { get; set; }
		public ZString PermitType { get; set; }
	}
}
