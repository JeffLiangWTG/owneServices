using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	class ChargeSplitterByChargeCountTestHelper
	{
		readonly GlbBranch chinaBranch;
		readonly OrgHeader fChinaClient;
		readonly OrgHeader fChinaClient2;
		readonly OrgHeader fAusClient;
		readonly Job fJob;

		readonly RefCurrency fChinaCurrency;
		readonly RefCurrency fAusCurrency;

		readonly BusinessObjectFactory fFactory;

		public ChargeSplitterByChargeCountTestHelper(BusinessObjectFactory factory)
		{
			fFactory = factory;

			chinaBranch = ObjectCreator.CreateNewBranch(ObjectCreator.CreateNewCompany("COM", Core.Constants.CountryCodes.China), "RNC");
			fFactory.Save();

			fChinaClient = fFactory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			var address = fChinaClient.Addresses.AddNew(OrgAddressType.Office, true);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			fChinaClient2 = fFactory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			address = fChinaClient2.Addresses.AddNew(OrgAddressType.Office, true);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			fAusClient = fFactory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			address = fAusClient.Addresses.AddNew(OrgAddressType.Office, true);
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			fChinaCurrency = fFactory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "CNY") as RefCurrency;
			fAusCurrency = fFactory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "AUD") as RefCurrency;

			fJob = fFactory.NewJobWithValidTestDataForTesting<Job>();
			fJob.JH_JobNum = "S001";
		}

		public Job Job
		{
			get { return fJob; }
		}

		public GlbBranch ChinaBranch
		{
			get { return chinaBranch; }
		}

		public GlbDepartment Department
		{
			get { return ObjectCreator.FESDepartment; }
		}

		public OrgHeader ChinaClient
		{
			get { return fChinaClient; }
		}

		public OrgHeader ChinaClient2
		{
			get { return fChinaClient2; }
		}

		public OrgHeader AusClient
		{
			get { return fAusClient; }
		}

		public RefCurrency ChinaCurrency
		{
			get { return fChinaCurrency; }
		}

		public RefCurrency AusCurrency
		{
			get { return fAusCurrency; }
		}

		public IReceivablesPostingChargeCollection GetChargeCollection(int chargeCount, OrgHeader debtor, GlbBranch branch = null, ZString placeOfSupply = default, GlbBranch taxBranch = null)
		{
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();

			for (int i = 0; i < chargeCount; i++)
			{
				Charge charge = fFactory.New<Charge>();
				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_OA_SellInvoiceAddress = debtor.Addresses[0].PK;
				charge.JR_LocalSellAmt = 250 + i * 10;
				if (branch != null)
				{
					charge.JR_GB = branch.PK;
				}
				if (!placeOfSupply.IsDefault)
				{
					charge.JR_SellPlaceOfSupply = placeOfSupply;
				}
				if (taxBranch != null)
				{
					charge.JR_GB_SellTaxBranch = taxBranch.PK;
				}
				charges.Add(charge);
			}

			return charges;
		}

		TestObjectCreator ObjectCreator { get { return objectCreator ?? (objectCreator = new TestObjectCreator(fFactory)); } }
		TestObjectCreator objectCreator;
	}
}
