//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientAUSProductImportRegistryValidation
//
//    This class should be used for overriding validation in AutoClientAUSProductImportRegistryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSProductImportRegistryValidation : AutoClientAUSProductImportRegistryValidation
	{
		public ClientAUSProductImportRegistryValidation(AutoClientAUSProductImportRegistry parent) : base(parent)
		{
		}

		#region Validation

		protected override void CheckT6_OH_Importer()
		{
			base.CheckT6_OH_Importer();

			if (!Parent.T6_OH_Importer.IsValid)
			{
				Parent.T6_OH_ImporterInfo.AddError("The Importer code entered is not valid");
			}
		}

		protected override void CheckT6_OH_Supplier()
		{
			base.CheckT6_OH_Supplier();

			if (!Parent.T6_OH_Supplier.IsValid)
			{
				Parent.T6_OH_SupplierInfo.AddError("The Supplier code entered is not valid");
			}
			else if (!Parent.T6_OH_Supplier.IsEmpty && Parent.HasChanges)
			{
				ZQuery keyFilter = new ZQuery(ClientAUSProductImportRegistrySchema.T6_OH_Importer, Parent.T6_OH_Importer);
				keyFilter.AddToFilter(ClientAUSProductImportRegistrySchema.T6_OH_Supplier, Parent.T6_OH_Supplier);
				keyFilter.AddToFilter(ClientAUSProductImportRegistrySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				ClientAUSProductImportRegistry productRegistryItem = Parent.Factory.LoadTop1<ClientAUSProductImportRegistry>(keyFilter);
				if (productRegistryItem != null)
				{
					Parent.T6_OH_SupplierInfo.AddError("Product Registry already exists for this Importer & Supplier");
				}
			}
		}

		protected override void CheckT6_AllProductsFileName()
		{
			base.CheckT6_AllProductsFileName();
			MandatoryValidation.CheckEntered(Parent.T6_AllProductsFileNameInfo, "All Products file name");
		}

		protected override void CheckT6_ClientInvoicingFileName()
		{
			base.CheckT6_ClientInvoicingFileName();
			MandatoryValidation.CheckEntered(Parent.T6_ClientInvoicingFileNameInfo, "Client Invoicing file name");
		}

		protected override void CheckT6_ProductUpdateFileName()
		{
			base.CheckT6_ProductUpdateFileName();
			MandatoryValidation.CheckEntered(Parent.T6_ProductUpdateFileNameInfo, "Product Update file name");
		}

		protected override void CheckT6_BackUpFileName()
		{
			base.CheckT6_BackUpFileName();
			MandatoryValidation.CheckEntered(Parent.T6_BackUpFileNameInfo, "Back-up file name");
		}

		protected override void CheckT6_DirectoryToStoreFiles()
		{
			base.CheckT6_DirectoryToStoreFiles();
			MandatoryValidation.CheckEntered(Parent.T6_DirectoryToStoreFilesInfo, "Directory to store all exported & back-up files");
		}

		protected override void CheckT6_DirectoryImportedParts()
		{
			base.CheckT6_DirectoryImportedParts();
			MandatoryValidation.CheckEntered(Parent.T6_DirectoryImportedPartsInfo, "Directory to store Imported Products log file");
		}

		protected override void CheckT6_DirectoryRejectedParts()
		{
			base.CheckT6_DirectoryRejectedParts();
			MandatoryValidation.CheckEntered(Parent.T6_DirectoryRejectedPartsInfo, "Directory to store Rejected Products log file");
		}

		#endregion
	}
}
