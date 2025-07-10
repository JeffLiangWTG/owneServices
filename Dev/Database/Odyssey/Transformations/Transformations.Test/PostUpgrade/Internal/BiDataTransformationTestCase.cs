using System;
using System.Threading;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	public abstract class BiDataTransformationTestCase : DataTransformationTestCase
	{
		protected override void RunTransformation()
		{
			using (CheckNoTablesCreated())
			{
				if (TransformationTestShouldBeRunAgainstNewInstance)
				{
					var transformation = GetBiTransformationInstance();
					transformation.RunBiTransformation(TestConnection, CancellationToken.None);
				}
				else
				{
					(TransformationToTest as BiDataTransformation).RunBiTransformation(TestConnection, CancellationToken.None);
				}
			}
		}

		protected abstract BiDataTransformation GetBiTransformationInstance();
	}

	public abstract class AuditDataTransformationTestCase : BiDataTransformationTestCase
	{
		protected override BiDataTransformation GetBiTransformationInstance()
		{
			var instance = GetNewTestTransformationInstance();
			if (instance is AuditDataTransformation)
			{
				return (AuditDataTransformation)instance;
			}
			else
			{
				throw new Exception($"{instance} is not an AuditDataTransformation class.");
			}
		}
	}

	public abstract class EdwDataTransformationTestCase : BiDataTransformationTestCase
	{
		protected override BiDataTransformation GetBiTransformationInstance()
		{
			var instance = GetNewTestTransformationInstance();
			if (instance is EdwDataTransformation)
			{
				return (EdwDataTransformation)instance;
			}
			else
			{
				throw new Exception($"{instance} is not an EdwDataTransformation class.");
			}
		}
	}
}
