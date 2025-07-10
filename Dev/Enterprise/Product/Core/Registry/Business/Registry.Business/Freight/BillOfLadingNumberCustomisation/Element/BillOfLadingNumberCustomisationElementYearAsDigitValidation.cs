using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	internal class BillOfLadingNumberCustomisationElementYearAsDigitValidation : BillOfLadingNumberCustomisationElementValidation
	{
		public BillOfLadingNumberCustomisationElementYearAsDigitValidation(BillOfLadingNumberCustomisationElement parent)
			: base(parent) { }

		#region Detail

		protected override void CheckDetail()
		{
			base.CheckDetail();

			ZInt length = ZInt.ParseSafe(Parent.Detail, 0);
			if (length != 1 && length != 2 && length != 4)
			{
				Parent.DetailInfo.AddError(Res.GetString("51c48b49-b163-4cd2-98dc-80f2e1df9768", "The year as digit length should be either 1, 2 or 4."));
			}
		}

		#endregion
	}
}
