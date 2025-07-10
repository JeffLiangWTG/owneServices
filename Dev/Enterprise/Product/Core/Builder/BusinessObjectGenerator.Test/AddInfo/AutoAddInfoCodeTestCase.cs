using System.Data;
using CargoWise.BuildTools;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	public abstract class AutoAddInfoCodeTestCase : AutoCodeTestCase
	{
		protected override BusinessObjectInfo CreateInfo(DataTable table, string[] indexes, string[] literalOnlyColumns, string[] nonBlankFilteredIndexColumns, string baseClassName = "BusinessObject", string @namespace = "Enterprise", bool masterFileReference = true, BuildXmlBizOEntryCollection masterFiles = null)
			=> CreateInfo(table, indexes, literalOnlyColumns, nonBlankFilteredIndexColumns, baseClassName, @namespace,null, null, masterFileReference, masterFiles, null, null);

		protected BusinessObjectInfo CreateInfo(DataTable table, string[] indexes, string[] literalOnlyColumns, string[] nonBlankFilteredIndexColumns, string baseClassName = "BusinessObject", string @namespace = "Enterprise", string parentSchema = null, string parentView = null, bool masterFileReference = true, BuildXmlBizOEntryCollection masterFiles = null, string childTableName = null, AutoProperty[] childTableProperties = null)
		{
			return new AddInfoBusinessObjectInfo(
				base.CreateInfo(table, indexes, literalOnlyColumns, nonBlankFilteredIndexColumns, baseClassName, @namespace, masterFileReference, masterFiles),
				oldPrefix: "TT",
				baseValidationClassName: baseClassName + "Validation",
				baseLookupsClassName: baseClassName + "Lookups",
				underlyingTableName: baseClassName,
				parentSchema: parentSchema,
				parentView: parentView,
				childTableName: childTableName,
				childTableProperties: childTableProperties
			);
		}
	}
}
