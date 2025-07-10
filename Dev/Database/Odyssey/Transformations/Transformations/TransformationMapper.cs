using System;
using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public abstract class TransformationMapper
	{
		public TransformationMapper(IUpgradeManager manager)
		{
			Manager = manager;
		}

		public DataTransformation[] GetOrderedTransformations()
		{
			return AllTransformations.Reverse().Where(x => x.IsRequired).ToArray();
		}

		internal DataTransformation[] AllTransformations => allTransformations = allTransformations ?? GetAllTransformations();
		DataTransformation[] allTransformations;

		protected readonly IUpgradeManager Manager;

		/// <summary>
		/// NEW TRANSFORMATIONS GO ON THE TOP!!
		/// </summary>
		protected abstract DataTransformation[] GetAllTransformations();
	}
}
