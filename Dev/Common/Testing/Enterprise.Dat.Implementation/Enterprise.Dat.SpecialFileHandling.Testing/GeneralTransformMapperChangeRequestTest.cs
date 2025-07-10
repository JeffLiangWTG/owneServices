using NUnit.Framework;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	abstract class GeneralTransformMapperChangeRequestTest : TestCase
	{
		protected const string TransformationVersionFileServerPath = "/Database/Odyssey/Resource/Version/TransformationVersion.cs";
		protected const string ShelfMapperFileServerPath = "/Database/Odyssey/Transformations/Transformations/Transforms/ShelfCheckinMapper.txt";
		protected const string MapperFileServerPath = "/Database/Odyssey/Transformations/Transformations/Transforms/Mapper.cs";

		protected const string OriginalTransformationVersionFileContents = @"
namespace Enterprise.DbUpgrader.Resource.Version
{
	public static class TransformationVersion
	{
		/// <summary>
		/// blah blah blah
		/// </summary>
		public static readonly VersionLabel ApplicationNumber = new VersionLabel(7000, 0);
	}
}
";

		protected const string UpdatedTransformationVersionFileContents = @"
namespace Enterprise.DbUpgrader.Resource.Version
{
	public static class TransformationVersion
	{
		/// <summary>
		/// blah blah blah
		/// </summary>
		public static readonly VersionLabel ApplicationNumber = new VersionLabel(7001, 0);
	}
}
";
	}
}
