using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class InvoiceLineNote
	{
		internal NoteType[] BuildLineNote(PostingJournal line)
		{
			return (new List<NoteType>() { new NoteType() { Value = line.Description.Value } }).ToArray();
		}
	}
}
