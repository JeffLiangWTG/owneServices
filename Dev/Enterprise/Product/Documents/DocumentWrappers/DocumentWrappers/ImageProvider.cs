
using CargoWise.Common;
namespace Enterprise.DocumentWrappers
{
	/// <summary>
	/// This class is a base for client specific logo providers
	/// </summary>
	public class ImageProvider
	{
		protected ImageProvider()
		{
		}

		public static ImageProvider GetInstance()
		{
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				return overridden();
			}
			else
			{
				return new ImageProvider();
			}
		}

		protected delegate ImageProvider NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		public virtual System.Drawing.Image Logo
		{
			get { return null; }
		}
	}
}
