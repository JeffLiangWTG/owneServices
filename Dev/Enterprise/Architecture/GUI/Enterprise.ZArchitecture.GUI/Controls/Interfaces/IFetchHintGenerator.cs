namespace Enterprise.ZArchitecture.GUI.Internal
{
	public interface IFetchHintGenerator
	{
		void AddFetchHint(object dataSource, string dataMember);
	}
}
