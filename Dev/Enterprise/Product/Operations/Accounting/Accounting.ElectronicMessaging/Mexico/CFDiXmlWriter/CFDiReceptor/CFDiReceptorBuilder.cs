using System;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiReceptorBuilder
	{
		ComprobanteReceptor BuildReceptorInfo(TransactionInfo transaction);
	}

	class CFDiReceptorBuilder : ICFDiReceptorBuilder
	{
		public CFDiReceptorBuilder()
		{
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
		}

		ComprobanteReceptor ICFDiReceptorBuilder.BuildReceptorInfo(TransactionInfo transaction)
		{
			var oReceptor = new ComprobanteReceptor();
			oReceptor.UsoCFDI = c_UsoCFDI.G03;

			if (transaction?.OrganizationAddress != null)
			{
				var registrationCode = LocallyGetRegistrationCode(MexicoOrgCusCodeInfo.OrgCusCodes.RFC);
				var registrationCodeRFG = LocallyGetRegistrationCode(MexicoOrgCusCodeInfo.OrgCusCodes.RFG);

				oReceptor.Rfc = registrationCode.IsNullOrEmpty()
								? registrationCodeRFG
								: registrationCode;

				if (transaction.OrganizationAddress.CompanyName.HasValue)
				{
					oReceptor.Nombre = transaction.OrganizationAddress.CompanyName;
				}

				var registrationCodeUsoCFDI = LocallyGetRegistrationCode(MexicoOrgCusCodeInfo.OrgCusCodes.CFD);

				if (Enum.TryParse(registrationCodeUsoCFDI, out c_UsoCFDI codigoCFDI)
					&& Enum.IsDefined(typeof(c_UsoCFDI), codigoCFDI))
				{
					oReceptor.UsoCFDI = codigoCFDI;
				}

				if (!registrationCodeRFG.IsNullOrEmpty() && (transaction.BranchAddress?.Postcode.HasValue ?? false))
				{
					oReceptor.DomicilioFiscalReceptor = transaction.BranchAddress.Postcode;
				}
				else if (transaction.OrganizationAddress.Postcode.HasValue)
				{
					oReceptor.DomicilioFiscalReceptor = transaction.OrganizationAddress.Postcode;
				}

				if (Enum.TryParse((NoResString)"Item" + LocallyGetRegistrationCode(MexicoOrgCusCodeInfo.OrgCusCodes.REG), out c_RegimenFiscal regimenFiscalEnumValue) // Enum Constant Data Format
					&& Enum.IsDefined(typeof(c_RegimenFiscal), regimenFiscalEnumValue))
				{
					oReceptor.RegimenFiscalReceptor = regimenFiscalEnumValue;
				}
			}

			return oReceptor;

			string LocallyGetRegistrationCode(string code) => TransactionInfoHelper.GetRegistrationCode(transaction.OrganizationAddress, CountryCodes.Mexico, code);
		}

		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif
	}
}
