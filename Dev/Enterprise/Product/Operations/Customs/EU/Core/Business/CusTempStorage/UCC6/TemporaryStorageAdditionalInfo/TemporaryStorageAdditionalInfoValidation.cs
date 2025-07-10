using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfoValidation : AdditionalInfoValidation
	{
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string ruleBR_PN_TS_050Message = Res.GetString("DA2B6B38-8AAB-4E68-8E76-4325149609D2", "The Additional information should be entered at Bill level or Bill Item level, not both.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		public TemporaryStorageAdditionalInfoValidation(TemporaryStorageAdditionalInfo parent) : base(parent)
		{
		}

		public new TemporaryStorageAdditionalInfo Parent => (TemporaryStorageAdditionalInfo)base.Parent;

		public override void ValidateAll()
		{
			var parent = Parent;

			parent.ClearRowNotifications();
			base.ValidateAll();

			CheckRuleBR_PN_TS_050(parent);
			ValidateMaxCountOfTheCollection();
		}

		public void ValidateMaxCountOfTheCollection()
		{
			var parent = Parent;

			if (parent.Parent is TemporaryStorageBill bill)
			{
				if (bill.AdditionalInfos.Count > 99)
				{
					parent.AddRowError(Res.GetString("A20D25EE-81AF-4EE0-83A0-EC73C8E0E2E9", "You are only allowed a maximum of 99 Additional Informations here."));
				}
			}
			else if (parent.Parent is TemporaryStoragePackedItem packedItem)
			{
				if (packedItem.AdditionalInfos.Cast<TemporaryStorageAdditionalInfo>().Count(x => x.CSI_SubType == parent.CSI_SubType) > 99)
				{
					parent.AddRowError(Res.GetString("169AF2A7-41DA-42B0-AAEC-FC058454E579", "You are only allowed a maximum of 99 {0} Additional Informations here.", parent.CSI_SubType));
				}
			}
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			var parent = Parent;

			if (parent.Parent is TemporaryStoragePackedItem)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
			}

			if (parent.Parent is TemporaryStorageBill bill)
			{
				if (bill.ABL_BolType == TemporaryStorageBill.ChildBolCode)
				{
					if (parent.CSI_SubType != AdditionalInfoSubTypeList.Codes.AdditionalInformation)
					{
						parent.AddRowMessageError(Res.GetString("8A71DBBB-E0BB-4D9F-A58A-405858582C07", "Only Additional Information supporting document allowed for Master Bills"));
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;

			if (parent.IsAnAdditionalReference)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			var parent = Parent;

			if (parent.IsAnAdditionalReference && parent.CSI_Code == "10600")
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_DescriptionInfo);
			}
		}

		void CheckRuleBR_PN_TS_050(TemporaryStorageAdditionalInfo parent)
		{
			if (parent.Parent is TemporaryStorageBill bill)
			{
				ValidateTemporaryStoragePackedItemsHasNoAdditionalInfoEntries(parent, bill);
			}
			else if (parent.Parent is TemporaryStoragePackedItem packedItem)
			{
				ValidateTemporaryStorageBillHasNoAdditionalInfoEntries(parent, packedItem);
			}
		}

		void ValidateTemporaryStoragePackedItemsHasNoAdditionalInfoEntries(TemporaryStorageAdditionalInfo additionalInfo, TemporaryStorageBill bill)
		{
			if (bill.PackedItems.Any(p => p.AdditionalInfos.Any()))
			{
				additionalInfo.AddRowMessageError(ruleBR_PN_TS_050Message);
			}
		}

		void ValidateTemporaryStorageBillHasNoAdditionalInfoEntries(TemporaryStorageAdditionalInfo additionalInfo, TemporaryStoragePackedItem packedItem)
		{
			if (packedItem.Bill.AdditionalInfos.Any())
			{
				additionalInfo.AddRowMessageError(ruleBR_PN_TS_050Message);
			}
		}

		protected override bool IsCodeEnabled => !(Parent.Parent is TemporaryStorageBill);

		protected override bool IsCodeMandatory => Parent.IsAnAdditionalReference;
	}
}
