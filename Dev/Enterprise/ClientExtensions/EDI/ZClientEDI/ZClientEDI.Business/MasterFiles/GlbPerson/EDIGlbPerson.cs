using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIGlbPerson : GlbPerson
	{
		public EDIGlbPerson(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void Delete()
		{
			base.Delete();

			var query = new ZQuery(EdiPersonMergeQueueSchema.EMQ_PER_DissolvePerson, PK);
			query.AddToFilter(JoinCondition.Or, EdiPersonMergeQueueSchema.EMQ_PER_RetainPerson, PK);
			var queueItems = Factory.Load<EdiPersonMergeQueue>(query);
			foreach (var item in queueItems)
			{
				item.Delete();
			}
		}

		#region MyAccount

		public void StorePersonalEmailPromptSkip(bool permanentSkip = false)
		{
			EdiPromptSkip.AddSkip(this, EdiPromptSkipTypes.Codes.PersonalEmail, permanentSkip ? ZDateTime.Empty : ZDateTime.Today.AddDays(7));
		}

		public bool ShouldSkipPersonalEmailPrompt()
		{
			return EdiPromptSkip.ShouldSkipPrompt(this, EdiPromptSkipTypes.Codes.PersonalEmail);
		}

		#endregion
	}
}
