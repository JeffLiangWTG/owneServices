using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IRefSysConfigType
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZString ZRT_ConfigCode { get; set; }
				ZString ZRT_Description { get; set; }
				ZString ZRT_LongDescription { get; set; }
			}
		}
	}
}
