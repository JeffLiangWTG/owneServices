using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UniversalTransactionWrapper : NonPersistentBusinessObject
	{
		public UniversalTransactionWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void Initialize(ZString xml, bool isCrossLedgerImport)
		{
			IsCrossLedger = isCrossLedgerImport;

			var importer = GetTransactionImporter();
			var universalTransactionAndMapper = importer.ImportUniversalTransactionFromXml(xml, Factory, isCrossLedgerImport);
			sourceUniversalTransaction = universalTransactionAndMapper?.Item1 as UniversalTransaction;
			codeMapperForUniversalTransaction = universalTransactionAndMapper?.Item2;

			Lines.RemoveAndDeleteAll();
			ClearJobData();
			ClearConsolData();

			if (sourceUniversalTransaction != null && sourceUniversalTransaction.PostingJournalCollection != null)
			{
				sourceUniversalTransaction.PostingJournalCollection.ForEach(universalTransactionLine => Lines.Add(new UniversalTransactionLineWrapper(universalTransactionLine, sourceUniversalTransaction, this)));
			}

			RefreshBinding();

			if (IsCrossLedger)
			{
				creditorOnTimeOfInitialization = GetCreditorFromDataObject();
			}
		}

		internal IOrgPatternMatchOverride UpdateCreditorCodeMapping(OrgHeader transactionOrganization)
		{
			if (transactionOrganization != null && !CreditorSource.IsEmpty)
			{
				if (IsCrossLedger)
				{
					var codeMapper = ObjectFactory.New<IUniversalCodeMapper>("", new DummyLogger(), Factory);
					return codeMapper.CreateOrUpdateCodeMapping(Constants.OrgPatternMatchOverrideRelationships.Organisation, CreditorSource, transactionOrganization.OH_Code, transactionOrganization.PK);
				}

				return codeMapperForUniversalTransaction.CreateOrUpdateCodeMapping<OrganizationAddress, ZCodeMappedZString?>(sourceUniversalTransaction, Factory, x => x.OrganizationCode, CreditorSource, transactionOrganization.OH_Code, transactionOrganization.PK);
			}

			return null;
		}

		internal void UpdateChargeCodeMapping(AccChargeCode transactionLineChargeCode, ZString foreignChargeCode)
		{
			if (transactionLineChargeCode != null && !foreignChargeCode.IsEmpty)
			{
				codeMapperForUniversalTransaction.CreateOrUpdateCodeMapping<ChargeCode, ZCodeMappedZString?>(sourceUniversalTransaction, Factory, x => x.Code, foreignChargeCode, transactionLineChargeCode.AC_Code, transactionLineChargeCode.PK);
			}
		}

		internal bool SetTransactionOrganisationAndUpdateMatchingForCrossLedgerOnly(OrgHeader transactionOrganization)
		{
			bool isMatchingUpdated = false;
			if (IsCrossLedger && transactionOrganization != null)
			{
				if (tempOrgMappingRule != null)
				{
					DeleteTempOrgMappingRule();
					isMatchingUpdated = true;
				}
				if (transactionOrganization.OH_Code != Creditor)
				{
					tempOrgMappingRule = UpdateCreditorCodeMapping(transactionOrganization);
					isMatchingUpdated |= tempOrgMappingRule != null;
				}
			}
			if (isMatchingUpdated)
			{
				codeMapperForUniversalTransaction.UpdateMappedCodes(sourceUniversalTransaction, Factory);
				ResetIsMappedCodesUpdated();
			}

			return isMatchingUpdated;
		}

		internal void DeleteTempOrgMappingRule()
		{
			if (tempOrgMappingRule != null)
			{
				var rule = (BusinessObject)tempOrgMappingRule;
				if (rule.IsInDatabase)
				{
					rule.Reload();
				}
				else
				{
					rule.Delete();
				}

				tempOrgMappingRule = null;
			}
		}

		IOrgPatternMatchOverride tempOrgMappingRule;

		internal void UpdateMappedCodes()
		{
			if (!isMappedCodesUpdated)
			{
				var importer = GetTransactionImporter();
				importer.TrySetMappedValueDirectlyFromSourceCode(sourceUniversalTransaction);
				isMappedCodesUpdated = true;
			}
		}
		bool isMappedCodesUpdated;

		internal Dictionary<ConsolKey, IJobCostingPlugIn> AssociatedConsols => associatedConsols ?? (associatedConsols = new Dictionary<ConsolKey, IJobCostingPlugIn>());
		Dictionary<ConsolKey, IJobCostingPlugIn> associatedConsols;

		internal Dictionary<ZString, Job> AssociatedJobs => associatedJobs ?? (associatedJobs = new Dictionary<ZString, Job>());
		Dictionary<ZString, Job> associatedJobs;

		void ResetIsMappedCodesUpdated()
		{
			isMappedCodesUpdated = false;
			foreach (UniversalTransactionLineWrapper line in Lines)
			{
				line.ResetIsMappedCodesUpdated();
			}
		}

		public OrgHeader CreditorOrgHeader
		{
			get
			{
				if (creditorOrgHeader == null)
				{
					var importer = GetTransactionImporter();
					var universalFactory = new UniversalObjectFactory();
					var orgHeaderInUniversalFactory = (OrgHeader)importer.GetOrganization(sourceUniversalTransaction, universalFactory, new DummyLogger(), IsCrossLedger);
					if (orgHeaderInUniversalFactory != null)
					{
						creditorOrgHeader = Factory.Load<OrgHeader>(orgHeaderInUniversalFactory.PK);
					}
				}

				return creditorOrgHeader;
			}
		}
		OrgHeader creditorOrgHeader;

		public ZString Creditor => creditorOnTimeOfInitialization ?? GetCreditorFromDataObject();

		ZString GetCreditorFromDataObject() => sourceUniversalTransaction.GetValueSafe(x => x.OrganizationAddress).GetValueSafe(x => x.OrganizationCode).GetValueOrDefault().MappedValue.GetValueOrDefault();

		ZString? creditorOnTimeOfInitialization;

		[ResourceStringData("CreditorSource", Caption = "Creditor")]
		public ZString CreditorSource => sourceUniversalTransaction.GetValueSafe(x => x.OrganizationAddress).GetValueSafe(x => x.OrganizationCode).GetValueOrDefault().SourceValue;

		[ResourceStringData("CreditorFullName", Caption = "Creditor Full Name")]
		public ZString CreditorFullName => sourceUniversalTransaction.GetValueSafe(x => x.OrganizationAddress).GetValueSafe(x => x.CompanyName).GetValueOrDefault();

		[ResourceStringData("Address", Caption = "Address")]
		public ZString Address
		{
			get
			{
				ZString result = ZString.Empty;
				var orgAddress = sourceUniversalTransaction.GetValueSafe(x => x.OrganizationAddress);
				if (orgAddress != null)
				{
					var addressShortCode = orgAddress.AddressShortCode.GetValueOrDefault();
					result = orgAddress == null ? "" : string.Format("{0} {1} {2} {3}",
						string.IsNullOrWhiteSpace(addressShortCode) ? orgAddress.Address1.GetValueOrDefault() : orgAddress.AddressShortCode.Value,
						orgAddress.City.GetValueOrDefault(),
						((ZString?)orgAddress.State).GetValueOrDefault(),
						orgAddress.Port.GetValueSafe(x => x.Code).GetValueOrDefault()).Trim();
				}

				return result;
			}
		}

		[ResourceStringData("Contact", Caption = "Contact")]
		public ZString Contact => sourceUniversalTransaction.GetValueSafe(x => x.OrganizationAddress).GetValueSafe(x => x.Contact).GetValueOrDefault();

		[ResourceStringData("TransactionDate", Caption = "Transaction Date", ShortCaption = "Trans. Date")]
		public ZDateTime TransactionDate => sourceUniversalTransaction.GetValueSafe(x => x.TransactionDate).GetValueOrDefault();

		[ResourceStringData("PostDate", Caption = "Post Date")]
		public ZDateTime PostDate => sourceUniversalTransaction.GetValueSafe(x => x.PostDate).GetValueOrDefault();

		[ResourceStringData("TransactionNumber", Caption = "Transaction Number")]
		public ZString TransactionNumber => sourceUniversalTransaction.GetValueSafe(x => x.Number).GetValueOrDefault();

		[ResourceStringData("NumberOfSupportingDocuments", Caption = "No. of Attachments")]
		public ZInt NumberOfSupportingDocuments => sourceUniversalTransaction.GetValueSafe(x => x.NumberOfSupportingDocuments).GetValueOrDefault();

		[ResourceStringData("DueDate", Caption = "Due Date")]
		public ZDateTime DueDate => sourceUniversalTransaction.GetValueSafe(x => x.DueDate).GetValueOrDefault();

		[ResourceStringData("OSCurrency", Caption = "Currency")]
		public ZString OSCurrency => sourceUniversalTransaction.GetValueSafe(x => x.OSCurrency).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("LocalCurrency", Caption = "Local Currency", ShortCaption = "L. Currency")]
		public ZString LocalCurrency => sourceUniversalTransaction.GetValueSafe(x => x.LocalCurrency).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("OSExGSTVATAmount", Caption = "Amount")]
		public ZDecimal OSExGSTVATAmount => sourceUniversalTransaction.GetValueSafe(x => x.OSExGSTVATAmount).GetValueOrDefault();

		[ResourceStringData("OSGSTVATAmount", Caption = "Tax")]
		public ZDecimal OSGSTVATAmount => sourceUniversalTransaction.GetValueSafe(x => x.OSGSTVATAmount).GetValueOrDefault();

		[ResourceStringData("OSTotal", Caption = "Total")]
		public ZDecimal OSTotal => sourceUniversalTransaction.GetValueSafe(x => x.OSTotal).GetValueOrDefault();

		[ResourceStringData("OSWHTAmount", Caption = "WHT Amount")]
		public ZDecimal OSWHTAmount => sourceUniversalTransaction.GetValueSafe(x => x.OSWHTAmount).GetValueOrDefault();

		[ResourceStringData("LocalExVATAmount", Caption = "Local Amount", ShortCaption = "L. Amount")]
		public ZDecimal LocalExVATAmount => sourceUniversalTransaction.GetValueSafe(x => x.LocalExVATAmount).GetValueOrDefault();

		[ResourceStringData("LocalVATAmount", Caption = "Local Tax", ShortCaption = "L. Tax")]
		public ZDecimal LocalVATAmount => sourceUniversalTransaction.GetValueSafe(x => x.LocalVATAmount).GetValueOrDefault();

		[ResourceStringData("LocalTotal", Caption = "Local Total", ShortCaption = "L. Total")]
		public ZDecimal LocalTotal => sourceUniversalTransaction.GetValueSafe(x => x.LocalTotal).GetValueOrDefault();

		[ResourceStringData("LocalWHTAmount", Caption = "Local WHT Amount", ShortCaption = "L. WHT")]
		public ZDecimal LocalWHTAmount => sourceUniversalTransaction.GetValueSafe(x => x.LocalWHTAmount).GetValueOrDefault();

		[ResourceStringData("Description", Caption = "Description")]
		public ZString Description => sourceUniversalTransaction.GetValueSafe(x => x.Description).GetValueOrDefault();

		[ResourceStringData("Branch", Caption = "Branch")]
		public ZString Branch => sourceUniversalTransaction.GetValueSafe(x => x.Branch).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("BranchName", Caption = "Branch Name")]
		public ZString BranchName => sourceUniversalTransaction.GetValueSafe(x => x.Branch).GetValueSafe(x => x.Name).GetValueOrDefault();

		[ResourceStringData("Department", Caption = "Department")]
		public ZString Department => sourceUniversalTransaction.GetValueSafe(x => x.Department).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("DepartmentName", Caption = "Department Name")]
		public ZString DepartmentName => sourceUniversalTransaction.GetValueSafe(x => x.Department).GetValueSafe(x => x.Name).GetValueOrDefault();

		[ResourceStringData("PlaceOfSupply", Caption = "Fixed Place Of Supply", ShortCaption = "FPOS")]
		public ZString PlaceOfSupply => sourceUniversalTransaction.GetValueSafe(x => x.PlaceOfSupply).GetValueSafe(x => x.Location.Code).GetValueOrDefault();

		[ResourceStringData("PlaceOfSupplyType", Caption = "Fixed Place Of Supply Type", ShortCaption = "FPOS Type")]
		public ZString PlaceOfSupplyType => sourceUniversalTransaction.GetValueSafe(x => x.PlaceOfSupply).GetValueSafe(x => x.LocationType.Code).GetValueOrDefault();

		public UniversalTransactionLineWrapperCollection Lines => lines ?? (lines = new UniversalTransactionLineWrapperCollection());
		UniversalTransactionLineWrapperCollection lines;

		#region XMLData

		#region JobXMLData

		internal JobXMLData JobData => jobData ?? (jobData = new JobXMLData());
		JobXMLData jobData;

		void ClearJobData()
		{
			jobData = null;
		}

