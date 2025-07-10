using System;
using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI.Design
{
	public abstract class WrappedCodeDomSerializer : CodeDomSerializer
	{
		protected WrappedCodeDomSerializer(string internalCodeDomSerializerTypeName)
		{
			internalControlCodeDomSerializer = GetWinFormsCodeDomSerializer(internalCodeDomSerializerTypeName);
		}

		readonly CodeDomSerializer internalControlCodeDomSerializer;

		static CodeDomSerializer GetWinFormsCodeDomSerializer(string typeName)
		{
			var designAssembly = typeof(CodeDomSerializer).Assembly;
			var winFormCodeDomSerializerType = designAssembly.GetType(typeName);
			var winFormCodeDomSerializer = (CodeDomSerializer)Activator.CreateInstance(winFormCodeDomSerializerType);
			return winFormCodeDomSerializer;
		}

		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			return internalControlCodeDomSerializer.Deserialize(manager, codeObject);
		}

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			return internalControlCodeDomSerializer.Serialize(manager, value);
		}
	}

	public class ControlCodeDomSerializer : WrappedCodeDomSerializer
	{
		public ControlCodeDomSerializer()
			: base(WinFormsControlCodeDomSerializerTypeFullName)
		{
		}

		public const string WinFormsControlCodeDomSerializerTypeFullName = "System.Windows.Forms.Design.ControlCodeDomSerializer";
	}

	public class ComponentCodeDomSerializer : WrappedCodeDomSerializer
	{
		public ComponentCodeDomSerializer()
			: base(WinFormsComponentCodeDomSerializerTypeFullName)
		{
		}

		public const string WinFormsComponentCodeDomSerializerTypeFullName = "System.ComponentModel.Design.Serialization.ComponentCodeDomSerializer";
	}
}
