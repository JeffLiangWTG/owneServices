using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class UpdateUseOAuth2ForIncomingRegistryItem : BoolRegistryItemToCodePairRegistryItemTransformation
	{
		public override string UserDescription => "Update UseOAuth2ForIncoming";

		public override string RegistryItemName => "UseOAuth2ForIncoming";

		public override string ReplaceTrueWith => "Ms365";

		public override string ReplaceFalseWith => "";
	}
}
