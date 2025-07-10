using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	[SystemDefinedValues]
	public class AsycudaBillScreening : ASYCUDA.Business.AsycudaBillScreening,
		IDocAddresses,
		Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public AsycudaBillScreening(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		public new AsycudaBillScreeningValidation Validation => (AsycudaBillScreeningValidation)base.Validation;
		protected override ManifestBase.AsycudaBillScreeningValidation GetNewValidation() => new AsycudaBillScreeningValidation(this);
		public new AsycudaBillScreeningLookups Lookups => (AsycudaBillScreeningLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillScreeningLookups GetNewLookups() => new AsycudaBillScreeningLookups(this);

		[List(nameof(Lookups) + "." + nameof(AsycudaBillScreeningLookups.ScreeningResultList))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBillScreening.ASR_Result", Caption = "Result")]
		public override ZString ASR_Result
		{
			get => base.ASR_Result;
			set => base.ASR_Result = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillScreeningLookups.ScreeningAuthorizedPersonTypes))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBillScreening.ASR_AuthorizedPersonType", Caption = "Authorized Person Type", ShortCaption = "Person Type")]
		public override ZString ASR_AuthorizedPersonType
		{
			get => base.ASR_AuthorizedPersonType;
			set
			{
				base.ASR_AuthorizedPersonType = value;

				if (value == EUICS2ScreeningAuthorizedPersonTypes.Codes.AP3)
				{
					ASR_PER_AuthorizedPerson = Guid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizedPersonFieldsRequirement();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBillScreening.ASR_PER_AuthorizedPerson", Caption = "Authorized Person", ShortCaption = "Person")]
		[ReadOnlyMember(nameof(ASR_PER_AuthorizedPerson_ReadOnly))]
		public override ZGuid ASR_PER_AuthorizedPerson
		{
			get => base.ASR_PER_AuthorizedPerson;
			set
			{
				base.ASR_PER_AuthorizedPerson = value;
				PopulateAuthorizedPersonNameAndIdentifier(ASR_PER_AuthorizedPerson);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizedPersonFieldsRequirement();
				}
			}
		}

		void PopulateAuthorizedPersonNameAndIdentifier(ZGuid authorizedPerson)
		{
			if (authorizedPerson.IsValid)
			{
				var glbPerson = Factory.Load<GlbPerson>(authorizedPerson);
				var contact = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.OC_PER, authorizedPerson));

				if (glbPerson != null)
				{
					var fullName = glbPerson.PER_FullName;
					ASR_AuthorizedPersonName = fullName.SubstringSafe(0, AsycudaBillScreeningSchema.ASR_AuthorizedPersonName.MaxLength);
				}
				else
				{
					ASR_AuthorizedPersonName = string.Empty;
				}

				if (contact != null)
				{
					var certificateNumber = contact.Certificates.GetFirstCertificateNumber(StaffDefaultCertificateIDAndTrainingTypes.PAS);
					ASR_AuthorizedPersonIdentifier = certificateNumber.SubstringSafe(0, AsycudaBillScreeningSchema.ASR_AuthorizedPersonIdentifier.MaxLength);
				}
				else
				{
					ASR_AuthorizedPersonIdentifier = string.Empty;
				}
			}
			else
			{
				ASR_AuthorizedPersonName = string.Empty;
				ASR_AuthorizedPersonIdentifier = string.Empty;
			}
		}

		bool ASR_PER_AuthorizedPerson_ReadOnly => ASR_AuthorizedPersonType == EUICS2ScreeningAuthorizedPersonTypes.Codes.AP3;

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBillScreening.ASR_AuthorizedPersonName", Caption = "Name")]
		[ReadOnlyMember(nameof(ASR_AuthorizedPersonName_ReadOnly))]
		public override ZString ASR_AuthorizedPersonName
		{
			get => base.ASR_AuthorizedPersonName;
			set => base.ASR_AuthorizedPersonName = value;
		}

		bool ASR_AuthorizedPersonName_ReadOnly => !ASR_PER_AuthorizedPerson.IsEmpty;

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBillScreening.ASR_AuthorizedPersonIdentifier", Caption = "Identifier")]
		public override ZString ASR_AuthorizedPersonIdentifier
		{
			get => base.ASR_AuthorizedPersonIdentifier;
			set => base.ASR_AuthorizedPersonIdentifier = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillScreeningLookups.TransportDocumentTypeList))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBillScreening.TransportDocumentType", Caption = "Transport Document (House) Type", ShortCaption = "Document Type")]
		public override ZString ASR_TransportNumberType
		{
			get => base.ASR_TransportNumberType;
			set => base.ASR_TransportNumberType = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBillScreening.TransportDocumentReferenceNumber", Caption = "Transport Document (House) Reference Number", ShortCaption = "Reference Number")]
		public override ZString ASR_TransportNumber
		{
			get => base.ASR_TransportNumber;
			set => base.ASR_TransportNumber = value;
		}

		#region FacilityPlace

		public ICS2JobDocAddress FacilityPlace
		{
			get
			{
				if (facilityPlace == null || facilityPlace.IsDeleted)
				{
					facilityPlace = DocAddresses.FindOrCreateWithRequirement(FacilityPlaceDocAddressRequirement);
				}
				return facilityPlace;
			}
		}

		ICS2JobDocAddress facilityPlace;

		[ChildEditable(true)]
		public ICS2JobDocAddressCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new ICS2JobDocAddressCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}
		ICS2JobDocAddressCollection docAddresses;

		public JobDocAddressRequirement FacilityPlaceDocAddressRequirement
		{
			get
			{
				if (facilityPlaceDocAddressRequirement == null)
				{
					facilityPlaceDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ICS2FacilityPlace);
				}

				return facilityPlaceDocAddressRequirement;
			}
		}
		JobDocAddressRequirement facilityPlaceDocAddressRequirement;

		public IReadOnlyList<DocAddressType> SupportedAddressTypes => new DocAddressType[] { DocAddressType.ICS2FacilityPlace };

		#endregion

		#region IDocAddresses

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		public Security.SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType) => null;

		public void DocAddressChanged(JobDocAddress docAddress) { }

		public void OrgAddressBeforeChange(JobDocAddress docAddress) { }

		public void OnBeforeDocAddressDeleted(JobDocAddress docAddress) { }

		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress) { }

		public void OrgHeaderAfterChange(JobDocAddress docAddress) { }

		public bool CanDeleteAddress(JobDocAddress docAddress) => false;

		public OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType) => null;

		JobDocAddressDependentCollection IDocAddresses.DocAddresses => DocAddresses;

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region ICusSupportingInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(HRCMAdditionalInfo) }
			};
		}

		#endregion

		#region Additional Infos

		[ChildEditable(true)]
		public HRCMAdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = new HRCMAdditionalInfoCollection(this);
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}

				return additionalInfos;
			}
		}

		HRCMAdditionalInfoCollection additionalInfos;

		#endregion
	}
}
