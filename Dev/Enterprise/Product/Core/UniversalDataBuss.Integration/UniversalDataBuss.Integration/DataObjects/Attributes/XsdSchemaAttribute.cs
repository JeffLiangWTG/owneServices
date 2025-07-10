using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class XsdSchemaAttribute : Attribute
	{
		public XsdSchemaAttribute(Placement placement)
		{
			this.Placement = placement;
		}

		public XsdSchemaAttribute(string schemaName)
		{
			this.Placement = Placement.External;
			this.SchemaName = schemaName;
		}

		public Placement Placement { get; private set; }
		public string SchemaName { get; private set; }
	}

	public enum Placement { Inner, Outer, External }
}
