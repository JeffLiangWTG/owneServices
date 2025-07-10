using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine
{
	public class AdditionalNoteProvider : IAdditionalNoteProvider
	{
		readonly IStmNoteParent parent;
		public AdditionalNoteProvider(IStmNoteParent parent)
		{
			this.parent = parent;
		}

		public IEnumerable<StmNote> AdditionalNotes
		{
			get
			{
				if (parent is IDocumentSupportable)
				{
					var documentNote = DocumentNote.RetrieveNote(parent);
					if (documentNote != null)
					{
						return new[] { (StmNote)documentNote };
					}
				}

				return Enumerable.Empty<StmNote>();
			}
		}
	}
}
