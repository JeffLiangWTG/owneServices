using System.Text;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.AutomaticContainerCreationRegistryItemEditor, Enterprise.Registry.GUI")]
	public class AutomaticContainerCreationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AutomaticContainerCreation>
	{
		public AutomaticContainerCreationRegistryDataType() : base(RegistryDataTypes.Codes.String, new AutomaticContainerCreation())
		{
		}

		protected override byte[] SerialiseCore(AutomaticContainerCreation value)
		{
			return Encoding.Unicode.GetBytes(value.CreateConfiguration);
		}

		protected override AutomaticContainerCreation DeserialiseCore(byte[] value)
		{
			return new AutomaticContainerCreation()
			{
				CreateConfiguration = Encoding.Unicode.GetString(value)
			};
		}
	}
}
