using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZIntCollectionSequencerTest : CollectionSequencerTest
	{
		protected override SchemaNumericColumn SequenceSchemaColumn
		{
			get { return DummyBizoSchema.Z0_Number; }
		}

		protected override Action<DummyBusinessObject> SequenceValidation
		{
			get { return d => d.Validation.ValidateZ0_Number(); }
		}

		protected override void SetSequence(DummyBusinessObject dummy, int value)
		{
			dummy[SequenceSchemaColumn] = value;
		}
	}
}
