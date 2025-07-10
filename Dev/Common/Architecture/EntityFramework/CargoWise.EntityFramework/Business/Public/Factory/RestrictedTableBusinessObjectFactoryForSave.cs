using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class RestrictedTableBusinessObjectFactoryForSave : BusinessObjectFactory
	{
		protected abstract ICollection<string> GetAllowedTablesToSave();

		public ZString AdditionalInformation { get; set; }

		protected override void SaveCore()
		{
			var invalidTablesFound = new List<string>();
			var allowedTables = GetAllowedTablesToSave();
			foreach (var bizo in ((IBusinessObjectFactoryInternals)this).AllBusinessObjects)
			{
				if (bizo.IsSavedByFactory && bizo.HasChanges && !allowedTables.Contains(bizo.TableName))
				{
					invalidTablesFound.Add(bizo.TableName);
				}
			}

			if (invalidTablesFound.Count > 0)
			{
				ErrorReporter.ReportOnce(
@$"Found BusinessObject from invalid table to be saved in {NameForDebugging} factory
Tables: [{string.Join(";", invalidTablesFound)}]
{AdditionalInformation}");
			}

			base.SaveCore();
		}
	}
}
