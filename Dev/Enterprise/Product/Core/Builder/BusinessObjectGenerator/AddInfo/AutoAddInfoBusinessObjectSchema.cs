namespace Enterprise.BusinessObjectGenerator
{
	public class AutoAddInfoBusinessObjectSchema : AutoBusinessObjectSchema
	{
		public AutoAddInfoBusinessObjectSchema(BusinessObjectInfo info) : base(info)
		{
		}

		protected override string InheritsFrom => "CargoWise.Schema.Schema, IAddInfoSchema";
	}
}
