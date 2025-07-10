using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Module.Testing
{
	public class DeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestRemovedColumns()
		{
			var declarations = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var userControl = new DeclarationFilterStripControl(declarations, new FilterStripBusinessObject()))
			{
				CombineAssertions(() =>
				{
					var grid = userControl.FilteredGrid;
					var removedColumns = new string[]
					{
						EMCSJobDeclaration.Schema.JE_MessageType,
						EMCSJobDeclaration.Schema.JE_ContainerMode,
						EMCSJobDeclaration.Schema.FreightContainerMode,
						EMCSJobDeclaration.Schema.JE_DateOfFirstArrival,
						EMCSJobDeclaration.Schema.JE_EntryStatus,
						EMCSJobDeclaration.Schema.JE_ETAOfDischarge,
						EMCSJobDeclaration.Schema.JE_ETDOfLoading,
						EMCSJobDeclaration.Schema.JE_ExportGoodsType,
						EMCSJobDeclaration.Schema.JE_RL_NKFinalDestination,
						EMCSJobDeclaration.Schema.JE_DateAtFinalDestination,
						EMCSJobDeclaration.Schema.JE_RL_NKPortOfFirstArrival,
						EMCSJobDeclaration.Schema.ForwarderName,
						EMCSJobDeclaration.Schema.ImporterName,
						EMCSJobDeclaration.Schema.JE_TotalVolume,
						EMCSJobDeclaration.Schema.JE_TotalWeight,
						EMCSJobDeclaration.Schema.JE_EntrySubmittedDate
					};

					foreach (var column in removedColumns)
					{
						AssertNull(column, grid.GetColumnStyle(column));
					}
				});
			}
		}

		public void TestAddedAndRenamedColumns()
		{
			var declarations = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var userControl = new DeclarationFilterStripControl(declarations, new FilterStripBusinessObject()))
			{
				CombineAssertions(() =>
				{
					var grid = userControl.FilteredGrid;
					grid.SetDataBinding(declarations, "");
					var addedColumns = new List<(string, string)>()
					{
						(EMCSJobDeclaration.Schema.JE_DeclarantType, "Declaration Type"),
						(EMCSJobDeclaration.Schema.EADNumber, "EAD Number"),
						(EMCSJobDeclaration.Schema.JE_EntryAuthorisationDate, "Report Date"),
						(EMCSJobDeclaration.Schema.JE_OH_Supplier, "Consignor"),
						(EMCSJobDeclaration.Schema.JE_OH_Importer, "Consignee"),
						(EMCSJobDeclaration.Schema.JE_MessageSubType, "Destination Type"),
						(EMCSJobDeclaration.Schema.ZG_SubmissionType, "Submission Type"),
						(EMCSJobDeclaration.Schema.ZG_DeferredSubmission, "Deferred"),
						(EMCSJobDeclaration.Schema.ZG_GuarantorType, "Guarantor(s)"),
						(EMCSJobDeclaration.Schema.ZG_OriginType, "Origin Type"),
						(EMCSJobDeclaration.Schema.InvoiceNumber, "Invoice Number"),
						(DeclarationFilterStripControl.GoodsOwner, "Goods Owner"),
						(DeclarationFilterStripControl.DispatchWarehouse, "Dispatch Warehouse"),
						(DeclarationFilterStripControl.DestinationWarehouse, "Destination Warehouse"),
						(DeclarationFilterStripControl.CarrierAgent, "Carrier Agent"),
						(DeclarationFilterStripControl.Transporter, "Transporter"),
						(EMCSJobDeclaration.Schema.JE_DateAtOrigin, "Dispatch Time"),
						(EMCSJobDeclaration.Schema.JE_OwnerRef, "Local Reference Number"),
						(EMCSJobDeclaration.Schema.JE_EntryStatusDescription, "Registration Status Description")
					};

					foreach (var (column, caption) in addedColumns)
					{
						AssertEquals(caption, grid.GetColumnCaption(column));
					}
				});
			}
		}

		public void TestDefaultColumns()
		{
			var declarations = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			string[] defaultColumnsOrder =
			{
				EMCSJobDeclaration.Schema.JE_DeclarantType,
				EMCSJobDeclaration.Schema.JE_GB,
				EMCSJobDeclaration.Schema.JE_DeclarationReference,
				EMCSJobDeclaration.Schema.JE_DateAtOrigin,
				EMCSJobDeclaration.Schema.JE_OwnerRef,
				EMCSJobDeclaration.Schema.EADNumber,
				EMCSJobDeclaration.Schema.JE_OH_Supplier,
				EMCSJobDeclaration.Schema.JE_OH_Importer,
				DeclarationFilterStripControl.GoodsOwner,
				DeclarationFilterStripControl.DispatchWarehouse,
				DeclarationFilterStripControl.DestinationWarehouse,
				EMCSJobDeclaration.Schema.JE_MessageSubType,
				EMCSJobDeclaration.Schema.ZG_SubmissionType,
				EMCSJobDeclaration.Schema.ZG_DeferredSubmission,
				EMCSJobDeclaration.Schema.ZG_GuarantorType,
				EMCSJobDeclaration.Schema.ZG_OriginType,
				EMCSJobDeclaration.Schema.InvoiceNumber,
				DeclarationFilterStripControl.CarrierAgent,
				DeclarationFilterStripControl.Transporter,
			};

			using (var userControl = new DeclarationFilterStripControl(declarations, new FilterStripBusinessObject()))
			{
				var grid = userControl.FilteredGrid;
				grid.SetDataBinding(declarations, "");
				AssertSequencesEqual(defaultColumnsOrder, grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}
	}
}
