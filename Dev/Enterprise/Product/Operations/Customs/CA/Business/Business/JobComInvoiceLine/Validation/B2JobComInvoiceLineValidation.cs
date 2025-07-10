namespace Enterprise.Customs.CA.Business
{
	public class B2JobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public B2JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		#region Suppress validations on customize fields

		protected override void CheckJI_CustomAttrib1()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib2()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib3()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib4()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib5()
		{
			//not required
		}

		protected override void CheckJI_CustomAttrib6()
		{
			//not required
		}

		protected override void CheckJI_CustomTextBlob1()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal1()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal2()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal3()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal4()
		{
			//not required
		}

		protected override void CheckJI_CustomDecimal5()
		{
			//not required
		}

		protected override void CheckJI_CustomDate1()
		{
			//not required
		}

		protected override void CheckJI_CustomDate2()
		{
			//not required
		}

		protected override void CheckJI_CustomDate3()
		{
			//not required
		}

		protected override void CheckJI_CustomDate4()
		{
			//not required
		}

		protected override void CheckJI_CustomDate5()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag1()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag2()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag3()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag4()
		{
			//not required
		}

		protected override void CheckJI_CustomFlag5()
		{
			//not required
		}
		#endregion
	}
}
