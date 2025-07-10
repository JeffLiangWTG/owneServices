using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.ClusterKey;
using static Enterprise.Integration.Customs.AU;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsDirection : AutoQuarantineColsDirection,
		IQuarantineColsDirection,
		IClusterKeyWorker
	{
		public QuarantineColsDirection(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(QuarantineColsDirection.ColsHeader))]
		public override ZGuid QCD_QCH_ColsHeader
		{
			get { return base.QCD_QCH_ColsHeader; }
			set { base.QCD_QCH_ColsHeader = value; }
		}

		public QuarantineColsHeader ColsHeader => Factory.Load<QuarantineColsHeader>(QCD_QCH_ColsHeader);
		QuarantineColsDirection firstDirection => ColsHeader.Directions.Cast<QuarantineColsDirection>().FirstOrDefault();

		public JobDocAddress AAAddress
		{
			get
			{
				if (fAAAddress == null || fAAAddress.IsDeleted)
				{
					fAAAddress = JobDocAddress.GetOrCreateDocAddressFromParent(this, DocAddressType.COLSDirectionAAAddress);
					fAAAddress.MakePersistentEvenIfEmpty();
					RegisterEditableChildObject(fAAAddress);
					RegisterListChangedCalledRefreshBinding(fAAAddress);
					fAAAddress.OverrideRequirement = new AAAddressRequirement(this);

					if (!fAAAddress.IsInDatabase)
					{
						using (fAAAddress.SuspendSettingHasChanges())
						{
							fAAAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
							fAAAddress.E2_GovRegNumType = GovRegNumTypeList.Codes.AAN;
							fAAAddress.E2_AddressOverride = true;

							var aanCode = ColsHeader?.QCH_ApprovedArrangementRefNum ?? ZString.Empty;
							var matchedAddress = GetAddressWithMatchedAANCode(aanCode);
							fAAAddress.E2_GovRegNum = aanCode;
							fAAAddress.E2_CompanyName = matchedAddress?.CompanyName ?? ZString.Empty;
							fAAAddress.E2_Address1 = matchedAddress?.Address1 ?? ZString.Empty;
						}
					}
				}
				return fAAAddress;
			}
		}
		JobDocAddress fAAAddress;

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineColsDirection|AAId", Caption = "(AA) ID")]
		[List(nameof(Lookups) + "." + nameof(QuarantineColsDirectionLookups.AAIDList))]
		public ZString AAId
		{
			get => AAAddress.E2_GovRegNum;
			set
			{
				if (AAAddress.E2_GovRegNum != value)
				{
					var matchedAddress = GetAddressWithMatchedAANCode(value);

					AAName = matchedAddress?.CompanyName ?? ZString.Empty;
					AALocation = matchedAddress?.Address1 ?? ZString.Empty;
				}

				AAAddress.E2_GovRegNum = value;
			}
		}

		public ZWrappedPropertyInfo AAIdInfo => GetWrappedZPropertyInfo(nameof(QuarantineColsDirection.AAId), x => AAAddress.E2_GovRegNumInfo);

		public OrgAddress GetAddressWithMatchedAANCode(string aanCode)
		{
			OrgAddress bestAddress = null;
			if (!AddressesWithAANCode.TryGetValue(aanCode, out bestAddress))
			{
				var addresses = ColsHeader?.DeliveryOrUnpack?.Organisation?.Addresses?.Cast<OrgAddress>().Where(x => x.CustomsCodes.GetCustomsRegNo(AustraliaCodeTypes.ApprovedArrangementNumber, Core.Constants.CountryCodes.Australia) == aanCode).ToArray() ?? Array.Empty<OrgAddress>();
				bestAddress = ColsHeader?.GetBestDeliveryOrUnpackAddress(addresses, false);
				if (bestAddress == null)
				{
					bestAddress = addresses.FirstOrDefault();
				}

				AddressesWithAANCode.Add(aanCode, bestAddress);
			}
			return bestAddress;
		}

		Dictionary<ZString, OrgAddress> AddressesWithAANCode => Factory.GetCachedValue($"QuarantineColsDirection|AddressesWithAANCode|{ColsHeader?.DeliveryOrUnpack?.OrganisationPK ?? ZGuid.Empty}", () => new Dictionary<ZString, OrgAddress>());

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineColsDirection|AAName", Caption = "(AA) Name")]
		public ZString AAName
		{
			get => AAAddress.E2_CompanyName;
			set => AAAddress.E2_CompanyName = value;
		}

		public ZWrappedPropertyInfo AANameInfo => GetWrappedZPropertyInfo(nameof(QuarantineColsDirection.AAName), x => AAAddress.E2_CompanyNameInfo);

		public const int AANameMaxLength = 100;

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineColsDirection|AALocation", Caption = "(AA) Location")]
		public ZString AALocation
		{
			get => AAAddress.E2_Address1;
			set => AAAddress.E2_Address1 = value;
		}

		public ZWrappedPropertyInfo AALocationInfo => GetWrappedZPropertyInfo(nameof(QuarantineColsDirection.AALocation), x => AAAddress.E2_Address1Info);

		public const int AALocationMaxLength = 20;

		#region Container

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineColsDirection|QCD_CO_Container", Caption = "Container")]
		[RelatedBusinessObject(nameof(QuarantineColsDirection.Container))]
		[List(nameof(Lookups) + "." + nameof(QuarantineColsDirectionLookups.ContainersList))]
		[ReadOnlyMember(nameof(QCD_CO_Container_ReadOnly))]
		public override ZGuid QCD_CO_Container
		{
			get { return base.QCD_CO_Container; }
			set { base.QCD_CO_Container = value; }
		}

		public CusContainer Container => Factory.Load<CusContainer>(QCD_CO_Container);

		public ZBool QCD_CO_Container_ReadOnly => (firstDirection?.QCD_CL_CusEntryLine.IsEmpty == false) || !QCD_CL_CusEntryLine.IsEmpty;

		#endregion

		#region Entry Line

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineColsDirection|QCD_CL_CusEntryLine", Caption = "Entry Line")]
		[RelatedBusinessObject(nameof(QuarantineColsDirection.CusEntryLine))]
		[List(nameof(Lookups) + "." + nameof(QuarantineColsDirectionLookups.EntryLinesList))]
		[ReadOnlyMember(nameof(QCD_CL_CusEntryLine_ReadOnly))]
		public override ZGuid QCD_CL_CusEntryLine
		{
			get { return base.QCD_CL_CusEntryLine; }
			set { base.QCD_CL_CusEntryLine = value; }
		}

		public CusEntryLine CusEntryLine => Factory.Load<CusEntryLine>(QCD_CL_CusEntryLine);

		public ZBool QCD_CL_CusEntryLine_ReadOnly => (firstDirection?.QCD_CO_Container.IsEmpty == false) || !QCD_CO_Container.IsEmpty;

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineColsDirection|QCD_Direction", Caption = "Direction")]
		[List(nameof(Lookups) + "." + nameof(QuarantineColsDirectionLookups.DirectionsList))]
		public override ZString QCD_Direction { get => base.QCD_Direction; set => base.QCD_Direction = value; }

		[ResourceStringData("Enterprise.Customs.AU.Business.EXDOC.QuarantineColsDirection|QCD_TreatmentType", Caption = "Treatment Type")]
		[List(nameof(Lookups) + "." + nameof(QuarantineColsDirectionLookups.TreatmentTypeList))]
		public override ZString QCD_TreatmentType { get => base.QCD_TreatmentType; set => base.QCD_TreatmentType = value; }

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)QCD_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(QuarantineColsHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)QCD_QCH_ColsHeaderInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion

		public override void Delete()
		{
			DeleteAAAddress();
			base.Delete();
		}

		void DeleteAAAddress()
		{
			if (fAAAddress == null)
			{
				var address = JobDocAddress.Load(this, DocAddressType.COLSDirectionAAAddress, !IsInDatabase);
				address?.Delete();
			}
			else if (!fAAAddress.IsDeleted)
			{
				fAAAddress.Delete();
			}
		}
	}
}
