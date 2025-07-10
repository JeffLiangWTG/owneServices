#if DEBUG
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating
{
	public abstract class UpdateTemplateCommand : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UpdateTemplateCommand(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[CargoWise.ComponentModel.MaxLength(128)]
		public abstract ZString Description { get; }

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		public abstract void Execute();
	}
}
#endif
