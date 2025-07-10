using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging;

public abstract class CustomsFieldDescriptionsProviderWithSequenceNumber<T> : CustomsFieldDescriptionsProvider<T>
	where T : CodeDescriptionPairList, new()
{
	protected CustomsFieldDescriptionsProviderWithSequenceNumber(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	Dictionary<ZString, ZString> Correlations => correlations ?? (correlations = GetCorrelations().ToDictionary(x => (ZString)x.Item1, x => (ZString)x.Item2));
	Dictionary<ZString, ZString> correlations;

	public ZString GetDescriptionBySequenceNumber(ZString sequenceNumber) => GetDescriptionByReferenceCode(GetReferenceCodeBySequenceNumber(sequenceNumber));

	public ZString GetReferenceCodeBySequenceNumber(ZString sequenceNumber) => Correlations.ContainsKey(sequenceNumber) ? Correlations[sequenceNumber] : ZString.Empty;

	public ZString GetSequenceNumberByReferenceCode(ZString referenceCode) => Correlations.ContainsValue(referenceCode) ? Correlations.FirstOrDefault(x => x.Value == referenceCode).Key : ZString.Empty;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	protected abstract IEnumerable<(string, string)> GetCorrelations();
}
