using System;
using System.CodeDom;

namespace CargoWise.Common.CodeDom
{
	[Serializable]
	public class KCodeTypeReference : CodeTypeReference
	{
		protected KCodeTypeReference(Type type) : base(type) { }
		public KCodeTypeReference() { }
		public KCodeTypeReference(CodeTypeParameter typeParameter) : base(typeParameter) { }
		public KCodeTypeReference(string typeName) : base(typeName) { }
		public KCodeTypeReference(CodeTypeReference arrayType, int rank) : base(arrayType, rank) { }
		public KCodeTypeReference(string typeName, CodeTypeReferenceOptions codeTypeReferenceOption) : base(typeName, codeTypeReferenceOption) { }
		public KCodeTypeReference(string baseType, int rank) : base(baseType, rank) { }
		public KCodeTypeReference(string typeName, params CodeTypeReference[] typeArguments) : base(typeName, typeArguments) { }
		public KCodeTypeReference(Type type, CodeTypeReferenceOptions codeTypeReferenceOption) : base(type, codeTypeReferenceOption) { }

		public static KCodeTypeReference FromType(Type type)
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
			if (type.IsGenericParameter)
			{
				return new KCodeTypeReference(new CodeTypeParameter(type.Name));
			}
			else
			{
				return new KCodeTypeReference(type);
			}
		}
	}
}
