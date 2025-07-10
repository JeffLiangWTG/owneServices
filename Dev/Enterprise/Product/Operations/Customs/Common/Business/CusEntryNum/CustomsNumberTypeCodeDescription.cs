using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public class CustomsNumberTypeCodeDescription : CodeDescriptionPair, ICustomsNumberTypeCodeDescription
	{
		public CustomsNumberTypeCodeDescription(object code, MultilingualString description, bool isUnique = true)
			: base(code, description)
		{
			IsUnique = isUnique;
		}

		public bool IsUnique { get; }

		public bool IsAutomation { get; }
	}
}
