using System;

namespace Enterprise.Customs.Common
{
	public interface ICusEntryNumberValidationDeciderOfType
	{
		Type GetCusEntryNumberValidationType();
	}
}
