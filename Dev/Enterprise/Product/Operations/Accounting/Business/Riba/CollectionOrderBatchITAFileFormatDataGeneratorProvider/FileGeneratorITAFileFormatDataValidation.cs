using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba
{
	class FileGeneratorITAFileFormatDataValidation : ICollectionBatchValidation
	{
		public FileGeneratorITAFileFormatDataValidation(AccCollectionBatch accCollectionBatch, GlbCompany glbCompany, bool isProduction, ZDateTime dateTime)
		{
			collectionBatch = Argument.NotNull(accCollectionBatch, nameof(accCollectionBatch));
			bankAccount = Argument.NotNull(collectionBatch.BankAccount, nameof(collectionBatch.BankAccount));
			currentCompany = Argument.NotNull(glbCompany, nameof(glbCompany));
			isProductionSystem = isProduction;
			this.dateTime = dateTime;
		}
		protected readonly AccCollectionBatch collectionBatch;
		protected readonly AccBankAccount bankAccount;
		protected readonly GlbCompany currentCompany;
		protected readonly bool isProductionSystem;
		protected readonly ZDateTime dateTime;

		bool HasNoBRCNPJ(AccCollectionOrderLine collectionOrderLine) =>
			collectionOrderLine?.Transaction?.Header?.CustomsCodes?.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ) == null;

		bool HasNoBRCPF(AccCollectionOrderLine collectionOrderLine) =>
			collectionOrderLine?.Transaction?.Header?.CustomsCodes?.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration) == null;

		ZString ICollectionBatchValidation.GetPreSaveValidationMessage()
		{
			ZString validationMessage = ZString.Empty;
			var branchOrCompany_OrgProxy = bankAccount.Branch?.OrgProxy ?? currentCompany.OrgProxy;
			var branchOrCompany_OrgCusCodeOfCJNType = branchOrCompany_OrgProxy?.CustomsCodes?.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ);

			if (branchOrCompany_OrgCusCodeOfCJNType == null)
			{
				return Res.GetString("17D767D5-931A-44EB-984D-7A6DCA559973", "Your Branch or Login Company does not have a BR CJN, please record a BR CJN in the corresponding Organization Proxy.");
			}

			var transactions = GetInvalidTransactionNumbers(orderLine => orderLine.Transaction.AH_TransactionNum.Length > 10);

			if (transactions?.Count() > 0)
			{
				return Res.GetString("9E729287-593F-49B1-B971-51B0E1B4E8C5", "The following selected transactions: '{0}' have Transaction Numbers longer than 10 digits, please review your setups to make sure transaction number is limited to 10 digits.", string.Join("', '", transactions));
			}

			transactions = GetInvalidTransactionNumbers(orderline => HasNoBRCNPJ(orderline) && HasNoBRCPF(orderline));

			if (transactions?.Count() > 0)
			{
				return Res.GetString("E7C10C37-915C-4007-99D0-6569A9D58CA3", "The Receivables Organization of transactions '{0}' are missing BR CJN or BR CPF, please review.", string.Join("', '", transactions));
			}

			return validationMessage;
		}

		IEnumerable<ZString> GetInvalidTransactionNumbers(Func<AccCollectionOrderLine, bool> isInvalid)
		{
			var transactionNumbers = collectionBatch.CollectionOrders?
				.Select(order => new {
					IncludeInBatch = order.IncludeInBatch,
					TransactionNums = order.CollectionOrderLines
											.Where(isInvalid)
											.Select(orderline => orderline.Transaction.AH_TransactionNum)
											.ToArray()
				})
				.Where(x => x.IncludeInBatch
					&& x.TransactionNums.Any()
				)
				.SelectMany(x => x.TransactionNums)
				.OrderBy(y => y)
				.Distinct();

			return transactionNumbers;
		}
	}
}
