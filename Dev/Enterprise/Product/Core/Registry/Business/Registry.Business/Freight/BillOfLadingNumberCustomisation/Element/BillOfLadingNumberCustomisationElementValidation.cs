using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class BillOfLadingNumberCustomisationElementValidation : AutoBillOfLadingNumberCustomisationElementValidation
	{
		public BillOfLadingNumberCustomisationElementValidation(AutoBillOfLadingNumberCustomisationElement parent)
			: base(parent) { }

		#region Order

		protected override void CheckOrder()
		{
			base.CheckOrder();

			if (Parent.Include && Parent.ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.OrderInfo);
			}
		}

		#endregion

		#region Fountain

		protected override void CheckFountain()
		{
			base.CheckFountain();

			if (Parent.Fountain && Parent.ParentCustomisation.UseShipmentSequenceNumber)
			{
				Parent.FountainInfo.AddError(Res.GetString("ab7de185-c678-4fe9-8f09-8368ca05ab4c", "This option is not valid when 'Use Shipment Sequence Number' is enabled"));
			}
		}

		#endregion

		#region CheckDigit

		protected override void CheckCheckDigit()
		{
			base.CheckCheckDigit();

			if (Parent.ParentCustomisation.CheckDigitAlgorithm != CheckDigitAlgorithmList.Codes.None
				&& !Parent.CheckDigit
				&& !Parent.ParentCustomisation.Elements.HasIncludedElementWithCheckDigit)
			{
				Parent.CheckDigitInfo.AddError(Res.GetString("397bceca-e2d9-4026-8b05-f18b74adf326", "The Check Digit Algorithm is {0}. At least one element needs to be included in the check digit algorithm.", Parent.ParentCustomisation.CheckDigitAlgorithm));
			}
		}

		#endregion

		#region Implementation

		public new BillOfLadingNumberCustomisationElement Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLadingNumberCustomisationElement)base.Parent; }
		}

		#endregion
	}
}
