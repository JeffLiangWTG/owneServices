using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BE.Business.Declaration;

public class JobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
{
	public JobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
	{
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	protected override void CheckAdditionalProcedureCodesAsString()
	{
		base.CheckAdditionalProcedureCodesAsString();

		var concessionsStartingWithADigit = new List<ZString>();
		var concessionsStartingWithALetter = new List<ZString>();

		var codes = new List<ZString>();
		codes.Add(Parent.JI_Procedure);
		codes.AddRange(Parent.AdditionalProcedureCodes.Select(c => c.CY_Code));

		concessionsStartingWithADigit.AddRange(codes.Where(c => IsCharAtIndexADigit(c, 4)));
		concessionsStartingWithALetter.AddRange(codes.Where(c => IsCharAtIndexALetter(c, 4)));

		if (concessionsStartingWithADigit.Count > 3)
		{
			Parent.AdditionalProcedureCodesAsStringInfo.AddMessageError(Res.GetString("18E712C0-B1C7-4F35-985E-96F2DB8361D4", "At most 3 Additional Procedures (including CPC), with a concession starting with a digit, can be selected. Values: {0}.", string.Join(", ", concessionsStartingWithADigit.Select(c => HighlightCharacterAtIndex(c, 4)))));
		}
		if (concessionsStartingWithALetter.Count > 3)
		{
			Parent.AdditionalProcedureCodesAsStringInfo.AddMessageError(Res.GetString("15B7A1D0-9B4F-42B0-B4CD-CC53A7DBE7D4", "At most 3 Additional Procedures (including CPC), with a concession starting with a letter, can be selected. Values: {0}.", string.Join(", ", concessionsStartingWithALetter.Select(c => HighlightCharacterAtIndex(c, 4)))));
		}
	}

	protected override void CheckJI_StateOrRegionOfOrigin()
	{
		base.CheckJI_StateOrRegionOfOrigin();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_StateOrRegionOfOriginInfo);
	}

	protected override void CheckJI_RN_NKCountryOfExport()
	{
		base.CheckJI_RN_NKCountryOfExport();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_RN_NKCountryOfExportInfo);
	}

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();
		var targetInfo = Parent.JI_ProcedureInfo;

		var procedure = Parent.JI_FormattedProcedure;
		if (!string.IsNullOrEmpty(procedure))
		{
			var entryInstruction = Parent.EntryInstruction;
			if (entryInstruction != null)
			{
				RefCusProcedure refCusProcedure = Parent.CusProcedure;

				if (refCusProcedure != null)
				{
					if (refCusProcedure.IsIntoWarehouse() && entryInstruction.CEI_OA_Warehouse2.IsEmpty)
					{
						targetInfo.AddMessageError(Res.GetString("CE863BC5-205A-4D4B-A4EA-D5B042CCEEC1", "The To Warehouse, must be entered and match a Warehouse record."));
					}
					if (refCusProcedure.IsOutOfWarehouse() && entryInstruction.CEI_OA_Warehouse.IsEmpty)
					{
						targetInfo.AddMessageError(Res.GetString("66413491-A88E-46A5-B32F-B27164435A4C", "The From Warehouse, must be entered and match a Warehouse record."));
					}
					if (refCusProcedure.IsIntoWarehouse() || refCusProcedure.IsOutOfWarehouse())
					{
						var defaultDocumentTypes = new ZString[] { Constants.SupportingDocumentTypes.C517, Constants.SupportingDocumentTypes.C518, Constants.SupportingDocumentTypes.C519 };

						if (Parent.SupportingDocuments?.OfType<SupportingDocument>().Count(x => defaultDocumentTypes.Contains(x.CSI_Code)) != 1)
						{
							targetInfo.AddMessageError(Res.GetString("1293F426-31D8-4B28-BA31-102401583748", "When the invoice line's Procedure has the To Warehouse or From Warehouse, then there must be only one C517/C518/C519 in the supporting document collection."));
						}
					}
				}
			}
		}
	}

	static bool IsCharAtIndexADigit(ZString stringToCheck, int charIndex)
	{
		return stringToCheck.Length > charIndex && char.IsDigit(stringToCheck[charIndex]);
	}

	static bool IsCharAtIndexALetter(ZString stringToCheck, int charIndex)
	{
		return stringToCheck.Length > charIndex && char.IsLetter(stringToCheck[charIndex]);
	}

	static ZString HighlightCharacterAtIndex(ZString stringToModify, int charIndex)
	{
		return stringToModify.InsertSafe(charIndex, "[").InsertSafe(charIndex + 2, "]");
	}
}
