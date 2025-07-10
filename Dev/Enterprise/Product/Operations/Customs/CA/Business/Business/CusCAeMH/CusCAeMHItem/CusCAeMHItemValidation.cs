//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHItemValidation
//
//    This class should be used for overriding validation in AutoCusCAeMHItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHItemValidation : AutoCusCAeMHItemValidation
	{
		public CusCAeMHItemValidation(AutoCusCAeMHItem parent) : base(parent)
		{
		}

		new CusCAeMHItem Parent
		{
			get { return (CusCAeMHItem)base.Parent; }
		}

		protected override void CheckBX_QuantityUQ()
		{
			base.CheckBX_QuantityUQ();
			if (Parent.BX_Quantity != 0 && Parent.BX_QuantityUQ.IsEmpty)
			{
				Parent.BX_QuantityUQInfo.AddMessageError(Res.GetString("9AB9FDD9-337F-4573-B8D4-1F8B9A2B9735", "You have not entered Quantity UQ."));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.BX_QuantityUQInfo);
		}

		protected override void CheckBX_Quantity()
		{
			base.CheckBX_Quantity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BX_QuantityInfo);
		}

		protected override void CheckBX_Description()
		{
			base.CheckBX_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BX_DescriptionInfo);
		}

		public void ValidateFirstUNDG()
		{
			ValidateCalculatedProperty(Parent.FirstUNDGInfo);
		}

		protected void CheckFirstUNDG()
		{
			if (Parent.BX_IsDangerousInBulk)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.FirstUNDGInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFirstUNDG();
		}
	}
}
