using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForEntryInstructionCollection : DependentBusinessObjectCollection<GuaranteeForEntryInstruction, CusEntryInstruction>
	{
		public GuaranteeForEntryInstructionCollection(CusEntryInstruction master) : base(master)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is GuaranteeForEntryInstruction guarantee)
			{
				guarantee.Parent = Master;
				guarantee.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				guarantee.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.FUL;
			}
		}

		protected sealed override string FkColumnName => CusBondDetailSchema.Constants.PW_ParentID;

		protected virtual int MaxCountForValidation
		{
			get => maxCountForValidation;
		}
		const int maxCountForValidation = 99;
	}
}
