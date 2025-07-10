using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EnableAddEditAndDeleteLogsItemCollection))]
	sealed class EnableAddEditAndDeleteLogsItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EnableAddEditAndDeleteLogsItemCollection>
	{
		public void TestDefaultValue()
		{
			var defaultCollection = EnableAddEditAndDeleteLogsItemCollection.DefaultValue;
			var expectedCollection = new EnableAddEditAndDeleteLogsItemCollection
			{
				new EnableAddEditAndDeleteLogsItem
				{
					Table = ProcessTasksSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				},
				new EnableAddEditAndDeleteLogsItem
				{
					Table = WorkItemSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				},
				new EnableAddEditAndDeleteLogsItem
				{
					Table = AccComplianceSequenceSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				},
				new EnableAddEditAndDeleteLogsItem
				{
					Table = HVLVConsignmentSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				},
				new EnableAddEditAndDeleteLogsItem
				{
					Table = HVLVBookingHeaderSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				},
				new EnableAddEditAndDeleteLogsItem
				{
					Table = HVLVOuterPackageSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				},
				new EnableAddEditAndDeleteLogsItem
				{
					Table = HVLVOriginLoadListSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				}
			};

			AssertEquals(expectedCollection.Count, defaultCollection.Count);
			foreach (var item in expectedCollection.Cast<EnableAddEditAndDeleteLogsItem>())
			{
				AssertEquals(true, defaultCollection.Cast<EnableAddEditAndDeleteLogsItem>().Any(
					t =>
					item.Table.ToString() == t.Table.ToString()
					&& item.EnableADDLogs == t.EnableADDLogs
					&& item.EnableEDTLogs == t.EnableEDTLogs
					&& item.EnableEDTLogs == t.EnableEDTLogs));
			}
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override EnableAddEditAndDeleteLogsItemCollection GetCollectionToTest()
		{
			return new EnableAddEditAndDeleteLogsItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EnableAddEditAndDeleteLogsItem();
		}

		#endregion
	}
}
