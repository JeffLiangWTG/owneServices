using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobLocalClientActionMethodApplicator : UpdateJobActionMethodApplicatorBase
	{
		public UpdateJobLocalClientActionMethodApplicator(BusinessObjectFactory factory) : base(factory, "UpdateJobLocalClientActionMethodApplicator")
		{
		}

		#region Schema

		public static class Schema
		{
			public const string LocalChargesAddr = "LocalChargesAddr";
			public const string LocalBillingContact = "LocalBillingContact";
		}

		#endregion

		protected override string[] UpdateJobProperty(JobHeader job)
		{
			var errors = Array.Empty<string>();

			job.JH_OA_LocalChargesAddr = LocalChargesAddr;
			if (job.JH_OA_LocalChargesAddrInfo.HasErrors())
			{
				errors = job.JH_OA_LocalChargesAddrInfo.GetErrors().Select(x => x.Message).ToArray();
			}

			job.JH_OC_LocalBillingContact = LocalBillingContact;
			if (job.JH_OC_LocalBillingContactInfo.HasErrors())
			{
				job.JH_OA_LocalChargesAddrInfo.GetErrors().Select(x => x.Message).ToArray().CopyTo(errors, 0);
			}

			return errors;
		}

		protected override bool GetPropertyReadonly(JobHeader job)
		{
			return job.JH_OA_LocalChargesAddrInfo.ReadOnly || job.JH_OC_LocalBillingContactInfo.ReadOnly;
		}

		protected override bool GetValueSameWithPrevious(JobHeader job)
		{
			return job.JH_OA_LocalChargesAddr == LocalChargesAddr && job.JH_OC_LocalBillingContact == LocalBillingContact;
		}

		[List("AddressList")]
		public ZGuid LocalChargesAddr
		{
			get
			{
				return fLocalChargesAddr;
			}
			set
			{
				SetNonPersistentPropertyValue(LocalChargesAddrInfo, ref fLocalChargesAddr, value);
				if (!IsValidationSuspended)
				{
					ValidateLocalChargesAddr();
				}
			}
		}
		ZGuid fLocalChargesAddr;

		public ZPropertyInfo LocalChargesAddrInfo => GetZPropertyInfo(Schema.LocalChargesAddr);

		public OrganisationsFindBoxCollection AddressList
		{
			get
			{
				if (fLocalClientList == null)
				{
					fLocalClientList = Factory.GetCachedValue(
						"UpdateJobLocalClient_" + FindboxLookupCollections.CachingKey, () =>
						{
							var collection = new OrganisationsFindBoxCollection(Factory);
							collection.FilterBusinessObjectDefaults.Add(
								new FilterBusinessObjectDefault("Organisation Types", "Property0",
									ZBool.True)); // It is not clear whether or how it should be replaced by a Resource String
							return collection;
						});

					fLocalClientList.OrganisationType = OrganisationTypes.Debtor;
				}

				return fLocalClientList;
			}
		}
		OrganisationsFindBoxCollection fLocalClientList;

		public ZGuid LocalBillingContact
		{
			get
			{
				return fLocalBillingContact;
			}
			set
			{
				SetNonPersistentPropertyValue(LocalBillingContactInfo, ref fLocalBillingContact, value);
				if (!IsValidationSuspended)
				{
					ValidateLocalBillingContact();
				}
			}
		}
		ZGuid fLocalBillingContact;

		public ZPropertyInfo LocalBillingContactInfo => GetZPropertyInfo(Schema.LocalBillingContact);

		public ZAddressWithContact LocalZAddressWithContact
		{
			get
			{
				if (fLocalZAddressWithContact == null)
				{
					fLocalZAddressWithContact = GetLocalZAddressWithContact();
				}

				return fLocalZAddressWithContact;
			}
		}
		ZAddressWithContact fLocalZAddressWithContact;

		ZAddressWithContact GetLocalZAddressWithContact()
		{
			var result = new ZAddressWithContact(LocalBillingContactInfo, LocalChargesAddrInfo);
			result.DefaultAddressType = AddressType.ARM;
			result.GetDefaultAddress = JobHeader.GetDefaultAddressByHeaderInCommonLanguage;
			return result;
		}

		#region Validation

		protected override void ValidateAll()
		{
			ValidateLocalChargesAddr();
			ValidateLocalBillingContact();
		}

		void ValidateLocalChargesAddr()
		{
			LocalChargesAddrInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(LocalChargesAddrInfo);
			LocalChargesAddrInfo.RunAdditionalValidation();
		}

		void ValidateLocalBillingContact()
		{
			LocalBillingContactInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(LocalBillingContactInfo);
		}

		#endregion
	}
}
