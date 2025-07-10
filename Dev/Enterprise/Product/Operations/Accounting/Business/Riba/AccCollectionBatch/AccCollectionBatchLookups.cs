using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionBatchLookups : AutoAccCollectionBatchLookups
	{
		public AccCollectionBatchLookups(AutoAccCollectionBatch parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ACB_CollectionFileFormat_List
		{
			get
			{
				var result = new CollectionFileFormatList();
				var countryCode = GlbCompany.CurrentCompany.Country.Code;
				result.RemoveCode(CollectionFileFormatList.Codes.itauBank);
				if (countryCode == Core.Constants.CountryCodes.Brazil)
				{
					result.Clear();
					result.AddPair(CollectionFileFormatList.Codes.itauBank, CollectionFileFormatList.Descriptions.itauBank);
				}
				if (!GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion &&
					(countryCode != Core.Constants.CountryCodes.Iceland ||
					countryCode != Core.Constants.CountryCodes.Liechtenstein ||
					countryCode != Core.Constants.CountryCodes.Norway))
				{
					result.RemoveCode(CollectionFileFormatList.Codes.sepaFormat);
				}
				if (countryCode != Core.Constants.CountryCodes.Italy)
				{
					result.RemoveCode(CollectionFileFormatList.Codes.ribaFormat);
				}
				return result;
			}
		}

		public override AccBankAccountCollection BankAccounts
		{
			get
			{
				if (bankAccounts == null)
				{
					bankAccounts = AccountingUtils.GetAccBankAccountCollection(Factory);
				}

				return bankAccounts;
			}
		}
		AccBankAccountCollection bankAccounts;

		public RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public CodeDescriptionPairList BatchTypes
		{
			get
			{
				return AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value.GetCodeDescriptionPairList();
			}
		}
	}
}

