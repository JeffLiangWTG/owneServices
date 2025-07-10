using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PaymentDetailRetrieverIncludingAQIS : PaymentDetailRetriever
	{
		public PaymentDetailRetrieverIncludingAQIS(JobDeclaration declaration, CMRMessageTypes messageType, EFTPaymentInformationCollection eFTPayInfos)
			: base(declaration)
		{
			if (messageType == CMRMessageTypes.Payment && eFTPayInfos != null)
			{
				this.customsChargeAmount = eFTPayInfos.CustomsChargeAmount;
				this.aQISAmount = eFTPayInfos.AQISAmount;
			}
			else
			{
				this.customsChargeAmount = declaration.CustomsEntryHeaders.TotalAmountPayableForThisSession;
				this.aQISAmount = 0m;
			}
		}

		#region Boolean Properties
		public bool IsPayingCustomsCharge
		{
			get { return customsChargeAmount > 0; }
		}

		public bool IsPayingAQISChargeOnly
		{
			get { return customsChargeAmount == 0 && aQISAmount > 0; }
		}
		#endregion

		#region Overrides

		protected override bool ShouldIgnorePaymentMethodsOnDeclaration
		{
			get { return IsPayingAQISChargeOnly; }
		}

		protected internal override bool ImporterWillPayDeclaration()
		{
			bool result = false;
			if (IsPayingAQISChargeOnly)
			{
				result = Declaration.Importer != null && Declaration.Importer.MiscServ != null && Declaration.Importer.MiscServ.OM_IMEftQuarantineFromImport;
			}
			else
			{
				result = base.ImporterWillPayDeclaration();
			}
			return result;
		}

		#endregion

		#region Payable Amounts

		internal readonly ZDecimal customsChargeAmount;
		internal readonly ZDecimal aQISAmount;

		protected override ZDecimal TotalAmountPayableCore
		{
			get { return customsChargeAmount + aQISAmount; }
		}

		#endregion
	}
}
