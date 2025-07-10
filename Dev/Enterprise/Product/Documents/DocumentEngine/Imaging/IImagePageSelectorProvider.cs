namespace Enterprise.DocumentEngine.Imaging
{
	public interface IImagePageSelectorProvider
	{
		IImagePageSelector PageSelector
		{
			get;
		}
	}
}
