using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceFeeLookups : AutoClientLicenceFeeLookups
	{
		public ClientLicenceFeeLookups(AutoClientLicenceFee parent) : base(parent)
		{
		}

		protected new ClientLicenceFee Parent
		{
			get { return (ClientLicenceFee)base.Parent; }
		}

		#region Fee Type

		public ReadOnlyCodeDescriptionPairList FeeTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(EDIDataRegistry.Instance.LicenceFeeTypes.Value);
				return result;
			}
		}

		#endregion

		#region System Codes

		public CodeDescriptionPairList SystemCodes
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(BillingConstants.BillingSystem.ODM, "Show on Billing ODPL/STL Report/Invoice");
				result.AddPair(BillingConstants.BillingSystem.Maintenance, "Show on Billing Maintenance Report/Invoice");
				return result;
			}
		}

		#endregion

		#region Databases

		public LicenceDatabaseCollectionView Databases
		{
			get
			{
				if (databases == null)
				{
					if (Parent.Company != null)
					{
						databases = new ActiveOrAllLicenceDatabaseCollection(Parent.Company.LicDatabases);
					}
					else
					{
						databases = new LicenceDatabaseCollectionView(new LicenceDatabaseNonDependentCollection(Factory, ZQuery.NoResultQuery));
					}
				}

				return databases;
			}
		}

		LicenceDatabaseCollectionView databases;

		#endregion

		public CodeDescriptionPairList TaxDateCodes => AllTaxDateCodes;

		public static CodeDescriptionPairList AllTaxDateCodes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(BillingConstants.Fee.TaxDateCode.FeeTaxAtCurrentDate, BillingConstants.Fee.TaxDateCode.Descriptions.FeeTaxAtInvoiceDate);
				result.AddPair(BillingConstants.Fee.TaxDateCode.FeeTaxAtStartDate, BillingConstants.Fee.TaxDateCode.Descriptions.FeeTaxAtStartDate);
				result.AddPair(BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate, BillingConstants.Fee.TaxDateCode.Descriptions.FeeTaxAtEndDate);
				return result;
			}
		}
	}
}

