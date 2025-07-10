using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	class UPEElectronicCreditNoteAutoDeliveryDummyFail : UPEElectronicCreditNoteAutoDelivery
	{
		public UPEElectronicCreditNoteAutoDeliveryDummyFail(UPEJobDeclaration documentSupportable) : base(documentSupportable)
		{
			SetUpTheRegistry();
		}

		protected override DeliveryInstructions GetInstructions()
		{
			DeliveryInstructions result = base.GetInstructions();
			DocDeliveryContact contact = result.Recipients.AddNew();
			result.RunPreSaveValidation();
			return result;
		}

		void SetUpTheRegistry()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			CreditNotificationGroup.Staff.Add(staff);
			staff.GS_EmailAddress = "sirko@sobaka.com";
			Factory.Save();
		}

		protected GlbGroup CreditNotificationGroup
		{
			get
			{
				GlbGroup result = Factory.Load<GlbGroup>(UPEDataRegistry.Instance.CreditNotificationGroup);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<GlbGroup>();
					UPEDataRegistry.Instance.CreditNotificationGroup = result.PK.ToGuid();
				}

				return result;
			}
		}
	}
}
