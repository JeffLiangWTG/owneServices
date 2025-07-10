using System.Collections;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class UserDefinedFieldCollectionBuilderForTest : UserDefinedFieldCollectionBuilder
	{
		internal UserDefinedFieldCollectionBuilderForTest(StringTreeNode rootNode, string dataContext, ValidatorPack validatorPack, MatchEvaluator evaluatorForDefaultValues)
			: base(rootNode, dataContext, validatorPack, evaluatorForDefaultValues)
		{ }

		internal ArrayList UserDefinedFieldbuilders
		{
			get
			{
				return base.UserDefinedFieldBuilders;
			}
		}
	}
}
