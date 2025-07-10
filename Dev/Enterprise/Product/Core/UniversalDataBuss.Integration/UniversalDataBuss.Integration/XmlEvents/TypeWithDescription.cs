using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public class TypeWithDescription
	{
		public TypeWithDescription(ZString type)
		{
			this.Type = type;
		}

		public TypeWithDescription(ZString type, ZString description)
			: this(type)
		{
			this.Description = description;
		}

		public ZString Type { get; private set; }
		public ZString Description { get; private set; }

		public override string ToString()
		{
			return Description.IsEmpty ? Type : Description;
		}
	}
}
