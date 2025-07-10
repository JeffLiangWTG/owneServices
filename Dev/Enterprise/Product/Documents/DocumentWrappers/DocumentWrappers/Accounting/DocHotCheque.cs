using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocHotCheque : DocBaseWrapperWithJobHeader
	{
		protected DocHotCheque(AccHotCheque hotCheque, BusinessObjectFactory factoryToWrap)
			: base(hotCheque, factoryToWrap)
		{
		}

		public static DocHotCheque New(AccHotCheque hotCheque, BusinessObjectFactory factoryToWrap)
		{
			return (hotCheque == null) ? null : new DocHotCheque(hotCheque, factoryToWrap);
		}

		AccHotCheque HotCheque
		{
			get { return (AccHotCheque)WrappedObject; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZInt FirstLinePaymentWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.FirstLinePaymentWidth, 0); }
		}

		#region Cheque Fields

		protected DocCheque fCheque;
		public DocCheque Cheque
		{
			get
			{
				if (fCheque == null)
				{
					fCheque = new DocCheque(ChequePayee, Amount, Currency, FirstLinePaymentWidth);
				}
				return fCheque;
			}
		}
		public ZString TransactionType
		{
			get { return "HCQ"; }
		}

		#endregion

		public ZBool ActualAmountIndicator
		{
			get { return HotCheque.AQ_Calc_ActualAmountIndicator; }
		}

		public ZBool MaximumAmountIndicator
		{
			get { return HotCheque.AQ_Calc_MaximumAmountIndicator; }
		}

		public DocCurrency Currency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, HotCheque.AQ_Calc_RX_NK);
				return DocCurrency.New(currency, Factory);
			}
		}

		public ZString ChequeStatus
		{
			get { return HotCheque.AQ_Calc_ChequeStatus; }
		}

		public ZString CreatingUser
		{
			get { return HotCheque.CreatingUser; }
		}

		public ZDateTime CreatedDate
		{
			get { return HotCheque.CreatedDate; }
		}

		#region Client Specific Fail Safes
		/* To avoid client specific cheque templates from failing. 
		 * These properties are used by UTI Cheque Templates while their counter-parts reside in
		 * ClientSharedComponents.DocAPPaymentShared().
		 * 
		 * DO NOT CALL THESE PROPERTIES.  The stop the report engine from failing and therefore
		 * not rendering the entire row for non-developer/debug situations.
		 * 
		 * Isolate these properties in the template with #if "<TransactionType>" != "HCQ".
		 */
		public ZString RemittanceAdviceDetailsHeadings
		{
			get { return ZString.Empty; }
		}

		public ZString RemittanceAdviceDetails
		{
			get { return ZString.Empty; }
		}
		#endregion

		public ZString ActualOrMaxIndicator
		{
			get { return HotCheque.AQ_ActualOrMaxIndicator; }
		}

		public DocTransactionHeader TransactionHeader
		{
			get { return DocTransactionHeader.New((TransactionHeader)HotCheque.TransactionHeader, Factory); }
		}

		public ZDecimal Amount
		{
			get { return HotCheque.AQ_Amount; }
		}

		public DocBankAccount BankAccount
		{
			get
			{
				DocBankAccount result = null;
				if (HotCheque != null && HotCheque.ChequeBook != null)
				{
					result = DocBankAccount.New(HotCheque.ChequeBook.BankAccount, Factory);
				}
				return result;
			}
		}

		public ZString RoutingTransitNumber
		{
			get { return BankAccount != null ? BankAccount.BSB : ZString.Empty; }
		}

		public ZString RoutingTransitNumberFractionForm
		{
			get
			{
				return DocTransactionHeader.BuildRoutingTransitNumberFractionForm(RoutingTransitNumber);
			}
		}

		public ZString MICRNumber
		{
			get
			{
				return DocTransactionHeader.BuildMICRNumber(HotCheque.AQ_ChequeNumber, RoutingTransitNumber, BankAccount);
			}
		}

		public ZBool Cancelled
		{
			get { return HotCheque.AQ_Cancelled; }
		}

		public ZDateTime ChequeDate
		{
			get { return HotCheque.AQ_ChequeDate; }
		}

		public ZString ChequeOrReference
		{
			get { return ChequeNumber; }
		}

		public ZString ChequeNumber
		{
			get { return HotCheque.AQ_ChequeNumber; }
		}

		public ZString ChequePayee
		{
			get { return HotCheque.AQ_ChequePayee; }
		}

		public ZString Description
		{
			get { return HotCheque.AQ_Description; }
		}

		public DocStaff Staff
		{
			get { return DocStaff.New(HotCheque.ResponsibleStaff, Factory); }
		}

		public ZString HouseBill
		{
			get { return HotCheque.AQ_HouseBill; }
		}

		public override DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(HotCheque.Job, Factory); }
		}

		public ZString MasterBill
		{
			get { return HotCheque.AQ_MasterBill; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(HotCheque.Header, Factory); }
		}

		internal ZString RemittanceAdviceContact
		{
			get
			{
				if (Organisation != null)
				{ return Organisation.PostalAddress; }
				else
				{ return ZString.Empty; }
			}
		}

		public ZString ChequePayToWithAddress
		{
			get
			{
				ZString result = ZString.Empty;

				if (HotCheque.Header != null)
				{
					foreach (DocAddress docAddress in Organisation.Addresses)
					{
						if (docAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Payables))
						{
							result = docAddress.PostalAddressExcludeCountryIfSame;
							break;
						}
					}

					if (result.IsEmpty)
					{
						result = Organisation.PostalAddress;
					}
				}

				return result;
			}
		}

		public ZString TransactionNumber
		{
			get { return (HotCheque != null && HotCheque.TransactionHeader != null) ? HotCheque.TransactionHeader.AH_TransactionNum : ZString.Empty; }
		}
	}
}
