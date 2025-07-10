using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public sealed class DescriptionOnlyTransformation : DataTransformation
	{
		public DescriptionOnlyTransformation(string description)
		{
			this.description = description;
		}
		readonly string description;

		public override bool IsRequired => true;

		public override string UserDescription => description;
	}
}
