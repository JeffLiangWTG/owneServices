using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPENotes : Notes
	{
		public UPENotes(IStmNoteParent parent) : base(parent)
		{
		}

		protected override Type ElementType => typeof(UPEStmNote);

		public override StmNote AddNew(bool isCustomDescription, ZString description, ZString noteText)
		{
			if (description == UPEPredefinedNoteTypes.Instance.Level1Record.Description)
			{
				var level1notes = Parent.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.Level1Record.Description);
				if (level1notes.Length > 0)
				{
					return level1notes[0];
				}
			}

			return base.AddNew(isCustomDescription, description, noteText);
		}
	}
}
