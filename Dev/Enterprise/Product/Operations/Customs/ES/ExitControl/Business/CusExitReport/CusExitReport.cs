using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitReport : EU.ExitControl.Business.CusExitReport
			, Integration.Customs.ESExitControl.ICusExitReport
			, IESMessageInfoProvider
			, IESResponseBusinessObject
			, IESResponseBOMessageStatus
			, IPollingTransactionParent
	{
		public CusExitReport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public class ESSchema : AutoCusExitReport.Schema
		{
			public const string ClearanceDate = nameof(CusExitReport.ClearanceDate);
			public const string Circuit = nameof(CusExitReport.Circuit);
			public const string ArrivalDate = nameof(CusExitReport.ArrivalDate);
		}

		public new CusExitReportLookups Lookups => (CusExitReportLookups)base.Lookups;

		protected override ExitControlBase.Business.CusExitReportLookups GetNewLookups() => new CusExitReportLookups(this);

		public new CusExitReportValidation Validation => (CusExitReportValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitReportValidation GetNewValidation() => new CusExitReportValidation(this);

		protected override IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		public new ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems => (ICusExitReportItemCollection<CusExitReportItem>)base.CusExitReportItems;

		protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection(ZQuery filter) => new CusExitReportItemCollection<CusExitReportItem>(this, filter);

		protected override ICusExitReportItemCollection<EU.ExitControl.Business.CusExitReportItem> CreateNewCusExitReportItemPackageCollection() => new CusExitReportItemCollection<CusExitReportItem>(this, new ZQuery(CusExitReportItemSchema.ERI_CXP_Package, SQLComparisonOperator.NotEqual, ZGuid.Empty));

		protected override bool SupportSetDefaultCER_TransportType => false;

		[MaxLength(17)]
		[List(nameof(Lookups) + "." + nameof(CusExitReportLookups.Locations))]
		public override ZString CER_Location { get => base.CER_Location; set => base.CER_Location = value; }

		[ResourceStringData("52112548-6BCA-4196-BCC8-4324667C1C15", Caption = "Circuit", ShortCaption = "Cir.")]
		[ReadOnly(true)]
		public ZString Circuit { get => GetCircuitCodeAndDescription; }

		[ResourceStringData("4678CE4B-F26E-4286-A699-D07EA5F32F53", Caption = "Clearance Date", ShortCaption = "Cl. Date")]
		[ReadOnly(true)]
		public ZDateTime ClearanceDate { get => ClearanceEntryNumber.CE_IssueDate; }

		[ResourceStringData("BE4566FD-F542-4E9E-9360-21BA5023B1C0", Caption = "Arrival Date", ShortCaption = "Arr. Date")]
		[ReadOnly(true)]
		public override ZDateTimeOffset CER_DateTime { get => base.CER_DateTime; set => base.CER_DateTime = value; }

		[ResourceStringData("BE4566FD-F542-4E9E-9360-21BA5023B1C0", Caption = "Arrival Date", ShortCaption = "Arr. Date")]
		[ReadOnly(true)]
		public ZDateTime ArrivalDate { get => CER_DateTime.ToZDateTime(); }

		protected override ZString StatusDescriptionCore
		{
			get
			{
				var result = string.Empty;
				var status = CER_Status;
				if (!status.IsEmpty)
				{
					result = Lookups.StatusList.GetDescriptionFromCode(status);
				}
				return result ?? base.StatusDescriptionCore;
			}
		}

		public CusEntryNumber ClearanceEntryNumber
		{
			get
			{
				if (clrEntryNumber == null || clrEntryNumber.IsDeleted)
				{
					clrEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Spain.ClearanceCSV, CountryCode);
					RegisterEditableChildObject(clrEntryNumber);
				}

				return clrEntryNumber;
			}
		}

		CusEntryNumber clrEntryNumber;

		public ZString ClearanceReferenceNumber => ClearanceEntryNumber.CE_EntryNum;

		ZString GetCircuitCodeAndDescription
		{
			get
			{
				var result = new ZString();
				switch (ClearanceEntryNumber.CE_EntryStatus)
				{
					case CircuitCodeList.Codes.GREEN:
						result = MessageFunctionCodeList.Codes.GreenCircuitText + " - " + CircuitCodeList.Descriptions.GREEN;
						break;
					case CircuitCodeList.Codes.RED:
						result = MessageFunctionCodeList.Codes.RedCircuitText + " - " + CircuitCodeList.Descriptions.RED;
						break;
					case CircuitCodeList.Codes.ORANGE:
						result = MessageFunctionCodeList.Codes.OrangeCircuitText + " - " + CircuitCodeList.Descriptions.ORANGE;
						break;
				}
				return result;
			}
		}

		GlbStaff IESMessageInfoProvider.Broker => Header?.CustomsAgent;

		ZString IESMessageInfoProvider.MRN => Consignment?.CXC_MovementReference ?? ZString.Empty;
		ZString IESMessageInfoProvider.DocumentJobReference => Consignment?.CXC_MovementReference ?? ZString.Empty;

		ZString IESMessageBusinessObject.EntryReference => Consignment?.CXC_MovementReference ?? ZString.Empty;

		ZGuid IESResponseBusinessObject.BranchPK => Header?.Branch.PK ?? ZGuid.Empty;

		EDIMessageCollection IESResponseBusinessObject.MessageCollection => Messages;

		ZString IPollingTransactionParent.CertificateName => Header?.CXH_CustomsProfile ?? ZString.Empty;

		ZBool IPollingTransactionParent.IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& (!registration.IsWiseTechGlobalInternalSystem()
								|| (bool)Header.TrainingEntry);
			}
		}

		GlbStaff IPollingTransactionParent.Broker => Header?.CustomsAgent;

		OrgHeader IPollingTransactionParent.Declarant => Header?.Carrier?.Header;

		#region ICusSupportingInfoTypeSupporter

		public IDictionary<ZInt, ZInt> AdditionalInfosItemNumberDictionary => Factory.GetValue(ref additionalInfosItemNumberDictionaryCached, () => AdditionalInfos.Cast<AdditionalInfo>().Select(x => x.CSI_ItemNumber).Where(x => x > ZInt.Zero).GroupBy(x => x).ToDictionary(x => x.Key, y => (ZInt)y.Count()));
		CachedProperty<IDictionary<ZInt, ZInt>> additionalInfosItemNumberDictionaryCached;

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		#endregion

		public new CusExitConsignment Consignment => (CusExitConsignment)base.Consignment;
		public new CusExitHeader Header => (CusExitHeader)base.Header;

		ZString IESResponseBOMessageStatus.MessageStatus { set => CER_MessageStatus = value; }

		public ZBool CanLaunchExitReportUrl() => CER_MessageStatus == LogicalStatusList.Codes.Accepted;

		public string GetUrlToLaunch() => !IsDeleted ? new UrlDecider(this).GetUrlExitReport() : ZString.Empty;

		protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;
	}
}
