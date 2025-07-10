using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

static class T2LPOUSWrapperMappingExtensions
{
	public static IReadOnlyCollection<DocumentCommonWrapper> ToReadOnlyWrapperCollection(this IEnumerable<AdditionalInfo> collection) =>
		collection
		.Select(doc => new { doc.CSI_Code, doc.CSI_ReferenceNumber })
		.Distinct()
		.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber))
		.ToList()
		.AsReadOnly();

	public static IReadOnlyCollection<DocumentCommonWrapper> ToReadOnlyWrapperCollection(this IEnumerable<PreviousDocument> collection) =>
		collection
		.Select(doc => new { doc.CSI_Code, doc.CSI_ReferenceNumber })
		.Distinct()
		.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber))
		.ToList()
		.AsReadOnly();

	public static IReadOnlyCollection<DocumentCommonWrapper> ToReadOnlyWrapperCollection(this IEnumerable<SupportingDocument> collection) =>
		collection
		.Select(doc => new { doc.CSI_Code, doc.CSI_ReferenceNumber })
		.Distinct()
		.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber))
		.ToList()
		.AsReadOnly();
}
