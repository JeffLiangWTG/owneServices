using System.Collections.Generic;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ClassificationDataLoadForTest : ClassificationDataLoad
	{
		public IEnumerable<string> ExtraColumns
		{
			get
			{
				return this.GetCountrySpecificFieldNames();
			}
		}
	}
}
