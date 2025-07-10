using System;
using CargoWise.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class ReverseStringPropertyDescriptor : KPropertyDescriptor
	{
		public ReverseStringPropertyDescriptor(string name, SchemaStringColumn schemaStringColumn)
			: base(null, name, Array.Empty<Attribute>())
		{
			this.schemaStringColumn = schemaStringColumn;
		}

		readonly SchemaStringColumn schemaStringColumn;

		public override Type PropertyType
		{
			get { return typeof(ZString); }
		}

		protected override object GetValueCore(object component)
		{
			var value = ((BusinessObject)component)[schemaStringColumn];
			return new ZString(Reverse(value.ToString()));
		}

		protected override void SetValueCore(object component, object value)
		{
			((BusinessObject)component)[schemaStringColumn] = Reverse(value.ToString());
		}

		protected string Reverse(string s)
		{
			var charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}
	}
}
