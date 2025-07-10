using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class DocumentRequestSendingAction : EU.H7.Business.DocumentRequestSendingAction
{
	public DocumentRequestSendingAction(AsycudaBill bill)
		: base(bill)
	{
	}

	public new AsycudaBill Bill => (AsycudaBill)base.Bill;

	[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.DocumentRequestSendingAction|G3LRN", Caption = "G3 Local Reference Number", ShortCaption = "LRN (G3)", FullDescription = "A system-generated local reference number to uniquely identify each single G3 declaration.")]
	public ZString G3LocalReferenceNumber => Bill.G3LocalReferenceNumber;

	public ZPropertyInfo G3LocalReferenceNumberInfo => GetZPropertyInfo(nameof(G3LocalReferenceNumber));

	[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.DocumentRequestSendingAction|G3MRN", Caption = "G3 Movement Reference Number", ShortCaption = "MRN (G3)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
	public ZString G3MovementReferenceNumber => Bill.G3MovementReferenceNumber;

	public ZPropertyInfo G3MovementReferenceNumberInfo => GetZPropertyInfo(nameof(G3MovementReferenceNumber));

	[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.DocumentRequestSendingAction|H7MRN", Caption = "H7 Movement Reference Number", ShortCaption = "MRN (H7)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
	public ZString H7MovementReferenceNumber => Bill.H7MovementReferenceNumber;

	public ZPropertyInfo H7MovementReferenceNumberInfo => GetZPropertyInfo(nameof(H7MovementReferenceNumber));
}
