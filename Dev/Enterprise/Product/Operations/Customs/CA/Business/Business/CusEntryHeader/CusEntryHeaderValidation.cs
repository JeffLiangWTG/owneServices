
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusEntryHeaderValidation : Customs.Business.CusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateEntryLineQuantities();
		}

		#region ValidateEntryLineQuantities

		public void ValidateEntryLineQuantities()
		{
			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsImport && !declaration.IsB3NotRequired && Parent.IsB3C && Parent.MergedLines.Count > MaxNumberOfEntryLines)
			{
				if (declaration.CA_MergeBy.IsEmpty || declaration.CA_MergeBy == B3MergeByList.Codes.NotMerge || declaration.CA_MergeBy == B3MergeByList.Codes.NotMergeUsingProductNumberInDescription)
				{
					Parent.AddRowMessageError(Res.GetString("68532271-4E43-4E31-8010-83E526E686AA", "The number of entry lines exceeds {0}, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.", MaxNumberOfEntryLines));
				}
				else
				{
					Parent.AddRowMessageError(Res.GetString("7E172A00-B60D-4741-844A-B43F76E3BF25", "The number of entry lines exceeds {0}, system may not be able to generate a B3 document for this job. Please reduce the number of entry lines and create an additional entry if necessary.", MaxNumberOfEntryLines));
				}
			}
		}

		public readonly int MaxNumberOfEntryLines = 4000;

		#endregion
	}
}
