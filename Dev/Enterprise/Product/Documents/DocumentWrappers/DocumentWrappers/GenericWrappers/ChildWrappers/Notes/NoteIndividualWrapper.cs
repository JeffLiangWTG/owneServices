using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class NoteIndividualWrapper : NoteWrapper
	{
		public NoteIndividualWrapper(StmNote noteBO, BusinessObjectFactory factory)
			: base(noteBO, factory)
		{
			NoteBO = noteBO ?? Factory.GetNull<StmNote>();
		}
		readonly StmNote NoteBO;

		protected override string GetText()
		{
			return NoteBO.ST_NoteDataAsText;
		}

		protected override string GetDescription()
		{
			return NoteBO.ST_Description;
		}

		protected override string GetDescriptionInDatabase()
		{
			return NoteBO.ST_DescriptionInDatabase;
		}

		protected override ZDateTime GetCreatedDate()
		{
			return NoteBO.ST_CreatedDateUtc;
		}
	}
}
