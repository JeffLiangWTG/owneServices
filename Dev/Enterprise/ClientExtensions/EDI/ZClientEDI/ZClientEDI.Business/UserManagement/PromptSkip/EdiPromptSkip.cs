using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiPromptSkip : AutoEdiPromptSkip
	{
		public EdiPromptSkip(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.Types")]
		public override ZString EPS_Type
		{
			get => base.EPS_Type;
			set => base.EPS_Type = value;
		}

		#endregion

		public static EdiPromptSkip AddSkip(BusinessObject owner, string type, ZDateTime skipUntilDate)
		{
			if (!EdiPromptSkipLookups.PromptTypes.ContainsCode(type) || !skipUntilDate.IsEmpty && !skipUntilDate.IsValid)
			{
				return null;
			}

			var existingRecordQuery = new ZQuery(EdiPromptSkipSchema.EPS_Owner, owner.PK);
			existingRecordQuery.AddToFilter(EdiPromptSkipSchema.EPS_Type, type);
			var recordToAddOrUpdate = owner.Factory.LoadTop1<EdiPromptSkip>(existingRecordQuery);

			if (recordToAddOrUpdate == null)
			{
				recordToAddOrUpdate = owner.Factory.New<EdiPromptSkip>();
				recordToAddOrUpdate.EPS_Owner = owner.PK;
				recordToAddOrUpdate.EPS_Type = type;
			}

			recordToAddOrUpdate.EPS_SkipUntilDate = skipUntilDate;

			return recordToAddOrUpdate;
		}

		public static bool ShouldSkipPrompt(BusinessObject owner, string type)
		{
			var existingRecordQuery = new ZQuery(EdiPromptSkipSchema.EPS_Owner, owner.PK);
			existingRecordQuery.AddToFilter(EdiPromptSkipSchema.EPS_Type, type);
			var existingRecord = owner.Factory.LoadTop1<EdiPromptSkip>(existingRecordQuery);

			if (existingRecord == null)
			{
				return false;
			}

			if (existingRecord.EPS_SkipUntilDate.IsEmpty)
			{
				return true;
			}

			return existingRecord.EPS_SkipUntilDate >= ZDateTime.Today;
		}
	}
}
