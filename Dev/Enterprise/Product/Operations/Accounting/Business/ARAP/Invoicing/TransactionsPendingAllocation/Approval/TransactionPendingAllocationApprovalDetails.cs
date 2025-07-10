using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class TransactionPendingAllocationApprovalDetails : ApprovalRequestDetails
	{
		#region Schema

		public new abstract class Schema : ApprovalRequestDetails.Schema
		{
			public const string DetailsXmlNode = "Details";
			public const string SourceXMLNode = "SourceXML";
			public const string SourceXML_XMLNode = "XML";
			public const string SourceXML_IsCrossLedgerImportAttribute = "IsCrossLedgerImport";

			public const string TransactionDate = "TransactionDate";
			public const string PostDate = "PostDate";
			public const string TransactionNumber = "TransactionNumber";
			public const string CreditorPK = "CreditorPK";
			public const string DueDate = "DueDate";
			public const string CurrencyCode = "CurrencyCode";
			public const string ExRate = "ExRate";
			public const string OSExTaxAmount = "OSExTaxAmount";
			public const string OSTaxAmount = "OSTaxAmount";
			public const string LocalExTaxAmount = "LocalExTaxAmount";
			public const string LocalTaxAmount = "LocalTaxAmount";
			public const string BranchPK = "BranchPK";
			public const string DepartmentPK = "DepartmentPK";
			public const string AddressPK = "AddressPK";
			public const string ContactPK = "ContactPK";
			public const string NumberOfSupportingDocuments = "NumberOfSupportingDocuments";
			public const string SourceXML = "SourceXML";
			public const string IsCrossLedgerImportFromXML = "IsCrossLedgerImportFromXML";
			public const string PlaceOfSupply = "PlaceOfSupply";
			public const string PlaceOfSupplyType = "PlaceOfSupplyType";
		}

		#endregion

		[Obsolete("For serializer only")]
		protected TransactionPendingAllocationApprovalDetails()
			: base(new BusinessObjectFactory())
		{
		}

		public TransactionPendingAllocationApprovalDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		[ResourceStringData("TransactionDate", Caption = "Transaction Date", ShortCaption = "Trans. Date")]
		public ZDateTime TransactionDate
		{
			get { return transactionDate; }
			set
			{
				SetNonPersistentPropertyValue(TransactionDateInfo, ref transactionDate, value);
			}
		}
		ZDateTime transactionDate;

		public ZPropertyInfo TransactionDateInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionDate); }
		}

		[ResourceStringData("PostDate", Caption = "Post Date")]
		public ZDateTime PostDate
		{
			get { return postDate; }
			set
			{
				SetNonPersistentPropertyValue(PostDateInfo, ref postDate, value);
			}
		}
		ZDateTime postDate;

		public ZPropertyInfo PostDateInfo
		{
			get { return GetZPropertyInfo(Schema.PostDate); }
		}

		[ResourceStringData("TransactionNumber", Caption = "Transaction Number")]
		public ZString TransactionNumber
		{
			get { return transactionNumber; }
			set
			{
				SetNonPersistentPropertyValue(TransactionNumberInfo, ref transactionNumber, value);
			}
		}
		ZString transactionNumber;

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionNumber); }
		}

		[List("OrgList")]
		[ResourceStringData("CreditorPK", Caption = "Creditor")]
		public ZGuid CreditorPK
		{
			get { return creditorPK; }
			set
			{
				SetNonPersistentPropertyValue(CreditorPKInfo, ref creditorPK, value);
			}
		}
		ZGuid creditorPK;

		public ZPropertyInfo CreditorPKInfo
		{
			get { return GetZPropertyInfo(Schema.CreditorPK); }
		}

		public OrgHeaderCollection OrgList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		[ResourceStringData("DueDate", Caption = "Due Date")]
		public ZDateTime DueDate
		{
			get { return dueDate; }
			set
			{
				SetNonPersistentPropertyValue(DueDateInfo, ref dueDate, value);
			}
		}
		ZDateTime dueDate;

		public ZPropertyInfo DueDateInfo
		{
			get { return GetZPropertyInfo(Schema.DueDate); }
		}

		[ResourceStringData("CurrencyCode", Caption = "Currency")]
		public ZString CurrencyCode
		{
			get { return currencyCode; }
			set
			{
				SetNonPersistentPropertyValue(CurrencyCodeInfo, ref currencyCode, value);
			}
		}
		ZString currencyCode;

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CurrencyCode); }
		}

		RefCurrency Currency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCode); }
		}

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		[ResourceStringData("ExRate", Caption = "Exchange Rate")]
		public ZDecimal ExRate
		{
			get { return exRate; }
			set
			{
				SetNonPersistentPropertyValue(ExRateInfo, ref exRate, value);
			}
		}
		ZDecimal exRate;

		public ZPropertyInfo ExRateInfo
		{
			get { return GetZPropertyInfo(Schema.ExRate); }
		}

		public int ExchangeRateDecimalPlaces => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		[ResourceStringData("OSExTaxAmount", Caption = "Amount Excl. Tax")]
		public ZDecimal OSExTaxAmount
		{
			get { return osExTaxAmount; }
			set
			{
				SetNonPersistentPropertyValue(OSExTaxAmountInfo, ref osExTaxAmount, value);
			}
		}
		ZDecimal osExTaxAmount;

		public ZPropertyInfo OSExTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.OSExTaxAmount); }
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		[ResourceStringData("OSTaxAmount", Caption = "Tax Amount")]
		public ZDecimal OSTaxAmount
		{
			get { return osTaxAmount; }
			set
			{
				SetNonPersistentPropertyValue(OSTaxAmountInfo, ref osTaxAmount, value);
			}
		}
		ZDecimal osTaxAmount;

		public ZPropertyInfo OSTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.OSTaxAmount); }
		}

		public int OSCurrencyDecimals => Currency?.Decimals ?? LocalCurrencyDecimals;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		[ResourceStringData("LocalExTaxAmount", Caption = "Local Amount Excl. Tax")]
		public ZDecimal LocalExTaxAmount
		{
			get { return localExTaxAmount; }
			set
			{
				SetNonPersistentPropertyValue(LocalExTaxAmountInfo, ref localExTaxAmount, value);
			}
		}
		ZDecimal localExTaxAmount;

		public ZPropertyInfo LocalExTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.LocalExTaxAmount); }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		[ResourceStringData("LocalTaxAmount", Caption = "Local Tax Amount")]
		public ZDecimal LocalTaxAmount
		{
			get { return localTaxAmount; }
			set
			{
				SetNonPersistentPropertyValue(LocalTaxAmountInfo, ref localTaxAmount, value);
			}
		}
		ZDecimal localTaxAmount;

		public ZPropertyInfo LocalTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.LocalTaxAmount); }
		}

		public int LocalCurrencyDecimals => Branch?.Company?.GetLocalDecimals() ?? GlbCompany.CurrentCompany.GetLocalDecimals();

		[List("BranchList")]
		[ResourceStringData("BranchPK", Caption = "Branch")]
		public ZGuid BranchPK
		{
			get { return branchPK; }
			set
			{
				SetNonPersistentPropertyValue(BranchPKInfo, ref branchPK, value);
			}
		}
		ZGuid branchPK;

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(Schema.BranchPK); }
		}

		public GlbBranchCollection BranchList
		{
			get { return new GlbBranchCollection(Factory); }
		}

		GlbBranch Branch
		{
			get { return Factory.Load<GlbBranch>(BranchPK); }
		}

		[List("DepartmentList")]
		[ResourceStringData("DepartmentPK", Caption = "Department")]
		public ZGuid DepartmentPK
		{
			get { return departmentPK; }
			set
			{
				SetNonPersistentPropertyValue(DepartmentPKInfo, ref departmentPK, value);
			}
		}
		ZGuid departmentPK;

		public ZPropertyInfo DepartmentPKInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentPK); }
		}

		public GlbDepartmentCollection DepartmentList
		{
			get { return new GlbDepartmentCollection(Factory); }
		}

		[List("AddressList")]
		[ResourceStringData("AddressPK", Caption = "Address")]
		public ZGuid AddressPK
		{
			get { return addressPK; }
			set
			{
				SetNonPersistentPropertyValue(AddressPKInfo, ref addressPK, value);
			}
		}
		ZGuid addressPK;

		public ZPropertyInfo AddressPKInfo
		{
			get { return GetZPropertyInfo(Schema.AddressPK); }
		}

		public OrgAddressCollection AddressList
		{
			get { return new OrgAddressCollection(Factory); }
		}

		[List("ContactList")]
		[ResourceStringData("ContactPK", Caption = "Contact")]
		public ZGuid ContactPK
		{
			get { return contactPK; }
			set
			{
				SetNonPersistentPropertyValue(ContactPKInfo, ref contactPK, value);
			}
		}
		ZGuid contactPK;

		public ZPropertyInfo ContactPKInfo
		{
			get { return GetZPropertyInfo(Schema.ContactPK); }
		}

		public OrgContactCollection ContactList
		{
			get { return new OrgContactCollection(Factory); }
		}

		[ResourceStringData("NumberOfSupportingDocuments", Caption = "No. of Attachments")]
		public ZByte NumberOfSupportingDocuments
		{
			get { return numberOfSupportingDocuments; }
			set
			{
				SetNonPersistentPropertyValue(NumberOfSupportingDocumentsInfo, ref numberOfSupportingDocuments, value);
			}
		}
		ZByte numberOfSupportingDocuments;

		public ZPropertyInfo NumberOfSupportingDocumentsInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfSupportingDocuments); }
		}

		#region UniversalTransaction

		public UniversalTransactionWrapper UniversalTransaction
		{
			get
			{
				if (universalTransaction == null)
				{
					universalTransaction = new UniversalTransactionWrapper(Factory);
					UpdateUniversalTransaction();
				}

				return universalTransaction;
			}
		}
		UniversalTransactionWrapper universalTransaction;

		void UpdateUniversalTransaction()
		{
			bool isUniversalTransactionAlreadyUsed = universalTransaction != null;
			if (isUniversalTransactionAlreadyUsed)
			{
				universalTransaction.Initialize(SourceXML, IsCrossLedgerImportFromXML);
			}
		}

		public ZString SourceXML
		{
			get { return sourceXML; }
			set
			{
				if (SetNonPersistentPropertyValue(SourceXMLInfo, ref sourceXML, value))
				{
					UpdateUniversalTransaction();
				}
			}
		}
		ZString sourceXML;

		public ZPropertyInfo SourceXMLInfo => GetZPropertyInfo(Schema.SourceXML);

		public ZBool IsCrossLedgerImportFromXML
		{
			get { return isCrossLedgerImportFromXML; }
			set
			{
				SetNonPersistentPropertyValue(IsCrossLedgerImportFromXMLInfo, ref isCrossLedgerImportFromXML, value);
			}
		}
		ZBool isCrossLedgerImportFromXML;

		public ZPropertyInfo IsCrossLedgerImportFromXMLInfo => GetZPropertyInfo(Schema.IsCrossLedgerImportFromXML);

		#endregion

		#region PlaceOfSupply
		[ResourceStringData("TransactionPendingAllocation|PlaceOfSupply", Caption = "Fixed Place of Supply")]
		public ZString PlaceOfSupply
		{
			get { return placeOfSupply; }
			set { SetNonPersistentPropertyValue(PlaceOfSupplyInfo, ref placeOfSupply, value); }
		}
		ZString placeOfSupply;

		public ZPropertyInfo PlaceOfSupplyInfo => GetZPropertyInfo(Schema.PlaceOfSupply);

		#endregion

		#region PlaceOfSupplyType

		public ZString PlaceOfSupplyType
		{
			get { return placeOfSupplyType; }
			set { SetNonPersistentPropertyValue(PlaceOfSupplyTypeInfo, ref placeOfSupplyType, value); }
		}
		ZString placeOfSupplyType;

		public ZPropertyInfo PlaceOfSupplyTypeInfo => GetZPropertyInfo(Schema.PlaceOfSupplyType);

		#endregion

		#endregion

		protected override void CopyFromCore(ApprovalRequestDetails approvalDetailsToCopy)
		{
			base.CopyFromCore(approvalDetailsToCopy);

			var approvalDetailsToCopyCasted = approvalDetailsToCopy as TransactionPendingAllocationApprovalDetails;
			if (approvalDetailsToCopyCasted != null)
			{
				TransactionDate = approvalDetailsToCopyCasted.TransactionDate;
				PostDate = approvalDetailsToCopyCasted.PostDate;
				TransactionNumber = approvalDetailsToCopyCasted.TransactionNumber;
				CreditorPK = approvalDetailsToCopyCasted.CreditorPK;
				DueDate = approvalDetailsToCopyCasted.DueDate;
				CurrencyCode = approvalDetailsToCopyCasted.CurrencyCode;
				ExRate = approvalDetailsToCopyCasted.ExRate;
				OSExTaxAmount = approvalDetailsToCopyCasted.OSExTaxAmount;
				OSTaxAmount = approvalDetailsToCopyCasted.OSTaxAmount;
				LocalExTaxAmount = approvalDetailsToCopyCasted.LocalExTaxAmount;
				LocalTaxAmount = approvalDetailsToCopyCasted.LocalTaxAmount;
				Description = approvalDetailsToCopyCasted.Description;
				BranchPK = approvalDetailsToCopyCasted.BranchPK;
				DepartmentPK = approvalDetailsToCopyCasted.DepartmentPK;
				AddressPK = approvalDetailsToCopyCasted.AddressPK;
				ContactPK = approvalDetailsToCopyCasted.ContactPK;
				NumberOfSupportingDocuments = approvalDetailsToCopyCasted.NumberOfSupportingDocuments;
				SourceXML = approvalDetailsToCopyCasted.SourceXML;
				IsCrossLedgerImportFromXML = approvalDetailsToCopyCasted.IsCrossLedgerImportFromXML;
				PlaceOfSupply = approvalDetailsToCopyCasted.PlaceOfSupply;
				PlaceOfSupplyType = approvalDetailsToCopyCasted.placeOfSupplyType;
			}
		}

		protected override bool IsEqual(ApprovalRequestDetails b)
		{
			bool isEqual = base.IsEqual(b);

			if (isEqual)
			{
				var b_Casted = b as TransactionPendingAllocationApprovalDetails;
				if (b_Casted != null)
				{
					isEqual &=
						TransactionDate == b_Casted.TransactionDate &&
						PostDate == b_Casted.PostDate &&
						TransactionNumber == b_Casted.TransactionNumber &&
						CreditorPK == b_Casted.CreditorPK &&
						DueDate == b_Casted.DueDate &&
						CurrencyCode == b_Casted.CurrencyCode &&
						ExRate == b_Casted.ExRate &&
						OSExTaxAmount == b_Casted.OSExTaxAmount &&
						OSTaxAmount == b_Casted.OSTaxAmount &&
						LocalExTaxAmount == b_Casted.LocalExTaxAmount &&
						LocalTaxAmount == b_Casted.LocalTaxAmount &&
						Description == b_Casted.Description &&
						BranchPK == b_Casted.BranchPK &&
						DepartmentPK == b_Casted.DepartmentPK &&
						AddressPK == b_Casted.AddressPK &&
						ContactPK == b_Casted.ContactPK &&
						NumberOfSupportingDocuments == b_Casted.NumberOfSupportingDocuments &&
						PlaceOfSupply == b_Casted.PlaceOfSupply &&
						PlaceOfSupplyType == b_Casted.placeOfSupplyType;
				}
			}

			return isEqual;
		}

		protected override bool IsPostingActionTheSameCore(ApprovalRequestDetails approvalDetails) => approvalDetails is TransactionPendingAllocationApprovalDetails; //only one posting action is allowed for this type

		#region Serialization

		protected override void ReadXmlCore(XmlReader reader)
		{
			base.ReadXmlCore(reader);

			var xElement = XNode.ReadFrom(reader) as XElement;

			SetValidValueHelper.SetDateIfValid(xElement.GetNodeValue(Schema.TransactionDate), x => TransactionDate = x);
			SetValidValueHelper.SetDateIfValid(xElement.GetNodeValue(Schema.PostDate), x => PostDate = x);
			SetValidValueHelper.SetStringIfValid(xElement.GetNodeValue(Schema.TransactionNumber), x => TransactionNumber = x);
			SetValidValueHelper.SetGuidIfValid(xElement.GetNodeValue(Schema.CreditorPK), x => CreditorPK = x);
			SetValidValueHelper.SetDateIfValid(xElement.GetNodeValue(Schema.DueDate), x => DueDate = x);
			SetValidValueHelper.SetStringIfValid(xElement.GetNodeValue(Schema.CurrencyCode), x => CurrencyCode = x);
			SetValidValueHelper.SetDecimalIfValid(xElement.GetNodeValue(Schema.ExRate), x => ExRate = x);
			SetValidValueHelper.SetDecimalIfValid(xElement.GetNodeValue(Schema.OSExTaxAmount), x => OSExTaxAmount = x);
			SetValidValueHelper.SetDecimalIfValid(xElement.GetNodeValue(Schema.OSTaxAmount), x => OSTaxAmount = x);
			SetValidValueHelper.SetDecimalIfValid(xElement.GetNodeValue(Schema.LocalExTaxAmount), x => LocalExTaxAmount = x);
			SetValidValueHelper.SetDecimalIfValid(xElement.GetNodeValue(Schema.LocalTaxAmount), x => LocalTaxAmount = x);
			SetValidValueHelper.SetStringIfValid(xElement.GetNodeValue(Schema.Description), x => Description = x);
			SetValidValueHelper.SetGuidIfValid(xElement.GetNodeValue(Schema.BranchPK), x => BranchPK = x);
			SetValidValueHelper.SetGuidIfValid(xElement.GetNodeValue(Schema.DepartmentPK), x => DepartmentPK = x);
			SetValidValueHelper.SetGuidIfValid(xElement.GetNodeValue(Schema.AddressPK), x => AddressPK = x);
			SetValidValueHelper.SetGuidIfValid(xElement.GetNodeValue(Schema.ContactPK), x => ContactPK = x);
			SetValidValueHelper.SetByteIfValid(xElement.GetNodeValue(Schema.NumberOfSupportingDocuments), x => NumberOfSupportingDocuments = x);

			var sourceXMLNode = xElement.Element(Schema.SourceXMLNode);
			SetValidValueHelper.SetStringIfValid(sourceXMLNode.GetNodeValue(Schema.SourceXML_XMLNode), x => SourceXML = x);
			SetValidValueHelper.SetBoolIfValid(sourceXMLNode.GetAttributeValue(Schema.SourceXML_IsCrossLedgerImportAttribute), x => IsCrossLedgerImportFromXML = x);
			SetValidValueHelper.SetStringIfValid(xElement.GetNodeValue(Schema.PlaceOfSupply), x => PlaceOfSupply = x);
			SetValidValueHelper.SetStringIfValid(xElement.GetNodeValue(Schema.PlaceOfSupplyType), x => PlaceOfSupplyType = x);
		}

		protected override void WriteXmlCore(XmlWriter writer)
		{
			base.WriteXmlCore(writer);

			var xElement = new XElement(Schema.DetailsXmlNode,
				new XElement(Schema.TransactionDate, TransactionDate.ToISO8601String()),
				new XElement(Schema.PostDate, PostDate.ToISO8601String()),
				new XElement(Schema.TransactionNumber, TransactionNumber.ToString()),
				new XElement(Schema.CreditorPK, CreditorPK.ToString()),
				new XElement(Schema.DueDate, DueDate.ToISO8601String()),
				new XElement(Schema.CurrencyCode, CurrencyCode.ToString()),
				new XElement(Schema.ExRate, ExRate.ToString()),
				new XElement(Schema.OSExTaxAmount, OSExTaxAmount.ToString()),
				new XElement(Schema.OSTaxAmount, OSTaxAmount.ToString()),
				new XElement(Schema.LocalExTaxAmount, LocalExTaxAmount.ToString()),
				new XElement(Schema.LocalTaxAmount, LocalTaxAmount.ToString()),
				new XElement(Schema.Description, Description.ToString()),
				new XElement(Schema.BranchPK, BranchPK.ToString()),
				new XElement(Schema.DepartmentPK, DepartmentPK.ToString()),
				new XElement(Schema.AddressPK, AddressPK.ToString()),
				new XElement(Schema.ContactPK, ContactPK.ToString()),
				new XElement(Schema.NumberOfSupportingDocuments, NumberOfSupportingDocuments.ToString()),
				SourceXML.IsEmpty ? null : new XElement(Schema.SourceXMLNode,
					new XAttribute(Schema.SourceXML_IsCrossLedgerImportAttribute, IsCrossLedgerImportFromXML.ToString()),
					new XElement(Schema.SourceXML_XMLNode, SourceXML.ToString())),
				PlaceOfSupply.IsEmpty ? null : new XElement(Schema.PlaceOfSupply, PlaceOfSupply.ToString()),
				PlaceOfSupplyType.IsEmpty ? null : new XElement(Schema.PlaceOfSupplyType, PlaceOfSupplyType.ToString())
			);

			xElement.WriteTo(writer);
		}

		#endregion
	}
}
