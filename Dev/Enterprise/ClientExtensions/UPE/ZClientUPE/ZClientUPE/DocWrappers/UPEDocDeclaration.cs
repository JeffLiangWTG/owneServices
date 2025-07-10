using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDocDeclaration : DocDeclaration, ICommercialInvoiceSupportable
	{
		#region Factory Method

		protected UPEDocDeclaration(JobDeclaration declaration, BusinessObjectFactory factoryToWrap)
			: base(declaration, factoryToWrap)
		{
		}

		public new static DocDeclaration New(JobDeclaration declaration, BusinessObjectFactory factoryToWrap)
		{
			if (declaration == null)
			{
				return null;
			}
			else
			{
				return new UPEDocDeclaration(declaration, factoryToWrap);
			}
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		internal static bool IsSubTypeRegistered
		{
			get { return OverridableNewDelegate.IsOverriden; }
		}

		#endregion

		#region Related Business Objects

		public DocCallout Callout
		{
			get { return DocCallout.New(JobDeclaration.Callout, Factory); }
		}

		#endregion

		#region New Properties

		public ZString MasterBillNoList
		{
			get { return JobDeclaration.Bills.MasterBillsCommaSeparated; }
		}

		public Image CommercialInvoiceImage
		{
			get { return JobDeclaration.CommercialInvoiceImage; }
		}

		public ZBool HasCommercialInvoice
		{
			get { return JobDeclaration.HasCommercialInvoice; }
		}

		#endregion

		#region Alternate Broker

		public ZDateTime AlternateBrokerStorageFeeStartDate
		{
			get { return JobDeclaration.AlternateBrokerStorageFeeStartDate; }
		}

		public ZDecimal AlternateBrokerStorageFeeIncludingGST
		{
			get { return JobDeclaration.AlternateBrokerStorageFeeIncludingGST; }
		}

		public ZDecimal FinanceFreightChargeIncludingGST
		{
			get { return JobDeclaration.Callout == null ? (ZDecimal)0m : JobDeclaration.Callout.FinanceFreightChargeIncludingGST; }
		}

		public ZDecimal FinanceSecurityFeeIncludingGST
		{
			get { return JobDeclaration.Callout == null ? (ZDecimal)0m : JobDeclaration.Callout.FinanceSecurityFeeIncludingGST; }
		}

		public ZDecimal FinanceTerminalFeeAmountIncludingGST
		{
			get { return JobDeclaration.Callout == null ? (ZDecimal)0m : JobDeclaration.Callout.FinanceTerminalFeeAmountIncludingGST; }
		}

		public ZDecimal EstimatedFreight
		{
			get { return JobDeclaration.JobComInvoiceGroupHeaders[0].Charges.GetCharge(AUChargeCodeList.Codes.OverseasFreight, GlbCompany.CurrentCompany.LocalCurrency); }
		}

		public ZDecimal AlternateBrokerAmountPayable
		{
			get { return FinanceFreightChargeIncludingGST + AlternateBrokerStorageFeeIncludingGST + FinanceTerminalFeeAmountIncludingGST + FinanceSecurityFeeIncludingGST; }
		}

		public ZBool HasAlternateBroker
		{
			get { return JobDeclaration.HasAlternateBroker; }
		}

		public ImageWrapperCollection CommercialInvoiceAsMultiPageImageCollection
		{
			get
			{
				var commercialInvoiceAsMultiPageImageCollection = new ImageWrapperCollection(Factory);

				if (CommercialInvoiceImage != null)
				{
					commercialInvoiceAsMultiPageImageCollection = new ImageWrapperCollection(Factory);

					using (StandardImagePageSelector imageSelector = new StandardImagePageSelector(CommercialInvoiceImage))
					{
						for (int i = 0; i < imageSelector.PageSelector.TotalPages; i++)
						{
							imageSelector.PageSelector.CurrentPageIndex = i;
							commercialInvoiceAsMultiPageImageCollection.Add(imageSelector.PageSelector.CurrentImage);
						}
					}
				}

				return commercialInvoiceAsMultiPageImageCollection;
			}
		}

		#endregion

		#region Implementation

		protected new UPEJobDeclaration JobDeclaration
		{
			get { return (UPEJobDeclaration)base.JobDeclaration; }
		}

		#endregion
	}
}
