using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.NZP
{
	public sealed class NZPDataRegistry : RegistryItemSet
	{
		NZPDataRegistry()
		{
		}

		#region Instance

		public static NZPDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new NZPDataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static NZPDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "NZ Post Client Extensions";

		#region CMSExportDirectory

		public ZString CMSExportDirectory
		{
			get { return new ZString(CMSExportDirectoryRaw.Value); }
			set { CMSExportDirectoryRaw.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion

		#region CMSLastDateExported

		public ZDateTime CMSLastDateExported
		{
			get { return new ZDateTime(CMSLastDateExportedRaw.Value); }
			set { CMSLastDateExportedRaw.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ConvertToDateTime(value)); }
		}

		#endregion

		#region ActualLastDateExported

		public ZDateTime ActualLastDateExported
		{
			get { return new ZDateTime(ActualLastDateExportedRaw.Value); }
			set { ActualLastDateExportedRaw.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ConvertToDateTime(value)); }
		}

		#endregion

		#region CMSWarehouseCode

		public ZString GetCMSWarehouseCode(ZString branchCode)
		{
			ZString result = ZString.Empty;
			if (!branchCode.IsEmpty)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				GlbBranch branch = (GlbBranch)factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, branchCode);
				if (branch != null)
				{
					result = GetCMSWarehouseCode(branch.PK);
				}
			}
			return result;
		}

		public ZString CurrentCMSWarehouseCode
		{
			get { return GetCMSWarehouseCode(Env.CurrentBranch.PK); }
			set { SetCMSWarehouseCode(Env.CurrentBranch.PK, value); }
		}

		ZString GetCMSWarehouseCode(ZGuid branchPK)
		{
			ZString result = ZString.Empty;
			result = new ZString(CMSWarehouseCodeRaw.GetValueWithoutFallback(Guid.Empty, branchPK.ToGuid(), Guid.Empty));
			return result;
		}

		void SetCMSWarehouseCode(ZGuid branchPK, ZString value)
		{
			CMSWarehouseCodeRaw.SetValue(Guid.Empty, branchPK.ToGuid(), Guid.Empty, value.ToString());
		}

		#endregion

		#region Invoice Terms in T File

		public ReadOnlyCodeDescriptionPairList InvoiceTermsInTFile
		{
			get { return InvoiceTermsInTFileRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { InvoiceTermsInTFileRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region Implementation

		const string CMSCategory = Category + "/CMS";

		#region CMSExportDirectory

		internal StringRegistryItem CMSExportDirectoryRaw
		{
			get
			{
				return GetItem("ExportDirectory", delegate
				{
					return new StringRegistryItem("ExportDirectory", (NoResString)CMSCategory, (NoResString)"Export Directory", (NoResString)"Specify Directory to export files for CMS", RegistryStorageFlags.Company, RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region CMSLastDateExportedRaw

		internal DateTimeRegistryItem CMSLastDateExportedRaw
		{
			get
			{
				return GetItem("LastDateExported", delegate
				{
					return new DateTimeRegistryItem("LastDateExported", (NoResString)CMSCategory, (NoResString)"Last Date Exported", (NoResString)"Last Date files were created", RegistryStorageFlags.Company, RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region ActualLastDateExportedRaw

		internal DateTimeRegistryItem ActualLastDateExportedRaw
		{
			get
			{
				return GetItem("ActualLastDateExported", delegate
				{
					return new DateTimeRegistryItem("ActualLastDateExported", (NoResString)CMSCategory, (NoResString)"Actual Last Date Exported", null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached, new DateTime(2006, 4, 1));
				});
			}
		}

		#endregion

		#region CMSWarehouseCodeRaw

		internal StringRegistryItem CMSWarehouseCodeRaw
		{
			get
			{
				return GetItem("WarehouseCodeRaw", delegate
				{
					return new StringRegistryItem("WarehouseCodeRaw", (NoResString)CMSCategory, (NoResString)"Warehouse Code", (NoResString)"Set up Warehouse Code for each branch", RegistryStorageFlags.Branch, RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region Invoice Terms in T File

		internal CodeDescriptionPairListRegistryItem InvoiceTermsInTFileRaw
		{
			get
			{
				return GetItem("InvoiceTermsInTFile", delegate
				{
					CodeDescriptionPairList invoiceTermsInTFileList = new CodeDescriptionPairList();
					invoiceTermsInTFileList.AddPair(Core.Constants.InvoiceTerms.CashOnDelivery);
					invoiceTermsInTFileList.AddPair(Core.Constants.InvoiceTerms.PaymentInAdvance);

					CodeDescriptionPairListEditorInfo editorInfo = new CodeDescriptionPairListEditorInfo(true, false, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);
					editorInfo.SetCodeColumnCaption((NoResString)"Invoice Term");

					return new CodeDescriptionPairListRegistryItem(
						"InvoiceTermsInTFile",
						(NoResString)CMSCategory,
						(NoResString)"Invoice Terms in T File",
						(NoResString)"Set up the Invoice Terms of A/R Invoices to be included in the T file (Cash Sales). All A/R Invoices with Invoice Terms not in this list will be included in the S file (Credit Sales)",
						OrgARTermsSchema.PY_InvoiceTerm.MaxLength,
						editorInfo,
						RegistryStorageFlags.Company,
						false,
						RegistryOptions.Default,
						invoiceTermsInTFileList,
						false);
				});
			}
		}

		#endregion

		DateTime ConvertToDateTime(ZDateTime value)
		{
			return value.IsValid ? value.ToDateTime() : DateTime.MinValue;
		}

		#endregion
	}
}
