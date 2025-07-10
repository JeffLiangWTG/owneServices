using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Note")]
	public abstract class NoteWrapper : GenericWrapper
	{
		public NoteWrapper(StmNote note, BusinessObjectFactory factory)
			: base(note, factory)
		{
		}

		public ZString Text
		{
			get { return fText ?? (fText = GetText()); }
		}
		string fText;
		protected abstract string GetText();

		public ZString Description
		{
			get { return fDescription ?? (fDescription = GetDescription()); }
		}
		string fDescription;
		protected abstract string GetDescription();

		public ZString DescriptionInDatabase
		{
			get { return descriptionInDatabase ?? (descriptionInDatabase = GetDescriptionInDatabase()); }
		}
		string descriptionInDatabase;
		protected abstract string GetDescriptionInDatabase();

		public ZDateTime CreatedDate
		{
			get { return GetCreatedDate(); }
		}
		protected abstract ZDateTime GetCreatedDate();
	}
}
