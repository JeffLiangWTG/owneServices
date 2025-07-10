using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Native.Common
{
	public interface IEntity : IGraphNode<IEntity>, IEntityInfo
	{
		event EventHandler InternalPKChanged;
		new Guid InternalPK { get; set; }

		object this[string index] { get; set; }

		EntityAction Action { get; set; }
		string EntityName { get; }
		IEntityDefinition Definition { get; }

		IEnumerable<Property> Properties { get; }
		int PropertyCount { get; }
		bool HasProperty(string properyName);

		IEnumerable<UnmatchOrgRecord> Notes { get; }
		void AddNote(UnmatchOrgRecord note);

		/// <summary>
		/// Set this to true to tell the engine not to bother trying to find an existing row (from natural keys) if the interceptor cannot find one. 
		/// </summary>
		bool DisableNativeEnginesOwnNaturalKeyMatch { get; set; }

		EntityCollection ChildrenCollection { get; }
	}
}
