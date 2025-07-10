using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Riba
{
	class FileGeneratorITAFileFormatDataHelper : ICollectionBatchFileGenerator
	{
		public FileGeneratorITAFileFormatDataHelper(AccCollectionBatch accCollectionBatch, GlbCompany glbCompany, bool isProduction, ZDateTime dateTime)
		{
			collectionBatch = Argument.NotNull(accCollectionBatch, nameof(accCollectionBatch));
			currentCompany = Argument.NotNull(glbCompany, nameof(glbCompany));
			bankAccount = Argument.NotNull(collectionBatch.BankAccount, nameof(collectionBatch.BankAccount));
			this.dateTime = dateTime;
			isProductionSystem = isProduction;

			var docManagerSupport = collectionBatch as IDocManagerSupport;
			docManagerInfo = Argument.NotNull(docManagerSupport.DocManagerInfo, nameof(docManagerSupport.DocManagerInfo));
		}
		protected readonly AccCollectionBatch collectionBatch;
		protected readonly GlbCompany currentCompany;
		protected readonly AccBankAccount bankAccount;
		protected readonly bool isProductionSystem;
		protected readonly DocManagerInfo docManagerInfo;
		protected readonly ZDateTime dateTime;

		string TypeOfOperationDescription => isProductionSystem ? "REMESSA" : "TESTE";
		string FinancialServiceCode => "01";
		string FinancialServiceCodeDescription => "COBRANCA";
		string BSBNumber => TruncateAndPadString(bankAccount.AB_BSB, 4);
		string AccountNumber => TruncateAndPadString(bankAccount.AB_AccountNum, 5);
		string UniqueAccountNumber => TruncateAndPadString(bankAccount.AB_FullAccountNumber, 1);
		string BankAccount_BSBAccNumUniqueAccNum => BSBNumber + "00" + AccountNumber + UniqueAccountNumber;
		string BankAccount_Abbreviation => bankAccount.AB_BankAbbreviation.PadLeft(3, '0');
		ZDate CollectionDate => collectionBatch.CollectionOrders.FirstOrDefault(x => x.IncludeInBatch).ACO_CollectionDate;

		string Truncate(string value, int length) => (value?.Length > length) ? value.Substring(0, length) : value;

		string TruncateAndPadString(ZString property, int maxLength, char paddingChar = '0')
		{
			if (property.Length > 0 && property.Length < maxLength)
			{
				return property.PadLeft(maxLength, paddingChar);
			}
			return Truncate(property, maxLength).PadRight(maxLength);
		}

		public string GetHeaderRecord(int rowNumber)
		{
			return "0" + "1" + TypeOfOperationDescription.PadRight(7) + FinancialServiceCode + FinancialServiceCodeDescription.PadRight(15)
			+ BankAccount_BSBAccNumUniqueAccNum
			+ string.Empty.PadRight(8)
			+ Truncate(bankAccount.AB_BankAccountName, 30).PadRight(30)
			+ BankAccount_Abbreviation
			+ Truncate(bankAccount.AB_BankName, 15).PadRight(15)
			+ CollectionDate.ToString("ddMMyy", CultureInfo.InvariantCulture)
			+ string.Empty.PadRight(294)
			+ rowNumber.ToString().PadLeft(6, '0');
		}

		public string GetDetailsRecord(TransactionHeader transaction, int rowNumber)
		{
			var bankAccountBranchOrCompany_OrgProxy = bankAccount.Branch?.OrgProxy ?? currentCompany.OrgProxy;
			var orgProxy_CustomsRegNo_BRCNPJ = bankAccountBranchOrCompany_OrgProxy.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ).OK_CustomsRegNo.RemoveNonNumericCharacters();

			var debtor_CustomsRegNo_BRCNPJ = (transaction.Header.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ)?.OK_CustomsRegNo)?.RemoveNonNumericCharacters() ?? ZString.Empty;

			var debtor_CustomsRegNo_BRCPF = (transaction.Header.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Brazil, BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration)?.OK_CustomsRegNo)?.RemoveNonNumericCharacters() ?? ZString.Empty;

			var taxRegTypeOfDebtor = !debtor_CustomsRegNo_BRCNPJ.IsEmpty ? "02" : "01";
			var debtor_CustomsRegNo_BRCNPJOrCPF = !debtor_CustomsRegNo_BRCNPJ.IsEmpty ? debtor_CustomsRegNo_BRCNPJ : debtor_CustomsRegNo_BRCPF;

			var mainOfficeAddress = transaction.Header.Addresses?.OfType<OrgAddress>()?.FirstOrDefault(a => a.IsMainAddressOfType(OrgAddressType.Office));
			var transactionPostedInForeignCurrency = transaction.AH_RX_NKTransactionCurrency != currentCompany.GC_RX_NKLocalCurrency;
			var oSTotalAmount = transaction.AH_OSTotalAmount;
			var localTotalAmount = transaction.AH_LocalTotalAmount;

			var totalInvoiceInForeignCurrency = transactionPostedInForeignCurrency ? (ZString)oSTotalAmount.ToString("00000000.00000", CultureInfo.InvariantCulture) : (ZString)"00000000.00000";
			var totalInvoiceInLocalCurrency = transactionPostedInForeignCurrency ? (ZString)localTotalAmount.ToString("00000000000.00", CultureInfo.InvariantCulture) : (ZString)oSTotalAmount.ToString("00000000000.00", CultureInfo.InvariantCulture);

			return "1" + "02" + orgProxy_CustomsRegNo_BRCNPJ.PadRight(14)
				+ BankAccount_BSBAccNumUniqueAccNum
				+ string.Empty.PadRight(4) + "0000"
				+ string.Empty.PadRight(25)
				+ transaction.InvoiceRemittanceReference.PadRight(8)
				+ totalInvoiceInForeignCurrency.RemoveNonNumericCharacters().Right(13)
				+ "021" + string.Empty.PadRight(21) + "I" + "01"
				+ transaction.AH_TransactionNum.PadLeft(10, '0')
				+ transaction.AH_DueDate.ToString("ddMMyy", CultureInfo.InvariantCulture)
				+ totalInvoiceInLocalCurrency.RemoveNonNumericCharacters().Right(13)
				+ BankAccount_Abbreviation + "00000" + "01" + "N"
				+ transaction.AH_InvoiceDate.ToString("ddMMyy", CultureInfo.InvariantCulture)
				+ "0".PadRight(62, '0')
				+ taxRegTypeOfDebtor.PadRight(2)
				+ debtor_CustomsRegNo_BRCNPJOrCPF.PadRight(14)
				+ transaction.Header.OH_FullName.SubstringSafe(0, 30).PadRight(30)
				+ string.Empty.PadRight(10)
				+ mainOfficeAddress.OA_Address1.SubstringSafe(0, 40).PadRight(40)
				+ mainOfficeAddress.OA_Address2.SubstringSafe(0, 12).PadRight(12)
				+ mainOfficeAddress.OA_PostCode.RemoveNonNumericCharacters().SubstringSafe(0, 8).PadRight(8)
				+ mainOfficeAddress.OA_City.SubstringSafe(0, 15).PadRight(15)
				+ mainOfficeAddress.OA_State.SubstringSafe(0, 2).PadRight(2)
				+ "0".PadRight(30, '0') + string.Empty.PadRight(4) + "0".PadRight(8, '0')
				+ " " + rowNumber.ToString().PadLeft(6, '0');
		}

		public string GetTrailerRecord(int rowNumber)
		{
			return "9" + string.Empty.PadRight(393) + rowNumber.ToString().PadLeft(6, '0');
		}

		string ICollectionBatchFileGenerator.GetFileData()
		{
			int rowNumber = 1;
			ZStringBuilder str = new ZStringBuilder();
			str.AppendLine(GetHeaderRecord(rowNumber));

			foreach (var collectionOrder in collectionBatch.CollectionOrders.Where(order => order.IncludeInBatch).ToArray())
			{
				var linesToGenerate = collectionOrder.CollectionOrderLines?.Select(y => y.Transaction);
				foreach (TransactionHeader transaction in linesToGenerate)
				{
					rowNumber += 1;
					str.AppendLine(GetDetailsRecord(transaction, rowNumber));
				}
			}

			rowNumber += 1;
			str.AppendLine(GetTrailerRecord(rowNumber));

			return str.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Data record")]
		bool ICollectionBatchFileGenerator.AttachFileToEdoc(string fileData)
		{
			if (string.IsNullOrEmpty(fileData))
			{
				return false;
			}

			var dataContents = System.Text.Encoding.UTF8.GetBytes(fileData);
			if (dataContents == null)
			{
				return false;
			}

			var fileName = string.Format(CultureInfo.InvariantCulture, (NoResString)"Remessa{0}{1}_{2}.txt", collectionBatch.ACB_CollectionFileFormat, CollectionDate.ToString("ddMMyyyy", CultureInfo.InvariantCulture), dateTime.ToString("HHmmss", CultureInfo.InvariantCulture));
			var documentType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			var description = "Miscellaneous Document";

			var eDoc = docManagerInfo.AddFileOrDocument(dataContents.ToArray(), fileName, documentType, description: description);
			eDoc.IsPublished = true;
			((StorageDocsBase)eDoc).SC_IsSystemGenerated = true;
			docManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			docManagerInfo.Save();

			return true;
		}
	}
}
