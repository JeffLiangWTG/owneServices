namespace Enterprise.Customs.IT.MessageBuilders;

public class TabbedFlatFileMessageSerializer : FlatFileMessageSerializer
{
	const string Tab = "\t";

	public TabbedFlatFileMessageSerializer() : base(Tab)
	{
	}
}
