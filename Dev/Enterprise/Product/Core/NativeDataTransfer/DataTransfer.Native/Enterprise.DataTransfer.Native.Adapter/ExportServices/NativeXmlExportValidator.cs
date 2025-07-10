using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeXmlExportValidator : IExportValidator
	{
		public IDefinitionFinder DefinitionFinder { get; set; }

		#region IExportValidator Members

		public bool CanBeExported(Type type)
		{
			var tableName = BusinessObjectFactory.GetTableNameFromType(type, false);

			return !NativeXMLSupportValidator.IsTableDeprecated(tableName) && DefinitionFinder.HasDefinitionWithTopTableName(tableName);
		}

		public void Validate(IEnumerable<IBusiness> businessObjects)
		{
			if (!businessObjects.Any())
			{
				throw new NativeXMLUserVisibleException("Select one or more items to create the XML file for.");
			}
			if (CheckHasChangesForBusinessObjects(businessObjects))
			{
				throw new NativeXMLUserVisibleException("You must save before you can export the data.");
			}
		}

		#endregion

		static bool CheckHasChangesForBusinessObjects(IEnumerable<IBusiness> businessObjects)
		{
			foreach (var businessObject in businessObjects)
			{
				if (businessObject.HasChanges)
				{
					return true;
				}
			}
			return false;
		}
	}
}
