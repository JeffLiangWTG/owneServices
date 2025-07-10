using System;
using System.Drawing;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualiserComponentForTesting : VisualiserComponent
	{
		public VisualiserComponentForTesting(Point location, Size size)
			: base(location, size)
		{
		}

		public override string GetControlDescriptionForTesting()
		{
			throw new NotImplementedException();
		}

		public override object GetRenderedControlValueForTesting()
		{
			throw new NotImplementedException();
		}
	}
}
