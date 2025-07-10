using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Manifest.H7.Business.BusinessObjects.Interfaces;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class UploadDocumentsSendingAction : EU.H7.Business.UploadDocumentsSendingAction, IH7CommonMessageSendingObject
	{
		public UploadDocumentsSendingAction(AsycudaBill bill) : base(bill)
		{
			this.bill = bill;
		}

		public new class Schema : AutoMessageSendingObject.Schema
		{
			public const string ClearanceRequested = "ClearanceRequested";
		}
		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		[ResourceStringData("Enterprise.Customs.ES.Manifest.H7.Business.UploadDocumentsSendingAction|ClearanceRequested", Caption = "Clearance Requested")]
		public ZBool ClearanceRequested
		{
			get { return clearanceRequested; }
			set
			{
				SetNonPersistentPropertyValue(ClearanceRequestedInfo, ref clearanceRequested, value);
			}
		}
		ZBool clearanceRequested;

		public ZPropertyInfo ClearanceRequestedInfo => GetZPropertyInfo(Schema.ClearanceRequested);

		AsycudaBill IH7CommonMessageSendingObject.Bill => bill;
		readonly AsycudaBill bill;

		protected override void SetDefaultData()
		{
			base.SetDefaultData();
			ClearanceRequested = true;
			Action = DeclarationMessageTypeList.Codes.H7Annexes;
		}

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.UploadDocumentsSendingAction|G3LRN", Caption = "G3 Local Reference Number", ShortCaption = "LRN (G3)", FullDescription = "A system-generated local reference number to uniquely identify each single G3 declaration.")]
		public ZString G3LocalReferenceNumber => Bill.G3LocalReferenceNumber;

		public ZPropertyInfo G3LocalReferenceNumberInfo => GetZPropertyInfo(nameof(G3LocalReferenceNumber));

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.UploadDocumentsSendingAction|G3MRN", Caption = "G3 Movement Reference Number", ShortCaption = "MRN (G3)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		public ZString G3MovementReferenceNumber => Bill.G3MovementReferenceNumber;

		public ZPropertyInfo G3MovementReferenceNumberInfo => GetZPropertyInfo(nameof(G3MovementReferenceNumber));

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.UploadDocumentsSendingAction|H7MRN", Caption = "H7 Movement Reference Number", ShortCaption = "MRN (H7)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		public ZString H7MovementReferenceNumber => Bill.H7MovementReferenceNumber;

		public ZPropertyInfo H7MovementReferenceNumberInfo => GetZPropertyInfo(nameof(H7MovementReferenceNumber));
	}
}
