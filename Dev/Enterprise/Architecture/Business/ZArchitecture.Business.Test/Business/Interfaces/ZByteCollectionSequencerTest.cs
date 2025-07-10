using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZByteCollectionSequencerTest : CollectionSequencerTest
	{
		protected override SchemaNumericColumn SequenceSchemaColumn
		{
			get { return DummyBizoSchema.Z0_Byte; }
		}

		protected override Action<DummyBusinessObject> SequenceValidation
		{
			get { return d => d.Validation.ValidateZ0_Byte(); }
		}

		protected override void SetSequence(DummyBusinessObject dummy, int value)
		{
			dummy[SequenceSchemaColumn] = ZByte.ParseSafe(value.ToString(), 0);
		}
	}
}
