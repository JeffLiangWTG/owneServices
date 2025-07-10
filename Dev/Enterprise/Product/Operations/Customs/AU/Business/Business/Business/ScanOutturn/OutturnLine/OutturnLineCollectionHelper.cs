using System;
using System.Linq;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OutturnLineCollectionHelper
	{
		protected static void FillImportMappingCollection(ImportWizardMappingCollection mappingCollection)
		{
			SetColumnMapping(mappingCollection, OutturnLine.Schema.ConsignmentRef, 0);
			SetColumnMapping(mappingCollection, OutturnLine.Schema.Status, 1);
			SetColumnMapping(mappingCollection, OutturnLine.Schema.ScannedDateTime, 2);
			SetColumnMapping(mappingCollection, OutturnLine.Schema.Count, 3);
		}

		protected static void AddCollectionProperties(ImportCollectionInfoImpl impl)
		{
			AddProperty(impl, AutoOutturnLine.Schema.ConsignmentRef, "ConsignmentRef"); // File column name
			AddProperty(impl, AutoOutturnLine.Schema.Status, "Status"); // File column name
			AddProperty(impl, AutoOutturnLine.Schema.ScannedDateTime, "ScannedDateTime"); // Round is ok for our purposes
			AddProperty(impl, AutoOutturnLine.Schema.Count, "Count"); // File column name
		}

		static void AddProperty(ImportCollectionInfoImpl impl, string fieldName, string headerText)
		{
			var a = new ImportPropertyInfoImpl<OutturnLine>(fieldName) { HeaderText = headerText };
			impl.Add(new ImportPropertyInfoImpl<OutturnLine>(fieldName) { HeaderText = headerText });
		}

		static void SetColumnMapping(ImportWizardMappingCollection mappingCollection, string propertyName, int fileColumnIndex)
		{
			var mapping = mappingCollection.Cast<ImportWizardMapping>().FirstOrDefault(m => m.Text == propertyName);

			if (mapping != null)
			{
				mapping.AddFileColumnIndex(fileColumnIndex);
			}
			else
			{
				throw new ArgumentException(String.Format("The property '{0}' does not exist", propertyName));
			}
		}
	}
}
