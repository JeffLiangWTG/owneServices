using Enterprise.Customs.Common.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class DutyAndTaxValidation : Customs.Business.MultiLineAddInfos.CusAddInfoValidation
	{
		public DutyAndTaxValidation(DutyAndTax parent)
			: base(parent)
		{
		}

		new DutyAndTax Parent
		{
			get { return (DutyAndTax)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateQuantity();
		}

		public void ValidateQuantity()
		{
			ValidateCalculatedProperty(Parent.QuantityInfo);
		}

		protected void CheckQuantity()
		{
			var dutyAndTax = Parent;
			if (dutyAndTax != null && dutyAndTax.C1_RateType == RateTypes.Codes.Specific && !dutyAndTax.C1_Rate.IsEmpty && dutyAndTax.Quantity.IsEmpty && !dutyAndTax.IsDutyOnProductOrCusClassification)
			{
				dutyAndTax.QuantityInfo.AddMessageError(Res.GetString("9273359f-f36f-407a-aacd-4622469e6a8f", "No Quantity has been found for this specific calculation line. Are you missing a second quantity with units {0}?", Parent.C1_UnitOfMeasure));
			}
		}
	}
}
