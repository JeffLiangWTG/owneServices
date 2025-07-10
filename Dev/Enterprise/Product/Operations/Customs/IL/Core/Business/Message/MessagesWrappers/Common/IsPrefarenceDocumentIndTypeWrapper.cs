using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public sealed class IsPrefarenceDocumentIndTypeWrapper : IIsPrefarenceDocumentIndType
	{
		IsPrefarenceDocumentIndTypeWrapper(bool value)
		{
			this.value = value;
		}

		public static IsPrefarenceDocumentIndTypeWrapper New(bool value) => new IsPrefarenceDocumentIndTypeWrapper(value);

		public bool Value => value;

		readonly bool value;
	}
}
