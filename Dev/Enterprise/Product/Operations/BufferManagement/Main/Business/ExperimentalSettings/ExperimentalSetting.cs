using CargoWise.EntityFramework;
using CargoWise.Types;
using Newtonsoft.Json;

namespace Enterprise.BufferManagement.Business
{
	[JsonConverter(typeof(ExperimentalSettingSerializer))]
	public class ExperimentalSetting : NonPersistentBusinessObject
	{
		public ZString Key { get; set; }
		public ZString Value { get; set; }
	}
}
