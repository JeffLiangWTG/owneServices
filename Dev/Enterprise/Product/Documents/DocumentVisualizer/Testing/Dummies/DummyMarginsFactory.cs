using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyMarginsFactory
	{
		public static Margins Get()
		{
			return new Margins
			{
				Header = 0,
				Footer = 0,

				Top = 75,
				Bottom = 75,

				Left = 70,
				Right = 70
			};
		}
	}
}