using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ElectronicLogisticInvoiceValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ElectronicLogisticInvoiceValidation(ElectronicLogisticInvoice parent) : base(parent)
		{
		}

		public new ElectronicLogisticInvoice Parent => (ElectronicLogisticInvoice)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var referenceNumber = Parent.CSI_ReferenceNumber;
			var targetInfo = Parent.CSI_ReferenceNumberInfo;
			MandatoryValidation.CheckEntered(Parent.CSI_ReferenceNumberInfo);

			if (!referenceNumber.IsEmpty)
			{
				if (!referenceNumber.IsNumbersOnlyOrEmpty)
				{
					targetInfo.AddWarning(Res.GetString("da78ea4c-1116-49a4-a2b3-a9ab43c991a9", "NF-E Key allows numeric characters."));
				}
				else if (referenceNumber.Length != 44)
				{
					targetInfo.AddWarning(Res.GetString("2ce4ed52-00e7-43b3-96f1-802f5725c040", "NF-E Key must consist 44 numeric characters."));
				}
			}
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			var lineNo = Parent.CSI_LineNo;
			var referenceNo = Parent.CSI_ReferenceNumber;
			var targetInfo = Parent.CSI_LineNoInfo;

			if (!lineNo.IsEmpty)
			{
				if (Parent.Parent.ElectronicLogisticInvoiceCollection?.Cast<ElectronicLogisticInvoice>().Any(x => x.CSI_LineNo == lineNo && x.CSI_ReferenceNumber == referenceNo && x.PK != Parent.PK) ?? false)
				{
					targetInfo.AddMessageError(Res.GetString("9df5a730-cb76-4f1b-b7a4-84fc9c91073a", "NF-E Key and Item Number combinations have been duplicated and must be unique."));
				}
			}
			else
			{
				targetInfo.AddWarning(Res.GetString("7d56e8d7-ef74-4629-8753-4f16f7a70e19", "NF-E Item Number cannot enter value ZERO."));
			}
		}
	}
}
