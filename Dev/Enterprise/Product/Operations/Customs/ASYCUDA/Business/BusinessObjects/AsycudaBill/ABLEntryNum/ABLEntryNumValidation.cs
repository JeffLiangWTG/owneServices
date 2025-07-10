using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ASYCUDA.Business
{
	// Note - this class will be loaded for all CusEntryNumbs, not just those under an ABL.   This is a flaw in Factory.Validation.MainGroup.RegisterValidationType -= it's not smart enough to register it for certain parent types, it registered it across the domain.  Pff. 
	// So be careful when assuming your parent. 
	public class ABLEntryNumValidation : CusEntryNumValidation
	{
		public ABLEntryNumValidation(ABLEntryNum entryNumber)
			: base(entryNumber)
		{
		}

		public new ABLEntryNum Parent => (ABLEntryNum)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAssociatedPacks();
		}

		protected override void CheckCE_EntryNum()
		{
			if (Parent.CE_EntryType == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				AddErrorIfNumberUsedMoreThanOnce();
			}
		}

		protected void AddErrorIfNumberUsedMoreThanOnce(bool checkOtherBills = true)
		{
			var parentBill = Parent.Bill;
			var numbersToCheck = (checkOtherBills
				? parentBill?.Header?.Bills.OfType<AsycudaBill>().SelectMany(bill => bill.CustomsEntryNumbers)
				: parentBill?.CustomsEntryNumbers
				)?.OfType<ABLEntryNum>();
			var duplicateLRN = numbersToCheck?.FirstOrDefault(num => num != Parent && num.CE_EntryNum == Parent.CE_EntryNum && num.CE_EntryType == Parent.CE_EntryType);
			if (duplicateLRN != null)
			{
				Parent.CE_EntryNumInfo.AddMessageError(LRNNumbersCanOnlyBeEnteredOnce(Parent.CE_EntryType, duplicateLRN.Bill == parentBill));
			}
		}

		public static string LRNNumbersCanOnlyBeEnteredOnce(string type, bool isSameCountry)
		{
			return Res.GetString("99DCC260-76C1-469A-90C1-9CC54C42004E", "This {1} has already been captured against {0} Bill, this must be unique per Bill.", isSameCountry ? "this" : "another", type);
		}

		internal void ValidateAssociatedPacks()
		{
			var entryType = Parent.CE_EntryType;
			if (entryType.IsEmpty)
			{
				entryType = Res.GetString("7B8E22F6-54CC-479D-8E66-D1CED95DA690", "Customs Number");
			}
			Parent.RemoveRowMessageError(AssociatedPacksRequired(entryType));
			if ((Parent.Bill?.Header?.SupportAssociatedPacks ?? false) && Parent.PackPivots.Count == 0)
			{
				Parent.AddRowMessageError(AssociatedPacksRequired(entryType));
			}
		}

		public static string AssociatedPacksRequired(string type)
		{
			return Res.GetString("D61BB4F1-79FB-4331-A66C-81C5659CBA77", "At least one Pack must be associated with this {0}.", type);
		}

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();
			if (!Parent.CE_EntryNum.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CE_EntryTypeInfo);
			}
			var list = Parent.Bill?.Lookups?.CustomsEntryNumberTypes;
			if (list?.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CE_EntryTypeInfo, list);
			}
		}
	}
}
