using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class CusGuaranteeReferenceValidation : JPCusReferenceValidation
	{
		public CusGuaranteeReferenceValidation(CusGuaranteeReference parent) : base(parent)
		{
		}

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();

			var instruction = Parent.Parent;
			var propertyInfo = Parent.CFR_ReferenceInfo;
			if (instruction != null && Factory.GetCachedValue<JPImportDeclarationTypeList.TakeoverDeclarationTypeList>().ContainsCode(instruction.CEI_Style)
				&& instruction.Guarantees.Count == 2)
			{
				propertyInfo.AddMessageError(Res.GetString("9352E0B1-5C0C-4D9D-9687-3A7F77E70539", "Do not enter more than one guarantee when Declaration Type is H or N."));
			}

			if (instruction != null && instruction.Guarantees.Any(x => x is CusGuaranteeReference info && info != Parent && info.CFR_Reference == Parent.CFR_Reference))
			{
				propertyInfo.AddMessageError(Res.GetString("2B673BD2-AE5B-444F-8797-9B83F4ABE39C", "The same information has been entered."));
			}
		}

		protected BusinessObjectFactory Factory => Parent.Factory;

		new CusGuaranteeReference Parent => (CusGuaranteeReference)base.Parent;
	}
}
