using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public partial class DocAPInvoice : DocARInvoiceCommon
	{
		#region Construction

		protected DocAPInvoice(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
			: base(invoicingBase, factoryToWrap)
		{
		}

		public static DocAPInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
		{
			DocAPInvoice result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(invoicingBase, factoryToWrap);
			}
			else if (invoicingBase != null)
			{
				result = new DocAPInvoice(invoicingBase, factoryToWrap);
			}

			return result;
		}

		public static DocAPInvoice New(AccTransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap)
		{
			DocAPInvoice result = null;

			if (transactionHeader != null)
			{
				var invoice = factoryToWrap.Load<TransactionHeader>(transactionHeader.PK);
				result = DocAPInvoice.New((InvoicingBase)invoice, factoryToWrap);
			}

			return result;
		}

		protected new delegate DocAPInvoice NewDelegate(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Properties

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZString RecipientTaxIDNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (Organisation != null && Organisation.Country != null)
				{
					if (Organisation.Country.Code == Core.Constants.CountryCodes.Netherlands)
					{
						DocOrganisation proxy = CurrentBranch.Organisation ?? CurrentCompany.Organisation;
						if (proxy != null)
						{
							result = proxy.LocalVATCode;
						}
					}
					else if (IsTaxed)
					{
						if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.India && TransactionHeader.Header != null && TransactionHeader.Header.CountryCode == Core.Constants.CountryCodes.India)
						{
							var gstCode = TransactionHeader.Header.RawTaxRegistrationNumber;
							if (!string.IsNullOrWhiteSpace(gstCode))
							{
								result = gstCode;
							}
							else
							{
								result = TransactionHeader.Header.UINCodeForIndia;
							}
						}
						else if (Organisation.Country.Code == Core.Constants.CountryCodes.SouthAfrica)
						{
							DocOrganisation proxy = CurrentBranch.Organisation ?? CurrentCompany.Organisation;
							if (proxy != null)
							{
								result = proxy.LocalBusinessRegNo;
							}
						}
						else if (Organisation.Country.Code == Core.Constants.CountryCodes.Taiwan)
						{
							DocOrganisation proxy = CurrentBranch.Organisation ?? CurrentCompany.Organisation;
							if (proxy != null)
							{
								result = proxy.LocalVATCode;
							}
						}
						else if (TransactionHeader.Header.Country.IsPartOfEuropeanUnion || TransactionHeader.Header.Country.RN_Code == Core.Constants.CountryCodes.UnitedKingdom)
						{
							if (Organisation.Country.Code == Core.Constants.CountryCodes.Spain && HasREGLineForSpain)
							{
								result = TransactionHeader.Header.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_CodeType == OrgCusCode.SpainCodeTypes.IGC).Select(x => x.OK_CustomsRegNo).FirstOrDefault();
							}
							else
							{
								OrgHeader proxy = TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy;
								if (proxy != null && !proxy.RawTaxRegistrationNumber.IsEmpty)
								{
									result = proxy.TaxRegistrationNumber;
								}
							}
						}
						else if (TransactionHeader.Header.Country.Code == Core.Constants.CountryCodes.Philippines)
						{
							OrgHeader proxy = TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy;
							if (proxy != null)
							{
								result = proxy.RawTaxRegistrationNumber;
							}
						}
						else if (result == ZString.Empty)
						{
							DocOrganisation proxy = CurrentBranch.Organisation ?? CurrentCompany.Organisation;
							if (proxy != null)
							{
								result = proxy.BusinessRegNo;
							}
						}
					}
				}

				return result;
			}
		}

		public override ZString RecipientTaxIDHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (Organisation != null && Organisation.Country != null)
				{
					if (Organisation.Country.Code == Core.Constants.CountryCodes.Netherlands)
					{
						DocOrganisation proxy = CurrentBranch.Organisation ?? CurrentCompany.Organisation;
						if (proxy != null && proxy.LocalVATCode != ZString.Empty)
						{
							result = Res.GetString("dfa161ec-a885-4616-8564-c323a20388a4", "Client VAT #:");
						}
					}
					else if (IsTaxed)
					{
						if (CurrentCompany.Country.Code == Core.Constants.CountryCodes.India && TransactionHeader.Header != null)
						{
							if (!string.IsNullOrWhiteSpace(TransactionHeader.Header.RawTaxRegistrationNumber))
							{
								result = Res.GetString("51718acc-b768-4d9a-9168-9c65fb43ed57", "Client GSTIN #:");
							}
							else if (!string.IsNullOrWhiteSpace(TransactionHeader.Header.UINCodeForIndia))
							{
								result = Res.GetString("10f7911a-26fa-4087-8af6-f2536bcd09aa", "Client UIN #:");
							}
						}
						else if (Organisation.Country.Code == Core.Constants.CountryCodes.SouthAfrica)
						{
							result = Res.GetString("dfa161ec-a885-4616-8564-c323a20388a4", "Client VAT #:");
						}
						else if (Organisation.Country.Code == Core.Constants.CountryCodes.Taiwan)
						{
							DocOrganisation proxy = CurrentBranch.Organisation ?? CurrentCompany.Organisation;
							if (proxy != null && proxy.LocalVATCode != ZString.Empty)
							{
								result = Res.GetString("b6461efb-dc7c-4329-9552-8a167beb32f5", "Client Tax #:");
							}
						}

						if (TransactionHeader.Header.Country.Code == Core.Constants.CountryCodes.Spain)
						{
							if (HasREGLineForSpain && IsIGICRecordedAgainstOrg(TransactionHeader.Header))
							{
								result = Res.GetString("7dbdfe61-8420-4da7-af28-d5f6d1487444", "Client NIF #:");
							}
							else
							{
								result = Res.GetString("dfa161ec-a885-4616-8564-c323a20388a4", "Client VAT #:");
							}
						}
						if (TransactionHeader.Header.Country.IsPartOfEuropeanUnion || TransactionHeader.Header.Country.RN_Code == Core.Constants.CountryCodes.UnitedKingdom)
						{
							result = Res.GetString("5c4666fa-d867-45ff-8993-78eba17b3713", "Client {0} #:", GetTranslatedTaxCodeFromCountryCode(TransactionHeader.Header.Country.Code));
						}
						else if (TransactionHeader.Header.Country.Code == Core.Constants.CountryCodes.Philippines)
						{
							result = Res.GetString("4ecc9fb8-07eb-4f8f-9eba-29aa2d162874", "TIN:");
						}
						else if (result == ZString.Empty)
						{
							result = Res.GetString("8b7c699a-8b50-4bac-bed4-e7118a508c9e", "Recipient Tax #:");
						}
					}
				}

				return result;
			}
		}

		public ZString SupplierTaxIDNumber
		{
			get { return base.RecipientTaxIDNumber; }
		}

		public ZString SupplierTaxIDHeading
		{
			get
			{
				ZString result;
				result = base.RecipientTaxIDHeading;
				if (base.IsTaxed && result == ZString.Empty)
				{
					result = Res.GetString("f0fbdafd-6477-43a2-88d0-155b14153823", "Supplier Tax #:");
				}
				return result;
			}
		}

		protected override ZBool HasREGLineForSpain
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(x => x.TaxRate != null && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.Spain && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.RegionalTax);
			}
		}

		public override ZBool IsTaxed
		{
			get
			{
				return InvoicingBase.IsTaxed;
			}
		}

		public ZString SupplierNameAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (Organisation != null)
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
						result = Organisation.PostalAddressExcludeCountryIfSame;
					}
				}
				return result;
			}
		}

		protected override DocARInvoiceLineCollection GetInvoiceLines()
		{
			return new DocARInvoiceLineCollection(Factory);
		}

		protected override DocARInvoiceLineCollection LinesForInvoiceCore()
		{
			return Lines;
		}

		public DocARInvoiceLineCollection LinesForCostConfirmationSummary
		{
			get
			{
				DocARInvoiceLineCollection result = Lines;
				DocARInvoiceLineCollection groupedLines = null;

				switch (CostConfirmationRollupSetting)
				{
					case DocRollUpConstants.CostConfirmationDocumentRollupSettingsCodes.ChargeCode:
						groupedLines = new RollupGrouperCCD(DocLineRollUpper, this, InvoicingBase.Factory, Lines).RollUp();
						break;
					case DocRollUpConstants.CostConfirmationDocumentRollupSettingsCodes.Job:
						groupedLines = new RollupGrouperJob(DocLineRollUpper, this, InvoicingBase.Factory, Lines).RollUp();
						break;
				}

				if (groupedLines != null && groupedLines.Count > 0)
				{
					result = groupedLines;
				}

				return result;
			}
		}

		#endregion

		#region Document Title & Message

		protected override ZString TaxInvoiceTitle
		{
			get { return AccountingConfigurationRegistry.Instance.TaxSelfBilledInvoiceTitle.Value; }
		}

		protected override ZString NonTaxInvoiceTitle
		{
			get { return AccountingConfigurationRegistry.Instance.NonTaxSelfBilledInvoiceTitle.Value; }
		}

		protected override ZString TaxCreditNoteTitle
		{
			get { return AccountingConfigurationRegistry.Instance.TaxSelfBilledCreditNoteTitle.Value; }
		}

		protected override ZString NonTaxCreditNoteTitle
		{
			get { return AccountingConfigurationRegistry.Instance.NonTaxSelfBilledCreditNoteTitle.Value; }
		}

		protected override ZString TaxAdjustmentNoteTitle
		{
			get { return AccountingConfigurationRegistry.Instance.TaxSelfBilledAdjustmentNoteTitle.Value; }
		}

		protected override ZString NonTaxAdjustmentNoteTitle
		{
			get { return AccountingConfigurationRegistry.Instance.NonTaxSelfBilledAdjustmentNoteTitle.Value; }
		}

		protected override string InvoiceMessage
		{
			get { return AccountingConfigurationRegistry.Instance.SelfBilledInvoiceMessage.Value; }
		}

		protected override string CreditNoteMessage
		{
			get { return AccountingConfigurationRegistry.Instance.SelfBilledCreditNoteMessage.Value; }
		}

		protected override string AdjustmentNoteMessage
		{
			get { return AccountingConfigurationRegistry.Instance.SelfBilledAdjustmentNoteMessage.Value; }
		}

		#endregion

		protected override ZString GetSupplierTaxIDNumberCore()
		{
			return SupplierTaxIDNumber;
		}

		protected override ZString GetSupplierTaxIDHeadingCore()
		{
			return SupplierTaxIDHeading;
		}

		protected override ZBool GetIsTaxedCore()
		{
			return IsTaxed;
		}

		protected override ZString GetApprovalRequestID()
		{
			var request = InvoicingBase.ApprovalRequestForAP;
			return request != null ? request.XP_RequestID : ZString.Empty;
		}

		protected override DocGenericTransactionLineCollection GetCostConfirmationSummaryLinesCore()
		{
			var result = new DocGenericTransactionLineCollection(InvoicingBase.Factory);

			foreach (IDocARInvoiceLine line in LinesForCostConfirmationSummary)
			{
				if (line is DocARInvoiceLine)
				{
					result.Add(DocGenericTransactionLine.New((DocARInvoiceLine)line, Factory));
				}
				else if (line is DocARInvoiceLineForRollUp)
				{
					result.Add(DocGenericTransactionLine.New((DocARInvoiceLineForRollUp)line, Factory));
				}
			}

			return result;
		}
	}
}
