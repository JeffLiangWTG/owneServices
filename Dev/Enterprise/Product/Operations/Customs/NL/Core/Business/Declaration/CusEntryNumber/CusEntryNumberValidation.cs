using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryNumberValidation : CusEntryNumValidation
{
	public CusEntryNumberValidation(CusEntryNumber parent)
		: base(parent)
	{
	}

	protected override void CheckCE_IssueDate()
	{
		base.CheckCE_IssueDate();

		if (!Parent.CE_IssueDate.IsEmpty && ShouldIssueDateBeEmpty())
		{
			Parent.CE_IssueDateInfo.AddMessageError(Res.GetString("EE942BC4-F4A6-4D05-A95A-884DD9E0676F", "Acceptance date must be empty"));
		}
	}

	bool ShouldIssueDateBeEmpty()
	{
		var issueDateShouldBeEmpty = true;

		if (EntryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
		{
			foreach (JobComInvoiceHeader header in EntryHeader.InvoiceHeaders)
			{
				foreach (AdditionalInfo info in header.AdditionalInfos)
				{
					issueDateShouldBeEmpty = issueDateShouldBeEmpty && CheckSubStyleForEarliestCustomsEntryIssueDate(info, entryInstruction);

					if (!issueDateShouldBeEmpty)
					{
						break;
					}
				}

				foreach (JobComInvoiceLine line in EntryHeader.InvoiceLines)
				{
					foreach (AdditionalInfo info in line.AdditionalInfos)
					{
						issueDateShouldBeEmpty = issueDateShouldBeEmpty && CheckSubStyleForEarliestCustomsEntryIssueDate(info, entryInstruction);

						if (!issueDateShouldBeEmpty)
						{
							break;
						}
					}
				}
			}
		}

		return issueDateShouldBeEmpty;
	}

	bool CheckSubStyleForEarliestCustomsEntryIssueDate(AdditionalInfo info, CusEntryInstruction instruction)
	{
		string[] listToTest;
		if (info.CSI_Type == "INF" && info.CSI_Code == "NP500")
		{
			var subStylesNP500 = new[] { "D", "E", "F" };
			listToTest = subStylesNP500;
		}
		else
		{
			var subStylesOther = new[] { "R", "V", "Z" };
			listToTest = subStylesOther;
		}

		return !listToTest.Contains<string>(instruction.CEI_SubStyle);
	}

	CusEntryHeader header;
	protected CusEntryHeader EntryHeader
	{
		get
		{
			if (header == null)
			{
				var result = Parent.Factory.Load<CusEntryHeader>(Parent.CE_ParentID);
				if (result != null)
				{
					header = result;
				}
			}
			return header;
		}
	}
}
