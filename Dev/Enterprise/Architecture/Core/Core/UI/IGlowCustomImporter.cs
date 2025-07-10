using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Core
{
	public interface IGlowCustomImporter
	{
		/// <summary>
		/// A function for importing into `parent` a child. The child is defined by values in
		/// childValues which correspond to the field names in childHeaders.
		/// </summary>
		/// <returns>True if it succeeded, False if it failed.</returns>

		//This importer involves both parent and children customized importing. However, not both customizations are always needed together.
		//It can be split into two separate interfaces. One focuses on the businessObject itself. Another one deals with its childless children.
		//In GlowCollectionImporter, when importing businessObject, use ObjectFactory.Get<interfaceDealingWithParent>. When importing child collection, use ObjectFactory.Get<interfaceDealingWithChildlessChildren>.

		bool ImportChildlessChildren(BusinessObject parent, INotifications logger, int rowIndex, string[] childHeaders, ImportPreviewLineDetails[] childValues);
		void ConvertCustomLine(BusinessObject parent, string code, INotifications logger, int rowIndex, string propertyName);
		bool IsEmptyValueAllowed(string propertyName);
		bool ShouldCustomizeChildrenImport { get; }
	}
}
