using System;
using CargoWise.Schema;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveItem
	{
		Guid PK { get; }

		SchemaColumn PKColumn { get; }

		bool Purgeable { get; set; }

		Guid ParentPK { get; }

		SchemaColumn ParentPKColumn { get; }

		bool IsReversed { get; }

		string HumanReadableName { get; }

		string TableCode { get; set; }

		int TotalDocumentsDeleted { get; set; }
	}
}
