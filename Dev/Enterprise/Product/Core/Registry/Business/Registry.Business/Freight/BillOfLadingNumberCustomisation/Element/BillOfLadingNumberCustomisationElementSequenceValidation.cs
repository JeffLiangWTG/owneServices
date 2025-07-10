using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	internal class BillOfLadingNumberCustomisationElementSequenceValidation : BillOfLadingNumberCustomisationElementValidation
	{
		public BillOfLadingNumberCustomisationElementSequenceValidation(BillOfLadingNumberCustomisationElement parent)
			: base(parent) { }

		#region Detail

		protected override void CheckDetail()
		{
			base.CheckDetail();

			ZInt length = ZInt.ParseSafe(Parent.Detail, 0);
			if (length < 3 || length > 19)
			{
				Parent.DetailInfo.AddError(Res.GetString("5e10eaa5-9779-4f9d-923d-44cfd02863da", "The sequence length should be in the range of 3-19."));
			}
		}

		#endregion
	}
}
