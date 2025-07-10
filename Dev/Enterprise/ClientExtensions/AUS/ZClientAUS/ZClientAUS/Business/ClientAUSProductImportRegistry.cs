using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSProductImportRegistry : AutoClientAUSProductImportRegistry
	{
		public ClientAUSProductImportRegistry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Lookups

		public OrgHeaderCollection ImporterList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public OrgHeaderCollection SupplierList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Properties

		public string DirectoryToStoreExportFiles
		{
			get
			{
				return T6_DirectoryToStoreFiles;
			}
		}

		public string AllProductsFileName
		{
			get
			{
				return T6_AllProductsFileName;
			}
		}

		public string ClientInvoicingFileName
		{
			get
			{
				return T6_ClientInvoicingFileName;
			}
		}

		public string ProductUpdateFileName
		{
			get
			{
				return T6_ProductUpdateFileName;
			}
		}

		[EmailAddress]
		public string UpdateEmailAddress
		{
			get
			{
				return T6_Email;
			}
		}

		public string BackUpFileName
		{
			get
			{
				return T6_BackUpFileName;
			}
		}

		public string DirectoryForImportedPartsLog
		{
			get
			{
				return T6_DirectoryImportedParts;
			}
		}

		public string DirectoryForRejectedPartsLog
		{
			get
			{
				return T6_DirectoryRejectedParts;
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
