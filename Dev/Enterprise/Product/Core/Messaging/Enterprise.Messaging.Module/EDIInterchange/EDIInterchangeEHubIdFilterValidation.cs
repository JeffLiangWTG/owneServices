using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Messaging.Module
{
	public class EDIInterchangeEHubIdFilterValidation : ModuleTextFilterValidation
	{
		internal EDIInterchangeEHubIdFilterValidation(EDIInterchangeEHubIdFilter parent)
			: base(parent)
		{
		}

		protected override void CheckProperty()
		{
			base.CheckProperty();
			if (Parent.Property != "" && !Guid.TryParse(Parent.Property, out _))
			{
				var error = ResString.GetMultilingualString("8446C1D0-B29A-4AE1-9ED7-9C4943E4CDE8", "The value is in incorrect format.");
				Parent.PropertyInfo.AddError(error);
			}
		}
	}
}
