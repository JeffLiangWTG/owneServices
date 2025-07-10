using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class NoteSummaryWrapper : NoteWrapper
	{
		public NoteSummaryWrapper(StmNote[] noteBOs, BusinessObjectFactory factory)
			: base(factory.GetNull<StmNote>(), factory)
		{
			NoteBOs = noteBOs;
		}

		readonly StmNote[] NoteBOs;

		protected override string GetText()
		{
			ZStringBuilder noteText = new ZStringBuilder();
			if (NoteBOs != null)
			{
				foreach (StmNote noteBO in NoteBOs)
				{
					noteText.AppendIfNotEmpty(noteBO.ST_NoteDataAsText);
				}
			}
			return noteText.ToStringWithNewLineBetweenAppends();
		}

		protected override string GetDescription()
		{
			return NoteBOs != null && NoteBOs.Length > 0 ? NoteBOs[0].ST_Description : ZString.Empty;
		}

		protected override string GetDescriptionInDatabase()
		{
			return NoteBOs != null && NoteBOs.Length > 0 ? NoteBOs[0].ST_DescriptionInDatabase : ZString.Empty;
		}

		protected override ZDateTime GetCreatedDate()
		{
			if (NoteBOs != null && NoteBOs.Length > 0)
			{
				return NoteBOs.OrderByDescending(note => note.ST_CreatedDateUtc).First().ST_CreatedDateUtc;
			}

			return ZDateTime.Empty;
		}
	}
}
