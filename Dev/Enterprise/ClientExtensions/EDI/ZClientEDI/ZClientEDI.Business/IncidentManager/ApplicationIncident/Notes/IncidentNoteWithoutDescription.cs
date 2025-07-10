using System.Data;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentNoteWithoutDescription : IncidentNote
	{
		public IncidentNoteWithoutDescription(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override StmNoteValidation GetNewValidation()
		{
			return new IncidentNoteWithoutDescriptionValidation(this);
		}

		#region class IncidentNoteWithoutDescriptionValidation

		class IncidentNoteWithoutDescriptionValidation : StmNoteValidation
		{
			public IncidentNoteWithoutDescriptionValidation(IncidentNoteWithoutDescription parent)
				: base(parent)
			{
			}

			protected override void CheckST_Description()
			{
			}
		}

		#endregion
	}
}

