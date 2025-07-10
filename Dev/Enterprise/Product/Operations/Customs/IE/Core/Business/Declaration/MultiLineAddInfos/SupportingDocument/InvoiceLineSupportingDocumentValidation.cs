using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	static class InvoiceLineSupportingDocumentValidation
	{
		internal static void CheckCSI_UnitOfQuantity(SupportingDocument parent)
		{
			var targetInfo = parent.CSI_UnitOfQuantityInfo;
			if (parent.CSI_Quantity.IsEmpty)
			{
				if (!parent.CSI_UnitOfQuantity.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("122D1D34-2251-4340-BEAE-D5C14C02FC52", "Supporting Documents' Measurement Unit is required at Invoice lines only if Quantity is provided."));
				}
			}
			else if (parent.CSI_UnitOfQuantity.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("329ED5D7-1147-4F28-8F33-13572DB5A3B1", "When Quantity is provided at Invoice line's Supporting Document, then Measurement Unit & Qualifier is mandatory"));
			}
		}

		internal static void CheckCSI_RX_NKCurrency(SupportingDocument parent)
		{
			var targetInfo = parent.CSI_RX_NKCurrencyInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (parent.CSI_Value.IsEmpty)
			{
				if (!parent.CSI_RX_NKCurrency.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("4E95B9E8-7002-4F36-925C-EE21708BAFDA", "Supporting Documents' Currency is required at Invoice lines only if Amount is provided."));
				}
			}
			else if (parent.CSI_RX_NKCurrency.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("A3F3952D-681B-4E62-A18E-274C30CEB327", "When Amount is provided at Invoice line's Supporting Document, then Currency is mandatory"));
			}
		}
	}
}
