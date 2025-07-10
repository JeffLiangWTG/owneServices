using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.Rohlig.DocWrappers
{
	public class DocROHARInvoice : DocARInvoice
	{
		#region Constructors and Type Overriding

		protected DocROHARInvoice(InvoicingBase invoicingBase, BusinessObjectFactory factory)
			: base(invoicingBase, factory)
		{
		}

		public new static DocROHARInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return (invoicingBase != null) ? new DocROHARInvoice(invoicingBase, factory) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocROHARInvoice OverriddenNewMethod(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return DocROHARInvoice.New(invoicingBase, factory);
		}

		#endregion

		#region New Fields

		public ZString AccountCodeOrDBRegistrationNumber
		{
			get
			{
				OrgHeader org = Factory.Load<OrgHeader>(Invoice.AH_OH);
				if (org != null)
				{
					ZString accountCode = ZString.Empty;
					if (!org.OH_IsGlobalAccount)
					{
						accountCode = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, Core.Constants.CountryCodes.Germany);
					}
					else
					{
						accountCode = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					}

					return accountCode.IsEmpty ? org.OH_Code : accountCode;
				}

				return ZString.Empty;
			}
		}

		public override ZBool PrintClientSpecific
		{
			get { return RohDataRegistry.Instance.UseRohligSpecificInvoiceLayout; }
		}

		public override ZBool PrintStandard
		{
			get { return !RohDataRegistry.Instance.UseRohligSpecificInvoiceLayout; }
		}

		#endregion
	}
}

#region Override
#endregion
#region Set Up
#endregion
