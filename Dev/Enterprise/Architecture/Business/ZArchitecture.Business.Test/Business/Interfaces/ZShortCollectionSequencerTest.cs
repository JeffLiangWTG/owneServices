using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZShortCollectionSequencerTest : CollectionSequencerTest
	{
		protected override SchemaNumericColumn SequenceSchemaColumn
		{
			get { return DummyBizoSchema.Z0_Short; }
		}

		protected override Action<DummyBusinessObject> SequenceValidation
		{
			get { return d => d.Validation.ValidateZ0_Short(); }
		}

		protected override void SetSequence(DummyBusinessObject dummy, int value)
		{
			dummy[SequenceSchemaColumn] = ZShort.ParseSafe(value.ToString(), 0);
		}
	}
}
