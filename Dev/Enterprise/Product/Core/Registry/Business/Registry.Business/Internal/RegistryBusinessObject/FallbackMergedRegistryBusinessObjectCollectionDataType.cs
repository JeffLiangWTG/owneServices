namespace Enterprise.Registry.Business
{
	public abstract class FallbackMergedRegistryBusinessObjectCollectionDataType<T> : NonPersistentBusinessObjectRegistryDataType<T> where T : RegistryBusinessObjectCollection
	{
		protected override bool IsFallBackMergeValuesImplementedCore
		{
			get { return true; }
		}

		protected override T FallBackMergeValuesCore(T fallBackValue, T value)
		{
			T result = CloneValue(fallBackValue);

			foreach (RegistryBusinessObject nextElement in value)
			{
				if (!result.ContainsCode(nextElement.Code))
				{
					result.Add(nextElement);
				}
			}

			return result;
		}
	}
}
