using System.Drawing;

namespace Enterprise.DocumentEngine.Visualisation
{
	internal interface IControlSizeProvider
	{
		Size GetCellRange(string macro);
	}
}
