namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Common;

	public sealed class ReleaseStatusCollection : NonPersistentBusinessObjectCollection<ReleaseStatus>
	{
		public ReleaseStatusCollection(JobDeclaration declaration, bool shouldSynchronize = true, bool takeReceivedMessagesOnly = true)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			this.shouldSynchronize = shouldSynchronize;
			this.takeReceivedMessagesOnly = takeReceivedMessagesOnly;

			if (shouldSynchronize)
			{
				declaration.CargoControlNumbers.HasChangesChanged += RefNumbers_HasChangesChanged;
				declaration.CargoControlNumbers.CountChanged += CargoControlNumbers_CountChanged;
				declaration.AdditionalReferenceNumbers.HasChangesChanged += RefNumbers_HasChangesChanged;
				declaration.AdditionalReferenceNumbers.CountChanged += AdditionalReferenceNumbers_CountChanged;
				declaration.Factory.Saved += Factory_Saved;
			}

			Load();
			this.CountChanged += ReleaseStatus_CountChanged;
		}

		#region Overrides

		public override void Load()
		{
			reloadingCollection = true;
			using (SuspendSettingHasChanges())
			{
				RemoveAndDeleteAll();
				ResetCachedValues();

				var releaseMessages = LastReleaseMessagesPerCCN.ToList();

				foreach (CusEntryNumber ccn1 in declaration.AdditionalReferenceNumbers)
				{
					if (shouldSynchronize)
					{
						ccn1.CE_EntryTypeInfo.ValueChanged -= CE_EntryTypeInfo_ValueChanged;
						ccn1.CE_EntryTypeInfo.ValueChanged += CE_EntryTypeInfo_ValueChanged;
					}

					if (ccn1.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN)
					{
						using (var suspender = new CargoControlNumbersCountChangedSuspender(this))
						{
							Add(new ReleaseStatus(ccn1, GetMessageByCCNAndRemoveFromListIfRequired(releaseMessages, ccn1.CE_EntryNum)));
						}
					}
				}

				foreach (CargoControlNumber ccn2 in declaration.CargoControlNumbers)
				{
					if (!ccn2.CA_IsFromNumbersTab)
					{
						Add(new ReleaseStatus(ccn2, GetMessageByCCNAndRemoveFromListIfRequired(releaseMessages, ccn2.CY_CargoControlNumber)));
					}
				}

				foreach (var message in releaseMessages)
				{
					if (declaration.CargoControlNumbers.Any(x => x.CY_CargoControlNumber == message.CargoControlNumber))
					{
						Add(new ReleaseStatus(message));
					}
				}
			}

			reloadingCollection = false;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			using (var suspender = new CargoControlNumbersCountChangedSuspender(this))
			{
				return new ReleaseStatus(declaration.CargoControlNumbers.AddNew());
			}
		}

		#region Remove

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var element = elementToDelete as ReleaseStatus;
			if (element != null && (element.PersistentCCN != null || reloadingCollection))
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			var element = elementToRemove as ReleaseStatus;
			if (element != null && (element.PersistentCCN != null || reloadingCollection))
			{
				base.Remove(elementToRemove);
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			if (!reloadingCollection && shouldSynchronize)
			{
				using (var suspender = new CargoControlNumbersCountChangedSuspender(this))
				{
					((ReleaseStatus)bizO).PersistentCCN.Delete();
				}
			}
		}

		#endregion

		#endregion

		#region Implementation

		#region GetMessageByCCNAndRemoveFromListIfRequired

		EDIReleaseMessage GetMessageByCCNAndRemoveFromListIfRequired(List<EDIReleaseMessage> messages, ZString ccn)
		{
			EDIReleaseMessage matched;
			var result = GetMessageByCCN(messages, ccn, out matched);
			if (matched != null)
			{
				messages.Remove(matched);
			}

			return result;
		}

		EDIReleaseMessage GetMessageByCCN(IEnumerable<EDIReleaseMessage> messages, ZString ccn)
		{
			EDIReleaseMessage matched;
			return GetMessageByCCN(messages, ccn, out matched);
		}

		EDIReleaseMessage GetMessageByCCN(IEnumerable<EDIReleaseMessage> messages, ZString ccn, out EDIReleaseMessage matched)
		{
			matched = null;
			var result = LastCommonReleaseMessage;
			if (messages.Any())
			{
				matched = messages.FirstOrDefault(m => m.CargoControlNumber.Replace(" ", "") == ccn.Replace(" ", "").ToUpper());
				if (result == null || (matched != null && ZDateTime.TruncateSeconds(matched.EM_SystemCreateTimeUtc) >= ZDateTime.TruncateSeconds(result.EM_SystemCreateTimeUtc)))
				{
					result = matched;
				}
			}
			return result;
		}

		#endregion

		#region Event Handlers

		void ReleaseStatus_CountChanged(object sender, EventArgs e)
		{
			declaration?.EffectiveCCNInfo.RefreshBinding();
		}

		void RefNumbers_HasChangesChanged(object sender, EventArgs e)
		{
			if (((IBusiness)sender).HasChanges)
			{
				shouldReloadCollection = true;
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				ReloadCollectionIfRequired();
			}
		}

		void ReloadCollectionIfRequired()
		{
			if (shouldReloadCollection)
			{
				using (SuspendSettingHasChanges())
				{
					Load();
				}
				shouldReloadCollection = false;
			}
		}

		void AdditionalReferenceNumbers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var ccn1 = ((CusEntryNumber)e.BizObject);
			if (e.ItemAdded)
			{
				ccn1.CE_EntryTypeInfo.ValueChanged += CE_EntryTypeInfo_ValueChanged;
				AddIfRequired(ccn1);
			}
			else if (e.ItemRemoved)
			{
				ccn1.CE_EntryTypeInfo.ValueChanged -= CE_EntryTypeInfo_ValueChanged;
				Remove(ccn1);
			}
		}

		void CE_EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs valueChangedEventArgs)
			{
				var ccn1 = ((CusEntryNumber)sender);
				if (valueChangedEventArgs.OldValue.ToString() != CanadaAdditionalReferenceNumberTypes.Codes.CCN)
				{
					AddIfRequired(ccn1);
				}
				else
				{
					Remove(ccn1);
				}
			}
		}

		void AddIfRequired(CusEntryNumber ccn1)
		{
			if (ccn1.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN)
			{
				using (var suspender = new CargoControlNumbersCountChangedSuspender(this))
				{
					Add(new ReleaseStatus(ccn1, GetMessageByCCN(LastReleaseMessagesPerCCN, ccn1.CE_EntryNum)));
				}
			}
		}

		void Remove(CusEntryNumber ccn1)
		{
			reloadingCollection = true;
			var ccnToRemove = this.FirstOrDefault(a => ((ReleaseStatus)a).PersistentCCN == ccn1) as ReleaseStatus;
			if (ccnToRemove != null)
			{
				if (ccnToRemove.CACCN != null)
				{
					using (var suspender = new CargoControlNumbersCountChangedSuspender(this))
					{
						declaration.CargoControlNumbers.RemoveAndDelete(ccnToRemove.CACCN);
					}
				}
				RemoveAndDelete(ccnToRemove);
			}
			reloadingCollection = false;
		}

		void CargoControlNumbers_CountChanged(object sender, EventArgs e)
		{
			if (!declaration.IsSettingEntryNum && !IsCargoControlNumbersCountChangeSuspended)
			{
				ReloadCollectionIfRequired();
			}
		}

		public bool IsCargoControlNumbersCountChangeSuspended
		{
			get { return cargoControlNumbersSuspenderIndex > 0; }
		}
		int cargoControlNumbersSuspenderIndex;

		class CargoControlNumbersCountChangedSuspender : IDisposable
		{
			public CargoControlNumbersCountChangedSuspender(ReleaseStatusCollection releaseStatuses)
			{
				this.releaseStatuses = releaseStatuses;
				releaseStatuses.cargoControlNumbersSuspenderIndex++;
			}
			readonly ReleaseStatusCollection releaseStatuses;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				releaseStatuses.cargoControlNumbersSuspenderIndex--;
			}

			#endregion
		}

		#endregion

		#region LastReceivedReleaseMessages

		IEnumerable<EDIReleaseMessage> LastReleaseMessagesPerCCN
		{
			get
			{
				return lastReleaseMessagesPerCCN
					   ?? (lastReleaseMessagesPerCCN =
						   LastReceivedReleaseMessagesPerCCNIncludingCommon.Where(m => !m.CargoControlNumber.IsEmpty));
			}
		}

		IEnumerable<EDIReleaseMessage> lastReleaseMessagesPerCCN;

		internal EDIReleaseMessage LastCommonReleaseMessage
		{
			get
			{
				return lastCommonReleaseMessage
					   ?? (lastCommonReleaseMessage =
						   LastReceivedReleaseMessagesPerCCNIncludingCommon.FirstOrDefault(m => m.CargoControlNumber.IsEmpty));
			}
		}

		EDIReleaseMessage lastCommonReleaseMessage;

		IEnumerable<EDIReleaseMessage> LastReceivedReleaseMessagesPerCCNIncludingCommon
		{
			get
			{
				return lastReceivedReleaseMessagesPerCCNIncludingCommon
					   ?? (lastReceivedReleaseMessagesPerCCNIncludingCommon =
						   from EDIMessage message in GetEDIReleaseEntryHeaderMessages()
						   where message.EM_MessageType == MessageTypeList.Codes.EDIRelease
								 && !EDIReleaseImportEntryStatusList.IsError(message.EM_MessageSubType)
								 && !message.IsTransmitMessage
								 && (!takeReceivedMessagesOnly || message.EM_Status == EDIMessage.Status.Received)
						   orderby message.EM_SystemCreateTimeUtc
						   group message by message.CargoControlNumber
							   into grouping
						   select (EDIReleaseMessage)grouping.Last());
			}
		}

		IEnumerable<EDIReleaseMessage> lastReceivedReleaseMessagesPerCCNIncludingCommon;

		IEnumerable<BusinessObject> GetEDIReleaseEntryHeaderMessages()
		{
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);
			return entryHeader != null ? entryHeader.Messages : Array.Empty<BusinessObject>();
		}

		void ResetCachedValues()
		{
			lastReceivedReleaseMessagesPerCCNIncludingCommon = null;
			lastCommonReleaseMessage = null;
			lastReleaseMessagesPerCCN = null;
		}

		#endregion

		#endregion

		readonly JobDeclaration declaration;
		readonly bool shouldSynchronize;
		readonly bool takeReceivedMessagesOnly;
		bool shouldReloadCollection;
		bool reloadingCollection;
	}
}
