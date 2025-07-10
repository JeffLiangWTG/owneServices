using System.Collections;

namespace CargoWise.ComponentModel
{
	public interface IMenuItem
	{
#if DEBUG
		[Common.Testing.SuppressWeaklyTypedCollectionMessage]
#endif
		IList MenuItems { get; }
		object Tag { get; set; }
		string Text { get; }
	}
}
