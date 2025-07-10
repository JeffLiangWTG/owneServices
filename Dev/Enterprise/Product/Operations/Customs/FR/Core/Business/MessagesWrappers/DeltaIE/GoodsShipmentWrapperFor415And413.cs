using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsShipmentWrapperFor415And413 : GoodsShipmentWrapper
	{
		GoodsShipmentWrapperFor415And413(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public new static GoodsShipmentWrapperFor415And413 New(CusEntryHeader entryHeader) => entryHeader == null ? null : new GoodsShipmentWrapperFor415And413(entryHeader);

		public override string DateOfAcceptance
		{
			get
			{
				if (dateOfAcceptance.IsNullOrEmpty())
				{
					if (entryHeader.Declaration.IsDeclarationStandard)
					{
						var previousDocumentNMRN = entryHeader.Declaration.PreviousDocuments.OfType<PreviousDocument>().FirstOrDefault(info => info.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN);
						dateOfAcceptance = previousDocumentNMRN?.CSI_DateOfIssue.ToString("yyyy-MM-ddTHH:mm:ss");
					}
					else if (entryHeader.EntryInstruction.CEI_SubStyle == "V")
					{
						dateOfAcceptance = base.DateOfAcceptance;
					}
				}
				return dateOfAcceptance;
			}
		}
		string dateOfAcceptance;
	}
}
