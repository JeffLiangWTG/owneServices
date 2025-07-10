using System.Data;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentNote : StmNote
	{
		public IncidentNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ST_NoteType = "";
		}

		public override CodeDescriptionPairList ST_NoteType_List
		{
			get
			{
				if (fST_NoteType_List == null)
				{
					fST_NoteType_List = base.ST_NoteType_List;
					fST_NoteType_List.RemoveCode(StmNoteDescription.PrvDescriptive);
					fST_NoteType_List.RemoveCode(StmNoteDescription.AgvDescriptive);
				}

				return fST_NoteType_List;
			}
		}

		CodeDescriptionPairList fST_NoteType_List;
	}
}

