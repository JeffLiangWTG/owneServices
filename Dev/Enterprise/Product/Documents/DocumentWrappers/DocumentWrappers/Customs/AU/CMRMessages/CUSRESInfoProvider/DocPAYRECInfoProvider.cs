using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocPAYRECInfoProvider : DocD99BCUSRESInfoProvider
	{
		DocPAYRECInfoProvider(PAYRECInfoProvider pAYRECInfoProvider, BusinessObjectFactory factory)
			: base(pAYRECInfoProvider, factory)
		{
		}

		public static DocPAYRECInfoProvider New(PAYRECInfoProvider pAYRECInfoProvider, BusinessObjectFactory factory)
		{
			return (pAYRECInfoProvider == null) ? null : new DocPAYRECInfoProvider(pAYRECInfoProvider, factory);
		}

		PAYRECInfoProvider PAYRECInfoProvider
		{
			get { return (PAYRECInfoProvider)WrappedObject; }
		}

		#region ZString Fields

		public ZString EFTRunNumber
		{
			get { return PAYRECInfoProvider.EFTRunNumber; }
		}

		public ZString ICSReceiptNumber
		{
			get { return PAYRECInfoProvider.ICSReceiptNumber; }
		}

		public ZString BankAccountName
		{
			get { return PAYRECInfoProvider.BankAccountName; }
		}

		public ZString BankAccountNumber
		{
			get { return PAYRECInfoProvider.BankAccountNumber; }
		}

		public ZString BSBNumber
		{
			get { return PAYRECInfoProvider.BSBNumber; }
		}

		#endregion

		#region Charge Items
		DocCMRMessageChargeItemCollection fCharges;
		public override DocCMRMessageChargeItemCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new DocCMRMessageChargeItemCollection(Factory);
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.TotalPayableAdmin, PAYRECInfoProvider.TotalPayableAdmin));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.AQISContainerCharges, PAYRECInfoProvider.AQISContainerCharge));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, PAYRECInfoProvider.AQISProcessingCharge));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge, PAYRECInfoProvider.DeclarationProcessingCharge));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.Woodlevy, PAYRECInfoProvider.TotalWoodLevy));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.AQISServicePaymentAmount, PAYRECInfoProvider.AQISServicePayment));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.DutyAmount, PAYRECInfoProvider.TotalPayableDuty));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.WetAmount, PAYRECInfoProvider.TotalPayableWET));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.LCTAmount, PAYRECInfoProvider.TotalPayableLCT));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.GSTAmount, PAYRECInfoProvider.TotalPayableGST));
					fCharges.Add(new DocCMRMessageChargeItem(CusEntryChargeTypeList.Descriptions.OtherCharges, PAYRECInfoProvider.TotalOtherCharges));
				}

				return fCharges;
			}
		}

		public override ZDecimal TotalPayable
		{
			get { return PAYRECInfoProvider.TotalPayable; }
		}

		#endregion
	}
}
