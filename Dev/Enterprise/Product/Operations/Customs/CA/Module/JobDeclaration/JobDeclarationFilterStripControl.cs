using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_EntrySubmittedDate, Res.GetString("57c10d7d-4750-4447-bc15-f5e47e37bce4", "Submitted Date"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_OH_Forwarder, Res.GetString("f5026fac-03e9-45e4-b75a-dfc059b5f93a", "Service Provider"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_OH_ShippingLine, Res.GetString("3e7a3b99-93c1-43d0-b1a7-b3573a4aa351", "Carrier"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_OH_Importer, Res.GetString("9420804e-3163-46ff-8df6-b5d5165a95ec", "Importer/Consignee"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_OH_Supplier, Res.GetString("565d7ade-8d8b-40cf-b011-f1a198cf180a", "Vendor/Exporter"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.ImporterName, Res.GetString("0746266D-43DD-436b-BA5A-049C34E60978", "Importer/Consignee Name"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.SupplierName, Res.GetString("3403D628-AAE5-4fe1-9F93-22A0DA2B0C76", "Vendor/Exporter Name"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_MessageSubType, Res.GetString("11ee8214-732e-4f6d-9c8f-8fa73656a071", "Entry Type"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.DeclarationNumber, Res.GetString("d03f2f10-7891-4b46-a6fa-fb4cdf6044c6", "Transaction Number"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_EntryStatus, Res.GetString("35dc361b-7229-41c4-a8f4-e43a008c03ea", "Release Status"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_EntryStatusDescription, Res.GetString("ce78137b-7909-402d-ab3b-9b4150326300", "Release Status Description"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_MessageStatus, Res.GetString("0976d950-257c-466b-9da0-e41025c8b735", "Last Message"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_MessageStatusDescription, Res.GetString("5ec294e9-f0ed-4982-8a7c-5cce8a00003f", "Last Message Description"));
				FilteredGrid.SetColumnCaption(JobDeclaration.Schema.JE_DateOfFirstArrival, Res.GetString("287DBC92-4D29-44A5-8D16-E89DBA18D11C", "Date of First Arrival"));
				var releaseDate = FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_EntryAuthorisationDate);
				releaseDate.Caption = Res.GetString("7EA12457-79F6-4E5F-8E0C-6ABD085F5E0C", "Release Date");
				releaseDate.IsVisible = true;
				((ZDateEditColumnStyleInfo)releaseDate).DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
				if (!UniversalReferenceConstants.IsCarmR2)
				{
					FilteredGrid.RemoveFromAvailableColumns(FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.GroupName.Caption == "Bond Information").Select(x => x.ColumnName).ToArray());
				}
			}
		}

		protected override ZBool ShouldSetColorContextKeyFromParentModuleID => true;

		protected override ZBool ShouldAddWHSStatusFieldToGrid
		{
			get { return true; }
		}
	}
}
