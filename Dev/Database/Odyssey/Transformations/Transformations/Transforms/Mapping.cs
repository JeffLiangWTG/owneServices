using System;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms
{
	public class Mapping
	{
		public static Mapping New<T>(VersionLabel mappedVersion) where T : DataTransformation
		{
			var newMapping = new Mapping(typeof(T), mappedVersion);
			return newMapping;
		}

		Mapping(Type transformationType, VersionLabel mappedVersion)
		{
			this.transformationType = transformationType;
			this.mappedVersion = mappedVersion;
		}

		public Type TransformationType
		{
			get { return transformationType; }
		}

		public VersionLabel MappedVersion
		{
			get { return mappedVersion; }
		}

		readonly Type transformationType;
		readonly VersionLabel mappedVersion;
	}
}
