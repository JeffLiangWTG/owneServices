namespace CargoWise.EntityFramework
{
	//design note: I'd rather have abstract NPBOWithPersistentParent subclass
	//(for both performance reasons, as well as to have abstract members),
	//but there's also NPBOWithLogsAndNotes, so I'd need NPBOWithPersistentParentWithLogsAndNotes,
	//but I do not think this can be done without code duplication in C#. mixins needed?
	public interface IWrapPersistentBizO
	{
		BusinessObject Parent { get; }
	}
}
