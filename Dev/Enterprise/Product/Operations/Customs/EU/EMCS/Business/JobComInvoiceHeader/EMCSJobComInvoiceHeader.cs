using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceHeader : Customs.Business.BaseJobComInvoiceHeader
		, Integration.Customs.EUEMCS.IJobComInvoiceHeader
		, Customs.Business.IInvoiceLineTypeSupporter
	{
		public EMCSJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New

		public new EMCSJobDeclaration JobDeclaration
		{
			get { return (EMCSJobDeclaration)base.JobDeclaration; }
		}

		public new EMCSJobComInvoiceLineViewCollection InvoiceLines
		{
			get { return (EMCSJobComInvoiceLineViewCollection)base.InvoiceLines; }
		}

		public new EMCSJobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (EMCSJobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		public new EMCSJobComInvoiceGroupHeader Master
		{
			get { return (EMCSJobComInvoiceGroupHeader)base.Master; }
		}

		public new EMCSJobComInvoiceGroupHeader GroupHeader
		{
			get { return (EMCSJobComInvoiceGroupHeader)base.GroupHeader; }
		}

		#endregion

		#region Overridden

		public override ZString JZ_InvoiceNumber
		{
			get => base.JZ_InvoiceNumber;
			set
			{
				bool hasChanged = JZ_InvoiceNumber != value;
				base.JZ_InvoiceNumber = value;
				if (hasChanged && !IsValidationSuspended)
				{
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JZ_InvoiceDate = ZDateTime.Empty;
		}
		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new EMCSJobComInvoiceHeaderLookups(this);
		}

		protected override Customs.Business.BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			if (JobDeclaration != null)
			{
				return new EMCSJobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
			}

			return null;
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => JobDeclaration?.EMCSProvider.GetNewInvoiceHeaderValidation(this) ?? new EMCSJobComInvoiceHeaderValidation(this);

		public override Type GetDeclarationTypeForFakeDeclarationCreatorForInvoice() => typeof(EMCSJobDeclaration);

		public override OrgHeader Supplier
		{
			get
			{
				var result = base.Supplier;

				if (result == null)
				{
					var dec = JobDeclaration;
					var decOwnerDocumentaryAddress = dec?.OwnerDocumentaryAddress;
					var decOwner = decOwnerDocumentaryAddress?.Organisation;
					if (decOwnerDocumentaryAddress != null
						&& !decOwnerDocumentaryAddress.E2_AddressOverride
						&& decOwner != null)
					{
						result = decOwner;
					}
					else
					{
						result = dec?.Supplier;
					}
				}
				return result;
			}
		}

		protected override ZBool IsInvoiceNumberToUpper => ZBool.False;

		#endregion

		#region PreviousDocuments

		[ChildEditable(true)]
		public EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection PreviousDocuments => fPreviousDocuments ?? (fPreviousDocuments = GetPreviousDocuments());
		EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection fPreviousDocuments;

		EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection(this);

		#endregion

		#region Type Decider

		public new static readonly EMCSJobComInvoiceHeaderTypeDecider TypeDecider = new EMCSJobComInvoiceHeaderTypeDecider();

		public class EMCSJobComInvoiceHeaderTypeDecider : Customs.Business.BaseJobComInvoiceHeaderTypeDecider
		{
			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return typeof(EMCSJobComInvoiceHeader);
			}

			public override Type GetTypeForBinding()
			{
				return typeof(EMCSJobComInvoiceHeader);
			}

			protected override Type GetTypeForNewCore(ITypeDeciderContext context)
			{
				return typeof(EMCSJobComInvoiceHeader);
			}

			protected override Type DefaultTypeForUnsupportedCountry
			{
				get { return typeof(EMCSJobComInvoiceHeader); }
			}
		}

		#endregion

		Type Customs.Business.IInvoiceLineTypeSupporter.InvoiceLineType => typeof(EMCSJobComInvoiceLine);

		protected override bool IsValidToDefaultIncoTermFromSupplier => false;
	}
}