#if DEBUG
		public string[] GetJobDataFieldNameList_ForTestOnly() => JobData.GetXMLDataFieldNameList_ForTestOnly();
#endif

		internal class JobXMLData
		{
			internal void AddXMLData(ZString number, Func<UniversalShipment> getUniversalShipment)
			{
				Argument.NotNull(getUniversalShipment, nameof(getUniversalShipment));

				if (!number.IsEmpty && !XMLDataPerJobNumber.ContainsKey(number))
				{
					var shipment = getUniversalShipment();
					XMLDataPerJobNumber.Add(number, shipment == null ? ZString.Empty : GenerateXMLDataFormatted(GetXMLDataMapping(shipment)));
				}
			}

			internal ZString GetXMLDataFormatted(ZString number)
			{
				ZString result;
				XMLDataPerJobNumber.TryGetValue(number, out result);

				return result;
			}

			ZString GenerateXMLDataFormatted(List<UniversalShipmentFieldMapping> fieldMapping)
			{
				Argument.NotNull(fieldMapping, nameof(fieldMapping));

				return ZString.Join(System.Environment.NewLine, fieldMapping.Where(x => !x.Value.IsEmpty).Select(x => ZString.Format("{0}: {1}", x.Name, x.Value)).ToArray());
			}

			Dictionary<ZString, ZString> XMLDataPerJobNumber => xmlDataPerJobNumber ?? (xmlDataPerJobNumber = new Dictionary<ZString, ZString>());
			Dictionary<ZString, ZString> xmlDataPerJobNumber;

			protected virtual List<UniversalShipmentFieldMapping> GetXMLDataMapping(UniversalShipment universalShipment)
			{
				Argument.NotNull(universalShipment, nameof(universalShipment));

				var mapping = new List<UniversalShipmentFieldMapping>();

				mapping.Add(new UniversalShipmentFieldMapping(universalShipment, ResString.GetMultilingualString("FBD9F586-51FF-479D-952D-9D3C4615A781", "Job Target #"),
					GetJobTarget));

				mapping.Add(new UniversalShipmentFieldMapping(universalShipment, ResString.GetMultilingualString("B5246A07-C7D0-417F-8711-150D09943A0E", "House Bill #"),
					x => x.WayBillNumber.GetValueOrDefault()));

				mapping.Add(new UniversalShipmentFieldMapping(universalShipment, ResString.GetMultilingualString("AC2C161C-CA77-431F-B300-866088CFDC5A", "Flight/Voyage # and Vessel"),
					x => NullableExtensions.JoinExcludingEmpty(" / ", new[] { x.VoyageFlightNo.GetValueOrDefault(), x.VesselName.GetValueOrDefault() })));

				mapping.Add(new UniversalShipmentFieldMapping(universalShipment, ResString.GetMultilingualString("86F15AAC-C6E2-4F8C-8867-418D4EEB7BF1", "Customs Entry #"),
					x => NullableExtensions.JoinExcludingEmpty(", ", GetCustomsEntryNumbers(x))));

				mapping.Add(new UniversalShipmentFieldMapping(universalShipment, ResString.GetMultilingualString("FA756A15-25B6-4996-928A-35BD40DAF099", "Order #"),
					GetOrderNumbers));

				mapping.Add(new UniversalShipmentFieldMapping(universalShipment, ResString.GetMultilingualString("DCE27463-32EA-4C9A-8BF9-9F97ADE4A960", "Transport Booking Reference"),
					GetTransportBookingReference));

				mapping.Add(new UniversalShipmentFieldMapping(universalShipment, ResString.GetMultilingualString("669084C5-A65A-43C1-BAD5-827547C6C4FB", "Container #"),
					x => NullableExtensions.JoinExcludingEmpty(", ", GetShipmentContainerNumbers(x))));

				return mapping;
			}

			protected ZString GetJobTarget(UniversalShipment universalShipment) => universalShipment.DataContext.GetValueSafe(context => context.DataTargetCollection).GetValueSafe(targetCollection => targetCollection.Any() ? targetCollection.First().Key : null).GetValueOrDefault();

			ZString[] GetCustomsEntryNumbers(UniversalShipment universalShipment)
			{
				var numbers = universalShipment.EntryNumberCollection.GetValueSafe(collection => collection.Select(entryNumber => entryNumber.Number.GetValueOrDefault())).GetValueSafe(x => x.ToArray());
				if (numbers == null || !numbers.Any())
				{
					var collectionOfNumberCollections = universalShipment.EntryHeaderCollection.GetValueSafe(headerCollection => headerCollection.Select(entryHeader => entryHeader.EntryNumberCollection.GetValueSafe(numberCollection => numberCollection.Select(entryNumber => entryNumber.Number.GetValueOrDefault()))));
					if (collectionOfNumberCollections != null)
					{
						numbers = collectionOfNumberCollections.Where(x => x != null).SelectMany(x => x).Distinct().ToArray();
					}
				}

				return numbers ?? Array.Empty<ZString>();
			}

			ZString GetOrderNumbers(UniversalShipment universalShipment) => universalShipment.LocalProcessing.GetValueSafe(local => NullableExtensions.JoinExcludingEmpty(", ",
				local.OrderNumberCollection.GetValueSafe(collection => collection.Select(orderNumber => orderNumber.OrderReference.GetValueOrDefault()))));

			ZString GetTransportBookingReference(UniversalShipment universalShipment) => universalShipment.LocalProcessing.GetValueSafe(local => local.ArrivalCartageRef.GetValueOrDefault());

			ZString[] GetShipmentContainerNumbers(UniversalShipment universalShipment)
			{
				var numbers = universalShipment.ContainerCollection.GetValueSafe(collection => collection.Select(container => container.ContainerNumber.GetValueOrDefault())).GetValueSafe(x => x.ToArray());
				if (numbers == null || !numbers.Any())
				{
					numbers = universalShipment.PackingLineCollection.GetValueSafe(collection => collection.Select(packingLine => packingLine.ContainerNumber.GetValueOrDefault()).Distinct()).GetValueSafe(x => x.ToArray());
				}

				return numbers ?? Array.Empty<ZString>();
			}

#if DEBUG
			public string[] GetXMLDataFieldNameList_ForTestOnly() => GetXMLDataMapping(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)).Select(x => x.Name.ToString()).ToArray();
