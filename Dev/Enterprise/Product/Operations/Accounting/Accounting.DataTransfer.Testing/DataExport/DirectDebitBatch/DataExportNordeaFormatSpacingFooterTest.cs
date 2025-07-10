using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportNordeaFormatSpacingFooter))]
	internal class DataExportNordeaFormatSpacingFooterTest : DataExportDirectDebitBatchTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportNordeaFormatSpacingFooter(Factory, DirectDebitBatchHeader);
		}

		#endregion
	}
}
