using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEStmNote : StmNote
	{
		public UPEStmNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ST_NoteType = nameof(StmNoteVisibility.INT);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (
				(ST_Table == CusHAWBSchema.Constants.TableName || ST_Table == JobDeclarationSchema.Constants.TableName || ST_Table == CusMAWBSchema.Constants.TableName)
				&&
				ST_Description != PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description)
			{
				IsReadOnlyAfterAdd = IsInDatabase;
			}
		}
	}
}
