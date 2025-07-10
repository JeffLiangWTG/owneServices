using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class DocumentUDFFieldBuilderWrapperCollection : GenericWrapperCollection<ReportFilterBuilderWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public DocumentUDFFieldBuilderWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var documenter in new UserDefinedFieldBuilderBuilder().UserDefinedFieldBuilderDocumenters)
			{
				Add(new ReportFilterBuilderWrapper(documenter, Factory));
			}
			this.Sort("Useage");
		}
	}
}
