
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingSeaPackLineValidation : AutoJobPackLinesValidation
	{
		public JXCForwardingSeaPackLineValidation(JASForwardingPackLine parent)
			: base(parent)
		{
		}

		public new JASForwardingPackLine Parent
		{
			get { return (JASForwardingPackLine)base.Parent; }
		}

		protected override void CheckJL_Description()
		{
			base.CheckJL_Description();
			if (RequiresValidation)
			{
				ValidationHelper.AddJXCWarningIfNotEntered(Parent.JL_DescriptionInfo);
			}
		}

		protected override void CheckJL_CustomAttrib1()
		{
			base.CheckJL_CustomAttrib1();
			if (RequiresValidation)
			{
				ValidationHelper.ValidateCurrencyCode(Parent.Factory, Parent.JL_CustomAttrib1Info, Parent.LinePriceCurrency);
			}
		}

		#region Implementation

		bool RequiresValidation
		{
			get { return Parent.JL_FreightMode == FreightConstants.OuterPackType; }
		}

		ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}
				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;

		#endregion
	}
}

#region Implementation
#endregion
