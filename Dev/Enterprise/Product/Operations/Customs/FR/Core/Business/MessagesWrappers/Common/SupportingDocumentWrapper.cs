using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class SupportingDocumentWrapper : DocumentWrapper, ISupportingDocumentOnly
	{
		public SupportingDocumentWrapper(SupportingDocument supportingDocument, CusEntryLine entryLine)
			: base(supportingDocument)
		{
			itemEntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			isCodeAPermitType = supportingDocument.IsCodeAPermitType;
		}

		#region Supporting document properties
		IEnumerable<IImputationSheet> ISupportingDocumentOnly.ImputationsSheets => GetImputationsSheets();

		ZBool IsD48 => ((SupportingDocument)csi).IsD48;

		ZBool ISupportingDocumentOnly.IsD48AndNotClosed => IsD48 && D48Amount > 0 && ((SupportingDocument)csi).CSI_Quantity3 > 0;

		public ZDecimal D48Amount => IsD48 ? GetD48Amount() : ZDecimal.Zero;

		sbyte ISupportingDocumentOnly.D48Deadline => IsD48 ? (sbyte)GetD48DeadlineInMonth() : (sbyte)0;

		ZBool IsUnderInvoiceLine => csi.CSI_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix;

		ZBool ISupportingDocumentOnly.IsUnderInvoiceLine => IsUnderInvoiceLine;

		#endregion

		#region Methods

		IEnumerable<IImputationSheet> GetImputationsSheets()
		{
			var result = new List<IImputationSheet>();

			if (itemEntryLine != null && isCodeAPermitType)
			{
				var allInvoiceLinesSupportingDocuments = itemEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>());
				var allInvoiceHeaderSupportingDocuments = itemEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray().Select(x => x.InvoiceHeader).SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>());
				var allSupportingDocuments = allInvoiceHeaderSupportingDocuments.Concat(allInvoiceLinesSupportingDocuments);
				foreach (var supportingDocument in allSupportingDocuments)
				{
					if (supportingDocument.CSI_Code == Code && supportingDocument.CSI_ReferenceNumber == RefNumber && supportingDocument.CSI_DateOfIssue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) == DateIssue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) && supportingDocument.CSI_LineNo != 0)
					{
						result.Add(new ImputationSheetWrapper(supportingDocument, itemEntryLine));
					}
				}
			}
			return result;
		}

		ZInt GetD48DeadlineInMonth() => csi.CSI_Quantity3.ToZInt();

		ZDecimal GetD48Amount()
		{
			var result = ZDecimal.Zero;

			if (itemEntryLine?.CL_LineNumber == 1 || IsUnderInvoiceLine)
			{
				result = csi.CSI_Value;
			}

			return result;
		}
		#endregion

		readonly CusEntryLine itemEntryLine;
		readonly bool isCodeAPermitType;
	}
}
