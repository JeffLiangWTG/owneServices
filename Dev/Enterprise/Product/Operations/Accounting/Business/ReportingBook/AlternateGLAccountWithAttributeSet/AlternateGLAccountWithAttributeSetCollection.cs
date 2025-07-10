using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	[ModuleID(ModuleId.AlternateGLAccounts)]
	public class AlternateGLAccountWithAttributeSetCollection : NonPersistentBusinessObjectCollection<AlternateGLAccountWithAttributeSet>
	{
		public AlternateGLAccountWithAttributeSetCollection(AlternateGLAccountWithAttributeSetDetails alternateGLAccountWithAttributeSetDetails, BusinessObjectFactory factory) : base(factory)
		{
			ParentGLAccountPK = alternateGLAccountWithAttributeSetDetails.ParentGLAccountPK;
			ChartPK = alternateGLAccountWithAttributeSetDetails.ChartPK;
			CurrentSequence = alternateGLAccountWithAttributeSetDetails.Sequence;
			AccountType = alternateGLAccountWithAttributeSetDetails.AccountType;
			Unit = alternateGLAccountWithAttributeSetDetails.Unit;
			CashFlowCategory = alternateGLAccountWithAttributeSetDetails.CashFlowCategory;
		}

		public ZGuid ParentGLAccountPK { get; set; }

		public ZGuid ChartPK { get; set; }

		public int CurrentSequence { get; set; }

		public ZString AccountType { get; set; }

		public ZString ReportSection { get; set; }

		public ZString DebitCredit { get; set; }

		public ZString CashFlowCategory { get; set; }

		public ZString Unit { get; set; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var alternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(ParentGLAccountPK, ChartPK, CurrentSequence, AccountType, CashFlowCategory, Unit);
			var alternateGLAccountWithAttributeSet = new AlternateGLAccountWithAttributeSet(alternateGLAccountWithAttributeSetDetails, Factory);
			CurrentSequence++;
			return alternateGLAccountWithAttributeSet;
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
