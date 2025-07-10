using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class ApprovalRequestDetailsWithCharges<DetailsType> : ApprovalRequestDetails
		where DetailsType : ApprovalRequestChargeDetails
	{
		#region Schema

		public new abstract class Schema : ApprovalRequestDetails.Schema
		{
			public const string PostingOption = "PostingOption";
		}

		#endregion

		[Obsolete("For serializer only")]
		protected ApprovalRequestDetailsWithCharges()
			: base(new BusinessObjectFactory())
		{
		}

		public ApprovalRequestDetailsWithCharges(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region PostingOption

		[ResourceStringData("ApprovalRequestDetailsWithCharges|PostingOption", Caption = "Posting Option")]
		public ZString PostingOption
		{
			get { return postingOption; }
			set
			{
				SetNonPersistentPropertyValue(PostingOptionInfo, ref postingOption, value);
			}
		}
		ZString postingOption;

		ZPropertyInfo PostingOptionInfo
		{
			get { return GetZPropertyInfo(Schema.PostingOption); }
		}

		#endregion

		public NonPersistentBusinessObjectCollection<DetailsType> Charges
		{
			get
			{
				if (charges == null)
				{
					charges = CreateChargesCollection();
					RegisterEditableChildObject(charges);
				}

				return charges;
			}
		}
		NonPersistentBusinessObjectCollection<DetailsType> charges;

		public NonPersistentBusinessObjectCollection<DetailsType> FilteredCharges
		{
			get
			{
				if (filteredCharges == null && Charges != null)
				{
					filteredCharges = CreateChargesCollection();
					foreach (DetailsType charge in Charges)
					{
						if (IsAllowedtoViewTransactionOutsideLoginPermission(charge))
						{
							filteredCharges.Add(charge);
						}
					}
				}

				return filteredCharges;
			}
		}
		NonPersistentBusinessObjectCollection<DetailsType> filteredCharges;

		protected abstract NonPersistentBusinessObjectCollection<DetailsType> CreateChargesCollection();

		protected abstract bool IsAllowedtoViewTransactionOutsideLoginPermission(DetailsType chargeApprovalRequest);

		#region Overrides

		protected override void CopyFromCore(ApprovalRequestDetails postingRequestToCopy)
		{
			base.CopyFromCore(postingRequestToCopy);

			var postingRequestToCopyCasted = postingRequestToCopy as ApprovalRequestDetailsWithCharges<DetailsType>;
			if (postingRequestToCopyCasted != null)
			{
				CopyInstanceSpecificFieldsFrom(postingRequestToCopyCasted);
				Charges.RemoveAndDeleteAll();
				foreach (DetailsType chargeToCopy in postingRequestToCopyCasted.Charges)
				{
					var newCharge = Charges.AddNew();
					IDisposable suspender = null;
					if (IsSettingHasChangesSuspended)
					{
						suspender = newCharge.SuspendSettingHasChanges();
					}
					try
					{
						newCharge.CopyFrom(chargeToCopy);
					}
					finally
					{
						if (suspender != null)
						{
							suspender.Dispose();
						}
					}
				}
			}
		}

		protected virtual void CopyInstanceSpecificFieldsFrom(ApprovalRequestDetailsWithCharges<DetailsType> postingRequestToCopy)
		{
			if (postingRequestToCopy != null)
			{
				PostingOption = postingRequestToCopy.PostingOption;
			}
		}

		protected override bool IsEqual(ApprovalRequestDetails b)
		{
			bool isEqual = base.IsEqual(b);

			if (isEqual)
			{
				var b_Casted = b as ApprovalRequestDetailsWithCharges<DetailsType>;
				if (b_Casted != null)
				{
					isEqual = AreInstanceSpecificFieldsEqual(b_Casted);
					if (isEqual)
					{
						bool allChargesEqual = Charges.Count == b_Casted.Charges.Count;
						if (allChargesEqual)
						{
							List<ApprovalRequestChargeDetails> listA = new List<ApprovalRequestChargeDetails>(Charges.ToArray<ApprovalRequestChargeDetails>());
							List<ApprovalRequestChargeDetails> listB = new List<ApprovalRequestChargeDetails>(b_Casted.Charges.ToArray<ApprovalRequestChargeDetails>());

							for (int indexA = 0; indexA < listA.Count; indexA++)
							{
								ApprovalRequestChargeDetails chargeA = listA[indexA];
								int indexB;
								for (indexB = 0; indexB < listB.Count; indexB++)
								{
									ApprovalRequestChargeDetails chargeB = listB[indexB];
									if (chargeA == chargeB)
									{
										listA.RemoveAt(indexA--);
										listB.RemoveAt(indexB--);
										break;
									}
								}
								if (indexB == listB.Count)
								{
									break;
								}
							}
							allChargesEqual = listA.Count == 0 && listB.Count == 0;
						}
						isEqual &= allChargesEqual;
					}
				}
			}

			return isEqual;
		}

		protected virtual bool AreInstanceSpecificFieldsEqual(ApprovalRequestDetailsWithCharges<DetailsType> b)
		{
			if (b != null)
			{
				return PostingOption == b.PostingOption;
			}

			return false;
		}

		protected override bool IsPostingActionTheSameCore(ApprovalRequestDetails postingApprovalDetails)
		{
			var postingApprovalDetails_Casted = postingApprovalDetails as ApprovalRequestDetailsWithCharges<DetailsType>;
			if (postingApprovalDetails_Casted != null)
			{
				return PostingOption == postingApprovalDetails_Casted.PostingOption;
			}

			return false;
		}

		#region Serialization

		protected sealed override void ReadXmlCore(XmlReader reader)
		{
			ReadXmlForInstanceSpecificFields(reader);

			base.ReadXmlCore(reader);

			UnRegisterEditableChildObject(Charges);
			var serializer = ZXmlSerializer.New(Charges.GetType());
			charges = (NonPersistentBusinessObjectCollection<DetailsType>)serializer.Deserialize(reader);
			RegisterEditableChildObject(Charges);
		}

		protected virtual void ReadXmlForInstanceSpecificFields(XmlReader reader)
		{
			if (reader.Name == Schema.PostingOption)
			{
				PostingOption = reader.ReadElementString(Schema.PostingOption);
			}
		}

		protected sealed override void WriteXmlCore(XmlWriter writer)
		{
			WriteXmlForInstanceSpecificFields(writer);

			base.WriteXmlCore(writer);

			var serializer = ZXmlSerializer.New(Charges.GetType());
			serializer.Serialize(writer, Charges);
		}

		protected virtual void WriteXmlForInstanceSpecificFields(XmlWriter writer)
		{
			writer.WriteElementString(Schema.PostingOption, PostingOption);
		}

		#endregion

		#endregion
	}
}
