using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawback : CusSupportingInfo, Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public new class Schema : CusSupportingInfo.Schema
		{
			public const string CSI_IsSupplierBeneficiary = "CSI_IsSupplierBeneficiary";
		}

		public SuspensionDrawback(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(14)]
		[ReadOnlyMember(nameof(Beneficiary_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_ReferenceNumber", Caption = "CNPJ Beneficiary")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_ReferenceNumber2", Caption = "CA Number")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_LineNo", Caption = "CA line item number")]
		[MaxLength(3)]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_Quantity", Caption = "Quantity used")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(SuspensionDrawbackLookups.TypeSuspensionDrawbackList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_SubType", Caption = "Type of Concession Act (CA)")]
		public override ZString CSI_SubType
		{
			get { return base.CSI_SubType; }
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (oldValue != value && !IsCopying)
				{
					CSI_IsSupplierBeneficiary = !Beneficiary_ReadOnly;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_Tariff", Caption = "Tariff of the CA import item")]
		public override ZString CSI_Tariff { get => base.CSI_Tariff; set => base.CSI_Tariff = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_Value", Caption = "Foreign exchange hedged VMLE")]
		[DecimalPlaces(2)]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(Beneficiary_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SuspensionDrawback|CSI_IsSupplierBeneficiary", Caption = "Is Main Supplier the Beneficiary?")]
		public ZBool CSI_IsSupplierBeneficiary
		{
			get
			{
				var result = false;
				if (Parent?.Supplier_Effective != null)
				{
					var cjnSupplier = Parent.Supplier_Effective.GetCNPJOrCPF();
					var referenceNumber = CSI_ReferenceNumber.KeepChars(ZString.NumericCharacters).SubstringSafe(0, 14);

					if (cjnSupplier == referenceNumber)
					{
						result = true;
					}
				}
				return result;
			}
			set
			{
				var oldValue = CSI_IsSupplierBeneficiary;

				if (oldValue != value)
				{
					CSI_ReferenceNumber = ZString.Empty;
					if (value)
					{
						if (Parent?.Supplier_Effective != null)
						{
							SetPropertyValue(CSI_ReferenceNumberInfo, Parent.Supplier_Effective.GetCNPJOrCPF());
							CSI_ReferenceNumberInfo.RefreshBinding();
						}
					}
				}
				CSI_IsSupplierBeneficiaryInfo.RefreshBinding(value);
			}
		}

		public ZPropertyInfo CSI_IsSupplierBeneficiaryInfo => GetZPropertyInfo(Schema.CSI_IsSupplierBeneficiary);

		public bool Beneficiary_ReadOnly => CSI_SubType == TypeSuspensionDrawbackList.Codes.Intermediate || CSI_SubType == TypeSuspensionDrawbackList.Codes.GenericIntermediary;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.SuspensionDrawback;
		}

		public new SuspensionDrawbackLookups Lookups => (SuspensionDrawbackLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result;
			if (Parent?.IsExport ?? ZBool.False)
			{
				result = new SuspensionDrawbackValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new SuspensionDrawbackLookups(this);

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		#region SuspensionDrawbackInvoice

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public SuspensionDrawbackInvoiceCollection SuspensionDrawbackInvoiceCollection
		{
			get
			{
				if (fSuspensionDrawbackInvoiceCollection == null)
				{
					fSuspensionDrawbackInvoiceCollection = new SuspensionDrawbackInvoiceCollection(this);
					fSuspensionDrawbackInvoiceCollection.Load();
					RegisterEditableChildObject(fSuspensionDrawbackInvoiceCollection);
				}
				return fSuspensionDrawbackInvoiceCollection;
			}
		}

		SuspensionDrawbackInvoiceCollection fSuspensionDrawbackInvoiceCollection;

		#endregion

		#region SuspensionDrawbackImportEntryDocument

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public SuspensionDrawbackImportEntryDocumentCollection SuspensionDrawbackImportEntryDocumentCollection
		{
			get
			{
				if (fSuspensionDrawbackImportEntryDocumentCollection == null)
				{
					fSuspensionDrawbackImportEntryDocumentCollection = new SuspensionDrawbackImportEntryDocumentCollection(this);
					fSuspensionDrawbackImportEntryDocumentCollection.Load();
					RegisterEditableChildObject(fSuspensionDrawbackImportEntryDocumentCollection);
				}
				return fSuspensionDrawbackImportEntryDocumentCollection;
			}
		}

		SuspensionDrawbackImportEntryDocumentCollection fSuspensionDrawbackImportEntryDocumentCollection;

		#endregion

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusSupportingInfoTypeList.Codes.SuspensionDrawbackInvoice, typeof(SuspensionDrawbackInvoice) },
				{ CusSupportingInfoTypeList.Codes.SuspensionDrawbackImportEntryDocument, typeof(SuspensionDrawbackImportEntryDocument) },
			};
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
	}
}
