using CargoWise.Types;

namespace Enterprise.ZArchitecture
{
	public interface ITextFindControl
	{
		string GetText();
		void HighlightText(ZInt startPosition, ZInt length);
	}
}
