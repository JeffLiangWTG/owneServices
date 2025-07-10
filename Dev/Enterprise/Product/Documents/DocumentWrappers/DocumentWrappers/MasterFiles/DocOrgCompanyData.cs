using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgCompanyData : DocumentWrapper
	{
		DocOrgCompanyData(OrgCompanyData orgCompanyData, BusinessObjectFactory factoryToWrap)
			: base(orgCompanyData, factoryToWrap)
		{
		}

		public static DocOrgCompanyData New(OrgCompanyData orgCompanyData, BusinessObjectFactory factoryToWrap)
		{
			if (orgCompanyData == null)
			{
				return null;
			}
			else
			{
				return new DocOrgCompanyData(orgCompanyData, factoryToWrap);
			}
		}

		OrgCompanyData OrgCompanyData
		{
			get { return (OrgCompanyData)WrappedObject; }
		}

		public override string ToString()
		{
			return OrgCompanyData.Code;
		}

		public ZString OB_APAirlineAccountNumber
		{
			get { return OrgCompanyData.OB_APAirlineAccountNumber; }
		}

		public ZString OB_ARCreditAgreedPaymentMethod
		{
			get
			{
				return OrgCompanyData.OB_ARCreditAgreedPaymentMethod_List.GetDescriptionFromCode(OrgCompanyData.OB_ARCreditAgreedPaymentMethod);
			}
		}

		public ZString OB_APCreditAgreedPaymentMethod
		{
			get
			{
				return OrgCompanyData.OB_APCreditAgreedPaymentMethod_List.GetDescriptionFromCode(OrgCompanyData.OB_APCreditAgreedPaymentMethod);
			}
		}

		public ZString OB_APAccountName
		{
			get
			{
				return OB_APDefaultAccountDetails != null ? OB_APDefaultAccountDetails.A1_AccountName : ZString.Empty;
			}
		}

		public ZString OB_APAccountNumber
		{
			get
			{
				return OB_APDefaultAccountDetails != null ? OB_APDefaultAccountDetails.A1_BankAccount : ZString.Empty;
			}
		}

		public ZString OB_APBankName
		{
			get
			{
				return OB_APDefaultAccountDetails != null ? OB_APDefaultAccountDetails.A1_BankName : ZString.Empty;
			}
		}

		public ZString OB_APBankBSB
		{
			get
			{
				return OB_APDefaultAccountDetails != null ? OB_APDefaultAccountDetails.A1_BankBsb : ZString.Empty;
			}
		}

		public ZString OB_RX_NKARDDefltCurrency
		{
			get
			{
				return OrgCompanyData.OB_RX_NKARDDefltCurrency;
			}
		}

		public ZBool OB_AROnCreditHold
		{
			get
			{
				return OrgCompanyData.OB_AROnCreditHold;
			}
		}

		public ZBool OB_ARUseSettlementGroupCreditLimit
		{
			get
			{
				return OrgCompanyData.OB_ARUseSettlementGroupCreditLimit;
			}
		}

		public ZGuid OB_GC
		{
			get
			{
				return OrgCompanyData.OB_GC;
			}
		}

		public DocOrgARTermsCollection ARTerms
		{
			get
			{
				DocOrgARTermsCollection result;
				if (OrgCompanyData != null)
				{
					result = new DocOrgARTermsCollection(OrgCompanyData.ARTerms, Factory);
				}
				else
				{
					result = new DocOrgARTermsCollection(Factory);
				}
				return result;
			}
		}

		internal ZString OB_TransactionCurrency
		{
			get;
			set;
		}

		AccAPAccountDetails OB_APDefaultAccountDetails
		{
			get
			{
				AccAPAccountDetails result = null;
				if (OrgCompanyData != null && OrgCompanyData.AccountDetailsCollection != null)
				{
					result = GetDefaultAccountForCurrency(OrgCompanyData.AccountDetailsCollection, OB_TransactionCurrency);
					if (result == null)
					{
						result = GetDefaultAccountForCurrency(OrgCompanyData.AccountDetailsCollection, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					}
					if (result == null)
					{
						foreach (AccAPAccountDetails accDetail in OrgCompanyData.AccountDetailsCollection)
						{
							if (accDetail.A1_IsDefaultAccount)
							{
								result = accDetail;
								break;
							}
						}
					}
				}
				return result;
			}
		}

		AccAPAccountDetails GetDefaultAccountForCurrency(AccountDetailsDependentCollection accountDetails, ZString currencyCode)
		{
			AccAPAccountDetails result = null;
			if (accountDetails != null)
			{
				foreach (AccAPAccountDetails accDetail in accountDetails)
				{
					if (accDetail.A1_IsDefaultAccount && accDetail.A1_PaymentMethod == AccAPAccountDetailsLookups.DefaultPayment && accDetail.A1_RX_NKAccountCurrency == currencyCode)
					{
						result = accDetail;
						break;
					}
				}
				if (result == null)
				{
					foreach (AccAPAccountDetails accDetail in accountDetails)
					{
						if (accDetail.A1_IsDefaultAccount && accDetail.A1_RX_NKAccountCurrency == currencyCode)
						{
							result = accDetail;
							break;
						}
					}
				}
			}
			return result;
		}

		public ZString GetPaymentMethodFromARTerms(ZString invoiceClass)
		{
			ZString result = ZString.Empty;
			var arTerms = OrgCompanyData.ARTerms.Cast<OrgARTerms>().FirstOrDefault(x => x.PY_InvoiceClass == invoiceClass);
			if (arTerms != null && !arTerms.PY_AgreedPaymentMethod.IsEmpty)
			{
				result = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList().GetDescriptionFromCode(arTerms.PY_AgreedPaymentMethod);
			}
			return result;
		}
	}
}
