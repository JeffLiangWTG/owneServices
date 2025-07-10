using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTransferBill : ManifestBase.AsycudaTransferBill
	{
		public AsycudaTransferBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaTransferHeader TransferHeader => (AsycudaTransferHeader)base.TransferHeader;

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferBill|ATB_BillNumber", Caption = "Bill Number")]
		public override ZString ATB_BillNumber
		{
			get => base.ATB_BillNumber;
			set => base.ATB_BillNumber = value;
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferBill|ATB_MessageStatus", Caption = "Message Status")]
		public override ZString ATB_MessageStatus
		{
			get => base.ATB_MessageStatus;
			set => base.ATB_MessageStatus = value;
		}

		public new AsycudaTransferBillLookups Lookups => (AsycudaTransferBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaTransferBillLookups GetNewLookups() => new AsycudaTransferBillLookups(this);
		public new AsycudaTransferBillValidation Validation => (AsycudaTransferBillValidation)base.Validation;
		protected override ManifestBase.AsycudaTransferBillValidation GetNewValidation() => new AsycudaTransferBillValidation(this);
	}
}
