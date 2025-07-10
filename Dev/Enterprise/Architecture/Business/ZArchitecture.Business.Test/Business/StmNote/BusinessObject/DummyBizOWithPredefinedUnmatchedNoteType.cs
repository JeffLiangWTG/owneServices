using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyBizOWithPredefinedUnmatchedNoteType : DummyBizOWithRelatedNotes
	{
		public DummyBizOWithPredefinedUnmatchedNoteType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
				return noteTypes;
			}
		}
	}
}
