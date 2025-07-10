using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class UserDefinedFieldBuilderBuilder
	{
		List<IReportDocumenter> userDefinedFieldBuilderDocumenters;
		public List<IReportDocumenter> UserDefinedFieldBuilderDocumenters
		{
			get
			{
				if (userDefinedFieldBuilderDocumenters == null)
				{
					userDefinedFieldBuilderDocumenters = new List<IReportDocumenter>();
					var fieldBuilders = new UserDefinedFieldCollectionBuilder(null, null, null, null).UserDefinedFieldBuilders;
					foreach (var fieldBuilder in fieldBuilders.OfType<UserDefinedFieldBuilder>())
					{
						userDefinedFieldBuilderDocumenters.Add(fieldBuilder.Documentation);
					}
				}
				return userDefinedFieldBuilderDocumenters;
			}
		}
	}
}
