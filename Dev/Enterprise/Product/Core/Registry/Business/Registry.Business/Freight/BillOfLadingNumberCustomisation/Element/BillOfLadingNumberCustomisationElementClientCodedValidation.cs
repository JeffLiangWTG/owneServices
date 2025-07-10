namespace Enterprise.Registry.Business
{
	internal class BillOfLadingNumberCustomisationElementClientCodedValidation : BillOfLadingNumberCustomisationElementValidation
	{
		public BillOfLadingNumberCustomisationElementClientCodedValidation(BillOfLadingNumberCustomisationElement parent)
			: base(parent) { }

		#region Detail

		protected override void CheckDetail()
		{
			base.CheckDetail();

			if (!Parent.ParentCustomisation.AllowNonAlphanumericCharacters)
			{
				foreach (char c in Parent.Detail)
				{
					if (!char.IsLetterOrDigit(c))
					{
						Parent.DetailInfo.AddError(Res.GetString("dfde85a4-856b-48df-96d7-f9678b0272ac", "Only letters and digits are valid here."));
						break;
					}
				}
			}
		}

		#endregion
	}
}
