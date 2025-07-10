using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class UpdateUseOAuth2ForOutgoingRegistryItem : BoolRegistryItemToCodePairRegistryItemTransformation
	{
		public override string UserDescription => "Update UseOAuth2ForOutgoing";

		public override string RegistryItemName => "UseOAuth2ForOutgoing";

		public override string ReplaceTrueWith => "Ms365";

		public override string ReplaceFalseWith => "";
	}
}
