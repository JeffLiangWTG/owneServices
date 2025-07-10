namespace CargoWise.UniversalCopy
{
	public enum CopyMethod
	{
		None,
		Empty,
		Copy,
		Default,
		Value,
		Macro,
		Property,
	}

	public enum RelatedEntityCopyMethod
	{
		None,
		Link,
		Copy,
		LinkCopied,
	}

	public enum CollectionCopyMethod
	{
		None,
		All,
		Filter,
	}
}
