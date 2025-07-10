using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	[DependentBusinessObject(typeof(CusStatementHeader), "ChargesDetail")]
	public class CusStatementChargesDetail : CusStatementLine
	{
		public CusStatementChargesDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusStatementHeader Statement => Factory.Load<CusStatementHeader>(B3_B2);

		public static CusStatementChargesDetail LoadOrCreate(CusStatementHeader parent)
		{
			return Load() ?? New();

			CusStatementChargesDetail Load()
			{
				var query = new ZDBOnlyQuery(typeof(CusStatementChargesDetail)) { };
				query.AddToFilter(CusStatementLineSchema.B3_B2, parent.PK);
				query.AddToFilter(CusStatementLineSchema.B3_EntryType, StatementEntryTypeList.Codes.DCG);
				return parent.Factory.LoadTop1<CusStatementChargesDetail>(query);
			}

			CusStatementChargesDetail New()
			{
				var result = parent.Factory.New<CusStatementChargesDetail>();
				using (result.SuspendSettingHasChanges())
				{
					result.B3_B2 = parent.PK;
					result.B3_EntryType = StatementEntryTypeList.Codes.DCG;
				}
				return result;
			}
		}

		[ChildEditable(true)]
		public CusStatementLineChargeCollection Charges
		{
			get
			{
				if (charges == null)
				{
					charges = new CusStatementLineChargeCollection(this);
					charges.Load();
					RegisterEditableChildObject(charges);
				}
				return charges;
			}
		}
		CusStatementLineChargeCollection charges;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Charges.RemoveAndDeleteAll();
			}
			base.Delete();
		}
	}
}
