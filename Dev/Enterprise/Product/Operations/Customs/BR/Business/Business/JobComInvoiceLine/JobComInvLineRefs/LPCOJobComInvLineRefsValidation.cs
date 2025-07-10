using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class LPCOJobComInvLineRefsValidation : JobComInvLineRefsValidation
	{
		public LPCOJobComInvLineRefsValidation(LPCOJobComInvLineRefs parent)
			: base(parent)
		{
		}

		public new LPCOJobComInvLineRefs Parent => (LPCOJobComInvLineRefs)base.Parent;

		protected override void CheckJG_ReferenceNumber()
		{
			base.CheckJG_ReferenceNumber();

			var targetInfo = Parent.JG_ReferenceNumberInfo;
			var parentInvoiceLine = Parent.InvoiceLine;
			var referenceNumber = Parent.JG_ReferenceNumber;

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (!referenceNumber.IsEmpty)
			{
				var lpcoCollection = parentInvoiceLine.LPCOJobComInvLineRefsCollection.Cast<LPCOJobComInvLineRefs>();

				bool hasDuplicates = lpcoCollection.Any(x => x.JG_ReferenceNumber == referenceNumber && x.PK != Parent.PK);
				if (hasDuplicates)
				{
					targetInfo.AddWarning(Res.GetString("D5806E76-CF7C-4228-83AC-1BD067FF2CFB", "This LPCO code already exists in this invoice line"));
				}
			}
		}
	}
}
