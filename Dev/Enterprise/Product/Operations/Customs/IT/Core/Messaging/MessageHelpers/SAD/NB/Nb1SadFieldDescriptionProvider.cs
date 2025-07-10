using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging;

public class Nb1SadFieldDescriptionProvider : NbSadFieldDescriptionProvider
{
	public Nb1SadFieldDescriptionProvider(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override int FieldStartingIndex => 7;

	protected override IEnumerable<CodeDescriptionPair> GetCorrelationsForNotRepeatedFields() => Enumerable.Empty<CodeDescriptionPair>();
}
