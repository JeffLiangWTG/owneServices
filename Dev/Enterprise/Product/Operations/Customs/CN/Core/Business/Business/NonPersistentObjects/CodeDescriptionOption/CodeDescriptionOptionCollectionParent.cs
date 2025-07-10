using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CodeDescriptionOptionCollectionParent : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CodeDescriptionOptionCollectionParent(BusinessObjectFactory factory, ICodeDescriptionOptionStorage storage) : base(factory)
		{
			Storage = Argument.NotNull(storage, nameof(storage));
			OptionCollection = new CodeDescriptionOptionCollection(this);
			OptionCollection.Load();
		}

		public ICodeDescriptionOptionStorage Storage { get; private set; }

		public CodeDescriptionOptionCollection OptionCollection { get; }
	}
}
