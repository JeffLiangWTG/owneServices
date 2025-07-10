namespace Enterprise.Core.Forms
{
	public interface IBackColorMutable
	{
		ActiveControlColorChanger ColorChanger { get; }
		bool EnableValidStateColor { get; set; }
	}
}
