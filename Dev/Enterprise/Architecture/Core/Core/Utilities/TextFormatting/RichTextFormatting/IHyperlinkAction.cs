namespace Enterprise.ZArchitecture.Core
{
	public interface IHyperlinkAction
	{
		LogHyperlink Hyperlink { get; }
		string Key { get; }
		void DoAction();
	}
}
