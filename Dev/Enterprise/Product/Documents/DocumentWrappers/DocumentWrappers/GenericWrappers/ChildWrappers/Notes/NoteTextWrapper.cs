using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class NoteTextWrapper : NoteWrapper
	{
		public NoteTextWrapper(ZString text, MultilingualString description, ZDateTime date, BusinessObjectFactory factory)
			: base(factory.GetNull<StmNote>(), factory)
		{
			this.text = text;
			this.description = description;
			this.date = date;
		}

		readonly ZString text;
		readonly MultilingualString description;
		readonly ZDateTime date;

		protected override string GetText()
		{
			return text;
		}

		protected override string GetDescription()
		{
			return description;
		}

		protected override string GetDescriptionInDatabase()
		{
			return description.GetUnresolvedString();
		}

		protected override ZDateTime GetCreatedDate()
		{
			return date;
		}
	}
}
