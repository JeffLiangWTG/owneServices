using CargoWise.Common;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	sealed class Mapping
	{
		public static Mapping New<T>(VersionLabel version) where T : IDataTransformation, new()
		{
			return new Mapping(new T(), version);
		}

		Mapping(IDataTransformation transformation, VersionLabel version)
		{
			Argument.NotNull(transformation, nameof(transformation));
			Argument.NotNull(version, nameof(version));

			this.transformation = transformation;
			this.version = version;
		}

		readonly IDataTransformation transformation;
		readonly VersionLabel version;

		public VersionLabel Version
		{
			get { return version; }
		}

		public IDataTransformation Transformation
		{
			get { return transformation; }
		}
	}
}