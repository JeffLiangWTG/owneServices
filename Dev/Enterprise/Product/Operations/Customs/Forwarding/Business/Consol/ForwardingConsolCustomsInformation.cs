using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Forwarding.Business
{
	public sealed class ForwardingConsolCustomsInformation : NonPersistentBusinessObject
	{
		public ForwardingConsolCustomsInformation(ForwardingConsol consol)
			: base(consol.Factory)
		{
			Argument.NotNull(consol, "consol");
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		public static class Schema
		{
			public const string CustomsCargoStatus = "CustomsCargoStatus";
			public const string AMSBillStatus = "AMSBillStatus";
			public const string AMSBillStatusDescription = "AMSBillStatusDescription";
			public const string LatestAMSDispositionCode = "LatestAMSDispositionCode";
			public const string LatestAMSDispositionDesc = "LatestAMSDispositionDesc";
			public const string AFRBillStatus = "AFRBillStatus";
			public const string AFRBillStatusDescription = "AFRBillStatusDescription";
			public const string OutwardReportStatus = "OutwardReportStatus";
			public const string OutwardReportStatusDescription = "OutwardReportStatusDescription";
			public const string OutwardReportEntryNumber = "OutwardReportEntryNumber";
		}

		#region CustomsStatus

		ForwardingConsolCustomsStatusProvider CustomsStatusProvider => customsStatusProvider ?? (customsStatusProvider = ForwardingConsolCustomsStatusProvider.New(consol));
		ForwardingConsolCustomsStatusProvider customsStatusProvider;

		public ZString CustomsCargoStatus => CustomsStatusProvider.CustomsCargoStatus();

		#endregion

		#region AMS Bill Status

		public ZString AMSBillStatus
		{
			get
			{
				if (amsBillStatus == null)
				{
					amsBillStatus = new CachedProperty<ZString>(Factory,
						() => AMSStatusHelper.GetAMSBillStatus(consol));
				}
				return amsBillStatus.Value;
			}
		}
		CachedProperty<ZString> amsBillStatus;

		public ZPropertyInfo AMSBillStatusInfo => GetZPropertyInfo(Schema.AMSBillStatus);

		public ZString AMSBillStatusDescription => Factory.GetCachedValue<AMSConsolBillCustomsStatusList>().GetDescriptionFromCode(AMSBillStatus);

		public ZPropertyInfo AMSBillStatusDescriptionInfo => GetZPropertyInfo(Schema.AMSBillStatusDescription);

		US.USAMS.IAMSStatusHelper AMSStatusHelper => Factory.GetCachedValue("AMSStatusHelper", ObjectFactory.Get<US.USAMS.IAMSStatusHelper>);

		#endregion

		#region Latest AMS Disposition

		LatestAMSDispositionProvider LatestAMSDispositionProvider
		{
			get { return latestAMSDispositionProvider ?? (latestAMSDispositionProvider = new LatestAMSDispositionProvider(consol)); }
		}
		LatestAMSDispositionProvider latestAMSDispositionProvider;

		public ZString LatestAMSDispositionCode => LatestAMSDispositionProvider.LatestAMSDispositionCode;

		public ZPropertyInfo LatestAMSDispositionCodeInfo => GetZPropertyInfo(Schema.LatestAMSDispositionCode);

		public ZString LatestAMSDispositionDesc => LatestAMSDispositionProvider.LatestAMSDispositionDesc;

		public ZPropertyInfo LatestAMSDispositionDescInfo => GetZPropertyInfo(Schema.LatestAMSDispositionDesc);

		#endregion

		#region AFR Bill Status

		JP.AFR.IAFRStatusHelper AFRStatusHelper => afrStatusHelper ?? (afrStatusHelper = ObjectFactory.Get<JP.AFR.IAFRStatusHelper>());
		JP.AFR.IAFRStatusHelper afrStatusHelper;

		public ZString AFRBillStatus
		{
			get
			{
				if (afrBillStatus == null)
				{
					afrBillStatus = new CachedProperty<ZString>(Factory, () => AFRStatusHelper.GetAFRBillStatus(consol));
				}
				return afrBillStatus.Value;
			}
		}
		CachedProperty<ZString> afrBillStatus;

		public ZPropertyInfo AFRBillStatusInfo => GetZPropertyInfo(Schema.AFRBillStatus);

		public ZString AFRBillStatusDescription
		{
			get
			{
				if (afrBillStatusDescription == null)
				{
					afrBillStatusDescription = new CachedProperty<ZString>(Factory, () => AFRStatusHelper.GetAFRBillStatusDescription(Factory, AFRBillStatus));
				}
				return afrBillStatusDescription.Value;
			}
		}
		CachedProperty<ZString> afrBillStatusDescription;

		public ZPropertyInfo AFRBillStatusDescriptionInfo => GetZPropertyInfo(Schema.AFRBillStatusDescription);

		#endregion

		#region NZ Specific, OutwardReport Status

		public ZString OutwardReportEntryNumber => OutwardReportNumber != null ? OutwardReportNumber.CE_EntryNum : ZString.Empty;

		[BusinessObjectTestExclude]
		public ZString OutwardReportStatus
		{
			get
			{
				ZString result = OutwardReportStatusList.Descriptions.NotSent;
				var outwardReportNumberCached = OutwardReportNumber;
				if (outwardReportNumberCached != null)
				{
					result = outwardReportNumberCached.CE_EntryStatus.IsEmpty ? OutwardReportStatusList.Codes.Acknowledgement : outwardReportNumberCached.CE_EntryStatus.ToString();
				}
				return result;
			}
		}

		public ZPropertyInfo OutwardReportStatusInfo => GetZPropertyInfo(Schema.OutwardReportStatus);

		public ZString OutwardReportStatusDescription => OutwardReportNumber != null ? ORNStatusList.GetDescriptionFromCode(OutwardReportStatus) : string.Empty;

		public ZPropertyInfo OutwardReportStatusDescriptionInfo => GetZPropertyInfo(Schema.OutwardReportStatusDescription);

		public OutwardReportStatusList ORNStatusList => fORNStatus ?? (fORNStatus = new OutwardReportStatusList());
		OutwardReportStatusList fORNStatus;

		CusEntryNumber OutwardReportNumber
		{
			get
			{
				if (outwardReportNumber == null)
				{
					var result = (CusEntryNumber[])Factory.Load(typeof(CusEntryNumber), GetEntryNumberFilter(consol));
					if (result.Length == 0)
					{
						return null;
					}

					if (result.Length > 1)
					{
						ErrorReporter.ReportOnce("EntryNumber for outward report has more than one record", "EntryNumber for outward cargo report has more than one record");
					}

					outwardReportNumber = result[0];
				}

				return outwardReportNumber;
			}
		}
		CusEntryNumber outwardReportNumber;

		ZQuery GetEntryNumberFilter(CommonConsol consol)
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, consol.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypeList.Codes.OutwardReportNumber);
			return filter;
		}

		#endregion

		#region ASYCUDA

		public ZString AsycudaRegistrationStatus => string.Join("; ", consol.GetGlobalManifestHeaders().Select(m => m.RegistrationStatus).Where(rs => !string.IsNullOrEmpty(rs)));

		#endregion
	}
}
