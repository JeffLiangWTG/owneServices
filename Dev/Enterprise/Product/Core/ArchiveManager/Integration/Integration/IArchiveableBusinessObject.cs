using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveableBusinessObject
	{
		ArchiveReferenceKey NaturalKey { get; }
		IEnumerable<ArchiveReferenceKey> AdditionalKeys { get; }
		IEnumerable<ArchiveDocumentDescriptor> ArchiveDocuments { get; }

		/// <summary>
		/// BusinessObject returned must also implement IDocumentSupportable, IDocManagerSupport, IStmNoteParent,
		/// otherwise Document Generation will log an error and skip archiving this set.
		/// </summary>
		BusinessObject ArchiveableBusinessObject { get; }

		/// <summary>
		/// Temporary user context switch to this branch
		/// </summary>
		Guid BranchPK { get; }
	}
}