#endif
		}

		#endregion

		#region ConsolXMLData

		internal ConsolXMLData ConsolData => consolData ?? (consolData = new ConsolXMLData());
		ConsolXMLData consolData;

		void ClearConsolData()
		{
			consolData = null;
		}

#if DEBUG
		public string[] GetConsolDataFieldNameList_ForTestOnly() => ConsolData.GetXMLDataFieldNameList_ForTestOnly();
#endif
		internal class ConsolXMLData : JobXMLData
		{
#pragma warning disable RECS0133 // Parameter name differs in base declaration
			protected override List<UniversalShipmentFieldMapping> GetXMLDataMapping(UniversalShipment universalConsol)
#pragma warning restore RECS0133 // Parameter name differs in base declaration
			{
				Argument.NotNull(universalConsol, nameof(universalConsol));

				var mapping = new List<UniversalShipmentFieldMapping>();

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("75BF6CBC-EBA5-484C-ADAB-E6BD142BF9DB", "Consol Target #"),
					GetJobTarget));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("FAFA05FD-943C-436E-9584-C8E72DE884A8", "Booking Reference #"),
					x => !IsCoLoadMaster(x) ? x.BookingConfirmationReference.GetValueOrDefault() : ZString.Empty));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("0043413F-8049-4CD8-BD11-C0613133E720", "Container #"),
					x => NullableExtensions.JoinExcludingEmpty(", ", x.ContainerCollection.GetValueSafe(collection => collection.Select(container => container.ContainerNumber.GetValueOrDefault())))));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("C13F68A1-8887-4AC4-821B-8BE31492ADA8", "Co-Load Master Bill #"),
					x => IsCoLoadMaster(x) ? x.BookingConfirmationReference.GetValueOrDefault() : ZString.Empty));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("D9D95401-D4D6-48B6-A3C7-8CFD45BB23F7", "Master Bill"),
					x => x.WayBillNumber.GetValueOrDefault()));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("7BD7CB7E-6395-4179-AD2A-71085FAE6F5D", "Flight/Voyage # and Vessel"),
					x => NullableExtensions.JoinExcludingEmpty(" / ", new[] { x.VoyageFlightNo.GetValueOrDefault(), x.VesselName.GetValueOrDefault() })));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("47F8EBF7-6586-466F-BF48-1248A777DDC8", "ETA"),
					x => NullableExtensions.JoinExcludingEmpty(", ", GetETADatesAsLongTimeStringStrings(x))));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("DF71FBC3-DFB4-4008-B284-5E262F81B88F", "ATA"),
					x => NullableExtensions.JoinExcludingEmpty(", ", GetATADatesAsLongTimeStringStrings(x))));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("F0D764D6-6926-4D55-8C89-8C040B0393A7", "ETD"),
					x => NullableExtensions.JoinExcludingEmpty(", ", GetETDDatesAsLongTimeStringStrings(x))));

				mapping.Add(new UniversalShipmentFieldMapping(universalConsol, ResString.GetMultilingualString("D8AD16D1-E0C9-4857-B67E-226069D0A68F", "ATD"),
					x => NullableExtensions.JoinExcludingEmpty(", ", GetATDDatesAsLongTimeStringStrings(x))));

				return mapping;
			}

			bool IsCoLoadMaster(UniversalShipment universalConsol) => universalConsol.ShipmentType.GetValueSafe(type => type.Code).GetValueOrDefault() == Constants.ShipmentTypes.CoLoadMaster;

			IEnumerable<string> GetETADatesAsLongTimeStringStrings(UniversalShipment universalConsol) => universalConsol.TransportLegCollection.GetValueSafe(collection => collection.Select(transportLeg => transportLeg.EstimatedArrival.GetValueOrDefault().ToLongTimeString()));

			IEnumerable<string> GetATADatesAsLongTimeStringStrings(UniversalShipment universalConsol) => universalConsol.TransportLegCollection.GetValueSafe(collection => collection.Select(transportLeg => transportLeg.ActualArrival.GetValueOrDefault().ToLongTimeString()));

			IEnumerable<string> GetETDDatesAsLongTimeStringStrings(UniversalShipment universalConsol) => universalConsol.TransportLegCollection.GetValueSafe(collection => collection.Select(transportLeg => transportLeg.EstimatedDeparture.GetValueOrDefault().ToLongTimeString()));

			IEnumerable<string> GetATDDatesAsLongTimeStringStrings(UniversalShipment universalConsol) => universalConsol.TransportLegCollection.GetValueSafe(collection => collection.Select(transportLeg => transportLeg.ActualDeparture.GetValueOrDefault().ToLongTimeString()));
		}

		#endregion

		internal class UniversalShipmentFieldMapping
		{
			public UniversalShipmentFieldMapping(UniversalShipment shipment, MultilingualString name, Func<UniversalShipment, ZString> getValue)
			{
				Argument.NotNull(shipment, nameof(shipment));
				Argument.NotNull(name, nameof(name));
				Argument.NotNull(getValue, nameof(getValue));

				Name = name;
				Value = getValue(shipment);
			}

			public readonly MultilingualString Name;
			public readonly ZString Value;
		}

		#endregion

		internal static ITransactionImporter GetTransactionImporter() => (ITransactionImporter)Activator.CreateInstance(ObjectFactory.GetType("ITransactionImporter"));

		UniversalTransaction sourceUniversalTransaction;
		ICodeMappingManager codeMapperForUniversalTransaction;
		internal bool IsCrossLedger { get; private set; }

		internal class ConsolKey
		{
			internal ConsolKey(ZString consolNumber, ZString? consolType)
			{
				ConsolNumber = consolNumber;
				ConsolType = consolType;
			}

			ZString ConsolNumber { get; }
			ZString? ConsolType { get; }

			public override bool Equals(object obj)
			{
				var key = obj as ConsolKey;
				return key != null && key.ConsolNumber == ConsolNumber && key.ConsolType == ConsolType;
			}

			public override int GetHashCode() => ConsolNumber.GetHashCode() ^ ConsolType.GetHashCode();
		}
	}
}
