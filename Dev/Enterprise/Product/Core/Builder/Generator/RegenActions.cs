using System;

namespace Enterprise.Builder.Generator
{
	[Flags]
	public enum RegenActions
	{
		DataRegen = 1,
		SchemaRegen = 2,
		FullRegen = SchemaRegen | DataRegen
	}
}
