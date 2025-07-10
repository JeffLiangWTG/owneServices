namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IDocDataObject : IAdHocValidationSupporter
	{
		object Identifier { get; }
	}
}
