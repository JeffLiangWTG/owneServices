using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture
{
	public interface IAdditionalNoteProvider
	{
		IEnumerable<StmNote> AdditionalNotes { get; }
	}
}
