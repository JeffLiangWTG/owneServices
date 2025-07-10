//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccApportionmentTemplateValidation
//
//    This class should be used for overriding validation in AutoAccApportionmentTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AccApportionmentTemplateValidation : AutoAccApportionmentTemplateValidation
	{
		public AccApportionmentTemplateValidation(AutoAccApportionmentTemplate parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePercentageTotal();
		}

		public void ValidatePercentageTotal()
		{
			AccApportionmentTemplate template = Parent as AccApportionmentTemplate;
			if (template != null)
			{
				ValidateCalculatedProperty(template.PercentageTotalInfo);
			}
		}

		protected void CheckPercentageTotal()
		{
			AccApportionmentTemplate template = Parent as AccApportionmentTemplate;
			if (template != null)
			{
				if (template.PercentageTotal != 100.0M)
				{
					template.PercentageTotalInfo.AddError(Res.GetString("f7c8945b-c873-40b0-8f49-84f9f53a62bb", "Must equal 100."));
				}
			}
		}
	}
}