using System;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiEmisorBuilder
	{
		ComprobanteEmisor BuildEmisorInfo(TransactionInfo transaction);
	}

	class CFDiEmisorBuilder : ICFDiEmisorBuilder
	{
		public CFDiEmisorBuilder()
		{
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
		}

		ComprobanteEmisor ICFDiEmisorBuilder.BuildEmisorInfo(TransactionInfo transaction)
		{
			var oEmisor = new ComprobanteEmisor();
			if (transaction?.BranchAddress != null)
			{
				oEmisor.Rfc = TransactionInfoHelper.GetRegistrationCode(transaction.BranchAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFC);
				oEmisor.Nombre = TransactionInfoHelper.GetRegistrationCode(transaction.BranchAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.ELN) ?? transaction.BranchAddress.CompanyName;
			}
			if (Enum.TryParse((NoResString)"Item" + AccountingElectronicMessagingRegistry.Instance.MexicoTaxRegimeID.Value, out c_RegimenFiscal regimenFiscalEnumValue) // Enum Constant Data Format
				&& Enum.IsDefined(typeof(c_RegimenFiscal), regimenFiscalEnumValue))
			{
				oEmisor.RegimenFiscal = regimenFiscalEnumValue;
			}

			return oEmisor;
		}

		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif
	}
}
