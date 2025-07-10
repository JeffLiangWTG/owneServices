using System.CodeDom;
using System.ComponentModel.Design.Serialization;
using System.Security;

namespace CargoWise.ComponentModel.Design
{
	[SecurityCritical]
	public class CompileTimeComponentValidationCodeDomSerializer : KCodeDomSerializer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Method name")]
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			object result = null;
			string message = value == null ? null : ((CompileTimeComponentValidation)value).Message;
			if (!string.IsNullOrEmpty(message))
			{
				CodeTypeReferenceExpression typeRef = new CodeTypeReferenceExpression(typeof(CompileTimeComponentValidation));
				result = new CodeMethodInvokeExpression(typeRef, "Error", new CodePrimitiveExpression(message));
			}
			return result;
		}

		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{ return null; }
	}
}
