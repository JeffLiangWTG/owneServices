using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ComplementaryLogisticInvoiceValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ComplementaryLogisticInvoiceValidation(ComplementaryLogisticInvoice parent) : base(parent)
		{
		}

		public new ComplementaryLogisticInvoice Parent => (ComplementaryLogisticInvoice)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var referenceNumber = Parent.CSI_ReferenceNumber;
			var targetInfo = Parent.CSI_ReferenceNumberInfo;

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			if (!referenceNumber.IsEmpty)
			{
				if (!referenceNumber.IsNumbersOnlyOrEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("da78ea4c-1116-49a4-a2b3-a9ab43c991a9", "NF-E Key allows numeric characters."));
				}
				else if (referenceNumber.Length != 44)
				{
					targetInfo.AddMessageError(Res.GetString("2ce4ed52-00e7-43b3-96f1-802f5725c040", "NF-E Key must consist 44 numeric characters."));
				}
				else if ((!Parent.Parent.JI_NFeNumber.IsEmpty) && (Parent.Parent.JI_NFeNumber == referenceNumber))
				{
					targetInfo.AddMessageError(Res.GetString("7fe8569c-9efc-4838-9db1-5b02be24a0a5", "Complementary NF-E Key must not be as the same as NF-E Key"));
				}
				else if ((!Parent.SupplierCNPJ.IsEmpty) && (Parent.SupplierCNPJ != referenceNumber.Substring(6, 14)))
				{
					targetInfo.AddMessageError(Res.GetString("08468670-e59c-47b2-883c-6a5aaebe6782", "Complementary NF-E Key was not issued by the supplier"));
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
				if (Parent.Parent.ComplementaryLogisticInvoiceCollection?.Cast<ComplementaryLogisticInvoice>().Any(x => x.CSI_LineNo == lineNo && x.CSI_ReferenceNumber == referenceNo && x.PK != Parent.PK) ?? false)
				{
					targetInfo.AddMessageError(Res.GetString("9df5a730-cb76-4f1b-b7a4-84fc9c91073a", "NF-E Key and Item Number combinations have been duplicated and must be unique."));
				}
			}
			else
			{
				targetInfo.AddMessageError(Res.GetString("7d56e8d7-ef74-4629-8753-4f16f7a70e19", "NF-E Item Number cannot enter value ZERO."));
			}
		}
	}
}
