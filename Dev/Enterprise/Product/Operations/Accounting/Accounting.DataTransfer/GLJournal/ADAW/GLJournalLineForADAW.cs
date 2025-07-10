using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalLineForADAW : GLJournalEnvironmentForADAW , IGLJournalLine
	{
		public GLJournalLineForADAW(BusinessObjectFactory factory, GLJournalHeaderForADAW header) : base(factory)
		{
			this.header = header;
		}

		readonly GLJournalHeaderForADAW header;

		public abstract class Schema
		{
			public const string PK = "IAL_PK";
		}

		public GLJournalHeaderForADAW Header => header;

		[BusinessObjectTestExclude]
		public ZString GLAccount { get; set; }
		public ZPropertyInfo GLAccountInfo
		{
			get { return GetZPropertyInfo(nameof(GLAccount)); }
		}

		[BusinessObjectTestExclude]
		public ZString DepartmentCode { get; set; }
		public ZPropertyInfo DepartmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentCode)); }
		}

		[BusinessObjectTestExclude]
		public ZString LocalAmount { get; set; }
		public ZPropertyInfo LocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmount)); }
		}

		[BusinessObjectTestExclude]
		public ZString Currency { get; set; }
		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}

		public RefCurrency CurrencyBizO
		{
			get
			{
				if (currencyBizO == null)
				{
					if (Currency.IsEmpty)
					{
						currencyBizO = Company?.Country.LocalCurrency;
					}
					else
					{
						currencyBizO = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency);
					}
				}

				return currencyBizO;
			}
		}
		RefCurrency currencyBizO;

		[BusinessObjectTestExclude]
		public ZString Amount { get; set; }
		public ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(nameof(Amount)); }
		}

		[BusinessObjectTestExclude]
		public ZString JournalLineDescription { get; set; }
		public ZPropertyInfo JournalLineDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(JournalLineDescription)); }
		}

		[BusinessObjectTestExclude]
		public ZString OrganisationCode { get; set; }
		public ZPropertyInfo OrganisationCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationCode)); }
		}

		[BusinessObjectTestExclude]
		public ZString SubAccountType1 { get; set; }
		public ZPropertyInfo SubAccountType1Info
		{
			get { return GetZPropertyInfo(nameof(SubAccountType1)); }
		}

		[BusinessObjectTestExclude]
		public ZString SubAccountValue1 { get; set; }
		public ZPropertyInfo SubAccountValue1Info
		{
			get { return GetZPropertyInfo(nameof(SubAccountValue1)); }
		}

		[BusinessObjectTestExclude]
		public ZString SubAccountType2 { get; set; }
		public ZPropertyInfo SubAccountType2Info
		{
			get { return GetZPropertyInfo(nameof(SubAccountType2)); }
		}

		[BusinessObjectTestExclude]
		public ZString SubAccountValue2 { get; set; }
		public ZPropertyInfo SubAccountValue2Info
		{
			get { return GetZPropertyInfo(nameof(SubAccountValue2)); }
		}

		[BusinessObjectTestExclude]
		public ZString AttributeORG { get; set; }
		public ZPropertyInfo AttributeORGInfo
		{
			get { return GetZPropertyInfo(nameof(AttributeORG)); }
		}

		[BusinessObjectTestExclude]
		public ZString AttributeOCG { get; set; }
		public ZPropertyInfo AttributeOCGInfo
		{
			get { return GetZPropertyInfo(nameof(AttributeOCG)); }
		}

		[BusinessObjectTestExclude]
		public ZString AttributeLFO { get; set; }
		public ZPropertyInfo AttributeLFOInfo
		{
			get { return GetZPropertyInfo(nameof(AttributeLFO)); }
		}

		[BusinessObjectTestExclude]
		public ZString AttributeLFE { get; set; }
		public ZPropertyInfo AttributeLFEInfo
		{
			get { return GetZPropertyInfo(nameof(AttributeLFE)); }
		}

		[BusinessObjectTestExclude]
		public ZString AttributeTIC { get; set; }
		public ZPropertyInfo AttributeTICInfo
		{
			get { return GetZPropertyInfo(nameof(AttributeTIC)); }
		}

		[BusinessObjectTestExclude]
		public ZString AttributeSPR { get; set; }
		public ZPropertyInfo AttributeSPRInfo
		{
			get { return GetZPropertyInfo(nameof(AttributeSPR)); }
		}

		public GLJournalLineSubAccountForADAWCollection SubAccounts => subAccounts ?? (subAccounts = new GLJournalLineSubAccountForADAWCollection(Factory));
		GLJournalLineSubAccountForADAWCollection subAccounts;
	}

	interface IGLJournalLine : IJournalCompanyAndBranchForImport
	{
		public ZString GLAccount { get; set; }
		public ZString DepartmentCode { get; set; }
		public ZString LocalAmount { get; set; }
		public ZString Currency { get; set; }
		public ZString Amount { get; set; }
		public ZString JournalLineDescription { get; set; }
		public ZString OrganisationCode { get; set; }
		public ZString SubAccountType1 { get; set; }
		public ZString SubAccountValue1 { get; set; }
		public ZString SubAccountType2 { get; set; }
		public ZString SubAccountValue2 { get; set; }
		public ZString AttributeORG { get; set; }
		public ZString AttributeOCG { get; set; }
		public ZString AttributeLFO { get; set; }
		public ZString AttributeLFE { get; set; }
		public ZString AttributeTIC { get; set; }
		public ZString AttributeSPR { get; set; }
	}
}
