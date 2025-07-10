using System.Collections;
using CargoWise.Types;

namespace Enterprise.Integration.ZArchitecture
{
	public interface INoteTypeCollection : IList
	{
		void Add(INoteTypeCollection listOfNoteTypes);
		ZBool IsPredefinedNoteTypeByDescription(ZString description);
	}
}
