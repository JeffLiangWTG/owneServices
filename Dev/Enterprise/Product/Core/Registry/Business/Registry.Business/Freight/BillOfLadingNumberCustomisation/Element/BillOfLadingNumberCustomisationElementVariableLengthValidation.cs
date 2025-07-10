using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	class BillOfLadingNumberCustomisationElementVariableLengthValidation : BillOfLadingNumberCustomisationElementValidation
	{
		public BillOfLadingNumberCustomisationElementVariableLengthValidation(BillOfLadingNumberCustomisationElement parent, int defaultDetail)
			: base(parent)
		{
			DefaultDetail = defaultDetail;
		}

		#region Detail

		protected override void CheckDetail()
		{
			base.CheckDetail();

			var length = ZInt.ParseSafe(Parent.Detail, 0);
			if (length < 1 || length > DefaultDetail)
			{
				Parent.DetailInfo.AddError(Res.GetString("0b532130-36cf-493c-b462-f49919e95b8a", "The {0} length should be in the range of 1-{1}.", Parent.ElementName, DefaultDetail));
			}
		}

		int DefaultDetail { get; }

		#endregion
	}
}
