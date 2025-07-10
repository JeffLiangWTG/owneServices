using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public interface ISupportJobDocumentRecipient
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Creating a new type for this would actually make instantiation more complex and does not improve the design")]
		IEnumerable<(MultilingualString organisationType, IOrgHeader orgHeader)> SuggestedOrganisations { get; }
	}
}
