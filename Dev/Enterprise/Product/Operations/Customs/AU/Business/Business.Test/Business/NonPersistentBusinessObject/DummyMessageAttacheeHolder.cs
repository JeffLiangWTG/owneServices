using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class DummyMessageAttacheeHolder : IMessageAttacheeParent
	{
		public DummyMessageAttacheeHolder(BusinessObjectFactory factory)
		{
			fFactory = factory;
			MessageAttacheesExposed = System.Array.Empty<IMessageAttachee>();
		}

		public BusinessObjectFactory Factory
		{
			get { return fFactory; }
		}
		readonly BusinessObjectFactory fFactory;

		public IMessageAttachee[] MessageAttacheesExposed;
		public IMessageAttachee[] MessageAttachees
		{
			get { return MessageAttacheesExposed; }
		}
	}
}
