using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IQueryDataProvider
	{
		IEnumerable<Context> GetContextCollection(ZGuid sourcePK);
		ZString ContextReference { get; }
		DataContextType ContextType { get; }
		ZString CredentialKey { get; }
	}
}
