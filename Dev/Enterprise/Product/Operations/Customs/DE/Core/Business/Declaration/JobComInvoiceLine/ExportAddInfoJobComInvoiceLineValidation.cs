using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ExportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckZG_IsMainPack()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine.JI_IsMainPack)
			{
				var otherLinePackagePKs = new List<ZGuid>();
				var otherLines = invoiceLine.Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(line => line.PK != invoiceLine.PK && line.JI_IsMainPack);
				otherLines.ForEach(line => otherLinePackagePKs.AddRange(
					from pivot in line.PackagesPivot.Cast<InvoiceLinePackagePivot>()
					where pivot.Package != null
					select pivot.Package.PK));

				if (otherLinePackagePKs.Any())
				{
					var existsOtherLineLinkedToSamePackageIsMainPack = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(pivot => pivot.Package != null && otherLinePackagePKs.Contains(pivot.Package.PK));
					if (existsOtherLineLinkedToSamePackageIsMainPack)
					{
						invoiceLine.JI_IsMainPackInfo.AddMessageError(Res.GetString("AA9D6274-6D46-442E-AD64-CA1EA80FDBA1", "Only one Invoice Line per Package can be marked as 'Is main Pack'."));
					}
				}
			}
		}

		protected override void CheckZG_UsualReplacement()
		{
			base.CheckZG_UsualReplacement();

			var parent = Parent;
			if (!parent.ZG_UsualReplacement)
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine.JI_Procedure.SubstringSafe(2, 2) == CustomsProcedureCodeList.Export.PreviousProcedureCode._48)
				{
					if (invoiceLine.EntryInstruction.Style1stDigitIs1And2ndIs2())
					{
						parent.ZG_UsualReplacementInfo.AddMessageError(Res.GetString("D9FEF604-E000-45AF-8503-17DE55A94001",
							"For the selected Type (Procedure) and CPC – Code Previous Procedure 48 the Usual Replacement flag must be set."));
					}
				}
			}
		}

		protected override void CheckZG_ReimportDate()
		{
			base.CheckZG_ReimportDate();

			var parent = Parent;
			var invoiceLine = InvoiceLine;
			var reimportDate = parent.ZG_ReimportDate;
			var targetInfo = parent.ZG_ReimportDateInfo;
			var instruction = invoiceLine.EntryInstruction;
			var styleStartsWith12 = instruction.Style1stDigitIs1And2ndIs2();
			if (reimportDate.IsEmpty)
			{
				if (styleStartsWith12)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
			else
			{
				var previousProcedureCode = invoiceLine.JI_Procedure.SubstringSafe(2, 2);
				if (reimportDate < ZDateTime.Today)
				{
					var mustNotBeEarlier = previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._00 && styleStartsWith12;
					if (!mustNotBeEarlier)
					{
						var entryHeaders = invoiceLine.Declaration?.CustomsEntryHeaders;
						mustNotBeEarlier = !parent.ZG_UsualReplacement && entryHeaders != null && entryHeaders.Cast<CusEntryHeader>().All(x => x.CH_EntryReleaseDate.IsEmpty);
					}
					if (mustNotBeEarlier)
					{
						targetInfo.AddMessageError(Res.GetString("b2373487-5f83-46aa-967d-535de20c67f8", "The date of Re-Importation must not be earlier than the current date."));
					}
				}
				else if (reimportDate > ZDateTime.Today && styleStartsWith12)
				{
					if (previousProcedureCode == "46" || previousProcedureCode == CustomsProcedureCodeList.Export.PreviousProcedureCode._48)
					{
						targetInfo.AddMessageError(Res.GetString("AE72ED6E-8B4C-4811-88D1-C03ED6013CA4", "The date of Re-Importation must be earlier than the current date."));
					}
				}
			}
		}
	}
}
